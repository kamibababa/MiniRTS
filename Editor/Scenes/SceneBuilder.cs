﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MiniEngine.CutScene;
using MiniEngine.GameLogic;
using MiniEngine.Pipeline.Basics.Components;
using MiniEngine.Pipeline.Basics.Factories;
using MiniEngine.Pipeline.Debug.Components;
using MiniEngine.Pipeline.Debug.Factories;
using MiniEngine.Pipeline.Lights.Components;
using MiniEngine.Pipeline.Lights.Factories;
using MiniEngine.Pipeline.Models.Components;
using MiniEngine.Pipeline.Models.Factories;
using MiniEngine.Pipeline.Particles.Factories;
using MiniEngine.Pipeline.Projectors.Factories;
using MiniEngine.Rendering;
using MiniEngine.Systems;
using MiniEngine.Systems.Components;
using MiniEngine.Systems.Factories;
using MiniEngine.Units;

namespace MiniEngine.Scenes
{

    // TODO: most building of things should be moved to commands and handled by the server

    public sealed class SceneBuilder
    {
        private readonly EntityController EntityController;
        private readonly Content Content;
        private readonly PipelineBuilder PipelineBuilder;

        private readonly Dictionary<Type, IComponentFactory> Factories;

        public SceneBuilder(
            EntityController entityController,
            Content content,
            PipelineBuilder pipelineBuilder,
            IEnumerable<IComponentFactory> factories)
        {
            this.EntityController = entityController;
            this.Content = content;
            this.PipelineBuilder = pipelineBuilder;

            this.Factories = new Dictionary<Type, IComponentFactory>();
            foreach (var factory in factories)
            {
                this.Factories.Add(factory.GetType(), factory);
            }
        }

        public T GetFactory<T>()
            where T : IComponentFactory
        {
            if (this.Factories.TryGetValue(typeof(T), out var factory))
            {
                return (T)factory;
            }
            throw new KeyNotFoundException($"Could not find factory of type {typeof(T)} did you forget to make it available for injection?");
        }

        public DebugLine CreateDebugLine(IReadOnlyList<Vector3> linePositions, Color color)
        {
            var entity = this.EntityController.CreateEntity();
            return this.GetFactory<DebugLineFactory>().Construct(entity, linePositions, color);
        }

        public AmbientLight BuildSponzaAmbientLight()
        {
            var entity = this.EntityController.CreateEntity();
            return this.GetFactory<AmbientLightFactory>().Construct(entity, Color.White * 0.5f);
        }

        public Sunlight BuildSponzaSunLight()
        {
            var entity = this.EntityController.CreateEntity();
            return this.GetFactory<SunlightFactory>().Construct(entity, Color.White, Vector3.Up, (Vector3.Left * 0.75f) + (Vector3.Backward * 0.1f));
        }

        public OpaqueModel BuildSponza(Vector3 position, float scale)
        {
            var entity = this.EntityController.CreateEntity();
            this.GetFactory<PoseFactory>().Construct(entity, position, scale);
            var (model, _) = this.GetFactory<OpaqueModelFactory>().Construct(entity, this.Content.Sponza);
            return model;
        }

        public (Pose, OpaqueModel, Bounds) BuildCube(Vector3 position, float scale)
        {
            var entity = this.EntityController.CreateEntity();
            var pose = this.GetFactory<PoseFactory>().Construct(entity, position, scale);
            var (model, bounds) = this.GetFactory<OpaqueModelFactory>().Construct(entity, this.Content.Cube);

            return (pose, model, bounds);
        }

        public ShadowCastingLight BuildLionSpotLight()
        {
            var entity = this.EntityController.CreateEntity();
            return this.GetFactory<ShadowCastingLightFactory>().Construct(entity, new Vector3(40, 13, 27), new Vector3(53, 11, 12), Color.White, 2048);
        }

        public void BuildSponzaLit(Vector3 position, float scale)
        {
            this.BuildSponzaAmbientLight();
            this.BuildSponzaSunLight();
            this.BuildSponza(position, scale);
        }

        public Entity[] BuildStainedGlass()
        {
            var entities = this.EntityController.CreateEntities(2);

            var position = new Vector3(-40.5f, 30.0f, 3.2f);
            this.GetFactory<PoseFactory>().Construct(entities[0], position, 4.4f * 0.01f, MathHelper.PiOver2, MathHelper.PiOver2, 0);
            this.GetFactory<TransparentModelFactory>().Construct(entities[0], this.Content.Plane);

            position = new Vector3(-40.5f, 30.0f, -7.2f);
            this.GetFactory<PoseFactory>().Construct(entities[1], position, 4.4f * 0.01f, MathHelper.PiOver4);
            this.GetFactory<TransparentModelFactory>().Construct(entities[1], this.Content.Plane);

            return entities;
        }

        public PointLight BuildFirePlace()
        {
            var entity = this.EntityController.CreateEntity();
            this.GetFactory<PoseFactory>().Construct(entity, new Vector3(-60.5f, 6.0f, 20.0f), Vector3.One * 2, 0, MathHelper.PiOver2, 0);
            this.GetFactory<AveragedEmitterFactory>().ConstructAveragedEmitter(entity, this.Content.Smoke, 1, 1);

            var entity2 = this.EntityController.CreateEntity();
            this.GetFactory<PoseFactory>().Construct(entity2, new Vector3(-60.5f, 6.0f, 20.0f), Vector3.One, 0, MathHelper.PiOver2, 0);
            this.GetFactory<AdditiveEmitterFactory>().ConstructAdditiveEmitter(entity2, this.Content.Explosion2, 1, 1);

            var entity3 = this.EntityController.CreateEntity();
            this.GetFactory<PoseFactory>().Construct(entity3, new Vector3(-60.5f, 6.0f, 20.0f), Vector3.One * 0.075f, 0, MathHelper.PiOver2, 0);
            var emitter = this.GetFactory<AdditiveEmitterFactory>().ConstructAdditiveEmitter(entity3, this.Content.Explosion, 8, 8);
            emitter.SpawnInterval = 0;
            emitter.Spread = 0.75f;
            emitter.TimeToLive = 2.25f;


            var pointLight = this.GetFactory<PointLightFactory>().Construct(entity, Color.IndianRed, 20.0f, 1.0f);

            var cameraPosition = new Vector3(-60.5f, 8.0f, 20.0f);
            var projectorPosition = new Vector3(-60.5f, 0.0f, 20.0f);
            var lookAt = cameraPosition + (new Vector3(0.001f, 1, 0) * 10);

            var lightEntity = this.EntityController.CreateEntity();
            var dynamicTexture = this.GetFactory<DynamicTextureFactory>().Construct(lightEntity, cameraPosition, lookAt, 1024, 1024, this.Content.NullSkybox, "Firewatcher");
            this.PipelineBuilder.AddParticlePipeline(dynamicTexture.Pipeline);

            var color = Color.White * 0.2f;
            var projector = this.GetFactory<ProjectorFactory>().Construct(lightEntity, dynamicTexture.FinalTarget, this.Content.Mask, color, projectorPosition, lookAt);
            projector.SetMinDistance(10.0f);
            projector.SetMaxDistance(30.0f);

            return pointLight;
        }

        public Parent BuildParent(string name)
        {
            var parentEntity = this.EntityController.CreateEntity(name);
            var parent = this.GetFactory<ParentFactory>().Construct(parentEntity);
            return parent;
        }

        public Entity BuildBulletHoles()
        {
            var entity = this.EntityController.CreateEntity();

            var random = new Random(12345);

            var center = new Vector3(-71.2f, 10, -25);
            var forward = Vector3.Left;

            for (var i = 0; i < 1 /*100*/; i++)
            {
                var u = (float)(random.NextDouble() * 15) - 7.5f;
                var v = (float)(random.NextDouble() * 15) - 7.5f;

                var offset = new Vector3(0, u, v);
                var projector = this.GetFactory<ProjectorFactory>().Construct(entity, this.Content.BulletHole, Color.White, center + offset, center + offset + forward);
                projector.SetMaxDistance(1.0f);
            }

            return entity;
        }

        public void BuildCutScene()
        {
            var speeds = new MetersPerSecond[]
            {
                new MetersPerSecond(15.0f),
                new MetersPerSecond(15.0f),
                new MetersPerSecond(15.0f),
                new MetersPerSecond(6.0f),
                new MetersPerSecond(15.0f),
                new MetersPerSecond(15.0f),
                new MetersPerSecond(15.0f),
                new MetersPerSecond(15.0f),
                new MetersPerSecond(5.0f),
                new MetersPerSecond(5.0f)
            };

            var positions = new Vector3[]
            {
                new Vector3(60, 10, 20), // start position
                new Vector3(-60, 10, 20), // near fireplace
                new Vector3(-50, 15, 0), // side stepping column
                new Vector3(-10, 40, 0),  // center stage
                new Vector3(-30, 13, -10), // inspect windows
                new Vector3(-25, 34, -10), // side step to upper row
                new Vector3(-10, 34, -10), // in upper row
                new Vector3(20, 25, -7), // in upper row
                new Vector3(49, 10, -7), // pass lion
                new Vector3(49, 10, 20), // start position
            };

            var lookAts = new Vector3[]
            {
                new Vector3(-60, 10, 20),
                new Vector3(-60, 0, 20),
                new Vector3(-60, 30, 20),
                new Vector3(-40, 0, 20),
                new Vector3(-50, 30, 10),
                new Vector3(-10, 40, 20),
                new Vector3(60, 40, 20),
                new Vector3(60, 10, 0),
                new Vector3(60, 10, 0),
                new Vector3(60, 10, 0)
            };

            for (var i = 0; i < positions.Length; i++)
            {
                var speed = speeds[i];
                var position = positions[i];
                var lookAt = lookAts[i];

                var entity = this.EntityController.CreateEntity();
                this.GetFactory<WaypointFactory>().Construct(entity, speed, position, lookAt);
            }
        }
    }
}

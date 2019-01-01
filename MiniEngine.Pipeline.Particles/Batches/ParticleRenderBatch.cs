﻿using Microsoft.Xna.Framework.Graphics;
using MiniEngine.Effects;
using MiniEngine.Effects.Techniques;
using MiniEngine.Pipeline.Particles.Components;
using MiniEngine.Primitives;
using MiniEngine.Primitives.Cameras;
using System.Collections.Generic;

namespace MiniEngine.Pipeline.Particles.Batches
{
    public sealed class ParticleRenderBatch
    {
        private readonly RenderEffect Effect;

        private readonly Texture2D Mask;
        private readonly Texture2D NormalMap;
        private readonly IReadOnlyList<ParticlePose> Particles;
        private readonly Quad Quad;
        private readonly Texture2D SpecularMap;
        private readonly IViewPoint ViewPoint;

        public ParticleRenderBatch(
            Quad quad,
            RenderEffect effect,
            IReadOnlyList<ParticlePose> particles,
            IViewPoint viewPoint,
            Texture2D mask,
            Texture2D normalMap,
            Texture2D specularMap)
        {
            this.Quad = quad;
            this.Effect = effect;
            this.Particles = particles;
            this.ViewPoint = viewPoint;
            this.Mask = mask;
            this.NormalMap = normalMap;
            this.SpecularMap = specularMap;
        }

        public void Draw(RenderEffectTechniques technique)
        {
            this.Effect.Mask = this.Mask;
            this.Effect.NormalMap = this.NormalMap;
            this.Effect.SpecularMap = this.SpecularMap;

            this.Effect.Projection = this.ViewPoint.Projection;
            this.Effect.View = this.ViewPoint.View;

            foreach (var particle in this.Particles)
            {
                this.Effect.DiffuseMap = particle.Texture;

                this.Effect.World = particle.Pose;
                this.Effect.Apply(technique);

                this.Quad.SetTextureCoordinates(particle.MinUv, particle.MaxUv);
                this.Quad.Render();
            }
        }
    }
}
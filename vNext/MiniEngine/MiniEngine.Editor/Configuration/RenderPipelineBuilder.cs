﻿using MiniEngine.Configuration;
using MiniEngine.Graphics.Geometry;
using MiniEngine.Graphics.Lighting;
using MiniEngine.Graphics.PostProcess;
using MiniEngine.Graphics.Rendering;
using MiniEngine.Graphics.Shadows;
using MiniEngine.Graphics.Skybox;
using MiniEngine.Graphics.Visibility;
using MiniEngine.Systems.Components;
using MiniEngine.Systems.Pipeline;
using MiniEngine.Systems.Services;

namespace MiniEngine.Editor.Configuration
{
    [Service]
    public sealed class RenderPipelineBuilder
    {
        private readonly PipelineBuilder Builder;

        public RenderPipelineBuilder(PipelineBuilder builder)
        {
            this.Builder = builder;
        }

        public ParallelPipeline Build()
        {
            var pipeline = this.Builder.Builder();
            return pipeline
                .System<ComponentFlushSystem>()
                    .Parallel()
                    .Produces("Containers")
                    .Build()
                .System<ClearBuffersSystem>()
                    .Parallel()
                    .Produces("Buffers")
                    .Build()
                .System<VisibilitySystem>()
                    .Parallel()
                    .Produces("Poses")
                    .RequiresAll("Containers")
                    .Build()
                .System<GeometrySystem>()
                    .InSequence()
                    .RequiresAll("Buffers")
                    .RequiresAll("Containers")
                    .RequiresAll("Poses")
                    .Produces("Meshes", "Geometry")
                    .Build()
                .System<ShadowMapSystem>()
                    .InSequence()
                    .RequiresAll("Buffers")
                    .RequiresAll("Containers")
                    .RequiresAll("Poses")
                    .Produces("Lights", "Shadows")
                    .Build()
                .System<ImageBasedLightSystem>()
                    .InSequence()
                    .RequiresAll("Buffers")
                    .RequiresAll("Meshes")
                    .Produces("Lights", "Image")
                    .Build()
                .System<PointLightSystem>()
                    .InSequence()
                    .RequiresAll("Buffers")
                    .RequiresAll("Containers")
                    .RequiresAll("Meshes")
                    .Produces("Lights", "Point")
                    .Build()
                .System<SpotLightSystem>()
                    .InSequence()
                    .RequiresAll("Buffers")
                    .RequiresAll("Containers")
                    .RequiresAll("Meshes")
                    .Requires("Lights", "Shadows")
                    .Produces("Lights", "Spot")
                    .Build()
                .System<SkyboxSystem>()
                    .InSequence()
                    .RequiresAll("Buffers")
                    .RequiresAll("Lights")
                    .Produces("Skyboxes")
                    .Build()
                .System<ToneMapSystem>()
                    .InSequence()
                    .RequiresAll("Buffers")
                    .RequiresAll("Lights")
                    .RequiresAll("Skyboxes")
                    .Produces("Final Image")
                    .Build()
                .Build();
        }
    }
}

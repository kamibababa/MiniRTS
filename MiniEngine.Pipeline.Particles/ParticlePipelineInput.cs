﻿using MiniEngine.Pipeline.Particles.Batches;
using MiniEngine.Primitives;
using MiniEngine.Primitives.Cameras;

namespace MiniEngine.Pipeline.Particles
{
    public sealed class ParticlePipelineInput : IPipelineInput
    {
        public void Update(PerspectiveCamera camera, ParticleRenderBatch batch, GBuffer gBuffer, string pass)
        {
            this.Camera = camera;
            this.Batch = batch;
            this.GBuffer = gBuffer;
            this.Pass = pass;
        }

        public PerspectiveCamera Camera { get; private set; }
        public ParticleRenderBatch Batch { get; private set; }
        public GBuffer GBuffer { get; private set; }
        public string Pass { get; private set; }
    }
}
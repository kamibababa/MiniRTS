﻿using Microsoft.Xna.Framework;
using MiniEngine.Systems;
using MiniEngine.Systems.Annotations;
using MiniEngine.Systems.Components;

namespace MiniEngine.Pipeline.Lights.Components
{
    public sealed class PointLight : IComponent
    {
        public PointLight(Entity entity, Color color, float radius, float intensity)
        {
            this.Entity = entity;
            this.Color = color;
            this.Radius = radius;
            this.Intensity = intensity;
        }

        public Entity Entity { get; }

        [Editor(nameof(Color))]
        public Color Color { get; set; }

        [Editor(nameof(Radius), nameof(Radius), 0, float.MaxValue)]
        public float Radius { get; set; }

        [Editor(nameof(Intensity), nameof(Intensity), 0, float.MaxValue)]
        public float Intensity { get; set; }
    }
}
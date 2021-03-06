﻿using Microsoft.Xna.Framework;
using MiniEngine.Pipeline.Shadows.Components;
using MiniEngine.Systems;
using MiniEngine.Systems.Annotations;
using MiniEngine.Systems.Components;

namespace MiniEngine.Pipeline.Lights.Components
{
    // TODO: use Pose 
    public sealed class Sunlight : IComponent
    {
        public Sunlight(Entity entity, CascadedShadowMap shadowMapCascades, Color color)
        {
            this.Entity = entity;
            this.ShadowMapCascades = shadowMapCascades;
            this.Color = color;
        }

        public Entity Entity { get; }

        public CascadedShadowMap ShadowMapCascades { get; }

        [Editor(nameof(Color))]
        public Color Color { get; set; }

        [Editor(nameof(Position), nameof(SetPosition), float.MinValue, float.MaxValue)]
        public Vector3 Position => this.ShadowMapCascades.Position;

        [Editor(nameof(LookAt), nameof(SetLookAt), float.MinValue, float.MaxValue)]
        public Vector3 LookAt => this.ShadowMapCascades.LookAt;

        public void Move(Vector3 position, Vector3 lookAt) => this.ShadowMapCascades.Move(position, lookAt);
        public void SetPosition(Vector3 position) => this.ShadowMapCascades.Move(position, this.ShadowMapCascades.LookAt);
        public void SetLookAt(Vector3 lookAt) => this.ShadowMapCascades.Move(this.ShadowMapCascades.Position, lookAt);
    }
}
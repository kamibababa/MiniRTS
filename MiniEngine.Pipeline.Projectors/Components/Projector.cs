﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiniEngine.Primitives.Cameras;
using MiniEngine.Systems.Annotations;
using MiniEngine.Systems.Components;
using MiniEngine.Units;

namespace MiniEngine.Pipeline.Projectors.Components
{
    [Label(nameof(Projector))]
    public sealed class Projector : IComponent
    {
        private const float Epsilon = 0.001f;

        public Projector(Texture2D texture, Color tint, Vector3 position, Vector3 lookAt, Meters minDistance, Meters maxDistance)
        {            
            this.Texture = texture;            
            this.Tint = tint;

            this.ViewPoint = new PerspectiveCamera(1);
            this.ViewPoint.Move(position, lookAt);

            this.SetMinDistance(minDistance);
            this.SetMaxDistance(maxDistance);
        }
        
        [Editor(nameof(Texture))]
        public Texture2D Texture { get; }         
        
        [Editor(nameof(Tint))]
        public Color Tint { get; set; }        

        [Editor(nameof(MinDistance), nameof(SetMinDistance), Epsilon, float.MaxValue)]
        public float MinDistance { get; private set; }

        [Editor(nameof(MaxDistance), nameof(SetMaxDistance), Epsilon, float.MaxValue)]
        public float MaxDistance { get; private set; }

        [Editor(nameof(ViewPoint))]
        public PerspectiveCamera ViewPoint { get; set; }

        [Boundary(BoundaryType.Frustum)]
        public BoundingFrustum Bounds => this.ViewPoint.Frustum;

        [Icon(IconType.Camera)]
        public Vector3 Position => this.ViewPoint.Position;

        [Icon(IconType.LookAt)]
        public Vector3 LookAt => this.ViewPoint.LookAt;

        public void SetMinDistance(float distance)
        {
            distance = MathHelper.Clamp(distance, Epsilon, this.MaxDistance - Epsilon);

            this.MinDistance = distance;
            this.ViewPoint.SetPlanes(this.MinDistance, this.MaxDistance);
        }

        public void SetMaxDistance(float distance)
        {
            distance = MathHelper.Clamp(distance, this.MinDistance + Epsilon, float.MaxValue);

            this.MaxDistance = distance;
            this.ViewPoint.SetPlanes(this.MinDistance, this.MaxDistance);
        }        
    }
}

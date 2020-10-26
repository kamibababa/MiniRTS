﻿using Microsoft.Xna.Framework.Graphics;

namespace MiniEngine.Graphics.Skybox
{
    public sealed class SkyboxGeometry
    {
        public SkyboxGeometry(Texture2D texture, VertexBuffer vertexBuffer, IndexBuffer indexBuffer)
        {
            this.Texture = texture;
            this.VertexBuffer = vertexBuffer;
            this.IndexBuffer = indexBuffer;

            this.Primitives = this.IndexBuffer.IndexCount / 3;
        }

        public Texture2D Texture { get; set; }
        public VertexBuffer VertexBuffer { get; }
        public IndexBuffer IndexBuffer { get; }

        public int Primitives { get; }
    }
}

﻿using Microsoft.Xna.Framework.Graphics;
using MiniEngine.Systems;

namespace MiniEngine.Pipeline.Models.Components
{
    public sealed class OpaqueModel : AModel
    {
        public OpaqueModel(Entity entity, Model model)
            : base(entity, model) { }
    }
}
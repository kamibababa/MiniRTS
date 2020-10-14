﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace MiniEngine.Systems
{
    public struct Entity : IEquatable<Entity>
    {
        internal Entity(int id)
        {
            this.Id = id;
        }

        public int Id { get; }

        public bool Equals([AllowNull] Entity other) => this.Id == other.Id;

        public override int GetHashCode() => this.Id.GetHashCode();

        public override string ToString() => $"{this.Id}";
    }
}
﻿using System;
using MiniEngine.Systems;

namespace MiniEngine.Gui.Editors
{
    public abstract class AEditor<T> : IPropertyEditor
    {
        public bool Draw(string name, Func<object, object> get, Action<object, object?> set, AComponent component)
        {
            T typedGet() => (T)get(component);
            void typedSet(T it) => set(component, it);
            return this.Draw(name, typedGet, typedSet);
        }

        public Type TargetType => typeof(T);

        public abstract bool Draw(string name, Func<T> get, Action<T> set);
    }
}

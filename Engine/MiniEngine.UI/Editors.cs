﻿using System;
using System.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiniEngine.Primitives.Cameras;
using MiniEngine.Systems.Components;
using MiniEngine.Units;
using Num = System.Numerics;

namespace MiniEngine.UI
{
    public sealed class Editors
    {
        private const int SliderDragThreshold = 100;
        private readonly ImGuiRenderer GuiRenderer;

        public Editors(ImGuiRenderer guiRenderer)
        {
            this.GuiRenderer = guiRenderer;
            this.DragSpeed = 0.01f;
        }

        public float DragSpeed { get; set; }

        public void Create(string label, object value, MinMaxDescription minMax, Action<object> setter, int index = 0)
        {
            if (setter == null)
            {
                switch (value)
                {
                    case Texture2D texture:
                        ImGui.Text(label);
                        var id = this.GuiRenderer.BindTexture(texture, index);
                        var scale = Math.Min(1.0f, 256.0f / texture.Width);
                        ImGui.Text($"Bounds: {texture.Bounds}");
                        ImGui.Text($"Format: {texture.Format}");
                        ImGui.Text($"Levels: {texture.LevelCount}");
                        ImGui.Image(id, new Num.Vector2(scale * texture.Width, scale * texture.Height));

                        break;

                    default:
                        ImGui.Text($"{label}: {value}");
                        break;
                }
                return;
            }

            switch (value)
            {
                case Vector3 vector:
                    var v = ToNum(vector);
                    if (this.CreateVector3(label, ref v, minMax)) { setter(ToXna(v)); }
                    break;
                case Vector2 vector2:
                    var v2 = ToNum(vector2);
                    if (this.CreateVector2(label, ref v2, minMax)) { setter(ToXna(v2)); }
                    break;
                case PerspectiveCamera camera:
                    {
                        ImGui.Text(label);

                        var position = ToNum(camera.Position);
                        var lookAt = ToNum(camera.LookAt);
                        var fov = camera.FieldOfView;

                        var set = false;
                        set |= this.CreateVector3("Position", ref position, MinMaxDescription.MinusInfinityToInfinity);
                        set |= this.CreateVector3("LookAt", ref lookAt, MinMaxDescription.MinusInfinityToInfinity);
                        set |= this.CreateFloat("FieldOfView", ref fov, new MinMaxDescription(0, MathHelper.Pi));

                        if (set)
                        {
                            camera.Move(ToXna(position), ToXna(lookAt));
                            camera.SetFieldOfView(fov);
                        }
                    }
                    break;

                case Color color:
                    // TODO: colors are pre-multiplied, handle that better
                    var c = ToNum(color.ToVector4());
                    if (ImGui.ColorEdit4(label, ref c, ImGuiColorEditFlags.AlphaBar))
                    {
                        setter(new Color(ToXna(c)));
                    }
                    break;

                case Seconds s:
                    var fs = s.Value;
                    if (this.CreateFloat(label, ref fs, minMax))
                    {
                        setter(new Seconds(fs));
                    }
                    break;
                case Meters m:
                    var fm = m.Value;
                    if (this.CreateFloat(label, ref fm, minMax))
                    {
                        setter(new Meters(fm));
                    }
                    break;
                case float f:
                    if (this.CreateFloat(label, ref f, minMax))
                    {
                        setter(f);
                    }
                    break;
                case float[] floats:
                    for (var i = 0; i < floats.Length; i++)
                    {
                        var fv = floats[i];
                        if (this.CreateFloat($"{label}:{i}", ref fv, minMax))
                        {
                            floats[i] = fv;
                            setter(floats);
                        }
                    }
                    break;
                case int i:
                    if (this.CreateInt(label, ref i, minMax))
                    {
                        setter(i);
                    }
                    break;

                case bool b:
                    if (ImGui.Checkbox(label, ref b))
                    {
                        setter(b);
                    }
                    break;

                case Enum e:
                    var items = Enum.GetNames(e.GetType()).ToList();
                    var ei = items.IndexOf(Enum.GetName(e.GetType(), e));
                    if (ImGui.Combo(label, ref ei, items.ToArray(), items.Count))
                    {
                        setter(Enum.Parse(e.GetType(), items[ei]));
                    }
                    break;
                default:
                    ImGui.Text($"{label}: {value}*");
                    return;
            }
        }


        private bool CreateVector3(string label, ref Num.Vector3 v, MinMaxDescription minMax)
        {
            switch (minMax.Type)
            {
                case MinMaxDescriptionType.ZeroToOne:
                    return ImGui.SliderFloat3(label, ref v, 0, 1);

                case MinMaxDescriptionType.MinusOneToOne:
                    return ImGui.SliderFloat3(label, ref v, -1, 1);

                case MinMaxDescriptionType.ZeroToInfinity:
                    return ImGui.DragFloat3(label, ref v, this.DragSpeed, 0, float.MaxValue);

                case MinMaxDescriptionType.Custom:
                    if ((minMax.Max - minMax.Min) < SliderDragThreshold)
                    {
                        return ImGui.SliderFloat3(label, ref v, minMax.Min, minMax.Max);
                    }
                    return ImGui.DragFloat3(label, ref v, this.DragSpeed, minMax.Min, minMax.Max);

                case MinMaxDescriptionType.MinusInfinityToInfinity:
                default:
                    return ImGui.DragFloat3(label, ref v, this.DragSpeed);
            }
        }

        private bool CreateVector2(string label, ref Num.Vector2 v, MinMaxDescription minMax)
        {
            switch (minMax.Type)
            {
                case MinMaxDescriptionType.ZeroToOne:
                    return ImGui.SliderFloat2(label, ref v, 0, 1);

                case MinMaxDescriptionType.MinusOneToOne:
                    return ImGui.SliderFloat2(label, ref v, -1, 1);

                case MinMaxDescriptionType.ZeroToInfinity:
                    return ImGui.DragFloat2(label, ref v, this.DragSpeed, 0, float.MaxValue);

                case MinMaxDescriptionType.Custom:
                    if ((minMax.Max - minMax.Min) < SliderDragThreshold)
                    {
                        return ImGui.SliderFloat2(label, ref v, minMax.Min, minMax.Max);
                    }
                    return ImGui.DragFloat2(label, ref v, this.DragSpeed, minMax.Min, minMax.Max);

                case MinMaxDescriptionType.MinusInfinityToInfinity:
                default:
                    return ImGui.DragFloat2(label, ref v, this.DragSpeed);
            }
        }

        private bool CreateFloat(string label, ref float v, MinMaxDescription minMax)
        {
            switch (minMax.Type)
            {
                case MinMaxDescriptionType.ZeroToOne:
                    return ImGui.SliderFloat(label, ref v, 0, 1);

                case MinMaxDescriptionType.MinusOneToOne:
                    return ImGui.SliderFloat(label, ref v, -1, 1);

                case MinMaxDescriptionType.ZeroToInfinity:
                    return ImGui.DragFloat(label, ref v, this.DragSpeed, 0, float.MaxValue);

                case MinMaxDescriptionType.Custom:
                    if ((minMax.Max - minMax.Min) < SliderDragThreshold)
                    {
                        return ImGui.SliderFloat(label, ref v, minMax.Min, minMax.Max);
                    }
                    return ImGui.DragFloat(label, ref v, this.DragSpeed, minMax.Min, minMax.Max);

                default:
                case MinMaxDescriptionType.MinusInfinityToInfinity:
                    return ImGui.DragFloat(label, ref v, this.DragSpeed);
            }
        }

        private bool CreateInt(string label, ref int v, MinMaxDescription minMax)
        {
            switch (minMax.Type)
            {
                case MinMaxDescriptionType.ZeroToOne:
                    return ImGui.SliderInt(label, ref v, 0, 1);

                case MinMaxDescriptionType.MinusOneToOne:
                    return ImGui.SliderInt(label, ref v, -1, 1);

                case MinMaxDescriptionType.ZeroToInfinity:
                    return ImGui.DragInt(label, ref v, this.DragSpeed, 0);

                case MinMaxDescriptionType.Custom:
                    if ((minMax.Max - minMax.Min) < SliderDragThreshold)
                    {
                        return ImGui.SliderInt(label, ref v, (int)Math.Round(minMax.Min), (int)Math.Round(minMax.Max));
                    }
                    return ImGui.DragInt(label, ref v, this.DragSpeed, (int)Math.Round(minMax.Min), (int)Math.Round(minMax.Max));

                default:
                case MinMaxDescriptionType.MinusInfinityToInfinity:
                    return ImGui.DragInt(label, ref v, this.DragSpeed);
            }
        }

        private static Num.Vector2 ToNum(Vector2 v) => new Num.Vector2(v.X, v.Y);
        private static Vector2 ToXna(Num.Vector2 v) => new Vector2(v.X, v.Y);
        private static Num.Vector3 ToNum(Vector3 v) => new Num.Vector3(v.X, v.Y, v.Z);
        private static Vector3 ToXna(Num.Vector3 v) => new Vector3(v.X, v.Y, v.Z);

        private static Num.Vector4 ToNum(Vector4 v) => new Num.Vector4(v.X, v.Y, v.Z, v.W);
        private static Vector4 ToXna(Num.Vector4 v) => new Vector4(v.X, v.Y, v.Z, v.W);
    }
}

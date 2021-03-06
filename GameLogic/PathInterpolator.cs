﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MiniEngine.GameLogic
{
    public sealed class PathInterpolator
    {
        private const float l = 0.15f;

        public static Path Interpolate(Path path)
        {
            if (path.WayPoints.Count > 2)
            {
                var points = new List<Vector3>(path.WayPoints);
                for (var r = 0; r < 5; r++)
                {
                    points = InterpolateInternal(points);
                }

                return new Path(points);
            }

            return path;
        }

        private static List<Vector3> InterpolateInternal(IReadOnlyList<Vector3> path)
        {
            var points = new List<Vector3>();
            points.Add(path[0]);

            for (var i = 3; i < path.Count; i += 2)
            {
                var start = path[i - 2];
                var control = path[i - 1];
                var end = path[i];

                if (AreOnOneLine(start, control, end))
                {
                    points.Add(start);
                    points.Add(end);
                }
                else
                {
                    var a = Vector3.Lerp(start, control, l);
                    var b = Vector3.Lerp(start, control, l * 2);
                    var c = Vector3.Lerp(start, control, l * 3);

                    var a2 = Vector3.Lerp(control, end, l);
                    var b2 = Vector3.Lerp(control, end, l * 2);
                    var c2 = Vector3.Lerp(control, end, l * 3);

                    points.Add(Vector3.Lerp(a, a2, l));
                    points.Add(Vector3.Lerp(b, b2, l * 2));
                    points.Add(Vector3.Lerp(c, c2, l * 3));
                }
            }


            points.Add(path[path.Count - 1]);
            return points;
        }

        private static bool AreOnOneLine(Vector3 start, Vector3 control, Vector3 end)
        {
            var fullDistance = Vector3.Distance(start, end);
            var viaDistance = Vector3.Distance(start, control) + Vector3.Distance(control, end);
            return Math.Abs(fullDistance - viaDistance) < 0.001f;
        }
    }
}

using System;

namespace RePhysics
{
    public static class ReCollisions
    {
        // Normal always points away from the first body

        public static bool IntersectingCircleBox(ReBody circle, ReVector[] boxVertices, out ReVector normal, out float depth)
        {
            normal = ReVector.Zero;
            depth = float.MaxValue;

            ReVector axis;
            float axisDepth;
            float minA, maxA, minB, maxB;

            for (int i = 0; i < boxVertices.Length; i++)
            {
                ReVector vA = boxVertices[i];
                ReVector vB = boxVertices[(i + 1) % boxVertices.Length]; // loops to beginning of array

                ReVector edge = vB - vA;
                axis = new ReVector(-edge.Y, edge.X); // normal of the current edge. Inverts

                ProjectVertices(boxVertices, axis, out minA, out maxA);
                ProjectCircle(circle.Pos, circle.Radius, axis, out minB, out maxB);

                if (minA > maxB || minB > maxA) return false;

                axisDepth = MathF.Min(maxB - minA, maxA - minB);
                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = (maxB > maxA) ? axis : -axis;
                }
            }

            int closestIndex = ClosestVertexIndex(circle.Pos, boxVertices);
            ReVector closestPoint = boxVertices[closestIndex];

            axis = closestPoint - circle.Pos;

            ProjectVertices(boxVertices, axis, out minA, out maxA);
            ProjectCircle(circle.Pos, circle.Radius, axis, out minB, out maxB);

            if (minA > maxB || minB > maxA) return false;

            axisDepth = MathF.Min(maxB - minA, maxA - minB);
            if (axisDepth < depth)
            {
                depth = axisDepth;
                normal = (maxB > maxA) ? axis : -axis;
            }

            depth /= normal.Length();
            normal.Normalise();

            return true;
        }

        private static void ProjectCircle(ReVector center, float radius, ReVector axis, out float min, out float max)
        {
            ReVector direction = axis.Normalised();
            ReVector directionByRadius = direction * radius;

            ReVector p1 = center - directionByRadius;
            ReVector p2 = center + directionByRadius;

            min = ReMath.Dot(p1, axis);
            max = ReMath.Dot(p2, axis);
        }
        private static int ClosestVertexIndex(ReVector circlecenter, ReVector[] vertices)
        {
            int result = -1;
            float minDist = float.MaxValue;

            for (int i = 0; i < vertices.Length; i++)
            {
                float distance = ReMath.DistanceSqrd(circlecenter, vertices[i]);

                if (distance < minDist)
                {
                    minDist = distance;
                    result = i;
                }
            }

            return result;
        }

        public static bool IntersectingOBBs(ReVector[] aVertices, ReVector[] bVertices, out ReVector normal, out float depth)
        {
            normal = ReVector.Zero;
            depth = float.MaxValue;

            for (int i = 0; i < aVertices.Length; i++)
            {
                ReVector vA = aVertices[i];
                ReVector vB = aVertices[(i+1)%aVertices.Length]; // loops to beginning of array

                ReVector edge = vB - vA;
                ReVector axis = new ReVector(-edge.Y, edge.X); // normal of the current edge. Inverts

                ProjectVertices(aVertices, axis, out float minA, out float maxA);
                ProjectVertices(bVertices, axis, out float minB, out float maxB);

                if (minA > maxB || minB > maxA) return false;

                float axisDepth = MathF.Min(maxB - minA, maxA - minB);
                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = (maxB > maxA) ? axis : -axis;
                }
            }

            for (int i = 0; i < bVertices.Length; i++)
            {
                ReVector vA = bVertices[i];
                ReVector vB = bVertices[(i + 1) % aVertices.Length]; // loops to beginning of array

                ReVector edge = vB - vA;
                ReVector axis = new ReVector(-edge.Y, edge.X);

                ProjectVertices(aVertices, axis, out float minA, out float maxA);
                ProjectVertices(bVertices, axis, out float minB, out float maxB);

                if (minA > maxB || minB > maxA) return false;

                float axisDepth = MathF.Min(maxB - minA, maxA - minB);
                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = (maxB > maxA) ? axis : -axis;
                }
            }

            depth /= normal.Length();
            normal.Normalise();

            return true;
        }

        private static void ProjectVertices(ReVector[] vertices, ReVector axis, out float min, out float max)
        {
            min = float.MaxValue;
            max = float.MinValue;

            foreach (ReVector v in vertices)
            {
                float proj = ReMath.Dot(v, axis);

                if (proj < min) min = proj;
                if (proj > max) max = proj;
            }
        }

        public static bool IntersectingCircles(ReBody a, ReBody b, out ReVector normal, out float depth)
        {
            normal = ReVector.Zero;
            depth = 0f;

            float distSqrd = ReMath.DistanceSqrd(a.Pos, b.Pos);
            float combinedRadii = a.Radius + b.Radius;


            if (distSqrd > combinedRadii * combinedRadii)
            {
                return false;
            }

            normal = ReMath.Normalise(b.Pos - a.Pos); // b = a + normal
            depth = combinedRadii - MathF.Sqrt(distSqrd);

            return true;
        }
    }
}

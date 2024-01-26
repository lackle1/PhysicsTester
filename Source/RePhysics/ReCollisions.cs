using System;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace RePhysics
{
    public static class ReCollisions
    {
        // Normal always points away from the first body

        public static bool Collide(ReBoxBody bodyA, ReBoxBody bodyB, out ReVector collisionNormal, out float collisionDepth)
        {
            collisionNormal = ReVector.Zero;
            collisionDepth = 0;


            if (bodyA.IsAABB)
            {
                if (bodyB.IsAABB)
                {
                    if (IntersectingAABBs(bodyA, bodyB, out ReVector normal, out float depth))
                    {
                        collisionNormal = normal;
                        collisionDepth = depth;

                        return true;
                    }
                }
                else if (bodyB.IsOBB)
                {
                    if (IntersectingOBBs(bodyA.GetTransformedVertices(), bodyB.PhysicsVer.GetTransformedVertices(), out ReVector normal, out float depth))
                    {
                        collisionNormal = normal;
                        collisionDepth = depth;

                        return true;
                    }
                }
                else if(bodyB.IsCircle)
                {
                    if (IntersectingCircleOBB(bodyB.PhysicsVer, bodyA.GetTransformedVertices(), out ReVector normal, out float depth))
                    {
                        collisionNormal = -normal;
                        collisionDepth = depth;

                        return true;
                    }
                }
            }
            else if (bodyA.IsOBB)
            {
                if (bodyB.IsAABB)
                {
                    if (ReCollisions.IntersectingOBBs(bodyA.PhysicsVer.GetTransformedVertices(), bodyB.GetTransformedVertices(), out ReVector normal, out float depth))
                    {
                        collisionNormal = normal;
                        collisionDepth = depth;

                        return true;
                    }
                }
                else if (bodyB.IsOBB)
                {
                    if (ReCollisions.IntersectingOBBs(bodyA.PhysicsVer.GetTransformedVertices(), bodyB.PhysicsVer.GetTransformedVertices(), out ReVector normal, out float depth))
                    {
                        collisionNormal = normal;
                        collisionDepth = depth;

                        return true;
                    }
                }
                else if (bodyB.IsCircle)
                {
                    if (ReCollisions.IntersectingCircleOBB(bodyB.PhysicsVer, bodyA.PhysicsVer.GetTransformedVertices(), out ReVector normal, out float depth))
                    {
                        collisionNormal = -normal;
                        collisionDepth = depth;

                        return true;
                    }
                }
            }
            else if (bodyA.IsCircle)
            {
                if (bodyB.IsAABB)
                {
                    if (IntersectingCircleOBB(bodyA.PhysicsVer, bodyB.GetTransformedVertices(), out ReVector normal, out float depth))
                    {
                        collisionNormal = normal;
                        collisionDepth = depth;

                        return true;
                    }
                }
                else if (bodyB.IsOBB)
                {
                    if (ReCollisions.IntersectingCircleOBB(bodyA.PhysicsVer, bodyB.PhysicsVer.GetTransformedVertices(), out ReVector normal, out float depth))
                    {
                        collisionNormal = normal;
                        collisionDepth = depth;

                        return true;
                    }
                }
                else if (bodyB.IsCircle)
                {
                    if (ReCollisions.IntersectingCircles(bodyA.PhysicsVer, bodyB.PhysicsVer, out ReVector normal, out float depth))
                    {
                        collisionNormal = normal;
                        collisionDepth = depth;

                        return true;
                    }
                }
            }

            return false;
        }

        public static void FindContactPoints(ReBoxBody bodyA, ReBoxBody bodyB, out ReVector cp1, out ReVector cp2, out int cpCount, out bool noRotation) // cp = contact point
        {
            cp1 = ReVector.Zero;
            cp2 = ReVector.Zero;
            cpCount = 0;

            noRotation = false;

            if (bodyA.IsAABB)
            {
                if (bodyB.IsOBB)
                {
                    FindBoxContactPoints(bodyA.GetTransformedVertices(), bodyB.PhysicsVer.GetTransformedVertices(), out cp1, out cp2, out cpCount, out noRotation);
                }
                else if (bodyB.IsCircle)
                {
                    FindBoxCircleContactPoint(bodyA.GetTransformedVertices(), bodyB.PhysicsVer, out cp1);
                    cpCount = 1;
                }
            }
            else if (bodyA.IsOBB)
            {
                if (bodyA.IsAABB)
                {
                    FindBoxContactPoints(bodyA.PhysicsVer.GetTransformedVertices(), bodyB.GetTransformedVertices(), out cp1, out cp2, out cpCount, out noRotation);
                }
                else if (bodyB.IsOBB)
                {
                    FindBoxContactPoints(bodyA.PhysicsVer.GetTransformedVertices(), bodyB.PhysicsVer.GetTransformedVertices(), out cp1, out cp2, out cpCount, out noRotation);
                }
                else if (bodyB.IsCircle)
                {
                    FindBoxCircleContactPoint(bodyA.PhysicsVer.GetTransformedVertices(), bodyB.PhysicsVer, out cp1);
                    cpCount = 1;
                }
            }
            else if (bodyA.IsCircle)
            {
                if (bodyA.IsAABB)
                {
                    FindBoxCircleContactPoint(bodyB.PhysicsVer.GetTransformedVertices(), bodyA.PhysicsVer, out cp1);
                    cpCount = 1;
                }
                else if (bodyB.IsOBB)
                {
                    FindBoxCircleContactPoint(bodyB.PhysicsVer.GetTransformedVertices(), bodyA.PhysicsVer, out cp1);
                    cpCount = 1;
                }
                else if (bodyB.IsCircle)
                {
                    FindCircleContactPoint(bodyA.Pos, bodyA.PhysicsVer.Radius, bodyB.Pos, out ReVector contactPoint);
                    cp1 = contactPoint;
                    cpCount = 1;
                }
            }
        }

        public static void FindBoxContactPoints(ReVector[] aVertices, ReVector[] bVertices, out ReVector cp1,  out ReVector cp2, out int cpCount, out bool noRotation)
        {
            cp1 = ReVector.Zero;
            cp2 = ReVector.Zero;
            cpCount = 0;
            noRotation = false;

            float minDistSqrd = float.MaxValue;

            float minXA = float.MaxValue; // For testing if the collision should allow rotation or not.
            float maxXA = float.MinValue;
            float minXB = float.MaxValue;
            float maxXB = float.MinValue;

            float minYA = float.MaxValue;
            float minYB = float.MaxValue;

            float highestBodyWidth; // width of the highest body


            for (int i = 0; i < aVertices.Length; i++)
            {
                ReVector p = aVertices[i];

                if (p.X < minXA) minXA = p.X;
                if (p.X > maxXA) maxXA = p.X;

                if (p.Y < minYA) minYA = p.Y;


                for (int j = 0; j < bVertices.Length; j++)
                {
                    ReVector vA = bVertices[j];
                    ReVector vB = bVertices[(j + 1) % bVertices.Length];

                    PointLineSegmentDist(p, vA, vB, out float distSqrd, out ReVector contact);

                    if (ReMath.AboutEqual(distSqrd, minDistSqrd, 0.005f))
                    {
                        if (!ReMath.AboutEqual(cp1, contact, 0.005f))
                        {
                            cp2 = contact;
                            cpCount = 2;
                        }
                    }
                    else if (distSqrd < minDistSqrd)
                    {
                        minDistSqrd = distSqrd;
                        cp1 = contact;
                        cpCount = 1;
                    }
                }
            }

            for (int i = 0; i < bVertices.Length; i++)
            {
                ReVector p = bVertices[i];

                if (p.X < minXB) minXB = p.X;
                if (p.X > maxXB) maxXB = p.X;

                if (p.Y < minYB) minYB = p.Y;

                for (int j = 0; j < aVertices.Length; j++)
                {
                    ReVector vA = aVertices[j];
                    ReVector vB = aVertices[(j + 1) % aVertices.Length];

                    PointLineSegmentDist(p, vA, vB, out float distSqrd, out ReVector contact);

                    if (ReMath.AboutEqual(distSqrd, minDistSqrd, 0.00005f))
                    {
                        if (!ReMath.AboutEqual(cp1, contact, 0.00005f))
                        {
                            cp2 = contact;
                            cpCount = 2;
                        }
                    }
                    if (distSqrd < minDistSqrd)
                    {
                        minDistSqrd = distSqrd;
                        cp1 = contact;
                        cpCount = 1;
                    }
                }
            }

            if (cpCount == 2) // Don't know if 'MathF.Abs(normal.Y) == 1' is needed here.
            {
                if (minYA < minYB)
                {
                    highestBodyWidth = maxXA - minXA;
                }
                else
                {
                    highestBodyWidth = maxXB - minXB;
                }

                if (cp1.X > minXA && cp1.X < maxXA && cp2.X > minXA && cp2.X < maxXA &&
                    !ReMath.AboutEqual(cp2.X - cp1.X, highestBodyWidth / 2, highestBodyWidth * 0.1f)) // center of contact must be close to centre of mass
                {
                    noRotation = true;
                }
                else if (cp1.X > minXB && cp1.X < maxXB && cp2.X > minXB && cp2.X < maxXB &&
                    !ReMath.AboutEqual(cp2.X - cp1.X, highestBodyWidth / 2, highestBodyWidth * 0.1f))
                {
                    noRotation = true;
                }
            }
        }
        public static void FindBoxCircleContactPoint(ReVector[] vertices, RePhysicsBody bodyB, out ReVector cp)
        {
            //ReVector circleCenter = bodyB.Pos;
            cp = ReVector.Zero;
            float minDistSqrd = float.MaxValue;

            for (int i = 0; i < vertices.Length; i++)
            {
                ReVector vA = vertices[i];
                ReVector vB = vertices[(i + 1) % vertices.Length];

                PointLineSegmentDist(bodyB.Pos, vA, vB, out float distSqrd, out ReVector contact);

                if (distSqrd < minDistSqrd)
                {
                    minDistSqrd = distSqrd;
                    cp = contact;
                }
            }
        }
        private static void PointLineSegmentDist(ReVector p, ReVector a, ReVector b, out float distSqrd, out ReVector cp) // point, line endpoint A, line endpoint B
        {
            ReVector ab = b - a;
            ReVector ap = p - a;

            float proj = ReMath.Dot(ap, ab);
            float abLenSqrd = ab.LengthSqrd();
            float d = proj / abLenSqrd; // normalised projection

            if (d <= 0f)
            {
                cp = a;
            }
            else if (d >= 1f)
            {
                cp = b;
            }
            else
            {
                cp = a + ab * d;
            }

            distSqrd = ReMath.DistanceSqrd(p, cp);
        }
        private static void FindCircleContactPoint(ReVector centerA, float radiusA, ReVector centerB, out ReVector cp)
        {
            ReVector ab = centerB - centerA;

            cp = centerA + ab.Normalised() * radiusA;
        }


        public static bool IntersectingRects(ReRect a, ReRect b)
        {
            return a.Left < b.Right && a.Right > b.Left && a.Top < b.Bottom && a.Bottom > b.Top;
        }


        public static bool IntersectingCircleOBB(RePhysicsBody circle, ReVector[] boxVertices, out ReVector normal, out float depth)
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
                //axis.Normalise(); // Unsure on if this is needed

                ProjectVertices(boxVertices, axis, out minA, out maxA);
                ProjectCircle(circle.Pos, circle.Radius, axis, out minB, out maxB);

                if (minA > maxB || minB > maxA) return false;

                axisDepth = MathF.Min(maxB - minA, maxA - minB);
                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = (maxB < maxA) ? axis : -axis;
                }
            }

            int closestPointIndex = ClosestVertexIndex(circle.Pos, boxVertices);
            ReVector closestPoint = boxVertices[closestPointIndex];

            axis = closestPoint - circle.Pos;
            //axis.Normalise(); // Unsure on if this is needed

            ProjectVertices(boxVertices, axis, out minA, out maxA);
            ProjectCircle(circle.Pos, circle.Radius, axis, out minB, out maxB);

            if (minA > maxB || minB > maxA) return false;

            axisDepth = MathF.Min(maxB - minA, maxA - minB);
            if (axisDepth < depth)
            {
                depth = axisDepth;
                normal = (maxB < maxA) ? axis : -axis;
            }

            depth /= normal.Length(); //
            normal.Normalise(); // Remove if normalising axis

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
                //axis.Normalise(); // Unsure on if this is needed

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
                //axis.Normalise(); // Unsure on if this is needed

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

            depth /= normal.Length(); //
            normal.Normalise(); // Remove if normalising axis

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

        public static bool IntersectingCircles(RePhysicsBody a, RePhysicsBody b, out ReVector normal, out float depth)
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

        public static bool IntersectingAABBs(ReBoxBody a, ReBoxBody b, out ReVector normal, out float depth)
        {
            normal = ReVector.Zero;
            depth = 0f;

            if (!a.Intersects(b))
            {
                return false;
            }

            // left, right etc. refers to the sides of body A

            float right = a.Right - b.Left;
            float left = a.Left - b.Right;
            float bottom = a.Bottom - b.Top;
            float top = a.Top - b.Bottom;

            //float right = b.Left - a.Right;
            //float left = b.Right - a.Left;
            //float bottom = b.Top - a.Bottom;
            //float top = b.Bottom - a.Top;

            float dx = ReMath.ClosestTo(0, right, left);
            float dy = ReMath.ClosestTo(0, bottom, top);

            depth = ReMath.ClosestTo(0, dx, dy);
            depth = MathF.Abs(depth);

            if (depth == dx)
            {
                normal = new ReVector(MathF.CopySign(1, dx), 0);
            }
            else
            {
                normal = new ReVector(0, MathF.CopySign(1, dy));
            }

            return true;
        }
    }
}

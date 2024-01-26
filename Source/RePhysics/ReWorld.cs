using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using Grondslag;
using Microsoft.Xna.Framework;
using PhysicsTester;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace RePhysics
{
    public class ReWorld
    {
        public static readonly float MinBodySize = 0.01f * 0.01f; // m^2
        public static readonly float MaxBodySize = 64f * 64f;

        public static readonly float MinDensity = 200f; // kg m^-3
        public static readonly float MaxDensity = 20_000f;

        public static readonly float MetresPerPixel = 0.01f;
        public static readonly float MetresPerPixelSqrd = 0.0001f;
        public static readonly float PixelsPerMetre = 100f;
        public static readonly float PixelsPerMetreSqrd = 10_000f;

        public static readonly int MinIterations = 1;
        public static readonly int MaxIterations = 128;

        private ReVector _gravity;

        private List<ReBoxBody> _bodyList;
        private List<(int, int)> _contactPairs;

        private ReVector[] contactList;
        private ReVector[] impulseList;
        private ReVector[] raList;
        private ReVector[] rbList;

        public List<ReVector> ContactPointsList;

        public int ResolveCount = 0;
        public int StepCount = 0;
        public int UpdateCount = 0;

        public List<ReManifold> ContactManifoldList = new();

        public ReWorld()
        {
            _gravity = new ReVector(0f, 9.81f);
            _bodyList = new List<ReBoxBody>();
            _contactPairs = new List<(int, int)>();

            ContactPointsList = new List<ReVector>();

            contactList = new ReVector[2];
            impulseList = new ReVector[2];
            raList = new ReVector[2];
            rbList = new ReVector[2];
        }

        public int BodyCount
        {
            get { return _bodyList.Count; }
        }
        public void AddBody(ReBoxBody body)
        {
            _bodyList.Add(body);
        }
        public bool RemoveBody(ReBoxBody body)
        {
            return _bodyList.Remove(body);
        }
        public void RemoveBody(int index)
        {
            _bodyList[index].Owner.Remove = true;
            _bodyList.RemoveAt(index);
        }
        public bool GetBody(int index, out ReBoxBody body)
        {
            body = null;
            if (index < 0 || index >= _bodyList.Count)
            {
                return false;
            }

            body = _bodyList[index];
            return true;
        }

        public void Step(float time, int iterations)
        {
            iterations = Math.Clamp(iterations, MinIterations, MaxIterations);
            time /= (float)iterations;

            ContactPointsList.Clear();
            ContactManifoldList.Clear();

            for (int currentIteration = 0; currentIteration < iterations; currentIteration++)
            {
                _contactPairs.Clear();

                // Move bodies
                StepBodies(time);

                // Find collisions
                BroadPhase();

                // Resolve collisions
                NarrowPhase();

                StepCount++;
            }

            UpdateCount++;
        }
        public void StepBodies(float time)
        {
            for (int i = _bodyList.Count - 1; i >= 0; i--)
            {
                _bodyList[i].Step(_gravity, time); // Seems to automatically call the physics version

                if (_bodyList[i].Pos.Y > 2000)
                {
                    RemoveBody(i);
                }
            }
        }
        public void BroadPhase()
        {
            for (int i = 0; i < _bodyList.Count - 1; i++)
            {
                ReBoxBody bodyA = _bodyList[i];

                for (int j = i + 1; j < _bodyList.Count; j++)
                {
                    ReBoxBody bodyB = _bodyList[j];

                    if (bodyA.IsStatic && bodyB.IsStatic)
                    {
                        continue;
                    }

                    if (!ReCollisions.IntersectingRects(bodyA.GetAABB(), bodyB.GetAABB()))
                    {
                        continue;
                    }

                    _contactPairs.Add((i, j));
                }
            }
        }
        public void NarrowPhase()
        {
            for (int i = _contactPairs.Count - 1; i >= 0; i--)
            {
                ReBoxBody bodyA = _bodyList[_contactPairs[i].Item1];
                ReBoxBody bodyB = _bodyList[_contactPairs[i].Item2];

                if (ReCollisions.Collide(bodyA, bodyB, out ReVector normal, out float depth))
                {
                    SeparateBodies(bodyA, bodyB, normal * depth);

                    ReManifold contact = default;

                    if (bodyA.IsAABB && bodyB.IsAABB)
                    {
                        contact = new ReManifold(bodyA, bodyB, normal, depth, ReVector.Zero, ReVector.Zero, 0, true);
                    }
                    else
                    {
                        ReCollisions.FindContactPoints(bodyA, bodyB, out ReVector cp1, out ReVector cp2, out int cpCount, out bool noRotation);
                        contact = new ReManifold(bodyA, bodyB, normal, depth, cp1, cp2, cpCount, noRotation);
                    }

                    ContactManifoldList.Add(contact);

                    ResolveCollision(contact);

                    // For displaying contact points (DEBUG).
                    //if (contact.ContactCount > 0)
                    //{
                    //    if (!ContactPointsList.Contains(contact.Contact1))
                    //    {
                    //        ContactPointsList.Add(contact.Contact1);
                    //    }

                    //    if (contact.ContactCount > 1 && !ContactPointsList.Contains(contact.Contact2))
                    //    {
                    //        ContactPointsList.Add(contact.Contact2);
                    //    }
                    //}
                }                
            }
        }

        public void SeparateBodies(ReBoxBody bodyA, ReBoxBody bodyB, ReVector mtv) // minimun translation vector
        {
            if (bodyA.IsStatic)
            {
                bodyB.Move(mtv);
            }
            else if (bodyB.IsStatic)
            {
                bodyA.Move(-mtv);
            }
            else
            {
                bodyA.Move(-mtv / 2f);
                bodyB.Move(mtv / 2f);
            }
        }
        public void ResolveCollision(in ReManifold contact) // Readonly reference
        {
            ReBoxBody bodyA = contact.BodyA;
            ReBoxBody bodyB = contact.BodyB;
            ReVector normal = contact.Normal;
            float depth = contact.Depth;

            if (!bodyA.IsAABB && !bodyB.IsAABB)
            {
                ResolvePhysicsCollisionWithRotationAndFriction(in contact);
            }
            else if (!bodyA.IsAABB && bodyB.IsAABB)
            {
                ResolveBoxPhysicsCollisionWithRotation(bodyB, bodyA.PhysicsVer, -contact.Normal, in contact); // may have to use -normal
            }
            else if (bodyA.IsAABB && !bodyB.IsAABB)
            {
                ResolveBoxPhysicsCollisionWithRotation(bodyA, bodyB.PhysicsVer, contact.Normal, in contact);
            }
            else if (bodyA.IsAABB && bodyB.IsAABB)
            {
                ReVector relativeVelocity = bodyB.LinearVelocity - bodyA.LinearVelocity;

                if (normal.X * relativeVelocity.X < 0) // Checks if different direction
                {
                    bodyA.LinearVelocityX = 0f;
                    bodyB.LinearVelocityX = 0f;
                }
                if (normal.Y * relativeVelocity.Y < 0)
                {
                    bodyA.LinearVelocityY = 0f;
                    bodyB.LinearVelocityY = 0f;
                }
            }
        }


        // Basic
        public void ResolvePhysicsCollisionBasic(RePhysicsBody bodyA, RePhysicsBody bodyB, ReVector normal, float depth)
        {
            // Using http://www.chrishecker.com/Rigid_Body_Dynamics part 3

            ReVector relativeVelocity = bodyB.LinearVelocity - bodyA.LinearVelocity;

            if (ReMath.Dot(relativeVelocity, normal) > 0f) return; // Checks if bodies are already moving out of each other

            float e = (bodyA.Restitution + bodyB.Restitution) / 2;
            float j = -(1 + e) * ReMath.Dot(relativeVelocity, normal);
            j /= bodyA.InvMass + bodyB.InvMass;

            ReVector impulse = j * normal;

            bodyA.LinearVelocity += -impulse * bodyA.InvMass;
            bodyB.LinearVelocity += impulse * bodyB.InvMass;
        }
        public void ResolveBoxAndPhysicsCollisionBasic(ReBoxBody bodyA, RePhysicsBody bodyB, ReVector normal, float depth)
        {
            ReVector relativeVelocity = bodyB.LinearVelocity - bodyA.LinearVelocity;

            if (ReMath.Dot(relativeVelocity, normal) > 0f) return; // Checks if bodies are already moving out of each other

            float e = (bodyA.Restitution + bodyB.Restitution) / 2;
            float j = -(1 + e) * ReMath.Dot(relativeVelocity, normal);
            j /= bodyB.InvMass;

            ReVector impulse = j * normal;

            bodyB.LinearVelocity += impulse * bodyB.InvMass;
        }


        // With rotation
        public void ResolvePhysicsCollisionWithRotationOld(in ReManifold contact) // Calculating for both contact points
        {
            RePhysicsBody bodyA = contact.BodyA.PhysicsVer;
            RePhysicsBody bodyB = contact.BodyB.PhysicsVer;
            ReVector normal = contact.Normal;
            ReVector contact1 = contact.Contact1;
            ReVector contact2 = contact.Contact2;
            int contactCount = contact.ContactCount;

            float e = MathF.Min(bodyA.Restitution, bodyB.Restitution);

            this.contactList[0] = contact1;
            this.contactList[1] = contact2;

            for (int i = 0; i < contactCount; i++)
            {
                this.impulseList[i] = ReVector.Zero;
                this.raList[i] = ReVector.Zero;
                this.rbList[i] = ReVector.Zero;
            }

            for (int i = 0; i < contactCount; i++)
            {
                ReVector ra = contactList[i] - bodyA.Pos;
                ReVector rb = contactList[i] - bodyB.Pos;

                raList[i] = ra;
                rbList[i] = rb;

                ReVector raPerp = new ReVector(-ra.Y, ra.X);
                ReVector rbPerp = new ReVector(-rb.Y, rb.X);

                ReVector angularLinearVelocityA = raPerp * bodyA.AngularVelocity;
                ReVector angularLinearVelocityB = rbPerp * bodyB.AngularVelocity;

                ReVector relativeVelocity =
                    (bodyB.LinearVelocityPixels + angularLinearVelocityB) -
                    (bodyA.LinearVelocityPixels + angularLinearVelocityA);

                float contactVelocityMag = ReMath.Dot(relativeVelocity, normal);

                if (contactVelocityMag > 0f)
                {
                    continue;
                }

                float raPerpDotN = ReMath.Dot(raPerp, normal);
                float rbPerpDotN = ReMath.Dot(rbPerp, normal);

                float denom = bodyA.InvMass + bodyB.InvMass +
                    (raPerpDotN * raPerpDotN) * bodyA.InvMomentOfInertia +
                    (rbPerpDotN * rbPerpDotN) * bodyB.InvMomentOfInertia;

                float j = -(1f + e) * contactVelocityMag;
                j /= denom;
                j /= (float)contactCount;

                ReVector impulse = j * normal;
                impulseList[i] = impulse;
            }

            for (int i = 0; i < contactCount; i++)
            {
                ReVector impulse = impulseList[i];
                ReVector ra = raList[i];
                ReVector rb = rbList[i];

                bodyA.LinearVelocityPixels += -impulse * bodyA.InvMass;
                bodyA.AngularVelocity += -ReMath.Cross(ra, impulse) * bodyA.InvMomentOfInertia;
                bodyB.LinearVelocityPixels += impulse * bodyB.InvMass;
                bodyB.AngularVelocity += ReMath.Cross(rb, impulse) * bodyB.InvMomentOfInertia;
            }
        }

        public void ResolvePhysicsCollisionWithRotation(in ReManifold contact) // Average of contact points
        {
            RePhysicsBody bodyA = contact.BodyA.PhysicsVer;
            RePhysicsBody bodyB = contact.BodyB.PhysicsVer;
            ReVector normal = contact.Normal;
            ReVector contact1 = contact.Contact1;
            ReVector contact2 = contact.Contact2;
            int contactCount = contact.ContactCount;

            ReVector avgContact = contact1;

            if (contactCount == 2)
            {
                avgContact += contact2;
                avgContact /= 2f;
            }
            for (int i = 0; i < contactCount; i++)
            {
                impulseList[i] = ReVector.Zero;
                raList[i] = ReVector.Zero;
                rbList[i] = ReVector.Zero;
            }

            float e = (bodyA.Restitution + bodyB.Restitution) / 2;

            ReVector ra = (avgContact - bodyA.Pos);
            ReVector rb = (avgContact - bodyB.Pos);

            ReVector rAPerp = new ReVector(-ra.Y, ra.X);
            ReVector rBPerp = new ReVector(-rb.Y, rb.X);

            ReVector pointLinearVelocityA = rAPerp * bodyA.AngularVelocity;
            ReVector pointLinearVelocityB = rBPerp * bodyB.AngularVelocity;

            ReVector relativeVelocity =
                bodyB.LinearVelocityPixels + pointLinearVelocityB
                - (bodyA.LinearVelocityPixels + pointLinearVelocityA);

            float contactVelocityMagnitude = ReMath.Dot(relativeVelocity, normal);

            if (contactVelocityMagnitude > 0f)
            {
                return;
            }

            float rAPerpDotN = ReMath.Dot(rAPerp, normal);
            float rBPerpDotN = ReMath.Dot(rBPerp, normal);

            float denominator = bodyA.InvMass + bodyB.InvMass +
                (rAPerpDotN * rAPerpDotN) * bodyA.InvMomentOfInertia +
                (rBPerpDotN * rBPerpDotN) * bodyB.InvMomentOfInertia;

            float j = -(1 + e) * contactVelocityMagnitude;
            j /= denominator;

            ReVector impulse = j * normal;

            bodyA.LinearVelocityPixels += -impulse * bodyA.InvMass;
            bodyA.AngularVelocity += -ReMath.Cross(ra, impulse) * bodyA.InvMomentOfInertia;

            bodyB.LinearVelocityPixels += impulse * bodyB.InvMass;
            bodyB.AngularVelocity += ReMath.Cross(rb, impulse) * bodyB.InvMomentOfInertia;
        }
        public void ResolveBoxPhysicsCollisionWithRotation(ReBoxBody bodyA, RePhysicsBody bodyB, ReVector normal, in ReManifold contact) // Average of contact points
        {
            ReVector contact1 = contact.Contact1;
            ReVector contact2 = contact.Contact2;
            int contactCount = contact.ContactCount;

            ReVector avgContact = contact1;

            if (contactCount == 2)
            {
                avgContact += contact2;
                avgContact /= 2f;
            }
            for (int i = 0; i < contactCount; i++)
            {
                impulseList[i] = ReVector.Zero;
                raList[i] = ReVector.Zero;
                rbList[i] = ReVector.Zero;
            }

            float e = (bodyA.Restitution + bodyB.Restitution) / 2;

            ReVector ra = (avgContact - bodyA.Pos);
            ReVector rb = (avgContact - bodyB.Pos);

            ReVector rAPerp = new ReVector(-ra.Y, ra.X);
            ReVector rBPerp = new ReVector(-rb.Y, rb.X);

            ReVector pointLinearVelocityB = rBPerp * bodyB.AngularVelocity;

            ReVector relativeVelocity =
                bodyB.LinearVelocityPixels + pointLinearVelocityB
                - (bodyA.LinearVelocityPixels);

            float contactVelocityMagnitude = ReMath.Dot(relativeVelocity, normal);

            if (contactVelocityMagnitude > 0f)
            {
                return;
            }

            float rAPerpDotN = ReMath.Dot(rAPerp, normal);
            float rBPerpDotN = ReMath.Dot(rBPerp, normal);

            float denominator = bodyB.InvMass +
                (rBPerpDotN * rBPerpDotN) * bodyB.InvMomentOfInertia;

            float j = -(1 + e) * contactVelocityMagnitude;
            j /= denominator;

            ReVector impulse = j * normal;

            bodyB.LinearVelocityPixels += impulse * bodyB.InvMass;
            bodyB.AngularVelocity += ReMath.Cross(rb, impulse) * bodyB.InvMomentOfInertia;
        }


        // With rotation and friction
        public void ResolvePhysicsCollisionWithRotationAndFriction(in ReManifold contact) // Average of contact points
        {
            RePhysicsBody bodyA = contact.BodyA.PhysicsVer;
            RePhysicsBody bodyB = contact.BodyB.PhysicsVer;
            ReVector normal = contact.Normal;
            ReVector contact1 = contact.Contact1;
            ReVector contact2 = contact.Contact2;
            int contactCount = contact.ContactCount;

            ReVector avgContact = contact1;

            if (contactCount == 2)
            {
                avgContact += contact2;
                avgContact /= 2f;
            }

            float e = (bodyA.Restitution + bodyB.Restitution) / 2f;
            //e = 0.05f;
            
            float staticFriction = (bodyA.StaticFriction + bodyB.StaticFriction) * 0.5f;
            float dynamicFriction = (bodyA.DynamicFriction + bodyB.DynamicFriction) * 0.5f;

            //bodyB.LinearVelocityPixels = ReVector.Zero;
            //bodyB.AngularVelocity = 0f;
            //bodyB.Angle = 4.71238888038f;
            //            4.71238898038

            // Everything seems to be good when angle is a multiple of half of pi
            // Try incrementing AngularVelocity by a value that brings Angle closer to the nearest multiple of PI / 2
            // Always oppose current velocity as well

            // bodyB.AngularVelocity +=

            //if (ReMath.AboutEqual(bodyB.AngularVelocity, 0f, 0.005f))
            //{
            //    bodyB.AngularVelocity = 0f;

            //    float closestAngle = bodyB.Angle / MathF.PI / 2;
            //    closestAngle = MathF.Round(closestAngle);
            //    bodyB.Angle = closestAngle;
            //    //if (ReMath.AboutEqual(bodyB.Angle, closestAngle, 0.005))
            //    //{

            //    //}
            //}

            ReVector ra = avgContact - bodyA.Pos;
            ReVector rb = avgContact - bodyB.Pos;

            //if (ReMath.AboutEqual(rb.X, 0f, 0.005f))
            //{
            //    rb.X = 0f;
            //}
            //if (ReMath.AboutEqual(rb.Y, 0f, 0.005f))
            //{
            //    rb.Y = 0f;
            //}

            //if (ReMath.AboutEqual(bodyA.LinearVelocityPixels.X, 0f, 0.005f))
            //{
            //    bodyA.LinearVelocityPixels = new ReVector(0f, bodyA.LinearVelocityPixels.Y);
            //}
            //if (ReMath.AboutEqual(bodyA.AngularVelocity, 0f, 0.005f))
            //{
            //    bodyA.AngularVelocity = 0f;
            //}

            //if (ReMath.AboutEqual(bodyB.LinearVelocityPixels.X, 0f, 0.05f))
            //{
            //    bodyB.LinearVelocityPixels = new ReVector(0f, bodyB.LinearVelocityPixels.Y);
            //}
            //if (ReMath.AboutEqual(bodyB.AngularVelocity, 0f, 0.0005f))
            //{
            //    bodyB.AngularVelocity = 0f;
            //}

            if (ReMath.AboutEqual(normal.X, 0f, 0.0005f))
            {
                normal.X = 0f;
            }

            ReVector rAPerp = new ReVector(-ra.Y, ra.X);
            ReVector rBPerp = new ReVector(-rb.Y, rb.X);

            ReVector pointLinearVelocityA = rAPerp * bodyA.AngularVelocity;
            ReVector pointLinearVelocityB = rBPerp * bodyB.AngularVelocity;

            ReVector relativeVelocity =
                bodyB.LinearVelocityPixels + pointLinearVelocityB
                - (bodyA.LinearVelocityPixels + pointLinearVelocityA);


            #region Normal

            float contactVelocityMagnitude = ReMath.Dot(relativeVelocity, normal);

            if (contactVelocityMagnitude > 0f)
            {
                return;
            }

            float rAPerpDotN = ReMath.Dot(rAPerp, normal);
            float rBPerpDotN = ReMath.Dot(rBPerp, normal);

            float denominator = bodyA.InvMass + bodyB.InvMass +
                (rAPerpDotN * rAPerpDotN) * bodyA.InvMomentOfInertia +
                (rBPerpDotN * rBPerpDotN) * bodyB.InvMomentOfInertia;

            float j = -(1 + e) * contactVelocityMagnitude;
            j /= denominator;

            ReVector impulse = j * normal;

            bodyA.LinearVelocityPixels += -impulse * bodyA.InvMass;
            bodyA.AngularVelocity += -ReMath.Cross(ra, impulse) * bodyA.InvMomentOfInertia * 0.5f;

            bodyB.LinearVelocityPixels += impulse * bodyB.InvMass;
            bodyB.AngularVelocity += ReMath.Cross(rb, impulse) * bodyB.InvMomentOfInertia * 0.5f;

            #endregion


            #region Friction

            ReVector tangent = relativeVelocity - ReMath.Dot(relativeVelocity, normal) * normal;
            if (ReMath.AboutEqual(tangent, ReVector.Zero, 0.005f))
            {
                if (contact.NoRotation)
                {
                    if (!bodyA.IsStatic && !bodyA.IsAABB)
                    {
                        bodyA.Angle = ReMath.ClosestMultipleOf(bodyA.Angle, MathF.PI / 2);
                        bodyA.AngularVelocity = 0f;

                        if (MathF.Abs(bodyA.LinearVelocityPixels.X) < 0.05)
                        {
                            bodyA.LinearVelocityPixels = new ReVector(0f, bodyA.LinearVelocityPixels.Y);
                        }
                    }

                    if (!bodyB.IsStatic && !bodyB.IsAABB)
                    {
                        bodyB.Angle = ReMath.ClosestMultipleOf(bodyB.Angle, MathF.PI / 2);
                        bodyB.AngularVelocity = 0f;

                        if (MathF.Abs(bodyB.LinearVelocityPixels.X) < 0.05)
                        {
                            bodyB.LinearVelocityPixels = new ReVector(0f, bodyB.LinearVelocityPixels.Y);
                        }
                    }
                }

                return;
            }

            tangent.Normalise();

            float rAPerpDotT = ReMath.Dot(rAPerp, tangent);
            float rBPerpDotT = ReMath.Dot(rBPerp, tangent);

            denominator = bodyA.InvMass + bodyB.InvMass +
                (rAPerpDotT * rAPerpDotT) * bodyA.InvMomentOfInertia +
                (rBPerpDotT * rBPerpDotT) * bodyB.InvMomentOfInertia;

            float jt = ReMath.Dot(relativeVelocity, tangent);
            jt /= denominator;

            ReVector frictionImpulse;

            if (MathF.Abs(jt) <= j * staticFriction)
            {
                frictionImpulse = -jt * tangent;
            }
            else
            {
                frictionImpulse = -j * tangent * dynamicFriction;
            }


            bodyA.LinearVelocityPixels += ReMath.Clamp(frictionImpulse * bodyA.InvMass, ReVector.Zero, -bodyA.LinearVelocityPixels);
            //bodyA.LinearVelocityPixels += frictionImpulse * bodyA.InvMass;
            bodyA.AngularVelocity += -ReMath.Cross(ra, frictionImpulse) * bodyA.InvMomentOfInertia;

            bodyB.LinearVelocityPixels += ReMath.Clamp(frictionImpulse * bodyB.InvMass, ReVector.Zero, -bodyB.LinearVelocityPixels);
            //bodyB.LinearVelocityPixels += frictionImpulse * bodyB.InvMass;
            bodyB.AngularVelocity += ReMath.Cross(rb, frictionImpulse) * bodyB.InvMomentOfInertia;

            #endregion

            // Extra acceleration towards zero velocity

            //ReVector linear = new ReVector(-MathF.CopySign(0.005f, bodyA.LinearVelocityPixels.X), -MathF.CopySign(0.005f, bodyA.LinearVelocityPixels.Y));
            //float angular = -MathF.CopySign(0.0005f, bodyA.AngularVelocity);


            //linear = new ReVector(-MathF.CopySign(0.005f, bodyB.LinearVelocityPixels.X), -MathF.CopySign(0.005f, bodyB.LinearVelocityPixels.Y));
            //angular = -MathF.CopySign(0.0005f, bodyB.AngularVelocity);

            if (contact.NoRotation)
            {
                if (!bodyA.IsStatic && !bodyA.IsAABB)
                {
                    bodyA.Angle = ReMath.ClosestMultipleOf(bodyA.Angle, MathF.PI / 2);
                    bodyA.AngularVelocity = 0f;

                    if (MathF.Abs(bodyA.LinearVelocityPixels.X) < 0.05)
                    {
                        bodyA.LinearVelocityPixels = new ReVector(0f, bodyA.LinearVelocityPixels.Y);
                    }
                }

                if (!bodyB.IsStatic && !bodyB.IsAABB)
                {
                    bodyB.Angle = ReMath.ClosestMultipleOf(bodyB.Angle, MathF.PI / 2);
                    bodyB.AngularVelocity = 0f;

                    if (MathF.Abs(bodyB.LinearVelocityPixels.X) < 0.05)
                    {
                        bodyB.LinearVelocityPixels = new ReVector(0f, bodyB.LinearVelocityPixels.Y);
                    }
                }
            }
        }
    }
}

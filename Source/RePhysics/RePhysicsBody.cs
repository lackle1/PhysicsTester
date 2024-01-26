using SharpDX.Direct3D9;
using System;
using System.Numerics;
using System.Security;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace RePhysics
{
    public enum ShapeType
    {
        AABB = 0,
        OBB = 1,
        Circle = 2
    }

    public class RePhysicsBody : ReBoxBody // Ragdoll limb/body part will inherit from RePhysicsBody
    {
        private float _angle; // radians
        private float _angularVelocity;
        protected ReVector _force; // force acting on this body - N (m)

        public readonly float Mass; // kg
        public readonly float InvMass;
        public readonly float Density; // kg m^-3
        public readonly float Area; // m^2
        public readonly float MomentOfInertia; // How much the object resists rotational acceleration
        public readonly float InvMomentOfInertia;

        public readonly float StaticFriction; // While object is stationary
        public readonly float DynamicFriction; // While object is moving

        public readonly float Radius;

        public RePhysicsBody(ICollidable owner, ReVector posOffset, float angle, float mass, float density, float restitution, float area, bool isStatic, float radius, float width, float height, ShapeType shapeType)
            : base(owner, posOffset, width, height, isStatic)
        {
            _posOffset = ReVector.Zero;
            Angle = angle;
            _angularVelocity = 0;
            _force = ReVector.Zero;

            Mass = mass;
            Density = density;
            Restitution = restitution;
            Area = area;
            Radius = radius;
            ShapeType = shapeType;
            MomentOfInertia = GetMomentOfInertia();

            StaticFriction = 0.6f;
            DynamicFriction = 0.3f;

            if (!IsStatic)
            {
                InvMass = 1f / Mass;
                InvMomentOfInertia = 1f / MomentOfInertia;
            }
            else
            {
                InvMass = 0;
                InvMomentOfInertia = 0;
            }

            if (IsOBB)
            {
                _vertices = CreateVertices();
                _transformedVertices = new ReVector[4];
                _transformUpdateRequired = true;
            }
        }

        public float Angle
        {
            get { return Owner.Angle; }
            set { Owner.Angle = value; }
        }
        public float AngularVelocity
        {
            get { return _angularVelocity; }
            internal set { _angularVelocity = value; }
        }

        internal override void Step(ReVector gravity, float time)
        {
            if (IsStatic)
            {
                return;
            }

            //Angle = 0f;
            //AngularVelocity = 0f;

            ReVector acceleration = _force / Mass;
            acceleration += gravity;
            _linearVelocity += acceleration * time;

            Pos += _linearVelocity * time * ReWorld.PixelsPerMetre;

            Angle += AngularVelocity * time;

            _force = ReVector.Zero;

            _transformUpdateRequired = true;
        }

        public void ApplyForce(ReVector amount)
        {
            _force += amount;
        }
        public void SetForce(ReVector force)
        {
            _force = force;
        }

        public override void Move(ReVector amount)
        {
            base.Move(amount);
            _transformUpdateRequired = true;
        }
        public override void MoveTo(ReVector pos)
        {
            base.MoveTo(pos);
            _transformUpdateRequired = true;
        }

        public void Rotate(float amount)
        {
            Angle += amount;
            _transformUpdateRequired = true;
        }
        public void RotateTo(float angle)
        {
            Angle = angle;
            _transformUpdateRequired = true;
        }

        //public bool CollidesWithCircle(ReBody b, out ReVector normal, out float depth)
        //{
        //    normal = ReVector.Zero;
        //    depth = 0f;

        //    if (IsCircle)
        //    {
        //        float distSqrd = ReMath.DistanceSqrd(Pos, b.Pos);
        //        float combinedRadii = Radius + b.Radius;


        //        if (distSqrd > combinedRadii * combinedRadii)
        //        {
        //            return false;
        //        }

        //        normal = ReMath.Normalise(b.Pos - Pos); // b = a + normal // points towards B
        //        depth = combinedRadii - MathF.Sqrt(distSqrd);

        //        return true;
        //    }

        //    return false;
        //}

        public override ReVector[] GetTransformedVertices()
        {
            if (_transformUpdateRequired)
            {
                ReTransform transform = new ReTransform(Pos, Angle);

                for (int i = 0; i < _vertices.Length; i++)
                {
                    _transformedVertices[i] = _vertices[i].Transform(transform);
                }
            }
            
            _transformUpdateRequired = false;
            return _transformedVertices;
        }
        public override ReRect GetAABB()
        {
            if (IsOBB)
            {
                ReVector[] vertices = GetTransformedVertices();

                float left = float.MaxValue;
                float right = float.MinValue;
                float top = float.MaxValue;
                float bottom = float.MinValue;

                foreach (var v in vertices)
                {
                    if (v.X < left) { left = v.X; }
                    if (v.X > right) { right = v.X; }
                    if (v.Y < top) { top = v.Y; }
                    if (v.Y > bottom) { bottom = v.Y; }
                }

                return new ReRect(left, right, top, bottom);
            }
            else if (IsCircle)
            {
                return new ReRect(Pos.X - Radius, Pos.X + Radius, Pos.Y - Radius, Pos.Y + Radius);
            }
            else
            {
                throw new Exception("Unknown shape type.");
            }
        }

        public float GetMomentOfInertia()
        {
            if (IsOBB)
            {
                return (1f / 12f) * Mass * (Height * Height + Width * Width);
            }
            else if (IsCircle)
            {
                return 0.5f * Mass * Radius * Radius;
            }
            else
            {
                throw new Exception("Unknown ShapeType.");
            }
        }
    }
}

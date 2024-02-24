using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace RePhysics
{
    public class RePhysicsBody : ReBoxBody // For physics stuff
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

        private readonly static int _maxNumStoredValues = 240; // Static so that it can be referenced below
        private int _numStoredValues;
        private ReVector[] _recentPositions = new ReVector[_maxNumStoredValues];
        private float[] _recentAngles = new float[_maxNumStoredValues];
        private int _currentIndex = 0;

        private List<ReJoint> _jointList;

        public RePhysicsBody(ICollidable owner, ReVector posOffset, float angle, float mass, float density, float restitution, float area, bool isStatic, float radius, float width, float height, ShapeType shapeType, int layer)
            : base(owner, posOffset, width, height, isStatic, layer)
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

            _jointList = new List<ReJoint>();
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

            RecordPositionAndAngle();
            GetAveragePositionAndAngle(out ReVector avgPosition, out float avgAngle);

            

            ReVector acceleration = _force / Mass;
            acceleration += gravity;
            LinearVelocity += acceleration * time;

            Move(LinearVelocity * time * ReWorld.PixelsPerMetre);

            Angle += AngularVelocity * time; //  * 0.995f

            _force = ReVector.Zero;

            _transformUpdateRequired = true;

            //if (_numStoredValues >= _maxNumStoredValues && ReMath.AboutEqual(avgPosition, LinearVelocity, 0.05f) && ReMath.AboutEqual(avgAngle, AngularVelcoity, 0.05f))
            if (_numStoredValues >= _maxNumStoredValues && ReMath.AboutEqual(avgPosition, Pos, 0.05f) && ReMath.AboutEqual(avgAngle, Angle, 0.0005f))
            {
                if (ReMath.AboutEqual(LinearVelocity, ReVector.Zero, 0.05f) && ReMath.AboutEqual(AngularVelocity, 0f, 0.005f))
                {
                    LinearVelocity = ReVector.Zero;
                    AngularVelocity = 0f;
                }
            }
        }

        private void RecordPositionAndAngle()
        {
            _recentPositions[_currentIndex] = Pos;
            _recentAngles[_currentIndex] = Angle;

            //_recentPositions[_currentIndex] = LinearVelocity;
            //_recentAngles[_currentIndex] = AngularVelocity;

            _currentIndex = (_currentIndex + 1) % _maxNumStoredValues;
            _numStoredValues = Math.Min(_numStoredValues + 1, _maxNumStoredValues);
        }
        private void GetAveragePositionAndAngle(out ReVector avgPosition, out float avgAngle)
        {
            ReVector totalPosition = ReVector.Zero;
            float totalAngle = 0f;

            for (int i = 0; i < _numStoredValues; i++)
            {
                totalPosition += _recentPositions[i];
                totalAngle += _recentAngles[i];
            }

            avgPosition = totalPosition / _numStoredValues;
            avgAngle = totalAngle / _numStoredValues;
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

        public void SnapToAngle() // Snaps to 0, 90, 180, 270, etc.
        {
            Angle = ReMath.ClosestMultipleOf(Angle, MathF.PI / 2);
        }

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
        public ReVector GetTransformedPoint(ReVector pos)
        {
            return pos.Transform(new ReTransform(Pos, Angle));
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
                return (1f / 12f) * Mass * (Height * Height + Width * Width) * 2;
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

        public void AddJoint(ReJoint joint)
        {
            _jointList.Add(joint);
        }
    }
}

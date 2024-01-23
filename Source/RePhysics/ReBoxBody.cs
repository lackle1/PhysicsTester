using System;

using Grondslag;
using Microsoft.Xna.Framework;
using PhysicsTester;

namespace RePhysics
{
    public class ReBoxBody
    {
        public ICollidable Owner { get; protected set; }

        protected ReVector _posOffset; // px - offset from owner's position, 
        protected ReVector _linearVelocity; // m s^-1
        private ReVector _acceleration; // m s^-2

        public float Restitution;
        public readonly bool IsStatic;

        protected ReVector[] _vertices;
        protected ReVector[] _transformedVertices;
        protected bool _transformUpdateRequired;

        public ShapeType ShapeType;

        public ReBoxBody(ICollidable owner, ReVector posOffset, float width, float height, bool isStatic)
        {
            Owner = owner;

            //_posOffset = new ReVector(-width / 2, -height / 2);
            _linearVelocity = ReVector.Zero;

            Restitution = 0f;
            IsStatic = isStatic;

            Width = width;
            Height = height;

            _vertices = CreateVertices();
            _transformedVertices = new ReVector[4];
            _transformUpdateRequired = true;
        }

        #region Properties
        public float Width { get; set; }
        public float Height { get; set; }

        public float Left
        { 
            get { return Pos.X - Width / 2; }
            set { Pos = new ReVector(value, Pos.Y); } 
        }
        public float Right
        {
            get { return Pos.X + Width / 2; }
            set { Pos = new ReVector(value - Width, Pos.Y); }
        }
        public float Top
        {
            get { return Pos.Y - Height / 2; }
            set { Pos = new ReVector(Pos.X, value); }
        }
        public float Bottom
        {
            get { return Pos.Y + Height / 2; }
            set { Pos = new ReVector(Pos.X, value - Height); }
        }

        public bool IsAABB
        {
            get { return ShapeType == ShapeType.AABB; }
        }
        public bool IsOBB
        {
            get { return ShapeType == ShapeType.OBB; }
        }
        public bool IsCircle
        {
            get { return ShapeType == ShapeType.Circle; }
        }

        public RePhysicsBody PhysicsVer
        {
            get { return (RePhysicsBody)this; }
        }

        public ReVector Pos
        {
            get { return OwnerPos + _posOffset; }
            set { Owner.Pos = ReConverter.ToMGVector2(value - _posOffset); }
        }
        public ReVector OwnerPos
        {
            get { return ReConverter.ReVector(Owner.Pos); }
            set { Owner.Pos = ReConverter.ToMGVector2(value); }
        }

        public ReVector LinearVelocity
        {
            get { return _linearVelocity; }
            internal set { _linearVelocity = value; }
        }
        public ReVector LinearVelocityPixels
        {
            get { return _linearVelocity * ReWorld.PixelsPerMetre; }
            internal set { _linearVelocity = value * ReWorld.MetresPerPixel; }
        }
        public float LinearVelocityX
        {
            get { return _linearVelocity.X; }
            internal set { _linearVelocity.X = value; }
        }
        public float LinearVelocityY
        {
            get { return _linearVelocity.Y; }
            internal set { _linearVelocity.Y = value; }
        }
        #endregion

        internal virtual void Step(ReVector gravity, float time)
        {
            if (IsStatic)
            {
                return;
            }

            _acceleration += gravity;
            _linearVelocity += _acceleration * time;

            Pos += _linearVelocity * time * ReWorld.PixelsPerMetre;

            _acceleration = ReVector.Zero;
            _transformUpdateRequired = true;
        }

        public bool Intersects(ReBoxBody box)
        {
            return Left < box.Right && Right > box.Left && Top < box.Bottom && Bottom > box.Top;
        }

        public virtual void Accelerate(ReVector amount)
        {
            _acceleration += amount;
        }
        public virtual void SetAcceleration(ReVector force)
        {
            _acceleration = force;
        }

        public virtual void Move(ReVector amount)
        {
            Pos += amount;
            _transformUpdateRequired = true;
        }
        public virtual void MoveTo(ReVector pos)
        {
            Pos = pos;
            _transformUpdateRequired = true;
        }

        //public virtual Rectangle GetAABB()
        //{

        //}

        public virtual ReVector[] CreateVertices()
        {
            float left = -Width / 2;
            float right = left + Width;
            float top = -Height / 2;
            float bottom = top + Height;

            ReVector[] vertices = new ReVector[4];
            vertices[0] = new ReVector(left, top);
            vertices[1] = new ReVector(right, top);
            vertices[2] = new ReVector(right, bottom);
            vertices[3] = new ReVector(left, bottom);

            return vertices;
        }
        public virtual ReVector[] GetTransformedVertices()
        {
            _transformedVertices[0] = new ReVector(Left, Top);
            _transformedVertices[1] = new ReVector(Right, Top);
            _transformedVertices[2] = new ReVector(Right, Bottom);
            _transformedVertices[3] = new ReVector(Left, Bottom);

            _transformUpdateRequired = false;

            return _transformedVertices;
        }
        public virtual ReRect GetAABB()
        {
            if (IsAABB)
            {
                return new ReRect(Left, Right, Top, Bottom);
            }
            else
            {
                return PhysicsVer.GetAABB(); // This is apparently unnecessary, for some reason, as it will go straight to the overriding version
            }
        }

        public static bool CreateCircleBody(ICollidable owner, ReVector posOffset, float radius, float angle, float density, bool isStatic, float restitution, out RePhysicsBody body, out string errorMessage)
        {
            body = null;
            errorMessage = string.Empty;

            float area = MathF.PI * radius * radius * ReWorld.MetresPerPixelSqrd;

            if (area < ReWorld.MinBodySize || area > ReWorld.MaxBodySize)
            {
                errorMessage = "Circle volume is either too large or small";
                return false;
            }

            if (density < ReWorld.MinDensity || density > ReWorld.MaxDensity)
            {
                errorMessage = "Circle density is either too high or low";
                return false;
            }

            float mass = area * density;

            restitution = ReMath.Clamp(restitution, 0, 1);

            body = new RePhysicsBody(owner, posOffset, angle, mass, density, restitution, area, isStatic, radius, 0, 0, ShapeType.Circle);

            return true;
        }

        public static bool CreateAABBBody(ICollidable owner, ReVector posOffset, float width, float height, bool isStatic, out ReBoxBody body)
        {                      
            body = new ReBoxBody(owner, posOffset, (int)width, (int)height, isStatic);

            return true;
        }

        public static bool CreateOBBBody(ICollidable owner, ReVector posOffset, float width, float height, float angle, float density, bool isStatic, float restitution, out RePhysicsBody body, out string errorMessage)
        {
            body = null;
            errorMessage = string.Empty;

            float area = width * height * ReWorld.MetresPerPixelSqrd;

            if (area < ReWorld.MinBodySize || area > ReWorld.MaxBodySize)
            {
                errorMessage = "Rectangle area out of range";
                return false;
            }

            if (density < ReWorld.MinDensity || density > ReWorld.MaxDensity)
            {
                errorMessage = "Rectangle density out of range";
                return false;
            }

            float mass = area * density;

            restitution = ReMath.Clamp(restitution, 0, 1);

            body = new RePhysicsBody(owner, posOffset, angle, mass, density, restitution, area, isStatic, 0, width, height, ShapeType.OBB);

            return true;
        }
    }
}

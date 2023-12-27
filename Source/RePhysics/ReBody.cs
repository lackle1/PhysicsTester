using System;

namespace RePhysics
{
    public enum ShapeType
    {
        AABB = 0,
        OBB = 1,
        Circle = 2
    }

    public class ReBody // Ragdoll limb/body part will inherit from PBody
    {
        private ICollidable _owner;

        private ReVector _posOffset;
        private ReVector _linearVelocity;
        private float _rotation;
        private float _rotationalVelocity;

        public readonly float Mass; // kg
        public readonly float Density; // g cm^-3
        public readonly float Restitution; // Ahhhh... Hmm.
        public readonly float Area; // m^2

        public readonly bool Static;

        public readonly float Radius;
        public readonly float Width;
        public readonly float Height;

        private readonly ReVector[] _vertices;
        private ReVector[] _transformedVertices;
        private bool _transformUpdateRequired;

        public readonly ShapeType ShapeType;

        public ReBody(ICollidable owner, ReVector posOffset, float rotation, float mass, float density, float restitution, float area, bool isStatic, float radius, float width, float height, ShapeType shapeType)
        {
            _owner = owner;

            _posOffset = posOffset;
            _linearVelocity = ReVector.Zero;
            _rotation = rotation;
            _rotationalVelocity = 0;

            Mass = mass;
            Density = density;
            Restitution = restitution;
            Area = area;
            
            Static = isStatic;

            Radius = radius;
            Width = width;
            Height = height;

            ShapeType = shapeType;

            if (ShapeType == ShapeType.OBB)
            {
                _vertices = CreateVertices();
                _transformedVertices = new ReVector[4];
                _transformUpdateRequired = true;
            }
        }

        public ReVector Pos
        {
            get { return ReConverter.ToPVector(_owner.Pos) + _posOffset; }
            set { _owner.Pos = ReConverter.ToMGVector2(value - _posOffset); }
        }
        public ReVector BodyPos
        {
            get { return ReConverter.ToPVector(_owner.Pos); }
            set { _owner.Pos = ReConverter.ToMGVector2(value); }
        }

        public ReVector[] CreateVertices()
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
        public ReVector[] GetTransformedVertices()
        {
            if (_transformUpdateRequired)
            {
                ReTransform transform = new ReTransform(BodyPos, _rotation);

                for (int i = 0; i < _vertices.Length; i++)
                {
                    _transformedVertices[i] = _vertices[i].Transform(transform);
                }
            }

            _transformUpdateRequired = false;
            return _transformedVertices;
        }

        public void Move(ReVector amount)
        {
            Pos += amount;
            _transformUpdateRequired = true;
        }
        public void MoveTo(ReVector pos)
        {
            Pos = pos;
            _transformUpdateRequired = true;
        }

        public void Rotate(float amount)
        {
            _rotation += amount;
            _transformUpdateRequired = true;
        }
        public void RotateTo(float rot)
        {
            _rotation = rot;
            _transformUpdateRequired = true;
        }

        //public bool CollidesWithCircle(ReBody b, out ReVector normal, out float depth)
        //{
        //    normal = ReVector.Zero;
        //    depth = 0f;

        //    if (ShapeType == ShapeType.Circle)
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

        public static bool CreateCircleBody(ICollidable owner, ReVector posOffset, float radius, float density, bool isStatic, float restitution, out ReBody body, out string errorMessage)
        {
            body = null;
            errorMessage = string.Empty;

            float area = MathF.PI * radius * radius;

            if (area < ReWorld.MinBodySize || area > ReWorld.MaxBodySize)
            {
                errorMessage = "Circle area is either too large or small";
                return false;
            }

            if (density < ReWorld.MinDensity || density > ReWorld.MaxDensity)
            {
                errorMessage = "Circle density is either too high or low";
                return false;
            }

            float mass = area * density * 1000; // or 0.001? Don't think so

            restitution = ReMath.Clamp(restitution, 0, 1);

            body = new ReBody(owner, posOffset, 0, mass, density, restitution, area, isStatic, radius, 0, 0, ShapeType.Circle);

            return true;
        }

        public static bool CreateAABBBody(ICollidable owner, ReVector posOffset, float width, float height, float density, bool isStatic, float restitution, out ReBody body, out string errorMessage)
        {
            body = null;
            errorMessage = string.Empty;

            float area = width * height;

            if (area < ReWorld.MinBodySize || area > ReWorld.MaxBodySize)
            {
                errorMessage = "Rectangle area is either too large or small";
                return false;
            }

            if (density < ReWorld.MinDensity || density > ReWorld.MaxDensity)
            {
                errorMessage = "Rectangle density is either too high or low";
                return false;
            }

            float mass = area * density * 1000; // or 0.001? Don't think so

            restitution = ReMath.Clamp(restitution, 0, 1);

            body = new ReBody(owner, posOffset, 0, mass, density, restitution, area, isStatic, 0, width, height, ShapeType.AABB);

            return true;
        }

        public static bool CreateOBBBody(ICollidable owner, ReVector posOffset, float width, float height, float density, bool isStatic, float restitution, out ReBody body, out string errorMessage)
        {
            body = null;
            errorMessage = string.Empty;

            float area = width * height;

            if (area < ReWorld.MinBodySize || area > ReWorld.MaxBodySize)
            {
                errorMessage = "Rectangle area is either too large or small";
                return false;
            }

            if (density < ReWorld.MinDensity || density > ReWorld.MaxDensity)
            {
                errorMessage = "Rectangle density is either too high or low";
                return false;
            }

            float mass = area * density * 1000; // or 0.001? Don't think so

            restitution = ReMath.Clamp(restitution, 0, 1);

            body = new ReBody(owner, posOffset, 0, mass, density, restitution, area, isStatic, 0, width, height, ShapeType.OBB);

            return true;
        }
    }
}

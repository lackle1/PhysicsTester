using System;

namespace RePhysics
{
    public struct ReVector
    {
        public float X;
        public float Y;

        public ReVector(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static ReVector Zero
        {
            get { return new ReVector(0, 0); }
        }

        public static ReVector operator +(ReVector a, ReVector b)
        {
            return new ReVector(a.X + b.X, a.Y + b.Y);
        }
        public static ReVector operator -(ReVector a, ReVector b)
        {
            return new ReVector(a.X - b.X, a.Y - b.Y);
        }
        public static ReVector operator *(ReVector v, float s)
        {
            return new ReVector(v.X * s, v.Y * s);
        }
        public static ReVector operator /(ReVector v, float s)
        {
            return new ReVector(v.X / s, v.Y / s);
        }

        public static ReVector operator -(ReVector v)
        {
            return new ReVector(-v.X, -v.Y);
        }

        //public static bool operator ==(Vector2 a, Vector2 b)
        //{
        //    return a.X == b.X && a.Y == b.Y;
        //}
        //public static bool operator !=(Vector2 a, Vector2 b)
        //{
        //    return a.X != b.X || a.Y != b.Y;
        //}

        public ReVector Transform(ReTransform transform)
        {
            return new ReVector(
                transform.Cos * X - transform.Sin * Y + transform.PositionX,
                transform.Sin * X + transform.Cos * Y + transform.PositionY);
        }

        public float Length()
        {
            return MathF.Sqrt(X * X + Y * Y);
        }
        public float LengthSqrd()
        {
            return X * X + Y * Y;
        }
        public void Normalise()
        {
            float len = Length();
            if (len != 0.0f)
            {
                X /= len;
                Y /= len;
                //this = new PVector(X / len, Y / len);
            }
        }
        public ReVector Normalised()
        {
            float len = Length();
            if (len == 0.0f)
            {
                return ReVector.Zero;
            }

            return new ReVector(X /  len, Y / len);
        }

        public override bool Equals(object obj)
        {
            if (obj is ReVector other)
            {
                return Equals(other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return new {this.X, this.Y}.GetHashCode();
        }

        public override string ToString()
        {
            return $"X: {X}, Y: {Y}";
        }
    }
}

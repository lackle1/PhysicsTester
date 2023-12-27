using System;

namespace RePhysics
{
    public static class ReMath
    {
        public static float Clamp(float value, float min, float max)
        {
            if (value < min) { return min; }
            if (value > max) { return max; } 
            else { return value; }
        }

        public static float Length(ReVector v)
        {
            return MathF.Sqrt(v.X * v.X + v.Y * v.Y);
        }
        public static float LengthSqrd(ReVector v)
        {
            return v.X * v.X + v.Y * v.Y;
        }
        public static float Distance(ReVector a, ReVector b) // or return Length(a - b);
        {
            float dx = b.X - a.X;
            float dy = b.Y - a.Y;

            return MathF.Sqrt(dx * dx + dy * dy);
        }
        public static float DistanceSqrd(ReVector a, ReVector b)
        {
            float dx = b.X - a.X;
            float dy = b.Y - a.Y;

            return dx * dx + dy * dy;
        }
        public static ReVector Normalise(ReVector v)
        {
            float len = Length(v);
            if (len == 0)
            {
                return ReVector.Zero;
            }

            return new ReVector(v.X / len, v.Y / len);
        }
        public static float Dot(ReVector a, ReVector b)
        {
            return a.X * b.X + a.Y * b.Y;
        }
        public static float Cross(ReVector a, ReVector b)
        {
            return a.X * b.Y - a.Y * b.X;
        }
    }
}

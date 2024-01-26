using SharpDX;
using System;
using System.Reflection.Metadata.Ecma335;

namespace RePhysics
{
    public static class ReMath
    {
        public static float Clamp(float value, float min, float max)
        {
            if (min > max)
            {
                throw new ArgumentException("'min' cannot be more than 'max'.");
            }

            if (value < min) { return min; }
            if (value > max) { return max; } 
            else { return value; }
        }
        public static float ClosestTo(float value, float a, float b)
        {
            //if (value < lower)
            //{
            //    return lower;
            //}
            //else if (value > upper)
            //{
            //    return upper;
            //}
            //else
            //{
            //    return value >= (upper + lower) / 2 ? upper : lower;
            //}

            if (MathF.Abs(a - value) < MathF.Abs(b - value))
            {
                return a;
            }

            return b;
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

        public static bool AboutEqual(float a, float b, float errorMargin)
        {
            return MathF.Abs(b - a) < errorMargin;
        }
        public static bool AboutEqual(ReVector a, ReVector b, float errorMargin)
        {
            //return AboutEqual(a.X, b.X, errorMargin) && AboutEqual(a.Y, b.Y, errorMargin);
            return ReMath.DistanceSqrd(a, b) < errorMargin * errorMargin;
        }

        public static ReVector ClampOld(ReVector value, ReVector a, ReVector b) // Clamp 'value' between 'a' and 'b'
        {
            ReVector result = value;
            float minX = MathF.Min(a.X, b.X);
            float maxX = MathF.Max(a.X, b.X);
            float minY = MathF.Min(a.Y, b.Y);
            float maxY = MathF.Max(a.Y, b.Y);

            result.X = MathF.Max(result.X, minX);
            result.X = MathF.Min(result.X, maxX);
            result.Y = MathF.Max(result.Y, minY);
            result.Y = MathF.Min(result.Y, maxY);

            return result;
        }
        public static ReVector Clamp(ReVector value, ReVector a, ReVector b) // Clamp 'value' between 'a' and 'b'
        {
            float clampedX = ClampBetween(value.X, a.X, b.X);
            float clampedY = ClampBetween(value.Y, a.Y, b.Y);

            return new ReVector(clampedX, clampedY);
        }
        public static float ClampBetween(float value, float a, float b) // If min/max is unknown
        {
            float min = Math.Min(a, b);
            float max = Math.Max(a, b);

            return Clamp(value, min, max);
        }

        public static float ClosestMultipleOf(float value, float factor)
        {
            float quotient = value / factor;

            float result = MathF.Round(quotient) * factor;

            return MathF.Round(quotient) * factor;
        }
    }
}

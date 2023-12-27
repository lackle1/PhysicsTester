using System;

namespace RePhysics
{
    public readonly struct ReTransform
    {
        public readonly float PositionX;
        public readonly float PositionY;
        public readonly float Sin;
        public readonly float Cos;

        public readonly static ReTransform Default = new ReTransform(0, 0, 0);

        public ReTransform(ReVector position, float angle)
        {
            PositionX = position.X;
            PositionY = position.Y;
            Sin = MathF.Sin(angle);
            Cos = MathF.Cos(angle);
        }

        public ReTransform(float positionX, float positionY, float angle)
        {
            PositionX = positionX;
            PositionY = positionY;
            Sin = MathF.Sin(angle);
            Cos = MathF.Cos(angle);
        }
    }
}

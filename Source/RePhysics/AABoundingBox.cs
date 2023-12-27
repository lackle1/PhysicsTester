using System;

using Grondslag;
using Microsoft.Xna.Framework;

namespace RePhysics
{
    public struct AABoundingBox
    {
        public AABoundingBox(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public int Left { get { return X; } set { X = value; } }
        public int Right { get { return X + Width; } set { X = value - Width; } }
        public int Top { get { return Y; } set { Y = value; } }
        public int Bottom { get { return Y + Height; } set { Y = value - Height; } }

        public bool Intersects(AABoundingBox box)
        {
            return Left < box.Right && Right > box.Left && Top < box.Bottom && Bottom > box.Top;
        }
    }
}

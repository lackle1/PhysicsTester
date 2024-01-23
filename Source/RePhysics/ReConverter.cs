using System;

using Microsoft.Xna.Framework;

namespace RePhysics
{
    public static class ReConverter
    {
        public static Microsoft.Xna.Framework.Vector2 ToMGVector2(ReVector v)
        {
            return new Microsoft.Xna.Framework.Vector2(v.X, v.Y);
        }
        public static ReVector ReVector(Vector2 v)
        {
            return new ReVector(v.X, v.Y);
        }
    }
}

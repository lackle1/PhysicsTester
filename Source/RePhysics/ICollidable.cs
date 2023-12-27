using Microsoft.Xna.Framework;
using System;

namespace RePhysics
{
    public interface ICollidable
    {
        public Vector2 Pos
        {
            get;
            set;
        }
        public ReBody CollisionBody { get; set; }
    }
}

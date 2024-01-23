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
        public float Angle
        {
            get;
            set;
        }
        public ReBoxBody CollisionBody { get; set; }

        public bool Remove {  get; set; }
    }
}

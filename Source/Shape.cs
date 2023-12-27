using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

using Microsoft.Xna.Framework.Graphics;
using RePhysics;

namespace Grondslag
{
    public class Shape : ICollidable
    {
        public ReBody CollisionBody { get; set; }
        public Vector2 Pos
        {
            get;
            set;
            //{
            //    _pos = value;
            //    CollisionBody.pos = PConverter.ToPVector(value);
            //}
        }

        private Texture2D _texture;
        //private Vector2 _pos;
        private Vector2 _dims; 
        private float _rotation;
        public Color colour;
        public ShapeType shapeType;

        private ReBody _body;



        public Shape(Texture2D texture, Vector2 pos, Vector2 dims, float rotation, Color colour, ShapeType shapeType, out ReBody body)
        {
            _texture = texture;
            Pos = pos;
            _dims = dims;
            _rotation = rotation;
            this.colour = colour;
            this.shapeType = shapeType;


            body = null;

            ReVector posOffset = ReVector.Zero;
            if (this.shapeType == ShapeType.AABB && ReBody.CreateAABBBody(this, posOffset, _dims.X, _dims.Y, 1, false, 0.5f, out body, out string errorMessage))
            {
                CollisionBody = body;
            }
            else if (this.shapeType == ShapeType.OBB && ReBody.CreateOBBBody(this, posOffset, _dims.X, _dims.Y, 1, false, 0.5f, out body, out errorMessage))
            {
                CollisionBody = body;
            }
            else if (this.shapeType == ShapeType.Circle && ReBody.CreateCircleBody(this, posOffset, dims.X / 2, 1, false, 0.5f, out body, out errorMessage))
            {
                CollisionBody = body;
            }
        }

        public void Move(ReVector amount)
        {
            CollisionBody.Move(amount);
        }
        public void Rotate(float amount)
        {
            _rotation += amount;
            CollisionBody.Rotate(amount);
        }

        public virtual void Draw()
        {
            Globals.spriteBatch.Draw(_texture, new Rectangle((int)Pos.X, (int)Pos.Y, (int)_dims.X, (int)_dims.Y), null, colour, _rotation, new Vector2(_texture.Width*0.5f, _texture.Height*0.5f), SpriteEffects.None, 0);
            //Globals.spriteBatch.Draw(_texture, _pos, null, _colour, _rotation, new Vector2(_texture.Width * 0.5f, _texture.Height * 0.5f), 4, SpriteEffects.None, 0);

            //CollisionBody.DrawVertices(_texture, Color.Black);

            if (shapeType == ShapeType.OBB)
            {
                ReVector[] vertices = CollisionBody.GetTransformedVertices();
                foreach (var v in vertices)
                {
                    Globals.spriteBatch.Draw(_texture, ReConverter.ToMGVector2(v), null, Color.Black, 0f, new Vector2(_texture.Width * 0.5f, _texture.Height * 0.5f), 8, SpriteEffects.None, 0);
                }
            }
        }
    }
}

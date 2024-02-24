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
        public ReBoxBody CollisionBody { get; set; }
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
        public bool Remove { get; set; }
        public int Layer { get; set; }

        private Texture2D _texture;
        //private Vector2 _pos;
        private Vector2 _dims; 
        public Color colour;
        public ShapeType shapeType;


        public Shape(Texture2D texture, Vector2 pos, Vector2 dims, float angle, bool isStatic, float restitution, Color colour, ShapeType shapeType, int layer, out ReBoxBody body)
        {
            _texture = texture;
            Pos = pos;
            _dims = dims;
            Angle = angle;
            this.colour = colour;
            this.shapeType = shapeType;


            body = null;
            string errorMessage = "Unknown shape type.";

            ReVector posOffset = ReVector.Zero;
            if (this.shapeType == ShapeType.AABB && ReBoxBody.CreateAABBBody(this, posOffset, _dims.X, _dims.Y, isStatic, out ReBoxBody boxBody))
            {
                body = boxBody;
                CollisionBody = body;
            }
            else if (this.shapeType == ShapeType.OBB && ReBoxBody.CreateOBBBody(this, posOffset, _dims.X, _dims.Y, angle, 1000, isStatic, 0.5f, layer, out RePhysicsBody physicsBody, out errorMessage))
            {
                CollisionBody = physicsBody;
                body = physicsBody;
            }
            else if (this.shapeType == ShapeType.Circle && ReBoxBody.CreateCircleBody(this, posOffset, dims.X / 2, angle, 1000, isStatic, 0.5f, out physicsBody, out errorMessage))
            {
                _dims = new Vector2(dims.X, dims.X);
                CollisionBody = physicsBody;
                body = physicsBody;
            }
            else
            {
                throw new Exception(errorMessage);
            }
        }

        public void Move(ReVector amount)
        {
            CollisionBody.Move(amount);
        }
        public void Rotate(float amount)
        {
            if (CollisionBody is RePhysicsBody body)
            {
                Angle += amount;
                body.Rotate(amount);
            }
        }

        public virtual void Draw(SpriteFont font)
        {
            //(int)MathF.Round(Pos.X + 0.01f), (int)MathF.Round(Pos.Y + 0.01f) // Does not really work, might just have to only use even numbers
            Globals.spriteBatch.Draw(_texture, new Rectangle((int)Pos.X, (int)Pos.Y, (int)_dims.X, (int)_dims.Y), null, colour, Angle, new Vector2(_texture.Width*0.5f, _texture.Height*0.5f), SpriteEffects.None, 0);

            if (CollisionBody.IsOBB)
            {
                ReVector[] vertices = CollisionBody.GetTransformedVertices();
                foreach (var v in vertices)
                {
                    //Globals.spriteBatch.Draw(_texture, ReConverter.ToMGVector2(v), null, Color.Black, 0f, new Vector2(_texture.Width * 0.5f, _texture.Height * 0.5f), 8, SpriteEffects.None, 0);
                }
            }

            ReRect rect = CollisionBody.GetAABB();
            ReVector[] edges = new ReVector[4]
            {
                new ReVector(rect.Left, rect.Top),
                new ReVector(rect.Right, rect.Top),
                new ReVector(rect.Right, rect.Bottom),
                new ReVector(rect.Left, rect.Bottom),
            };
            foreach (var v in edges)
            {
                //Globals.spriteBatch.Draw(_texture, ReConverter.ToMGVector2(v), null, Color.Black, 0f, new Vector2(_texture.Width * 0.5f, _texture.Height * 0.5f), 8, SpriteEffects.None, 0);
            }

            //Globals.spriteBatch.DrawString(font, CollisionBody.PhysicsVer.LinearVelocity.ToString(), ReConverter.ToMGVector2(edges[0]), Color.Black);
            //Globals.spriteBatch.DrawString(font, CollisionBody.PhysicsVer.AngularVelocity.ToString(), ReConverter.ToMGVector2(edges[3]), Color.Black);
        }
    }
}

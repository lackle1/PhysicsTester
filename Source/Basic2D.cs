using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Grondslag
{
    [Serializable]
    //[JsonObject(MemberSerialization.Fields)]

    public class Basic2D
    {
        public float rot;

        public SpriteEffects effect = SpriteEffects.None;

        public Vector2 pos, dims;

        public Color colour;

        [JsonIgnore]
        protected Texture2D _texture;
        [JsonIgnore]
        private Dictionary<string, Texture2D> _availableSprites = new();

        public Basic2D(Texture2D texture, Vector2 pos, Vector2 dims)
        {
            this.pos = pos;
            this.dims = dims;

            colour = Color.White;

            rot = 0.0f;

            AddSprite("Default", texture);
            _texture = _availableSprites["Default"];
        }

        public Basic2D(string file, Vector2 pos, Vector2 dims)
        {
            this.pos = pos;
            this.dims = dims;

            colour = Color.White;

            rot = 0.0f;

            AddSprite("Default", LoadTexture(file));
            _texture = _availableSprites["Default"];

            // Why is _texture null here?
        }

        public static Texture2D LoadTexture(string file)
        {
            return Globals.content.Load<Texture2D>(file);
        }

        public virtual void Update()
        {

        }

        public virtual void AddSprite(string name, Texture2D texture) // Change out these for a scroll bar sprite sheet
        {
            _availableSprites.Add(name, texture);
        }
        public virtual void ChangeSpriteByName(string name)
        {
            _texture = _availableSprites[name];
        }

        public virtual bool Hover()
        {
            Vector2 mousePos = new Vector2(Globals.mouse.newMousePos.X, Globals.mouse.newMousePos.Y);

            if (mousePos.X >= pos.X - dims.X / 2 && mousePos.X <= pos.X + dims.X / 2 && mousePos.Y >= pos.Y - dims.Y / 2 && mousePos.Y <= pos.Y + dims.Y / 2)
            {
                return true;
            }

            return false;
        }

        public virtual bool Hover(Vector2 offset)
        {
            Vector2 mousePos = new Vector2(Globals.mouse.newMousePos.X, Globals.mouse.newMousePos.Y);

            if (mousePos.X >= pos.X + offset.X - dims.X / 2 && mousePos.X <= pos.X + offset.X + dims.X / 2 && mousePos.Y >= pos.Y + offset.Y - dims.Y / 2 && mousePos.Y <= pos.Y + offset.Y + dims.Y / 2)
            {
                return true;
            }

            return false;
        }

        public virtual void Draw()
        {
            if (_texture != null)
            {
                Globals.spriteBatch.Draw(_texture, new Rectangle((int)pos.X, (int)pos.Y, (int)dims.X, (int)dims.Y), null, colour, rot, new Vector2(_texture.Bounds.Width / 2, _texture.Bounds.Height / 2), effect, 0);
            }
        }

        public virtual void Draw(Vector2 offset)
        {
            if (_texture != null)
            {
                //if sprite.Bounds is an even number, the image will draw in the wrong position
                Globals.spriteBatch.Draw(_texture, new Rectangle((int)(pos.X + offset.X), (int)(pos.Y + offset.Y), (int)dims.X, (int)dims.Y), null, colour, rot, new Vector2(_texture.Bounds.Width / 2, _texture.Bounds.Height / 2), effect, 0);
            }
        }

        public virtual void Draw(Vector2 dimsmultiplier, Vector2 offset, object info)
        {
            if (_texture != null)
            {
                Globals.spriteBatch.Draw(_texture, new Rectangle((int)(pos.X + offset.X), (int)(pos.Y + offset.Y), (int)(dims.X * dimsmultiplier.X), (int)(dims.Y * dimsmultiplier.Y)), null, colour, rot, new Vector2(_texture.Bounds.Width / 2, _texture.Bounds.Height / 2), effect, 0);
            }
        }

        public virtual void Draw(Color colour, Vector2 offset)
        {
            if (_texture != null)
            {
                Globals.spriteBatch.Draw(_texture, new Rectangle((int)(pos.X + offset.X), (int)(pos.Y + offset.Y), (int)dims.X, (int)dims.Y), null, colour, rot, new Vector2(_texture.Bounds.Width / 2, _texture.Bounds.Height / 2), effect, 0);
            }
        }

        //public virtual void Draw(ParticleDataPacket packet, Vector2 offset)
        //{
        //    if (_texture != null)
        //    {
        //        Globals.spriteBatch.Draw(_texture, new Rectangle((int)(pos.X + packet.relativePos.X + offset.X), (int)(pos.Y + packet.relativePos.Y + offset.Y), (int)(dims.X * packet.dimsMultiplier), (int)(dims.Y * packet.dimsMultiplier)), null, packet.colour, packet.rot, new Vector2(_texture.Bounds.Width / 2, _texture.Bounds.Height / 2), effect, 0);
        //    }
        //}

        public virtual void Draw(Vector2 offset, Vector2 origin, Color colour)
        {
            if (_texture != null)
            {
                Globals.spriteBatch.Draw(_texture, new Rectangle((int)Math.Round(pos.X + offset.X), (int)Math.Round(pos.Y + offset.Y), (int)dims.X, (int)dims.Y), null, colour, rot, new Vector2(origin.X, origin.Y), effect, 0);
            }
        }

        public virtual void Draw(Vector2 offset, Vector2 origin)
        {
            if (_texture != null)
            {
                Globals.spriteBatch.Draw(_texture, new Rectangle((int)Math.Round(pos.X + offset.X), (int)Math.Round(pos.Y + offset.Y), (int)dims.X, (int)dims.Y), null, colour, rot, new Vector2(origin.X, origin.Y), effect, 0);
            }
        }
    }
}

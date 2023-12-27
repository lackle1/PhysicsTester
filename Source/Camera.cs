using System;
using Microsoft.Xna.Framework;

namespace Grondslag
{
    public class Camera
    {
        private float _zoom;
        private float _rotation;
        private Matrix _transform;
        private Vector2 _pos;

        public Camera(Vector2 pos, float zoom)
        {
            _zoom = zoom;
            _rotation = 0.0f;
            _pos = pos;
        }

        public float Zoom
        {
            get { return _zoom; }
            set { _zoom = Math.Abs(value); }
        }

        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        public Vector2 Pos
        {
            get { return _pos; }
            set { _pos = value; }
        }

        public void Update()
        {
            if (Globals.keyboard.GetPress("Up"))
            {
                MoveCamera(new Vector2(0, -10 / _zoom));
            }
            if (Globals.keyboard.GetPress("Left"))
            {
                MoveCamera(new Vector2(-10 / _zoom, 0));
            }
            if (Globals.keyboard.GetPress("Down"))
            {
                MoveCamera(new Vector2(0, 10 / _zoom));
            }
            if (Globals.keyboard.GetPress("Right"))
            {
                MoveCamera(new Vector2(10 / _zoom, 0));
            }

            if (Globals.keyboard.GetPress("D1"))
            {
                _zoom *= 0.95f;
            }
            if (Globals.keyboard.GetPress("D2"))
            {
                _zoom *= 1.05f;
            }
        }

        public void MoveCamera(Vector2 amount)
        {
            _pos += amount;
        }

        public Rectangle GetExtents() // Doesn't account for rotation
        {
            float zoomedX = Globals.screenWidth / _zoom;
            float zoomedY = Globals.screenHeight / _zoom;

            return new Rectangle((int)(_pos.X - zoomedX / 2), (int)(_pos.Y - zoomedY / 2), (int)zoomedX, (int)zoomedY);
        }

        public Matrix GetMatrix()
        {
            _transform = Matrix.CreateTranslation(new Vector3(-_pos.X, -_pos.Y, 0)) * 
                Matrix.CreateRotationZ(_rotation) * 
                Matrix.CreateScale(new Vector3(_zoom, _zoom, 1.0f)) * 
                Matrix.CreateTranslation(new Vector3(Globals.screenWidth / 2, Globals.screenHeight / 2, 0));
            return _transform;
        }
    }
}

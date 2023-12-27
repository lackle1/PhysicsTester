using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Grondslag
{
    public class LsMouseControl
    {
        private MouseState _newMouse, _oldMouse, _firstMouse;

        private bool _dragging, _rightDrag;

        public Vector2 newMousePos, oldMousePos, firstMousePos, newMouseAdjustedPos, systemCursorPos, screenLoc;

        public LsMouseControl()
        {
            _dragging = false;

            _newMouse = Mouse.GetState();
            _oldMouse = _newMouse;
            _firstMouse = _newMouse;

            newMousePos = new Vector2(_newMouse.Position.X, _newMouse.Position.Y);
            oldMousePos = new Vector2(_newMouse.Position.X, _newMouse.Position.Y);
            firstMousePos = new Vector2(_newMouse.Position.X, _newMouse.Position.Y);

            GetMouseAndAdjust();

            //screenLoc = new Vector2((int)(systemCursorPos.X/Globals.screenWidth), (int)(systemCursorPos.Y/Globals.screenHeight));

        }

        public MouseState First
        {
            get { return _firstMouse; }
        }

        public MouseState New
        {
            get { return _newMouse; }
        }

        public MouseState Old
        {
            get { return _oldMouse; }
        }

        public void Update()
        {
            GetMouseAndAdjust();


            if (_newMouse.LeftButton == ButtonState.Pressed && _oldMouse.LeftButton == ButtonState.Released)
            {
                _firstMouse = _newMouse;
                firstMousePos = newMousePos = GetScreenPos(_firstMouse);
            }


        }

        public void UpdateOld()
        {
            _oldMouse = _newMouse;
            oldMousePos = GetScreenPos(_oldMouse);
        }

        public virtual float GetDistanceFromClick()
        {
            return Globals.GetDistance(newMousePos, firstMousePos);
        }

        public virtual void GetMouseAndAdjust()
        {
            _newMouse = Mouse.GetState();
            newMousePos = GetScreenPos(_newMouse);

        }




        public int GetMouseWheelChange()
        {
            return _newMouse.ScrollWheelValue - _oldMouse.ScrollWheelValue;
        }


        public Vector2 GetScreenPos(MouseState mouse)
        {
            Vector2 tempVec = new Vector2(mouse.Position.X, mouse.Position.Y);


            return tempVec;
        }

        public virtual bool LeftClick()
        {
            if (_newMouse.LeftButton == ButtonState.Pressed && _oldMouse.LeftButton != ButtonState.Pressed && _newMouse.Position.X >= 0 && _newMouse.Position.X <= Globals.screenWidth && _newMouse.Position.Y >= 0 && _newMouse.Position.Y <= Globals.screenHeight)
            {
                return true;
            }

            return false;
        }

        public virtual bool LeftClickDown()
        {
            if (_newMouse.LeftButton == ButtonState.Pressed && _newMouse.Position.X >= 0 && _newMouse.Position.X <= Globals.screenWidth && _newMouse.Position.Y >= 0 && _newMouse.Position.Y <= Globals.screenHeight)
            {
                return true;
            }

            return false;
        }

        public virtual bool LeftClickHold()
        {
            bool holding = false;

            if (_newMouse.LeftButton == ButtonState.Pressed && _oldMouse.LeftButton == ButtonState.Pressed && _newMouse.Position.X >= 0 && _newMouse.Position.X <= Globals.screenWidth && _newMouse.Position.Y >= 0 && _newMouse.Position.Y <= Globals.screenHeight)
            {
                holding = true;

                if (Math.Abs(_newMouse.Position.X - _firstMouse.Position.X) > 8 || Math.Abs(_newMouse.Position.Y - _firstMouse.Position.Y) > 8)
                {
                    _dragging = true;
                }
            }



            return holding;
        }

        public virtual bool LeftClickRelease()
        {
            if (_newMouse.LeftButton == ButtonState.Released && _oldMouse.LeftButton == ButtonState.Pressed)
            {
                _dragging = false;
                return true;
            }

            return false;
        }

        public virtual bool RightClick()
        {
            if (_newMouse.RightButton == ButtonState.Pressed && _oldMouse.RightButton != ButtonState.Pressed && _newMouse.Position.X >= 0 && _newMouse.Position.X <= Globals.screenWidth && _newMouse.Position.Y >= 0 && _newMouse.Position.Y <= Globals.screenHeight)
            {
                return true;
            }

            return false;
        }

        public virtual bool RightClickDown()
        {
            if (_newMouse.RightButton == ButtonState.Pressed && _newMouse.Position.X >= 0 && _newMouse.Position.X <= Globals.screenWidth && _newMouse.Position.Y >= 0 && _newMouse.Position.Y <= Globals.screenHeight)
            {
                return true;
            }

            return false;
        }

        public virtual bool RightClickHold()
        {
            bool holding = false;

            if (_newMouse.RightButton == ButtonState.Pressed && _oldMouse.RightButton == ButtonState.Pressed && _newMouse.Position.X >= 0 && _newMouse.Position.X <= Globals.screenWidth && _newMouse.Position.Y >= 0 && _newMouse.Position.Y <= Globals.screenHeight)
            {
                holding = true;

                if (Math.Abs(_newMouse.Position.X - _firstMouse.Position.X) > 8 || Math.Abs(_newMouse.Position.Y - _firstMouse.Position.Y) > 8)
                {
                    _rightDrag = true;
                }
            }



            return holding;
        }

        public virtual bool RightClickRelease()
        {
            if (_newMouse.RightButton == ButtonState.Released && _oldMouse.RightButton == ButtonState.Pressed)
            {
                _dragging = false;
                return true;
            }

            return false;
        }

        public void SetFirst()
        {

        }
    }
}

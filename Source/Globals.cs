using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Grondslag
{
    public delegate void PassObject(object i);
    public delegate bool StringToBool(string i);
    public delegate bool ReturnBool();

    public class Globals
    {
        public static int screenWidth, screenHeight;

        public const int scaleFactor = 3;

        public static System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("en-GB");

        public static Random rand = new Random();

        public static ContentManager content;
        public static SpriteBatch spriteBatch;

        //public static OptionsMenu optionsMenu;

        public static Effect shader;

        public static LsKeyboard keyboard;
        public static LsMouseControl mouse;

        public static GameTime gameTime;
        public static float deltaTime; // in seconds

        public static bool hideCursor;

        public static Vector2 MouseWorldPosition;

        public static float GetDistance(Vector2 pos, Vector2 target)
        {
            return (float)Math.Sqrt(Math.Pow(pos.X - target.X, 2) + Math.Pow(pos.Y - target.Y, 2));
        }

        public static bool AboutEqualVectors(Vector2 first, Vector2 second, float amount)
        {
            if (first.X > second.X - amount && first.X < second.X + amount && first.Y > second.Y - amount && first.Y < second.Y + amount)
            {
                return true;
            }

            return false;
        }

        public static float ZeroOrCLosestIfBetween(float value, float lower, float upper)
        {
            if (value == 0)
            {
                return value;
            }
            else if (value > lower && value < upper)
            {
                return value >= (upper + lower) / 2 ? upper : lower;
            }

            return value;
        }

        public static Vector2 RadialMovement(Vector2 pos, Vector2 focus, float speed)
        {
            //returns distance
            float dist = GetDistance(pos, focus);

            if (dist <= speed)
            {
                //gets the remaining distance
                return focus - pos;
            }
            else
            {
                //gets the remaing distance, times it by the speed, then divides it by the distance to normalise it
                return (focus - pos) * speed / dist;
            }
        }

        public static float RotateTowards(Vector2 pos, Vector2 focus)
        {
            if (pos != focus)
            {
                Vector2 triangle = focus - pos;

                float angle = MathF.Atan2(triangle.Y, triangle.X);

                return angle;
            }
            else
            {
                return 0;
            }
        }
    }
}

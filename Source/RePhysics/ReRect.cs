using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RePhysics
{
    public struct ReRect
    {
        public float Left;
        public float Right;
        public float Top;
        public float Bottom;

        public ReRect(float left, float right, float top, float bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }
    }
}

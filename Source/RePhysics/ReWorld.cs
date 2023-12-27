using System;
using System.Security.Cryptography.X509Certificates;
using Grondslag;
using Microsoft.Xna.Framework;

namespace RePhysics
{
    public class ReWorld
    {
        public static readonly float MinBodySize = 0.01f * 0.01f; // m^2
        public static readonly float MaxBodySize = 64f * 64f;

        public static readonly float MinDensity = 0.2f; // g cm^-3
        public static readonly float MaxDensity = 20.0f;

        public ReWorld()
        {
            
        }
    }
}

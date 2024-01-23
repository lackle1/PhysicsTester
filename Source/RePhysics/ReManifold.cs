using RePhysics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RePhysics
{
    public readonly struct ReManifold
    {
        public readonly ReBoxBody BodyA;
        public readonly ReBoxBody BodyB;
        public readonly ReVector Normal;
        public readonly float Depth;

        public readonly ReVector Contact1;
        public readonly ReVector Contact2;
        public readonly int ContactCount;

        public ReManifold(
            ReBoxBody bodyA, ReBoxBody bodyB,
            ReVector normal, float depth,
            ReVector contact1, ReVector contact2, int contactCount)
        {
            BodyA = bodyA;
            BodyB = bodyB;
            Normal = normal;
            Depth = depth;

            Contact1 = contact1;
            Contact2 = contact2;
            ContactCount = contactCount;
        }
    }
}

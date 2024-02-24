using Grondslag;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RePhysics
{
    public class ReJoint
    {
        private readonly RePhysicsBody _bodyA, _bodyB;

        private readonly ReVector _posA, _posB; // position relative to the body
        private ReVector _tPosA, _tPosB;
        private readonly float _minAngle, _maxAngle;

        private Texture2D _debugTexture;


        public ReJoint(RePhysicsBody bodyA, RePhysicsBody bodyB, Texture2D debugTexture)
        {
            _bodyA = bodyA;
            _bodyB = bodyB;

            _posA = new ReVector(_bodyA.Width / 2, -_bodyA.Height / 2);
            _posB = new ReVector(-_bodyB.Width / 2, _bodyB.Height / 2);
            _tPosA = _tPosB =  ReVector.Zero;
            _minAngle = _maxAngle = 0;
            
            _debugTexture = debugTexture;
        }

        public void SolveConstraints()
        {
            _tPosA = _bodyA.GetTransformedPoint(_posA);
            _tPosB = _bodyB.GetTransformedPoint(_posB);
            
            ReVector posDiff = (_tPosB - _tPosA) / 2;
            _bodyA.Move(posDiff);
            _bodyB.Move(-posDiff);

            float angleDiff = (_bodyB.Angle - _bodyA.Angle) / 2;
            _bodyA.Rotate(angleDiff*2);
            //_bodyB.Rotate(-angleDiff);

            ReVector avgLinearVel = (_bodyA.LinearVelocity + _bodyB.LinearVelocity) / 2;
            _bodyA.LinearVelocity = avgLinearVel;
            _bodyB.LinearVelocity = avgLinearVel;

            //float avgAngularVel = (_bodyA.AngularVelocity + _bodyB.AngularVelocity) / 2;
            //_bodyA.AngularVelocity = avgAngularVel;
            //_bodyB.AngularVelocity = avgAngularVel;
        }

        public void Draw()
        {
            Globals.spriteBatch.Draw(_debugTexture, ReConverter.ToMGVector2(_tPosA), null, Color.Black, 0f, new Vector2(_debugTexture.Width * 0.5f, _debugTexture.Height * 0.5f), 8, SpriteEffects.None, 0);
            Globals.spriteBatch.Draw(_debugTexture, ReConverter.ToMGVector2(_tPosB), null, Color.Black, 0f, new Vector2(_debugTexture.Width * 0.5f, _debugTexture.Height * 0.5f), 8, SpriteEffects.None, 0);
        }
    }
}

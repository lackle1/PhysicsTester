using System;
using Grondslag;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RePhysics;
using System.Collections.Generic;
using System.Diagnostics;

namespace PhysicsTester
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Camera _camera;

        private List<Shape> _shapes = new();
        private Texture2D _rectangleTexture;
        private Texture2D _circleTexture;

        private List<ReBody> _bodies = new();

        private Random _rand = new Random();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            this.IsFixedTimeStep = true;
            this.TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0 / 144.0f); // 60 FPS
            //_graphics.SynchronizeWithVerticalRetrace = false;
        }

        protected override void Initialize()
        {
            Globals.screenWidth = 1600;
            Globals.screenHeight = 900;

            _graphics.PreferredBackBufferWidth = Globals.screenWidth;
            _graphics.PreferredBackBufferHeight = Globals.screenHeight;

            _graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            Globals.spriteBatch = new SpriteBatch(GraphicsDevice);
            Globals.content = this.Content;

            _camera = new Camera(new Vector2(Globals.screenWidth * 0.5f, Globals.screenHeight*0.5f), 1f);

            Globals.keyboard = new LsKeyboard();
            Globals.mouse = new LsMouseControl();

            _rectangleTexture = Basic2D.LoadTexture("square");
            _circleTexture = Basic2D.LoadTexture("circle512");

            Rectangle cameraExtents = _camera.GetExtents();

            Texture2D texture = _rectangleTexture;
            Vector2 size = new Vector2(64);
            ShapeType type = ShapeType.OBB;
            ReBody body;

            //_shapes.Add(new Shape(texture, new Vector2(cameraExtents.Left, cameraExtents.Top), size, 0, Color.White, type, out ReBody body));
            //_bodies.Add(body);
            //_shapes.Add(new Shape(texture, new Vector2(cameraExtents.Right, cameraExtents.Top), size, 0, Color.Black, type, out body));
            //_bodies.Add(body);
            //_shapes.Add(new Shape(texture, new Vector2(cameraExtents.Left, cameraExtents.Bottom), size, 0, Color.Blue, type, out body));
            //_bodies.Add(body);
            //_shapes.Add(new Shape(texture, new Vector2(cameraExtents.Right, cameraExtents.Bottom), size, 0, Color.Red, type, out body));
            //_bodies.Add(body);

            for (int i = 0; i < 10; i++)
            {
                int randInt = _rand.Next(0, 2);
                if (randInt == 0)
                {
                    texture = _rectangleTexture;
                    type = ShapeType.OBB;
                }
                else
                {
                    texture = _circleTexture;
                    type = ShapeType.Circle;
                }

                Vector2 randPos = new Vector2(_rand.Next(cameraExtents.Left, cameraExtents.Right), _rand.Next(cameraExtents.Top, cameraExtents.Bottom));

                _shapes.Add(new Shape(texture, randPos, size, 0, Color.White, type, out body));
                _bodies.Add(body);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Back))
                Exit();

            Globals.gameTime = gameTime;
            Globals.deltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;

            Globals.keyboard.Update();
            Globals.mouse.Update();

            _camera.Update();

            ReVector movementVector = ReVector.Zero;
            float speed = 500;
            if (Globals.keyboard.GetPress("W")) { movementVector.Y--; }
            if (Globals.keyboard.GetPress("A")) { movementVector.X--; }
            if (Globals.keyboard.GetPress("S")) { movementVector.Y++; }
            if (Globals.keyboard.GetPress("D")) { movementVector.X++; }

            if (movementVector.X != 0 || movementVector.Y != 0)
            {
                movementVector.Normalise();
                movementVector *= speed * Globals.deltaTime; 
            }

            _shapes[0].Move(movementVector);

            //_shapes[0].Rotate(MathF.PI / 2 * Globals.deltaTime);

            foreach (var shape in _shapes)
            {
                shape.Rotate(MathF.PI / 2 * Globals.deltaTime);
                shape.colour = Color.White;
            }

            for (int i = 0; i < _bodies.Count - 1; i++)
            {
                ReBody bodyA = _bodies[i];

                for (int j = i + 1; j < _bodies.Count; j++)
                {
                    ReBody bodyB = _bodies[j];

                    if (bodyA.ShapeType == ShapeType.OBB)
                    {
                        if (bodyB.ShapeType == ShapeType.OBB)
                        {
                            if (ReCollisions.IntersectingOBBs(bodyA.GetTransformedVertices(), bodyB.GetTransformedVertices(), out ReVector normal, out float depth))
                            {
                                bodyA.Move(-normal * depth / 2);
                                bodyB.Move(normal * depth / 2);
                            }
                        }
                        else if (bodyB.ShapeType == ShapeType.Circle)
                        {
                            if (ReCollisions.IntersectingCircleBox(bodyB, bodyA.GetTransformedVertices(), out ReVector normal, out float depth))
                            {
                                bodyA.Move(normal * depth / 2);
                                bodyB.Move(-normal * depth / 2); // Need to reverse normal
                            }
                        }
                    }
                    else if (bodyA.ShapeType == ShapeType.Circle)
                    {
                        if (bodyB.ShapeType == ShapeType.OBB)
                        {
                            if (ReCollisions.IntersectingCircleBox(bodyA, bodyB.GetTransformedVertices(), out ReVector normal, out float depth))
                            {
                                bodyA.Move(-normal * depth / 2);
                                bodyB.Move(normal * depth / 2);
                            }
                        }
                        else if (bodyB.ShapeType == ShapeType.Circle)
                        {
                            if (ReCollisions.IntersectingCircles(bodyA, bodyB, out ReVector normal, out float depth))
                            {
                                bodyA.Move(-normal * depth / 2);
                                bodyB.Move(normal * depth / 2);
                            }
                        }
                    }







                    //if (ReCollisions.IntersectingCircles(bodyA, bodyB, out ReVector normal, out float depth))
                    //{
                    //    bodyA.Move(-normal * depth / 2);
                    //    bodyB.Move(normal * depth / 2);
                    //}

                    //if (ReCollisions.IntersectingOBBs(bodyA.GetTransformedVertices(), bodyB.GetTransformedVertices(), out ReVector normal, out float depth))
                    //{
                    //    _shapes[i].colour = Color.Red;
                    //    _shapes[j].colour = Color.Red;

                    //    bodyA.Move(-normal * depth / 2);
                    //    bodyB.Move(normal * depth / 2);
                    //}
                }
            }            

            //Debug.WriteLine(Globals.mouse.newMousePos);

            Globals.keyboard.UpdateOld();
            Globals.mouse.UpdateOld();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            Globals.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _camera.GetMatrix());

            foreach (Shape shape in _shapes)
            {
                shape.Draw();
            }

            Globals.spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

using System;
using Grondslag;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RePhysics;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Linq;

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

        private SpriteFont _consolas18;

        private ReWorld _physicsWorld;

        private Random _rand = new Random();

        private Stopwatch _stopwatch;

        private double _totalWorldStepTime;
        private int _totalSampleCount;
        private int _totalBodyCount;
        private Stopwatch _sampleTimer = new Stopwatch();

        private string _worldStepTimeString = "0";
        private string _bodyCountString = "0";

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

            _stopwatch = new Stopwatch();
            _sampleTimer.Start();

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

            _consolas18 = Globals.content.Load<SpriteFont>("Consolas18");

            Rectangle cameraExtents = _camera.GetExtents();

            _physicsWorld = new ReWorld();

            _shapes.Add(new Shape(_rectangleTexture, new Vector2(800, 800), new Vector2(3200, 96), 0f, true, 0f, Color.DarkOliveGreen, ShapeType.OBB, out ReBoxBody groundBody));
            _physicsWorld.AddBody(groundBody);
            _shapes.Add(new Shape(_rectangleTexture, new Vector2(400, 200), new Vector2(600, 32), 0.45f, true, 0f, Color.DarkOliveGreen, ShapeType.OBB, out groundBody));
            _physicsWorld.AddBody(groundBody);
            _shapes.Add(new Shape(_rectangleTexture, new Vector2(1200, 300), new Vector2(600, 32), -0.45f, true, 0f, Color.DarkOliveGreen, ShapeType.OBB, out groundBody));
            _physicsWorld.AddBody(groundBody);

            #region Random placement
            //Texture2D texture;
            //Vector2 size = new Vector2(64, 64);
            //ShapeType type = ShapeType.OBB;
            //ReBody body;

            //for (int i = 0; i < 50; i++)
            //{
            //    int randInt = _rand.Next(0, 2);
            //    if (randInt == 0)
            //    {
            //        texture = _rectangleTexture;
            //        type = ShapeType.OBB;
            //        size = new Vector2(57, 57);
            //    }
            //    else
            //    {
            //        texture = _circleTexture;
            //        type = ShapeType.Circle;
            //        size = new Vector2(64, 64);
            //    }

            //    Vector2 randPos = new Vector2(_rand.Next(cameraExtents.Left, cameraExtents.Right), _rand.Next(cameraExtents.Top, cameraExtents.Bottom));
            //    bool isStatic = _rand.Next(0, 2) == 1;

            //    _shapes.Add(new Shape(texture, randPos, size, 0, isStatic, (isStatic) ? new Color(40, 40, 40) : Color.White, type, out body));
            //    _physicsWorld.AddBody(body);
            //}
            #endregion
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

            Globals.MouseWorldPosition = Vector2.Transform(Globals.mouse.mousePos, Matrix.Invert(_camera.GetMatrix()));

            ReVector forceDirection = ReVector.Zero;
            float magnitude = 3000;
            if (Globals.keyboard.GetPress("W")) { forceDirection.Y--; }
            if (Globals.keyboard.GetPress("A")) { forceDirection.X--; }
            if (Globals.keyboard.GetPress("S")) { forceDirection.Y++; }
            if (Globals.keyboard.GetPress("D")) { forceDirection.X++; }

            if (forceDirection.X != 0 || forceDirection.Y != 0)
            {
                forceDirection.Normalise();
                forceDirection *= magnitude;

                if (_shapes[0].CollisionBody is RePhysicsBody body)
                {
                    body.ApplyForce(forceDirection);
                }
            }
            //_shapes[0].colour = Color.DarkRed;

            //foreach (var shape in _shapes)
            //{
            //    shape.Rotate(MathF.PI / 2 * Globals.deltaTime);
            //}

            if (Globals.mouse.LeftClick())
            {
                float width = _rand.Next(32, 129);
                float height = _rand.Next(32, 129);

                if (width % 2 == 1)
                {
                    width += 1;
                }
                if (height % 2 == 1)
                {
                    height += 1;
                }

                _shapes.Add(new Shape(_rectangleTexture, Globals.MouseWorldPosition, new Vector2(width, height), 0f, false, 0.5f, Color.White, ShapeType.OBB, out ReBoxBody body));
                _physicsWorld.AddBody(body);
                //_shapes.Add(new Shape(_rectangleTexture, Globals.MouseWorldPosition, new Vector2(width, height), 0f, false, 0.5f, Color.White, ShapeType.OBB, out body));
                //_physicsWorld.AddBody(body);
                //_shapes.Add(new Shape(_rectangleTexture, Globals.MouseWorldPosition, new Vector2(width, height), 0f, false, 0.5f, Color.White, ShapeType.OBB, out body));
                //_physicsWorld.AddBody(body);
            }
            if (Globals.mouse.RightClick())
            {
                float diameter = _rand.Next(32, 129);

                _shapes.Add(new Shape(_circleTexture, Globals.MouseWorldPosition, new Vector2(diameter), 0f, false, 0.5f, Color.White, ShapeType.Circle, out ReBoxBody body));
                _physicsWorld.AddBody(body);
            }

            _stopwatch.Restart();
            _physicsWorld.Step(Globals.deltaTime, 20);
            _stopwatch.Stop();

            _totalWorldStepTime += _stopwatch.Elapsed.TotalMilliseconds;
            _totalBodyCount += _physicsWorld.BodyCount;
            _totalSampleCount++;

            if (_sampleTimer.Elapsed.TotalSeconds > 1d)
            {
                _bodyCountString = Math.Round(_totalBodyCount / (double)_totalSampleCount, 4).ToString();
                _worldStepTimeString = Math.Round(_totalWorldStepTime / (double)_totalSampleCount, 4).ToString();

                _totalWorldStepTime = 0;
                _totalSampleCount = 0;
                _totalBodyCount = 0;

                _sampleTimer.Restart();
            }

            for (int i = _shapes.Count - 1; i >= 0; i--)
            {
                if (_shapes[i].Remove)
                {
                    _shapes.RemoveAt(i);
                }
            }

            Globals.keyboard.UpdateOld();
            Globals.mouse.UpdateOld();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(40, 40, 40));

            // TODO: Add your drawing code here

            Globals.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _camera.GetMatrix());

            foreach (Shape shape in _shapes)
            {
                shape.Draw();
            }

            float rot = MathF.PI / 4;
            //foreach (var point in _physicsWorld.ContactPointsList)
            //{
            //    Globals.spriteBatch.Draw(_rectangleTexture, ReConverter.ToMGVector2(point), null, Color.Black, rot, new Vector2(_rectangleTexture.Width * 0.5f, _rectangleTexture.Height * 0.5f), 8, SpriteEffects.None, 0);
            //}
            //foreach (var contact in _physicsWorld.ContactManifoldList)
            //{
            //    Color colour = Color.Black;
            //    if (contact.NoRotation) colour = Color.Orange;
            //    Globals.spriteBatch.Draw(_rectangleTexture, ReConverter.ToMGVector2(contact.Contact1), null, colour, rot, new Vector2(_rectangleTexture.Width * 0.5f, _rectangleTexture.Height * 0.5f), 8, SpriteEffects.None, 0);

            //    if (contact.ContactCount == 2)
            //    {
            //        Globals.spriteBatch.Draw(_rectangleTexture, ReConverter.ToMGVector2(contact.Contact2), null, colour, rot, new Vector2(_rectangleTexture.Width * 0.5f, _rectangleTexture.Height * 0.5f), 8, SpriteEffects.None, 0);
            //    }
            //}

            Globals.spriteBatch.End();

            // Second batch

            Globals.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            Globals.spriteBatch.DrawString(_consolas18, "WorldStepTime: " + _worldStepTimeString + "ms", new Vector2(32, 32), Color.White);
            Globals.spriteBatch.DrawString(_consolas18, "BodyCount: " + _bodyCountString, new Vector2(32, 64), Color.White);

            Globals.spriteBatch.End();

            base.Draw(gameTime);
        }
        private void WrapScreen()
        {
            Rectangle cameraExtents = _camera.GetExtents();

            for (int i = 0; i < _physicsWorld.BodyCount; i++)
            {
                if (!_physicsWorld.GetBody(i, out ReBoxBody body))
                {
                    throw new Exception();
                }

                if (body.Pos.X < cameraExtents.Left) body.Move(new ReVector(cameraExtents.Width, 0));
                if (body.Pos.X > cameraExtents.Right) body.Move(new ReVector(-cameraExtents.Width, 0));
                if (body.Pos.Y < cameraExtents.Top) body.Move(new ReVector(0, cameraExtents.Height));
                if (body.Pos.Y > cameraExtents.Bottom) body.Move(new ReVector(0, -cameraExtents.Height));
            }
        }
    }
}

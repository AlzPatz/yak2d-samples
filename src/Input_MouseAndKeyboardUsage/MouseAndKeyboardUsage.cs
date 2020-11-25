using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SampleBase;
using Yak2D;

namespace Input_MouseAndKeyboardUsage
{
    /// <summary>
    /// 
    /// </summary>
    public class MouseAndKeyboardUsage : ApplicationBase
    {
        private const float FONT_SIZE = 32.0f;
        private const float BOUNDARY_SIZE = 12.0f;
        private const float MIN_LAUNCH_SPEED = 300.0f;
        private const float MAX_LAUNCH_SPEED = 600.0f;
        private const float LAUNCH_Y = 0.0f;
        private const float GRAVITY = 980f;
        private const float KEY_DEPTH = 0.8f;
        private const float OUTLINE_WIDTH = 3.0f;
        private const float PARTICLE_DEPTH = 0.9f;
        private const int NUM_PARTICLES_EXPLOSION = 64;
        private const int NUM_SIDES_PARTICLE = 16;
        private const float PARTICLE_MIN_LAUNCH_SPEED = 100.0f;
        private const float PARTICLE_MAX_LAUNCH_SPEED = 600.0f;
        private const float EXPLOSION_PARTICLE_LIFESPAN = 2.0f;
        private const float SHOT_MARKER_LIFESPAN = 1.0f;
        private const float EXPLOSION_PARTICLE_RADIUS = 4.0f;
        private const float SHOT_MARKER_RADIUS = 5.0f;
        private const float CROSS_HAIRS_SIZE = 32.0f;
        private const float CROSS_HAIRS_WIDTH = 4.0f;
        private const float CROSS_HAIRS_SPACING = 4.0f; 
        private const float CROSS_HAIRS_DEPTH = 0.2f;
        private Colour ALIVE_COLOUR = Colour.Green;
        private Colour DEAD_COLOUR = Colour.Red;
        private Colour EXPLOSION_PARTICLE_COLOUR = Colour.DarkRed;
        private Colour SHOT_MARKER_COLOUR = Colour.Yellow;
        private Colour CROSS_HAIRS_COLOUR = Colour.Blue;

        private IDrawStage _drawStage;
        private ICamera2D _camera;

        private class KeyTarget
        {
            public bool Alive { get; set; }
            public KeyCode Key { get; set; }
            public Vector2 Position { get; set; }
            public Vector2 Velocity { get; set; }
            public Vector2 Dimensions { get; set; }
        }

        private List<KeyTarget> _keys;
        private Random _rnd;

        private class Particle
        {
            public bool GravityEffected { get; set; }
            public float Radius { get; set; }
            public Colour Colour { get; set; }
            public Vector2 Position { get; set; }
            public Vector2 Velocity { get; set; }
            public float LifeSpan { get; set; }
            public float Age { get; set; }
        }

        private List<Particle> _particles;

        public override string ReturnWindowTitle() => "Mouse & Keyboard: Type to Generate Letters and Left Click to Shoot Them";

        public override void OnStartup() 
        { 
            _keys = new List<KeyTarget>();
            _particles = new List<Particle>();
            _rnd = new Random();
        }

        public override bool CreateResources(IServices yak)
        {
            _drawStage = yak.Stages.CreateDrawStage();

            _camera = yak.Cameras.CreateCamera2D();

            yak.Display.SetCursorVisible(true);

            return true;
        }

        //BEST practice is to do ALL input HANDLING in the UPDATES
        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds)
        {
            var input = yak.Input;

            input.KeysReleasedThisFrame().ToList().ForEach(released =>
            {
                _keys.ForEach(key =>
                {
                    if(key.Key == released)
                    {
                        key.Alive = false;
                    }
                });
            });

            input.KeysPressedThisFrame().ToList().ForEach(pressed =>
            {
                var rx = -1.0f + (2.0f * (float)_rnd.NextDouble());
                var ry = (float)_rnd.NextDouble();

                var direction = new Vector2(rx, ry);
                if(direction != Vector2.Zero)
                {
                    direction = Vector2.Normalize(direction);
                }

                var strWidth = yak.Fonts.MeasureStringLength(pressed.ToString(), FONT_SIZE);
                var dimensions = new Vector2(strWidth + (2.0f * FONT_SIZE),  FONT_SIZE + (2.0f * BOUNDARY_SIZE));

                _keys.Add(new KeyTarget
                {
                    Alive = true,
                    Key = pressed,
                    Position = new Vector2(0.0f, LAUNCH_Y),
                    Velocity = direction * (MIN_LAUNCH_SPEED + ((float)_rnd.NextDouble() * (MAX_LAUNCH_SPEED - MIN_LAUNCH_SPEED))),
                    Dimensions = dimensions
                });
            });

            Vector2? fired = null;
            if(input.WasMousePressedThisFrame(MouseButton.Left))
            {
                fired = yak.Helpers.CoordinateTransforms.ScreenFromWindow(input.MousePosition, _camera).Position;
                _particles.Add(new Particle
                {
                    Age = 0.0f,
                    GravityEffected = false,
                    LifeSpan = SHOT_MARKER_LIFESPAN,
                    Colour = SHOT_MARKER_COLOUR,
                    Radius = SHOT_MARKER_RADIUS,
                    Position = (Vector2)fired,
                    Velocity = Vector2.Zero
                });
            }

            Func<Vector2, Vector2, Vector2, bool> detectCollision = (point, position, dimensions) =>
            {
                var half = 0.5f * dimensions;

                return point.X >= position.X - half.X && 
                        point.X <= position.X + half.X &&
                        point.Y >= position.Y - half.Y &&
                        point.Y <= position.Y + half.Y;
            };

            var keysToRemove = new List<KeyTarget>();
            _keys.ForEach(key =>
            {
                if(fired != null)
                {
                    if(detectCollision((Vector2)fired, key.Position, key.Dimensions))
                    {
                        keysToRemove.Add(key);
                        
                        for(var p = 0; p < NUM_PARTICLES_EXPLOSION; p++)
                        {
                            var rx = -1.0f + (2.0f * (float)_rnd.NextDouble()); 
                            var ry = -1.0f + (2.0f * (float)_rnd.NextDouble()); 

                            var direction = new Vector2(rx, ry);
                            if(direction != Vector2.Zero)
                            {
                                direction = Vector2.Normalize(direction);
                            }
                            var velocity = direction * (PARTICLE_MIN_LAUNCH_SPEED + ((float)_rnd.NextDouble() * (PARTICLE_MAX_LAUNCH_SPEED - PARTICLE_MIN_LAUNCH_SPEED))); 

                            _particles.Add(new Particle
                            {
                                Age = 0.0f,
                                GravityEffected = true,
                                LifeSpan = EXPLOSION_PARTICLE_LIFESPAN,
                                Colour = EXPLOSION_PARTICLE_COLOUR,
                                Radius = EXPLOSION_PARTICLE_RADIUS,
                                Position = (Vector2)fired,
                                Velocity = velocity
                            });
                        }
                    }
                }

                if(!key.Alive)
                {
                    key.Velocity += -new Vector2(0.0f, GRAVITY) * timeSinceLastUpdateSeconds;
                }

                key.Position += key.Velocity * timeSinceLastUpdateSeconds;

                if(key.Position.Y < -8000.0f)
                {
                    keysToRemove.Add(key);
                }
            });

            keysToRemove.ForEach(toRemove => 
            {
                _keys.Remove(toRemove);
            });

            var particlesToRemove = new List<Particle>();
            _particles.ForEach(particle =>
            {
                particle.Age += timeSinceLastUpdateSeconds;
                if(particle.Age > particle.LifeSpan)
                {
                    particle.Age = particle.LifeSpan;
                    particlesToRemove.Add(particle);
                }

                if(particle.GravityEffected)
                {
                    particle.Velocity += new Vector2(0.0f, -GRAVITY) * timeSinceLastUpdateSeconds;
                }
                particle.Position += particle.Velocity * timeSinceLastUpdateSeconds;
            });

            particlesToRemove.ForEach(toRemove => 
            {
                _particles.Remove(toRemove);
            });

            return true;
        }

        /*
            Any INPUT queries made during a draw-related method cannot rely on accurately capturing all 
            INPUTs made "WAS X THIS FRAME". It is only the UPDATE loop cycle that will accurately capture all
            of these types of inputs. INPUTS queried during draw-related methods should be restricted to
            "Currently Held" or Positional type queries that would persist over multiple UPDATES
         */
        public override void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            //Can miss inputs here...
            var uselessK = yak.Input.WasKeyReleasedThisFrame(KeyCode.A);
            var uselessM = yak.Input.WasMouseReleasedThisFrame(MouseButton.Left);
        }

        /*
            Any INPUT queries made during a draw-related method cannot rely on accurately capturing all 
            INPUTs made "WAS X THIS FRAME". It is only the UPDATE loop cycle that will accurately capture all
            of these types of inputs. INPUTS queried during draw-related methods should be restricted to
            "Currently Held" or Positional type queries that would persist over multiple UPDATES
        */
        public override void Drawing(IDrawing draw, IFps fps, IInput input, ICoordinateTransforms transform, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            //Can miss inputs here...
            var uselessK = input.WasKeyPressedThisFrame(KeyCode.A);
            var uselessM = input.WasMousePressedThisFrame(MouseButton.Left);

            _keys.ForEach(key =>
            {
                draw.Helpers.Construct().Coloured(key.Alive ? ALIVE_COLOUR : DEAD_COLOUR)
                    .Quad(key.Position, key.Dimensions.X, key.Dimensions.Y)
                    .Outline(OUTLINE_WIDTH)
                    .SubmitDraw(_drawStage, CoordinateSpace.Screen, KEY_DEPTH, 0);

                draw.DrawString(_drawStage,
                                CoordinateSpace.Screen,
                                key.Key.ToString(),
                                key.Alive ? ALIVE_COLOUR : DEAD_COLOUR,
                                FONT_SIZE,
                                key.Position + new Vector2(0.0f, 0.25f * FONT_SIZE),
                                TextJustify.Centre,
                                KEY_DEPTH,
                                0);
            });

            _particles.ForEach(particle =>
            {
                var col = (1.0f - (particle.Age / particle.LifeSpan)) * particle.Colour;
                draw.Helpers.DrawColouredPoly(_drawStage,
                                              CoordinateSpace.Screen,
                                              col,
                                              particle.Position,
                                              NUM_SIDES_PARTICLE,
                                              particle.Radius,
                                              PARTICLE_DEPTH,
                                              0);
            });

            //Here we are going to use some input within drawing - MousePosition. This is fine as it's
            //value is valid across updates and draws

            var mousePosition = transform.ScreenFromWindow(input.MousePosition, _camera).Position;
            DrawCrossHairs(draw, mousePosition);

            draw.DrawString(_drawStage,
                            CoordinateSpace.Screen,
                            input.IsMouseOverWindow ? "Mouse is over window" : "Mouse is not over window",
                            Colour.White,
                            24,
                            new Vector2(-460.0f, 260.0f),
                            TextJustify.Left,
                            0.1f,
                            0);
        }

        private void DrawCrossHairs(IDrawing draw, Vector2 screenPosition)
        {
            var crossHairsLength = 0.5f * (CROSS_HAIRS_SIZE - (2.0f * CROSS_HAIRS_SPACING));
            draw.Helpers.DrawColouredQuad(_drawStage,
                                          CoordinateSpace.Screen,
                                          CROSS_HAIRS_COLOUR,
                                          screenPosition + new Vector2(0.0f, -(CROSS_HAIRS_SPACING + (0.5f * crossHairsLength))),
                                          CROSS_HAIRS_WIDTH,
                                          crossHairsLength,
                                          CROSS_HAIRS_DEPTH,
                                          0);
            draw.Helpers.DrawColouredQuad(_drawStage,
                                          CoordinateSpace.Screen,
                                          CROSS_HAIRS_COLOUR,
                                          screenPosition + new Vector2(0.0f, (CROSS_HAIRS_SPACING + (0.5f * crossHairsLength))),
                                          CROSS_HAIRS_WIDTH,
                                          crossHairsLength,
                                          CROSS_HAIRS_DEPTH,
                                          0);

            draw.Helpers.DrawColouredQuad(_drawStage,
                                          CoordinateSpace.Screen,
                                          CROSS_HAIRS_COLOUR,
                                          screenPosition + new Vector2(-(CROSS_HAIRS_SPACING + (0.5f * crossHairsLength)), 0.0f),
                                          crossHairsLength,
                                          CROSS_HAIRS_WIDTH,
                                          CROSS_HAIRS_DEPTH,
                                          0);
            draw.Helpers.DrawColouredQuad(_drawStage,
                                          CoordinateSpace.Screen,
                                          CROSS_HAIRS_COLOUR,
                                          screenPosition + new Vector2((CROSS_HAIRS_SPACING + (0.5f * crossHairsLength)), 0.0f),
                                          crossHairsLength,
                                          CROSS_HAIRS_WIDTH,
                                          CROSS_HAIRS_DEPTH,
                                          0);
        }

        public override void Rendering(IRenderQueue q, IRenderTarget windowRenderTarget)
        {
            q.ClearColour(windowRenderTarget, Colour.Clear);
            q.ClearDepth(windowRenderTarget);
            q.Draw(_drawStage, _camera, windowRenderTarget);
        }

        public override void Shutdown() { }
    }
}

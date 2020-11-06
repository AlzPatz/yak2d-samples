using System;
using System.Drawing;
using System.Numerics;
using SampleBase;
using Yak2D;

namespace Draw_Camera2DWorldAndScreen
{
    /// <summary>
    /// Drawing in world and screen positions
    /// Extremely bad driving 'physics' :)
    /// </summary>
    public class WorldAndScreenDrawing : ApplicationBase
    {
        private IDrawStage _drawStage;
        private ICamera2D _camera;
        private ITexture _textureRoads;
        private ITexture _textureCar;
        private Size _roadTextureSize;
        private Size _carTextureSize;

        private const float CAR_SIZE_SCALE = 0.4f;
        private const float CAR_MAX_SPEED = 700.0f;
        private const float CAR_MAX_TURN_SPEED = 200.0f;
        private const float CAR_MAX_TURN_RATE = 2.0f;
        private const float CAR_MIN_TURN_SCALAR = 0.5f;
        private const float CAR_ACCELERATION = 280.0f;
        private const float CAR_DECELERATION = 600.0f;
        private const float CAR_ROLLING_DECELERATION = 120.0f;
        private const float MAX_ZOOM = 2.0f;
        private const float MIN_ZOOM = 0.5f;
        private const float SECONDS_TO_SMOOTH_HALF = 0.2f;

        private Vector2 _carPosition;
        private float _carAngle;
        private float _carSpeed;

        private float _camZoom;
        private float _camAngle;

        public override string ReturnWindowTitle() => "Drawing - World and Screen Coorindate Systems";

        public override void OnStartup()
        {
            _carPosition = new Vector2(0.0f, 0.0f);
            _carAngle = 0.0f;
            _carSpeed = 0.0f;

            _camAngle = 0.0f;
            _camZoom = MAX_ZOOM;
        }

        public override bool CreateResources(IServices yak)
        {
            _drawStage = yak.Stages.CreateDrawStage();

            _camera = yak.Cameras.CreateCamera2D(960, 540, 1.0f);

            _textureRoads = yak.Surfaces.LoadTexture("roads", AssetSourceEnum.Embedded);
            _textureCar = yak.Surfaces.LoadTexture("car", AssetSourceEnum.Embedded);

            _roadTextureSize = yak.Surfaces.GetSurfaceDimensions(_textureRoads);
            _carTextureSize = yak.Surfaces.GetSurfaceDimensions(_textureCar);

            return true;
        }

        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds)
        {
            //Update Speed

            var acceleration = 0.0f;

            if (yak.Input.IsKeyCurrentlyPressed(KeyCode.Up))
            {
                acceleration = 1.0f;
            }

            if (yak.Input.IsKeyCurrentlyPressed(KeyCode.Down))
            {
                acceleration = -1.0f;
            }

            var acceleration_amount = 0.0f;

            if (acceleration == 1.0f)
            {
                acceleration_amount = _carSpeed >= 0.0f ? CAR_ACCELERATION : CAR_DECELERATION;
            }

            if (acceleration == -1.0f)
            {
                acceleration_amount = _carSpeed >= 0.0f ? CAR_DECELERATION : CAR_ACCELERATION;
            }

            _carSpeed += acceleration * acceleration_amount * timeSinceLastUpdateSeconds;

            var s = (float)Math.Abs(_carSpeed);
            if (s > CAR_MAX_SPEED)
            {
                _carSpeed *= CAR_MAX_SPEED / s;
            }

            var minSpeed = 0.001f;
            if (acceleration == 0.0f && Math.Abs(_carSpeed) > minSpeed)
            {
                var decceleration = -CAR_ROLLING_DECELERATION * timeSinceLastUpdateSeconds;

                if (_carSpeed < 0.0f)
                {
                    decceleration *= -1.0f;
                }

                if (Math.Abs(decceleration) > Math.Abs(_carSpeed))
                {
                    decceleration *= Math.Abs(_carSpeed) / Math.Abs(decceleration);
                }

                _carSpeed += decceleration;
            }

            if (Math.Abs(_carSpeed) < minSpeed)
            {
                _carSpeed = 0.0f;
            }

            //Update Angle

            var turn = 0.0f;

            if (yak.Input.IsKeyCurrentlyPressed(KeyCode.Left))
            {
                turn = _carSpeed >= 0.0f ? -1.0f : 1.0f;
            }

            if (yak.Input.IsKeyCurrentlyPressed(KeyCode.Right))
            {
                turn = _carSpeed >= 0.0f ? 1.0f : -1.0f;
            }

            //This turn scaling doesn't work properly, but it's not important to the demo so leaving for now
            var turnScale = 0.0f;
            var speed = (float)Math.Abs(_carSpeed);
            if (speed < CAR_MAX_TURN_SPEED)
            {
                turnScale = speed / CAR_MAX_TURN_SPEED;
            }
            else
            {
                turnScale = 1.0f - ((1.0f - CAR_MIN_TURN_SCALAR) * ((speed - CAR_MAX_TURN_SPEED) / (CAR_MAX_SPEED - CAR_MAX_TURN_SPEED)));
            }

            _carAngle += turnScale * turn * CAR_MAX_TURN_RATE * timeSinceLastUpdateSeconds;

            var twoPi = (float)Math.PI * 2.0f;
            while (_carAngle > twoPi)
            {
                _carAngle -= twoPi;
            }

            while (_carAngle < 0.0f)
            {
                _carAngle += twoPi;
            }

            //Update Position

            var direction = Vector2.Transform(Vector2.UnitY, Matrix3x2.CreateRotation(-_carAngle));

            _carPosition += direction * _carSpeed * timeSinceLastUpdateSeconds;

            //Teleport

            while (_carPosition.X > _roadTextureSize.Width * 0.5f)
            {
                _carPosition.X -= _roadTextureSize.Width;
            }

            while (_carPosition.X < -_roadTextureSize.Width * 0.5f)
            {
                _carPosition.X += _roadTextureSize.Width;
            }

            while (_carPosition.Y > _roadTextureSize.Height * 0.5f)
            {
                _carPosition.Y -= _roadTextureSize.Height;
            }

            while (_carPosition.Y < -_roadTextureSize.Height * 0.5f)
            {
                _carPosition.Y += _roadTextureSize.Height;
            }

            return true;
        }

        public override void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            var speed = (float)Math.Abs(_carSpeed);
            var targetZoom = MIN_ZOOM + ((MAX_ZOOM - MIN_ZOOM) * (1.0f - (speed / CAR_MAX_SPEED)));

            var twoPi = (float)Math.PI * 2.0f;
            if (Math.Abs(_camAngle - _carAngle) > Math.PI)
            {
                if (_carAngle < _camAngle)
                {
                    _camAngle -= twoPi;
                }
                else
                {
                    _camAngle += twoPi;
                }
            }

            _camAngle = SmoothTowards(_camAngle, _carAngle, timeSinceLastDrawSeconds);
            _camZoom = SmoothTowards(_camZoom, targetZoom, timeSinceLastDrawSeconds);

            yak.Cameras.SetCamera2DFocusZoomAndRotation(_camera, _carPosition, _camZoom, _camAngle);
        }

        private float SmoothTowards(float current, float target, float seconds)
        {
            var diff = target - current;
            var half = 0.5f * diff;
            var amount_in_period = half * (seconds / SECONDS_TO_SMOOTH_HALF);

            return current + amount_in_period;
        }

        public override void Drawing(IDrawing draw,
                                     IFps fps,
                                     IInput input,
                                     ICoordinateTransforms transforms,
                                     float timeSinceLastDrawSeconds,
                                     float timeSinceLastUpdateSeconds)
        {
            var helper = draw.Helpers;

            //Draw a 9x9 equivalent quad for the roads, so that the car can drive over the edge before being teleported back
            helper.DrawTexturedQuad(_drawStage,
                                    CoordinateSpace.World,
                                    _textureRoads,
                                    Colour.White,
                                    Vector2.Zero,
                                    3 * _roadTextureSize.Width,
                                    3 * _roadTextureSize.Height,
                                    0.9f,
                                    0,
                                    0.0f,
                                    -1.0f,
                                    -1.0f,
                                    2.0f,
                                    2.0f,
                                    TextureCoordinateMode.Wrap);

            //Draw the car
            helper.DrawTexturedQuad(_drawStage,
                        CoordinateSpace.World,
                        _textureCar,
                        Colour.White,
                        _carPosition,
                        _carTextureSize.Width * CAR_SIZE_SCALE,
                        _carTextureSize.Height * CAR_SIZE_SCALE,
                        0.7f,
                        0,
                        _carAngle,
                        0.0f,
                        0.0f,
                        1.0f,
                        1.0f,
                        TextureCoordinateMode.Wrap);

            //Draw a UI Element in Screen Space
            helper.DrawColouredQuad(_drawStage,
                                    CoordinateSpace.Screen,
                                    Colour.PaleVioletRed,
                                    new Vector2(0.0f, 250f),
                                    960.0f,
                                    40.0f,
                                    0.6f,
                                    0);

            //Draw UI Speed in Screen Space
            draw.DrawString(_drawStage,
                               CoordinateSpace.Screen,
                               string.Concat("Speed: ", (_carSpeed / 10.0f).ToString("0"), "mph"),
                               Colour.White,
                               28,
                               new Vector2(-470.0f, 258.0f),
                               TextJustify.Left,
                               0.4f,
                               0);


            //Controls Message UI
            draw.DrawString(_drawStage,
                               CoordinateSpace.Screen,
                               string.Concat("Drive with the Arrow Keys"),
                               Colour.White,
                               28,
                               new Vector2(470.0f, 258.0f),
                               TextJustify.Right,
                               0.4f,
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
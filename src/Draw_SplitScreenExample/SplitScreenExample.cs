using System;
using System.Drawing;
using System.Numerics;
using SampleBase;
using Yak2D;

namespace Draw_SplitScreenExample
{
    public class SplitScreenExample : ApplicationBase
    {
        private IDrawStage _drawStageTanks;
        private IDrawStage _drawStageGui;

        private ICamera2D _cameraP1;
        private ICamera2D _cameraP2;
        private ICamera2D _cameraGui;

        private ITexture _texMap;
        private ITexture _texTank;
        private ITexture _texWheels;
        private IViewport _viewportP1;
        private IViewport _viewportP2;

        private Size _mapTextureSize;
        private Size _wheelsTextureSize;
        private Size _tankTextureSize;

        private Vehicle _tankP1;
        private Vehicle _tankP2;
        private TrackingCamera _trackingCamP1;
        private TrackingCamera _trackingCamP2;

        private const float TANK_SIZE_SCALE = 0.1f;
        private const float TANK_MAX_SPEED = 700.0f;
        private const float TANK_MAX_TURN_SPEED = 200.0f;
        private const float TANK_MAX_TURN_RATE = 2.0f;
        private const float TANK_MIN_TURN_SCALAR = 0.5f;
        private const float TANK_ACCELERATION = 280.0f;
        private const float TANK_DECELERATION = 600.0f;
        private const float TANK_ROLLING_DECELERATION = 120.0f;
        private const float MAX_ZOOM = 1.2f;
        private const float MIN_ZOOM = 0.8f;
        private const float SECONDS_TO_SMOOTH_HALF = 0.2f;

        public override string ReturnWindowTitle() => "Drawing - Splitscreen using Viewports and the same draw queue";

        public override void OnStartup()
        {
            _tankP1 = new Vehicle(-1024.0f,
                                  0.0f,
                                  KeyCode.W,
                                  KeyCode.S,
                                  KeyCode.A,
                                  KeyCode.D,
                                  TANK_MAX_SPEED,
                                  TANK_MAX_TURN_SPEED,
                                  TANK_MAX_TURN_RATE,
                                  TANK_MIN_TURN_SCALAR,
                                  TANK_ACCELERATION,
                                  TANK_DECELERATION,
                                  TANK_ROLLING_DECELERATION);

            _tankP2 = new Vehicle(1024.0f,
                                  0.0f,
                                  KeyCode.Up,
                                  KeyCode.Down,
                                  KeyCode.Left,
                                  KeyCode.Right,
                                  TANK_MAX_SPEED,
                                  TANK_MAX_TURN_SPEED,
                                  TANK_MAX_TURN_RATE,
                                  TANK_MIN_TURN_SCALAR,
                                  TANK_ACCELERATION,
                                  TANK_DECELERATION,
                                  TANK_ROLLING_DECELERATION);

            _trackingCamP1 = new TrackingCamera(_tankP1.Angle,
                                                MAX_ZOOM,
                                                MIN_ZOOM,
                                                SECONDS_TO_SMOOTH_HALF,
                                                TANK_MAX_SPEED);

            _trackingCamP2 = new TrackingCamera(_tankP2.Angle,
                                                MAX_ZOOM,
                                                MIN_ZOOM,
                                                SECONDS_TO_SMOOTH_HALF,
                                                TANK_MAX_SPEED);
        }

        public override bool CreateResources(IServices yak)
        {
            _drawStageTanks = yak.Stages.CreateDrawStage();
            _drawStageGui = yak.Stages.CreateDrawStage();

            _cameraP1 = yak.Cameras.CreateCamera2D(480, 540, 1.0f);
            _cameraP2 = yak.Cameras.CreateCamera2D(480, 540, 1.0f);
            _cameraGui = yak.Cameras.CreateCamera2D(960, 540, 1.0f);

            _texMap = yak.Surfaces.LoadTexture("map", AssetSourceEnum.Embedded);
            _texTank = yak.Surfaces.LoadTexture("tank", AssetSourceEnum.Embedded);
            _texWheels = yak.Surfaces.LoadTexture("wheels", AssetSourceEnum.Embedded);

            _viewportP1 = yak.Stages.CreateViewport(0, 0, 480, 540);
            _viewportP2 = yak.Stages.CreateViewport(480, 0, 480, 540);

            _mapTextureSize = yak.Surfaces.GetSurfaceDimensions(_texMap);
            _wheelsTextureSize = yak.Surfaces.GetSurfaceDimensions(_texWheels);
            _tankTextureSize = yak.Surfaces.GetSurfaceDimensions(_texTank);

            return true;
        }

        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds)
        {
            var input = yak.Input;

            _tankP1.Update(input, timeSinceLastUpdateSeconds);
            _tankP2.Update(input, timeSinceLastUpdateSeconds);

            return true;
        }

        public override void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            _trackingCamP1.Update(_tankP1, timeSinceLastDrawSeconds);
            _trackingCamP2.Update(_tankP2, timeSinceLastDrawSeconds);

            yak.Cameras.SetCamera2DFocusZoomAndRotation(_cameraP1,
                                                                                             _tankP1.Position,
                                                                                             _trackingCamP1.Zoom,
                                                                                             _trackingCamP1.Angle);

            yak.Cameras.SetCamera2DFocusZoomAndRotation(_cameraP2,
                                                                                             _tankP2.Position,
                                                                                             _trackingCamP2.Zoom,
                                                                                             _trackingCamP2.Angle);
        }

        public override void Drawing(IDrawing draw,
                                     IFps fps,
                                     IInput input,
                                     ICoordinateTransforms transforms,
                                     float timeSinceLastDrawSeconds,
                                     float timeSinceLastUpdateSeconds)
        {
            var helper = draw.Helpers;

            //Draw the map
            helper.DrawTexturedQuad(_drawStageTanks,
                                    CoordinateSpace.World,
                                    _texMap,
                                    Colour.White,
                                    Vector2.Zero,
                                    _mapTextureSize.Width,
                                    _mapTextureSize.Height,
                                    0.9f,
                                    0,
                                    0.0f,
                                    0.0f,
                                    0.0f,
                                    1.0f,
                                    1.0f,
                                    TextureCoordinateMode.Wrap);

            //Draw Tank P1

            helper.DrawTexturedQuad(_drawStageTanks,
                        CoordinateSpace.World,
                        _texWheels,
                        Colour.White,
                        _tankP1.Position,
                        _wheelsTextureSize.Width * TANK_SIZE_SCALE,
                        _wheelsTextureSize.Height * TANK_SIZE_SCALE,
                        0.7f,
                        0,
                        _tankP1.Angle,
                        0.0f,
                        0.0f,
                        1.0f,
                        1.0f,
                        TextureCoordinateMode.Wrap);

            helper.DrawTexturedQuad(_drawStageTanks,
                    CoordinateSpace.World,
                    _texTank,
                    Colour.Red,
                    _tankP1.Position,
                    _tankTextureSize.Width * TANK_SIZE_SCALE,
                    _tankTextureSize.Height * TANK_SIZE_SCALE,
                    0.65f,
                    0,
                    _tankP1.Angle,
                    0.0f,
                    0.0f,
                    1.0f,
                    1.0f,
                    TextureCoordinateMode.Wrap);

            //Draw Tank P2

            helper.DrawTexturedQuad(_drawStageTanks,
                 CoordinateSpace.World,
                 _texWheels,
                 Colour.White,
                 _tankP2.Position,
                 _wheelsTextureSize.Width * TANK_SIZE_SCALE,
                 _wheelsTextureSize.Height * TANK_SIZE_SCALE,
                 0.6f,
                 0,
                 _tankP2.Angle,
                 0.0f,
                 0.0f,
                 1.0f,
                 1.0f,
                 TextureCoordinateMode.Wrap);

            helper.DrawTexturedQuad(_drawStageTanks,
                    CoordinateSpace.World,
                    _texTank,
                    Colour.Blue,
                    _tankP2.Position,
                    _tankTextureSize.Width * TANK_SIZE_SCALE,
                    _tankTextureSize.Height * TANK_SIZE_SCALE,
                    0.55f,
                    0,
                    _tankP2.Angle,
                    0.0f,
                    0.0f,
                    1.0f,
                    1.0f,
                    TextureCoordinateMode.Wrap);

            //Draw a UI Elements in Screen Space

            //A rectangle for each screen split (use fluent draw interface helper for fun)
            helper.Construct().Coloured(Colour.Black).Quad(new Vector2(-240.0f, 0.0f), 480.0f, 540.0f).Outline(1.0f).SubmitDraw(_drawStageGui, CoordinateSpace.Screen, 0.3f, 0);
            helper.Construct().Coloured(Colour.Black).Quad(new Vector2(240.0f, 0.0f), 480.0f, 540.0f).Outline(1.0f).SubmitDraw(_drawStageGui, CoordinateSpace.Screen, 0.3f, 0);

            //Coloured bar
            helper.DrawColouredQuad(_drawStageGui,
                                    CoordinateSpace.Screen,
                                    Colour.Chocolate,
                                    new Vector2(0.0f, 250f),
                                    960.0f,
                                    40.0f,
                                    0.2f,
                                    0);

            //Draw some UI text
            draw.DrawString(_drawStageGui,
                               CoordinateSpace.Screen,
                               "Tanks",
                               Colour.White,
                               28,
                               new Vector2(-470.0f, 258.0f),
                               TextJustify.Left,
                               0.1f,
                               0);


            //Controls Message UI
            draw.DrawString(_drawStageGui,
                               CoordinateSpace.Screen,
                               string.Concat("Control with WASD and Arrow Keys"),
                               Colour.White,
                               28,
                               new Vector2(470.0f, 258.0f),
                               TextJustify.Right,
                               0.1f,
                               0);
        }

        public override void Rendering(IRenderQueue q, IRenderTarget windowRenderTarget)
        {
            q.ClearColour(windowRenderTarget, Colour.Yellow);
            q.ClearDepth(windowRenderTarget);

            q.SetViewport(_viewportP1);
            q.Draw(_drawStageTanks, _cameraP1, windowRenderTarget);

            q.SetViewport(_viewportP2);
            q.Draw(_drawStageTanks, _cameraP2, windowRenderTarget);

            q.RemoveViewport();
            q.Draw(_drawStageGui, _cameraGui, windowRenderTarget);
        }

        public override void Shutdown() { }
    }
}
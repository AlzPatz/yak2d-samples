using Yak2D;
using SampleBase;
using System.Numerics;
using System;
using System.Drawing;
using System.Collections.Generic;

namespace Helper_CoordinateTranforms
{
    /// <summary>
    /// 
    /// </summary>
    public class CoordinateTransformsExample : ApplicationBase
    {
        private const float FRAC_HORIZONTAL_WIDTH_ON_MOVE = 0.1f;

        private IDrawStage _drawStageViewport;
        private IDrawStage _drawStageGUI;
        private ICamera2D _cameraViewport;
        private ICamera2D _cameraGUI;
        private IViewport _viewport;
        private ITexture _texture;
        private Size _textureSize;

        private float _zoom;
        private Vector2 _worldFocus;
        private float _rotation;
        private float _textureSizeScalar;

        public override string ReturnWindowTitle() => "Coordinate Transforms - Converting Between Window, Camera 'Screen' and Camera 'World' Coordinates";

        public override void OnStartup() { }

        public override bool CreateResources(IServices yak)
        {
            _drawStageViewport = yak.Stages.CreateDrawStage();
            _drawStageGUI = yak.Stages.CreateDrawStage();

            _cameraViewport = yak.Cameras.CreateCamera2D(1920, 1080);
            _cameraGUI = yak.Cameras.CreateCamera2D();

            _viewport = yak.Stages.CreateViewport(220, 120, 720, 405);

            _texture = yak.Surfaces.LoadTexture("sprite", AssetSourceEnum.Embedded);
            _textureSize = yak.Surfaces.GetSurfaceDimensions(_texture);

            _textureSizeScalar = 2.0f;

            _zoom = 1.0f;
            _worldFocus = Vector2.Zero;
            _rotation = 0.0f;

            yak.Cameras.SetCamera2DFocusZoomAndRotation(_cameraViewport, _worldFocus, _zoom, _rotation);

            return true;
        }
        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds)
        {
            var camMove = Vector2.Zero;
            if (yak.Input.WasKeyReleasedThisFrame(KeyCode.Up))
            {
                camMove += Vector2.UnitY;
            }
            if (yak.Input.WasKeyReleasedThisFrame(KeyCode.Down))
            {
                camMove -= Vector2.UnitY;
            }
            if (yak.Input.WasKeyReleasedThisFrame(KeyCode.Left))
            {
                camMove -= Vector2.UnitX;
            }
            if (yak.Input.WasKeyReleasedThisFrame(KeyCode.Right))
            {
                camMove += Vector2.UnitX;
            }
            var moveAmount = (FRAC_HORIZONTAL_WIDTH_ON_MOVE * 1920.0f) / _zoom;
            _worldFocus += moveAmount * camMove;

            if (yak.Input.WasKeyReleasedThisFrame(KeyCode.PageUp))
            {
                _zoom *= 2.0f;
            }

            if (yak.Input.WasKeyReleasedThisFrame(KeyCode.PageDown))
            {
                _zoom /= 2.0f;
            }

            if (yak.Input.WasKeyReleasedThisFrame(KeyCode.A))
            {
                _rotation -= 0.125f * (float)Math.PI;
                if (_rotation > 0.0f)
                {
                    _rotation += 2.0f * (float)Math.PI;
                }
            }

            if (yak.Input.WasKeyReleasedThisFrame(KeyCode.D))
            {
                _rotation += 0.125f * (float)Math.PI;
                if (_rotation > 2.0f * (float)Math.PI)
                {
                    _rotation -= 2.0f * (float)Math.PI;
                }
            }

            return true;
        }

        public override void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) 
        {
            yak.Cameras.SetCamera2DFocusZoomAndRotation(_cameraViewport, _worldFocus, _zoom, _rotation);
        }

        public override void Drawing(IDrawing draw, IFps fps, IInput input, ICoordinateTransforms transform, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            //Draw Black Background across viewport
            draw.Helpers.DrawColouredQuad(_drawStageViewport,
                                             CoordinateSpace.Screen,
                                             Colour.Black,
                                              Vector2.Zero,
                                              1920,
                                              1080,
                                              0.9f,
                                              0);

            //Draw single sprite
            draw.Helpers.DrawTexturedQuad(_drawStageViewport,
                                          CoordinateSpace.World,
                                          _texture,
                                          Colour.White,
                                          Vector2.Zero,
                                          _textureSizeScalar * _textureSize.Width,
                                          _textureSizeScalar * _textureSize.Height,
                                          0.5f,
                                          0);

            //WINDOW to SCREEN

            var mouseScreen = transform.ScreenFromWindow(input.MousePosition, _cameraViewport, _viewport);

            var textShift = new Vector2(0.0f, 60.0f);

            if (mouseScreen.Contained)
            {
                draw.DrawString(_drawStageViewport,
                                CoordinateSpace.Screen,
                                string.Concat("Window Point in Screen Space: ", mouseScreen.Position.ToString("0"), "Blue Circle in World Space"),
                                Colour.White,
                                42,
                                mouseScreen.Position + textShift,
                                TextJustify.Centre,
                                0.8f,
                                1);
            }

            // WINDOW to WORLD

            var mouseWorld = transform.WorldFromWindow(input.MousePosition, _cameraViewport, _viewport);

            if (mouseWorld.Contained)
            {
                draw.Helpers.Construct().Coloured(Colour.Blue).Poly(mouseWorld.Position, 32, 32.0f).Outline(16.0f).SubmitDraw(_drawStageViewport, CoordinateSpace.World, 0.9f, 1);
            }

            // WORLD to SCREEN

            var screenFromWorld = transform.ScreenFromWorld(new Vector2(-400.0f, -300.0f), _cameraViewport);

            draw.DrawString(_drawStageViewport,
                               CoordinateSpace.Screen,
                               string.Concat("Fixed World Point Rendered In Screen Space"),
                               Colour.Green,
                               56,
                               screenFromWorld + textShift,
                               TextJustify.Centre,
                               0.7f,
                               1);

            draw.Helpers.Construct().Coloured(Colour.Green).Poly(screenFromWorld, 32, 16.0f).Filled().SubmitDraw(_drawStageViewport, CoordinateSpace.Screen, 0.6f, 1);

            //SCREEN to WORLD

            var fixedScreenPoint = new Vector2(300.0f, 300.0f);

            var worldFromScreeen = transform.WorldFromScreen(fixedScreenPoint, _cameraViewport);

            draw.DrawString(_drawStageViewport,
                              CoordinateSpace.World,
                              string.Concat("Fixed Screen Point Rendered in World!"),
                              Colour.Yellow,
                              56,
                              worldFromScreeen + textShift,
                              TextJustify.Centre,
                              0.7f,
                              1);

            draw.Helpers.Construct().Coloured(0.5f * Colour.Yellow).Quad(worldFromScreeen, 48.0f, 48.0f).Outline(10.0f).SubmitDraw(_drawStageViewport, CoordinateSpace.World, 0.9f, 1);

            //GUI TEXT

            draw.DrawString(_drawStageGUI,
                   CoordinateSpace.Screen,
                   "Transforming Coordinates",
                   Colour.White,
                   34,
                   new Vector2(-460.0f, 250.0f),
                   TextJustify.Left,
                   0.4f,
                   2);

            draw.DrawString(_drawStageGUI,
                   CoordinateSpace.Screen,
                   "Window -> Screen -> World, within a viewport",
                   Colour.White,
                   24,
                   new Vector2(-460.0f, 200.0f),
                   TextJustify.Left,
                   0.4f,
                   2);

            var msgs = new List<string>
            {
                "Move Mouse Pointer",
                "Move Camera: Arrow Keys",
                "Zoom: Page Up/Down",
                "Rotate Camera: A/D",
            };

            var fontSize = 14;
            var spacing = 22;
            var yPos = 120.0f;

            msgs.ForEach(msg =>
            {
                yPos -= spacing;
                draw.DrawString(_drawStageGUI,
                 CoordinateSpace.Screen,
                 msg,
                 Colour.White,
                 fontSize,
                 new Vector2(-460.0f, yPos),
                 TextJustify.Left,
                 0.4f,
                 2);
            });
        }

        public override void Rendering(IRenderQueue q, IRenderTarget windowRenderTarget)
        {
            q.ClearColour(windowRenderTarget, Colour.Red);
            q.ClearDepth(windowRenderTarget);
            q.SetViewport(_viewport);
            q.Draw(_drawStageViewport, _cameraViewport, windowRenderTarget);
            q.RemoveViewport();
            q.Draw(_drawStageGUI, _cameraGUI, windowRenderTarget);
        }

        public override void Shutdown() { }
    }
}

using SampleBase;
using System.Collections.Generic;
using System.Numerics;
using Yak2D;

namespace CustomVeldrid_ComputeShaderExample
{
    /// <summary>
    /// An example that uses Veldrid.SPIRV to generate and run a compute shader version of Conway's Game of Life
    /// </summary>
    public class CustomVeldridComputeExample : ApplicationBase
    {
        private const int THREAD_GROUP_SIZE = 16;

        private const uint GRID_WIDTH = 64;  //Multiple of Threadgroup Size
        private const uint GRID_HEIGHT = 64; //Multiple of Threadgroup Size

        private const float FRAC_SCREEN_WIDTH_ON_MOVE = 0.1f;

        private uint gridWidth;
        private uint gridHeight;

        private GameOfLife _gameOfLife;
        private ICustomVeldridStage _customVeldridStage;
        private IRenderTarget _gameOfLifeRenderTarget;

        private IDrawStage _drawGrid;
        private IDrawStage _drawGui; //Has different blend state

        private ICamera2D _camera;
        private float _initZoomRequired;
        private float _zoom;
        private Vector2 _cameraFocus;

        public override string ReturnWindowTitle() => "Custom Veldrid Example - Conway's Game of Life Compute Shader";

        public override void OnStartup()
        {
            gridWidth = GRID_WIDTH;
            gridHeight = GRID_HEIGHT;

            if (gridWidth % THREAD_GROUP_SIZE != 0)
            {
                gridWidth = (gridWidth / THREAD_GROUP_SIZE) * THREAD_GROUP_SIZE;
                if (gridWidth == 0)
                {
                    gridWidth = THREAD_GROUP_SIZE;
                }
            }

            if (gridHeight % THREAD_GROUP_SIZE != 0)
            {
                gridHeight = (gridHeight / THREAD_GROUP_SIZE) * THREAD_GROUP_SIZE;
                if (gridHeight == 0)
                {
                    gridHeight = THREAD_GROUP_SIZE;
                }
            }
        }

        public override bool CreateResources(IServices yak)
        {
            _gameOfLife = new GameOfLife((int)gridWidth, (int)gridWidth, THREAD_GROUP_SIZE);
            _customVeldridStage = yak.Stages.CreateCustomVeldridStage(_gameOfLife);
            _gameOfLifeRenderTarget = yak.Surfaces.CreateRenderTarget(gridWidth, gridHeight, true, SamplerType.PointMagLinearMin);

            var initZoomRequiredWidth = 960.0f / (1.0f * gridWidth);
            var initZoomRequiredHeight = 540.0f / (1.0f * gridHeight);

            _initZoomRequired = initZoomRequiredWidth < initZoomRequiredHeight ? initZoomRequiredWidth : initZoomRequiredHeight;
            _zoom = _initZoomRequired;

            _cameraFocus = Vector2.Zero;
            _camera = yak.Cameras.CreateCamera2D(960, 540, _zoom);
            _drawGrid = yak.Stages.CreateDrawStage(true, BlendState.Override);
            _drawGui = yak.Stages.CreateDrawStage(true, BlendState.Alpha);

            return true;
        }

        public override bool Update_(IServices yak, float secondsSinceLastUpdate)
        {
            if (yak.Input.WasKeyReleasedThisFrame(KeyCode.P))
            {
                _gameOfLife.Paused = !_gameOfLife.Paused;
            }

            if (yak.Input.WasKeyReleasedThisFrame(KeyCode.C))
            {
                _gameOfLife.ClearFlag = true;
            }

            if (yak.Input.WasKeyReleasedThisFrame(KeyCode.R))
            {
                _zoom = _initZoomRequired;
                _cameraFocus = Vector2.Zero;
            }

            if (yak.Input.WasKeyReleasedThisFrame(KeyCode.A))
            {
                if (_gameOfLife.NumberFramesToWaitForUpdate == 0)
                {
                    _gameOfLife.NumberFramesToWaitForUpdate = 1;
                }
                else
                {
                    _gameOfLife.NumberFramesToWaitForUpdate *= 2;
                }
            }

            if (yak.Input.WasKeyReleasedThisFrame(KeyCode.Z))
            {
                if (_gameOfLife.NumberFramesToWaitForUpdate == 1)
                {
                    _gameOfLife.NumberFramesToWaitForUpdate = 0;
                }
                else
                {
                    _gameOfLife.NumberFramesToWaitForUpdate /= 2;
                }
            }

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
            var moveAmount = (FRAC_SCREEN_WIDTH_ON_MOVE * 960.0f) / _zoom;
            _cameraFocus += moveAmount * camMove;

            if (yak.Input.WasKeyReleasedThisFrame(KeyCode.PageUp))
            {
                _zoom *= 2.0f;
            }

            if (yak.Input.WasKeyReleasedThisFrame(KeyCode.PageDown))
            {
                _zoom /= 2.0f;
            }

            if (yak.Input.IsMouseCurrentlyPressed(MouseButton.Left) || yak.Input.IsMouseCurrentlyPressed(MouseButton.Right))
            {
                var transformed = yak.Helpers.CoordinateTransforms.WorldFromWindow(yak.Input.MousePosition, _camera);

                if(transformed.Contained)
                {
                    //Shouldn't not get here as no viewport being used, so all positions should be valid
                    var posWorld = transformed.Position;

                    var gridTopLeft = new Vector2(-0.5f * gridWidth, 0.5f * gridHeight);

                    var relGridTopLeft = posWorld - gridTopLeft;
                    relGridTopLeft.Y = -relGridTopLeft.Y;

                    var ix = (int)relGridTopLeft.X;
                    var iy = (int)relGridTopLeft.Y;

                    if (ix >= 0 && ix < gridWidth && iy >= 0 && iy < gridHeight)
                    {
                        if (yak.Input.IsMouseCurrentlyPressed(MouseButton.Left))
                        {
                            _gameOfLife.PointsToAdd.Add(new Veldrid.Point(ix, iy));
                        }

                        if (yak.Input.IsMouseCurrentlyPressed(MouseButton.Right))
                        {
                            _gameOfLife.PointsToRemove.Add(new Veldrid.Point(ix, iy));
                        }
                    }

                }
            }

            return true;
        }

        public override void PreDrawing(IServices yak, float secondsSinceLastDraw, float secondsSinceLastUpdate)
        {
            yak.Cameras.SetCamera2DFocusAndZoom(_camera, _cameraFocus, _zoom);
        }

        public override void Drawing(IDrawing draw,
                                     IFps fps,
                                     IInput input,
                                     ICoordinateTransforms transform,
                                     float secondsSinceLastDraw,
                                     float secondsSinceLastUpdate)
        {
            //Draw the Game Grid
            draw.Helpers.DrawTexturedQuad(_drawGrid,
                                          CoordinateSpace.World,
                                          _gameOfLifeRenderTarget,
                                          Colour.White,
                                          Vector2.Zero,
                                          gridWidth,
                                          gridHeight,
                                          0.9f,
                                          0);

            var helper = draw.Helpers;

            //Coloured side bar
            helper.DrawColouredQuad(_drawGui,
                                    CoordinateSpace.Screen,
                                    0.8f * Colour.LimeGreen,
                                    new Vector2(-350.0f, 0.0f),
                                    260.0f,
                                    540.0f,
                                    0.5f,
                                    0);

            //Draw some UI text
            draw.DrawString(_drawGui,
                               CoordinateSpace.Screen,
                               "Game of Life",
                               Colour.White,
                               34,
                               new Vector2(-460.0f, 250.0f),
                               TextJustify.Left,
                               0.4f,
                               0);


            //Controls Message UI
            draw.DrawString(_drawGui,
                               CoordinateSpace.Screen,
                               _gameOfLife.Paused ? "::PAUSED::" : "::RUNNING::",
                               Colour.White,
                               24,
                               new Vector2(-460.0f, 200.0f),
                               TextJustify.Left,
                               0.4f,
                               0);

            draw.DrawString(_drawGui,
                   CoordinateSpace.Screen,
                   "#Delay Frames: " + _gameOfLife.NumberFramesToWaitForUpdate,
                   Colour.White,
                   18,
                   new Vector2(-460.0f, 160.0f),
                   TextJustify.Left,
                   0.4f,
                   0);

            var msgs = new List<string>
            {
                "Add Point: Left Mouse",
                "Remove Point: Right Mouse",
                "Pause: 'P'",
                "Move Camera: Arrow Keys",
                "Zoom: Page Up/Down",
                "+Frames Per Update: 'A'",
                "-Frames Per Update: 'Z'",
                "Reset Camera: 'R'",
                "Clear Grid: 'C'"
            };

            var fontSize = 14;
            var spacing = 22;
            var yPos = 120.0f;

            msgs.ForEach(msg =>
            {
                yPos -= spacing;
                draw.DrawString(_drawGui,
                 CoordinateSpace.Screen,
                 msg,
                 Colour.White,
                 fontSize,
                 new Vector2(-460.0f, yPos),
                 TextJustify.Left,
                 0.4f,
                 0);
            });
        }

        public override void Rendering(IRenderQueue q, IRenderTarget windowRenderTarget)
        {
            q.ClearColour(windowRenderTarget, Colour.DarkSalmon);
            q.ClearDepth(windowRenderTarget);
            q.CustomVeldrid(_customVeldridStage, null, null, null, null, _gameOfLifeRenderTarget);
            q.Draw(_drawGrid, _camera, windowRenderTarget);
            q.Draw(_drawGui, _camera, windowRenderTarget);
        }

        public override void Shutdown() { }
    }
}
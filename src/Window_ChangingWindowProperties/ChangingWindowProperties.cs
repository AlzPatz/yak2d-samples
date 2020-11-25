using System.Collections.Generic;
using System.Numerics;
using Yak2D;
using SampleBase;

namespace Window_ChangingWindowProperties
{
    /// <summary>
    /// 
    /// </summary>
    public class ChangingWindowProperties : ApplicationBase
    {
        private bool _isViewportStale;
        private IViewport _viewport;
        private ICamera2D _cameraViewport;
        private ICamera2D _cameraGUI;
        private IDrawStage _drawStageViewport;
        private IDrawStage _drawStageGUI;

        public override string ReturnWindowTitle() => "Changing Window Properties";

        public override void OnStartup()
        {
            //In ApplicationBase of these samples, please note that a Framework Message is sent to the Application
            //When the swapchainbackbuffer is re-created. The Subscribed to event simply fires from this
            SwapChainFramebufferReCreated += SwapChainBackBufferReCreated;
        }

        private void SwapChainBackBufferReCreated(object sender, System.EventArgs e)
        {
            //Camera's use a concept of Virtual Resolution, we can therefore be relatively resolution agnostic
            //We do not need to change our camera when the window size changes

            //Viewports are not Resolution Agnostic, so any viewports being used should be re-created
            _isViewportStale = true;
        }

        public override bool CreateResources(IServices yak)
        {
            _cameraViewport = yak.Cameras.CreateCamera2D(1920, 1080);
            _cameraGUI = yak.Cameras.CreateCamera2D(960, 540);

            CreateViewport(yak);

            _drawStageViewport = yak.Stages.CreateDrawStage();
            _drawStageGUI = yak.Stages.CreateDrawStage();

            return true;
        }

        private void CreateViewport(IServices yak)
        {
            //The viewport should remain in the centre of the screen, with half dimensions of the window

            var windowSize = yak.Surfaces.GetSurfaceDimensions(yak.Surfaces.ReturnMainWindowRenderTarget());
            var viewportSize = 0.5f * windowSize;

            _viewport = yak.Stages.CreateViewport((uint)(0.25f * windowSize.Width), (uint)(0.25f * windowSize.Height), (uint)viewportSize.Width, (uint)viewportSize.Height);
        }

        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds)
        {
            var display = yak.Display;
            var input = yak.Input;

            var currentState = display.DisplayState;

            if (input.WasKeyReleasedThisFrame(KeyCode.F))
            {
                if (currentState != DisplayState.BorderlessFullScreen)
                {
                    display.SetDisplayState(DisplayState.BorderlessFullScreen);
                }
                else
                {
                    display.SetDisplayState(DisplayState.Normal);
                }
            }

            if (input.WasKeyReleasedThisFrame(KeyCode.N))
            {
                if (currentState != DisplayState.Minimised)
                {
                    display.SetDisplayState(DisplayState.Minimised);
                }
            }

            if (input.WasKeyReleasedThisFrame(KeyCode.M))
            {
                if (currentState != DisplayState.Maximised)
                {
                    display.SetDisplayState(DisplayState.Maximised);
                }
            }

            if (input.WasKeyReleasedThisFrame(KeyCode.T))
            {
                display.SetWindowTitle("Title Changed");
            }

            if (input.WasKeyReleasedThisFrame(KeyCode.O))
            {
                display.WindowOpacity = display.WindowOpacity == 1.0f ? 0.5f : 1.0f;
            }

            if (input.WasKeyReleasedThisFrame(KeyCode.C))
            {
                display.SetWindowResolution(128, 128);
            }

            if (input.WasKeyReleasedThisFrame(KeyCode.B))
            {
                display.WindowBorderVisible = !display.WindowBorderVisible;
            }

            //I don't think anyone should use hidden... it just sends the window off into the ether never to return :)
            //if (input.WasKeyReleasedThisFrame(KeyCode.H))
            //{
            //    if (currentState != DisplayState.Hidden)
            //    {
            //        display.SetDisplayState(DisplayState.Hidden);
            //    }
            //    else
            //    {
            //        display.SetDisplayState(DisplayState.Normal);
            //    }
            //}

            if (input.WasKeyReleasedThisFrame(KeyCode.H))
            {
                display.SetCursorVisible(false);
            }

            if (input.WasKeyReleasedThisFrame(KeyCode.S))
            {
                display.WindowResizable = ! display.WindowResizable;
            }

            if (input.WasKeyReleasedThisFrame(KeyCode.R))
            {
                display.WindowBorderVisible = true;
                display.SetWindowResolution(960, 540);
                display.SetWindowTitle("Changing Window Properties");
                display.SetCursorVisible(true);
                display.WindowOpacity = 1.0f;
                display.SetDisplayState(DisplayState.Normal);
            }

            return true;
        }

        public override void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            if (_isViewportStale)
            {
                CreateViewport(yak);
            }
        }

        public override void Drawing(IDrawing draw, IFps fps, IInput input, ICoordinateTransforms transform, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            //Draw in viewport

            draw.Helpers.DrawColouredQuad(_drawStageViewport, CoordinateSpace.Screen, Colour.Black, Vector2.Zero, 1920, 1080, 1.0f, 0);

            draw.Helpers.DrawColouredPoly(_drawStageViewport, CoordinateSpace.Screen, Colour.Red, Vector2.Zero, 64, 128.0f, 0.5f, 0);

            //draw GUI

            draw.DrawString(_drawStageGUI,
                CoordinateSpace.Screen,
                "Modifying Window Properties",
                Colour.Black,
                34,
                new Vector2(-460.0f, 250.0f),
                TextJustify.Left,
                0.4f,
                2);

            draw.DrawString(_drawStageGUI,
                   CoordinateSpace.Screen,
                   "Camera is Resolution Agnostic / Viewports described by absolute window positions",
                   Colour.Black,
                   24,
                   new Vector2(-460.0f, 200.0f),
                   TextJustify.Left,
                   0.4f,
                   2);

            var msgs = new List<string>
            {
                "Toggle Fullscreen: F",
                "Maximise: M",
                "Minimise: N",
                "Change Title Text: T",
                "Change Opacity: O",
                "Change to small window size: C",
                "Toggle Window Borders: B",
                "Toggle Window Resizable: S",
                "Reset Changes: R"
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
                 Colour.Black,
                 fontSize,
                 new Vector2(-460.0f, yPos),
                 TextJustify.Left,
                 0.4f,
                 2);
            });
        }

        public override void Rendering(IRenderQueue q, IRenderTarget windowRenderTarget)
        {
            q.ClearColour(windowRenderTarget, Colour.Pink);
            q.ClearDepth(windowRenderTarget);
            q.SetViewport(_viewport);
            q.Draw(_drawStageViewport, _cameraViewport, windowRenderTarget);
            q.RemoveViewport();
            q.Draw(_drawStageGUI, _cameraGUI, windowRenderTarget);
        }

        public override void Shutdown() { }
    }
}
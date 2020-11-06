using SampleBase;
using System.Numerics;
using Yak2D;

namespace StyleEffects_Static
{
    /// <summary>
    /// Static Examples
    /// </summary>
    public class StaticExample : ApplicationBase
    {
        private IRenderTarget _shapes;
        private ICamera2D _camera;
        private IDrawStage _drawStage;

        private IStyleEffectsStage _styleEffect0;
        private IStyleEffectsStage _styleEffect1;

        private IViewport _viewport0;
        private IViewport _viewport1;

        public override string ReturnWindowTitle() => "Style Effect: Edge Detection";

        public override void OnStartup() { }

        public override bool CreateResources(IServices yak)
        {
            _shapes = yak.Surfaces.CreateRenderTarget(480, 540);
            _camera = yak.Cameras.CreateCamera2D(480, 540);
            _drawStage = yak.Stages.CreateDrawStage();

            _viewport0 = yak.Stages.CreateViewport(60, 45, 360, 450);
            _viewport1 = yak.Stages.CreateViewport(540, 45, 360, 450);

            _styleEffect0 = yak.Stages.CreateStyleEffectsStage();
            _styleEffect1 = yak.Stages.CreateStyleEffectsStage();

            //Ignore Transparent
            yak.Stages.SetStyleEffectsStaticConfig(_styleEffect0, new StaticConfiguration
            {
                IgnoreTransparent = 1,
                Intensity = 0.6f,
                TexelScaler = 20.0f,
                TimeSpeed = 0.2f
            });

            //Over everything, opacity
            yak.Stages.SetStyleEffectsStaticConfig(_styleEffect1, new StaticConfiguration
            {
                IgnoreTransparent = 0,
                Intensity = 1.0f,
                TexelScaler = 1.0f,
                TimeSpeed = 10.0f
            });

            return true;
        }

        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds) => true;

        public override void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Drawing(IDrawing draw,
                                     IFps fps,
                                     IInput input,
                                     ICoordinateTransforms transforms,
                                     float timeSinceLastDrawSeconds,
                                     float timeSinceLastUpdateSeconds)
        {
            draw.Helpers.DrawColouredPoly(_drawStage,
                                                    CoordinateSpace.Screen,
                                                    Colour.Purple,
                                                    Vector2.Zero,
                                                    8,
                                                    100.0f,
                                                    0.5f);
        }

        public override void Rendering(IRenderQueue q, IRenderTarget windowRenderTarget)
        {
            q.ClearColour(windowRenderTarget, Colour.Clear);
            q.ClearDepth(windowRenderTarget);

            q.Draw(_drawStage, _camera, _shapes);

            q.SetViewport(_viewport0);
            q.StyleEffects(_styleEffect0, _shapes, windowRenderTarget);

            q.SetViewport(_viewport1);
            q.StyleEffects(_styleEffect1, _shapes, windowRenderTarget);

            q.RemoveViewport();
        }

        public override void Shutdown() { }
    }
}
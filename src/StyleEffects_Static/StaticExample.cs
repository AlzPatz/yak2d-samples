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

        public override bool CreateResources(IServices services)
        {
            _shapes = services.Surfaces.CreateRenderTarget(480, 540);
            _camera = services.Cameras.CreateCamera2D(480, 540);
            _drawStage = services.Stages.CreateDrawStage();

            _viewport0 = services.Stages.CreateViewport(60, 45, 360, 450);
            _viewport1 = services.Stages.CreateViewport(540, 45, 360, 450);

            _styleEffect0 = services.Stages.CreateStyleEffectsStage();
            _styleEffect1 = services.Stages.CreateStyleEffectsStage();

            //Ignore Transparent
            services.Stages.SetStyleEffectsStaticConfig(_styleEffect0, new StaticConfiguration
            {
                IgnoreTransparent = 1,
                Intensity = 0.6f,
                TexelScaler = 20.0f,
                TimeSpeed = 0.2f
            });

            //Over everything, opacity
            services.Stages.SetStyleEffectsStaticConfig(_styleEffect1, new StaticConfiguration
            {
                IgnoreTransparent = 0,
                Intensity = 1.0f,
                TexelScaler = 1.0f,
                TimeSpeed = 10.0f
            });

            return true;
        }

        public override bool Update_(IServices services, float timeSinceLastUpdateSeconds) => true;

        public override void PreDrawing(IServices services, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Drawing(IDrawing drawing, IFps fps, IInput input, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) 
        {
            drawing.DrawingHelpers.DrawColouredPoly(_drawStage,
                                                    CoordinateSpace.Screen,
                                                    Colour.Purple,
                                                    Vector2.Zero,
                                                    8,
                                                    100.0f,
                                                    0.5f);    
        }

        public override void Rendering(IRenderQueue queue)
        {
            queue.ClearColour(WindowRenderTarget, Colour.Clear);
            queue.ClearDepth(WindowRenderTarget);

            queue.Draw(_drawStage, _camera, _shapes);

            queue.SetViewport(_viewport0);
            queue.StyleEffects(_styleEffect0, _shapes, WindowRenderTarget);

            queue.SetViewport(_viewport1);
            queue.StyleEffects(_styleEffect1, _shapes, WindowRenderTarget);

            queue.RemoveViewport();
        }

        public override void Shutdown() { }
    }
}

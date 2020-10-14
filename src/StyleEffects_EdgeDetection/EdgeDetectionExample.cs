using SampleBase;
using Yak2D;

namespace StyleEffects_EdgeDetection
{
    /// <summary>
    /// 
    /// </summary>
    public class EdgeDetectionExample : ApplicationBase
    {
        private ITexture _textureNinja;
        private IStyleEffectsStage _styleEffect0;
        private IStyleEffectsStage _styleEffect1;
        private IViewport _viewport0;
        private IViewport _viewport1;

        public override string ReturnWindowTitle() => "Style Effect: Edge Detection";

        public override void OnStartup() { }

        public override bool CreateResources(IServices services)
        {
            _textureNinja = services.Surfaces.LoadTexture("ninja", AssetSourceEnum.Embedded);

            _viewport0 = services.Stages.CreateViewport(60, 45, 360, 450);
            _viewport1 = services.Stages.CreateViewport(540, 45, 360, 450);

            _styleEffect0 = services.Stages.CreateStyleEffectsStage();
            _styleEffect1 = services.Stages.CreateStyleEffectsStage();

            //Sobel
            services.Stages.SetStyleEffectsEdgeDetectionConfig(_styleEffect0, new EdgeDetectionConfiguration
            {
                Intensity = 1.0f,
                IsFreichen = false
            });

            //Freichen
            services.Stages.SetStyleEffectsEdgeDetectionConfig(_styleEffect1, new EdgeDetectionConfiguration
            {
                Intensity = 1.0f,
                IsFreichen = true
            });

            return true;
        }

        public override bool Update_(IServices services, float timeSinceLastUpdateSeconds) => true;

        public override void PreDrawing(IServices services, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Drawing(IDrawing drawing, IFps fps, IInput input, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }
        public override void Rendering(IRenderQueue queue)
        {
            queue.ClearColour(WindowRenderTarget, Colour.Clear);
            queue.ClearDepth(WindowRenderTarget);

            queue.SetViewport(_viewport0);
            queue.StyleEffects(_styleEffect0, _textureNinja, WindowRenderTarget);

            queue.SetViewport(_viewport1);
            queue.StyleEffects(_styleEffect1, _textureNinja, WindowRenderTarget);

            queue.RemoveViewport();
        }

        public override void Shutdown() { }
    }
}
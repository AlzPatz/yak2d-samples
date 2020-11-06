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

        public override bool CreateResources(IServices yak)
        {
            _textureNinja = yak.Surfaces.LoadTexture("ninja", AssetSourceEnum.Embedded);

            _viewport0 = yak.Stages.CreateViewport(60, 45, 360, 450);
            _viewport1 = yak.Stages.CreateViewport(540, 45, 360, 450);

            _styleEffect0 = yak.Stages.CreateStyleEffectsStage();
            _styleEffect1 = yak.Stages.CreateStyleEffectsStage();

            //Sobel
            yak.Stages.SetStyleEffectsEdgeDetectionConfig(_styleEffect0, new EdgeDetectionConfiguration
            {
                Intensity = 1.0f,
                IsFreichen = false
            });

            //Freichen
            yak.Stages.SetStyleEffectsEdgeDetectionConfig(_styleEffect1, new EdgeDetectionConfiguration
            {
                Intensity = 1.0f,
                IsFreichen = true
            });

            return true;
        }

        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds) => true;

        public override void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Drawing(IDrawing draw, IFps fps, IInput input, ICoordinateTransforms transforms, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }
        public override void Rendering(IRenderQueue q, IRenderTarget windowRenderTarget)
        {
            q.ClearColour(windowRenderTarget, Colour.Clear);
            q.ClearDepth(windowRenderTarget);

            q.SetViewport(_viewport0);
            q.StyleEffects(_styleEffect0, _textureNinja, windowRenderTarget);

            q.SetViewport(_viewport1);
            q.StyleEffects(_styleEffect1, _textureNinja, windowRenderTarget);

            q.RemoveViewport();
        }

        public override void Shutdown() { }
    }
}
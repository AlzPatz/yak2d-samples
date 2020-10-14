using SampleBase;
using System.Numerics;
using Yak2D;

namespace StyleEffects_CRT
{
    /// <summary>
    /// 2D CRT Effect Examples (scanlines etc)
    /// </summary>
    public class CrtEffectsExample : ApplicationBase
    {
        private ITexture _texture;

        private IStyleEffectsStage _styleEffect1;
        private IStyleEffectsStage _styleEffect2;
        private IStyleEffectsStage _styleEffect3;
        private IColourEffectsStage _colourEffect;

        private IViewport _viewport0;
        private IViewport _viewport1;
        private IViewport _viewport2;
        private IViewport _viewport3;

        public override string ReturnWindowTitle() => "Style Effect: 2D CRT (Top Left is No Effect)";

        public override void OnStartup() { }

        public override bool CreateResources(IServices services)
        {
            _texture = services.Surfaces.LoadTexture("platformer", AssetSourceEnum.Embedded);

            _viewport0 = services.Stages.CreateViewport(0, 0, 480, 270);
            _viewport1 = services.Stages.CreateViewport(480, 0, 480, 270);
            _viewport2 = services.Stages.CreateViewport(0, 270, 480, 270);
            _viewport3 = services.Stages.CreateViewport(480, 270, 480, 270);

            _styleEffect1 = services.Stages.CreateStyleEffectsStage();
            _styleEffect2 = services.Stages.CreateStyleEffectsStage();
            _styleEffect3 = services.Stages.CreateStyleEffectsStage();

            //RGB filters Only
            services.Stages.SetStyleEffectsCrtConfig(_styleEffect1, new CrtEffectConfiguration
            {
                NumRgbFiltersHorizontally = 96,
                NumRgbFiltersVertically = 54,
                RgbPixelFilterAmount = 0.75f,
                RgbPixelFilterIntensity = 0.5f,
                SimpleScanlinesIntensity = 0.0f
            });

            //Simple Scanlines
            services.Stages.SetStyleEffectsCrtConfig(_styleEffect2, new CrtEffectConfiguration
            {
                NumRgbFiltersHorizontally = 0,
                NumRgbFiltersVertically = 0,
                RgbPixelFilterAmount = 0.0f,
                RgbPixelFilterIntensity = 0.0f,
                SimpleScanlinesIntensity = 1.0f
            });

            //RGB filters and Simple Scanlines
            services.Stages.SetStyleEffectsCrtConfig(_styleEffect3, new CrtEffectConfiguration
            {
                NumRgbFiltersHorizontally = 96,
                NumRgbFiltersVertically = 54,
                RgbPixelFilterAmount = 0.75f,
                RgbPixelFilterIntensity = 0.5f,
                SimpleScanlinesIntensity = 1.0f
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
            queue.Copy(_texture, WindowRenderTarget);

            queue.SetViewport(_viewport1);
            queue.StyleEffects(_styleEffect1, _texture, WindowRenderTarget);

            queue.SetViewport(_viewport2);
            queue.StyleEffects(_styleEffect2, _texture, WindowRenderTarget);

            queue.SetViewport(_viewport3);
            queue.StyleEffects(_styleEffect3, _texture, WindowRenderTarget);

            queue.RemoveViewport();
        }

        public override void Shutdown() { }
    }
}
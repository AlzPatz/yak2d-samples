using SampleBase;
using System;
using Yak2D;

namespace StyleEffects_UsingConfigurationHelpers
{
    /// <summary>
    /// 
    /// </summary>
    public class ConfigurationHelperExample : ApplicationBase
    {
        private const float DURATION = 16.0f;
        private float _count = 0.0f;
        private float _fraction = 0.0f;

        private ITexture _texture;

        private IStyleEffectsStage _pixellate;
        private IStyleEffectsStage _edgeDetection;
        private IStyleEffectsStage _oldmovie;
        private IStyleEffectsStage _crt;

        private IViewport _viewport0;
        private IViewport _viewport1;
        private IViewport _viewport2;
        private IViewport _viewport3;

        public override string ReturnWindowTitle() => "Style Effect: Configuration Helpers";

        public override void OnStartup() { }

        public override bool CreateResources(IServices yak)
        {
            _texture = yak.Surfaces.LoadTexture("yak", AssetSourceEnum.Embedded);

            _viewport0 = yak.Stages.CreateViewport(0, 0, 480, 270);
            _viewport1 = yak.Stages.CreateViewport(480, 0, 480, 270);
            _viewport2 = yak.Stages.CreateViewport(0, 270, 480, 270);
            _viewport3 = yak.Stages.CreateViewport(480, 270, 480, 270);

            _pixellate = yak.Stages.CreateStyleEffectsStage();
            _edgeDetection = yak.Stages.CreateStyleEffectsStage();
            _oldmovie = yak.Stages.CreateStyleEffectsStage();
            _crt = yak.Stages.CreateStyleEffectsStage();

            yak.Stages.SetStyleEffectsPixellateConfig(_pixellate, PixellateConfiguration.PreSet(0.0f));
            yak.Stages.SetStyleEffectsEdgeDetectionConfig(_pixellate, EdgeDetectionConfiguration.PreSet(0.0f));
            yak.Stages.SetStyleEffectsOldMovieConfig(_pixellate, OldMovieConfiguration.PreSet(0.0f));
            yak.Stages.SetStyleEffectsCrtConfig(_pixellate, CrtEffectConfiguration.PreSet(0.0f, 960.0f / 540.0f));

            return true;
        }

        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds)
        {
            _count += timeSinceLastUpdateSeconds;

            while (_count > DURATION)
            {
                _count -= DURATION;
            }

            _fraction = _count / DURATION;

            return true;
        }

        public override void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            var amount = 0.5f * ((float)Math.Sin(_fraction * 2.0f * Math.PI) + 1.0f);

            yak.Stages.SetStyleEffectsPixellateConfig(_pixellate, PixellateConfiguration.PreSet(amount));
            yak.Stages.SetStyleEffectsEdgeDetectionConfig(_edgeDetection, EdgeDetectionConfiguration.PreSet(amount));
            yak.Stages.SetStyleEffectsOldMovieConfig(_oldmovie, OldMovieConfiguration.PreSet(amount));
            yak.Stages.SetStyleEffectsCrtConfig(_crt, CrtEffectConfiguration.PreSet(amount, 960.0f / 540.0f));
        }

        public override void Drawing(IDrawing draw,
                                     IFps fps,
                                     IInput input,
                                     ICoordinateTransforms transforms,
                                     float timeSinceLastDrawSeconds,
                                     float timeSinceLastUpdateSeconds)
        { }

        public override void Rendering(IRenderQueue q, IRenderTarget windowRenderTarget)
        {
            q.ClearColour(windowRenderTarget, Colour.Clear);
            q.ClearDepth(windowRenderTarget);

            q.SetViewport(_viewport0);
            q.StyleEffects(_pixellate, _texture, windowRenderTarget);

            q.SetViewport(_viewport1);
            q.StyleEffects(_edgeDetection, _texture, windowRenderTarget);

            q.SetViewport(_viewport2);
            q.StyleEffects(_crt, _texture, windowRenderTarget);

            q.SetViewport(_viewport3);
            q.StyleEffects(_oldmovie, _texture, windowRenderTarget);

            q.RemoveViewport(); //Not really needed at the end
        }

        public override void Shutdown() { }
    }
}
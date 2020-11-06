using SampleBase;
using System.Numerics;
using Yak2D;

namespace StyleEffects_OldMovie
{
    /// <summary>
    /// Example usage of the Old Movie Effect 
    /// </summary>
    public class OldMovieExample : ApplicationBase
    {
        private ITexture _texture;
        private IRenderTarget _offScreenTarget;

        private IStyleEffectsStage _styleEffect0;
        private IStyleEffectsStage _styleEffect1;
        private IStyleEffectsStage _styleEffect2;
        private IStyleEffectsStage _styleEffect3;
        private IColourEffectsStage _colourEffect;

        private IViewport _viewport0;
        private IViewport _viewport1;
        private IViewport _viewport2;
        private IViewport _viewport3;

        public override string ReturnWindowTitle() => "Style Effect: Old Movie";

        public override void OnStartup() { }

        public override bool CreateResources(IServices yak)
        {
            _texture = yak.Surfaces.LoadTexture("ghost-town", AssetSourceEnum.Embedded);
            _offScreenTarget = yak.Surfaces.CreateRenderTarget(540, 270);

            _viewport0 = yak.Stages.CreateViewport(0, 0, 480, 270);
            _viewport1 = yak.Stages.CreateViewport(480, 0, 480, 270);
            _viewport2 = yak.Stages.CreateViewport(0, 270, 480, 270);
            _viewport3 = yak.Stages.CreateViewport(480, 270, 480, 270);

            _styleEffect0 = yak.Stages.CreateStyleEffectsStage();
            _styleEffect1 = yak.Stages.CreateStyleEffectsStage();
            _styleEffect2 = yak.Stages.CreateStyleEffectsStage();
            _styleEffect3 = yak.Stages.CreateStyleEffectsStage();


            //Using Default Config Helper
            yak.Stages.SetStyleEffectsOldMovieConfig(_styleEffect0, OldMovieConfiguration.GenerateDefault());

            //Intense
            yak.Stages.SetStyleEffectsOldMovieConfig(_styleEffect1, new OldMovieConfiguration
            {
                Intensity = 1.0f,
                Scratch = 0.04f,
                Noise = 0.3f,
                RndShiftCutOff = 1.0f,
                RndShiftScalar = 0.3f,
                Dim = 0.8f,

                ProbabilityRollStarts = 0.05f,
                ProbabilityRollEnds = 0.08f,
                RollSpeedMin = 3.92f,
                RollSpeedMax = 5.62f,
                RollAccelerationMin = 2.8f,
                RollAccelerationMax = 4.8f,
                RollShakeFactor = 0.9f,
                RollOverallScale = 0.8f,

                OverExposureProbabilityStart = 0.07f,
                OverExposureFlickerTimeMin = 19.4f,
                OverExposureFlickerTimeMax = 64.0f,
                OverExposureIntensityMin = 3.7f,
                OverExposureIntensityMax = 9.4f,
                OverExposureOscillationsMin = 4,
                OverExposureOscillationsMax = 12
            });

            //Light, no roll, no overexposure
            yak.Stages.SetStyleEffectsOldMovieConfig(_styleEffect2, new OldMovieConfiguration
            {
                Intensity = 0.6f,
                Scratch = 0.05f,
                Noise = 0.2f,
                RndShiftCutOff = 0.0f,
                RndShiftScalar = 0.0f,
                Dim = 0.0f,

                ProbabilityRollStarts = 0.0f,
                ProbabilityRollEnds = 1.0f,
                RollSpeedMin = 0.0f,
                RollSpeedMax = 0.0f,
                RollAccelerationMin = 0.0f,
                RollAccelerationMax = 0.0f,
                RollShakeFactor = 0.0f,
                RollOverallScale = 0.0f,

                OverExposureProbabilityStart = 0.0f,
                OverExposureFlickerTimeMin = 0.0f,
                OverExposureFlickerTimeMax = 0.0f,
                OverExposureIntensityMin = 0.0f,
                OverExposureIntensityMax = 0.0f,
                OverExposureOscillationsMin = 0,
                OverExposureOscillationsMax = 0
            });

            //Mixing in a Gray Scale effect
            _colourEffect = yak.Stages.CreateColourEffectsStage();
            yak.Stages.SetColourEffectsConfig(_colourEffect, new ColourEffectConfiguration
            {
                ClearBackground = false,
                GrayScale = 1.0f,
                BackgroundClearColour = Colour.Clear,
                ColourForSingleColourAndColourise = Colour.Clear,
                Colourise = 0.0f,
                Negative = 0.0f,
                Opacity = 1.0f,
                SingleColour = 0.0f
            });
            //Use default
            yak.Stages.SetStyleEffectsOldMovieConfig(_styleEffect3, OldMovieConfiguration.GenerateDefault());

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
            q.StyleEffects(_styleEffect0, _texture, windowRenderTarget);

            q.SetViewport(_viewport1);
            q.StyleEffects(_styleEffect1, _texture, windowRenderTarget);

            q.SetViewport(_viewport2);
            q.StyleEffects(_styleEffect2, _texture, windowRenderTarget);

            q.RemoveViewport();

            q.ColourEffects(_colourEffect, _texture, _offScreenTarget);
            q.SetViewport(_viewport3);
            q.StyleEffects(_styleEffect3, _offScreenTarget, windowRenderTarget);

            q.RemoveViewport();
        }

        public override void Shutdown() { }
    }
}
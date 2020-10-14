using SampleBase;
using System;
using Yak2D;

namespace ColourEffects_Example
{
    /// <summary>
    /// Examples of Simple Colour Effects, Transitions (and viewports!)
    /// No drawstage is used to draw texture, just effect stage applied using a texture source into different regions (viewports) of the main window render target
    /// </summary>
    public class ColourEffectsExample : ApplicationBase
    {
        private ITexture _texture;

        private IColourEffectsStage _effect_SingleColour;
        private IColourEffectsStage _effect_GrayScale;
        private IColourEffectsStage _effect_Colourise;
        private IColourEffectsStage _effect_Negative;
        private IColourEffectsStage _effect_Opacity;
        private IColourEffectsStage _effect_VariableCombination;
        private IViewport[] _viewports;

        private const float DURATION = 1.0f;
        private float _count = 0.0f;
        private bool _setARandomConfig = true;

        public override string ReturnWindowTitle() => "Colour Effects: SingleColourMix, Colourise, Grayscale, Negative, Opacity, Random Transitioning";

        public override void OnStartup() { }

        public override bool CreateResources(IServices services)
        {
            _texture = services.Surfaces.LoadTexture("yak", AssetSourceEnum.Embedded);

            //We use a different effect stage for each effect as you cannot modify an effects stage property within a single draw/render frame/queue of commands
            _effect_SingleColour = services.Stages.CreateColourEffectsStage();
            _effect_GrayScale = services.Stages.CreateColourEffectsStage();
            _effect_Colourise = services.Stages.CreateColourEffectsStage();
            _effect_Negative = services.Stages.CreateColourEffectsStage();
            _effect_Opacity = services.Stages.CreateColourEffectsStage();
            _effect_VariableCombination = services.Stages.CreateColourEffectsStage();

            //Viewports are given in absolute pixel positions and sizes with origin at top left
            //This coordinate system is different to that use in the rest of the framework
            _viewports = new IViewport[]
            {
                services.Stages.CreateViewport(0, 0, 320, 270),
                services.Stages.CreateViewport(320, 0, 320, 270),
                services.Stages.CreateViewport(640, 0, 320, 270),
                services.Stages.CreateViewport(0, 270, 320, 270),
                services.Stages.CreateViewport(320, 270, 320, 270),
                services.Stages.CreateViewport(640, 270, 320, 270),
            };

            SetInitialEffectConfigurations(services);

            return true;
        }

        private void SetInitialEffectConfigurations(IServices services)
        {
            //Work off a base "non effect" configuration, copy and modify the struct for each effect config later
            var defaultConfig = new ColourEffectConfiguration
            {
                BackgroundClearColour = Colour.Clear,
                ColourForSingleColourAndColourise = Colour.White,
                ClearBackground = false,
                SingleColour = 0.0f,
                Colourise = 0.0f,
                GrayScale = 0.0f,
                Negative = 0.0f,
                Opacity = 1.0f
            };

            var singleColourConfig = defaultConfig;
            singleColourConfig.SingleColour = 0.5f;
            singleColourConfig.ColourForSingleColourAndColourise = Colour.HotPink;
            services.Stages.SetColourEffectsConfig(_effect_SingleColour, singleColourConfig);

            var colouriseConfig = defaultConfig;
            colouriseConfig.Colourise = 1.0f;
            colouriseConfig.ColourForSingleColourAndColourise = Colour.HotPink;
            services.Stages.SetColourEffectsConfig(_effect_Colourise, colouriseConfig);

            var grayScaleConfig = defaultConfig;
            grayScaleConfig.GrayScale = 1.0f;
            services.Stages.SetColourEffectsConfig(_effect_GrayScale, grayScaleConfig);

            var negativeConfig = defaultConfig;
            negativeConfig.Negative = 1.0f;
            services.Stages.SetColourEffectsConfig(_effect_Negative, negativeConfig);

            var opacityConfig = defaultConfig;
            opacityConfig.Opacity = 0.3f;
            services.Stages.SetColourEffectsConfig(_effect_Opacity, opacityConfig);

            //The variable / transition effect, just set to defaults to behin with 
            services.Stages.SetColourEffectsConfig(_effect_VariableCombination, defaultConfig);
        }

        public override bool Update_(IServices services, float timeSinceLastUpdateSeconds)
        {
            //Trigger a boolean switch for a new config every DURATION seconds
            _count += timeSinceLastUpdateSeconds;

            while (_count > DURATION)
            {
                _count -= DURATION;
                _setARandomConfig = true;
            }

            return true;
        }

        public override void PreDrawing(IServices services, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            //Update the varying / transitioning effect combination stage
            if (_setARandomConfig)
            {
                _setARandomConfig = false;

                var rnd = new Random();

                var config = new ColourEffectConfiguration
                {
                    BackgroundClearColour = Colour.Clear,
                    ColourForSingleColourAndColourise = new Colour((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble()),
                    ClearBackground = false, //Clears who texture not just viewport area so not suitable here
                    Colourise = (float)rnd.NextDouble(),
                    GrayScale = (float)rnd.NextDouble(),
                    Negative = (float)rnd.NextDouble(),
                    Opacity = 0.5f + (0.5f * (float)rnd.NextDouble()),
                    SingleColour = 0.5f * (float)rnd.NextDouble(),
                };

                services.Stages.SetColourEffectsConfig(_effect_VariableCombination, config, DURATION);
            }
        }

        public override void Drawing(IDrawing drawing, IFps fps, IInput input, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }


        public override void Rendering(IRenderQueue queue)
        {
            queue.ClearColour(WindowRenderTarget, Colour.Clear);
            queue.ClearDepth(WindowRenderTarget);

            queue.SetViewport(_viewports[0]);
            queue.ColourEffects(_effect_SingleColour, _texture, WindowRenderTarget);

            queue.SetViewport(_viewports[1]);
            queue.ColourEffects(_effect_Colourise, _texture, WindowRenderTarget);

            queue.SetViewport(_viewports[2]);
            queue.ColourEffects(_effect_GrayScale, _texture, WindowRenderTarget);

            queue.SetViewport(_viewports[3]);
            queue.ColourEffects(_effect_Negative, _texture, WindowRenderTarget);

            queue.SetViewport(_viewports[4]);
            queue.ColourEffects(_effect_Opacity, _texture, WindowRenderTarget);

            queue.SetViewport(_viewports[5]);
            queue.ColourEffects(_effect_VariableCombination, _texture, WindowRenderTarget);

            queue.RemoveViewport(); //Uneeded here, as viewports will clear before a new frame starts, but included to show how to go back to rendering to entire surfaces within a queue if required
        }

        public override void Shutdown() { }
    }
}
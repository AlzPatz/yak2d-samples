using System;
using Yak2D;
using SampleBase;
using System.Numerics;

namespace Mix_SimpleWholeTextureMixingWithFactors
{
    /// <summary>
    /// Example of simple mixing of textures based on overall factors
    /// Also makes sure of texture pixel data load function to enable a smaller textures to be made form larger
    /// </summary>
    public class SimpleMixing : ApplicationBase
    {
        private const float DURATION = 10.0f;
        private float _timecount = 0.0f;

        private ITexture[] _textures;

        private IMixStage _mixStage;
        private IViewport _viewport;

        public override string ReturnWindowTitle() => "Mixing Example - Simple Whole Texture Mixing Factors";

        public override void OnStartup() { }

        public override bool CreateResources(IServices services)
        {
            _viewport = services.Stages.CreateViewport(300, 0, 360, 540);

            //We want to cut the texture up, so we load the colour data instead of generating an ITexture

            var texData = services.Surfaces.LoadTextureColourData("seasons", AssetSourceEnum.Embedded);

            var subTexWidth = texData.Width / 4;

            var subPixels = new Vector4[4][];

            for (var t = 0; t < 4; t++)
            {
                subPixels[t] = new Vector4[subTexWidth * texData.Height];
            }

            for (var y = 0; y < texData.Height; y++)
            {
                for (var x = 0; x < texData.Width; x++)
                {
                    var nSub = x / subTexWidth;
                    var xSub = x % subTexWidth;

                    var subIndex = (y * subTexWidth) + xSub;
                    var index = (y * texData.Width) + x;

                    subPixels[nSub][subIndex] = texData.Pixels[index];
                }
            }

            _textures = new ITexture[4];

            for (var t = 0; t < 4; t++)
            {
                _textures[t] = services.Surfaces.CreateRgbaFromData(subTexWidth, texData.Height, subPixels[t]);
            }

            _mixStage = services.Stages.CreateMixStage();

            return true;
        }

        public override bool Update_(IServices services, float timeSinceLastUpdateSeconds) => true;

        public override void PreDrawing(IServices services, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            //Set the fractional mixing for the textures (just loop through them)

            _timecount += timeSinceLastDrawSeconds;

            while (_timecount > DURATION)
            {
                _timecount -= DURATION;
            }

            var fraction = _timecount / DURATION;

            var scaled = 4.0f * fraction;

            var vals = new float[4];
            for (var n = 0; n < 4; n++)
            {
                var frac = scaled;

                if (n == 0 && frac > 3.0f)
                {
                    frac -= 4.0f;
                }

                var delta = (float)Math.Abs((float)n - frac);

                if (delta > 1.0f)
                {
                    delta = 1.0f;
                }

                vals[n] = 1.0f - delta;
            }

            services.Stages.SetMixStageProperties(_mixStage, new Vector4(vals[0], vals[1], vals[2], vals[3]));
        }

        public override void Drawing(IDrawing drawing, IFps fps, IInput input, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Rendering(IRenderQueue queue)
        {
            queue.ClearColour(WindowRenderTarget, Colour.Clear);
            queue.ClearDepth(WindowRenderTarget);
            queue.SetViewport(_viewport);
            queue.Mix(_mixStage, null, _textures[0], _textures[1], _textures[2], _textures[3], WindowRenderTarget);
            queue.RemoveViewport();
        }

        public override void Shutdown() { }
    }
}

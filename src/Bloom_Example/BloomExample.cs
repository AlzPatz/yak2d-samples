using SampleBase;
using System;
using System.Numerics;
using Yak2D;

namespace Bloom_Example
{
    /// <summary>
    /// Simple Bloom Effect
    /// </summary>
    public class BloomExample : ApplicationBase
    {
        private IDrawStage _drawStage;
        private ICamera2D _camera;
        private IRenderTarget _renderTarget;
        private ITexture _texture;
        private IBloomStage _bloomStage;

        private const float DURATION = 4.0f;
        private float _count = 0.0f;
        private float _fraction = 0.0f;

        public override string ReturnWindowTitle() => "Simple Bloom Example";

        public override void OnStartup() { }

        public override bool CreateResources(IServices services)
        {
            _drawStage = services.Stages.CreateDrawStage();

            _camera = services.Cameras.CreateCamera2D(960, 540, 1.0f);

            _renderTarget = services.Surfaces.CreateRenderTarget(960, 540);

            _texture = services.Surfaces.LoadTexture("hongkong", AssetSourceEnum.Embedded);

            _bloomStage = services.Stages.CreateBloomStage(240, 135); // The smaller the internal intermediate blur surface, the lower quality the but the broader and faster the effect

            return true;
        }

        public override bool Update_(IServices services, float timeSinceLastUpdateSeconds)
        {
            //Generate a repeating 0 to 1 fraction loop

            _count += timeSinceLastUpdateSeconds;

            while (_count > DURATION)
            {
                _count -= DURATION;
            }

            _fraction = _count / DURATION;

            return true;
        }

        public override void PreDrawing(IServices services, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            services.Stages.SetBloomConfig(_bloomStage, new BloomEffectConfiguration
            {
                AdditiveMixAmount = ((float)Math.Sin(_fraction * 2.0f * Math.PI) + 1.0f) * 0.5f,
                BrightnessThreshold = 0.5f,
                NumberOfBlurSamples = 8,
                ReSamplerType = ResizeSamplerType.Average4x4
            });
        }

        public override void Drawing(IDrawing drawing, IFps fps, IInput input, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            drawing.DrawingHelpers.DrawTexturedQuad(_drawStage, CoordinateSpace.Screen, _texture, Colour.White, Vector2.Zero, 960, 540, 0.5f);
        }

        public override void Rendering(IRenderQueue queue)
        {
            queue.ClearColour(WindowRenderTarget, Colour.Clear);
            queue.ClearDepth(WindowRenderTarget);
            queue.Draw(_drawStage, _camera, _renderTarget);
            queue.Bloom(_bloomStage, _renderTarget, WindowRenderTarget);
        }

        public override void Shutdown() { }
    }
}
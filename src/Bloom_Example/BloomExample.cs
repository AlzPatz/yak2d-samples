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

        public override bool CreateResources(IServices yak)
        {
            _drawStage = yak.Stages.CreateDrawStage();

            _camera = yak.Cameras.CreateCamera2D(960, 540, 1.0f);

            _renderTarget = yak.Surfaces.CreateRenderTarget(960, 540);

            _texture = yak.Surfaces.LoadTexture("hongkong", AssetSourceEnum.Embedded);

            _bloomStage = yak.Stages.CreateBloomStage(240, 135); // The smaller the internal intermediate blur surface, the lower quality the but the broader and faster the effect

            return true;
        }

        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds)
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

        public override void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            yak.Stages.SetBloomConfig(_bloomStage, new BloomEffectConfiguration
            {
                AdditiveMixAmount = ((float)Math.Sin(_fraction * 2.0f * Math.PI) + 1.0f) * 0.5f,
                BrightnessThreshold = 0.5f,
                NumberOfBlurSamples = 8,
                ReSamplerType = ResizeSamplerType.Average4x4
            });
        }

        public override void Drawing(IDrawing draw,
                                     IFps fps,
                                     IInput input,
                                     ICoordinateTransforms transform,
                                     float timeSinceLastDrawSeconds,
                                     float timeSinceLastUpdateSeconds)
        {
            draw.Helpers.DrawTexturedQuad(_drawStage, CoordinateSpace.Screen, _texture, Colour.White, Vector2.Zero, 960, 540, 0.5f);
        }

        public override void Rendering(IRenderQueue q, IRenderTarget windowRenderTarget)
        {
            q.ClearColour(windowRenderTarget, Colour.Clear);
            q.ClearDepth(windowRenderTarget);
            q.Draw(_drawStage, _camera, _renderTarget);
            q.Bloom(_bloomStage, _renderTarget, windowRenderTarget);
        }

        public override void Shutdown() { }
    }
}
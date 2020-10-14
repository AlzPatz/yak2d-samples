using SampleBase;
using System;
using System.Numerics;
using Yak2D;

namespace Blur_Example
{
    /// <summary>
    /// Simple Blur Effect (2D, non-directional blur)
    /// </summary>
    public class BlurExample : ApplicationBase
    {
        private IDrawStage _drawStage;
        private ICamera2D _camera;
        private IRenderTarget _renderTarget;
        private ITexture _texture;
        private IBlurStage _blurStage;

        private const float DURATION = 4.0f;
        private float _count = 0.0f;
        private float _fraction = 0.0f;

        public override string ReturnWindowTitle() => "Simple Blur (2D) Example";

        public override void OnStartup() { }

        public override bool CreateResources(IServices services)
        {
            _drawStage = services.Stages.CreateDrawStage();

            _camera = services.Cameras.CreateCamera2D(960, 540, 1.0f);

            _renderTarget = services.Surfaces.CreateRenderTarget(960, 540);

            _texture = services.Surfaces.LoadTexture("camera", AssetSourceEnum.Embedded);

            _blurStage = services.Stages.CreateBlurStage(240, 135); // The smaller the internal intermediate blur surface, the lower quality the blur but the broader the blur spread (and faster the render)

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
            services.Stages.SetBlurConfig(_blurStage, new BlurEffectConfiguration
            {
                MixAmount = ((float)Math.Sin(_fraction * 2.0f * Math.PI) + 1.0f) * 0.5f,
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
            queue.Blur(_blurStage, _renderTarget, WindowRenderTarget);
        }

        public override void Shutdown() { }
    }
}
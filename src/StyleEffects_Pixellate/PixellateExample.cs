using SampleBase;
using System.Numerics;
using Yak2D;

namespace StyleEffects_Pixellate
{
    /// <summary>
    /// 
    /// </summary>
    public class PixellateExample : ApplicationBase
    {
        private ITexture _textureNinja;
        private IStyleEffectsStage _styleEffect;
        private IViewport _viewport;

        //private IRenderTarget _target;
        //private IDrawStage _drawStage;
        //private ICamera2D _camera

        public override string ReturnWindowTitle() => "Style Effect: Pixellate";

        public override void OnStartup() { }

        public override bool CreateResources(IServices yak)
        {
            _textureNinja = yak.Surfaces.LoadTexture("ninja", AssetSourceEnum.Embedded, ImageFormat.PNG, SamplerType.Point); //Use point sampling to avoid artifacts related to interpolation and mipmaps

            _viewport = yak.Stages.CreateViewport(480 - 253, 270 - 260, 506, 520);

            _styleEffect = yak.Stages.CreateStyleEffectsStage();

            yak.Stages.SetStyleEffectsPixellateConfig(_styleEffect, new PixellateConfiguration
            {
                Intensity = 1.0f,
                NumXDivisions = 32,
                NumYDivisions = 32
            });

            //_target = yak.Surfaces.CreateRenderTarget(960, 540);
            //_drawStage = yak.Stages.CreateDrawStage();
            //_camera = yak.Cameras.CreateCamera2D();

            return true;
        }

        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds) => true;

        public override void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Drawing(IDrawing drawing,
                                     IFps fps,
                                     IInput input,
                                     ICoordinateTransforms transforms,
                                     float timeSinceLastDrawSeconds,
                                     float timeSinceLastUpdateSeconds)
        {
            //drawing.DrawingHelpers.DrawTexturedQuad(_drawStage, CoordinateSpace.Screen, _textureNinja, Colour.White, Vector2.Zero, 506, 520, 0.5f);
        }

        public override void Rendering(IRenderQueue q, IRenderTarget windowRenderTarget)
        {
            q.ClearColour(windowRenderTarget, Colour.Clear);
            q.ClearDepth(windowRenderTarget);

            //queue.Draw(_drawStage, _camera, _target);

            q.SetViewport(_viewport);
            q.StyleEffects(_styleEffect, _textureNinja, windowRenderTarget);
            q.RemoveViewport();
        }

        public override void Shutdown() { }
    }
}
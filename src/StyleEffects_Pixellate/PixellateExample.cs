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

        public override bool CreateResources(IServices services)
        {
            _textureNinja = services.Surfaces.LoadTexture("ninja", AssetSourceEnum.Embedded);

            _viewport = services.Stages.CreateViewport(480 - 253, 270 - 260, 506, 520);

            _styleEffect = services.Stages.CreateStyleEffectsStage();

            services.Stages.SetStyleEffectsPixellateConfig(_styleEffect, new PixellateConfiguration
            {
                Intensity = 1.0f,
                NumXDivisions = 32,
                NumYDivisions = 32
            });

            //_target = services.Surfaces.CreateRenderTarget(960, 540);
            //_drawStage = services.Stages.CreateDrawStage();
            //_camera = services.Cameras.CreateCamera2D();

            return true;
        }

        public override bool Update_(IServices services, float timeSinceLastUpdateSeconds) => true;

        public override void PreDrawing(IServices services, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Drawing(IDrawing drawing, IFps fps, IInput input, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            //drawing.DrawingHelpers.DrawTexturedQuad(_drawStage, CoordinateSpace.Screen, _textureNinja, Colour.White, Vector2.Zero, 506, 520, 0.5f);
        }

        public override void Rendering(IRenderQueue queue)
        {
            queue.ClearColour(WindowRenderTarget, Colour.Clear);
            queue.ClearDepth(WindowRenderTarget);

            //queue.Draw(_drawStage, _camera, _target);

            queue.SetViewport(_viewport);
            queue.StyleEffects(_styleEffect, _textureNinja, WindowRenderTarget);
            queue.RemoveViewport();
        }

        public override void Shutdown() { }
    }
}
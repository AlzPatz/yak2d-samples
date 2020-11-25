using SampleBase;
using System.Numerics;
using Yak2D;

namespace Temp
{
    public class TempExample : ApplicationBase
    {
        private ITexture _createdTexture;
        private ITexture _copiedTexture;
        private ISurfaceCopyStage _transfer;

        private bool _copied = false;

        public override string ReturnWindowTitle() => "Copy Example - Full Surface to Render Target Blit";

        public override void OnStartup() { }

        public override bool CreateResources(IServices yak)
        {
            var pixelData = new Vector4[]
            {
                            new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                            new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                            new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                            new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
            };


            _createdTexture = yak.Surfaces.CreateRgbaFromData(2, 2, pixelData, SamplerType.Point, false);

            _transfer = yak.Stages.CreateSurfaceCopyDataStage(2, 2, (data) =>
            {
                _copiedTexture = yak.Surfaces.CreateRgbaFromData(2, 2, data.Pixels, SamplerType.Point, false);
                _copied = true;
            });

            return true;
        }
        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds) => true;

        public override void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Drawing(IDrawing draw, IFps fps, IInput input, ICoordinateTransforms transform, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Rendering(IRenderQueue q, IRenderTarget windowRenderTarget)
        {
            q.ClearColour(windowRenderTarget, Colour.Pink);
            q.ClearDepth(windowRenderTarget);

            if (!_copied)
            {
                q.CopySurfaceData(_transfer, _createdTexture);
            }
            else
            {
                q.Copy(_copiedTexture, windowRenderTarget);
            }
        }

        public override void Shutdown() { }
    }
}
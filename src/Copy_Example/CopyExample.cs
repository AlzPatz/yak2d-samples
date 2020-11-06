using SampleBase;
using Yak2D;

namespace Copy_Example
{
    /// <summary>
    /// Surface Copy Example - Just copies one surface to any render target. Source and Target surface size agnostic
    /// </summary>
    public class CopyExample : ApplicationBase
    {
        private ITexture _texture;

        public override string ReturnWindowTitle() => "Copy Example - Full Surface to Render Target Blit";

        public override void OnStartup() { }

        public override bool CreateResources(IServices yak)
        {
            _texture = yak.Surfaces.LoadTexture("yak", AssetSourceEnum.Embedded);

            return true;
        }
        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds) => true;

        public override void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Drawing(IDrawing draw, IFps fps, IInput input, ICoordinateTransforms transform, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Rendering(IRenderQueue q, IRenderTarget windowRenderTarget)
        {
            q.ClearColour(windowRenderTarget, Colour.Clear);
            q.ClearDepth(windowRenderTarget);

            q.Copy(_texture, windowRenderTarget);
        }

        public override void Shutdown() { }
    }
}
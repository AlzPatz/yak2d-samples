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

        public override bool CreateResources(IServices services)
        {
            _texture = services.Surfaces.LoadTexture("yak", AssetSourceEnum.Embedded);

            return true;
        }
        public override bool Update_(IServices services, float timeSinceLastUpdateSeconds) => true;

        public override void PreDrawing(IServices services, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Drawing(IDrawing drawing, IFps fps, IInput input, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Rendering(IRenderQueue queue)
        {
            queue.ClearColour(WindowRenderTarget, Colour.Clear);
            queue.ClearDepth(WindowRenderTarget);

            queue.Copy(_texture, WindowRenderTarget);
        }

        public override void Shutdown() { }
    }
}
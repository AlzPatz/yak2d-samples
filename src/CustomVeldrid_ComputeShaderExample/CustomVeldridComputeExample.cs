using SampleBase;
using Yak2D;

namespace CustomVeldrid_ComputeShaderExample
{
    public class CustomVeldridComputeExample : ApplicationBase
    {
        private const int GRID_WIDTH = 256;
        private const int GRID_HEIGHT = 256;

        private GameOfLife _gameOfLife;
        private ICustomVeldridStage _customVeldridStage;
        private IRenderTarget _gameOfLifeRenderTarget;
        private IViewport _viewport;

        public override string ReturnWindowTitle() => "Custom Veldrid Example";

        public override void OnStartup() { }

        public override bool CreateResources(IServices services)
        {
            _gameOfLife = new GameOfLife(GRID_WIDTH, GRID_WIDTH);
            _customVeldridStage = services.Stages.CreateCustomVeldridStage(_gameOfLife);
            _gameOfLifeRenderTarget = services.Surfaces.CreateRenderTarget(GRID_WIDTH, GRID_HEIGHT);

            var aspect = 540.0f / (1.0f * GRID_HEIGHT);
            var width = (int)(GRID_WIDTH * aspect);

            _viewport = services.Stages.CreateViewport((uint)(480 - (0.5f * width)), 0, (uint)width, 540);

            return true;
        }

        public override bool Update_(IServices services, float timeSinceLastUpdateSeconds) => true;

        public override void PreDrawing(IServices services, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Drawing(IDrawing drawing, IFps fps, IInput input, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Rendering(IRenderQueue queue)
        {
            queue.ClearColour(WindowRenderTarget, Colour.Pink);
            queue.ClearDepth(WindowRenderTarget);
            queue.CustomVeldrid(_customVeldridStage, null, null, null, null, _gameOfLifeRenderTarget);
            queue.SetViewport(_viewport);
            queue.Copy(_gameOfLifeRenderTarget, WindowRenderTarget);
            queue.RemoveViewport(); //Not really needed at end of queue
        }

        public override void Shutdown() { }
    }
}

using SampleBase;
using System.Numerics;
using Yak2D;

namespace Draw_BasicPolygonHelperFunctions
{
    /// <summary>
    /// Drawing Regular Polygons using Helper Functions
    /// </summary>
    public class DrawUsingHelperFunctions : ApplicationBase
    {
        private IDrawStage _drawStage;
        private ICamera2D _camera;
        private ITexture _textureCity;
        private ITexture _textureGrass;
        private ITexture _textureMud;
        private ITexture _textureWall;

        public override string ReturnWindowTitle() => "Drawing using Helper Functions";

        public override void OnStartup() { }

        public override bool CreateResources(IServices services)
        {
            _drawStage = services.Stages.CreateDrawStage();

            _camera = services.Cameras.CreateCamera2D(960, 540, 1.0f);

            _textureCity = services.Surfaces.LoadTexture("city", AssetSourceEnum.Embedded);
            _textureGrass = services.Surfaces.LoadTexture("grass", AssetSourceEnum.Embedded);
            _textureMud = services.Surfaces.LoadTexture("mudrock", AssetSourceEnum.Embedded);
            _textureWall = services.Surfaces.LoadTexture("stonewall", AssetSourceEnum.Embedded);

            return true;
        }

        public override bool Update_(IServices services, float timeSinceLastUpdateSeconds) => true;
        public override void PreDrawing(IServices services, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Drawing(IDrawing drawing, IFps fps, IInput input, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            var helper = drawing.DrawingHelpers;

            helper.DrawColouredQuad(_drawStage, CoordinateSpace.Screen, Colour.Aqua, new Vector2(-300.0f, 150.0f), 200.0f, 180.0f, 0.9f);
            helper.DrawTexturedQuad(_drawStage, CoordinateSpace.Screen, _textureWall, Colour.White, new Vector2(-150.0f, 50.0f), 220.0f, 160.0f, 0.7f);
            helper.DrawColouredPoly(_drawStage, CoordinateSpace.Screen, Colour.LimeGreen, new Vector2(0.0f, -50.0f), 8, 120.0f, 0.3f);
            helper.DrawLine(_drawStage, CoordinateSpace.Screen, new Vector2(0.0f, -80.0f), new Vector2(200.0f, -150.0f), 40.0f, Colour.HotPink, 0.7f, 1, true);
            helper.DrawArrow(_drawStage, CoordinateSpace.Screen, new Vector2(200.0f, -200.0f), new Vector2(300.0f, 200.0f), 50.0f, 100.0f, 100.0f, Colour.Yellow, 0.3f, 1, true);          
        }

        public override void Rendering(IRenderQueue queue)
        {
            queue.ClearColour(WindowRenderTarget, Colour.Clear);
            queue.ClearDepth(WindowRenderTarget);
            queue.Draw(_drawStage, _camera, WindowRenderTarget);
        }

        public override void Shutdown() { }
    }
}
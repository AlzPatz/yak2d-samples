using SampleBase;
using System;
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
        private ITexture _textureWall;

        private float _angle = 0.0f;

        public override string ReturnWindowTitle() => "Drawing using Helper Functions";

        public override void OnStartup() { }

        public override bool CreateResources(IServices yak)
        {
            _drawStage = yak.Stages.CreateDrawStage();

            _camera = yak.Cameras.CreateCamera2D(960, 540, 1.0f);

            _textureWall = yak.Surfaces.LoadTexture("stonewall", AssetSourceEnum.Embedded);

            return true;
        }

        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds) => true;
        public override void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Drawing(IDrawing draw,
                                     IFps fps,
                                     IInput input,
                                     ICoordinateTransforms transforms,
                                     float timeSinceLastDrawSeconds,
                                     float timeSinceLastUpdateSeconds)
        {
            var helper = draw.Helpers;

            helper.DrawColouredQuad(_drawStage, CoordinateSpace.Screen, Colour.Aqua, new Vector2(-300.0f, 150.0f), 200.0f, 180.0f, 0.9f);
            helper.DrawTexturedQuad(_drawStage, CoordinateSpace.Screen, _textureWall, Colour.White, new Vector2(-150.0f, 50.0f), 220.0f, 160.0f, 0.7f);
            helper.DrawColouredPoly(_drawStage, CoordinateSpace.Screen, Colour.LimeGreen, new Vector2(0.0f, -50.0f), 8, 120.0f, 0.3f);
            helper.DrawLine(_drawStage, CoordinateSpace.Screen, new Vector2(0.0f, -80.0f), new Vector2(200.0f, -150.0f), 40.0f, Colour.HotPink, 0.7f, 1, true);
            helper.DrawArrow(_drawStage, CoordinateSpace.Screen, new Vector2(200.0f, -200.0f), new Vector2(300.0f, 200.0f), 50.0f, 100.0f, 100.0f, Colour.Yellow, 0.3f, 1, true);

            _angle += timeSinceLastDrawSeconds;

            if (_angle > 2.0f * Math.PI)
            {
                _angle -= (float)Math.PI * 2.0f;
            }

            helper.DrawColouredQuad(_drawStage, CoordinateSpace.Screen, Colour.Azure, new Vector2(-200.0f, -150.0f), 60.0f, 40.0f, 0.5f, 1, _angle);
        }

        public override void Rendering(IRenderQueue q, IRenderTarget windowRenderTarget)
        {
            q.ClearColour(windowRenderTarget, Colour.Clear);
            q.ClearDepth(windowRenderTarget);
            q.Draw(_drawStage, _camera, windowRenderTarget);
        }

        public override void Shutdown() { }
    }
}
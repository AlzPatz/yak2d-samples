using SampleBase;
using System;
using System.Numerics;
using Yak2D;

namespace Draw_FluentInterfaceDrawingHelper
{
    /// <summary>
    /// Drawing Regular Polygons using The Fluent Interface Helper Functions
    /// </summary>
    public class DrawFluentInterfaceExamples : ApplicationBase
    {
        private IDrawStage _drawStage;
        private ICamera2D _camera;
        private ITexture _textureCity;
        private ITexture _textureGrass;
        private ITexture _textureMud;
        private ITexture _textureWall;

        public override string ReturnWindowTitle() => "Drawing using Fluent Interface Helper Functions";

        public override void OnStartup() { }

        public override bool CreateResources(IServices yak)
        {
            _drawStage = yak.Stages.CreateDrawStage();

            _camera = yak.Cameras.CreateCamera2D(960, 540, 1.0f);

            _textureCity = yak.Surfaces.LoadTexture("city", AssetSourceEnum.Embedded);
            _textureGrass = yak.Surfaces.LoadTexture("grass", AssetSourceEnum.Embedded);
            _textureMud = yak.Surfaces.LoadTexture("mudrock", AssetSourceEnum.Embedded);
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

            //Moving Pentagon Example 

            var startPosition = new Vector2(-350.0f, 150.0f);
            var startColour = new Colour(0.22f, 0.0f, 0.35f, 1.0f);
            var startDepth = 1.0f;

            //Create original drawing object and generate draw request
            var d = helper.Construct().Coloured(startColour).Poly(startPosition, 5, 100.0f).Filled();
            draw.Draw(_drawStage, d.GenerateDrawRequest(CoordinateSpace.Screen, startDepth, 0));

            var endPosition = new Vector2(350.0f, -150.0f);
            var endColour = new Colour(1.0f, 0.71f, 0.76f, 0.5f);
            var endDepth = 0.0f;

            var totalRotation = 2.0f * (float)Math.PI;
            var numberSteps = 24;

            var shiftAmount = (endPosition - startPosition) / (1.0f * numberSteps);
            var rotAmount = totalRotation / (1.0f * numberSteps);

            for (var n = 0; n < numberSteps; n++)
            {
                var frac = (1.0f + n) / (1.0f * numberSteps);
                var col = startColour + (frac * (endColour - startColour));
                var depth = startDepth + (frac * (endDepth - startDepth));

                //Modify draw object incrementally and draw each coonfiguration
                d = d.ShiftPosition(shiftAmount).Rotate(rotAmount).ChangeColour(col);
                draw.Draw(_drawStage, d.GenerateDrawRequest(CoordinateSpace.Screen, depth, 0));
            }


            //Using a stretch texture brush and changing texture
            var brushWallStretch = new TextureBrush(_textureWall, TextureCoordinateMode.None, TextureScaling.Stretch, Vector2.Zero);

            var t = helper.Construct().Textured(brushWallStretch, Colour.White).Quad(new Vector2(0.0f, 210.0f), 200.0f, 100.0f).Filled();
            draw.Draw(_drawStage, t.GenerateDrawRequest(CoordinateSpace.Screen, 0.5f, 1));
            t = t.ShiftPosition(new Vector2(200.0f, -100.0f)).Scale(0.5f, 1.0f);
            draw.Draw(_drawStage, t.GenerateDrawRequest(CoordinateSpace.Screen, 0.5f, 1));

            t = t.ChangeTexture0(_textureCity).ShiftPosition(new Vector2(150.0f, 0.0f));
            draw.Draw(_drawStage, t.GenerateDrawRequest(CoordinateSpace.Screen, 0.5f, 1));


            //Using a Mirror Wrapped Texture 
            var brushCityTiledMirror = new TextureBrush(_textureCity, TextureCoordinateMode.Mirror, TextureScaling.Tiled, new Vector2(0.1f, 0.07f));
            t = helper.Construct().Textured(brushCityTiledMirror, Colour.White).Quad(new Vector2(-200.0f, -170.0f), 400.0f, 120.0f).Filled();
            draw.Draw(_drawStage, t.GenerateDrawRequest(CoordinateSpace.Screen, 0.5f, 1));


            //Using a Repeat Wrap Dual Texture (textures tile at a different rate per texture and per axis)
            var brushGrassTiledMirror = new TextureBrush(_textureGrass, TextureCoordinateMode.Mirror, TextureScaling.Tiled, new Vector2(1.0f, 3.0f));
            var brushMudTiledRepeat = new TextureBrush(_textureMud, TextureCoordinateMode.Wrap, TextureScaling.Tiled, new Vector2(0.1f, 10.0f));
            t = helper.Construct().DualTextured(brushMudTiledRepeat, brushGrassTiledMirror, TextureMixDirection.Horizontal, Colour.White).Quad(new Vector2(-350.0f, -60.0f), 200.0f, 80.0f).Filled();
            draw.Draw(_drawStage, t.GenerateDrawRequest(CoordinateSpace.Screen, 0.3f, 1));

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
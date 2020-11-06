using SampleBase;
using System.Numerics;
using Yak2D;

namespace Draw_FontExample
{
    /// <summary>
    /// Bitmap Font text rendering example
    /// </summary>
    public class DrawFontExample : ApplicationBase
    {
        private IDrawStage _drawStage;
        private ICamera2D _camera;
        private IFont _font;

        public override string ReturnWindowTitle() => "Drawing Bitmap Fonts";

        public override void OnStartup() { }

        public override bool CreateResources(IServices yak)
        {
            _drawStage = yak.Stages.CreateDrawStage();

            _camera = yak.Cameras.CreateCamera2D(960, 540, 1.0f);

            _font = yak.Fonts.LoadFont("snappy", AssetSourceEnum.Embedded); // Yak2D will load all .fnt files for snappy (you will note there are two sizes in the folder)

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
            draw.DrawString(_drawStage,
                              CoordinateSpace.Screen,
                              "This string is rendered using the default font",
                              Colour.IndianRed,
                              18.0f,
                              new Vector2(-430.0f, 220.0f), 
                              TextJustify.Left,
                              0.5f,
                              0);

            draw.DrawString(_drawStage,
                               CoordinateSpace.Screen,
                               "The quick brown fox jumps over the lazy dog",
                               Colour.PowderBlue,
                               38.0f,
                               new Vector2(0.0f, 19.0f), //Aiming for centre screen. Y position is always the top of the letters, so shift up by half height to centre
                               TextJustify.Centre,
                               0.5f,
                               0,
                               _font);

            // An example that shows the nearest size source textures will be scaled to fit chosen font size
            // Here the largest size source texture is 64, but the font size required is 120.0f
            // You will see some blurring 
            draw.DrawString(_drawStage,
                       CoordinateSpace.Screen,
                       "Large letters",
                       Colour.LawnGreen,
                       120.0f,
                       new Vector2(455.0f, -100.0f),
                       TextJustify.Right,
                       0.5f,
                       0,
                       _font);

            draw.DrawString(_drawStage,
                CoordinateSpace.Screen,
                "Small source textures",
                Colour.PaleVioletRed,
                120.0f,
                new Vector2(455.0f, -220.0f),
                TextJustify.Right,
                0.5f,
                0,
                _font);
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


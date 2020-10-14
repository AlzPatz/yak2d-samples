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

        public override bool CreateResources(IServices services)
        {
            _drawStage = services.Stages.CreateDrawStage();

            _camera = services.Cameras.CreateCamera2D(960, 540, 1.0f);

            _font = services.Fonts.LoadFont("snappy", AssetSourceEnum.Embedded); // Yak2D will load all .fnt files for snappy (you will note there are two sizes in the folder)

            return true;
        }

        public override bool Update_(IServices services, float timeSinceLastUpdateSeconds) => true;
        public override void PreDrawing(IServices services, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Drawing(IDrawing drawing, IFps fps, IInput input, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            drawing.DrawString(_drawStage,
                              CoordinateSpace.Screen,
                              "This string is rendered using the default font",
                              Colour.IndianRed,
                              18.0f,
                              new Vector2(-430.0f, 220.0f), 
                              TextJustify.Left,
                              0.5f,
                              0);

            drawing.DrawString(_drawStage,
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
            drawing.DrawString(_drawStage,
                       CoordinateSpace.Screen,
                       "Large letters",
                       Colour.LawnGreen,
                       120.0f,
                       new Vector2(455.0f, -100.0f),
                       TextJustify.Right,
                       0.5f,
                       0,
                       _font);

            drawing.DrawString(_drawStage,
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

        public override void Rendering(IRenderQueue queue)
        {
            queue.ClearColour(WindowRenderTarget, Colour.Clear);
            queue.ClearDepth(WindowRenderTarget);
            queue.Draw(_drawStage, _camera, WindowRenderTarget);
        }

        public override void Shutdown() { }
    }
}


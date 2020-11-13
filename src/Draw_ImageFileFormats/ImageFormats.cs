using SampleBase;
using System.Collections.Generic;
using System.Drawing;
using Yak2D;

namespace Draw_ImageFileFormats
{
    /// <summary>
    /// Image Formats - Loading an image of each of the image encoding filetypes supported
    /// </summary>
    public class ImageFormats : ApplicationBase
    {
        private List<ITexture> _textures;
        //private ITexture _texTest;
        private Size _texSize;
        private IDrawStage _drawStage;
        private ICamera2D _camera;

        public override string ReturnWindowTitle() => "Image Formats - PNG, BMP, GIF, JPG, TGA";

        public override void OnStartup() { }

        public override bool CreateResources(IServices yak)
        {
            //Example on loading file directly from a directory
            //var fs = System.IO.File.OpenRead(@"c:\image.png");
            //_texTest = yak.Surfaces.LoadTexture(fs);

            _textures = new List<ITexture>();
            _textures.Add(yak.Surfaces.LoadTexture("dino", AssetSourceEnum.Embedded, ImageFormat.PNG));
            _textures.Add(yak.Surfaces.LoadTexture("dino", AssetSourceEnum.Embedded, ImageFormat.BMP));
            _textures.Add(yak.Surfaces.LoadTexture("dino", AssetSourceEnum.Embedded, ImageFormat.GIF));
            _textures.Add(yak.Surfaces.LoadTexture("dino", AssetSourceEnum.Embedded, ImageFormat.JPG));
            _textures.Add(yak.Surfaces.LoadTexture("dino", AssetSourceEnum.Embedded, ImageFormat.TGA));

            _texSize = yak.Surfaces.GetSurfaceDimensions(_textures[0]);

            _drawStage = yak.Stages.CreateDrawStage();

            _camera = yak.Cameras.CreateCamera2D();

            return true;
        }
        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds) => true;

        public override void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Drawing(IDrawing draw, IFps fps, IInput input, ICoordinateTransforms transform, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            var xPos = -480.0f + (0.5f * _texSize.Width);
            _textures.ForEach(tex =>
            {
                draw.Helpers.DrawTexturedQuad(_drawStage, CoordinateSpace.Screen, tex, Colour.White, new System.Numerics.Vector2(xPos, 0.0f), _texSize.Width, _texSize.Height, 0.9f, 0);
                xPos += _texSize.Width;
            });

            //draw.Helpers.DrawTexturedQuad(_drawStage, CoordinateSpace.Screen, _texTest, Colour.White, new System.Numerics.Vector2(0.0f, 0.0f), 200, 200, 0.8f, 0);
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
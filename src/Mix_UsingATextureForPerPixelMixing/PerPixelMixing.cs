using System;
using System.Numerics;
using Yak2D;
using SampleBase;

namespace Mix_UsingATextureForPerPixelMixing
{
    /// <summary>
    /// Per Pixel Mixing using a Texture as a Mask
    /// </summary>
    public class PerPixelMixing : ApplicationBase
    {
        private IMixStage _mixStage;
        private ITexture _textureLight;
        private ITexture _textureNight;
        private ITexture _textureDay;
        private IRenderTarget _mixTarget;
        private IDrawStage _drawStage;
        private ICamera2D _camera;
        private Vector2 _mousePosition;

        public override string ReturnWindowTitle() => "Mixing Example - Simple Whole Texture Mixing Factors";

        public override void OnStartup() { }

        public override bool CreateResources(IServices services)
        {
            _mixStage = services.Stages.CreateMixStage();
            services.Stages.SetMixStageProperties(_mixStage, Vector4.One); //Turn all on at the per texture level

            _textureNight = services.Surfaces.LoadTexture("city-night", AssetSourceEnum.Embedded);
            _textureDay = services.Surfaces.LoadTexture("city-day", AssetSourceEnum.Embedded);

            _mixTarget = services.Surfaces.CreateRenderTarget(960, 540);
            _drawStage = services.Stages.CreateDrawStage(true, BlendState.Override); //Override important to use 4 components of texture as seperate channels to draw too

            _camera = services.Cameras.CreateCamera2D(960, 540);

            var radius = 128;
            var dim = 2 * radius;
            var pixels = new Vector4[dim * dim];

            var rad = (float)radius;
            for (var y = 0; y < dim; y++)
            {
                for (var x = 0; x < dim; x++)
                {
                    var xf = (float)x;
                    var yf = (float)y;

                    var dx = 128.0f - xf;
                    var dy = 128.0f - yf;

                    var dis = (float)Math.Sqrt((dx * dx) + (dy * dy));

                    var pixel = Vector4.Zero;

                    var frac = 0.0f;
                    if (dis <= rad)
                    {
                        frac = 1.0f - (dis / rad);
                    }
                    pixel.Y = frac;
                    pixel.X = 1.0f - frac;

                    var index = (y * dim) + x;

                    pixels[index] = pixel;
                }
            }

            _textureLight = services.Surfaces.CreateRgbaFromData((uint)dim, (uint)dim, pixels);

            return true;
        }

        public override bool Update_(IServices services, float timeSinceLastUpdateSeconds)
        {
            _mousePosition = (services.Input.MousePosition - (0.5f * new Vector2(960.0f, 540.0f)));
            _mousePosition.Y = -_mousePosition.Y;
            return true;
        }

        public override void PreDrawing(IServices services, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Drawing(IDrawing drawing, IFps fps, IInput input, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            var size = 256.0f;
            drawing.DrawingHelpers.DrawTexturedQuad(_drawStage,
                                                    CoordinateSpace.Screen,
                                                    _textureLight,
                                                    Colour.White,
                                                    _mousePosition,
                                                    size,
                                                    size,
                                                    0.5f);
        }

        public override void Rendering(IRenderQueue queue)
        {
            queue.ClearColour(WindowRenderTarget, Colour.Clear);
            queue.ClearDepth(WindowRenderTarget);

            queue.ClearColour(_mixTarget, new Colour(1.0f, 0.0f, 0.0f, 0.0f));
            queue.Draw(_drawStage, _camera, _mixTarget);

            queue.Mix(_mixStage, _mixTarget, _textureNight, _textureDay, null, null, WindowRenderTarget);
        }

        public override void Shutdown() { }
    }
}
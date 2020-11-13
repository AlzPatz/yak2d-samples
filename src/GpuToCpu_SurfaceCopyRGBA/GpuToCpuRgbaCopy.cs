using System;
using System.Numerics;
using Yak2D;
using SampleBase;

namespace GpuToCpu_SurfaceCopyRGBA
{
    /// <summary>
    /// 
    /// </summary>
    public class GpuToCpuRgbaCopy : ApplicationBase
    {
        private const float PROBABILITY_OF_SPLAT = 0.01f;
        private const float MIN_RADIUS = 10.0f;
        private const float MAX_RADIUS = 200.0f;

        private IRenderTarget _renderTarget;
        private ISurfaceCopyStage _gpuToCpuCopyStage;
        private IDrawStage _drawStageSplats;
        private IDrawStage _drawStageGui;
        private ICamera2D _camera;
        private Vector4[] _data;

        private Random _rnd;

        public override string ReturnWindowTitle() => "Gpu to Cpu Surface Copy - RGBA";

        public override void OnStartup()
        {
            _rnd = new Random();
        }

        public override bool CreateResources(IServices yak)
        {
            _renderTarget = yak.Surfaces.CreateRenderTarget(960, 540, false);

            var callBack = new Action<uint, TextureData>((index, data) =>
            {
                _data = data.Pixels;
            });

            _gpuToCpuCopyStage = yak.Stages.CreateSurfaceCopyDataStage(960, 540, callBack, false);

            _drawStageSplats = yak.Stages.CreateDrawStage();
            _drawStageGui = yak.Stages.CreateDrawStage();

            _camera = yak.Cameras.CreateCamera2D(960, 540);

            return true;
        }

        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds) => true;

        public override void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Drawing(IDrawing draw, IFps fps, IInput input, ICoordinateTransforms transform, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            //Splats

            if (_rnd.NextDouble() < PROBABILITY_OF_SPLAT)
            {
                var x = (float)_rnd.NextDouble() * 960.0f;
                var y = (float)_rnd.NextDouble() * 540.0f;
                x -= 480.0f;
                y -= 270.0f;

                var rad = MIN_RADIUS + (float)(_rnd.NextDouble() * (MAX_RADIUS - MIN_RADIUS));
                var colour = new Colour(0.3f + (0.7f * (float)_rnd.NextDouble()), 0.3f + (0.7f * (float)_rnd.NextDouble()), 0.3f + (0.7f * (float)_rnd.NextDouble()), 1.0f);

                draw.Helpers.DrawColouredPoly(_drawStageSplats, CoordinateSpace.Screen, colour, new Vector2(x, y), 64, rad, 0.9f, 0);
            }

            //GUI

            if (_data != null)
            {
                //Textures share top left origin coordinate with screen position, do not use transfer
                //Check this across backends!!
                var mouse = input.MousePosition;
                var mx = (int)mouse.X;
                var my = (int)mouse.Y;

                var linearPixel = (my * 960) + mx;

                var sample = _data[linearPixel];

                var mouseScreen = transform.ScreenFromWindow(mouse, _camera);

                draw.DrawString(_drawStageGui,
                                CoordinateSpace.Screen,
                                sample.ToString("0.0"),
                                Colour.White,
                                22,
                                mouseScreen.Position + new Vector2(0.0f, 48.0f),
                                TextJustify.Centre,
                                0.5f,
                                0);
            }
        }

        public override void Rendering(IRenderQueue q, IRenderTarget windowRenderTarget)
        {
            q.ClearDepth(windowRenderTarget);
            q.ClearColour(windowRenderTarget, Colour.Clear);
            q.ClearDepth(_renderTarget);
            q.Draw(_drawStageSplats, _camera, _renderTarget);
            q.CopySurfaceData(_gpuToCpuCopyStage, _renderTarget);
            q.Copy(_renderTarget, windowRenderTarget);
            q.Draw(_drawStageGui, _camera, windowRenderTarget);
        }

        public override void Shutdown() { }
    }
}
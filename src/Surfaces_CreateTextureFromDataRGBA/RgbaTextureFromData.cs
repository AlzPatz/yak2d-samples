using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Yak2D;
using SampleBase;

namespace Surfaces_CreateTextureFromDataRGBA
{
    public class RgbaTextureFromData : ApplicationBase
    {
        private const int WIDTH = 960;
        private const int HEIGHT = 540;

        private const float ZOOM_IN_STEP_FACTOR = 1.2f;
        private const float ZOOM_OUT_STEP_FACTOR = 0.83f;

        private const float BASE_NUM_ITERATIONS = 200.0f;
        private const float NUM_ITERATIONS_STEP_SCALER = 1.05f;
        private const float NUM_ITERATIONS_ZOOM_SCALER = 0.02f;

        private IDrawStage _drawStage;
        private ICamera2D _camera;

        private ITexture _texture;
        private ITexture _textureOneBeforeADestructionCache;

        private double _zoom = 200.0f;
        private Vector2 _target = Vector2.Zero;
        private double _zoomOfLastTexture;
        private Vector2 _targetOfLastTexture;

        private int _iteration;
        private bool _calculating;

        private class Completion
        {
            public float Percent { get; set; }
        }
        private Completion _completion;
        private CancellationTokenSource _cancellationSource;

        public override string ReturnWindowTitle() => "Generating Textures from RGBA pixel data";

        public override void OnStartup() { }

        public override bool CreateResources(IServices yak)
        {
            _drawStage = yak.Stages.CreateDrawStage();
            _camera = yak.Cameras.CreateCamera2D();

            //Generate a clear texture if no fractal texture yet computed
            if (_texture == null)
            {
                var pixels = Enumerable.Repeat(Vector4.Zero, WIDTH * HEIGHT).ToArray();

                //This is actually the only line required to demonstrate texture creation from data
                //=================================================================================
                _texture = yak.Surfaces.CreateRgbaFromData(WIDTH, HEIGHT, pixels);
                //=================================================================================
            }

            _calculating = false;
            _iteration = 0;

            _zoomOfLastTexture = _zoom;
            _targetOfLastTexture = _target;

            return true;
        }

        private async void TriggerFractalCalculation(IServices yak, Vector2 target, double zoom, int iteration)
        {
            _cancellationSource = new CancellationTokenSource();
            var cancellationToken = _cancellationSource.Token;

            _completion = new Completion();
            var progress = new Progress<float>(percent =>
            {
                _completion.Percent = percent;
            });

            _calculating = true;

            Vector4[] pixelData = await Task.Run(() =>
            {
                var pixels = GenerateMandelbrot(target, zoom, iteration, cancellationToken, progress);
                return pixels;
            });

            if (pixelData != null)
            {
                _targetOfLastTexture = target;
                _zoomOfLastTexture = zoom;
                _iteration++;

                //The cache ensures we do not destroy a texture being used ina currently inflight render cycle
                //As these calculations are triggered only once per render in pre-draw, the cached texture wil never be in 
                //use when requested to destroy it
                if (_textureOneBeforeADestructionCache != null)
                {
                    yak.Surfaces.DestroySurface(_textureOneBeforeADestructionCache);
                }
                _textureOneBeforeADestructionCache = _texture;
                _texture = yak.Surfaces.CreateRgbaFromData(WIDTH, HEIGHT, pixelData);
            }

            _calculating = false;
        }

        private Vector4[] GenerateMandelbrot(Vector2 target, double zoom, int iteration, CancellationToken cancellationToken, IProgress<float> progress)
        {
            //Calculating the Mandelbrot set on the CPU for use in an example of a GPU focused library is all kinds of sad
            //fc(z) = z^2 + c, where c = x + yi
            //Colour based on how many iterations until goes past 2 (and onto infinity)

            var maxIterations = (int)(BASE_NUM_ITERATIONS + ((zoom * NUM_ITERATIONS_ZOOM_SCALER) * (iteration * NUM_ITERATIONS_STEP_SCALER)));

            var pixels = new Vector4[WIDTH * HEIGHT];

            zoom = zoom <= 0.0 ? zoom = 0.000000001 : zoom;

            var oneOverZoom = 1.0 / zoom;

            var min_x = target.X - (0.5 * WIDTH * oneOverZoom);
            var max_x = target.X + (0.5 * WIDTH * oneOverZoom);

            var min_y = target.Y - (0.5 * HEIGHT * oneOverZoom);
            var max_y = target.Y + (0.5 * HEIGHT * oneOverZoom);

            var totalSteps = WIDTH * HEIGHT;

            for (var y = 0; y < HEIGHT; y++)
            {
                for (var x = 0; x < WIDTH; x++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return null;
                    }

                    var a = Map(x, 0, WIDTH, min_x, max_x);
                    var b = Map(y, 0, HEIGHT, min_y, max_y);

                    var aInit = a;
                    var bInit = b;

                    var num = 0;
                    for (var n = 0; n < maxIterations; n++)
                    {
                        //z^2 + c
                        //(x + yi) *(x + yi) = x^2 + 2xyi - y^2

                        //x^2 - y^2
                        double an = (a * a) - (b * b);
                        //2xy
                        double bn = 2.0 * a * b;

                        a = an + aInit;
                        b = bn + bInit;

                        if ((a + b) > 2.0)
                        {
                            //If this rises above 2.0, it will tend to infinity
                            break;
                        }

                        num++;
                    }

                    //Generate the Colour
                    var pix = (y * WIDTH) + x;

                    var frac = (1.0 * num) / (1.0 * maxIterations);

                    if (num == maxIterations)
                    {
                        frac = 0.0;
                    }

                    if (frac < 0.05)
                    {
                        frac = 0.0;
                    }

                    var alpha = frac;

                    frac *= 255.0;

                    var red = frac;
                    var green = Map(Math.Sqrt(frac), 0.0, Math.Sqrt(255.0), 0.0, 255.0);
                    var blue = Map(frac * frac, 0.0, 255.0 * 255, 0.0, 255.0);

                    red /= 255.0;
                    green /= 255.0;
                    blue /= 255.0;

                    //Store geenrated pixel colour
                    pixels[pix] = new Vector4((float)red, (float)green, (float)blue, (float)alpha);

                    //Report back percentage complete
                    var percent = (1.0f * ((y * WIDTH) + x + 1)) / (1.0f * totalSteps);
                    progress.Report(percent);
                }
            }

            return pixels;
        }

        private double Map(double val, double in_min, double in_max, double out_min, double out_max)
        {
            return out_min + (((val - in_min) / (in_max - in_min)) * (out_max - out_min));
        }

        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds)
        {
            var input = yak.Input;

            var change = false;
            var newZoom = _zoom;

            if (input.WasMouseReleasedThisFrame(MouseButton.Left))
            {
                newZoom *= ZOOM_IN_STEP_FACTOR;
                change = true;
            }

            if (input.WasMouseReleasedThisFrame(MouseButton.Right))
            {
                newZoom *= ZOOM_OUT_STEP_FACTOR;
                change = true;
            }

            if (change)
            {
                var mousePosDeltaScreen = (input.MousePosition - (0.5f * new Vector2(WIDTH, HEIGHT)));
                var ooz = 1.0f / _zoom;

                var targetShift = (float)ooz * mousePosDeltaScreen;

                _target += targetShift;
                _zoom = newZoom;

                _cancellationSource.Cancel();

                _iteration = 0;
            }

            return true;
        }

        public override void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            if (!_calculating)
            {
                TriggerFractalCalculation(yak, _target, _zoom, _iteration);
            }
        }

        public override void Drawing(IDrawing draw,
                                     IFps fps,
                                     IInput input,
                                     ICoordinateTransforms transforms,
                                     float timeSinceLastDrawSeconds,
                                     float timeSinceLastUpdateSeconds)
        {
            var widthInCurrentZoom = (_zoom / _zoomOfLastTexture) * WIDTH;
            var heightInCurrentZoom = (_zoom / _zoomOfLastTexture) * HEIGHT;
            var relativePos = (_targetOfLastTexture - _target) * (float)_zoom;
            relativePos.Y = -relativePos.Y;

            draw.Helpers.DrawTexturedQuad(_drawStage, CoordinateSpace.Screen, _texture, Colour.White, relativePos, (float)widthInCurrentZoom, (float)heightInCurrentZoom, 0.5f, 0);


            var barWidth = 200.0f;
            var barHeight = 30.0f;

            var leftBarPos = new Vector2((0.5f * WIDTH) - 20.0f - barWidth, (0.5f * HEIGHT) - 20.0f - (0.5f * barHeight));
            draw.Helpers.DrawColouredQuad(_drawStage, CoordinateSpace.Screen, Color.DarkSlateBlue, leftBarPos + new Vector2(0.5f * barWidth, 0.0f), barWidth, barHeight, 0.9f, 0);
            draw.Helpers.DrawColouredQuad(_drawStage, CoordinateSpace.Screen, Color.Yellow, leftBarPos + new Vector2(0.5f * (_completion.Percent * barWidth), 0.0f), (_completion.Percent * barWidth), barHeight, 0.8f, 0);
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
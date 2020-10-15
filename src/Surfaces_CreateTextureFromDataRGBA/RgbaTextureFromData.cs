using SampleBase;
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Yak2D;

namespace Surfaces_CreateTextureFromDataRGBA
{
    public class RgbaTextureFromData : ApplicationBase
    {
        private const int WIDTH = 960;
        private const int HEIGHT = 540;

        private bool _calculating;
        private ITexture _texture;
        private ITexture _textureOneBeforeADestructionCache;

        public override string ReturnWindowTitle() => "Generating Textures from RGBA pixel data";

        public override void OnStartup() { }

        public override bool CreateResources(IServices services)
        {
            //Generate a clear texture if no fractal texture yet computed
            if (_texture == null)
            {
                var pixels = Enumerable.Repeat(Vector4.Zero, WIDTH * HEIGHT).ToArray();
                _texture = services.Surfaces.CreateRgbaFromData(WIDTH, HEIGHT, pixels);
            }

            _calculating = false;

            return true;
        }

        private async void TriggerFractalCalculation(IServices services)
        {
            _calculating = true;

            Vector4[] pixelData = await Task.Run(() =>
            {
                //var rnd = new Random();
                //var colour = new Vector4((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble(), 1.0f);
                //return Enumerable.Repeat(colour, 960 * 540).ToArray();
                var pixels = GenerateMandelbrot();
                return pixels;
            });

            //The cache ensures we do not destroy a texture being used ina currently inflight render cycle
            //As these calculations are triggered only once per render in pre-draw, the cahced texture wil never be in 
            //use when requested to destroy it
            if (_textureOneBeforeADestructionCache != null)
            {
                services.Surfaces.DestroySurface(_textureOneBeforeADestructionCache);
            }
            _textureOneBeforeADestructionCache = _texture;
            _texture = services.Surfaces.CreateRgbaFromData(WIDTH, HEIGHT, pixelData);

            _calculating = false;
        }


        /*
         * 
         *count for iterations
         *
         max -= 0.1 * factor;
        min += 0.15 * factor;
        factor *= 0.9349
        MAX_ITERATIONS +=5

        iuf()count > 30)
        {
        max_terations * 1.02;
         */


        private Vector4[] GenerateMandelbrot()
        {
            //Calculating the Mandelbrot set on the CPU for use in an example of a GPU focused library is all kinds of sad
            //I'll rectify this in the Custom Veldrid Example

            //fc(z) = z^2 + c
            //Colour based on how many iterations until goes to infinity
            // c = x + yi

            var pixels = new Vector4[WIDTH * HEIGHT];

            double min = -2.84;// -2.0;
            double max = 1.0;//  2.0;
            double factor = 1.0;

            int maxIterations = 200;

            for (var y = 0; y < HEIGHT; y++)
            {
                for (var x = 0; x < WIDTH; x++)
                {
                    var a = Map(x, 0, WIDTH, min, max);
                    var b = Map(y, 0, HEIGHT, min, max);

                    var ai = a;
                    var bi = b;

                    var num = 0;
                    for (var n = 0; n < maxIterations; n++)
                    {
                        //z^2 + c
                        //(x + yi) *(x + yi) = x^2 + 2xyi - y^2

                        double a1 = a * a - b * b;
                        double b1 = 2.0 * a * b;

                        a = a1 + ai;
                        b = b1 + bi;

                        if ((a + b) > 2.0)
                        {
                            //Know for sure goes to inifinity
                            break;
                        }

                        num++;
                    }

                    var pix = (y * WIDTH) + x;

                    var frac = (1.0 * num) / (1.0 * maxIterations);

                    if(num == maxIterations)
                    {
                        frac = 0.0;
                    }

                    if(frac < 0.1)
                    {
                        frac = 0.0;
                    }

                    var alpha = frac;

                    frac *= 255.0;

                    var red = Map(frac * frac, 0.0, 255.0 * 255, 0.0, 255.0);
                    var green = frac;
                    var blue = Map(Math.Sqrt(frac), 0.0, Math.Sqrt(255.0), 0.0, 255.0);

                    red /= 255.0;
                    green /= 255.0;
                    blue /= 255.0;

                    pixels[pix] = new Vector4((float)red, (float)green, (float)blue, (float)alpha);

                }
            }

            return pixels;
        }

        private double Map(double val, double in_min, double in_max, double out_min, double out_max)
        {
            return out_min + (((val - in_min) / (in_max - in_min)) * (out_max - out_min));
        }

        public override bool Update_(IServices services, float timeSinceLastUpdateSeconds)
        {
            return true;
        }

        public override void PreDrawing(IServices services, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            if (!_calculating)
            {
                TriggerFractalCalculation(services);
            }
        }

        public override void Drawing(IDrawing drawing, IFps fps, IInput input, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Rendering(IRenderQueue queue)
        {
            queue.Copy(_texture, WindowRenderTarget);
        }

        public override void Shutdown() { }

        //private async void GenerateNewTrackAsync()
        //{
        //    if (_lock)
        //    {
        //        if (_cancellationSource != null)
        //            _cancellationSource.Cancel();
        //        return;
        //    }

        //    _cancellationSource = new CancellationTokenSource();
        //    var cancellationToken = _cancellationSource.Token;

        //    var progress = new Progress<TrackGeneratorReport>(x => ProcessGeneratorProgressRecord(x)) as IProgress<TrackGeneratorReport>;

        //    _lock = true;

        //    //var data = await _trackGenerator.Generate(_config, _seedGenerator.CreateSeedFromFourCharacterCharString("ALEX"), _random, progress, cancellationToken);
        //    //var data = await _trackGenerator.Generate(_config, _seedGenerator.CreateRandomSeed(), _random, progress, cancellationToken);
        //    var data = await _trackGenerator.Generate(_config, _seedGenerator.CreateSeedFromEightCharacterHexString("6B7EC0F5"), _random, progress, cancellationToken);

        //    if (!cancellationToken.IsCancellationRequested && data != null)
        //        _track.Init(data);

        //    _lock = false;
        //}
    }
}
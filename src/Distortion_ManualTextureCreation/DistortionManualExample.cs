using System;
using System.Numerics;
using System.Collections.Generic;
using Yak2D;
using SampleBase;

namespace Distortion_ManualTextureCreation
{
    /// <summary>
    /// Try and turn a static flame image into a roaring fire with a touch of noise
    /// </summary>
    public class DistortionManualExample : ApplicationBase
    {
        /*
            General Technique:
            Render a flame texture to a partial screen viewport, applying a distortion effect
            The distortion effect consist of a single quad draw from a moving portion of a distortion texture
            The distortion texture (float32 texture, not RGBA for high distortion fidelity) is generated at start up (more accurately, whenever resources are (re)created)
            The noise required needs to be cohernat (linear gradients), I use perlin noise (link to example provided)
            The distortion texture is twice as high and the same width as the viewport area / texture render size
            The portion of the distortion texture that is rendered as a quad in the distortion phase is moved 'upwards' over time
            This creates the impression of rising heat / flames
            When the portion of the distortion texture reaches the top, it wraps to the bottom
            The distortion texture is wrapped (not mirrored). The middle line of the rendered quad moves from 0 to 1 in y tex coords
            Therefore the bottom and tops of the quad render into wrapped texture coordinate space
            The distortion internal calculation surfaces are half dimension as this applies some bluring byu unature of downsampling and just looked better than full res (plus is faster)
         */

        private const float LOOP_DURATION = 2.5f;
        private float duration = 0.0f;

        private ITexture _textureHeatHaze;
        private ITexture _textureFire;

        private IDistortionStage _distortionDrawStage;
        private ICamera2D _camera;
        private IViewport _viewport;

        public override string ReturnWindowTitle() => "Distortion Example - Manual Distortion Texture Creation and Distortion Draw";

        public override void OnStartup() { }

        public override bool CreateResources(IServices services)
        {
            _distortionDrawStage = services.Stages.CreateDistortionStage(190, 270, true); //half dimension internal surfaces used 

            services.Stages.SetDistortionConfig(_distortionDrawStage, new DistortionEffectConfiguration
            {
                DistortionScalar = 30.0f
            });

            _textureHeatHaze = CreateHeatHazeTexture(services);

            _textureFire = services.Surfaces.LoadTexture("fire", AssetSourceEnum.Embedded);

            _camera = services.Cameras.CreateCamera2D(380, 540);

            _viewport = services.Stages.CreateViewport(290, 0, 380, 540);

            return true;
        }

        private ITexture CreateHeatHazeTexture(IServices services)
        {
            // We need coherant noise (Perlin will do) - smooth gradients
            // Explains it http://devmag.org.za/2009/04/25/perlin-noise/

            var rnd = new Random();

            var w = 380;
            var h = 1080;

            var baseRandomNoise = new float[w, h];
            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    baseRandomNoise[x, y] = (float)rnd.NextDouble();
                }
            }

            var octaves = new List<float[,]>();
            octaves.Add(baseRandomNoise);

            Func<float, float, float, float> lerp = (n, m, frac) =>
            {
                return n + ((m - n) * frac);
            };

            Func<float[,], int, float[,]> octaveGen = (baseNoise, octave) =>
            {
                var noise = new float[w, h];

                var period = 1 << octave;
                var frequency = 1.0f / (float)period;

                for (var y = 0; y < h; y++)
                {
                    var sy0 = (y / period) * period;
                    var sy1 = (sy0 + period) % h;
                    var yfrac = (y - sy0) * frequency;

                    for (var x = 0; x < w; x++)
                    {
                        var sx0 = (x / period) * period;
                        var sx1 = (sx0 + period) % w;
                        var xfrac = (x - sx0) * frequency;

                        var top = lerp(baseNoise[sx0, sy0], baseNoise[sx1, sy0], xfrac);
                        var bottom = lerp(baseNoise[sx0, sy1], baseNoise[sx1, sy1], xfrac);
                        noise[x, y] = lerp(top, bottom, yfrac);
                    }
                }

                return noise;
            };

            for (var o = 1; o < 9; o++)
            {
                octaves.Add(octaveGen(baseRandomNoise, o));
            }

            var perlin = new float[w, h];
            //Set to zero not required as float value type will default to this, but old habbits..
            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    perlin[x, y] = 0.0f;
                }
            }

            //Combine the noise octaves
            float totalAmplitude = 0.0f;
            var amplitudes = new float[] { 0.2f, 0.2f, 0.3f, 0.1f, 0.08f, 0.06f, 0.03f, 0.02f, 0.01f };

            for (var o = 0; o < 9; o++)
            {
                var octave = octaves[o];

                var amplitude = amplitudes[o];

                totalAmplitude += amplitude;

                for (var y = 0; y < h; y++)
                {
                    for (var x = 0; x < w; x++)
                    {
                        perlin[x, y] += octave[x, y] * amplitude;
                    }
                }
            }

            //Normalise - then scale into range -1 to 1, then put into linear array
            var pixels = new float[380 * 1080];
            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    perlin[x, y] /= totalAmplitude;
                    perlin[x, y] = (2.0f * perlin[x, y]) - 1.0f;

                    var loc = (y * w) + x;

                    pixels[loc] = perlin[x, y];
                }
            }

            return services.Surfaces.CreateFloat32FromData(380, 1080, pixels);
        }

        public override bool Update_(IServices services, float timeSinceLastUpdateSeconds) => true;

        public override void PreDrawing(IServices services, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Drawing(IDrawing drawing, IFps fps, IInput input, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            float fraction = duration / LOOP_DURATION;

            var request = new DistortionDrawRequest
            {
                CoordinateSpace = CoordinateSpace.Screen,
                FillType = FillType.Textured,
                Colour = Colour.White,
                Intensity = 1.0f,
                Texture0 = _textureHeatHaze,
                Texture1 = null,
                TextureWrap0 = TextureCoordinateMode.Wrap,
                TextureWrap1 = TextureCoordinateMode.None,
                Vertices = CreateQuadWithScrollingTextureCoordinates(fraction),
                Indices = new int[] { 0, 1, 3, 0, 3, 2 }
            };

            drawing.DrawDistortion(_distortionDrawStage, request);

            duration += timeSinceLastDrawSeconds;
        }

        private Vertex2D[] CreateQuadWithScrollingTextureCoordinates(float fraction)
        {
            fraction = 1.0f - fraction; //reverse direction (so heat / flames appear to rise)

            var bottom_ty = 1.25f - fraction;
            var top_ty = 0.75f - fraction;

            return new Vertex2D[]
            {
                new Vertex2D
                {
                      Position = new Vector2(-190, 270),
                      Colour = Colour.White, //Full effect at the top
                      TexCoord0 = new Vector2(0.0f, top_ty),
                      TexCoord1 = Vector2.Zero,
                      TexWeighting = 1.0f
                },
                new Vertex2D
                {
                      Position = new Vector2(190, 270),
                      Colour = Colour.White, //Full effect at the top
                      TexCoord0 = new Vector2(1.0f, top_ty),
                      TexCoord1 = Vector2.Zero,
                      TexWeighting = 1.0f
                },
                new Vertex2D
                {
                      Position = new Vector2(-190, -270),
                      Colour = Colour.Clear, //Low distortion at the bottom
                      TexCoord0 = new Vector2(0.0f, bottom_ty),
                      TexCoord1 = Vector2.Zero,
                      TexWeighting = 1.0f
                },
                new Vertex2D
                {
                      Position = new Vector2(190, -270),
                      Colour = Colour.Clear, //Low distortion at the bottom
                      TexCoord0 = new Vector2(1.0f, bottom_ty),
                      TexCoord1 = Vector2.Zero,
                      TexWeighting = 1.0f
                },
            };
        }

        public override void Rendering(IRenderQueue queue)
        {
            queue.ClearColour(WindowRenderTarget, Colour.Clear);
            queue.ClearDepth(WindowRenderTarget);

            queue.SetViewport(_viewport);
            queue.Distortion(_distortionDrawStage, _camera, _textureFire, WindowRenderTarget);
            queue.RemoveViewport();
        }

        public override void Shutdown() { }
    }
}
using Yak2D;
using SampleBase;
using System;
using System.Numerics;
using System.Collections.Generic;

namespace DrawingAndEffects_PrerenderingTexturesForUseLater
{
    /// <summary>
    /// 
    /// </summary>
    public class PreRenderExample : ApplicationBase
    {
        private const int PRE_RENDER_DIMENSION = 1024;
        private const int PRE_RENDER_NUMBER_FRAMES = 256;
        private const int MAX_PIXEL_DIVS = 256;
        private const int MIN_PIXEL_DIVS = 16;
        private const float MIN_FRACTIONAL_SIZE_OF_FIRST_FRAME = 0.05f;
        private const float SHOCK_WAVE_RING_WIDTH = 24.0f;
        private const float CHANCE_OF_EXPLOSION = 0.5f;
        private const float MIN_EXPLOSION_SIZE = 256.0f;
        private const float MAX_EXPLOSION_SIZE = 1024.0f;
        private const float MIN_EXPLOSION_LIFESPAN = 0.7f;
        private const float MAX_EXPLOSION_LIFESPAN = 1.2f;

        private ITexture _fireball;
        private IRenderTarget _step0;
        private IRenderTarget _step1;
        private IRenderTarget _step2;
        private IDrawStage _drawStageExplosionTexture;
        private IRenderTarget[] _baked;
        private IDrawStage _drawStageShockWave;
        private ICamera2D _cameraPre;
        private IDistortionStage _distortion;
        private IStyleEffectsStage _effects;
        private ITexture _distortionTexture;
        private IDrawStage _drawStagePost;
        private ICamera2D _cameraPost;

        private bool _preBakeRequired;
        private int _preBakeFrame;

        private Random _rnd;

        private class Explosion
        {
            public Vector2 Position { get; set; }
            public float Size { get; set; }
            public float Lifespan { get; set; }
            public float Age { get; set; }
        }

        private List<Explosion> _explosions;

        public override string ReturnWindowTitle() => "PreRender Example - PreBake Effect into Animation Textures";

        public override void OnStartup()
        {
            _rnd = new Random();

            _explosions = new List<Explosion>();
        }

        public override bool CreateResources(IServices yak)
        {
            _fireball = yak.Surfaces.LoadTexture("fireball", AssetSourceEnum.Embedded);

            _baked = new IRenderTarget[PRE_RENDER_NUMBER_FRAMES];
            for (var f = 0; f < PRE_RENDER_NUMBER_FRAMES; f++)
            {
                _baked[f] = yak.Surfaces.CreateRenderTarget(PRE_RENDER_DIMENSION, PRE_RENDER_DIMENSION, false);
            }

            _step0 = yak.Surfaces.CreateRenderTarget(PRE_RENDER_DIMENSION, PRE_RENDER_DIMENSION, true);
            _step1 = yak.Surfaces.CreateRenderTarget(PRE_RENDER_DIMENSION, PRE_RENDER_DIMENSION, true);
            _step2 = yak.Surfaces.CreateRenderTarget(PRE_RENDER_DIMENSION, PRE_RENDER_DIMENSION, true);
            _drawStageExplosionTexture = yak.Stages.CreateDrawStage();
            _drawStageShockWave = yak.Stages.CreateDrawStage();
            _cameraPre = yak.Cameras.CreateCamera2D(PRE_RENDER_DIMENSION, PRE_RENDER_DIMENSION);
            _distortion = yak.Stages.CreateDistortionStage((uint)PRE_RENDER_DIMENSION, (uint)PRE_RENDER_DIMENSION, true);
            _effects = yak.Stages.CreateStyleEffectsStage();
            _distortionTexture = yak.Helpers.DistortionHelper.TextureGenerator.ConcentricSinusoidalFloat32(1024, 1024, 16, true, true);

            _drawStagePost = yak.Stages.CreateDrawStage();
            _cameraPost = yak.Cameras.CreateCamera2D();

            _preBakeRequired = true;
            _preBakeFrame = 0;

            return true;
        }
        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds) => true;

        public override void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            //Update existing explosions and remove old ones
            var toRemove = new List<Explosion>();
            _explosions.ForEach(exp =>
            {
                exp.Age += timeSinceLastDrawSeconds;
                if (exp.Age > exp.Lifespan)
                {
                    toRemove.Add(exp);
                }
            });
            toRemove.ForEach(remove =>
            {
                _explosions.Remove(remove);
            });

            //Add any random explosions
            if (_rnd.NextDouble() < CHANCE_OF_EXPLOSION)
            {
                _explosions.Add(new Explosion
                {
                    Age = 0.0f,
                    Lifespan = MIN_EXPLOSION_LIFESPAN + (float)(_rnd.NextDouble() * (MAX_EXPLOSION_LIFESPAN - MIN_EXPLOSION_LIFESPAN)),
                    Position = new Vector2(-480.0f + (float)(_rnd.NextDouble() * 960.0f), -270.0f + (float)(_rnd.NextDouble() * 540.0f)),
                    Size = MIN_EXPLOSION_SIZE + ((float)_rnd.NextDouble() * (MAX_EXPLOSION_SIZE - MIN_EXPLOSION_SIZE))
                });
            }

            //Setting effects configurations
            if (_preBakeRequired)
            {
                var fraction = (1.0f * _preBakeFrame) / (1.0f * (PRE_RENDER_NUMBER_FRAMES - 1));
                var divs = MAX_PIXEL_DIVS - (int)(fraction * (MAX_PIXEL_DIVS - MIN_PIXEL_DIVS));

                //Distortion Settings
                yak.Stages.SetStyleEffectsGroupConfig(_effects, new StyleEffectGroupConfiguration
                {
                    Pixellate = new PixellateConfiguration
                    {
                        Intensity = 1.0f,
                        NumXDivisions = divs,
                        NumYDivisions = divs
                    },
                    Static = new StaticConfiguration
                    {
                        IgnoreTransparent = 0,
                        Intensity = fraction, //should get clipped
                        TexelScaler = 12.0f, 
                        TimeSpeed = 0.1f
                    },
                    //Turn the other effects off
                    CRT = new CrtEffectConfiguration
                    {
                        RgbPixelFilterAmount = 0.0f,
                        SimpleScanlinesIntensity = 0.0f
                    },
                    EdgeDetection = new EdgeDetectionConfiguration
                    {
                        Intensity = 0.0f
                    },
                    OldMovie = new OldMovieConfiguration
                    {
                        Intensity = 0.0f
                    }
                });

                //Update Distortion Configuration (doesn't change)
                yak.Stages.SetDistortionConfig(_distortion, new DistortionEffectConfiguration
                {
                    DistortionScalar = 50.0f
                });
            }
        }

        public override void Drawing(IDrawing draw, IFps fps, IInput input, ICoordinateTransforms transform, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            //We must not toggle boolean to false until last frame has rendered (a call to Rendering() made), so switch here rather than when testing the ++ below
            if (_preBakeRequired)
            {
                if (_preBakeFrame == PRE_RENDER_NUMBER_FRAMES)
                {
                    _preBakeRequired = false;
                }
            }

            if (_preBakeRequired)
            {
                var fraction = (1.0f * _preBakeFrame) / (1.0f * (PRE_RENDER_NUMBER_FRAMES - 1));
                var size = MIN_FRACTIONAL_SIZE_OF_FIRST_FRAME + (fraction * (1.0f - MIN_FRACTIONAL_SIZE_OF_FIRST_FRAME));
                size *= (1.0f * PRE_RENDER_DIMENSION);

                //Draw Explosion Texture onto Surface
                draw.Helpers.DrawTexturedQuad(_drawStageExplosionTexture, CoordinateSpace.Screen, _fireball, Colour.White, Vector2.Zero, size, size, 0.5f, 0);

                //Draw Styled Expolision onto Surface and then add ontop a Shockwave ring                                    
                draw.Helpers.DrawTexturedQuad(_drawStageShockWave, CoordinateSpace.Screen, _step1, Colour.White, Vector2.Zero, PRE_RENDER_DIMENSION, PRE_RENDER_DIMENSION, 0.9f, 0);
                var ringRadius = 0.5f * ((SHOCK_WAVE_RING_WIDTH * 2.0f) + (fraction * ((1.0f * PRE_RENDER_DIMENSION) - (SHOCK_WAVE_RING_WIDTH * 2.0f))));
                draw.Helpers.Construct().Coloured((1.0f - fraction) * Colour.Yellow).Poly(Vector2.Zero, 128, ringRadius).Outline(SHOCK_WAVE_RING_WIDTH).SubmitDraw(_drawStageShockWave, CoordinateSpace.Screen, 0.5f, 0);

                //Draw Shock Wave Distortion
                var dFastSize = 1.1f * fraction;
                var halfSize = 0.5f * dFastSize * PRE_RENDER_DIMENSION;
                var dRequest = new DistortionDrawRequest
                {
                    Colour = Colour.White,
                    CoordinateSpace = CoordinateSpace.Screen,
                    FillType = FillType.Textured,
                    Intensity = 1.0f,
                    Texture0 = _distortionTexture,
                    Texture1 = null,
                    TextureWrap0 = TextureCoordinateMode.Mirror,
                    TextureWrap1 = TextureCoordinateMode.Mirror,
                    Vertices = new Vertex2D[]
                                {
                                    new Vertex2D {  Position = new Vector2(-halfSize, halfSize), Colour = Colour.White, TexCoord0 = new Vector2(0.0f, 0.0f), TexCoord1 = Vector2.Zero, TexWeighting = 1.0f },
                                    new Vertex2D {  Position = new Vector2(halfSize, halfSize), Colour = Colour.White, TexCoord0 = new Vector2(1.0f, 0.0f), TexCoord1 = Vector2.Zero, TexWeighting = 1.0f },
                                    new Vertex2D {  Position = new Vector2(-halfSize, -halfSize), Colour = Colour.White, TexCoord0 = new Vector2(0.0f, 1.0f), TexCoord1 = Vector2.Zero, TexWeighting = 1.0f },
                                    new Vertex2D {  Position = new Vector2(halfSize, -halfSize), Colour = Colour.White, TexCoord0 = new Vector2(1.0f, 1.0f), TexCoord1 = Vector2.Zero, TexWeighting = 1.0f }
                                },
                    Indices = new int[]
                                {
                                    0, 1, 2, 2, 1, 3
                                }

                };

                draw.DrawDistortion(_distortion, dRequest);

                draw.DrawString(_drawStagePost,
                                CoordinateSpace.Screen,
                                "LOADING",
                                Colour.Red,
                                72.0f,
                                Vector2.Zero,
                                TextJustify.Centre,
                                0.5f,
                                0);
            }
            else
            {
                //Draw the Pre-Baked Explosions onto the Screen

                var depthShift = 1.0f / (1.0f * _explosions.Count + 2);
                var depth = depthShift;
                _explosions.ForEach(exp =>
                {

                    var frac = exp.Age / exp.Lifespan;
                    var frame = (int)(frac * PRE_RENDER_NUMBER_FRAMES);
                    if (frame == PRE_RENDER_NUMBER_FRAMES)
                    {
                        frame--;
                    }
                    draw.Helpers.DrawTexturedQuad(_drawStagePost,
                                                  CoordinateSpace.Screen,
                                                  _baked[frame],
                                                  (1.0f - frac) * Colour.White,
                                                  exp.Position,
                                                  exp.Size,
                                                  exp.Size,
                                                  depth,
                                                  0);
                    depth += depthShift;
                });
            }
        }

        public override void Rendering(IRenderQueue q, IRenderTarget windowRenderTarget)
        {
            q.ClearColour(windowRenderTarget, Colour.Clear);
            q.ClearDepth(windowRenderTarget);

            if (_preBakeRequired)
            {
                q.Draw(_drawStageExplosionTexture, _cameraPre, _step0);
                q.StyleEffects(_effects, _step0, _step1);
                q.Draw(_drawStageShockWave, _cameraPre, _step2);
                q.ClearColour(_baked[_preBakeFrame], Colour.Clear);
                q.ClearDepth(_baked[_preBakeFrame]);
                q.Distortion(_distortion, _cameraPre, _step2, _baked[_preBakeFrame]);
                _preBakeFrame++;
            }

            q.Draw(_drawStagePost, _cameraPost, windowRenderTarget);
        }

        public override void Shutdown() { }
    }
}
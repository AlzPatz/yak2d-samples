using System;
using System.Numerics;
using Yak2D;
using SampleBase;

namespace Distortion_ExampleUsingHelperFunctions
{
    /// <summary>
    /// Using Helpers to generate a concentric ring Float32 distortion texture, as well as manage a collection of 'ripples' in a pool. Sort of...
    /// </summary>
    public class DistortionHelperExample : ApplicationBase
    {
        private ITexture _texturePool;
        private ITexture _textureDistortion;
        private IDistortionStage _distortionDrawStage;
        private IDistortionCollection _distortionCollection;
        private ICamera2D _camera;

        private Random _rnd;

        private const double PROBABILITY_OF_NEW_RIPPLE = 0.03;
        private const double RADIUS_MIN = 64;
        private const double RADIUS_MAX = 384;
        private const double EXPANSION_SPEED = 128.0;

        public override string ReturnWindowTitle() => "Distortion Example - Using Helpers";

        public override void OnStartup()
        {
            _rnd = new Random();
        }


        public override bool CreateResources(IServices services)
        {
            _distortionDrawStage = services.Stages.CreateDistortionStage(480, 270, true);

            services.Stages.SetDistortionConfig(_distortionDrawStage, new DistortionEffectConfiguration
            { 
                 DistortionScalar = 30.0f
            });

            _texturePool = services.Surfaces.LoadTexture("pool", AssetSourceEnum.Embedded);

            _textureDistortion = services.Helpers.DistortionHelper.TextureGenerator.ConcentricSinusoidalFloat32(256, 256, 8, true, true);

            _distortionCollection = services.Helpers.DistortionHelper.CreateNewCollection();

            _camera = services.Cameras.CreateCamera2D(960, 540);

            return true;
        }

        public override bool Update_(IServices services, float timeSinceLastUpdateSeconds)
        {
            var input = services.Input;

            if (input.WasMouseReleasedThisFrame(MouseButton.Left))
            {
                var mousePosition = (input.MousePosition - (0.5f * new Vector2(960.0f, 540.0f)));
                mousePosition.Y = -mousePosition.Y;

                _distortionCollection.Add(LifeCycle.Single,
                                         CoordinateSpace.Screen,
                                         3.0f,
                                         _textureDistortion,
                                         mousePosition,
                                         mousePosition,
                                         Vector2.Zero,
                                         new Vector2(1920.0f),
                                         1.0f,
                                         0.0f,
                                         0.0f,
                                         0.0f);
            }

            return true;
        }

        public override void PreDrawing(IServices services, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            if (_rnd.NextDouble() < PROBABILITY_OF_NEW_RIPPLE)
            {
                var radius = RADIUS_MIN + ((RADIUS_MAX - RADIUS_MIN) * _rnd.NextDouble());

                var intensity = radius / RADIUS_MAX;

                var dim = 2.0 * radius;

                var duration = radius / EXPANSION_SPEED;

                var position = new Vector2(-480.0f + (float)(960.0 * _rnd.NextDouble()), -270.0f + (float)(540.0 * _rnd.NextDouble()));

                _distortionCollection.Add(LifeCycle.Single,
                                          CoordinateSpace.Screen,
                                          (float)duration,
                                          _textureDistortion,
                                          position,
                                          position,
                                          Vector2.Zero,
                                          new Vector2((float)dim),
                                          (float)intensity,
                                          0.0f,
                                          0.0f,
                                          0.0f);
            }

            _distortionCollection.Update(timeSinceLastDrawSeconds);
        }

        public override void Drawing(IDrawing drawing, IFps fps, IInput input, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            _distortionCollection.Draw(drawing, _distortionDrawStage);
        }

        public override void Rendering(IRenderQueue queue)
        {
            queue.ClearColour(WindowRenderTarget, Colour.Clear);
            queue.ClearDepth(WindowRenderTarget);
            queue.Distortion(_distortionDrawStage, _camera, _texturePool, WindowRenderTarget);
        }

        public override void Shutdown() { }
    }
}
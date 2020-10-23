using Yak2D;
using SampleBase;
using System.Numerics;

namespace Mesh_HelperExamples
{
    public class MeshHelperExamples : ApplicationBase
    {
        private ITexture _texCity;
        private ITexture _texMap;
        private ITexture _texDice;
        private ITexture _texGrid;

        private IRenderTarget _offScreenTarget_Quad;
        private IRenderTarget _offScreenTarget_CRT;
        private IRenderTarget _offScreenTarget_Sphere;
        private IRenderTarget _offScreenTarget_Rect;

        private IDrawStage _drawStage_Quad;
        private IDrawStage _drawStage_CRT;
        private IDrawStage _drawStage_Sphere;
        private IDrawStage _drawStage_Rect;

        private ICamera2D _camera2D;
        private ICamera3D _camera3D;

        private IMeshRenderStage _meshStage_Quad;
        private IMeshRenderStage _meshStage_CRT;
        private IMeshRenderStage _meshStage_Sphere;
        private IMeshRenderStage _meshStage_Rect;

        private IViewport _viewport_Quad;
        private IViewport _viewport_CRT;
        private IViewport _viewport_Sphere;
        private IViewport _viewport_Rect;

        private ICommonMeshBuilder _meshBuilder;

        private Vector2 _camPosition;
        private float _camZoom;

        private Vector3 _cam3DPosition;
        private Vector3 _cam3DLookAt;

        private float _rotationAngle = 0.0f;
        private const float ROTATION_SPEED = 140.0f;

        public override string ReturnWindowTitle() => "Mesh Example - Using Helpers";

        public override void OnStartup()
        {
            _camPosition = Vector2.Zero;
            _camZoom = 1.0f;

            _cam3DPosition = new Vector3(0.0f, 0.0f, 200.0f);
            _cam3DLookAt = Vector3.Zero;
        }

        public override bool CreateResources(IServices services)
        {
            _texCity = services.Surfaces.LoadTexture("city", AssetSourceEnum.Embedded);
            _texMap = services.Surfaces.LoadTexture("map", AssetSourceEnum.Embedded);
            _texDice = services.Surfaces.LoadTexture("onetosix", AssetSourceEnum.Embedded);
            _texGrid = services.Surfaces.LoadTexture("grid", AssetSourceEnum.Embedded);

            _offScreenTarget_Quad = services.Surfaces.CreateRenderTarget(960, 540);
            _offScreenTarget_CRT = services.Surfaces.CreateRenderTarget(960, 540);
            _offScreenTarget_Sphere = services.Surfaces.CreateRenderTarget(960, 540);
            _offScreenTarget_Rect = services.Surfaces.CreateRenderTarget(960, 540);

            _drawStage_Quad = services.Stages.CreateDrawStage();
            _drawStage_CRT = services.Stages.CreateDrawStage();
            _drawStage_Sphere = services.Stages.CreateDrawStage();
            _drawStage_Rect = services.Stages.CreateDrawStage();

            _viewport_Quad = services.Stages.CreateViewport(0, 0, 480, 270);
            _viewport_CRT = services.Stages.CreateViewport(480, 0, 480, 270);
            _viewport_Sphere = services.Stages.CreateViewport(0, 270, 480, 270);
            _viewport_Rect = services.Stages.CreateViewport(480, 270, 480, 270);

            _camera2D = services.Cameras.CreateCamera2D();

            _camera3D = services.Cameras.CreateCamera3D(_cam3DPosition, _cam3DLookAt, Vector3.UnitY);

            _meshStage_Quad = services.Stages.CreateMeshRenderStage();
            _meshStage_CRT = services.Stages.CreateMeshRenderStage();
            _meshStage_Sphere = services.Stages.CreateMeshRenderStage();
            _meshStage_Rect = services.Stages.CreateMeshRenderStage();

            _meshBuilder = services.Helpers.CommonMeshBuilder;

            //Directional Light with Yellow Specular highlights for the Globe
            services.Stages.SetMeshRenderLightingProperties(_meshStage_Sphere, new MeshRenderLightingPropertiesConfiguration
            {
                NumberOfActiveLights = 1,
                Shininess = 10.0f,
                SpecularColour = Colour.Yellow.ToVector3(),
            });

            services.Stages.SetMeshRenderLights(_meshStage_Sphere, new MeshRenderLightConfiguration[]
            {
                new MeshRenderLightConfiguration
                {
                     LightType =  LightType.Directional,
                     Position = new Vector3(-1.0f, -1.0f, 0.0f),
                     AmbientCoefficient = 0.1f,
                     Attenuation = 0.0f,
                     Colour = Colour.White.ToVector3(),
                     ConeAngle = 0.0f,
                     ConeDirection = Vector3.Zero
                }
            });

            //Directional Light (all white) for CRT surface
            services.Stages.SetMeshRenderLightingProperties(_meshStage_CRT, new MeshRenderLightingPropertiesConfiguration
            {
                NumberOfActiveLights = 1,
                Shininess = 10.0f,
                SpecularColour = Colour.White.ToVector3()
            });

            services.Stages.SetMeshRenderLights(_meshStage_CRT, new MeshRenderLightConfiguration[]
            {
                new MeshRenderLightConfiguration
                {
                     LightType =  LightType.Directional,
                     Position = new Vector3(-1.0f, -1.0f, -1.0f),
                     AmbientCoefficient = 0.1f,
                     Attenuation = 0.0f,
                     Colour = Colour.White.ToVector3(),
                     ConeAngle = 0.0f,
                     ConeDirection = Vector3.Zero
                }
            });

            //Spotlight for Quad Surface
            services.Stages.SetMeshRenderLightingProperties(_meshStage_Quad, new MeshRenderLightingPropertiesConfiguration
            {
                NumberOfActiveLights = 1,
                Shininess = 0.0f,
                SpecularColour = Colour.White.ToVector3()
            });

            services.Stages.SetMeshRenderLights(_meshStage_Quad, new MeshRenderLightConfiguration[]
            {
                new MeshRenderLightConfiguration
                {
                     LightType =  LightType.Spotlight,
                     Position = new Vector3(0.0f, 0.0f, 200.0f),
                     AmbientCoefficient = 0.1f,
                     Attenuation = 0.0f,
                     Colour = Colour.White.ToVector3(),
                     ConeAngle = 15.0f,
                     ConeDirection = -Vector3.UnitZ
                }
            });

            //Cube uses default face on light

            return true;
        }

        public override bool Update_(IServices services, float timeSinceLastUpdateSeconds) => true;

        public override void PreDrawing(IServices services, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            services.Cameras.SetCamera2DFocusAndZoom(_camera2D, _camPosition, _camZoom);

            services.Cameras.SetCamera3DProjection(_camera3D, 75, 960.0f / 540.0f, 10.0f, 1000.0f);
            services.Cameras.SetCamera3DView(_camera3D, _cam3DPosition, _cam3DLookAt, Vector3.UnitY);

            _rotationAngle += ROTATION_SPEED * timeSinceLastDrawSeconds;
            while (_rotationAngle > 360)
            {
                _rotationAngle -= 360;
            }

            var meshQuad = _meshBuilder.CreateQuadMesh(320, 180);
            var meshCRT = _meshBuilder.CreateCrtMesh(320, 180, 128, 0.1f, 0.5f);
            var meshSphere = _meshBuilder.CreateSphericalMesh(Vector3.Zero, 150.0f, 150.0f, 150.0f, _rotationAngle, 64, 64);
            var meshRect = _meshBuilder.CreateRectangularCuboidMesh(new Vector3(-150.0f, -100.0f, -100.0f), 100.0f, 100.0f, 100.0f, _rotationAngle);

            services.Stages.SetMeshRenderMesh(_meshStage_Quad, meshQuad);
            services.Stages.SetMeshRenderMesh(_meshStage_CRT, meshCRT);
            services.Stages.SetMeshRenderMesh(_meshStage_Sphere, meshSphere);
            services.Stages.SetMeshRenderMesh(_meshStage_Rect, meshRect);
        }

        public override void Drawing(IDrawing drawing, IFps fps, IInput input, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            drawing.ClearDynamicDrawRequestQueue(_drawStage_Quad);
            drawing.ClearDynamicDrawRequestQueue(_drawStage_CRT);
            drawing.ClearDynamicDrawRequestQueue(_drawStage_Sphere);
            drawing.ClearDynamicDrawRequestQueue(_drawStage_Rect);

            var tools = drawing.DrawingHelpers;

            tools.DrawTexturedQuad(_drawStage_Quad, CoordinateSpace.Screen, _texGrid, Colour.White, Vector2.Zero, 960, 540, 0.5f);
            tools.DrawTexturedQuad(_drawStage_CRT, CoordinateSpace.Screen, _texCity, Colour.White, Vector2.Zero, 960, 540, 0.5f);
            tools.DrawTexturedQuad(_drawStage_Sphere, CoordinateSpace.Screen, _texMap, Colour.White, Vector2.Zero, 960, 540, 0.5f);
            tools.DrawTexturedQuad(_drawStage_Rect, CoordinateSpace.Screen, _texDice, Colour.White, Vector2.Zero, 960, 540, 0.5f);
        }

        public override void Rendering(IRenderQueue queue)
        {
            queue.ClearColour(WindowRenderTarget, Colour.Clear);
            queue.ClearDepth(WindowRenderTarget);

            queue.RemoveViewport();

            queue.ClearColour(_offScreenTarget_Quad, new Colour(0.5f, 0.5f, 0.3f, 1.0f));
            queue.ClearDepth(_offScreenTarget_Quad);
            queue.ClearColour(_offScreenTarget_CRT, Colour.Red);
            queue.ClearDepth(_offScreenTarget_CRT);
            queue.ClearColour(_offScreenTarget_Sphere, Colour.White);
            queue.ClearDepth(_offScreenTarget_Sphere);
            queue.ClearColour(_offScreenTarget_Rect, Colour.White);
            queue.ClearDepth(_offScreenTarget_Rect);

            var textureMeshes = true;
            if (textureMeshes)
            {
                queue.Draw(_drawStage_Quad, _camera2D, _offScreenTarget_Quad);
                queue.Draw(_drawStage_CRT, _camera2D, _offScreenTarget_CRT);
                queue.Draw(_drawStage_Sphere, _camera2D, _offScreenTarget_Sphere);
                queue.Draw(_drawStage_Rect, _camera2D, _offScreenTarget_Rect);
            }
            queue.SetViewport(_viewport_Quad);
            queue.MeshRender(_meshStage_Quad, _camera3D, _offScreenTarget_Quad, WindowRenderTarget);

            queue.SetViewport(_viewport_CRT);
            queue.MeshRender(_meshStage_CRT, _camera3D, _offScreenTarget_CRT, WindowRenderTarget);

            queue.SetViewport(_viewport_Sphere);
            queue.MeshRender(_meshStage_Sphere, _camera3D, _offScreenTarget_Sphere, WindowRenderTarget);

            queue.SetViewport(_viewport_Rect);
            queue.MeshRender(_meshStage_Rect, _camera3D, _offScreenTarget_Rect, WindowRenderTarget);
        }

        public override void Shutdown() { }
    }
}
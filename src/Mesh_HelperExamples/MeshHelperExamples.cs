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

        public override bool CreateResources(IServices yak)
        {
            _texCity = yak.Surfaces.LoadTexture("city", AssetSourceEnum.Embedded);
            _texMap = yak.Surfaces.LoadTexture("map", AssetSourceEnum.Embedded);
            _texDice = yak.Surfaces.LoadTexture("onetosix", AssetSourceEnum.Embedded);
            _texGrid = yak.Surfaces.LoadTexture("grid", AssetSourceEnum.Embedded);

            _offScreenTarget_Quad = yak.Surfaces.CreateRenderTarget(960, 540);
            _offScreenTarget_CRT = yak.Surfaces.CreateRenderTarget(960, 540);
            _offScreenTarget_Sphere = yak.Surfaces.CreateRenderTarget(960, 540);
            _offScreenTarget_Rect = yak.Surfaces.CreateRenderTarget(960, 540);

            _drawStage_Quad = yak.Stages.CreateDrawStage();
            _drawStage_CRT = yak.Stages.CreateDrawStage();
            _drawStage_Sphere = yak.Stages.CreateDrawStage();
            _drawStage_Rect = yak.Stages.CreateDrawStage();

            _viewport_Quad = yak.Stages.CreateViewport(0, 0, 480, 270);
            _viewport_CRT = yak.Stages.CreateViewport(480, 0, 480, 270);
            _viewport_Sphere = yak.Stages.CreateViewport(0, 270, 480, 270);
            _viewport_Rect = yak.Stages.CreateViewport(480, 270, 480, 270);

            _camera2D = yak.Cameras.CreateCamera2D();

            _camera3D = yak.Cameras.CreateCamera3D(_cam3DPosition, _cam3DLookAt, Vector3.UnitY);

            _meshStage_Quad = yak.Stages.CreateMeshRenderStage();
            _meshStage_CRT = yak.Stages.CreateMeshRenderStage();
            _meshStage_Sphere = yak.Stages.CreateMeshRenderStage();
            _meshStage_Rect = yak.Stages.CreateMeshRenderStage();

            _meshBuilder = yak.Helpers.CommonMeshBuilder;

            //Directional Light with Yellow Specular highlights for the Globe
            yak.Stages.SetMeshRenderLightingProperties(_meshStage_Sphere, new MeshRenderLightingPropertiesConfiguration
            {
                NumberOfActiveLights = 1,
                Shininess = 10.0f,
                SpecularColour = Colour.Yellow.ToVector3(),
            });

            yak.Stages.SetMeshRenderLights(_meshStage_Sphere, new MeshRenderLightConfiguration[]
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
            yak.Stages.SetMeshRenderLightingProperties(_meshStage_CRT, new MeshRenderLightingPropertiesConfiguration
            {
                NumberOfActiveLights = 1,
                Shininess = 10.0f,
                SpecularColour = Colour.White.ToVector3()
            });

            yak.Stages.SetMeshRenderLights(_meshStage_CRT, new MeshRenderLightConfiguration[]
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
            yak.Stages.SetMeshRenderLightingProperties(_meshStage_Quad, new MeshRenderLightingPropertiesConfiguration
            {
                NumberOfActiveLights = 1,
                Shininess = 0.0f,
                SpecularColour = Colour.White.ToVector3()
            });

            yak.Stages.SetMeshRenderLights(_meshStage_Quad, new MeshRenderLightConfiguration[]
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

        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds) => true;

        public override void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            yak.Cameras.SetCamera2DFocusAndZoom(_camera2D, _camPosition, _camZoom);

            yak.Cameras.SetCamera3DProjection(_camera3D, 75, 960.0f / 540.0f, 10.0f, 1000.0f);
            yak.Cameras.SetCamera3DView(_camera3D, _cam3DPosition, _cam3DLookAt, Vector3.UnitY);

            _rotationAngle += ROTATION_SPEED * timeSinceLastDrawSeconds;
            while (_rotationAngle > 360)
            {
                _rotationAngle -= 360;
            }

            var meshQuad = _meshBuilder.CreateQuadMesh(320, 180);
            var meshCRT = _meshBuilder.CreateCrtMesh(320, 180, 128, 0.1f, 0.5f);
            var meshSphere = _meshBuilder.CreateSphericalMesh(Vector3.Zero, 150.0f, 150.0f, 150.0f, _rotationAngle, 64, 64);
            var meshRect = _meshBuilder.CreateRectangularCuboidMesh(new Vector3(-150.0f, -100.0f, -100.0f), 100.0f, 100.0f, 100.0f, _rotationAngle);

            yak.Stages.SetMeshRenderMesh(_meshStage_Quad, meshQuad);
            yak.Stages.SetMeshRenderMesh(_meshStage_CRT, meshCRT);
            yak.Stages.SetMeshRenderMesh(_meshStage_Sphere, meshSphere);
            yak.Stages.SetMeshRenderMesh(_meshStage_Rect, meshRect);
        }

        public override void Drawing(IDrawing draw,
                                     IFps fps,
                                     IInput input,
                                     ICoordinateTransforms transforms,
                                     float timeSinceLastDrawSeconds,
                                     float timeSinceLastUpdateSeconds)
        {
            draw.ClearDynamicDrawRequestQueue(_drawStage_Quad);
            draw.ClearDynamicDrawRequestQueue(_drawStage_CRT);
            draw.ClearDynamicDrawRequestQueue(_drawStage_Sphere);
            draw.ClearDynamicDrawRequestQueue(_drawStage_Rect);

            var tools = draw.Helpers;

            tools.DrawTexturedQuad(_drawStage_Quad, CoordinateSpace.Screen, _texGrid, Colour.White, Vector2.Zero, 960, 540, 0.5f);
            tools.DrawTexturedQuad(_drawStage_CRT, CoordinateSpace.Screen, _texCity, Colour.White, Vector2.Zero, 960, 540, 0.5f);
            tools.DrawTexturedQuad(_drawStage_Sphere, CoordinateSpace.Screen, _texMap, Colour.White, Vector2.Zero, 960, 540, 0.5f);
            tools.DrawTexturedQuad(_drawStage_Rect, CoordinateSpace.Screen, _texDice, Colour.White, Vector2.Zero, 960, 540, 0.5f);
        }

        public override void Rendering(IRenderQueue q, IRenderTarget windowRenderTarget)
        {
            q.ClearColour(windowRenderTarget, Colour.Clear);
            q.ClearDepth(windowRenderTarget);

            q.RemoveViewport();

            q.ClearColour(_offScreenTarget_Quad, new Colour(0.5f, 0.5f, 0.3f, 1.0f));
            q.ClearDepth(_offScreenTarget_Quad);
            q.ClearColour(_offScreenTarget_CRT, Colour.Red);
            q.ClearDepth(_offScreenTarget_CRT);
            q.ClearColour(_offScreenTarget_Sphere, Colour.White);
            q.ClearDepth(_offScreenTarget_Sphere);
            q.ClearColour(_offScreenTarget_Rect, Colour.White);
            q.ClearDepth(_offScreenTarget_Rect);

            var textureMeshes = true;
            if (textureMeshes)
            {
                q.Draw(_drawStage_Quad, _camera2D, _offScreenTarget_Quad);
                q.Draw(_drawStage_CRT, _camera2D, _offScreenTarget_CRT);
                q.Draw(_drawStage_Sphere, _camera2D, _offScreenTarget_Sphere);
                q.Draw(_drawStage_Rect, _camera2D, _offScreenTarget_Rect);
            }
            q.SetViewport(_viewport_Quad);
            q.MeshRender(_meshStage_Quad, _camera3D, _offScreenTarget_Quad, windowRenderTarget);

            q.SetViewport(_viewport_CRT);
            q.MeshRender(_meshStage_CRT, _camera3D, _offScreenTarget_CRT, windowRenderTarget);

            q.SetViewport(_viewport_Sphere);
            q.MeshRender(_meshStage_Sphere, _camera3D, _offScreenTarget_Sphere, windowRenderTarget);

            q.SetViewport(_viewport_Rect);
            q.MeshRender(_meshStage_Rect, _camera3D, _offScreenTarget_Rect, windowRenderTarget);
        }

        public override void Shutdown() { }
    }
}
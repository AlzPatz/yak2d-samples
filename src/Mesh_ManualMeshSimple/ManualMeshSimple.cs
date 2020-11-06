using System.Numerics;
using Yak2D;
using SampleBase;
using System.Collections.Generic;

namespace Mesh_ManualMeshSimple
{
    public class ManualMeshSimple : ApplicationBase
    {
        private ITexture _texFlag;
        private ICamera3D _camera3D;
        private IMeshRenderStage _meshStage;

        private Vector3 _cam3DPosition;
        private Vector3 _cam3DLookAt;

        public override string ReturnWindowTitle() => "Mesh Example - Manual Model Creation";

        public override void OnStartup()
        {
            //Framework uses a Right Handed Coordinate System in 3D (x positive to the right, y positive upwards, z positive towards camera)
            _cam3DPosition = new Vector3(0.0f, 0.0f, 200.0f);
            _cam3DLookAt = Vector3.Zero;
        }

        public override bool CreateResources(IServices yak)
        {
            _texFlag = yak.Surfaces.LoadTexture("pirate-flag", AssetSourceEnum.Embedded);

            _camera3D = yak.Cameras.CreateCamera3D(_cam3DPosition, _cam3DLookAt, Vector3.UnitY);

            _meshStage = yak.Stages.CreateMeshRenderStage();

            yak.Stages.SetMeshRenderLightingProperties(_meshStage, new MeshRenderLightingPropertiesConfiguration
            {
                NumberOfActiveLights = 1,
                Shininess = 10.0f,
                SpecularColour = Colour.White.ToVector3(),
            });

            yak.Stages.SetMeshRenderLights(_meshStage, new MeshRenderLightConfiguration[]
            {
                new MeshRenderLightConfiguration
                {
                     LightType =  LightType.Directional,
                     Position = new Vector3(-1.0f, 0.0f, -1.0f),
                     AmbientCoefficient = 0.1f,
                     Attenuation = 0.0f,
                     Colour = Colour.White.ToVector3(),
                     ConeAngle = 0.0f,
                     ConeDirection = Vector3.Zero
                }
            });

            var mesh = BuildMesh();

            yak.Stages.SetMeshRenderMesh(_meshStage, mesh);

            return true;
        }

        private Vertex3D[] BuildMesh()
        {
            var width = 360.0f;
            var height = 200.0f;

            var hw = 0.5f * width;
            var hh = 0.5f * height;

            //Define four corners of quad
            var vertices = new Vertex3D[]
            {
               new Vertex3D
               {
                   Position = new Vector3(-hw, hh, 0.0f),
                   Normal = Vector3.UnitZ,
                   TexCoord = new Vector2(0.0f, 0.0f)
               },
               new Vertex3D
               {
                   Position = new Vector3(hw, hh, 0.0f),
                   Normal = Vector3.UnitZ,
                   TexCoord = new Vector2(1.0f, 0.0f)
               },
               new Vertex3D
               {
                   Position = new Vector3(-hw, -hh, 0.0f),
                   Normal = Vector3.UnitZ,
                   TexCoord = new Vector2(0.0f, 1.0f)
               },
               new Vertex3D
               {
                   Position = new Vector3(hw, -hh, 0.0f),
                   Normal = Vector3.UnitZ,
                   TexCoord = new Vector2(1.0f, 1.0f)
               }
            };

            //Meshes are triangle lists, no indexing, so a bit inefficient, but this is not a 3D engine..
            var mesh = new List<Vertex3D>();

            mesh.Add(vertices[0]);
            mesh.Add(vertices[1]);
            mesh.Add(vertices[2]);
            mesh.Add(vertices[2]);
            mesh.Add(vertices[1]);
            mesh.Add(vertices[3]);

            return mesh.ToArray();
        }

        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds) => true;

        public override void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Drawing(IDrawing draw, IFps fps, IInput input, ICoordinateTransforms transforms, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Rendering(IRenderQueue q, IRenderTarget windowRenderTarget)
        {
            q.ClearColour(windowRenderTarget, Colour.DarkGray);
            q.ClearDepth(windowRenderTarget);

            q.MeshRender(_meshStage, _camera3D, _texFlag, windowRenderTarget);
        }

        public override void Shutdown() { }
    }
}
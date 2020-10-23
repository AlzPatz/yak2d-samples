using System.Numerics;
using Yak2D;
using SampleBase;
using System;

namespace Mesh_ManualChangingMesh
{
    public class ManualMesh : ApplicationBase
    {
        private const float DURATION = 10.0f;
        private float _timecount = 0.0f;

        private ITexture _texFlag;
        private ICamera3D _camera3D;
        private IMeshRenderStage _meshStage;

        private Vector3 _cam3DPosition;
        private Vector3 _cam3DLookAt;

        public override string ReturnWindowTitle() => "Mesh Example - Manual Model Creation";

        public override void OnStartup()
        {
            _cam3DPosition = new Vector3(0.0f, 0.0f, 200.0f);
            _cam3DLookAt = Vector3.Zero;
        }

        public override bool CreateResources(IServices services)
        {
            _texFlag = services.Surfaces.LoadTexture("pirate-flag", AssetSourceEnum.Embedded);

            _camera3D = services.Cameras.CreateCamera3D(_cam3DPosition, _cam3DLookAt, Vector3.UnitY);

            _meshStage = services.Stages.CreateMeshRenderStage();

            services.Stages.SetMeshRenderLightingProperties(_meshStage, new MeshRenderLightingPropertiesConfiguration
            {
                NumberOfActiveLights = 1,
                Shininess = 10.0f,
                SpecularColour = Colour.White.ToVector3(),
            });

            services.Stages.SetMeshRenderLights(_meshStage, new MeshRenderLightConfiguration[]
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

            return true;
        }

        public override bool Update_(IServices services, float timeSinceLastUpdateSeconds) => true;

        public override void PreDrawing(IServices services, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            _timecount += timeSinceLastDrawSeconds;

            while (_timecount > DURATION)
            {
                _timecount -= DURATION;
            }

            var fraction = _timecount / DURATION;

            services.Cameras.SetCamera3DProjection(_camera3D, 75, 960.0f / 540.0f, 10.0f, 1000.0f);
            services.Cameras.SetCamera3DView(_camera3D, _cam3DPosition, _cam3DLookAt, Vector3.UnitY);

            var mesh = BuildFlagMesh(fraction);

            services.Stages.SetMeshRenderMesh(_meshStage, mesh);
        }

        private Vertex3D[] BuildFlagMesh(float frac)
        {
            //Compeletly freestyling this approach - if it looks bad, well so be it. I'm not hear to teach you how to render realistic flags

            //Flag boundaires, along z, width 360 ish , height 200 ish

            //Flag pinned by left edge, riddled with sinwaves, dampened to zero on at the left edge
            //Distances between points in x,y plane deformed by actual distance between neighbour (last point) in 3d space
            //First a middle seam is calculated where positional deformation on cares about the last point on the line
            //For other points takes into account distance to nearest point towards middle in vertical direction

            var numVertsHorizontal = 512;
            var numVertsVertical = 285; //Make it odd to give a middle seam

            //Force it
            if(numVertsVertical % 2 ==0)
            {
                numVertsVertical++;
            }

            var width = 360.0f;
            var height = 200.0f;

            var cx = width / (1.0f * numVertsHorizontal);
            var cy = height / (1.0f * numVertsVertical);

            var leftEdge = -0.5f * width;

            var vPositions = new Vector3[numVertsHorizontal, numVertsVertical];

            //Run through the middle seam of points first
            var mid = numVertsVertical / 2; //integer division relies on rounding. 9 /2 = 4 which is middle seem in [0 - 8] array
            var px = leftEdge;
            var py = 0.0f; //y == 0 line
            for (var x = 0; x < numVertsHorizontal; x++)
            {
                if (x == 0)
                {
                    vPositions[0, mid] = new Vector3(leftEdge, py, 0.0f);
                    continue;
                }

                var last = vPositions[x - 1, mid];

                var xTheoretical = x * cx;
                var yTheoretical = 0.0f;

                var pz = CalculateWavesSummedHeights(frac, width, height, xTheoretical, yTheoretical);

                var xShift = CalculateXShift(cx, last.Z, pz);

                px += xShift;

                vPositions[x, mid] = new Vector3(px, py, pz);
            }

            for (var y = 1; y < (numVertsVertical / 2) + 1; y++)
            {
                var top = mid - y;
                var bottom = mid + y;

                for (var x = 0; x < numVertsHorizontal; x++)
                {
                    var yTheoreticalT = y * cy;
                    var yTheoreticalB = -y * cy;

                    if (x == 0)
                    {
                        vPositions[0, top] = new Vector3(leftEdge, yTheoreticalT, 0.0f);
                        vPositions[0, bottom] = new Vector3(leftEdge, yTheoreticalB, 0.0f);
                        continue;
                    }

                    var top_last_vert = vPositions[x, top + 1];
                    var top_last_hori = vPositions[x - 1, top];
                    var bottom_last_vert = vPositions[x, bottom - 1];
                    var bottom_last_hori = vPositions[x - 1, bottom];

                    var xTheoretical = x * cx;

                    var pzt = CalculateWavesSummedHeights(frac, width, height, xTheoretical, yTheoreticalT);
                    var pzb = CalculateWavesSummedHeights(frac, width, height, xTheoretical, yTheoreticalB);

                    var shiftT = CalculateShiftFromLastHori(top_last_vert, top_last_hori, xTheoretical, yTheoreticalT, pzt);
                    var shiftB = CalculateShiftFromLastHori(bottom_last_vert, bottom_last_hori, xTheoretical, yTheoreticalB, pzb);

                    vPositions[x, top] = top_last_vert + new Vector3(shiftT, 0.0f);
                    vPositions[x, top].Z = pzt;
                    vPositions[x, bottom] = bottom_last_vert + new Vector3(shiftB, 0.0f);
                    vPositions[x, bottom].Z = pzb; 
                }
            }

            //Generate TexCoords (just un modified quad)
            var tx = 1.0f / (1.0f * numVertsHorizontal);
            var ty = 1.0f / (1.0f * numVertsVertical);

            var texCoords = new Vector2[numVertsHorizontal, numVertsVertical];
            for(var y = 0; y < numVertsVertical; y++)
            {
                for(var x = 0; x < numVertsHorizontal; x++)
                {
                    texCoords[x, y] = new Vector2(tx * x, ty * y);
                }
            }

            //Generate Normals 
            var normals = new Vector3[numVertsHorizontal, numVertsVertical];
            for (var y = 0; y < numVertsVertical; y++)
            {
                for (var x = 0; x < numVertsHorizontal; x++)
                {
                    
                }
            }


            //HERE unwrap grid, work out normals, etc, make mesh

            //Mesh should be a triangle list of vertices



            return null;
        }

        private float CalculateWavesSummedHeights(float frac, float width, float height, float xTheoreticalGridPosition, float yTheoreticalGridPosition)
        {
            throw new NotImplementedException();
        }

        private float CalculateXShift(float grid_x_size, float z_last, float z)
        {
            throw new NotImplementedException();
        }

        private Vector2 CalculateShiftFromLastHori(Vector3 last_vertical, Vector3 last_horizontal, float xTheoreticalGridPosition, float yTheoreticalGridPosition, float z)
        {
            throw new NotImplementedException();
        }

        public override void Drawing(IDrawing drawing, IFps fps, IInput input, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Rendering(IRenderQueue queue)
        {
            queue.ClearColour(WindowRenderTarget, Colour.DarkGray);
            queue.ClearDepth(WindowRenderTarget);

            queue.MeshRender(_meshStage, _camera3D, _texFlag, WindowRenderTarget);
        }

        public override void Shutdown() { }
    }
}
using System;
using System.Numerics;
using System.Collections.Generic;
using SampleBase;
using Yak2D;

namespace Mesh_ManualChangingMesh
{
    /// <summary>
    /// This got a little too complex and a touch hacky to clearly explain a pretty simple feature
    /// Anyway - a waving flag!
    /// </summary>
    public class ManualMesh : ApplicationBase
    {
        private class QuickWave
        {
            public float Amplitude { get; set; }
            public float FractionalOffset { get; set; }
            public float WaveLength { get; set; }
            public int Cycles { get; set; }
            public bool Reverse { get; set; }
            public bool Vertical { get; set; }

            public float Value(float frac, float xpos, float ypos)
            {
                var positional = Vertical ? ypos / WaveLength : xpos / WaveLength;
                positional = Reverse ? -positional : positional;
                var f = frac + FractionalOffset + positional;
                var o = f * Math.PI * 2.0f * Cycles;
                var val = Amplitude * (float)Math.Sin(o);
                return val;
            }
        }

        private const float DURATION = 0.7f;
        private float _timecount = 0.0f;

        private ITexture _texFlag;
        private ICamera3D _camera3D;
        private IMeshRenderStage _meshStage;

        private Vector3 _cam3DPosition;
        private Vector3 _cam3DLookAt;
        private List<QuickWave> _waves;

        public override string ReturnWindowTitle() => "Mesh Example - Manual Evolving Model Creation";

        public override void OnStartup()
        {
            //Framework uses a Right Handed Coordinate System in 3D (x positive to the right, y positive upwards, z positive towards camera)
            _cam3DPosition = new Vector3(-80.0f, 100.0f, 230.0f);
            _cam3DLookAt = Vector3.Zero;

            _waves = new List<QuickWave>()
            {
                new QuickWave
                {
                     Amplitude = 20.0f,
                     Cycles = 1,
                     FractionalOffset= 0.0f,
                     Reverse = true,
                     Vertical = false,
                     WaveLength = 360.0f
                },
                new QuickWave
                {
                     Amplitude = 12.0f,
                     Cycles = 2,
                     FractionalOffset= 0.5f,
                     Reverse = true,
                     Vertical = false,
                     WaveLength = 310.0f
                },
                new QuickWave
                {
                     Amplitude = 17.0f,
                     Cycles = 1,
                     FractionalOffset= 0.0f,
                     Reverse = true,
                     Vertical = true,
                     WaveLength = 200.0f
                },
                new QuickWave
                {
                     Amplitude = 11.0f,
                     Cycles = 2,
                     FractionalOffset= 0.8f,
                     Reverse = false,
                     Vertical = true,
                     WaveLength = 250.0f
                },
            };
        }

        public override bool CreateResources(IServices yak)
        {
            _texFlag = yak.Surfaces.LoadTexture("pirate-flag", AssetSourceEnum.Embedded);

            _camera3D = yak.Cameras.CreateCamera3D(_cam3DPosition, _cam3DLookAt, Vector3.UnitY);

            _meshStage = yak.Stages.CreateMeshRenderStage();

            yak.Stages.SetMeshRenderLightingProperties(_meshStage, new MeshRenderLightingPropertiesConfiguration
            {
                NumberOfActiveLights = 1,
                Shininess = 8.0f,
                SpecularColour = 0.3f * Colour.White.ToVector3(),
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

            return true;
        }

        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds) => true;

        public override void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            _timecount += timeSinceLastDrawSeconds;

            while (_timecount > DURATION)
            {
                _timecount -= DURATION;
            }

            var fraction = _timecount / DURATION;

            yak.Cameras.SetCamera3DProjection(_camera3D, 75, 960.0f / 540.0f, 10.0f, 1000.0f);
            yak.Cameras.SetCamera3DView(_camera3D, _cam3DPosition, _cam3DLookAt, Vector3.UnitY);

            var mesh = BuildFlagMesh(fraction);

            yak.Stages.SetMeshRenderMesh(_meshStage, mesh);
        }

        private Vertex3D[] BuildFlagMesh(float frac)
        {
            //Compeletly freestyling this approach - if it looks bad, well so be it. I'm not here to teach you how to render realistic flags

            //Flag boundaires, along z, width 360 ish , height 200 ish

            //Flag pinned by left edge, riddled with sinwaves, dampened to zero on at the left edge
            //Distances between points in x,y plane deformed by actual distance between neighbour (last point) in 3d space
            //First a middle seam is calculated where positional deformation on cares about the last point on the line
            //For other points takes into account distance to nearest point towards middle in vertical direction

            var numVertsHorizontal = 192; // 512;
            var numVertsVertical = 95; //285; //Make it odd to give a middle seam

            //Force it
            if (numVertsVertical % 2 == 0)
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

                var xTheoretical = leftEdge + (x * cx);
                var yTheoretical = 0.0f;

                var pz = CalculateWavesSummedHeights(frac, width, height, xTheoretical, yTheoretical, leftEdge);

                var xShift = CalculateLinearXShift(cx, last.Z, pz);

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

                    var xTheoretical = leftEdge + (x * cx);

                    var pzt = CalculateWavesSummedHeights(frac, width, height, xTheoretical, yTheoreticalT, leftEdge);
                    var pzb = CalculateWavesSummedHeights(frac, width, height, xTheoretical, yTheoreticalB, leftEdge);

                    var shiftT = CalculateShiftFromLastHori(cx, cy, top_last_vert, top_last_hori, xTheoretical, yTheoreticalT, pzt);
                    var shiftB = CalculateShiftFromLastHori(cx, cy, bottom_last_vert, bottom_last_hori, xTheoretical, yTheoreticalB, pzb);

                    vPositions[x, top] = top_last_hori + new Vector3(shiftT, 0.0f);
                    vPositions[x, top].Z = pzt;
                    vPositions[x, bottom] = bottom_last_hori + new Vector3(shiftB, 0.0f);
                    vPositions[x, bottom].Z = pzb;
                }
            }

            //Hack Fix - as we end up with slightly difference distance scaling across x for the linear middle and the top / bottoms
            for (var x = 0; x < numVertsHorizontal; x++)
            {
                var curr = vPositions[x, mid];
                var top = vPositions[x, mid + 1];
                var bottom = vPositions[x, mid - 1];

                vPositions[x, mid] = new Vector3(0.5f * (top.X + bottom.X), curr.Y, curr.Z);
            }

            //Generate TexCoords (just un modified quad)
            var tx = 1.0f / (1.0f * numVertsHorizontal);
            var ty = 1.0f / (1.0f * numVertsVertical);

            var texCoords = new Vector2[numVertsHorizontal, numVertsVertical];
            for (var y = 0; y < numVertsVertical; y++)
            {
                for (var x = 0; x < numVertsHorizontal; x++)
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
                    //Finite difference method (seemed interesting as doesnt use x prod. but also does use current point for calc
                    //Let's see what it looks like
                    //https://stackoverflow.com/questions/13983189/opengl-how-to-calculate-normals-in-a-terrain-height-grid

                    float hL = 0.0f, hR = 0.0f, hU = 0.0f, hD = 0.0f;

                    if (x > 0)
                    {
                        hL = vPositions[x - 1, y].Z;
                    }

                    if (x < numVertsHorizontal - 1)
                    {
                        hR = vPositions[x + 1, y].Z;
                    }

                    if (y > 0)
                    {
                        hU = vPositions[x, y - 1].Z;
                    }

                    if (y < numVertsVertical - 1)
                    {
                        hD = vPositions[x, y + 1].Z;
                    }

                    var h = vPositions[x, y].Z;

                    if (x == 0)
                    {
                        hL = h - (h - hR);
                    }

                    if (x == numVertsHorizontal - 1)
                    {
                        hR = h - (h - hL);
                    }

                    if (y == 0)
                    {
                        hU = h - (h - hD);
                    }

                    if (y == numVertsVertical - 1)
                    {
                        hD = h - (h - hU);
                    }

                    var nX = hL - hR;
                    var nY = hD - hU;
                    var nZ = 2.0f;

                    var n = new Vector3(nX, nY, nZ);
                    normals[x, y] = Vector3.Normalize(n);
                }
            }

            //Mesh should be a tirangle list of vertices

            var vertices = new List<Vertex3D>();

            for (var y0 = 0; y0 < numVertsVertical - 1; y0++)
            {
                for (var x0 = 0; x0 < numVertsHorizontal - 1; x0++)
                {
                    var y1 = y0 + 1;
                    var x1 = x0 + 1;

                    vertices.Add(new Vertex3D { Position = vPositions[x0, y0], Normal = normals[x0, y0], TexCoord = texCoords[x0, y0] });
                    vertices.Add(new Vertex3D { Position = vPositions[x1, y0], Normal = normals[x1, y0], TexCoord = texCoords[x1, y0] });
                    vertices.Add(new Vertex3D { Position = vPositions[x0, y1], Normal = normals[x0, y1], TexCoord = texCoords[x0, y1] });

                    vertices.Add(new Vertex3D { Position = vPositions[x0, y1], Normal = normals[x0, y1], TexCoord = texCoords[x0, y1] });
                    vertices.Add(new Vertex3D { Position = vPositions[x1, y0], Normal = normals[x1, y0], TexCoord = texCoords[x1, y0] });
                    vertices.Add(new Vertex3D { Position = vPositions[x1, y1], Normal = normals[x1, y1], TexCoord = texCoords[x1, y1] });
                }
            }

            return vertices.ToArray();
        }

        private float CalculateWavesSummedHeights(float frac, float width, float height, float xTheoreticalGridPosition, float yTheoreticalGridPosition, float leftEdge)
        {
            var fracX = (xTheoreticalGridPosition - leftEdge) / width; //Overall Scaler to ensure no depth change at "flag pole"

            var val = 0.0f;

            _waves.ForEach(w =>
            {
                val += w.Value(frac, xTheoreticalGridPosition, yTheoreticalGridPosition);
            });

            return fracX * val;
        }

        private float CalculateLinearXShift(float cell_width, float z_last, float z)
        {
            //Scale horizontal distance between points based on assuming a fixed distance in 3d space to those of different z heights
            var dz = z_last - z;
            var dis = (float)Math.Sqrt((cell_width * cell_width) + (dz * dz));
            return cell_width * (cell_width / dis);
        }

        private Vector2 CalculateShiftFromLastHori(float cx, float cy, Vector3 last_vertical, Vector3 last_horizontal, float xTheoreticalGridPosition, float yTheoreticalGridPosition, float z)
        {
            //Bit of a mushy way to try and constrain the size of the cell based on adjoining points. Let's see how this works out
            var opposite = new Vector3(last_horizontal.X, last_vertical.Y, 0.5f * (last_vertical.Z + last_horizontal.Z));
            var point = new Vector3(xTheoreticalGridPosition, yTheoreticalGridPosition, z);

            var dx = point.X - opposite.X;
            var dy = point.Y - opposite.Y;
            var dz = point.Z - opposite.Z;

            var hScale = cx / (float)Math.Sqrt((dx * dx) + (dz * dz));
            var vScale = cy / (float)Math.Sqrt((dy * dy) + (dz * dz));
            var scaleXY = new Vector2(hScale, vScale);

            var pointXY = new Vector2(point.X, point.Y);
            var oppXY = new Vector2(opposite.X, opposite.Y);
            var oppToPointXY = pointXY - oppXY;

            var pointScaledXY = oppXY + (oppToPointXY * scaleXY);

            var shiftFromLastHori = pointScaledXY - new Vector2(last_horizontal.X, last_horizontal.Y);

            return shiftFromLastHori;
        }

        public override void Drawing(IDrawing draw,
                                     IFps fps,
                                     IInput input,
                                     ICoordinateTransforms transforms,
                                     float timeSinceLastDrawSeconds,
                                     float timeSinceLastUpdateSeconds)
        { }

        public override void Rendering(IRenderQueue q, IRenderTarget windowRenderTarget)
        {
            q.ClearColour(windowRenderTarget, Colour.DarkGray);
            q.ClearDepth(windowRenderTarget);

            q.MeshRender(_meshStage, _camera3D, _texFlag, windowRenderTarget);
        }

        public override void Shutdown() { }
    }
}
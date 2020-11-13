using System;
using System.Numerics;
using System.Collections.Generic;
using Yak2D;
using SampleBase;

namespace GpuToCpu_SurfaceCopyFloat32
{
    /// <summary>
    /// 
    /// </summary>
    public class GpuToCpuFloat32Copy : ApplicationBase
    {
        private const float MESH_HEIGHT_AMPLITUDE = 256.0f;
        private const float PIXEL_SIZE_SCALAR = 10.0f;

        private ITexture _float32Texture;
        private ISurfaceCopyStage _gpuToCpuCopyStage;
        private IMeshRenderStage _meshStage;
        private ICamera3D _camera3D;
        private ITexture _whiteTexture;

        private Vector3 _cam3DPositionStart;
        private Vector3 _cam3DPositionEnd;
        private Vector3 _cam3DLookAt;

        private bool _first;
        private bool _meshDataReady;

        private const float DURATION = 5.0f;
        private float _count = 0.0f;

        public override string ReturnWindowTitle() => "Gpu to Cpu Surface Copy - Float32";

        public override void OnStartup()
        {
            //Framework uses a Right Handed Coordinate System in 3D (x positive to the right, y positive upwards, z positive towards camera)
            //_cam3DPositionStart = new Vector3(0.0f, 2800.0f, 700.0f);
            //_cam3DPositionEnd = new Vector3(0.0f, -2800.0f, 700.0f);
            _cam3DPositionStart = new Vector3(2800.0f, 0.0f, 700.0f);
            _cam3DPositionEnd = new Vector3(-2800.0f, 0.0f, 700.0f);

            _cam3DLookAt = Vector3.Zero;
        }

        public override bool CreateResources(IServices yak)
        {
            _first = true;
            _meshDataReady = false;

            _float32Texture = yak.Helpers.DistortionHelper.TextureGenerator.ConcentricSinusoidalFloat32(128, 128, 18, false, true);

            var callBack = new Action<uint, TextureData>((index, data) =>
               {
                   GenerateMeshFromFloatData(yak.Stages, data);
               });

            _gpuToCpuCopyStage = yak.Stages.CreateSurfaceCopyDataStage(64, 64, callBack, true);

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

            _camera3D = yak.Cameras.CreateCamera3D(_cam3DPositionStart, _cam3DLookAt, Vector3.UnitY, 60.0f, 1.777779f, 0.00001f, 10000.0f);

            _whiteTexture = yak.Surfaces.LoadTexture("oilpaint", AssetSourceEnum.Embedded);

            return true;
        }

        private void GenerateMeshFromFloatData(IStages stages, TextureData data)
        {
            var positions = new Vector3[data.Width, data.Height];

            var tx = 1.0f / (1.0f * data.Width);
            var ty = 1.0f / (1.0f * data.Height);

            var texCoords = new Vector2[data.Width, data.Height];

            var minx = (-0.5f * data.Width) * PIXEL_SIZE_SCALAR;
            var miny = (-0.5f * data.Height) * PIXEL_SIZE_SCALAR;

            for (var y = 0; y < data.Height; y++)
            {
                for (var x = 0; x < data.Width; x++)
                {
                    var linear = InlineIndex(x, y, (int)data.Width);

                    texCoords[x, y] = new Vector2(tx * x, ty * y);

                    positions[x, y] = new Vector3(minx + (x * PIXEL_SIZE_SCALAR),
                                                  miny + (y * PIXEL_SIZE_SCALAR),
                                                  data.Pixels[linear].X * MESH_HEIGHT_AMPLITUDE);
                }
            }

            //Generate Normals 
            var normals = new Vector3[data.Width, data.Height];
            for (var y = 0; y < data.Height; y++)
            {
                for (var x = 0; x < data.Width; x++)
                {
                    //Finite difference method (seemed interesting as doesnt use x prod. but also does use current point for calc
                    //Let's see what it looks like
                    //https://stackoverflow.com/questions/13983189/opengl-how-to-calculate-normals-in-a-terrain-height-grid

                    float hL = 0.0f, hR = 0.0f, hU = 0.0f, hD = 0.0f;

                    if (x > 0)
                    {
                        hL = positions[x - 1, y].Z;
                    }

                    if (x < data.Width - 1)
                    {
                        hR = positions[x + 1, y].Z;
                    }

                    if (y > 0)
                    {
                        hU = positions[x, y - 1].Z;
                    }

                    if (y < data.Height - 1)
                    {
                        hD = positions[x, y + 1].Z;
                    }

                    var h = positions[x, y].Z;

                    if (x == 0)
                    {
                        hL = h - (h - hR);
                    }

                    if (x == data.Width - 1)
                    {
                        hR = h - (h - hL);
                    }

                    if (y == 0)
                    {
                        hU = h - (h - hD);
                    }

                    if (y == data.Height - 1)
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

            for (var y0 = 0; y0 < data.Height - 1; y0++)
            {
                for (var x0 = 0; x0 < data.Width - 1; x0++)
                {
                    var y1 = y0 + 1;
                    var x1 = x0 + 1;

                    vertices.Add(new Vertex3D { Position = positions[x0, y0], Normal = normals[x0, y0], TexCoord = texCoords[x0, y0] });
                    vertices.Add(new Vertex3D { Position = positions[x1, y0], Normal = normals[x1, y0], TexCoord = texCoords[x1, y0] });
                    vertices.Add(new Vertex3D { Position = positions[x0, y1], Normal = normals[x0, y1], TexCoord = texCoords[x0, y1] });

                    vertices.Add(new Vertex3D { Position = positions[x0, y1], Normal = normals[x0, y1], TexCoord = texCoords[x0, y1] });
                    vertices.Add(new Vertex3D { Position = positions[x1, y0], Normal = normals[x1, y0], TexCoord = texCoords[x1, y0] });
                    vertices.Add(new Vertex3D { Position = positions[x1, y1], Normal = normals[x1, y1], TexCoord = texCoords[x1, y1] });
                }
            }

            stages.SetMeshRenderMesh(_meshStage, vertices.ToArray());

            _meshDataReady = true;
        }

        private int InlineIndex(int x, int y, int width)
        {
            return (y * width) + x;
        }

        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds) => true;

        public override void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            _count += timeSinceLastDrawSeconds;

            while (_count > DURATION)
            {
                _count -= DURATION;
            }

            var fraction = 0.5f * ((float)Math.Sin((_count / DURATION) * Math.PI * 2.0f) + 1.0f);

            var camDelta = _cam3DPositionEnd - _cam3DPositionStart;

            yak.Cameras.SetCamera3DView(_camera3D, _cam3DPositionStart + (fraction * camDelta), _cam3DLookAt, Vector3.UnitY);
        }

        public override void Drawing(IDrawing draw, IFps fps, IInput input, ICoordinateTransforms transform, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Rendering(IRenderQueue q, IRenderTarget windowRenderTarget)
        {
            if (_first)
            {
                q.CopySurfaceData(_gpuToCpuCopyStage, _float32Texture);
                _first = false;
            }

            q.ClearDepth(windowRenderTarget);
            q.ClearColour(windowRenderTarget, _meshDataReady ? Colour.Clear : Colour.Pink);

            if (_meshDataReady)
            {
                q.MeshRender(_meshStage, _camera3D, _whiteTexture, windowRenderTarget);
            }
        }

        public override void Shutdown() { }
    }
}
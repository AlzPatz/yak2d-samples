using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;
using Veldrid.SPIRV;
using Yak2D;
using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections.Generic;

namespace CustomVeldrid_ComputeShaderExample
{
    public class GameOfLife : CustomVeldridBase
    {
        private const int INIT_NUM_FRAMES_TO_WAIT_FOR_UPDATE = 16;

        public int NumberFramesToWaitForUpdate { get; set; }
        public bool Paused { get; set; }
        public bool ClearFlag { get; set; }
        public List<Point> PointsToAdd { get; set; }
        public List<Point> PointsToRemove { get; set; }

        private struct GridSize
        {
            public int Width;
            public int Height;
            public int Pad0;
            public int Pad1;
        };

        private struct WriteToggle
        {
            public int Write;
            public int Pad2;
            public int Pad3;
            public int Pad4;
        };

        private int frameCount = 0;

        private int _width;
        private int _height;

        private Random _rnd;

        private int _threadGroupSize;

        private Shader _computeShader;
        private ResourceLayout _computeLayout;
        private Pipeline _computePipeline;
        private TextureView[] _conwayTextureViews;
        private ResourceSet[] _conwayResourceSets;
        private DeviceBuffer _gridSizeBuffer;
        private DeviceBuffer _writeToggleBuffer;

        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        private ResourceLayout _graphicsLayout;
        private Pipeline _graphicsPipeline;
        private ResourceSet _graphicsResourceSet;

        private Texture _stagingTexture;
        private Texture[] _conwayTextures;
        private Texture _renderableTexture;
        private TextureView _renderableTextureView;

        private int step = 0;

        public GameOfLife(int gridWidth, int gridHeight, int threadGroupSize)
        {
            Paused = true;

            _threadGroupSize = threadGroupSize;
            _width = gridWidth;
            _height = gridHeight;

            _rnd = new Random();

            PointsToAdd = new List<Point>();
            PointsToRemove = new List<Point>();

            NumberFramesToWaitForUpdate = INIT_NUM_FRAMES_TO_WAIT_FOR_UPDATE;
        }

        public override void Initialise(GraphicsDevice device, Sdl2Window window, DisposeCollectorResourceFactory factory)
        {
            var shaderBytes = ReadEmbeddedAssetBytes("Shaders/conway.glsl");

            _gridSizeBuffer = factory.CreateBuffer(new BufferDescription(16, BufferUsage.UniformBuffer));
            _writeToggleBuffer = factory.CreateBuffer(new BufferDescription(16, BufferUsage.UniformBuffer));

            _computeShader = factory.CreateFromSpirv(new ShaderDescription(
                ShaderStages.Compute,
                shaderBytes,
                "main"));

            _computeLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Last", ResourceKind.TextureReadWrite, ShaderStages.Compute),
                new ResourceLayoutElementDescription("Current", ResourceKind.TextureReadWrite, ShaderStages.Compute),
                new ResourceLayoutElementDescription("Tex", ResourceKind.TextureReadWrite, ShaderStages.Compute),
                new ResourceLayoutElementDescription("GridSizeBuffer", ResourceKind.UniformBuffer, ShaderStages.Compute),
                new ResourceLayoutElementDescription("WriteToggleBuffer", ResourceKind.UniformBuffer, ShaderStages.Compute)));

            ComputePipelineDescription computePipelineDesc = new ComputePipelineDescription(_computeShader,
                                                                                          _computeLayout,
                                                                                          16,
                                                                                          16,
                                                                                          1);
            _computePipeline = factory.CreateComputePipeline(ref computePipelineDesc);

            _stagingTexture = factory.CreateTexture(TextureDescription.Texture2D(
                      (uint)_width,
                      (uint)_height,
                      1,
                      1,
                      PixelFormat.R8_UInt,
                      TextureUsage.Staging));

            _conwayTextures = new Texture[2];

            var zeros = Enumerable.Repeat((byte)0, _width * _height).ToArray();

            _conwayTextureViews = new TextureView[2];

            _conwayResourceSets = new ResourceSet[2];

            for (var n = 0; n < 2; n++)
            {
                _conwayTextures[n] = factory.CreateTexture(TextureDescription.Texture2D(
                      (uint)_width,
                      (uint)_height,
                      1,
                      1,
                      PixelFormat.R8_UInt,
                      TextureUsage.Sampled | TextureUsage.Storage));


                //Load it with zeros
                device.UpdateTexture(_conwayTextures[n], zeros, 0, 0, 0, (uint)_width, (uint)_height, 1, 0, 0);

                _conwayTextureViews[n] = factory.CreateTextureView(_conwayTextures[n]);

            }

            _renderableTexture = factory.CreateTexture(TextureDescription.Texture2D(
                      (uint)_width,
                      (uint)_height,
                      1,
                      1,
                      PixelFormat.R32_G32_B32_A32_Float,
                      TextureUsage.Sampled | TextureUsage.Storage));

            _renderableTextureView = factory.CreateTextureView(_renderableTexture);

            for (var n = 0; n < 2; n++)
            {
                var last = n == 0 ? 1 : 0;

                _conwayResourceSets[n] = factory.CreateResourceSet(new ResourceSetDescription(
                    _computeLayout,
                    _conwayTextureViews[last],
                    _conwayTextureViews[n],
                    _renderableTextureView,
                    _gridSizeBuffer,
                    _writeToggleBuffer
                    ));
            }

            //Quad Rendering

            _vertexBuffer = factory.CreateBuffer(new BufferDescription(16 * 4, BufferUsage.VertexBuffer));
            _indexBuffer = factory.CreateBuffer(new BufferDescription(2 * 6, BufferUsage.IndexBuffer));

            Shader[] shaders = factory.CreateFromSpirv(
                new ShaderDescription(
                    ShaderStages.Vertex,
                    ReadEmbeddedAssetBytes("Shaders/vertex.glsl"),
                    "main"),
                new ShaderDescription(
                    ShaderStages.Fragment,
                    ReadEmbeddedAssetBytes("Shaders/fragment.glsl"),
                    "main"));

            ShaderSetDescription shaderSet = new ShaderSetDescription(
                new VertexLayoutDescription[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                        new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2))
                },
                shaders);

            _graphicsLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Tex", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SS", ResourceKind.Sampler, ShaderStages.Fragment)));

            GraphicsPipelineDescription fullScreenQuadDesc = new GraphicsPipelineDescription(
               BlendStateDescription.SingleOverrideBlend,
               DepthStencilStateDescription.Disabled,
               new RasterizerStateDescription(FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.Clockwise, true, false),
               PrimitiveTopology.TriangleList,
               shaderSet,
               new[] { _graphicsLayout },
               device.MainSwapchain.Framebuffer.OutputDescription);

            _graphicsPipeline = factory.CreateGraphicsPipeline(ref fullScreenQuadDesc);

            _graphicsResourceSet = factory.CreateResourceSet(new ResourceSetDescription(
                   _graphicsLayout,
                   _renderableTextureView,
                   device.PointSampler));

            InitResources(device, factory);
        }

        private byte[] ReadEmbeddedAssetBytes(string path)
        {
            var assemblyName = this.GetType().Assembly.GetName().Name;

            path = path.Replace('/', '.');
            path = string.Concat(assemblyName, ".", path);

            using (Stream stream = OpenEmbeddedAssetStream(path, this.GetType()))
            {
                byte[] bytes = new byte[stream.Length];
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    stream.CopyTo(ms);
                    return bytes;
                }
            }
        }

        public static Stream OpenEmbeddedAssetStream(string name, Type t) => t.Assembly.GetManifestResourceStream(name);

        private void InitResources(GraphicsDevice device, DisposeCollectorResourceFactory factory)
        {
            var cl = factory.CreateCommandList();

            cl.Begin();

            var gridSize = new GridSize
            {
                Width = _width,
                Height = _height,
                Pad0 = 0,
                Pad1 = 0
            };

            cl.UpdateBuffer(_gridSizeBuffer, 0, gridSize);

            Vector4[] quadVerts =
{
                new Vector4(-1, 1, 0, 0),
                new Vector4(1, 1, 1, 0),
                new Vector4(1, -1, 1, 1),
                new Vector4(-1, -1, 0, 1),
            };

            ushort[] indices = { 0, 1, 2, 0, 2, 3 };

            cl.UpdateBuffer(_vertexBuffer, 0, quadVerts);
            cl.UpdateBuffer(_indexBuffer, 0, indices);

            cl.End();

            device.SubmitCommands(cl);
            device.WaitForIdle();

            cl?.Dispose();
        }

        public override void Update(float timeStepSeconds, InputSnapshot inputSnapshot) { }

        public override void Render(CommandList cl, GraphicsDevice device, ResourceSet texture0, ResourceSet texture1, ResourceSet texture2, ResourceSet texture3, Framebuffer framebufferTarget)
        {
            if (PointsToAdd.Count > 0 || PointsToRemove.Count > 0 || ClearFlag)
            {
                //Pull Byte Data from Staging Texture

                var data = new byte[_width * _height];

                MappedResourceView<byte> mapRead = device.Map<byte>(_stagingTexture, MapMode.Read);

                Marshal.Copy(mapRead.MappedResource.Data, data, 0, _width * _height);

                device.Unmap(_stagingTexture);

                // Modify Data
                PointsToAdd.ForEach(p =>
                {
                    var index = (p.Y * _width) + p.X;
                    data[index] = (byte)1;
                });

                PointsToAdd.Clear();

                PointsToRemove.ForEach(p =>
                {
                    var index = (p.Y * _width) + p.X;
                    data[index] = (byte)0;
                });

                PointsToRemove.Clear();

                if(ClearFlag)
                {
                    ClearFlag = false;
                    data = Enumerable.Repeat((byte)0, _width * _height).ToArray();
                }

                //Push Updated Data back to Staging Texture

                MappedResourceView<byte> mapped = device.Map<byte>(_stagingTexture, MapMode.Write);

                Marshal.Copy(data, 0, mapped.MappedResource.Data, _width * _height);

                device.Unmap(_stagingTexture);
                cl.CopyTexture(_stagingTexture, _conwayTextures[step]);
            }

            var runUpdateIteration = false;
            if (!Paused)
            {
                frameCount++;
            }

            if (!Paused && frameCount >= NumberFramesToWaitForUpdate)
            {
                step = step == 0 ? 1 : 0;
                frameCount = 0;
                runUpdateIteration = true;
            }

            var writeToggle = new WriteToggle
            {
                Write = runUpdateIteration ? 1 : 0,
                Pad2 = 0,
                Pad3 = 0,
                Pad4 = 0
            };

            cl.UpdateBuffer(_writeToggleBuffer, 0, writeToggle);

            cl.SetPipeline(_computePipeline);
            cl.SetComputeResourceSet(0, _conwayResourceSets[step]);
            cl.Dispatch((uint)(_width / _threadGroupSize), (uint)(_height / _threadGroupSize), 1);
            cl.CopyTexture(_conwayTextures[step], _stagingTexture);

            cl.SetFramebuffer(framebufferTarget);
            cl.SetFullViewports();
            cl.SetFullScissorRects();
            cl.ClearColorTarget(0, RgbaFloat.Clear);
            cl.SetPipeline(_graphicsPipeline);
            cl.SetVertexBuffer(0, _vertexBuffer);
            cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            cl.SetGraphicsResourceSet(0, _graphicsResourceSet);
            cl.DrawIndexed(6, 1, 0, 0, 0);
        }

        public override void DisposeOfResources()
        {
            _computeShader?.Dispose();
            _computeLayout?.Dispose();
            _computePipeline?.Dispose();
            _gridSizeBuffer?.Dispose();
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
            _graphicsLayout?.Dispose();
            _graphicsPipeline?.Dispose();
            _graphicsResourceSet?.Dispose();
            _stagingTexture?.Dispose();
            _renderableTexture?.Dispose();
            _renderableTextureView?.Dispose();

            for (var n = 0; n < 2; n++)
            {
                _conwayTextures[n]?.Dispose();
                _conwayTextureViews[n]?.Dispose();
                _conwayResourceSets[n]?.Dispose();
            }
        }
    }
}
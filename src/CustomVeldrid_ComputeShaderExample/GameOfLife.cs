using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;
using Veldrid.SPIRV;
using Yak2D;
using System;
using System.IO;
using System.Numerics;

namespace CustomVeldrid_ComputeShaderExample
{
    /// <summary>
    /// An example that uses Veldrid.SPIRV to generate and run a compute shader version of Conway's Game of Life
    /// </summary>
    public class GameOfLife : CustomVeldridBase
    {
        private int _width;
        private int _height;

        private Shader _computeShader;
        private ResourceLayout _computeLayout;
        private Pipeline _computePipeline;
        private Texture _computeTargetTexture;
        private TextureView _computeTargetTextureView;
        private ResourceSet _computeResourceSet;
        private DeviceBuffer _screenSizeBuffer;
        private DeviceBuffer _shiftBuffer;

        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        private ResourceLayout _graphicsLayout;
        private Pipeline _graphicsPipeline;
        private ResourceSet _graphicsResourceSet;

        private float _ticks;

        public GameOfLife(int gridWidth, int gridHeight)
        {
            _width = gridWidth;
            _height = gridHeight;
        }

        public override void Initialise(GraphicsDevice device, Sdl2Window window, DisposeCollectorResourceFactory factory)
        {
            //Compute Shader

            _screenSizeBuffer = factory.CreateBuffer(new BufferDescription(16, BufferUsage.UniformBuffer));
            _shiftBuffer = factory.CreateBuffer(new BufferDescription(16, BufferUsage.UniformBuffer));

            var shaderBytes = ReadEmbeddedAssetBytes("Shaders/conway.glsl");

            _computeShader = factory.CreateFromSpirv(new ShaderDescription(
                ShaderStages.Compute,
                shaderBytes,
                "main"));

            _computeLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Tex", ResourceKind.TextureReadWrite, ShaderStages.Compute),
                new ResourceLayoutElementDescription("ScreenSizeBuffer", ResourceKind.UniformBuffer, ShaderStages.Compute),
                new ResourceLayoutElementDescription("ShiftBuffer", ResourceKind.UniformBuffer, ShaderStages.Compute)));

            ComputePipelineDescription computePipelineDesc = new ComputePipelineDescription(
                _computeShader,
                _computeLayout,
                16, 16, 1);
            _computePipeline = factory.CreateComputePipeline(ref computePipelineDesc);

            _computeTargetTexture = factory.CreateTexture(TextureDescription.Texture2D(
                          (uint)_width,
                          (uint)_height,
                          1,
                          1,
                          PixelFormat.R32_G32_B32_A32_Float,
                          TextureUsage.Sampled | TextureUsage.Storage));

            _computeTargetTextureView = factory.CreateTextureView(_computeTargetTexture);

            _computeResourceSet = factory.CreateResourceSet(new ResourceSetDescription(
                _computeLayout,
                _computeTargetTextureView,
                _screenSizeBuffer,
                _shiftBuffer));

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
                new ResourceLayoutElementDescription("Tex11", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Tex22", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
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
               _computeTargetTextureView,
               _computeTargetTextureView,
               _computeTargetTextureView,
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

            cl.UpdateBuffer(_screenSizeBuffer, 0, new Vector4(_width, _height, 0, 0));

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

        public override void Update(float timeStepSeconds, InputSnapshot inputSnapshot)
        {
            _ticks += timeStepSeconds * 1000f;
        }

        public override void Render(CommandList cl, GraphicsDevice device, ResourceSet texture0, ResourceSet texture1, ResourceSet texture2, ResourceSet texture3, Framebuffer framebufferTarget)
        {
            Vector4 shifts = new Vector4(
                _width * (float)Math.Cos(_ticks / 500f), // Red shift
                _height * (float)Math.Sin(_ticks / 1250f), // Green shift
                (float)Math.Sin(_ticks / 1000f), // Blue shift
                0); // Padding
            cl.UpdateBuffer(_shiftBuffer, 0, ref shifts);

            cl.SetPipeline(_computePipeline);
            cl.SetComputeResourceSet(0, _computeResourceSet);
            cl.Dispatch((uint)(_width / 16), (uint)(_height / 16), 1);

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
            _computeTargetTexture?.Dispose();
            _computeTargetTextureView?.Dispose();
            _computeResourceSet?.Dispose();
        }
    }
}
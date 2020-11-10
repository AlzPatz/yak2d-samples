using System.Numerics;
using System.Runtime.InteropServices;
using Yak2D;
using SampleBase;
using System;

namespace CustomShader_Example
{
    public class CustomShaderExample : ApplicationBase
    {
        private const bool USE_SPIRV_COMPILE_TIME_SHADER = true;

        [StructLayout(LayoutKind.Explicit)]
        private struct UniformsCustomShader
        {
            [FieldOffset(0)]
            public float Amount;
            [FieldOffset(16)]
            public Vector4 Pad;
        }

        private ITexture _texture;
        private ICustomShaderStage _customShaderStage;

        private const float DURATION = 1.0f;
        private float _count = 0.0f;

        public override string ReturnWindowTitle() => "Custom Shader Example";

        public override void OnStartup() { }

        public override bool CreateResources(IServices yak)
        {
            _texture = yak.Surfaces.LoadTexture("city", AssetSourceEnum.Embedded);

            _customShaderStage = yak.Stages.CreateCustomShaderStage("CustomBinaryFragment",
                        AssetSourceEnum.Embedded,
                       new ShaderUniformDescription[]
                       {
                           new ShaderUniformDescription
                           {
                               Name = "Texture",
                               UniformType = ShaderUniformType.Texture,
                               SizeInBytes = 0
                           },
                           new ShaderUniformDescription
                           {
                               Name = "Threshold",
                               UniformType = ShaderUniformType.Data,
                               SizeInBytes = 32
                           }
                       }, BlendState.Override, USE_SPIRV_COMPILE_TIME_SHADER);

            return true;
        }
        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds) => true;

        public override void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            _count += timeSinceLastDrawSeconds;

            while (_count > DURATION)
            {
                _count -= DURATION;
            }

            var frac = _count / DURATION;

            yak.Stages.SetCustomShaderUniformValues<UniformsCustomShader>(_customShaderStage, "Threshold", new UniformsCustomShader { Amount = 0.5f * ((float)Math.Sin(frac * Math.PI * 2.0f) + 1.0f) });
        }

        public override void Drawing(IDrawing draw, IFps fps, IInput input, ICoordinateTransforms transform, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Rendering(IRenderQueue q, IRenderTarget windowRenderTarget)
        {
            q.ClearColour(windowRenderTarget, Colour.Clear);
            q.ClearDepth(windowRenderTarget);
            q.CustomShader(_customShaderStage, _texture, null, null, null, windowRenderTarget);
        }

        public override void Shutdown() { }
    }
}
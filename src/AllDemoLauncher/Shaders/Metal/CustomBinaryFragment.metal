#include <metal_stdlib>
using namespace metal;

struct Threshold
{
	float Amount;
	float4 Pad;
};

struct FragmentIn
{
    float4 Position [[attribute(0)]];
    float2 FTex [[attribute(1)]];
};

fragment float4 shader( FragmentIn fIn [[stage_in]],
                    texture2d<float, access::sample> Texture [[ texture(0)]],
                    sampler Sampler [[ sampler(0)]],
                    constant Threshold &threshold [[buffer(0)]])
                    {
						float am = threshold.Amount;
						float max = am * am * am;
						max = clamp(am, 0.0, 1.0);
						float4 sample = Texture.sample(Sampler, fIn.FTex);
						float val = (sample.r * sample.g * sample.b) / sample.a;
						float binary = (val - max) + 1.0;
						binary = floor(binary);

						return float4(binary, binary, binary, binary);
                    }
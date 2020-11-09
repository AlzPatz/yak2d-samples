SamplerState Sampler : register(s0);
Texture2D Texture : register(t0);

cbuffer Threshold : register(b0)
{
	float Amount;
	float4 Pad;
};

struct FragmentIn
{
	float4 Position : SV_Position;
	float2 FTex : TEXCOORD;
};

float4 main(FragmentIn input) : SV_Target
{
	float am = Amount;
	float max = am * am * am;
	max = clamp(am, 0.0, 1.0);
	float4 sample = Texture.Sample(Sampler, input.FTex);
	float val = (sample.r * sample.g * sample.b) / sample.a;
	float binary = (val - max) + 1.0;
	binary = floor(binary);

	return float4(binary, binary, binary, binary);
}
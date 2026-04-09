#version 450

layout(set = 0, binding = 1) uniform texture2D Texture_Texture;
layout(set = 0, binding = 0) uniform sampler Sampler_Texture;

layout(set = 1, binding = 0) uniform Threshold
{
    float Amount;
    vec4 Pad;
};

layout(location = 0) in vec2 FTex;
layout(location = 0) out vec4 fragColor;

void main()
{
	float am = Amount;
	float max = am * am * am;
	max = clamp(am, 0.0, 1.0);
    vec4 samp = texture(sampler2D(Texture_Texture, Sampler_Texture), FTex);
	float val = (samp.r * samp.g * samp.b) / samp.a;
	float binary = (val - max) + 1.0;
	binary = floor(binary);
	fragColor = vec4(binary, binary, binary, binary);
}
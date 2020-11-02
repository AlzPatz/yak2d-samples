#version 330 core

uniform sampler2D Sampler; 

uniform Threshold
{
    float Amount;
    vec4 Pad;
};

in vec2 FTex;
out vec4 fragColor;

void main()
{
	float am = Amount;
	float max = am * am * am;
	max = clamp(am, 0.0, 1.0);
    vec4 sample = texture(Sampler, FTex);
	float val = (sample.r * sample.g * sample.b) / sample.a;
	float binary = (val - max) + 1.0;
	binary = floor(binary);

	fragColor = vec4(binary, binary, binary, binary);
}
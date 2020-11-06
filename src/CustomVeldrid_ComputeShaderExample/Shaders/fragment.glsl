#version 450

layout(set = 0, binding = 0) uniform texture2D Tex;
layout(set = 0, binding = 1) uniform sampler SS;

layout(location = 0) in vec2 fsin_TexCoords;
layout(location = 0) out vec4 OutColor;

void main()
{
   OutColor =  texture(sampler2D(Tex, SS), fsin_TexCoords);
}

#version 450

layout(set = 0, binding = 0, r8ui) uniform uimage2D Last;
layout(set = 0, binding = 1, r8ui) uniform uimage2D Current;
layout(set = 0, binding = 2, rgba32f) uniform image2D Tex;

layout(set = 0, binding = 3) uniform GridSizeBuffer
{
    int GridWidth;
    int GridHeight;
    int Pad0;
    int Pad1;
};

layout(set = 0, binding = 4) uniform WriteToggleBuffer
{
    int Write;
    int Pad2;
    int Pad3;
    int Pad4;
};

layout(local_size_x = 16, local_size_y = 16, local_size_z = 1) in;

/*
From Wikipedia:
Any live cell with two or three live neighbours survives.
Any dead cell with three live neighbours becomes a live cell.
All other live cells die in the next generation. Similarly, all other dead cells stay dead.
*/

void main()
{
    if(Write == 0)
    {
        imageStore(Tex, ivec2(gl_GlobalInvocationID.xy), vec4(imageLoad(Current, ivec2(gl_GlobalInvocationID.xy)).r));
        return;
    }

    uint left = gl_GlobalInvocationID.x - 1;
    if(left < 0)
    {
        left += GridWidth;
    }
    uint right = gl_GlobalInvocationID.x + 1;
    if(right >= GridWidth)
    {
        right -= GridWidth;
    }
    uint top = gl_GlobalInvocationID.y - 1;
    if(top < 0)
    {
        top += GridHeight;
    }
    uint bottom = gl_GlobalInvocationID.y + 1;
    if(bottom >= GridHeight)
    {
        bottom -= GridHeight;
    }

    uint sum = 0;

    sum +=  imageLoad(Last, ivec2(gl_GlobalInvocationID.x, top)).r;
    sum +=  imageLoad(Last, ivec2(gl_GlobalInvocationID.x, bottom)).r;
    sum +=  imageLoad(Last, ivec2(left, gl_GlobalInvocationID.y)).r;
    sum +=  imageLoad(Last, ivec2(right, gl_GlobalInvocationID.y)).r;
    sum +=  imageLoad(Last, ivec2(left, top)).r;
    sum +=  imageLoad(Last, ivec2(right, top)).r;
    sum +=  imageLoad(Last, ivec2(left, bottom)).r;
    sum +=  imageLoad(Last, ivec2(right, bottom)).r;

    uint val = imageLoad(Last, ivec2(gl_GlobalInvocationID.xy)).r;

    if(val == 1)
    {
        if(sum != 2 && sum != 3)
        {
            val = 0;
        }
    }
    else
    {
        if(sum == 3)
        {
            val = 1;
        }
    }
    
    imageStore(Current, ivec2(gl_GlobalInvocationID.xy), uvec4(val)); //No overload to take single unsigned int

    //Store to float texture so can be sampled in next shader (HLSL wouldn't allow integer sampling)

    imageStore(Tex, ivec2(gl_GlobalInvocationID.xy), vec4(val));
}
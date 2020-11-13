using System;

namespace GpuToCpu_SurfaceCopyRGBA
{
    class Program
    {
        static void Main(string[] args)
        {
            Yak2D.Launcher.Run(new GpuToCpuRgbaCopy());
        }
    }
}

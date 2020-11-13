using System;

namespace GpuToCpu_SurfaceCopyFloat32
{
    class Program
    {
        static void Main(string[] args)
        {
            Yak2D.Launcher.Run(new GpuToCpuFloat32Copy());
        }
    }
}

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var demo = new Dictionary<int, (string Name, Action Run)>()
            {
                { 0, ("Bloom Example", () => Yak2D.Launcher.Run(new BloomExample())) },
                { 1, ("Blur Example", () => Yak2D.Launcher.Run(new BlurExample())) },
                { 2, ("Colour Effects Example", () => Yak2D.Launcher.Run(new ColourEffectsExample())) },
                { 3, ("Copy Example", () => Yak2D.Launcher.Run(new CopyExample())) },
                { 4, ("Custom Shader Example", () => Yak2D.Launcher.Run(new CustomShaderExample())) },
                { 5, ("Custom Veldrid Compute Shader Example", () => Yak2D.Launcher.Run(new CustomVeldridComputeExample())) },
                { 6, ("Distortion Using Helper Functions", () => Yak2D.Launcher.Run(new DistortionHelperExample())) },
                { 7, ("Distortion Manual Texture Creation", () => Yak2D.Launcher.Run(new DistortionManualExample())) },
                { 8, ("Prerendering Textures for Later Use", () => Yak2D.Launcher.Run(new PreRenderExample())) },
                { 9, ("Draw Basic Polygon Helpers", () => Yak2D.Launcher.Run(new DrawUsingHelperFunctions())) },
                { 10, ("Draw Camera 2D World and Screen", () => Yak2D.Launcher.Run(new WorldAndScreenDrawing())) },
                { 11, ("Draw Fluent Interface Helper", () => Yak2D.Launcher.Run(new DrawFluentInterfaceExamples())) },
                { 12, ("Draw Font Example", () => Yak2D.Launcher.Run(new DrawFontExample())) },
                { 13, ("Draw Image File Formats", () => Yak2D.Launcher.Run(new ImageFormats())) },
                { 14, ("Persistent Draw Queue", () => Yak2D.Launcher.Run(new PersistentDrawQueueExample())) },
                { 15, ("Draw Polygons from Vertices", () => Yak2D.Launcher.Run(new DrawCustomPolygons())) },
                { 16, ("Split Screen Example", () => Yak2D.Launcher.Run(new SplitScreenExample())) },
                { 17, ("Framework Items Creation and Destruction", () => Yak2D.Launcher.Run(new CreationAndDestruction())) },
                { 18, ("GPU to CPU Surface Copy Float32", () => Yak2D.Launcher.Run(new GpuToCpuFloat32Copy())) },
                { 19, ("GPU to CPU Surface Copy RGBA", () => Yak2D.Launcher.Run(new GpuToCpuRgbaCopy())) },
                { 20, ("Coordinate Transforms Helper", () => Yak2D.Launcher.Run(new CoordinateTransformsExample())) },
                { 21, ("Gamepad Input Usage", () => Yak2D.Launcher.Run(new GamepadUsage())) },
                { 22, ("Mouse and Keyboard Input Usage", () => Yak2D.Launcher.Run(new MouseAndKeyboardUsage())) },
                { 23, ("Mesh Helper Examples", () => Yak2D.Launcher.Run(new MeshHelperExamples())) },
                { 24, ("Manual Mesh Modification", () => Yak2D.Launcher.Run(new ManualMesh())) },
                { 25, ("Simple Manual Mesh", () => Yak2D.Launcher.Run(new ManualMeshSimple())) },
                { 26, ("Texture Mixing with Factors", () => Yak2D.Launcher.Run(new SimpleMixing())) },
                { 27, ("Per-Pixel Texture Mixing", () => Yak2D.Launcher.Run(new PerPixelMixing())) },
                { 28, ("CRT Style Effect", () => Yak2D.Launcher.Run(new CrtEffectsExample())) },
                { 29, ("Edge Detection Effect", () => Yak2D.Launcher.Run(new EdgeDetectionExample())) },
                { 30, ("Old Movie Effect", () => Yak2D.Launcher.Run(new OldMovieExample())) },
                { 31, ("Pixelate Effect", () => Yak2D.Launcher.Run(new PixellateExample())) },
                { 32, ("Static Noise Effect", () => Yak2D.Launcher.Run(new StaticExample())) },
                { 33, ("Style Effects with Configuration Helpers", () => Yak2D.Launcher.Run(new ConfigurationHelperExample())) },
                { 34, ("Create Texture from RGBA Data", () => Yak2D.Launcher.Run(new RgbaTextureFromData())) },
                { 35, ("Window Properties Changing", () => Yak2D.Launcher.Run(new ChangingWindowProperties())) },
            };

            var running = true;
            while (running)
            {
                //Console.Clear();
                Console.WriteLine("Yak2D Demo Launcher");
                Console.WriteLine("-------------------");
                Console.WriteLine();

                foreach (var app in demo.OrderBy(a => a.Key))
                {
                    Console.WriteLine($"[{app.Key}] {app.Value.Name}");
                }

                Console.WriteLine();
                Console.WriteLine("Enter selection (or 'q' to quit): ");

                var inputLine = Console.ReadLine();
                if (int.TryParse(inputLine, out int choice) && demo.ContainsKey(choice))
                {
                    var selected = demo[choice];

                    Console.WriteLine($"You selected: {selected.Name}");
                    selected.Run();
                }
                else
                {
                    if (inputLine == "q")
                    {
                        Console.WriteLine("Exiting...");
                        running = false;
                    }
                    else
                    {
                        Console.WriteLine("Invalid selection.");
                    }
                }

                Console.WriteLine();
            }
        }
    }
}
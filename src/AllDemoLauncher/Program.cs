namespace Demo
{
    class Program
    {
        enum eBackend { Default, D3D, Vulkan, OpenGL, OpenGLES, Metal }

        static void Main(string[] args)
        {
            var backends = new Dictionary<eBackend, string> {
                { eBackend.Default, "default" },
                { eBackend.D3D, "d3d" },
                { eBackend.Vulkan, "vulkan" },
                { eBackend.OpenGL, "opengl" },
                { eBackend.OpenGLES, "opengles" },
                { eBackend.Metal, "metal" }
            };

            Action<string, dynamic> run = (backend, ex) =>
            {
                ex.ChangeBackendStartUpConfig(backend);
                Yak2D.Launcher.Run(ex);
            };

            var demo = new Dictionary<int, (string Name, Action<string> Run)>()
            {
                { 0, ("Bloom Example", (b) => run(b, new BloomExample())) },
                { 1, ("Blur Example", (b) => run(b, new BlurExample())) },
                { 2, ("Colour Effects Example", (b) => run(b, new ColourEffectsExample())) },
                { 3, ("Copy Example", (b) => run(b, new CopyExample())) },
                { 4, ("Custom Shader Example", (b) => run(b, new CustomShaderExample())) },
                { 5, ("Custom Veldrid Compute Shader Example", (b) => run(b, new CustomVeldridComputeExample())) },
                { 6, ("Distortion Using Helper Functions", (b) => run(b, new DistortionHelperExample())) },
                { 7, ("Distortion Manual Texture Creation", (b) => run(b, new DistortionManualExample())) },
                { 8, ("Prerendering Textures for Later Use", (b) => run(b, new PreRenderExample())) },
                { 9, ("Draw Basic Polygon Helpers", (b) => run(b, new DrawUsingHelperFunctions())) },
                { 10, ("Draw Camera 2D World and Screen", (b) => run(b, new WorldAndScreenDrawing())) },
                { 11, ("Draw Fluent Interface Helper", (b) => run(b, new DrawFluentInterfaceExamples())) },
                { 12, ("Draw Font Example", (b) => run(b, new DrawFontExample())) },
                { 13, ("Draw Image File Formats", (b) => run(b, new ImageFormats())) },
                { 14, ("Persistent Draw Queue", (b) => run(b, new PersistentDrawQueueExample())) },
                { 15, ("Draw Polygons from Vertices", (b) => run(b, new DrawCustomPolygons())) },
                { 16, ("Split Screen Example", (b) => run(b, new SplitScreenExample())) },
                { 17, ("Framework Items Creation and Destruction", (b) => run(b, new CreationAndDestruction())) },
                { 18, ("GPU to CPU Surface Copy Float32", (b) => run(b, new GpuToCpuFloat32Copy())) },
                { 19, ("GPU to CPU Surface Copy RGBA", (b) => run(b, new GpuToCpuRgbaCopy())) },
                { 20, ("Coordinate Transforms Helper", (b) => run(b, new CoordinateTransformsExample())) },
                { 21, ("Gamepad Input Usage", (b) => run(b, new GamepadUsage())) },
                { 22, ("Mouse and Keyboard Input Usage", (b) => run(b, new MouseAndKeyboardUsage())) },
                { 23, ("Mesh Helper Examples", (b) => run(b, new MeshHelperExamples())) },
                { 24, ("Manual Mesh Modification", (b) => run(b, new ManualMesh())) },
                { 25, ("Simple Manual Mesh", (b) => run(b, new ManualMeshSimple())) },
                { 26, ("Texture Mixing with Factors", (b) => run(b, new SimpleMixing())) },
                { 27, ("Per-Pixel Texture Mixing", (b) => run(b, new PerPixelMixing())) },
                { 28, ("CRT Style Effect", (b) => run(b, new CrtEffectsExample())) },
                { 29, ("Edge Detection Effect", (b) => run(b, new EdgeDetectionExample())) },
                { 30, ("Old Movie Effect", (b) => run(b, new OldMovieExample())) },
                { 31, ("Pixelate Effect", (b) => run(b, new PixellateExample())) },
                { 32, ("Static Noise Effect", (b) => run(b, new StaticExample())) },
                { 33, ("Style Effects with Configuration Helpers", (b) => run(b, new ConfigurationHelperExample())) },
                { 34, ("Create Texture from RGBA Data", (b) => run(b, new RgbaTextureFromData())) },
                { 35, ("Window Properties Changing", (b) => run(b, new ChangingWindowProperties())) },
            };

            var backend = eBackend.Default;

            var running = true;
            while (running)
            {
                //Console.Clear();
                Console.WriteLine("Yak2D Demo Launcher");
                Console.WriteLine("-------------------");
                Console.WriteLine("Selected Backend: " + backend.ToString());
                Console.WriteLine();

                foreach (var app in demo.OrderBy(a => a.Key))
                {
                    Console.WriteLine($"[{app.Key}] {app.Value.Name}");
                }

                Console.WriteLine();
                Console.WriteLine("Enter Demo Selection (or 'b' to change backend, or 'q' to quit): ");

                var inputLine = Console.ReadLine();
                if (int.TryParse(inputLine, out int choice) && demo.ContainsKey(choice))
                {
                    var selected = demo[choice];

                    Console.WriteLine($"You selected: {selected.Name}");
                    Console.WriteLine("Tip: Escape will quit application when running...");
                    selected.Run(backends[backend]);
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
                        if (inputLine == "b")
                        {
                            Console.WriteLine();
                            Console.WriteLine("Select New Backend:");
                            Console.WriteLine();
                            var n = 0;
                            var backends_array = new eBackend[backends.Count];
                            foreach (var be in backends.OrderBy(a => a.Key))
                            {
                                Console.WriteLine($"[{n}] {be.Value}");
                                backends_array[n] = be.Key;
                                n++;
                            }
                            Console.WriteLine();
                            Console.WriteLine("Enter Backend #: ");

                            var anotherInputLine = Console.ReadLine();
                            if (int.TryParse(anotherInputLine, out int option) && option >= 0 && option < n)
                            {
                                backend = backends_array[option];
                                Console.WriteLine($"You selected: {backends[backend]}");
                            }
                            else
                            {
                                Console.WriteLine("Invalid backend selection.");
                            }
                            Console.WriteLine();
                        }
                        else
                        {
                            Console.WriteLine("Invalid demo or option selection.");
                        }
                    }
                }

                Console.WriteLine();
            }
        }
    }
}
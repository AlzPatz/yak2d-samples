using System;
using Yak2D;

namespace SampleBase
{
    public abstract class ApplicationBase : IApplication
    {
        public event EventHandler SwapChainFramebufferReCreated;
        public event EventHandler NumberOfGamepadsChanged;

        private bool _appClosing = false;

        public abstract void OnStartup();

        public virtual StartupConfig Configure()
        {
            return new StartupConfig
            {
                PreferredGraphicsApi = GraphicsApi.Direct3D11,
                WindowState = DisplayState.Normal,
                WindowPositionX = 100,
                WindowPositionY = 100,
                WindowWidth = 960,
                WindowHeight = 540,
                WindowTitle = ReturnWindowTitle(),
                SyncToVerticalBlank = true,
                UpdatePeriodType = UpdatePeriod.Fixed,
                ProcessFractionalUpdatesBeforeDraw = true,
                FixedOrSmallestUpdateTimeStepInSeconds = 1.0f / 120.0f,
                RequireAtleastOneUpdatePerDraw = true,
                FpsCalculationUpdatePeriod = 1.0f,
                TextureFolderRootName = "Textures",
                FontFolder = "Fonts",
                AutoClearMainWindowColourEachFrame = false,
                AutoClearMainWindowDepthEachFrame = false
            };
        }

        public void ProcessMessage(FrameworkMessage msg, IServices services)
        {
            switch (msg)
            {
                case FrameworkMessage.GraphicsDeviceRecreated:
                    CreateResources(services);
                    break;
                case FrameworkMessage.ApplicationWindowClosing:
                    _appClosing = true;
                    break;
                case FrameworkMessage.SwapChainFramebufferReCreated:
                    SwapChainFramebufferReCreated?.Invoke(this, new EventArgs());
                    break;
                case FrameworkMessage.GamepadAdded:
                case FrameworkMessage.GamepadRemoved:
                    NumberOfGamepadsChanged?.Invoke(this, new EventArgs());
                    break;
                case FrameworkMessage.WindowWasResized:
                case FrameworkMessage.LowMemoryReported:
                case FrameworkMessage.WindowGainedFocus:
                case FrameworkMessage.WindowLostFocus:
                    //Not handled in samples
                    break;
            }
        }

        public abstract string ReturnWindowTitle();

        public abstract bool CreateResources(IServices services);

        public bool Update(IServices yak, float timeSinceLastUpdateSeconds)
        {
            var input = yak.Input;

            if(input.WasKeyReleasedThisFrame(KeyCode.Escape))
            {
                _appClosing = true;
            }

            //Switching backends is not very reliable. For example on OSX the application closes
            //I believe this behaviour is more dictated by Veldrid and the OS than this framework
            //Investigate the trigger for "Shutdown Signal given to Application" to confirm

            if (input.WasKeyReleasedThisFrame(KeyCode.Number1))
            {
                yak.Backend.SetGraphicsApi(GraphicsApi.OpenGL);
            }

            if (input.WasKeyReleasedThisFrame(KeyCode.Number2))
            {
                yak.Backend.SetGraphicsApi(GraphicsApi.Direct3D11);
            }

            if (input.WasKeyReleasedThisFrame(KeyCode.Number3))
            {
                yak.Backend.SetGraphicsApi(GraphicsApi.Metal);
            }

            if (input.WasKeyReleasedThisFrame(KeyCode.Number4))
            {
                yak.Backend.SetGraphicsApi(GraphicsApi.Vulkan);
            }

            return !_appClosing && Update_(yak, timeSinceLastUpdateSeconds);
        }

        public abstract bool Update_(IServices yak, float timeSinceLastUpdateSeconds);

        public abstract void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds);

        public abstract void Drawing(IDrawing draw, IFps fps, IInput input, ICoordinateTransforms transforms, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds);

        public abstract void Rendering(IRenderQueue q, IRenderTarget windowRenderTarget);

        public abstract void Shutdown();
    }
}
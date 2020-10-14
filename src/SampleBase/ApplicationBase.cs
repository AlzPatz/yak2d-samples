using Yak2D;

namespace SampleBase
{
    public abstract class ApplicationBase : IApplication
    {
        protected IRenderTarget WindowRenderTarget;

        private bool _appClosing = false;

        public abstract void OnStartup();

        public virtual StartupConfig Configure()
        {
            return new StartupConfig
            {
                PreferredGraphicsApi = GraphicsApi.OpenGL,
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
                case FrameworkMessage.WindowWasResized:
                case FrameworkMessage.SwapChainFramebufferReCreated:
                    WindowRenderTarget = services.Surfaces.ReturnMainWindowRenderTarget();
                    break;
                case FrameworkMessage.ApplicationWindowClosing:
                    _appClosing = true;
                    break;
                case FrameworkMessage.GamepadAdded:
                case FrameworkMessage.GamepadRemoved:
                case FrameworkMessage.LowMemoryReported:
                case FrameworkMessage.WindowGainedFocus:
                case FrameworkMessage.WindowLostFocus:
                    //Not handled in samples
                    break;
            }
        }

        public abstract string ReturnWindowTitle();

        public abstract bool CreateResources(IServices services);

        public bool Update(IServices services, float timeSinceLastUpdateSeconds)
        {
            if (WindowRenderTarget == null)
            {
                WindowRenderTarget = services.Surfaces.ReturnMainWindowRenderTarget();
            }

            if(services.Input.WasKeyReleasedThisFrame(KeyCode.Escape))
            {
                _appClosing = true;
            }

            return !_appClosing && Update_(services, timeSinceLastUpdateSeconds);
        }

        public abstract bool Update_(IServices services, float timeSinceLastUpdateSeconds);

        public abstract void PreDrawing(IServices services, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds);

        public abstract void Drawing(IDrawing drawing, IFps fps, IInput input, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds);

        public abstract void Rendering(IRenderQueue queue);

        public abstract void Shutdown();
    }
}
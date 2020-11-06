using SampleBase;
using Yak2D;

namespace Input_GamepadUsage
{
    /// <summary>
    /// 
    /// </summary>
    public class MouseAndKeyboardUsage : ApplicationBase
    {
        private IDrawStage _drawStage;
        private ICamera2D _camera;

        public override string ReturnWindowTitle() => "Mouse and Keyboard Usage";

        public override void OnStartup() { }

        public override bool CreateResources(IServices yak)
        {
            _drawStage = yak.Stages.CreateDrawStage();

            _camera = yak.Cameras.CreateCamera2D();

            return true;
        }

        //BEST practice is to do ALL input HANDLING in the UPDATES
        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds)
        {

            return true;
        }

        /*
            Any INPUT queries made during a draw-related method cannot rely on accurately capturing all 
            INPUTs made "WAS X THIS FRAME". It is only the UPDATE loop cycle that will accurately capture all
            of these types of inputs. INPUTS queried during draw-related methods should be restricted to
            "Currently Held" or Positional type queries that would persist over multiple UPDATES
         */
        public override void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            //Can miss inputs here...
            var uselessK = yak.Input.WasKeyReleasedThisFrame(KeyCode.A);
            var uselessM = yak.Input.WasMouseReleasedThisFrame(MouseButton.Left);
        }

        /*
            Any INPUT queries made during a draw-related method cannot rely on accurately capturing all 
            INPUTs made "WAS X THIS FRAME". It is only the UPDATE loop cycle that will accurately capture all
            of these types of inputs. INPUTS queried during draw-related methods should be restricted to
            "Currently Held" or Positional type queries that would persist over multiple UPDATES
        */
        public override void Drawing(IDrawing draw, IFps fps, IInput input, ICoordinateTransforms transform, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            //Can miss inputs here...
            var uselessK = input.WasKeyPressedThisFrame(KeyCode.A);
            var uselessM = input.WasMousePressedThisFrame(MouseButton.Left);


        }

        public override void Rendering(IRenderQueue q, IRenderTarget windowRenderTarget)
        {
            q.ClearColour(windowRenderTarget, Colour.Clear);
            q.ClearDepth(windowRenderTarget);
            q.Draw(_drawStage, _camera, windowRenderTarget);
        }

        public override void Shutdown() { }
    }
}

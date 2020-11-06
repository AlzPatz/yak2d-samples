using SampleBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Yak2D;

namespace Input_GamepadUsage
{
    /// <summary>
    /// 
    /// </summary>
    public class GamepadUsage : ApplicationBase
    {
        private const float ON_FRAME_ACTION_DISPLAY_TIME = 0.1f;

        private IDrawStage _drawStage;
        private ICamera2D _camera;

        private bool _gamepadConfigChanged;
        private int? _gamepadId;

        private class ButtonTracker
        {
            public bool Pressed;
            public float Value;
        }

        private Dictionary<GamepadButton, ButtonTracker> _buttons;

        public override string ReturnWindowTitle() => "Gamepad Usage";

        public override void OnStartup()
        {
            //In ApplicationBase of these samples, please note that a Framework Message is sent to the Application
            //When a gamepad is added or removed. The Subscribed to event simply fires when one of those happens
            NumberOfGamepadsChanged += NumberOfGamePadsChanged;

            _gamepadId = null;
            _gamepadConfigChanged = false;
        }

        private void NumberOfGamePadsChanged(object sender, System.EventArgs e)
        {
            _gamepadConfigChanged = true;
        }

        public override bool CreateResources(IServices yak)
        {
            _drawStage = yak.Stages.CreateDrawStage();

            _camera = yak.Cameras.CreateCamera2D();

            return true;
        }

        //BEST practice is to do ALL input HANDLING in the UPDATES
        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds)
        {
            var input = yak.Input;

            //A gamepad was added or removed from the system
            if (_gamepadConfigChanged)
            {
                if (_gamepadId != null)
                {
                    if (!input.ConnectedGamepadIds().Contains((int)_gamepadId))
                    {
                        //We lost connection to the gamepad
                        _gamepadId = null;
                    }
                }

                _gamepadConfigChanged = false;
            }

            //If NULL, then no active gamepad is curently chosen
            if (_gamepadId == null)
            {
                var gamepads = input.ConnectedGamepadIds();

                gamepads.ForEach(id =>
                {
                    if (input.WasGamepadButtonReleasedThisFrame(id, GamepadButton.A))
                    {
                        _gamepadId = id;
                        ResetButtonTrackers();
                    }
                });
            }

            //If not NULL we have an active gamepad and should track input
            if (_gamepadId != null)
            {
                var buttonsPressed = input.GamepadButtonsPressedThisFrame((int)_gamepadId);

                buttonsPressed.ForEach(pressed =>
                {
                    var button = _buttons[pressed];
                    button.Pressed = true;
                    button.Value = 0.0f;
                });

                var buttonsReleased = input.GamepadButtonsReleasedThisFrame((int)_gamepadId);

                buttonsReleased.ForEach(pressed =>
                {
                    var button = _buttons[pressed];
                    button.Pressed = false;
                    button.Value = 1.0f;
                });

                //HERE
                _buttons.Values.ToList().ForEach(button =>
                {
                    if(button.Pressed && button.Value < 1.0f)
                    {
                        button.Value += timeSinceLastUpdateSeconds / ON_FRAME_ACTION_DISPLAY_TIME;
                        if(button.Value > 1.0f)
                        {
                            button.Value = 1.0f;
                        }
                    }

                    if(!button.Pressed && button.Value > 0.0f)
                    {
                        button.Value -= timeSinceLastUpdateSeconds / ON_FRAME_ACTION_DISPLAY_TIME;
                        if (button.Value < 0.0f)
                        {
                            button.Value = 0.0f;
                        }
                    }
                });
            }

            //We can use individual polling rather than getting lists of buttons pressed or released
            //Passing false back to update causes program exit (exist when back released if start held)
            return _gamepadId == null || !((input.WasGamepadButtonReleasedThisFrame((int)_gamepadId, GamepadButton.Back) && input.IsGamepadButtonCurrentlyPressed((int)_gamepadId,  GamepadButton.Start)));
        }

        private void ResetButtonTrackers()
        {
            _buttons = new Dictionary<GamepadButton, ButtonTracker>();
            Enum.GetValues(typeof(GamepadButton)).Cast<GamepadButton>().ToList().ForEach(button =>
            {
                _buttons.Add(button, new ButtonTracker { Pressed = false, Value = 0.0f });
            });
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
            var useless = yak.Input.WasGamepadButtonReleasedThisFrame(0, GamepadButton.A);
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
            var useless = input.WasGamepadButtonPressedThisFrame(0, GamepadButton.A);

            //Waiting for gamepad to be chosen
            if (_gamepadId == null)
            {
                draw.DrawString(_drawStage, CoordinateSpace.Screen, "PRESS A ON ANY GAMEPAD", Colour.White, 64.0f, new Vector2(0.0f, 32.0f), TextJustify.Centre, 0.9f, 0);
            }

            var h = draw.Helpers;

            var outlineDepth = 0.9f;
            var filledDepth = 0.8f;

            var outlineThickness = 2.0f;
            var outlineColour = Colour.White;
            var roundButtonNumSegments = 32;

            var pressColour = Colour.Green;
            var releaseColour = Colour.Red;
            var holdColour = Colour.Blue;

            var faceButtonsCentre = new Vector2(200.0f, 0.0f);
            var faceButtonSpacing = 60.0f;
            var faceButtonRadius = 16.0f;

            var controlButtonsCentre = new Vector2(0.0f, 0.0f);
            var controlButtonSpacing = 60.0f;
            var controlButtonRadius = 8.0f;

            var stickButtonRadius = 16.0f;
            var leftStickCentre = new Vector2(-200.0f, 100.0f);
            var rightStickCentre = new Vector2(150.0f, -100.0f);

            var shoulderButtonsY = 150.0f;
            var shoulderButtonsThickness = 30.0f;
            var shoulderButtonsWidth = 80.0f;
            var leftShoulderButtonX = -200.0f;
            var rightShoulderButtonX = 200.0f;

            var dPadCentre = new Vector2(-100.0f, -150.0f);
            var dPadDisFromOrigin = 24.0f;
            var dPadLen = 24.0f;
            var dPadWidth = 12.0f;

            var triggerBaseY = 200.0f;
            var triggerMaxHeight = 50.0f;
            var triggerWidth = 60.0f;

            var stickGap = 9.0f;
            var stickMaxLength = 20.0f;
            var stickWidth = 12.0f;
            var stickHeadWidth = 20.0f;
            var stickHeadLength = 8.0f;

            var deadzone = 0.1f;

            //We have active gamepad
            if (_gamepadId != null)
            {
                Action<IDrawingHelpers, Vector2, float, ButtonTracker> drawRoundButton = (h, pos, radius, btn) =>
                {
                    var d = h.Construct().Coloured(outlineColour).Poly(pos, roundButtonNumSegments, radius);
                    d.Outline(outlineThickness).SubmitDraw(_drawStage, CoordinateSpace.Screen, outlineDepth, 0);

                    if (btn.Value > 0.0f)
                    {
                        var dFilled = d.Filled();
                        if (btn.Value == 1.0f)
                        {
                            dFilled.ChangeColour(holdColour).SubmitDraw(_drawStage, CoordinateSpace.Screen, filledDepth, 0);
                        }
                        else
                        {
                            if (btn.Pressed)
                            {
                                dFilled.ChangeColour(pressColour).SubmitDraw(_drawStage, CoordinateSpace.Screen, filledDepth, 0);
                            }
                            else
                            {
                                dFilled.ChangeColour(releaseColour).SubmitDraw(_drawStage, CoordinateSpace.Screen, filledDepth, 0);
                            }
                        }
                    }
                };

                drawRoundButton(h, faceButtonsCentre + new Vector2(0.0f, -faceButtonSpacing), faceButtonRadius, _buttons[GamepadButton.A]);
                drawRoundButton(h, faceButtonsCentre + new Vector2(0.0f, faceButtonSpacing), faceButtonRadius, _buttons[GamepadButton.Y]);
                drawRoundButton(h, faceButtonsCentre + new Vector2(-faceButtonSpacing, 0.0f), faceButtonRadius, _buttons[GamepadButton.X]);
                drawRoundButton(h, faceButtonsCentre + new Vector2(faceButtonSpacing, 0.0f), faceButtonRadius, _buttons[GamepadButton.B]);

                drawRoundButton(h, controlButtonsCentre + new Vector2(-0.5f * controlButtonSpacing, 0.0f), controlButtonRadius, _buttons[GamepadButton.Back]);
                drawRoundButton(h, controlButtonsCentre + new Vector2(0.5f * controlButtonSpacing, 0.0f), controlButtonRadius, _buttons[GamepadButton.Start]);

                drawRoundButton(h, leftStickCentre, stickButtonRadius, _buttons[GamepadButton.LeftStick]);
                drawRoundButton(h, rightStickCentre, stickButtonRadius, _buttons[GamepadButton.RightStick]);

                Action<IDrawingHelpers, Vector2, float, float, ButtonTracker> drawRectangleButton = (h, pos, width, height, btn) =>
                {
                    var d = h.Construct().Coloured(outlineColour).Quad(pos, width, height);
                    d.Outline(outlineThickness).SubmitDraw(_drawStage, CoordinateSpace.Screen, outlineDepth, 0);

                    if (btn.Value > 0.0f)
                    {
                        var dFilled = d.Filled();
                        if (btn.Value == 1.0f)
                        {
                            dFilled.ChangeColour(holdColour).SubmitDraw(_drawStage, CoordinateSpace.Screen, filledDepth, 0);
                        }
                        else
                        {
                            if (btn.Pressed)
                            {
                                dFilled.ChangeColour(pressColour).SubmitDraw(_drawStage, CoordinateSpace.Screen, filledDepth, 0);
                            }
                            else
                            {
                                dFilled.ChangeColour(releaseColour).SubmitDraw(_drawStage, CoordinateSpace.Screen, filledDepth, 0);
                            }
                        }
                    }
                };

                drawRectangleButton(h, new Vector2(leftShoulderButtonX, shoulderButtonsY), shoulderButtonsWidth, shoulderButtonsThickness, _buttons[GamepadButton.LeftShoulder]);
                drawRectangleButton(h, new Vector2(rightShoulderButtonX, shoulderButtonsY), shoulderButtonsWidth, shoulderButtonsThickness, _buttons[GamepadButton.RightShoulder]);

                drawRectangleButton(h, dPadCentre + new Vector2(0.0f, dPadDisFromOrigin), dPadWidth, dPadLen, _buttons[GamepadButton.DPadUp]);
                drawRectangleButton(h, dPadCentre - new Vector2(0.0f, dPadDisFromOrigin), dPadWidth, dPadLen, _buttons[GamepadButton.DPadDown]);
                drawRectangleButton(h, dPadCentre + new Vector2(-dPadDisFromOrigin, 0.0f), dPadLen, dPadWidth, _buttons[GamepadButton.DPadLeft]);
                drawRectangleButton(h, dPadCentre + new Vector2(dPadDisFromOrigin, 0.0f), dPadLen, dPadWidth, _buttons[GamepadButton.DPadRight]);

                Action<IDrawingHelpers, Vector2, float, float, float> drawVariableSizedRectangle = (h, posAtBase, width, height, maxHeight) =>
                {
                    var midOutline = posAtBase;
                    midOutline.Y += 0.5f * maxHeight;

                    h.Construct().Coloured(outlineColour).Quad(midOutline, width, maxHeight).Outline(outlineThickness).SubmitDraw(_drawStage, CoordinateSpace.Screen, outlineDepth, 0);

                    if (height > 0.0f)
                    {
                        var midColoured = posAtBase;
                        midColoured.Y += 0.5f * height;

                        h.Construct().Coloured(pressColour).Quad(midColoured, width, height).Filled().SubmitDraw(_drawStage, CoordinateSpace.Screen, filledDepth, 0);
                    }
                };

                var leftTriggerAmount = input.GamepadAxisValue((int)_gamepadId, GamepadAxis.TriggerLeft);
                var rightTriggerAmount = input.GamepadAxisValue((int)_gamepadId, GamepadAxis.TriggerRight);
                drawVariableSizedRectangle(h, new Vector2(leftShoulderButtonX, triggerBaseY), triggerWidth, leftTriggerAmount * triggerMaxHeight, triggerMaxHeight);
                drawVariableSizedRectangle(h, new Vector2(rightShoulderButtonX, triggerBaseY), triggerWidth, rightTriggerAmount * triggerMaxHeight, triggerMaxHeight);

                var leftStick = new Vector2(input.GamepadAxisValue((int)_gamepadId, GamepadAxis.LeftX), input.GamepadAxisValue((int)_gamepadId, GamepadAxis.LeftY));
                var rightStick = new Vector2(input.GamepadAxisValue((int)_gamepadId, GamepadAxis.RightX), input.GamepadAxisValue((int)_gamepadId, GamepadAxis.RightY));

                leftStick.Y = -leftStick.Y;
                rightStick.Y = -rightStick.Y;

                Action<IDrawingHelpers, Vector2, Vector2, float, float> drawStick = (h, centre, value, boundary, maxLength) =>
                {
                    var d = h.Construct().Coloured(outlineColour).Poly(centre, roundButtonNumSegments, stickButtonRadius + (2.0f * boundary) + maxLength);
                    d.Outline(outlineThickness).SubmitDraw(_drawStage, CoordinateSpace.Screen, outlineDepth, 0);

                    if(Math.Abs(value.X) > deadzone || Math.Abs(value.Y) > deadzone)
                    {
                        var dir = Vector2.Normalize(value);

                        h.Construct()
                            .Coloured(pressColour)
                            .Line(centre + (dir * (stickButtonRadius + boundary)), centre + (dir * (stickButtonRadius + boundary) + (value * maxLength)), stickWidth)
                            .Arrow(stickHeadWidth, stickHeadLength)
                            .Filled()
                            .SubmitDraw(_drawStage, CoordinateSpace.Screen, filledDepth, 0);
                    }
                };

                drawStick(h, leftStickCentre, leftStick, stickGap, stickMaxLength);
                drawStick(h, rightStickCentre, rightStick, stickGap, stickMaxLength);
            }
        }

        public override void Rendering(IRenderQueue q, IRenderTarget windowRenderTarget)
        {
            q.ClearColour(windowRenderTarget, _gamepadId == null ? Colour.Red : Colour.Clear);
            q.ClearDepth(windowRenderTarget);
            q.Draw(_drawStage, _camera, windowRenderTarget);
        }

        public override void Shutdown() { }
    }
}
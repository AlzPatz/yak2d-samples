using System.Numerics;
using Yak2D;

namespace SampleBase
{
    public class FlyCam
    {
        public Vector3 Position { get; private set; }

        public Vector3 Up { get; private set; }
        public Vector3 LookDirection { get; private set; }
        public Vector3 Left { get { return Vector3.Cross(Up, LookDirection); } }
        public Vector3 LookAt { get { return Position + (1.0f * LookDirection); } }

        private Vector3 _originalUp;
        private Vector3 _originalLookDirection;
        private Vector3 _originalPosition;

        private Quaternion _cameraRotation;

        public FlyCam(Vector3 position, Vector3 up, Vector3 lookAt)
        {
            _originalUp = up;

            var delta = lookAt - position;
            _originalLookDirection = delta == Vector3.Zero ? -Vector3.UnitZ : Vector3.Normalize(delta);

            _originalPosition = position;

            Reset();
        }
        
        public void Reset()
        {
            Position = _originalPosition;
            Up = _originalUp;
            LookDirection = _originalLookDirection;
            _cameraRotation = Quaternion.Identity;
        }

        public void SetPosition(Vector3 position)
        {
            Position = position;
        }

        public void RollLeft(float rads)
        {
            var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, rads);

            _cameraRotation *= rotation;

            Calculate();
        }

        public void PitchUp(float rads)
        {
            var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, rads);

            _cameraRotation *= rotation;

            Calculate();
        }

        public void YawLeft(float rads)
        {
            var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, rads);

            _cameraRotation *= rotation;

            Calculate();
        }

        public void MoveForward(float distance)
        {
            Position += LookDirection * distance;
        }

        public void MoveLeft(float distance)
        {
            Position += Left * distance;
        }

        public void MoveUp(float distance)
        {
            Position += Up * distance;
        }

        private void Calculate()
        {
            Up = Vector3.Transform(_originalUp, _cameraRotation);
            LookDirection = Vector3.Transform(_originalLookDirection, _cameraRotation);
        }

        public void UpdateInputWithDefaultControls(IInput input, float move_speed, float rotate_speed, float seconds)
        {
            if (input.IsKeyCurrentlyPressed(KeyCode.W))
            {
                MoveForward(move_speed * seconds);
            }

            if (input.IsKeyCurrentlyPressed(KeyCode.S))
            {
                MoveForward(-move_speed * seconds);
            }

            if (input.IsKeyCurrentlyPressed(KeyCode.A))
            {
                MoveLeft(move_speed * seconds);
            }

            if (input.IsKeyCurrentlyPressed(KeyCode.D))
            {
                MoveLeft(-move_speed * seconds);
            }

            if (input.IsKeyCurrentlyPressed(KeyCode.Q))
            {
                MoveUp(move_speed * seconds);
            }

            if (input.IsKeyCurrentlyPressed(KeyCode.Z))
            {
                MoveUp(-move_speed * seconds);
            }

            if (input.IsKeyCurrentlyPressed(KeyCode.Left))
            {
                RollLeft(rotate_speed * seconds);
            }

            if (input.IsKeyCurrentlyPressed(KeyCode.Right))
            {
                RollLeft(-rotate_speed * seconds);
            }

            if (input.IsKeyCurrentlyPressed(KeyCode.Up))
            {
                //Flight stick style, press up to nose down
                PitchUp(-rotate_speed * seconds);
            }

            if (input.IsKeyCurrentlyPressed(KeyCode.Down))
            {
                //Flight stick style, press down to nose up
                PitchUp(rotate_speed * seconds);
            }

            if (input.IsKeyCurrentlyPressed(KeyCode.Delete))
            {
                YawLeft(rotate_speed * seconds);
            }

            if (input.IsKeyCurrentlyPressed(KeyCode.PageDown))
            {
                YawLeft(-rotate_speed * seconds);
            }
        }
    }
}
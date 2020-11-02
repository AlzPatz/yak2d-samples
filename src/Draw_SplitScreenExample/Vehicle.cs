using System;
using System.Numerics;
using Yak2D;

namespace Draw_SplitScreenExample
{
    public class Vehicle
    {
        public Vector2 Position { get; internal set; }
        public float Angle { get; internal set; }
        public float Speed { get; internal set; }

        private readonly KeyCode _accelerate;
        private readonly KeyCode _brake;
        private readonly KeyCode _left;
        private readonly KeyCode _right;

        private readonly float TANK_MAX_SPEED;
        private readonly float TANK_MAX_TURN_SPEED;
        private readonly float TANK_MAX_TURN_RATE;
        private readonly float TANK_ACCELERATION;
        private readonly float TANK_DECELERATION;
        private readonly float TANK_ROLLING_DECELERATION;

        public Vehicle(float startX,
                       float startY,
                       KeyCode accelerate,
                       KeyCode brake,
                       KeyCode left,
                       KeyCode right,
                       float TANK_MAX_SPEED,
                       float TANK_MAX_TURN_SPEED,
                       float TANK_MAX_TURN_RATE,
                       float TANK_ACCELERATION,
                       float TANK_DECELERATION,
                       float TANK_ROLLING_DECELERATION)
        {
            Position = new Vector2(startX, startY);
            Angle = 0.0f;
            Speed = 0.0f;

            _accelerate = accelerate;
            _brake = brake;
            _left = left;
            _right = right;

            this.TANK_MAX_SPEED = TANK_MAX_SPEED;
            this.TANK_MAX_TURN_SPEED = TANK_MAX_TURN_SPEED;
            this.TANK_MAX_TURN_RATE = TANK_MAX_TURN_RATE;
            this.TANK_ACCELERATION = TANK_ACCELERATION;
            this.TANK_DECELERATION = TANK_DECELERATION;
            this.TANK_ROLLING_DECELERATION = TANK_ROLLING_DECELERATION;
        }

        internal void Update(IInput input, float timeSinceLastUpdateSeconds)
        {
            //Update Speed

            var acceleration = 0.0f;

            if (input.IsKeyCurrentlyPressed(_accelerate))
            {
                acceleration = 1.0f;
            }

            if (input.IsKeyCurrentlyPressed(_brake))
            {
                acceleration = -1.0f;
            }

            var acceleration_amount = 0.0f;

            if (acceleration == 1.0f)
            {
                acceleration_amount = Speed >= 0.0f ? TANK_ACCELERATION : TANK_DECELERATION;
            }

            if (acceleration == -1.0f)
            {
                acceleration_amount = Speed >= 0.0f ? TANK_DECELERATION : TANK_ACCELERATION;
            }

            Speed += acceleration * acceleration_amount * timeSinceLastUpdateSeconds;

            var s = (float)Math.Abs(Speed);
            if (s > TANK_MAX_SPEED)
            {
                Speed *= TANK_MAX_SPEED / s;
            }

            var minSpeed = 0.001f;
            if (acceleration == 0.0f && Math.Abs(Speed) > minSpeed)
            {
                var decceleration = -TANK_ROLLING_DECELERATION * timeSinceLastUpdateSeconds;

                if (Speed < 0.0f)
                {
                    decceleration *= -1.0f;
                }

                if (Math.Abs(decceleration) > Math.Abs(Speed))
                {
                    decceleration *= Math.Abs(Speed) / Math.Abs(decceleration);
                }

                Speed += decceleration;
            }

            if (Math.Abs(Speed) < minSpeed)
            {
                Speed = 0.0f;
            }

            //Update Angle

            var turn = 0.0f;

            if (input.IsKeyCurrentlyPressed(_left))
            {
                turn = Speed >= 0.0f ? -1.0f : 1.0f;
            }

            if (input.IsKeyCurrentlyPressed(_right))
            {
                turn = Speed >= 0.0f ? 1.0f : -1.0f;
            }

            //This turn scaling doesn't work properly, but it's not important to the demo so leaving for now
            var turnScale = 0.0f;
            var speed = (float)Math.Abs(Speed);
            if (speed < TANK_MAX_TURN_SPEED)
            {
                turnScale = speed / TANK_MAX_TURN_SPEED;
            }
            else
            {
                turnScale = 1.0f - ((speed - TANK_MAX_TURN_SPEED) / TANK_MAX_TURN_SPEED);
            }

            Angle += turnScale * turn * TANK_MAX_TURN_RATE * timeSinceLastUpdateSeconds;

            var twoPi = (float)Math.PI * 2.0f;
            while (Angle > twoPi)
            {
                Angle -= twoPi;
            }

            while (Angle < 0.0f)
            {
                Angle += twoPi;
            }

            //Update Position

            var direction = Vector2.Transform(Vector2.UnitY, Matrix3x2.CreateRotation(Angle));

            Position += direction * Speed * timeSinceLastUpdateSeconds;
        }
    }
}
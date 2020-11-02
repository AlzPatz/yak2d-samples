using System;

namespace Draw_SplitScreenExample
{
    public class TrackingCamera
    {
        public float Angle { get; internal set; }
        public float Zoom { get; internal set; }

        private readonly float _minZoom;
        private readonly float _maxZoom;
        private readonly float _secondsToSmoothHalf;
        private readonly float _tankMaxSpeed;

        public TrackingCamera(float angle,
                              float MAX_ZOOM,
                              float MIN_ZOOM,
                              float SECONDS_TO_SMOOTH_HALF,
                              float TANK_MAX_SPEED)
        {
            Angle = angle;
            Zoom = MAX_ZOOM;

            _minZoom = MIN_ZOOM;
            _maxZoom = MAX_ZOOM;
            _secondsToSmoothHalf = SECONDS_TO_SMOOTH_HALF;
            _tankMaxSpeed = TANK_MAX_SPEED;
        }

        internal void Update(Vehicle tank, float seconds)
        {
            var speed = (float)Math.Abs(tank.Speed);
            var targetZoom = _minZoom + ((_maxZoom - _minZoom) * (1.0f - (speed / _tankMaxSpeed)));

            var twoPi = (float)Math.PI * 2.0f;
            if (Math.Abs(Angle - tank.Angle) > Math.PI)
            {
                if (tank.Angle < Angle)
                {
                    Angle -= twoPi;
                }
                else
                {
                    Angle += twoPi;
                }
            }

            Angle = SmoothTowards(Angle, tank.Angle, seconds);
            Zoom = SmoothTowards(Zoom, targetZoom, seconds);
        }

        private float SmoothTowards(float current, float target, float seconds)
        {
            var diff = target - current;
            var half = 0.5f * diff;
            var amount_in_period = half * (seconds / _secondsToSmoothHalf);

            return current + amount_in_period;
        }
    }
}
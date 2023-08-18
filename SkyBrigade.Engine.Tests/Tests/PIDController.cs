namespace Horizon.Tests.Tests
{
    internal class PIDController
    {
        public float kP, kI, kD;
        public float integral, previousError;

        public float Min;
        public float Max;

        public PIDController(float kP, float kI, float kD, float min = -1.0f, float max = 1.0f)
        {
            this.kP = kP;
            this.kI = kI;
            this.kD = kD;

            this.Min = min;
            this.Max = max;
        }

        private float Clamp(float variableToClamp)
        {
            if (variableToClamp <= Min) { return Min; }
            if (variableToClamp >= Max) { return Max; }
            return variableToClamp;
        }

        public float Update(float error, float dt)
        {
            integral += error * dt;
            var derivative = (error - previousError) / dt;

            var output = (kP * error + kI * integral + kD * derivative);

            previousError = error;

            return Clamp(output);
        }
    }
}
namespace Mirage
{
    // implementation of N-day EMA
    // it calculates an exponential moving average roughly equivalent to the last n observations
    // https://en.wikipedia.org/wiki/Moving_average#Exponential_moving_average
    public class ExponentialMovingAverage
    {
        private readonly float _alpha;
        private bool _initialized;

        public ExponentialMovingAverage(int n)
        {
            // standard N-day EMA alpha calculation
            _alpha = 2.0f / (n + 1);
        }

        public void Reset()
        {
            _initialized = false;
            Value = 0;
            Var = 0;
        }

        public void Add(double newValue)
        {
            // simple algorithm for EMA described here:
            // https://en.wikipedia.org/wiki/Moving_average#Exponentially_weighted_moving_variance_and_standard_deviation
            if (_initialized)
            {
                var delta = newValue - Value;
                Value += _alpha * delta;
                Var = (1 - _alpha) * (Var + (_alpha * delta * delta));
            }
            else
            {
                Value = newValue;
                _initialized = true;
            }
        }

        public double Value { get; private set; }

        public double Var { get; private set; }
    }
}

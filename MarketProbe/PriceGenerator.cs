using System;

namespace MarketProbe
{
    public class PriceGenerator
    {
        private double _numberOfSamples;

        public double NumberOfSamples
        {
            get { return _numberOfSamples; }
            set { _numberOfSamples = value; }
        }

        private double _initialValue;

        public double InitialValue
        {
            get { return _initialValue; }
            set { _initialValue = value; }
        }

        private double _mu;

        public double Mu
        {
            get { return _mu; }
            set { _mu = value; }
        }

        private double _sigma;

        public double Sigma
        {
            get { return _sigma; }
            set { _sigma = value; }
        }

        private double _t;

        public double T
        {
            get { return _t; }
            set { _t = value; }
        }


        private double _dt;
        private double _st;
        private Random _rng;

        private double _currentPrice;

        public PriceGenerator(int seed, double t, double numberOfSamples, double initialValue, double mu, double sigma, double spread)
        {
            _t = t;
            _numberOfSamples = numberOfSamples;
            _initialValue = initialValue;
            _mu = mu;
            _sigma = sigma;
            _spread = spread;

            _dt = _t / (_numberOfSamples - 1);
            _st = initialValue;

            _currentPrice = _st;

            _rng = new Random(seed);
        }

        private double _spread;

        public double Spread
        {
            get { return _spread; }
            set { _spread = value; }
        }

        public Quote NextQuote
        {
            get
            {
                var nextQuote = new Quote() { Bid = _currentPrice + (_spread/2), Ask = _currentPrice - (_spread/2) };

                double volatility = 0.001;
                double rnd = _rng.NextDouble(); // generate number, 0 <= x < 1.0
                double change_percent = 2d * volatility * rnd;
                if (change_percent > volatility)
                    change_percent -= (2 * volatility);
                var change_amount = _currentPrice * change_percent;
                _currentPrice = _currentPrice + change_amount;


                return nextQuote;
            }
        }
    }
}

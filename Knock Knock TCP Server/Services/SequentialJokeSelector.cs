using Knock_Knock_TCP_Server.Configuration;

namespace Knock_Knock_TCP_Server.Services
{
    /// <summary>
    /// Sequential joke selection strategy.
    /// Cycles through jokes in order. When the last joke is reached, wraps back to the first.
    /// 
    /// Each ClientHandler instance gets its own SequentialJokeSelector, so the index
    /// is per-client and independent. No synchronization needed.
    /// </summary>
    public class SequentialJokeSelector : IJokeSelector
    {
        private int _currentIndex = 0;

        public string StrategyName => "Sequential";

        public JokeModel SelectJoke(List<JokeModel> jokes)
        {
            if (jokes == null || jokes.Count == 0)
                throw new InvalidOperationException("No jokes available for selection.");

            var selectedJoke = jokes[_currentIndex];

            // Move to next index, wrapping around if necessary
            _currentIndex = (_currentIndex + 1) % jokes.Count;

            return selectedJoke;
        }
    }
}

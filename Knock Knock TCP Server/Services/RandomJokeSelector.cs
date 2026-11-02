using Knock_Knock_TCP_Server.Configuration;

namespace Knock_Knock_TCP_Server.Services
{
    /// <summary>
    /// Random joke selection strategy.
    /// Selects a random joke from the available list each time.
    /// This strategy is stateless and inherently thread-safe.
    /// </summary>
    public class RandomJokeSelector : IJokeSelector
    {
        private static readonly Random _random = new Random();

        public string StrategyName => "Random";

        public JokeModel SelectJoke(List<JokeModel> jokes)
        {
            if (jokes == null || jokes.Count == 0)
                throw new InvalidOperationException("No jokes available for selection.");

            int randomIndex = _random.Next(jokes.Count);
            return jokes[randomIndex];
        }
    }
}

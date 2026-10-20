namespace Knock_Knock_TCP_Server.Services
{
    /// <summary>
    /// Factory for creating IJokeSelector instances based on strategy name from configuration.
    /// </summary>
    internal class JokeSelectorFactory
    {
        /// <summary>
        /// Creates a joke selector based on the provided strategy name.
        /// </summary>
        /// <param name="strategyName">Name of the strategy ("Random" or "Sequential")</param>
        /// <returns>An instance of IJokeSelector</returns>
        /// <exception cref="ArgumentException">If strategy name is not recognized</exception>
        public static IJokeSelector CreateSelector(string strategyName)
        {
            switch (strategyName?.ToLower ())
            {
                case "random":
                    return new RandomJokeSelector ();
                case "sequential":
                    return new SequentialJokeSelector ();
                default:
                    throw new ArgumentException ($"Unknown joke selection strategy: '{strategyName}'. " +
                        $"Supported strategies: 'Random', 'Sequential'");
            }
        }
    }
}

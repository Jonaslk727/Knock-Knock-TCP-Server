using Knock_Knock_TCP_Server.Configuration;

namespace Knock_Knock_TCP_Server.Services
{
    /// <summary>
    /// Strategy interface for selecting which knock-knock joke to tell.
    /// Implementations allow for different joke selection algorithms (Random, Sequential, etc.)
    /// </summary>
    internal interface IJokeSelector
    {
        /// <summary>
        /// Selects the next joke based on the strategy's algorithm.
        /// </summary>
        /// <param name="jokes">The list of available jokes</param>
        /// <returns>The selected joke</returns>
        JokeModel SelectJoke(List<JokeModel> jokes);

        /// <summary>
        /// Gets the name/identifier of this strategy for logging purposes.
        /// </summary>
        string StrategyName { get; }
    }
}

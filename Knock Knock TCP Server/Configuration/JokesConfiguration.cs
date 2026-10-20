namespace Knock_Knock_TCP_Server.Configuration
{
    /// <summary>
    /// Strongly typed options for jokes and selection strategy configuration.
    /// Consolidates JokeOptions and JokeSelectionOptions into a single configuration class.
    /// Bound from appsettings.json ["Jokes"] and ["JokeStrategy"] sections via IOptions pattern.
    /// </summary>
    public class JokesConfiguration
    {
        /// <summary>
        /// List of available knock-knock jokes.
        /// Must contain at least 5 jokes.
        /// Bound from appsettings.json ["Jokes"]["Jokes"] section.
        /// </summary>
        public List<JokeModel> Jokes { get; set; } = new List<JokeModel>();

        /// <summary>
        /// The strategy to use for selecting jokes: "Random" or "Sequential".
        /// Bound from appsettings.json ["JokeStrategy"]["Strategy"] section.
        /// </summary>
        public string SelectionStrategy { get; set; } = "Random";
    }
}

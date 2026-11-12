using Microsoft.Extensions.Options;

namespace Knock_Knock_TCP_Server.Configuration.Validation
{
    /// <summary>
    /// Validates JokesConfiguration for correct values at startup.
    /// Consolidates validation logic from JokeOptionsValidator and JokeSelectionOptionsValidator.
    /// </summary>
    public class JokesConfigurationValidator : IValidateOptions<JokesConfiguration>
    {
        public ValidateOptionsResult Validate(string? name, JokesConfiguration options)
        {
            if (options == null)
                return ValidateOptionsResult.Fail("JokesConfiguration is null");

            // Validate jokes collection
            if (options.Jokes == null || options.Jokes.Count == 0)
                return ValidateOptionsResult.Fail(
                    "Jokes list is empty. Please configure at least one joke in appsettings.json");

            if (options.Jokes.Count < 5)
                return ValidateOptionsResult.Fail(
                    $"At least 5 jokes are required. Currently configured: {options.Jokes.Count}");

            // Validate each joke
            for (int i = 0; i < options.Jokes.Count; i++)
            {
                var joke = options.Jokes[i];
                if (string.IsNullOrWhiteSpace(joke.Who))
                    return ValidateOptionsResult.Fail(
                        $"Joke at index {i} has an empty 'Who' field");

                if (string.IsNullOrWhiteSpace(joke.Punchline))
                    return ValidateOptionsResult.Fail(
                        $"Joke at index {i} ('{joke.Who}...') has an empty 'Punchline' field");
            }

            // Validate selection strategy
            if (string.IsNullOrWhiteSpace(options.Strategy))
                return ValidateOptionsResult.Fail(
                    "JokeStrategy.Strategy cannot be empty. Use 'Random' or 'Sequential'");

            var validStrategies = new[] { "Random", "Sequential", "random", "sequential" };
            if (!validStrategies.Contains(options.Strategy))
                return ValidateOptionsResult.Fail(
                    $"JokeStrategy.Strategy '{options.Strategy}' is not recognized. " +
                    $"Supported strategies: 'Random', 'Sequential'");

            return ValidateOptionsResult.Success;
        }
    }
}

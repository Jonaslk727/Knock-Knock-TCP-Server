using Knock_Knock_TCP_Server.Configuration;
using Knock_Knock_TCP_Server.Configuration.Validation;
using Microsoft.Extensions.Options;
using Xunit;

namespace Knock_Knock_TCP_Server.Tests
{
    public class JokesConfigurationValidatorTests
    {
        [Fact]
        public void Validate_WithValidJokes_ReturnsSuccess()
        {
            // Arrange
            var validator = new JokesConfigurationValidator();
            var options = new JokesConfiguration
            {
                Jokes = new List<JokeModel>
                {
                    new JokeModel { Who = "Lettuce", Punchline = "Lettuce in, it's cold out here!" },
                    new JokeModel { Who = "Boo", Punchline = "Don't cry, it's only a joke!" }
                },
                Strategy = "Random"
            };

            // Act
            var result = validator.Validate("Jokes", options);

            // Assert
            Assert.Equal(ValidateOptionsResult.Success, result);
        }

        [Fact]
        public void Validate_WithValidSequentialStrategy_ReturnsSuccess()
        {
            // Arrange
            var validator = new JokesConfigurationValidator();
            var options = new JokesConfiguration
            {
                Jokes = new List<JokeModel>
                {
                    new JokeModel { Who = "Knock", Punchline = "Punchline 1" }
                },
                Strategy = "Sequential"
            };

            // Act
            var result = validator.Validate("Jokes", options);

            // Assert
            Assert.Equal(ValidateOptionsResult.Success, result);
        }

        [Fact]
        public void Validate_WithEmptyJokesList_ReturnsFail()
        {
            // Arrange
            var validator = new JokesConfigurationValidator();
            var options = new JokesConfiguration
            {
                Jokes = new List<JokeModel>(),
                Strategy = "Random"
            };

            // Act
            var result = validator.Validate("Jokes", options);

            // Assert
            Assert.NotEqual(ValidateOptionsResult.Success, result);
            Assert.NotNull(result.Failures);
            Assert.NotEmpty(result.Failures);
        }

        [Fact]
        public void Validate_WithNullJokesList_ReturnsFail()
        {
            // Arrange
            var validator = new JokesConfigurationValidator();
            var options = new JokesConfiguration
            {
                Jokes = null,
                Strategy = "Random"
            };

            // Act
            var result = validator.Validate("Jokes", options);

            // Assert
            Assert.NotEqual(ValidateOptionsResult.Success, result);
            Assert.NotNull(result.Failures);
            Assert.NotEmpty(result.Failures);
        }

        [Theory]
        [InlineData("InvalidStrategy")]
        [InlineData("")]
        [InlineData("random")]  // Case sensitivity test
        [InlineData("Shuffle")]
        public void Validate_WithInvalidStrategy_ReturnsFail(string invalidStrategy)
        {
            // Arrange
            var validator = new JokesConfigurationValidator();
            var options = new JokesConfiguration
            {
                Jokes = new List<JokeModel>
                {
                    new JokeModel { Who = "Test", Punchline = "Test punchline" }
                },
                Strategy = invalidStrategy
            };

            // Act
            var result = validator.Validate("Jokes", options);

            // Assert
            Assert.NotEqual(ValidateOptionsResult.Success, result);
            Assert.NotNull(result.Failures);
            Assert.NotEmpty(result.Failures);
        }

        [Fact]
        public void Validate_WithJokeWithoutWho_ReturnsFail()
        {
            // Arrange
            var validator = new JokesConfigurationValidator();
            var options = new JokesConfiguration
            {
                Jokes = new List<JokeModel>
                {
                    new JokeModel { Who = null, Punchline = "Some punchline" }
                },
                Strategy = "Random"
            };

            // Act
            var result = validator.Validate("Jokes", options);

            // Assert
            Assert.NotEqual(ValidateOptionsResult.Success, result);
            Assert.NotNull(result.Failures);
            Assert.NotEmpty(result.Failures);
        }

        [Fact]
        public void Validate_WithJokeWithoutPunchline_ReturnsFail()
        {
            // Arrange
            var validator = new JokesConfigurationValidator();
            var options = new JokesConfiguration
            {
                Jokes = new List<JokeModel>
                {
                    new JokeModel { Who = "Who", Punchline = null }
                },
                Strategy = "Random"
            };

            // Act
            var result = validator.Validate("Jokes", options);

            // Assert
            Assert.NotEqual(ValidateOptionsResult.Success, result);
            Assert.NotNull(result.Failures);
            Assert.NotEmpty(result.Failures);
        }

        [Fact]
        public void Validate_WithSingleValidJoke_ReturnsSuccess()
        {
            // Arrange
            var validator = new JokesConfigurationValidator();
            var options = new JokesConfiguration
            {
                Jokes = new List<JokeModel>
                {
                    new JokeModel { Who = "Solo", Punchline = "I'm alone!" }
                },
                Strategy = "Random"
            };

            // Act
            var result = validator.Validate("Jokes", options);

            // Assert
            Assert.Equal(ValidateOptionsResult.Success, result);
        }

        [Fact]
        public void Validate_ErrorMessageMentionsStrategy_OnInvalidStrategy()
        {
            // Arrange
            var validator = new JokesConfigurationValidator();
            var options = new JokesConfiguration
            {
                Jokes = new List<JokeModel>
                {
                    new JokeModel { Who = "Test", Punchline = "Test" }
                },
                Strategy = "BadStrategy"
            };

            // Act
            var result = validator.Validate("Jokes", options);

            // Assert
            Assert.NotEqual(ValidateOptionsResult.Success, result);
            Assert.NotNull(result.Failures);
            var failures = result.Failures.ToList();
            Assert.NotEmpty(failures);
            Assert.True(failures.Any(f => f.Contains("Strategy", StringComparison.OrdinalIgnoreCase) || 
                                          f.Contains("Random", StringComparison.OrdinalIgnoreCase) ||
                                          f.Contains("Sequential", StringComparison.OrdinalIgnoreCase)),
                        "Error message should mention strategy options");
        }

        [Fact]
        public void Validate_ErrorMessageMentionsJokes_OnEmptyList()
        {
            // Arrange
            var validator = new JokesConfigurationValidator();
            var options = new JokesConfiguration
            {
                Jokes = new List<JokeModel>(),
                Strategy = "Random"
            };

            // Act
            var result = validator.Validate("Jokes", options);

            // Assert
            Assert.NotEqual(ValidateOptionsResult.Success, result);
            Assert.NotNull(result.Failures);
            var failures = result.Failures.ToList();
            Assert.NotEmpty(failures);
            Assert.True(failures.Any(f => f.Contains("Jokes", StringComparison.OrdinalIgnoreCase) ||
                                          f.Contains("empty", StringComparison.OrdinalIgnoreCase) ||
                                          f.Contains("at least", StringComparison.OrdinalIgnoreCase)),
                        "Error message should indicate jokes list is empty");
        }

        [Fact]
        public void Validate_WithLargeJokesList_ReturnsSuccess()
        {
            // Arrange
            var validator = new JokesConfigurationValidator();
            var jokes = Enumerable.Range(1, 100)
                .Select(i => new JokeModel { Who = $"Setup{i}", Punchline = $"Punch{i}" })
                .ToList();

            var options = new JokesConfiguration
            {
                Jokes = jokes,
                Strategy = "Random"
            };

            // Act
            var result = validator.Validate("Jokes", options);

            // Assert
            Assert.Equal(ValidateOptionsResult.Success, result);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Knock_Knock_TCP_Server.Configuration;
using Knock_Knock_TCP_Server.Configuration.Validation;
using Microsoft.Extensions.Options;
using Xunit;

namespace Knock_Knock_TCP_Server.Tests.Client
{
    public class JokesConfigurationValidatorTests
    {
        [Fact]
        public void Validate_WithValidJokes_ReturnsSuccess()
        {
            var validator = new JokesConfigurationValidator();
            var options = new JokesConfiguration
            {
                Jokes = new List<JokeModel>
                {
                    new JokeModel { Who = "Lettuce", Punchline = "Lettuce in!" },
                    new JokeModel { Who = "Boo", Punchline = "Don't cry!" },
                    new JokeModel { Who = "Tank", Punchline = "You're welcome!" },
                    new JokeModel { Who = "Interrupt", Punchline = "MOOOO!" },
                    new JokeModel { Who = "Dozen", Punchline = "Dozen anybody want to let me in?" }
                },
                SelectionStrategy = "Random"
            };
            var result = validator.Validate("Jokes", options);
            Assert.Equal(ValidateOptionsResult.Success, result);
        }

        [Fact]
        public void Validate_WithValidSequentialStrategy_ReturnsSuccess()
        {
            var validator = new JokesConfigurationValidator();
            var options = new JokesConfiguration
            {
                Jokes = new List<JokeModel>
                {
                    new JokeModel { Who = "Knock", Punchline = "Punchline 1" },
                    new JokeModel { Who = "Joke2", Punchline = "Punchline 2" },
                    new JokeModel { Who = "Joke3", Punchline = "Punchline 3" },
                    new JokeModel { Who = "Joke4", Punchline = "Punchline 4" },
                    new JokeModel { Who = "Joke5", Punchline = "Punchline 5" }
                },
                SelectionStrategy = "Sequential"
            };
            var result = validator.Validate("Jokes", options);
            Assert.Equal(ValidateOptionsResult.Success, result);
        }

        [Fact]
        public void Validate_WithEmptyJokesList_ReturnsFail()
        {
            var validator = new JokesConfigurationValidator();
            var options = new JokesConfiguration
            {
                Jokes = new List<JokeModel>(),
                SelectionStrategy = "Random"
            };
            var result = validator.Validate("Jokes", options);
            Assert.NotEqual(ValidateOptionsResult.Success, result);
        }

        [Fact]
        public void Validate_WithNullJokesList_ReturnsFail()
        {
            var validator = new JokesConfigurationValidator();
            var options = new JokesConfiguration { Jokes = null, SelectionStrategy = "Random" };
            var result = validator.Validate("Jokes", options);
            Assert.NotEqual(ValidateOptionsResult.Success, result);
        }

        [Theory]
        [InlineData("InvalidStrategy")]
        [InlineData("")]
        [InlineData("Shuffle")]
        public void Validate_WithInvalidStrategy_ReturnsFail(string invalidStrategy)
        {
            var validator = new JokesConfigurationValidator();
            var options = new JokesConfiguration
            {
                Jokes = new List<JokeModel>
                {
                    new JokeModel { Who = "Test", Punchline = "Test punchline" }
                },
                SelectionStrategy = invalidStrategy
            };
            var result = validator.Validate("Jokes", options);
            Assert.NotEqual(ValidateOptionsResult.Success, result);
        }

        [Fact]
        public void Validate_WithJokeWithoutWho_ReturnsFail()
        {
            var validator = new JokesConfigurationValidator();
            var options = new JokesConfiguration
            {
                Jokes = new List<JokeModel>
                {
                    new JokeModel { Who = null, Punchline = "Some punchline" }
                },
                SelectionStrategy = "Random"
            };
            var result = validator.Validate("Jokes", options);
            Assert.NotEqual(ValidateOptionsResult.Success, result);
        }

        [Fact]
        public void Validate_WithJokeWithoutPunchline_ReturnsFail()
        {
            var validator = new JokesConfigurationValidator();
            var options = new JokesConfiguration
            {
                Jokes = new List<JokeModel>
                {
                    new JokeModel { Who = "Who", Punchline = null }
                },
                SelectionStrategy = "Random"
            };
            var result = validator.Validate("Jokes", options);
            Assert.NotEqual(ValidateOptionsResult.Success, result);
        }

        [Fact]
        public void Validate_WithSingleValidJoke_ReturnsSuccess()
        {
            var validator = new JokesConfigurationValidator();
            var options = new JokesConfiguration
            {
                Jokes = new List<JokeModel>
                {
                    new JokeModel { Who = "Solo", Punchline = "I'm alone!" },
                    new JokeModel { Who = "Joke2", Punchline = "Punchline 2" },
                    new JokeModel { Who = "Joke3", Punchline = "Punchline 3" },
                    new JokeModel { Who = "Joke4", Punchline = "Punchline 4" },
                    new JokeModel { Who = "Joke5", Punchline = "Punchline 5" }
                },
                SelectionStrategy = "Random"
            };
            var result = validator.Validate("Jokes", options);
            Assert.Equal(ValidateOptionsResult.Success, result);
        }

        [Fact]
        public void Validate_WithLargeJokesList_ReturnsSuccess()
        {
            var validator = new JokesConfigurationValidator();
            var jokes = Enumerable.Range(1, 100)
                .Select(i => new JokeModel { Who = $"Setup{i}", Punchline = $"Punch{i}" })
                .ToList();
            var options = new JokesConfiguration { Jokes = jokes, SelectionStrategy = "Random" };
            var result = validator.Validate("Jokes", options);
            Assert.Equal(ValidateOptionsResult.Success, result);
        }
    }
}

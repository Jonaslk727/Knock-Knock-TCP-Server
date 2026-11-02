using System;
using System.Collections.Generic;
using System.Linq;
using Knock_Knock_TCP_Server.Configuration;
using Knock_Knock_TCP_Server.Services;
using Xunit;

namespace Knock_Knock_TCP_Server.Tests.Client
{
    public class RandomJokeSelectorTests
    {
        [Fact]
        public void SelectJoke_WithSingleJoke_ReturnsThatJoke()
        {
            var selector = new RandomJokeSelector();
            var jokes = new List<JokeModel>
            {
                new JokeModel { Who = "OnlyOne", Punchline = "Only joke" }
            };

            var selectedJoke = selector.SelectJoke(jokes);

            Assert.NotNull(selectedJoke);
            Assert.Equal("OnlyOne", selectedJoke.Who);
        }

        [Fact]
        public void SelectJoke_WithMultipleJokes_ReturnsValidJoke()
        {
            var selector = new RandomJokeSelector();
            var jokes = new List<JokeModel>
            {
                new JokeModel { Who = "Joke1", Punchline = "Punchline1" },
                new JokeModel { Who = "Joke2", Punchline = "Punchline2" },
                new JokeModel { Who = "Joke3", Punchline = "Punchline3" }
            };

            var selectedJoke = selector.SelectJoke(jokes);

            Assert.NotNull(selectedJoke);
            Assert.True(jokes.Contains(selectedJoke));
        }

        [Fact]
        public void SelectJoke_WithEmptyList_ThrowsInvalidOperationException()
        {
            var selector = new RandomJokeSelector();
            var jokes = new List<JokeModel>();

            Assert.Throws<InvalidOperationException>(() => selector.SelectJoke(jokes));
        }

        [Fact]
        public void SelectJoke_WithNullList_ThrowsInvalidOperationException()
        {
            var selector = new RandomJokeSelector();
            Assert.Throws<InvalidOperationException>(() => selector.SelectJoke(null!));
        }

        [Fact]
        public void SelectJoke_MultipleCallsProduceVariety()
        {
            var selector = new RandomJokeSelector();
            var jokes = new List<JokeModel>
            {
                new JokeModel { Who = "Joke1", Punchline = "Punchline1" },
                new JokeModel { Who = "Joke2", Punchline = "Punchline2" },
                new JokeModel { Who = "Joke3", Punchline = "Punchline3" },
                new JokeModel { Who = "Joke4", Punchline = "Punchline4" },
                new JokeModel { Who = "Joke5", Punchline = "Punchline5" }
            };

            var selectedJokes = new HashSet<string>();
            for (int i = 0; i < 50; i++)
            {
                var selected = selector.SelectJoke(jokes);
                selectedJokes.Add(selected.Who);
            }

            Assert.True(selectedJokes.Count > 1);
        }

        [Fact]
        public void StrategyName_ReturnsRandom()
        {
            var selector = new RandomJokeSelector();
            Assert.Equal("Random", selector.StrategyName);
        }
    }

    public class SequentialJokeSelectorTests
    {
        [Fact]
        public void SelectJoke_WithSingleJoke_AlwaysReturnsSameJoke()
        {
            var selector = new SequentialJokeSelector();
            var jokes = new List<JokeModel>
            {
                new JokeModel { Who = "OnlyOne", Punchline = "Only joke" }
            };

            var joke1 = selector.SelectJoke(jokes);
            var joke2 = selector.SelectJoke(jokes);
            var joke3 = selector.SelectJoke(jokes);

            Assert.Equal("OnlyOne", joke1.Who);
            Assert.Equal("OnlyOne", joke2.Who);
            Assert.Equal("OnlyOne", joke3.Who);
        }

        [Fact]
        public void SelectJoke_WithMultipleJokes_CyclesThroughInOrder()
        {
            var selector = new SequentialJokeSelector();
            var jokes = new List<JokeModel>
            {
                new JokeModel { Who = "Joke1", Punchline = "Punchline1" },
                new JokeModel { Who = "Joke2", Punchline = "Punchline2" },
                new JokeModel { Who = "Joke3", Punchline = "Punchline3" }
            };

            var results = new List<string>();
            for (int i = 0; i < 6; i++)
            {
                results.Add(selector.SelectJoke(jokes).Who);
            }

            Assert.Equal(new[] { "Joke1", "Joke2", "Joke3", "Joke1", "Joke2", "Joke3" }, results);
        }

        [Fact]
        public void SelectJoke_StartsWithFirstJoke()
        {
            var selector = new SequentialJokeSelector();
            var jokes = new List<JokeModel>
            {
                new JokeModel { Who = "First", Punchline = "First punchline" },
                new JokeModel { Who = "Second", Punchline = "Second punchline" }
            };

            var first = selector.SelectJoke(jokes);

            Assert.Equal("First", first.Who);
        }

        [Fact]
        public void SelectJoke_WithEmptyList_ThrowsInvalidOperationException()
        {
            var selector = new SequentialJokeSelector();
            var jokes = new List<JokeModel>();

            Assert.Throws<InvalidOperationException>(() => selector.SelectJoke(jokes));
        }

        [Fact]
        public void SelectJoke_WithNullList_ThrowsInvalidOperationException()
        {
            var selector = new SequentialJokeSelector();
            Assert.Throws<InvalidOperationException>(() => selector.SelectJoke(null!));
        }

        [Fact]
        public void SelectJoke_WrapsAroundAfterLastJoke()
        {
            var selector = new SequentialJokeSelector();
            var jokes = new List<JokeModel>
            {
                new JokeModel { Who = "Joke1", Punchline = "Punchline1" },
                new JokeModel { Who = "Joke2", Punchline = "Punchline2" }
            };

            selector.SelectJoke(jokes);
            selector.SelectJoke(jokes);
            var wrapped = selector.SelectJoke(jokes);

            Assert.Equal("Joke1", wrapped.Who);
        }

        [Fact]
        public void StrategyName_ReturnsSequential()
        {
            var selector = new SequentialJokeSelector();
            Assert.Equal("Sequential", selector.StrategyName);
        }

        [Fact]
        public void SelectJoke_DeterministicOrder()
        {
            var selector1 = new SequentialJokeSelector();
            var selector2 = new SequentialJokeSelector();
            var jokes = new List<JokeModel>
            {
                new JokeModel { Who = "A", Punchline = "A punch" },
                new JokeModel { Who = "B", Punchline = "B punch" },
                new JokeModel { Who = "C", Punchline = "C punch" }
            };

            var results1 = new List<string>();
            var results2 = new List<string>();
            for (int i = 0; i < 6; i++)
            {
                results1.Add(selector1.SelectJoke(jokes).Who);
                results2.Add(selector2.SelectJoke(jokes).Who);
            }

            Assert.Equal(results1, results2);
        }
    }
}

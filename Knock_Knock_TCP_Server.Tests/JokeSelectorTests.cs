using Knock_Knock_TCP_Server.Configuration;
using Knock_Knock_TCP_Server.Services;
using Xunit;

namespace Knock_Knock_TCP_Server.Tests
{
    public class RandomJokeSelectorTests
    {
        [Fact]
        public void SelectJoke_WithSingleJoke_ReturnsThatJoke()
        {
            // Arrange
            var selector = new RandomJokeSelector();
            var jokes = new List<JokeModel>
            {
                new JokeModel { Who = "OnlyOne", Punchline = "Only joke" }
            };

            // Act
            var selectedJoke = selector.SelectJoke(jokes);

            // Assert
            Assert.NotNull(selectedJoke);
            Assert.Equal("OnlyOne", selectedJoke.Who);
            Assert.Equal("Only joke", selectedJoke.Punchline);
        }

        [Fact]
        public void SelectJoke_WithMultipleJokes_ReturnsValidJoke()
        {
            // Arrange
            var selector = new RandomJokeSelector();
            var jokes = new List<JokeModel>
            {
                new JokeModel { Who = "Joke1", Punchline = "Punchline1" },
                new JokeModel { Who = "Joke2", Punchline = "Punchline2" },
                new JokeModel { Who = "Joke3", Punchline = "Punchline3" }
            };

            // Act
            var selectedJoke = selector.SelectJoke(jokes);

            // Assert
            Assert.NotNull(selectedJoke);
            Assert.True(jokes.Contains(selectedJoke), "Selected joke should be from the provided list");
        }

        [Fact]
        public void SelectJoke_WithEmptyList_ThrowsInvalidOperationException()
        {
            // Arrange
            var selector = new RandomJokeSelector();
            var jokes = new List<JokeModel>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => selector.SelectJoke(jokes));
        }

        [Fact]
        public void SelectJoke_WithNullList_ThrowsInvalidOperationException()
        {
            // Arrange
            var selector = new RandomJokeSelector();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => selector.SelectJoke(null!));
        }

        [Fact]
        public void SelectJoke_MultipleCallsProduceVariety()
        {
            // Arrange
            var selector = new RandomJokeSelector();
            var jokes = new List<JokeModel>
            {
                new JokeModel { Who = "Joke1", Punchline = "Punchline1" },
                new JokeModel { Who = "Joke2", Punchline = "Punchline2" },
                new JokeModel { Who = "Joke3", Punchline = "Punchline3" },
                new JokeModel { Who = "Joke4", Punchline = "Punchline4" },
                new JokeModel { Who = "Joke5", Punchline = "Punchline5" }
            };

            // Act - call many times and collect results
            var selectedJokes = new HashSet<string>();
            for (int i = 0; i < 50; i++)
            {
                var selected = selector.SelectJoke(jokes);
                selectedJokes.Add(selected.Who);
            }

            // Assert - expect multiple different jokes to be selected
            Assert.True(selectedJokes.Count > 1, "Random selector should produce variety over many selections");
        }

        [Fact]
        public void StrategyName_ReturnsRandom()
        {
            // Arrange
            var selector = new RandomJokeSelector();

            // Act
            var name = selector.StrategyName;

            // Assert
            Assert.Equal("Random", name);
        }

        [Fact]
        public void SelectJoke_AllJokesAreReachable()
        {
            // Arrange
            var selector = new RandomJokeSelector();
            var jokes = new List<JokeModel>
            {
                new JokeModel { Who = "Joke1", Punchline = "Punchline1" },
                new JokeModel { Who = "Joke2", Punchline = "Punchline2" },
                new JokeModel { Who = "Joke3", Punchline = "Punchline3" }
            };
            var selectedWhos = new HashSet<string>();

            // Act - perform many selections to ensure all jokes can be reached
            for (int i = 0; i < 200; i++)
            {
                var selected = selector.SelectJoke(jokes);
                selectedWhos.Add(selected.Who);
            }

            // Assert
            Assert.Equal(3, selectedWhos.Count);
            Assert.Contains("Joke1", selectedWhos);
            Assert.Contains("Joke2", selectedWhos);
            Assert.Contains("Joke3", selectedWhos);
        }
    }

    public class SequentialJokeSelectorTests
    {
        [Fact]
        public void SelectJoke_WithSingleJoke_AlwaysReturnsSameJoke()
        {
            // Arrange
            var selector = new SequentialJokeSelector();
            var jokes = new List<JokeModel>
            {
                new JokeModel { Who = "OnlyOne", Punchline = "Only joke" }
            };

            // Act
            var joke1 = selector.SelectJoke(jokes);
            var joke2 = selector.SelectJoke(jokes);
            var joke3 = selector.SelectJoke(jokes);

            // Assert
            Assert.Equal("OnlyOne", joke1.Who);
            Assert.Equal("OnlyOne", joke2.Who);
            Assert.Equal("OnlyOne", joke3.Who);
        }

        [Fact]
        public void SelectJoke_WithMultipleJokes_CyclesThroughInOrder()
        {
            // Arrange
            var selector = new SequentialJokeSelector();
            var jokes = new List<JokeModel>
            {
                new JokeModel { Who = "Joke1", Punchline = "Punchline1" },
                new JokeModel { Who = "Joke2", Punchline = "Punchline2" },
                new JokeModel { Who = "Joke3", Punchline = "Punchline3" }
            };

            // Act - select 6 times (should cycle through twice)
            var results = new List<string>();
            for (int i = 0; i < 6; i++)
            {
                results.Add(selector.SelectJoke(jokes).Who);
            }

            // Assert
            Assert.Equal(new[] { "Joke1", "Joke2", "Joke3", "Joke1", "Joke2", "Joke3" }, results);
        }

        [Fact]
        public void SelectJoke_StartsWithFirstJoke()
        {
            // Arrange
            var selector = new SequentialJokeSelector();
            var jokes = new List<JokeModel>
            {
                new JokeModel { Who = "First", Punchline = "First punchline" },
                new JokeModel { Who = "Second", Punchline = "Second punchline" }
            };

            // Act
            var first = selector.SelectJoke(jokes);

            // Assert
            Assert.Equal("First", first.Who);
        }

        [Fact]
        public void SelectJoke_ProgressesToSecondJoke()
        {
            // Arrange
            var selector = new SequentialJokeSelector();
            var jokes = new List<JokeModel>
            {
                new JokeModel { Who = "First", Punchline = "First punchline" },
                new JokeModel { Who = "Second", Punchline = "Second punchline" },
                new JokeModel { Who = "Third", Punchline = "Third punchline" }
            };

            // Act
            selector.SelectJoke(jokes); // Get first
            var second = selector.SelectJoke(jokes); // Get second

            // Assert
            Assert.Equal("Second", second.Who);
        }

        [Fact]
        public void SelectJoke_WrapsAroundAfterLastJoke()
        {
            // Arrange
            var selector = new SequentialJokeSelector();
            var jokes = new List<JokeModel>
            {
                new JokeModel { Who = "Joke1", Punchline = "Punchline1" },
                new JokeModel { Who = "Joke2", Punchline = "Punchline2" }
            };

            // Act
            selector.SelectJoke(jokes); // Index 0
            selector.SelectJoke(jokes); // Index 1 (last)
            var wrapped = selector.SelectJoke(jokes); // Should wrap to 0

            // Assert
            Assert.Equal("Joke1", wrapped.Who);
        }

        [Fact]
        public void SelectJoke_WithEmptyList_ThrowsInvalidOperationException()
        {
            // Arrange
            var selector = new SequentialJokeSelector();
            var jokes = new List<JokeModel>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => selector.SelectJoke(jokes));
        }

        [Fact]
        public void SelectJoke_WithNullList_ThrowsInvalidOperationException()
        {
            // Arrange
            var selector = new SequentialJokeSelector();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => selector.SelectJoke(null!));
        }

        [Fact]
        public void SelectJoke_DeterministicOrder()
        {
            // Arrange
            var selector1 = new SequentialJokeSelector();
            var selector2 = new SequentialJokeSelector();
            var jokes = new List<JokeModel>
            {
                new JokeModel { Who = "A", Punchline = "A punch" },
                new JokeModel { Who = "B", Punchline = "B punch" },
                new JokeModel { Who = "C", Punchline = "C punch" }
            };

            // Act
            var results1 = new List<string>();
            var results2 = new List<string>();
            for (int i = 0; i < 6; i++)
            {
                results1.Add(selector1.SelectJoke(jokes).Who);
                results2.Add(selector2.SelectJoke(jokes).Who);
            }

            // Assert - both selectors should follow same sequence
            Assert.Equal(results1, results2);
        }

        [Fact]
        public void StrategyName_ReturnsSequential()
        {
            // Arrange
            var selector = new SequentialJokeSelector();

            // Act
            var name = selector.StrategyName;

            // Assert
            Assert.Equal("Sequential", name);
        }

        [Fact]
        public void SelectJoke_MaintainsStateIndependently()
        {
            // Arrange
            var selector1 = new SequentialJokeSelector();
            var selector2 = new SequentialJokeSelector();
            var jokes = new List<JokeModel>
            {
                new JokeModel { Who = "Joke1", Punchline = "Punchline1" },
                new JokeModel { Who = "Joke2", Punchline = "Punchline2" }
            };

            // Act
            var s1_1 = selector1.SelectJoke(jokes).Who; // Joke1
            var s2_1 = selector2.SelectJoke(jokes).Who; // Joke1
            var s1_2 = selector1.SelectJoke(jokes).Who; // Joke2
            var s2_2 = selector2.SelectJoke(jokes).Who; // Joke2

            // Assert
            Assert.Equal("Joke1", s1_1);
            Assert.Equal("Joke1", s2_1);
            Assert.Equal("Joke2", s1_2);
            Assert.Equal("Joke2", s2_2);
        }

        [Fact]
        public void SelectJoke_LargeJokeList_CyclesThroughAll()
        {
            // Arrange
            var selector = new SequentialJokeSelector();
            var jokes = Enumerable.Range(1, 20)
                .Select(i => new JokeModel { Who = $"Joke{i}", Punchline = $"Punchline{i}" })
                .ToList();

            // Act - cycle through twice
            var results = new List<string>();
            for (int i = 0; i < 40; i++)
            {
                results.Add(selector.SelectJoke(jokes).Who);
            }

            // Assert
            // First 20 should be Joke1-Joke20, next 20 should be the same
            for (int i = 0; i < 20; i++)
            {
                Assert.Equal(results[i], results[i + 20]);
            }
        }
    }
}

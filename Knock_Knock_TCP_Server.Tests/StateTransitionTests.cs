using Knock_Knock_TCP_Server.Configuration;
using Knock_Knock_TCP_Server.Services;
using Knock_Knock_TCP_Server.States;
using Microsoft.Extensions.Options;
using Xunit;
using Moq;

namespace Knock_Knock_TCP_Server.Tests
{
    public class WaitingForJokeRequestStateTests
    {
        private Mock<IConversationContext> _mockContext = null!;
        private Mock<IJokeSelector> _mockJokeSelector = null!;
        private IOptions<JokesConfiguration> _jokeOptions = null!;

        private void Setup()
        {
            _mockContext = new Mock<IConversationContext>();
            _mockJokeSelector = new Mock<IJokeSelector>();

            var jokesConfig = new JokesConfiguration
            {
                Jokes = new List<JokeModel>
                {
                    new JokeModel { Who = "Lettuce", Punchline = "Lettuce in!" },
                    new JokeModel { Who = "Boo", Punchline = "Don't cry!" }
                },
                SelectionStrategy = "Random"
            };

            _jokeOptions = Options.Create(jokesConfig);
        }

        [Fact]
        public void Handle_WithJokeMessage_TransitionsToWaitingForWhoState()
        {
            // Arrange
            Setup();
            var state = new WaitingForJokeRequestState();
            var selectedJoke = new JokeModel { Who = "Lettuce", Punchline = "Lettuce in!" };

            _mockContext.Setup(c => c.ReadMessage()).Returns("joke");
            _mockJokeSelector.Setup(s => s.SelectJoke(It.IsAny<List<JokeModel>>())).Returns(selectedJoke);

            // Act
            var nextState = state.Handle(_mockContext.Object, _jokeOptions, _mockJokeSelector.Object);

            // Assert
            Assert.NotNull(nextState);
            Assert.IsType<WaitingForWhoState>(nextState);
            _mockContext.Verify(c => c.SetCurrentJoke(selectedJoke), Times.Once);
            _mockContext.Verify(c => c.SendMessage("Knock! Knock!"), Times.Once);
        }

        [Fact]
        public void Handle_WithCaseInsensitiveJoke_TransitionsCorrectly()
        {
            // Arrange
            Setup();
            var state = new WaitingForJokeRequestState();
            var selectedJoke = new JokeModel { Who = "Test", Punchline = "Test punch" };

            _mockContext.Setup(c => c.ReadMessage()).Returns("JOKE");
            _mockJokeSelector.Setup(s => s.SelectJoke(It.IsAny<List<JokeModel>>())).Returns(selectedJoke);

            // Act
            var nextState = state.Handle(_mockContext.Object, _jokeOptions, _mockJokeSelector.Object);

            // Assert
            Assert.NotNull(nextState);
            Assert.IsType<WaitingForWhoState>(nextState);
        }

        [Theory]
        [InlineData("help")]
        [InlineData("test")]
        [InlineData("")]
        [InlineData("jokee")]
        public void Handle_WithInvalidMessage_StaysInCurrentState(string message)
        {
            // Arrange
            Setup();
            var state = new WaitingForJokeRequestState();
            _mockContext.Setup(c => c.ReadMessage()).Returns(message);

            // Act
            var nextState = state.Handle(_mockContext.Object, _jokeOptions, _mockJokeSelector.Object);

            // Assert
            Assert.Same(state, nextState);
            _mockContext.Verify(c => c.SendMessage(It.IsAny<string>()), Times.Once);
            _mockContext.Verify(c => c.SendMessage(It.Is<string>(s => s.Contains("Please send"))), Times.Once);
        }

        [Fact]
        public void Handle_WithNullMessage_StaysInCurrentState()
        {
            // Arrange
            Setup();
            var state = new WaitingForJokeRequestState();
            _mockContext.Setup(c => c.ReadMessage()).Returns((string?)null);

            // Act
            var nextState = state.Handle(_mockContext.Object, _jokeOptions, _mockJokeSelector.Object);

            // Assert
            Assert.Same(state, nextState);
        }

        [Fact]
        public void Handle_CallsJokeSelector_WithCurrentJokes()
        {
            // Arrange
            Setup();
            var state = new WaitingForJokeRequestState();
            var selectedJoke = _jokeOptions.Value.Jokes[0];

            _mockContext.Setup(c => c.ReadMessage()).Returns("joke");
            _mockJokeSelector.Setup(s => s.SelectJoke(It.IsAny<List<JokeModel>>())).Returns(selectedJoke);

            // Act
            state.Handle(_mockContext.Object, _jokeOptions, _mockJokeSelector.Object);

            // Assert
            _mockJokeSelector.Verify(s => s.SelectJoke(_jokeOptions.Value.Jokes), Times.Once);
        }
    }

    public class WaitingForWhoStateTests
    {
        private Mock<IConversationContext> _mockContext = null!;
        private Mock<IJokeSelector> _mockJokeSelector = null!;
        private IOptions<JokesConfiguration> _jokeOptions = null!;

        private void Setup()
        {
            _mockContext = new Mock<IConversationContext>();
            _mockJokeSelector = new Mock<IJokeSelector>();

            var jokesConfig = new JokesConfiguration
            {
                Jokes = new List<JokeModel>
                {
                    new JokeModel { Who = "Lettuce", Punchline = "Lettuce in!" }
                },
                SelectionStrategy = "Random"
            };

            _jokeOptions = Options.Create(jokesConfig);
        }

        [Fact]
        public void Handle_WithCorrectWhoResponse_TransitionsToSendingPunchlineState()
        {
            // Arrange
            Setup();
            var state = new WaitingForWhoState();
            var joke = new JokeModel { Who = "Lettuce", Punchline = "Lettuce in!" };

            _mockContext.Setup(c => c.ReadMessage()).Returns("Who's there?");
            _mockContext.Setup(c => c.GetCurrentJoke()).Returns(joke);

            // Act
            var nextState = state.Handle(_mockContext.Object, _jokeOptions, _mockJokeSelector.Object);

            // Assert
            Assert.NotNull(nextState);
            Assert.IsType<SendingPunchlineState>(nextState);
            _mockContext.Verify(c => c.SendMessage("Lettuce"), Times.Once);
        }

        [Fact]
        public void Handle_WithCaseInsensitiveWhoResponse_TransitionsCorrectly()
        {
            // Arrange
            Setup();
            var state = new WaitingForWhoState();
            var joke = new JokeModel { Who = "Test", Punchline = "Test!" };

            _mockContext.Setup(c => c.ReadMessage()).Returns("WHO'S THERE?");
            _mockContext.Setup(c => c.GetCurrentJoke()).Returns(joke);

            // Act
            var nextState = state.Handle(_mockContext.Object, _jokeOptions, _mockJokeSelector.Object);

            // Assert
            Assert.IsType<SendingPunchlineState>(nextState);
        }

        [Theory]
        [InlineData("Who is there?")]
        [InlineData("What?")]
        [InlineData("help")]
        [InlineData("")]
        public void Handle_WithInvalidResponse_StaysInCurrentState(string response)
        {
            // Arrange
            Setup();
            var state = new WaitingForWhoState();
            var joke = new JokeModel { Who = "Test", Punchline = "Test!" };

            _mockContext.Setup(c => c.ReadMessage()).Returns(response);
            _mockContext.Setup(c => c.GetCurrentJoke()).Returns(joke);

            // Act
            var nextState = state.Handle(_mockContext.Object, _jokeOptions, _mockJokeSelector.Object);

            // Assert
            Assert.Same(state, nextState);
            _mockContext.Verify(c => c.SendMessage(It.Is<string>(s => s.Contains("Who's there?"))), Times.Once);
        }

        [Fact]
        public void Handle_WithNullCurrentJoke_ReturnsToWaitingForJokeRequestState()
        {
            // Arrange
            Setup();
            var state = new WaitingForWhoState();

            _mockContext.Setup(c => c.ReadMessage()).Returns("Who's there?");
            _mockContext.Setup(c => c.GetCurrentJoke()).Returns((JokeModel?)null);

            // Act
            var nextState = state.Handle(_mockContext.Object, _jokeOptions, _mockJokeSelector.Object);

            // Assert
            Assert.NotNull(nextState);
            Assert.IsType<WaitingForJokeRequestState>(nextState);
            _mockContext.Verify(c => c.SendMessage(It.Is<string>(s => s.Contains("error"))), Times.Once);
        }
    }

    public class SendingPunchlineStateTests
    {
        private Mock<IConversationContext> _mockContext = null!;
        private Mock<IJokeSelector> _mockJokeSelector = null!;
        private IOptions<JokesConfiguration> _jokeOptions = null!;

        private void Setup()
        {
            _mockContext = new Mock<IConversationContext>();
            _mockJokeSelector = new Mock<IJokeSelector>();

            var jokesConfig = new JokesConfiguration
            {
                Jokes = new List<JokeModel>
                {
                    new JokeModel { Who = "Lettuce", Punchline = "Lettuce in!" }
                },
                SelectionStrategy = "Random"
            };

            _jokeOptions = Options.Create(jokesConfig);
        }

        [Fact]
        public void Handle_WithAnyNonEmptyMessage_SendsPunchlineAndTransitions()
        {
            // Arrange
            Setup();
            var state = new SendingPunchlineState();
            var joke = new JokeModel { Who = "Lettuce", Punchline = "Lettuce in!" };

            _mockContext.Setup(c => c.ReadMessage()).Returns("Lettuce who?");
            _mockContext.Setup(c => c.GetCurrentJoke()).Returns(joke);

            // Act
            var nextState = state.Handle(_mockContext.Object, _jokeOptions, _mockJokeSelector.Object);

            // Assert
            Assert.NotNull(nextState);
            Assert.IsType<LoopOrExitState>(nextState);
            _mockContext.Verify(c => c.SendMessage("Lettuce in!"), Times.Once);
        }

        [Fact]
        public void Handle_WithEmptyMessage_StaysInCurrentState()
        {
            // Arrange
            Setup();
            var state = new SendingPunchlineState();
            var joke = new JokeModel { Who = "Test", Punchline = "Test!" };

            _mockContext.Setup(c => c.ReadMessage()).Returns("");
            _mockContext.Setup(c => c.GetCurrentJoke()).Returns(joke);

            // Act
            var nextState = state.Handle(_mockContext.Object, _jokeOptions, _mockJokeSelector.Object);

            // Assert
            Assert.Same(state, nextState);
        }

        [Fact]
        public void Handle_WithWhitespaceOnlyMessage_StaysInCurrentState()
        {
            // Arrange
            Setup();
            var state = new SendingPunchlineState();
            var joke = new JokeModel { Who = "Test", Punchline = "Test!" };

            _mockContext.Setup(c => c.ReadMessage()).Returns("   ");
            _mockContext.Setup(c => c.GetCurrentJoke()).Returns(joke);

            // Act
            var nextState = state.Handle(_mockContext.Object, _jokeOptions, _mockJokeSelector.Object);

            // Assert
            Assert.Same(state, nextState);
        }

        [Fact]
        public void Handle_WithNullCurrentJoke_ReturnsToWaitingForJokeRequestState()
        {
            // Arrange
            Setup();
            var state = new SendingPunchlineState();

            _mockContext.Setup(c => c.ReadMessage()).Returns("anything");
            _mockContext.Setup(c => c.GetCurrentJoke()).Returns((JokeModel?)null);

            // Act
            var nextState = state.Handle(_mockContext.Object, _jokeOptions, _mockJokeSelector.Object);

            // Assert
            Assert.NotNull(nextState);
            Assert.IsType<WaitingForJokeRequestState>(nextState);
        }

        [Fact]
        public void Handle_WithNullMessage_StaysInCurrentState()
        {
            // Arrange
            Setup();
            var state = new SendingPunchlineState();
            var joke = new JokeModel { Who = "Test", Punchline = "Test!" };

            _mockContext.Setup(c => c.ReadMessage()).Returns((string?)null);
            _mockContext.Setup(c => c.GetCurrentJoke()).Returns(joke);

            // Act
            var nextState = state.Handle(_mockContext.Object, _jokeOptions, _mockJokeSelector.Object);

            // Assert
            Assert.Same(state, nextState);
        }
    }

    public class LoopOrExitStateTests
    {
        private Mock<IConversationContext> _mockContext = null!;
        private Mock<IJokeSelector> _mockJokeSelector = null!;
        private IOptions<JokesConfiguration> _jokeOptions = null!;

        private void Setup()
        {
            _mockContext = new Mock<IConversationContext>();
            _mockJokeSelector = new Mock<IJokeSelector>();

            var jokesConfig = new JokesConfiguration
            {
                Jokes = new List<JokeModel>
                {
                    new JokeModel { Who = "Test", Punchline = "Test!" }
                },
                SelectionStrategy = "Random"
            };

            _jokeOptions = Options.Create(jokesConfig);
        }

        [Fact]
        public void Handle_WithYesResponse_TransitionsToWaitingForJokeRequestState()
        {
            // Arrange
            Setup();
            var state = new LoopOrExitState();

            _mockContext.Setup(c => c.ReadMessage()).Returns("yes");

            // Act
            var nextState = state.Handle(_mockContext.Object, _jokeOptions, _mockJokeSelector.Object);

            // Assert
            Assert.NotNull(nextState);
            Assert.IsType<WaitingForJokeRequestState>(nextState);
        }

        [Fact]
        public void Handle_WithYesResponseCaseInsensitive_TransitionsCorrectly()
        {
            // Arrange
            Setup();
            var state = new LoopOrExitState();

            _mockContext.Setup(c => c.ReadMessage()).Returns("YES");

            // Act
            var nextState = state.Handle(_mockContext.Object, _jokeOptions, _mockJokeSelector.Object);

            // Assert
            Assert.IsType<WaitingForJokeRequestState>(nextState);
        }

        [Fact]
        public void Handle_WithNoResponse_EndsConversation()
        {
            // Arrange
            Setup();
            var state = new LoopOrExitState();

            _mockContext.Setup(c => c.ReadMessage()).Returns("no");

            // Act
            var nextState = state.Handle(_mockContext.Object, _jokeOptions, _mockJokeSelector.Object);

            // Assert
            Assert.Null(nextState);
            _mockContext.Verify(c => c.SendMessage(It.Is<string>(s => s.Contains("Thanks") || s.Contains("Goodbye"))), Times.Once);
        }

        [Fact]
        public void Handle_WithNoResponseCaseInsensitive_EndsConversation()
        {
            // Arrange
            Setup();
            var state = new LoopOrExitState();

            _mockContext.Setup(c => c.ReadMessage()).Returns("NO");

            // Act
            var nextState = state.Handle(_mockContext.Object, _jokeOptions, _mockJokeSelector.Object);

            // Assert
            Assert.Null(nextState);
        }

        [Theory]
        [InlineData("maybe")]
        [InlineData("perhaps")]
        [InlineData("y")]
        [InlineData("n")]
        [InlineData("")]
        [InlineData("help")]
        public void Handle_WithInvalidResponse_StaysInCurrentState(string response)
        {
            // Arrange
            Setup();
            var state = new LoopOrExitState();

            _mockContext.Setup(c => c.ReadMessage()).Returns(response);

            // Act
            var nextState = state.Handle(_mockContext.Object, _jokeOptions, _mockJokeSelector.Object);

            // Assert
            Assert.Same(state, nextState);
            _mockContext.Verify(c => c.SendMessage(It.Is<string>(s => s.Contains("yes") && s.Contains("no"))), Times.Once);
        }

        [Fact]
        public void Handle_SendsPromptBeforeReadingMessage()
        {
            // Arrange
            Setup();
            var state = new LoopOrExitState();
            var callOrder = new List<string>();

            _mockContext.Setup(c => c.SendMessage(It.IsAny<string>()))
                .Callback(() => callOrder.Add("SendMessage"));
            _mockContext.Setup(c => c.ReadMessage())
                .Callback(() => callOrder.Add("ReadMessage"))
                .Returns("yes");

            // Act
            state.Handle(_mockContext.Object, _jokeOptions, _mockJokeSelector.Object);

            // Assert
            Assert.Equal(new[] { "SendMessage", "ReadMessage" }, callOrder);
        }

        [Fact]
        public void Handle_WithNullMessage_StaysInCurrentState()
        {
            // Arrange
            Setup();
            var state = new LoopOrExitState();

            _mockContext.Setup(c => c.ReadMessage()).Returns((string?)null);

            // Act
            var nextState = state.Handle(_mockContext.Object, _jokeOptions, _mockJokeSelector.Object);

            // Assert
            Assert.Same(state, nextState);
        }
    }
}

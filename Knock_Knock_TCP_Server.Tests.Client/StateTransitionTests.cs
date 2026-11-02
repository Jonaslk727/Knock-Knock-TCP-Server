using System;
using System.Collections.Generic;
using Knock_Knock_TCP_Server.Configuration;
using Knock_Knock_TCP_Server.Services;
using Knock_Knock_TCP_Server.States;
using Microsoft.Extensions.Options;
using Xunit;
using Moq;

namespace Knock_Knock_TCP_Server.Tests.Client
{
    public class WaitingForJokeRequestStateTests
    {
        [Fact]
        public void Handle_WithJokeMessage_TransitionsToWaitingForWhoState()
        {
            var mockContext = new Mock<IConversationContext>();
            var mockJokeSelector = new Mock<IJokeSelector>();
            var state = new WaitingForJokeRequestState();
            var selectedJoke = new JokeModel { Who = "Lettuce", Punchline = "Lettuce in!" };
            var jokesConfig = new JokesConfiguration
            {
                Jokes = new List<JokeModel> { selectedJoke },
                SelectionStrategy = "Random"
            };
            var jokeOptions = Options.Create(jokesConfig);

            mockContext.Setup(c => c.ReadMessage()).Returns("joke");
            mockJokeSelector.Setup(s => s.SelectJoke(It.IsAny<List<JokeModel>>())).Returns(selectedJoke);

            var nextState = state.Handle(mockContext.Object, jokeOptions, mockJokeSelector.Object);

            Assert.NotNull(nextState);
            Assert.IsType<WaitingForWhoState>(nextState);
        }

        [Theory]
        [InlineData("help")]
        [InlineData("test")]
        [InlineData("")]
        public void Handle_WithInvalidMessage_StaysInCurrentState(string message)
        {
            var mockContext = new Mock<IConversationContext>();
            var mockJokeSelector = new Mock<IJokeSelector>();
            var state = new WaitingForJokeRequestState();
            var jokeOptions = Options.Create(new JokesConfiguration { Jokes = new List<JokeModel>(), SelectionStrategy = "Random" });

            mockContext.Setup(c => c.ReadMessage()).Returns(message);

            var nextState = state.Handle(mockContext.Object, jokeOptions, mockJokeSelector.Object);

            Assert.Same(state, nextState);
        }
    }

    public class WaitingForWhoStateTests
    {
        [Fact]
        public void Handle_WithCorrectWhoResponse_TransitionsToSendingPunchlineState()
        {
            var mockContext = new Mock<IConversationContext>();
            var mockJokeSelector = new Mock<IJokeSelector>();
            var state = new WaitingForWhoState();
            var joke = new JokeModel { Who = "Lettuce", Punchline = "Lettuce in!" };
            var jokeOptions = Options.Create(new JokesConfiguration { Jokes = new List<JokeModel> { joke }, SelectionStrategy = "Random" });

            mockContext.Setup(c => c.ReadMessage()).Returns("Who's there?");
            mockContext.Setup(c => c.GetCurrentJoke()).Returns(joke);

            var nextState = state.Handle(mockContext.Object, jokeOptions, mockJokeSelector.Object);

            Assert.NotNull(nextState);
            Assert.IsType<SendingPunchlineState>(nextState);
        }

        [Theory]
        [InlineData("Who is there?")]
        [InlineData("What?")]
        public void Handle_WithInvalidResponse_StaysInCurrentState(string response)
        {
            var mockContext = new Mock<IConversationContext>();
            var mockJokeSelector = new Mock<IJokeSelector>();
            var state = new WaitingForWhoState();
            var joke = new JokeModel { Who = "Test", Punchline = "Test!" };
            var jokeOptions = Options.Create(new JokesConfiguration { Jokes = new List<JokeModel> { joke }, SelectionStrategy = "Random" });

            mockContext.Setup(c => c.ReadMessage()).Returns(response);
            mockContext.Setup(c => c.GetCurrentJoke()).Returns(joke);

            var nextState = state.Handle(mockContext.Object, jokeOptions, mockJokeSelector.Object);

            Assert.Same(state, nextState);
        }
    }

    public class SendingPunchlineStateTests
    {
        [Fact]
        public void Handle_WithAnyNonEmptyMessage_SendsPunchlineAndTransitions()
        {
            var mockContext = new Mock<IConversationContext>();
            var mockJokeSelector = new Mock<IJokeSelector>();
            var state = new SendingPunchlineState();
            var joke = new JokeModel { Who = "Lettuce", Punchline = "Lettuce in!" };
            var jokeOptions = Options.Create(new JokesConfiguration { Jokes = new List<JokeModel> { joke }, SelectionStrategy = "Random" });

            mockContext.Setup(c => c.ReadMessage()).Returns("Lettuce who?");
            mockContext.Setup(c => c.GetCurrentJoke()).Returns(joke);

            var nextState = state.Handle(mockContext.Object, jokeOptions, mockJokeSelector.Object);

            Assert.NotNull(nextState);
            Assert.IsType<LoopOrExitState>(nextState);
        }

        [Fact]
        public void Handle_WithEmptyMessage_StaysInCurrentState()
        {
            var mockContext = new Mock<IConversationContext>();
            var mockJokeSelector = new Mock<IJokeSelector>();
            var state = new SendingPunchlineState();
            var joke = new JokeModel { Who = "Test", Punchline = "Test!" };
            var jokeOptions = Options.Create(new JokesConfiguration { Jokes = new List<JokeModel> { joke }, SelectionStrategy = "Random" });

            mockContext.Setup(c => c.ReadMessage()).Returns("");
            mockContext.Setup(c => c.GetCurrentJoke()).Returns(joke);

            var nextState = state.Handle(mockContext.Object, jokeOptions, mockJokeSelector.Object);

            Assert.Same(state, nextState);
        }
    }

    public class LoopOrExitStateTests
    {
        [Fact]
        public void Handle_WithYesResponse_TransitionsToWaitingForJokeRequestState()
        {
            var mockContext = new Mock<IConversationContext>();
            var mockJokeSelector = new Mock<IJokeSelector>();
            var state = new LoopOrExitState();
            var jokeOptions = Options.Create(new JokesConfiguration { Jokes = new List<JokeModel>(), SelectionStrategy = "Random" });

            mockContext.Setup(c => c.ReadMessage()).Returns("yes");

            var nextState = state.Handle(mockContext.Object, jokeOptions, mockJokeSelector.Object);

            Assert.NotNull(nextState);
            Assert.IsType<WaitingForJokeRequestState>(nextState);
        }

        [Fact]
        public void Handle_WithNoResponse_EndsConversation()
        {
            var mockContext = new Mock<IConversationContext>();
            var mockJokeSelector = new Mock<IJokeSelector>();
            var state = new LoopOrExitState();
            var jokeOptions = Options.Create(new JokesConfiguration { Jokes = new List<JokeModel>(), SelectionStrategy = "Random" });

            mockContext.Setup(c => c.ReadMessage()).Returns("no");

            var nextState = state.Handle(mockContext.Object, jokeOptions, mockJokeSelector.Object);

            Assert.Null(nextState);
        }

        [Theory]
        [InlineData("maybe")]
        [InlineData("perhaps")]
        public void Handle_WithInvalidResponse_StaysInCurrentState(string response)
        {
            var mockContext = new Mock<IConversationContext>();
            var mockJokeSelector = new Mock<IJokeSelector>();
            var state = new LoopOrExitState();
            var jokeOptions = Options.Create(new JokesConfiguration { Jokes = new List<JokeModel>(), SelectionStrategy = "Random" });

            mockContext.Setup(c => c.ReadMessage()).Returns(response);

            var nextState = state.Handle(mockContext.Object, jokeOptions, mockJokeSelector.Object);

            Assert.Same(state, nextState);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Knock_Knock_TCP_Server.Tests.Client
{
    /// <summary>
    /// Provides test data, expected outputs, and utilities for consistent test scenarios.
    /// </summary>
    public static class TestDataFactory
    {
        public static class ExpectedMessages
        {
            public const string KnockKnockPrompt = "Knock knock?";
            public const string WhosThere = "Who's there?";
            public const string WhoFormat = "{0} who?";
        }

        public static class TestJokes
        {
            public static List<(string Who, string Punchline)> GetStandardJokes()
            {
                return new List<(string, string)>
                {
                    ("Lettuce", "Lettuce in, it's cold out here!"),
                    ("Tank", "You're welcome!"),
                    ("Boo", "Don't cry, it's only a joke!")
                };
            }

            public static List<(string Who, string Punchline)> GetSequentialJokes()
            {
                return new List<(string, string)>
                {
                    ("TestSetup1", "TestPunchline1"),
                    ("TestSetup2", "TestPunchline2")
                };
            }

            public static List<(string Who, string Punchline)> GetSingleJoke()
            {
                return new List<(string, string)>
                {
                    ("SingleJoke", "Only one joke available")
                };
            }
        }

        public static class ServerConfigurations
        {
            public const int DefaultPort = 5000;
            public const int TestPort1 = 5001;
            public const int TestPort2 = 5002;
            public const int TestPort3 = 5003;
            public const int TestPort4 = 5004;

            public const int DefaultMaxClients = 10;
            public const int MaxClientsOne = 1;
            public const int MaxClientsTwo = 2;
            public const int MaxClientsFive = 5;

            public const string StrategyRandom = "Random";
            public const string StrategySequential = "Sequential";
            public const string StrategyInvalid = "Invalid";
        }

        /// <summary>
        /// Gets expected conversation flow for a successful Knock-Knock exchange.
        /// </summary>
        public static List<string> GetExpectedConversationFlow()
        {
            return new List<string>
            {
                ExpectedMessages.KnockKnockPrompt,
                // Client responds: Who's there?
                // Server sends setup (joke Who)
                // Client responds: {Who} who?
                // Server sends punchline
                // Client responds: Play again? or similar
                // Server either sends next joke or goodbye
            };
        }

        /// <summary>
        /// Gets valid user responses for various server prompts.
        /// </summary>
        public static Dictionary<string, List<string>> GetValidUserResponses()
        {
            return new Dictionary<string, List<string>>
            {
                { ExpectedMessages.KnockKnockPrompt, new List<string> { "Who's there?" } },
                { "Play again?", new List<string> { "Yes", "No", "yes", "no", "Y", "N", "y", "n" } }
            };
        }

        /// <summary>
        /// Gets invalid/malformed inputs for error testing.
        /// </summary>
        public static List<string> GetMalformedInputs()
        {
            return new List<string>
            {
                "",
                "   ",
                "\x00\x01\x02",
                new string('A', 10000),
                "!@#$%^&*()",
                "SELECT * FROM Users;",
                "../../etc/passwd"
            };
        }

        /// <summary>
        /// Gets wrong answers that should not advance the conversation.
        /// </summary>
        public static List<string> GetWrongAnswers()
        {
            return new List<string>
            {
                "I don't know",
                "Maybe",
                "Seriously?",
                "WrongAnswer",
                "123",
                "Skip this"
            };
        }

        /// <summary>
        /// Validates expected message structure and format.
        /// </summary>
        public static bool ValidateMessage(string message, string expectedPattern)
        {
            if (string.IsNullOrEmpty(message))
                return false;

            return message.Contains(expectedPattern, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Generates test scenarios combining various configurations.
        /// </summary>
        public static List<TestScenario> GenerateTestScenarios()
        {
            return new List<TestScenario>
            {
                new TestScenario
                {
                    Name = "Single Client - Random Strategy",
                    ClientCount = 1,
                    Strategy = ServerConfigurations.StrategyRandom,
                    Port = ServerConfigurations.TestPort1,
                    MaxClients = ServerConfigurations.DefaultMaxClients,
                    ExpectedSuccess = true
                },
                new TestScenario
                {
                    Name = "Three Clients - Random Strategy",
                    ClientCount = 3,
                    Strategy = ServerConfigurations.StrategyRandom,
                    Port = ServerConfigurations.TestPort1,
                    MaxClients = ServerConfigurations.DefaultMaxClients,
                    ExpectedSuccess = true
                },
                new TestScenario
                {
                    Name = "Single Client - Sequential Strategy",
                    ClientCount = 1,
                    Strategy = ServerConfigurations.StrategySequential,
                    Port = ServerConfigurations.TestPort2,
                    MaxClients = ServerConfigurations.DefaultMaxClients,
                    ExpectedSuccess = true
                },
                new TestScenario
                {
                    Name = "Max Clients Reached",
                    ClientCount = 3,
                    Strategy = ServerConfigurations.StrategyRandom,
                    Port = ServerConfigurations.TestPort3,
                    MaxClients = ServerConfigurations.MaxClientsOne,
                    ExpectedSuccess = false
                }
            };
        }
    }

    /// <summary>
    /// Represents a test scenario with configuration.
    /// </summary>
    public class TestScenario
    {
        public string Name { get; set; } = string.Empty;
        public int ClientCount { get; set; }
        public string Strategy { get; set; } = string.Empty;
        public int Port { get; set; }
        public int MaxClients { get; set; }
        public bool ExpectedSuccess { get; set; }
    }
}

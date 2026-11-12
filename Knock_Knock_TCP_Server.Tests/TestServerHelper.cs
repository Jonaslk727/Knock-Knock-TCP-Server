using Knock_Knock_TCP_Server;
using Knock_Knock_TCP_Server.Configuration;
using Knock_Knock_TCP_Server.Configuration.Validation;
using Knock_Knock_TCP_Server.Services;
using Knock_Knock_TCP_Server.States;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Knock_Knock_TCP_Server.Tests
{
    /// <summary>
    /// Test helper for creating and managing a server instance for integration tests.
    /// </summary>
    public class TestServerHelper
    {
        private IHost? _host;
        private Knock_KnockServer? _server;
        private Thread? _serverThread;
        private readonly int _port;
        private readonly string _strategy;
        private readonly List<JokeModel> _jokes;

        public int Port => _port;
        public bool IsRunning { get; private set; }

        public TestServerHelper(int port = 5010, string strategy = "Random", List<JokeModel>? jokes = null)
        {
            _port = port;
            _strategy = strategy;
            _jokes = jokes ?? new List<JokeModel>
            {
                new JokeModel { Who = "Lettuce", Punchline = "Lettuce in, it's cold out here!" },
                new JokeModel { Who = "Tank", Punchline = "You're welcome!" },
                new JokeModel { Who = "Boo", Punchline = "Don't cry, it's only a joke!" }
            };
        }

        /// <summary>
        /// Starts the test server in a background thread.
        /// </summary>
        public async Task StartAsync()
        {
            if (IsRunning)
                return;

            var builder = Host.CreateApplicationBuilder();

            // Configure with in-memory settings
            var configDict = new Dictionary<string, string?>
            {
                { "Server:Port", _port.ToString() },
                { "Server:MaxClients", "10" },
                { "JokeStrategy:Strategy", _strategy }
            };

            builder.Configuration.AddInMemoryCollection(configDict);

            // Add services
            builder.Services.Configure<ServerOptions>(builder.Configuration.GetSection("Server"));
            builder.Services.AddSingleton<IValidateOptions<ServerOptions>, ServerOptionsValidator>();

            builder.Services.Configure<JokesConfiguration>(options =>
            {
                options.Jokes = _jokes;
                options.Strategy = _strategy;
            });
            builder.Services.AddSingleton<IValidateOptions<JokesConfiguration>, JokesConfigurationValidator>();

            builder.Services.AddSingleton(provider =>
            {
                var jokesConfig = provider.GetRequiredService<IOptions<JokesConfiguration>>();
                return JokeSelectorFactory.CreateSelector(jokesConfig.Value.Strategy);
            });

            builder.Services.AddTransient<WaitingForJokeRequestState>();
            builder.Services.AddTransient<WaitingForWhoState>();
            builder.Services.AddTransient<SendingPunchlineState>();
            builder.Services.AddTransient<LoopOrExitState>();

            builder.Services.AddSingleton<Knock_KnockServer>();

            _host = builder.Build();

            // Validate configuration
            var services = _host.Services;
            try
            {
                _ = services.GetRequiredService<IOptions<ServerOptions>>().Value;
                _ = services.GetRequiredService<IOptions<JokesConfiguration>>().Value;
            }
            catch (OptionsValidationException ex)
            {
                throw new InvalidOperationException($"Configuration validation failed: {ex.Message}", ex);
            }

            _server = _host.Services.GetRequiredService<Knock_KnockServer>();

            // Start server in background thread
            _serverThread = new Thread(() =>
            {
                try
                {
                    _server.Start();
                }
                catch (Exception)
                {
                    // Suppress exceptions during server run
                }
            })
            {
                IsBackground = true,
                Name = "TestServer"
            };

            _serverThread.Start();
            IsRunning = true;

            // Give server time to start
            await Task.Delay(500);
        }

        /// <summary>
        /// Stops the test server.
        /// </summary>
        public async Task StopAsync()
        {
            if (!IsRunning || _server == null)
                return;

            IsRunning = false;

            try
            {
                // The server is in an AcceptConnections loop, which we need to break
                // This is a limitation of the current server design
                // For now, just wait for graceful shutdown
                if (_serverThread?.IsAlive == true)
                {
                    _serverThread.Join(TimeSpan.FromSeconds(3));
                }
            }
            catch (Exception)
            {
                // Ignore
            }

            _host?.Dispose();
            await Task.CompletedTask;
        }

        /// <summary>
        /// Disposes resources.
        /// </summary>
        public void Dispose()
        {
            StopAsync().Wait(TimeSpan.FromSeconds(5));
            _host?.Dispose();
        }
    }
}

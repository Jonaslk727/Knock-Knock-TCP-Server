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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Knock_Knock_TCP_Server.Tests.Client
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
                new JokeModel { Who = "Boo", Punchline = "Don't cry, it's only a joke!" },
                new JokeModel { Who = "Interrupt", Punchline = "MOOOOOOOO!" },
                new JokeModel { Who = "Dozen", Punchline = "Dozen anybody want to let me in?" }
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
                options.SelectionStrategy = _strategy;
            });
            builder.Services.AddSingleton<IValidateOptions<JokesConfiguration>, JokesConfigurationValidator>();

            builder.Services.AddSingleton(provider =>
            {
                var jokesConfig = provider.GetRequiredService<IOptions<JokesConfiguration>>();
                return JokeSelectorFactory.CreateSelector(jokesConfig.Value.SelectionStrategy);
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
                catch (Exception ex)
                {
                    Console.WriteLine($"Server thread exception: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
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
                // Explicitly call Stop() on the server to interrupt the AcceptTcpClient() call
                _server.Stop();

                // Give the Stop() signal time to propagate through the listener
                await Task.Delay(100);

                // Wait for server thread to finish (with extended timeout for client cleanup)
                if (_serverThread?.IsAlive == true)
                {
                    if (!_serverThread.Join(TimeSpan.FromSeconds(5)))
                    {
                        Console.WriteLine("Warning: Server thread did not finish within timeout");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping server: {ex.Message}");
            }

            // Give a little more time for background client threads to complete
            await Task.Delay(200);

            // Dispose the host AFTER all threads have had time to finish
            _host?.Dispose();
            await Task.CompletedTask;
        }

        /// <summary>
        /// Disposes resources.
        /// </summary>
        public void Dispose()
        {
            try
            {
                StopAsync().Wait(TimeSpan.FromSeconds(8));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during dispose: {ex.Message}");
            }
        }
    }
}

using Knock_Knock_TCP_Server;
using Knock_Knock_TCP_Server.Configuration;
using Knock_Knock_TCP_Server.Configuration.Validation;
using Knock_Knock_TCP_Server.Services;
using Knock_Knock_TCP_Server.States;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Knock_Knock_TCP_Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Build the host with configuration and dependency injection
                var builder = Host.CreateApplicationBuilder(args);

                builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
                builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

                // Configure services and dependency injection
                var configuration = builder.Configuration;

                // Register strongly typed options with validation
                builder.Services.Configure<ServerOptions>(configuration.GetSection("Server"));
                builder.Services.AddSingleton<IValidateOptions<ServerOptions>, ServerOptionsValidator>();

                builder.Services.Configure<JokesConfiguration>(options =>
                {
                    configuration.GetSection("Jokes").Bind(options);
                    configuration.GetSection("JokeStrategy").Bind(options);
                });
                builder.Services.AddSingleton<IValidateOptions<JokesConfiguration>, JokesConfigurationValidator>();

                // Register joke selection strategy factory
                builder.Services.AddSingleton(provider =>
                {
                    var jokesConfig = provider.GetRequiredService<IOptions<JokesConfiguration>>();
                    return JokeSelectorFactory.CreateSelector(jokesConfig.Value.SelectionStrategy);
                });

                // Register state classes (Transient = a new instance is provided every time it is requested)
                builder.Services.AddTransient<WaitingForJokeRequestState>();
                builder.Services.AddTransient<WaitingForWhoState>();
                builder.Services.AddTransient<SendingPunchlineState>();
                builder.Services.AddTransient<LoopOrExitState>();

                // Register the Knock Knock server
                builder.Services.AddSingleton<Knock_KnockServer>();

                var host = builder.Build();

                // Validate all configuration options at startup
                ValidateAllOptions(host.Services);

                // Get the server and start it
                var server = host.Services.GetRequiredService<Knock_KnockServer>();
                server.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Details: {ex.InnerException.Message}");
            }
            finally
            {
                Console.WriteLine("Server has been stopped.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Validates all configuration options at startup.
        /// Throws OptionsValidationException if any option is invalid.
        /// </summary>
        static void ValidateAllOptions(IServiceProvider services)
        {
            Console.WriteLine("Validating configuration...");

            try
            {
                Console.Write("  - Validating ServerOptions... ");
                _ = services.GetRequiredService<IOptions<ServerOptions>>().Value;
                Console.WriteLine("✓");

                Console.Write("  - Validating JokesConfiguration... ");
                _ = services.GetRequiredService<IOptions<JokesConfiguration>>().Value;
                Console.WriteLine("✓");

                Console.WriteLine("All configurations validated successfully!\n");
            }
            catch (OptionsValidationException ex)
            {
                throw new InvalidOperationException(
                    $"Configuration validation failed: {ex.Message}", ex);
            }
        }
    }
}

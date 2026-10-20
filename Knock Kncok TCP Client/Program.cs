using Knock_Kncok_TCP_Client.Configuration;
using Knock_Kncok_TCP_Client.Configuration.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Knock_Kncok_TCP_Client
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
                builder.Services.Configure<ClientOptions>(configuration.GetSection("Connection"));
                builder.Services.AddSingleton<IValidateOptions<ClientOptions>, ClientOptionsValidator>();

                // Register the Knock Knock client
                builder.Services.AddSingleton<Knock_Kncok_Client>();

                var host = builder.Build();

                // Validate options at startup (this will throw if options are invalid)
                _ = host.Services.GetRequiredService<IOptions<ClientOptions>>().Value;

                // Get the client and start it
                using (var client = host.Services.GetRequiredService<Knock_Kncok_Client>())
                {
                    client.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Details: {ex.InnerException.Message}");
            }
            finally
            {
                Console.WriteLine("Client has disconnected.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}

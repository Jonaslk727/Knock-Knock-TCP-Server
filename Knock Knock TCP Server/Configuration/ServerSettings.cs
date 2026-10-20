using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.IO;

namespace Knock_Knock_TCP_Server.Configuration
{
    public class ServerSettings
    {
        public int Port { get; set; }
        public int MaxClients { get; set; }
        public List<JokeModel> Jokes { get; set; } = new List<JokeModel>();

        public static ServerSettings Load(string filePath = "appsettings.json")
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"appsettings.json not found at {filePath}");

            var json = File.ReadAllText(filePath);
            var jsonDocument = JsonDocument.Parse(json);
            var settingsElement = jsonDocument.RootElement.GetProperty("ServerSettings");

            var settings = new ServerSettings
            {
                Port = settingsElement.GetProperty("Port").GetInt32(),
                MaxClients = settingsElement.GetProperty("MaxClients").GetInt32()
            };

            var jokesArray = settingsElement.GetProperty("Jokes");
            foreach (var jokeElement in jokesArray.EnumerateArray())
            {
                var joke = new JokeModel
                {
                    Who = jokeElement.GetProperty("Who").GetString() ?? string.Empty,
                    Punchline = jokeElement.GetProperty("Punchline").GetString() ?? string.Empty
                };
                settings.Jokes.Add(joke);
            }

            return settings;
        }
    }
}

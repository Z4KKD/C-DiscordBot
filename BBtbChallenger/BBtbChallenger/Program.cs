using BBtbChallenger.GameLogic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BBtbChallenger
{
    class Program
    {
        private DiscordSocketClient? _client; // Removed nullable
        private CommandService? _commands; // Removed nullable
        private IServiceProvider? _services;

        static Task Main(string[] args) => new Program().MainAsync();

        private async Task MainAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged |
                     GatewayIntents.MessageContent |
                     GatewayIntents.GuildMessageReactions |
                     GatewayIntents.DirectMessageReactions
            });


            _commands = new CommandService();
            _services = ConfigureServices();

            _client.Log += Log;
            _commands.Log += Log;

            _client.Ready += () =>
            {
                Console.WriteLine("Bot is connected and ready!");
                return Task.CompletedTask;
            };

            _client.MessageReceived += HandleCommandAsync;

            var token = "MTM0MzY4NTk3NjMyMDYzODk3Ng.G3FVUZ.nY8QTKRTxd34zezx3fsxOFOFI44IRkcFsYauTo"; // Replace with your bot token
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            // Keep the program alive
            await Task.Delay(-1);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            if (messageParam is not SocketUserMessage message || message.Author.IsBot) return;

            int argPos = 0;
            if (!message.HasCharPrefix('!', ref argPos)) return;

            var context = new SocketCommandContext(_client, message);
            try
            {
                if (_commands != null)
                {
                    await _commands.ExecuteAsync(context, argPos, _services);
                }
                else
                {
                    Console.WriteLine("Warning: Commands service is not initialized.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing command: {ex.Message}");
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{msg.Severity}] {msg.Source}: {msg.Message}");
            Console.ResetColor();
            return Task.CompletedTask;
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(_client!)
                .AddSingleton(_commands!)
                .BuildServiceProvider();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBtbChallenger
{
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    using Microsoft.Extensions.DependencyInjection;
    using System.Reflection;

    class Program
    {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        static Task Main(string[] args) => new Program().MainAsync();

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
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
            _client.ButtonExecuted += BlackjackModule.HandleButton;

            var token = "//"; // Replace with your bot token
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            // Keep the program alive
            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            if (messageParam is not SocketUserMessage message || message.Author.IsBot) return;

            int argPos = 0;
            if (!message.HasCharPrefix('!', ref argPos)) return;

            var context = new SocketCommandContext(_client, message);

            await _commands.ExecuteAsync(context, argPos, _services);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }

}

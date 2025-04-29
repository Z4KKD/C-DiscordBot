using BBtbChallenger.GameLogic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BBtbChallenger;

class Program
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly IServiceProvider _services;

    private Program()
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
    }

    public static Task Main(string[] args) => new Program().RunAsync();

    private async Task RunAsync()
    {
        _client.Log += LogAsync;
        _commands.Log += LogAsync;
        _client.Ready += OnReadyAsync;
        _client.MessageReceived += HandleCommandAsync;

        var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
        if (string.IsNullOrWhiteSpace(token))
        {
            Console.WriteLine("Bot token is missing!");
            return;
        }

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        await Task.Delay(Timeout.Infinite);
    }

    private Task OnReadyAsync()
    {
        Console.WriteLine("Bot is connected and ready!");
        return Task.CompletedTask;
    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        if (messageParam is not SocketUserMessage message || message.Author.IsBot) return;

        int argPos = 0;
        if (!message.HasCharPrefix('!', ref argPos)) return;

        var context = new SocketCommandContext(_client, message);
        await _commands.ExecuteAsync(context, argPos, _services);
    }

    private Task LogAsync(LogMessage msg)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[{msg.Severity}] {msg.Source}: {msg.Message}");
        Console.ResetColor();
        return Task.CompletedTask;
    }

    private IServiceProvider ConfigureServices() => new ServiceCollection()
        .AddSingleton(_client)
        .AddSingleton(_commands)
        .BuildServiceProvider();
}

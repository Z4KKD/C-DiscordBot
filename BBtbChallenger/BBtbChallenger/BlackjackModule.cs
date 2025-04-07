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
    using System.Collections.Concurrent;

    public class BlackjackModule : ModuleBase<SocketCommandContext>
    {
        private static ConcurrentDictionary<ulong, BlackjackGame> games = new();

        [Command("blackjack")]
        public async Task StartBlackjack()
        {
            var game = new BlackjackGame();
            games[Context.User.Id] = game;

            var builder = new ComponentBuilder()
                .WithButton("Hit", customId: "bj_hit")
                .WithButton("Stand", customId: "bj_stand");

            await ReplyAsync(
                $"**Blackjack Game Started!**\nYour Hand: {game.GetPlayerHand()}\nDealer: {game.GetDealerHand()}",
                components: builder.Build());
        }

        public static async Task HandleButton(SocketMessageComponent component)
        {
            if (!games.TryGetValue(component.User.Id, out var game) || game.IsGameOver)
            {
                await component.RespondAsync("No active game found or game is over.", ephemeral: true);
                return;
            }

            if (component.Data.CustomId == "bj_hit")
            {
                bool alive = game.PlayerHit();
                if (!alive)
                {
                    await component.UpdateAsync(msg =>
                    {
                        msg.Content = $"**BUST!**\nYour Hand: {game.GetPlayerHand()}\nDealer: {game.GetDealerHand(true)}\nGame over.";
                        msg.Components = new ComponentBuilder().Build();
                    });
                }
                else
                {
                    await component.UpdateAsync(msg =>
                    {
                        msg.Content = $"Your Hand: {game.GetPlayerHand()}\nDealer: {game.GetDealerHand()}";
                    });
                }
            }
            else if (component.Data.CustomId == "bj_stand")
            {
                string result = game.PlayerStand();
                await component.UpdateAsync(msg =>
                {
                    msg.Content = $"**{result}**\nYour Hand: {game.GetPlayerHand()}\nDealer: {game.GetDealerHand(true)}";
                    msg.Components = new ComponentBuilder().Build();
                });
            }
        }
    }

}

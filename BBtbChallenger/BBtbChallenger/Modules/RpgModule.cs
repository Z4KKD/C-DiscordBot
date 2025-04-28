using BBtbChallenger.GameLogic;
using BBtbChallenger.Data;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBtbChallenger.Modules
{
    public class RpgModule : ModuleBase<SocketCommandContext>
    {
        private static ConcurrentDictionary<ulong, RpgCharacter> characters = new();
        private static readonly Dictionary<string, (string Description, int Price)> shopItems = new()
        {
            { "potion", ("Restores 30 HP", 25) },
            { "elixir", ("Fully restores health", 60) },
        };

        [Command("start")]
        public async Task StartRpg()
        {
            if (characters.ContainsKey(Context.User.Id))
            {
                await ReplyAsync("You've already started your adventure!");
                return;
            }

            var loaded = SaveManager.LoadCharacter(Context.User.Id);
            var character = loaded ?? new RpgCharacter(Context.User.Id, Context.User.Username);

            characters[Context.User.Id] = character;
            SaveManager.SaveCharacter(Context.User.Id, character);

            await ReplyAsync($"Welcome to the world, **{character.Name}**! Your journey begins now. 🗡️");
        }

        [Command("stats")]
        public async Task ShowStats()
        {
            if (!TryLoadCharacter(Context.User.Id, Context.User.Username, out var character))
            {
                await ReplyAsync("You haven't started yet. Use `!start` to begin your adventure.");
                return;
            }

            await ReplyAsync($"**{character.Name}** - Level {character.Level}\n" +
                             $"HP: {character.Health}/{character.MaxHealth}\n" +
                             $"Attack: {character.Attack} | Defense: {character.Defense}\n" +
                             $"XP: {character.Experience}/{character.ExperienceToNextLevel()}\n" +
                             $"💰 Coins: {character.Coins}");
        }

        [Command("fight")]
        public async Task StartFightCommand()
        {
            if (!TryLoadCharacter(Context.User.Id, Context.User.Username, out var character))
            {
                await ReplyAsync("You haven't started yet. Use `!start` to begin your adventure.");
                return;
            }

            var enemies = EnemyFactory.GetEnemiesForLevel(character.Level);
            var random = new Random();
            var randomEnemy = enemies[random.Next(enemies.Count)];

            var placeholderMessage = await ReplyAsync("⚔️ Preparing for battle...");
            var fightText = await BattleManager.StartFight(character, randomEnemy, placeholderMessage.Id, placeholderMessage.Channel.Id);

            await placeholderMessage.ModifyAsync(msg => msg.Content = fightText);
        }

        [Command("action")]
        public async Task ActionCommand(string action)
        {
            if (!BattleManager.OngoingBattles.TryGetValue(Context.User.Id, out var battle))
            {
                await ReplyAsync("You have no ongoing battle.");
                return;
            }

            var result = await BattleManager.ProcessAction(battle, action);
            await ReplyAsync(result);
        }

        [Command("use")]
        public async Task UseItem(string item)
        {
            if (!TryLoadCharacter(Context.User.Id, Context.User.Username, out var character))
            {
                await ReplyAsync("You haven't started yet. Use `!start` to begin your adventure.");
                return;
            }

            item = item.ToLowerInvariant();

            if (!character.Inventory.Any(i => i.Equals(item, StringComparison.OrdinalIgnoreCase)))
            {
                await ReplyAsync("You don't have that item.");
                return;
            }

            string response = item switch
            {
                "potion" => UsePotion(character),
                "elixir" => UseElixir(character),
                _ => "That item can't be used or isn't recognized."
            };

            SaveManager.SaveCharacter(Context.User.Id, character);
            await ReplyAsync(response);
        }

        [Command("shop")]
        public async Task ShowShop()
        {
            var sb = new StringBuilder();
            sb.AppendLine("🛒 **Welcome to the Item Shop!**");
            foreach (var item in shopItems)
            {
                sb.AppendLine($"**{item.Key}** - {item.Value.Description} | 💰 {item.Value.Price} coins");
            }
            sb.AppendLine("\nType `!buy [item]` to purchase something.");
            await ReplyAsync(sb.ToString());
        }

        [Command("buy")]
        public async Task BuyItem(string itemName)
        {
            if (!TryLoadCharacter(Context.User.Id, Context.User.Username, out var character))
            {
                await ReplyAsync("You haven't started yet. Use `!start` to begin your adventure.");
                return;
            }

            itemName = itemName.ToLowerInvariant();
            if (!shopItems.TryGetValue(itemName, out var itemInfo))
            {
                await ReplyAsync("That item doesn't exist in the shop.");
                return;
            }

            if (character.Coins < itemInfo.Price)
            {
                await ReplyAsync($"You need {itemInfo.Price} coins to buy a {itemName}, but you only have {character.Coins}.");
                return;
            }

            character.Coins -= itemInfo.Price;
            character.Inventory.Add(itemName);
            SaveManager.SaveCharacter(Context.User.Id, character);
            await ReplyAsync($"✅ You bought a **{itemName}**! It’s now in your inventory.");
        }

        [Command("inventory")]
        public async Task ShowInventory()
        {
            if (!TryLoadCharacter(Context.User.Id, Context.User.Username, out var character))
            {
                await ReplyAsync("You haven't started yet. Use `!start` to begin your adventure.");
                return;
            }

            if (!character.Inventory.Any())
            {
                await ReplyAsync("Your inventory is empty.");
                return;
            }

            var groupedItems = character.Inventory.GroupBy(i => i).ToDictionary(g => g.Key, g => g.Count());

            var sb = new StringBuilder();
            sb.AppendLine("🎒 **Your Inventory:**");
            foreach (var item in groupedItems)
                sb.AppendLine($"• {item.Key} x{item.Value}");

            await ReplyAsync(sb.ToString());
        }

        // --- Private helper methods below ---

        private bool TryLoadCharacter(ulong userId, string username, out RpgCharacter character)
        {
            if (!characters.TryGetValue(userId, out character))
            {
                var loaded = SaveManager.LoadCharacter(userId);
                if (loaded != null)
                {
                    characters[userId] = loaded;
                    character = loaded;
                }
            }
            return character != null;
        }

        private static string UsePotion(RpgCharacter character)
        {
            if (character.Health == character.MaxHealth)
                return "You're already at full health.";

            character.Health = Math.Min(character.Health + 30, character.MaxHealth);
            character.Inventory.Remove("potion");
            return $"{character.Name} used a Potion and healed 30 HP! 💊";
        }

        private static string UseElixir(RpgCharacter character)
        {
            if (character.Health == character.MaxHealth)
                return "You're already at full health.";

            character.Health = character.MaxHealth;
            character.Inventory.Remove("elixir");
            return $"{character.Name} used an Elixir and fully restored their HP! ✨";
        }
    }
}

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
        { "elixir", ("Fully restores health", 75) },
        { "small mana potion", ("Restores 30 Mana", 25) },  
        { "large mana potion", ("Restores 70 Mana", 50) }  
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
                             $"Mana: {character.Mana}/{character.MaxMana}\n" + // Added Mana to stats
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
            character.UserId = Context.User.Id;
            var enemies = EnemyFactory.GetEnemiesForLevel(character.Level);
            var random = new Random();
            var randomEnemy = enemies[random.Next(enemies.Count)];

            var placeholderMessage = await ReplyAsync("⚔️ Preparing for battle...");
            var fightText = await BattleManager.StartFight(character, randomEnemy, placeholderMessage.Id, placeholderMessage.Channel.Id);

            await placeholderMessage.ModifyAsync(msg => msg.Content = fightText);
        }

        [Command("a")]
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
                "mana potion" => UseManaPotion(character),
                "mana elixir" => UseManaElixir(character),
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
        public async Task BuyItem(params string[] itemNameParts)
        {
            if (!TryLoadCharacter(Context.User.Id, Context.User.Username, out var character))
            {
                await ReplyAsync("You haven't started yet. Use `!start` to begin your adventure.");
                return;
            }

            if (itemNameParts.Length == 0)
            {
                await ReplyAsync("Please specify an item to buy. Example: `!buy bronze shield`");
                return;
            }

            // Join the item name parts into a single string
            string itemName = string.Join(" ", itemNameParts).ToLowerInvariant();

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

            // Deduct coins and add the item to inventory
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

        [Command("help")]
        public async Task ShowHelp()
        {
            var sb = new StringBuilder();
            sb.AppendLine("📖 **RPG Commands Help**");
            sb.AppendLine("`!start` - Begin your adventure.");
            sb.AppendLine("`!stats` - View your character's stats.");
            sb.AppendLine("`!fight` - Fight a random enemy.");
            sb.AppendLine("`!a [a|d|m]` - Perform an action in battle. Type `!a a` for attack, `!a d` for defend, `!a m` for magic.");
            sb.AppendLine("`!use [item]` - Use an item from your inventory.");
            sb.AppendLine("`!shop` - View items available in the shop.");
            sb.AppendLine("`!buy [item]` - Buy an item from the shop. Example: `!buy bronze shield`.");
            sb.AppendLine("`!sell [item]` - Sell an item from your inventory.");
            sb.AppendLine("`!inventory` - Check your inventory.");
            sb.AppendLine("`!equip [sword|shield|armor|helmet]` - Equip your best gear.");
            sb.AppendLine("`!help` - Show this help menu.");

            await ReplyAsync(sb.ToString());
        }


        [Command("sell")]
        public async Task SellItem(params string[] itemNameParts)
        {
            if (!TryLoadCharacter(Context.User.Id, Context.User.Username, out var character))
            {
                await ReplyAsync("You haven't started yet. Use `!start` to begin your adventure.");
                return;
            }

            if (itemNameParts.Length == 0)
            {
                await ReplyAsync("Please specify an item to sell. Example: `!sell bronze shield`");
                return;
            }

            string itemName = string.Join(" ", itemNameParts).ToLowerInvariant();

            // Check if they have the item
            var inventoryItem = character.Inventory.FirstOrDefault(i => i.Equals(itemName, StringComparison.OrdinalIgnoreCase));
            if (inventoryItem == null)
            {
                await ReplyAsync("You don't have that item in your inventory.");
                return;
            }

            // Calculate sell price (half of shop price if exists, otherwise a default)
            int sellPrice = shopItems.TryGetValue(itemName, out var itemInfo) ? itemInfo.Price / 2 : 10;

            // Remove the item and add coins
            character.Inventory.Remove(inventoryItem);
            character.Coins += sellPrice;

            SaveManager.SaveCharacter(Context.User.Id, character);
            await ReplyAsync($"💰 You sold **{itemName}** for **{sellPrice} coins**!");
        }


        [Command("equip")]
        public async Task EquipItem(string type)
        {
            if (!characters.TryGetValue(Context.User.Id, out var character))
            {
                await ReplyAsync("You need to start your adventure first with `!start`.");
                return;
            }

            type = type.ToLower();

            // Define item strength ranking
                    Dictionary<string, int> weaponPower = new()
            {
                { "bronze sword", 5 }, { "iron sword", 10 }, { "steel sword", 15 },
                { "silver sword", 20 }, { "gold sword", 25 }, { "mithril sword", 30 },
                { "adamantite sword", 35 }, { "platinum sword", 40 }
            };

                    Dictionary<string, int> armorPower = new()
            {
                { "iron armor", 10 }, { "steel armor", 15 }, { "silver armor", 20 },
                { "gold armor", 25 }, { "mithril armor", 30 }, { "adamantite armor", 35 },
                { "platinum armor", 40 }
            };

                    Dictionary<string, int> shieldPower = new()
            {
                { "bronze shield", 5 }, { "iron shield", 10 }, { "steel shield", 15 },
                { "silver shield", 20 }, { "gold shield", 25 }, { "mithril shield", 30 },
                { "adamantite shield", 35 }, { "platinum shield", 40 }
            };

                    Dictionary<string, int> helmetPower = new()
            {
                { "bronze helmet", 5 }, { "iron helmet", 10 }, { "steel helmet", 15 },
                { "gold helmet", 20 }
            };

            string? bestItem = null;
            int bestBonus = 0;

            if (type == "sword")
            {
                foreach (var item in character.Inventory)
                {
                    if (weaponPower.TryGetValue(item.ToLower(), out var bonus) && bonus > bestBonus)
                    {
                        bestItem = item;
                        bestBonus = bonus;
                    }
                }

                if (bestItem != null)
                {
                    if (character.Weapon != null) character.UnequipWeapon();
                    character.EquipWeapon(bestItem, bestBonus, isTwoHanded: false);
                    await ReplyAsync($"You equipped **{bestItem}** (+{bestBonus} Attack)!");
                }
                else
                {
                    await ReplyAsync("You don't have a sword to equip.");
                }
            }
            else if (type == "shield")
            {
                foreach (var item in character.Inventory)
                {
                    if (shieldPower.TryGetValue(item.ToLower(), out var bonus) && bonus > bestBonus)
                    {
                        bestItem = item;
                        bestBonus = bonus;
                    }
                }

                if (bestItem != null)
                {
                    if (character.Shield != null) character.UnequipShield();
                    character.EquipShield(bestItem, bestBonus);
                    await ReplyAsync($"You equipped **{bestItem}** (+{bestBonus} Defense)!");
                }
                else
                {
                    await ReplyAsync("You don't have a shield to equip.");
                }
            }
            else if (type == "armor")
            {
                foreach (var item in character.Inventory)
                {
                    if (armorPower.TryGetValue(item.ToLower(), out var bonus) && bonus > bestBonus)
                    {
                        bestItem = item;
                        bestBonus = bonus;
                    }
                }

                if (bestItem != null)
                {
                    if (character.Armor != null) character.UnequipArmor();
                    character.EquipArmor(bestItem, bestBonus);
                    await ReplyAsync($"You equipped **{bestItem}** (+{bestBonus} Defense)!");
                }
                else
                {
                    await ReplyAsync("You don't have armor to equip.");
                }
            }
            else if (type == "helmet")
            {
                foreach (var item in character.Inventory)
                {
                    if (helmetPower.TryGetValue(item.ToLower(), out var bonus) && bonus > bestBonus)
                    {
                        bestItem = item;
                        bestBonus = bonus;
                    }
                }

                if (bestItem != null)
                {
                    if (character.Helmet != null) character.UnequipHelmet();
                    character.EquipHelmet(bestItem, bestBonus);
                    await ReplyAsync($"You equipped **{bestItem}** (+{bestBonus} Defense)!");
                }
                else
                {
                    await ReplyAsync("You don't have a helmet to equip.");
                }
            }
            else
            {
                await ReplyAsync("Please specify a valid item type: `sword`, `shield`, `armor`, or `helmet`.");
            }

            SaveManager.SaveCharacter(Context.User.Id, character);
        }


        // --- Private helper methods below ---

        private bool TryLoadCharacter(ulong userId, string username, out RpgCharacter character)
        {
            character = null!; // prevent warning

            if (!characters.TryGetValue(userId, out var foundCharacter))
            {
                var loaded = SaveManager.LoadCharacter(userId);
                if (loaded != null)
                {
                    characters[userId] = loaded;
                    character = loaded;
                }
            }
            else
            {
                character = foundCharacter;
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

        private static string UseManaPotion(RpgCharacter character)
        {
            if (character.Mana == character.MaxMana)
                return "Your mana is already full.";

            character.Mana = Math.Min(character.Mana + 30, character.MaxMana);
            character.Inventory.Remove("mana potion");
            return $"{character.Name} used a Mana Potion and restored 30 Mana! 🔵";
        }

        private static string UseManaElixir(RpgCharacter character)
        {
            if (character.Mana == character.MaxMana)
                return "Your mana is already full.";

            character.Mana = character.MaxMana;
            character.Inventory.Remove("mana elixir");
            return $"{character.Name} used a Mana Elixir and fully restored their Mana! ✨🔵";
        }

    }
}

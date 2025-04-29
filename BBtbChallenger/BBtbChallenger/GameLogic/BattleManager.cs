using BBtbChallenger.Data;
using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BBtbChallenger.GameLogic
{
    internal class BattleManager
    {
        public static Dictionary<ulong, BattleState> OngoingBattles = new();

        public static Task<string> StartFight(RpgCharacter character, Enemy enemy, ulong messageId, ulong channelId)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"⚔️ **{character.Name}** engages **{enemy.Name}**!");

            OngoingBattles[character.UserId] = new BattleState
            {
                Character = character,
                Enemy = enemy,
                IsPlayerTurn = true,
                BattleMessageId = messageId,
                ChannelId = channelId
            };

            sb.AppendLine("It's your turn! Type:\n`!a a (attack)`\n`!a d (defend)`\n`!a m (magic)`");

            return Task.FromResult(sb.ToString());
        }

        public static Task<string> ProcessAction(BattleState battle, string action)
        {
            var character = battle.Character;
            var enemy = battle.Enemy;
            var sb = new StringBuilder();

            if (!battle.IsPlayerTurn)
                return Task.FromResult("It's not your turn yet!");

            int damage = 0;

            // Process action
            switch (action.ToLower())
            {
                case "a":
                    // Attack action
                    damage = character.GetAttackDamage();
                    enemy.Health -= damage;
                    sb.AppendLine($"{character.Name} attacks {enemy.Name} for {damage} damage!");
                    break;

                case "d":
                    // Defend action
                    sb.AppendLine(Defend(character));
                    break;

                case "m":
                    // Magic action - use all unlocked abilities
                    string magicUsed = string.Empty;
                    bool magicCast = false;

                    // Loop through all available abilities and cast them
                    foreach (var ability in character.MagicAbilities)
                    {
                        if (MagicManager.Spells.ContainsKey(ability))
                        {
                            var spell = MagicManager.Spells[ability];
                            int manaCost = spell.ManaCost;

                            // Check if the character has enough mana for the current ability
                            if (character.Mana >= manaCost)
                            {
                                sb.AppendLine(spell.Item3(character, enemy)); // Cast the ability
                                magicUsed += $"{ability}, "; // Add the used ability to the magicUsed string
                                magicCast = true;
                            }
                            else
                            {
                                sb.AppendLine($"Not enough mana for '{ability}' spell!");
                            }
                        }
                    }

                    if (magicCast)
                    {
                        sb.AppendLine($"Used magic abilities: {magicUsed.TrimEnd(',', ' ')}.");
                    }
                    else
                    {
                        sb.AppendLine("You have no magic ability available or insufficient mana!");
                    }
                    break;

                default:
                    return Task.FromResult("Invalid action. You can choose 'a' (attack), 'd' (defend), or 'm' (magic).");
            }

            // Show updated HP and Mana for the player and HP for the enemy
            sb.AppendLine($"\nCurrent Stats: HP: {character.Health}/{character.MaxHealth} | Mana: {character.Mana}/{character.MaxMana}");
            sb.AppendLine($"**{enemy.Name}** HP: {enemy.Health}/{enemy.MaxHealth}");

            // Check if enemy defeated
            if (!enemy.IsAlive)
            {
                sb.AppendLine($"You defeated the {enemy.Name}! 🎉");
                character.GainExperience(enemy.ExperienceReward);
                int coins = enemy.GetCoinDrop();
                character.Coins += coins;
                var loot = enemy.GetLootDrop();
                if (loot != null)
                {
                    character.Inventory.Add(loot.Item);
                    sb.AppendLine($"You found a **{loot.Item}**!");
                }
                SaveManager.SaveCharacter(character.UserId, character);
                OngoingBattles.Remove(character.UserId);
                return Task.FromResult(sb.ToString());
            }

            enemy.UpdateStun();
            if (enemy.IsStunned)
            {
                sb.AppendLine($"**{enemy.Name}** is stunned and cannot act this turn!");
            }
            else
            {
                // Enemy attacks
                sb.AppendLine("\nIt's the enemy's turn!");
                int enemyDamage = Math.Max(1, enemy.Attack - character.Defense);
                character.Health -= enemyDamage;
                sb.AppendLine($"{enemy.Name} strikes you for {enemyDamage} damage!");
            }

            // Show current stats after enemy attack or skip
            sb.AppendLine($"\nCurrent Stats: HP: {character.Health}/{character.MaxHealth} | Mana: {character.Mana}/{character.MaxMana}");
            sb.AppendLine($"**{enemy.Name}** HP: {enemy.Health}/{enemy.MaxHealth}");

            if (!character.IsAlive)
            {
                character.Die();
                sb.AppendLine("You were defeated... 😵 Use `!start` to revive.");
                SaveManager.SaveCharacter(character.UserId, character);
                OngoingBattles.Remove(character.UserId);
            }
            else
            {
                battle.IsPlayerTurn = true;
            }

            return Task.FromResult(sb.ToString());
        }

        public static string Defend(RpgCharacter character)
        {
            int defenseBoost = 10;
            character.Defense += defenseBoost;
            return $"{character.Name} braces and boosts defense by {defenseBoost} this turn!";
        }
    }

    public class BattleState
    {
        public required RpgCharacter Character { get; set; }
        public required Enemy Enemy { get; set; }
        public bool IsPlayerTurn { get; set; }
        public ulong BattleMessageId { get; set; }
        public ulong ChannelId { get; set; }
    }
}

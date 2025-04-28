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

        public static async Task<string> StartFight(RpgCharacter character, Enemy enemy, ulong messageId, ulong channelId)
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

            sb.AppendLine("It's your turn! Type:\n`!action attack`\n`!action defend`\n`!action magic`");

            return sb.ToString();
        }

        public static async Task<string> ProcessAction(BattleState battle, string action)
        {
            var character = battle.Character;
            var enemy = battle.Enemy;
            var sb = new StringBuilder();

            if (!battle.IsPlayerTurn)
                return "It's not your turn yet!";

            int damage = 0;

            switch (action.ToLower())
            {
                case "attack":
                    damage = character.GetAttackDamage();
                    enemy.Health -= damage;
                    sb.AppendLine($"{character.Name} attacks {enemy.Name} for {damage} damage!");
                    break;

                case "defend":
                    sb.AppendLine(Defend(character));
                    break;

                case "magic":
                    if (MagicManager.Spells.ContainsKey("Fireball"))
                        sb.AppendLine(MagicManager.Spells["Fireball"].Item3(character, enemy));
                    else
                        sb.AppendLine("You have no magic ability!");
                    break;

                default:
                    return "Invalid action. You can choose 'attack', 'defend', or 'magic'.";
            }

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

                OngoingBattles.Remove(character.UserId);
                return sb.ToString();
            }

            // Enemy attacks
            sb.AppendLine("\nIt's the enemy's turn!");
            int enemyDamage = Math.Max(0, enemy.Attack - character.Defense);
            character.Health -= enemyDamage;
            sb.AppendLine($"{enemy.Name} strikes you for {enemyDamage} damage!");

            if (!character.IsAlive)
            {
                character.Die();
                sb.AppendLine("You were defeated... 😵 Use `!start` to revive.");
                OngoingBattles.Remove(character.UserId);
            }
            else
            {
                battle.IsPlayerTurn = true;
            }

            return sb.ToString();
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
        public RpgCharacter Character { get; set; }
        public Enemy Enemy { get; set; }
        public bool IsPlayerTurn { get; set; }
        public ulong BattleMessageId { get; set; }
        public ulong ChannelId { get; set; }
    }
}

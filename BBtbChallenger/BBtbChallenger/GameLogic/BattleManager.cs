using BBtbChallenger.Data;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBtbChallenger.GameLogic
{
    internal class BattleManager
    {
        public static Dictionary<ulong, BattleState> OngoingBattles = new();

        private const string InvalidActionMessage = "Invalid action. You can choose 'a' (attack), 'd' (defend), or 'm' (magic).";
        private const string PlayerTurnMessage = "It's your turn! Type:\n`!a a (attack)`\n`!a d (defend)`\n`!a m (magic)`";

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

            sb.AppendLine(PlayerTurnMessage);
            return Task.FromResult(sb.ToString());
        }

        public static Task<string> ProcessAction(BattleState battle, string action)
        {
            if (!battle.IsPlayerTurn)
                return Task.FromResult("It's not your turn yet!");

            var character = battle.Character;
            var enemy = battle.Enemy;
            var sb = new StringBuilder();
            int damage = 0;

            switch (action.ToLower())
            {
                case "a":
                    damage = AttackAction(character, enemy);
                    sb.AppendLine($"{character.Name} attacks {enemy.Name} for {damage} damage!");
                    break;

                case "d":
                    sb.AppendLine(DefendAction(character));
                    break;

                case "m":
                    sb.AppendLine(MagicAction(character, enemy));
                    break;

                default:
                    return Task.FromResult(InvalidActionMessage);
            }

            sb.AppendLine(ShowUpdatedStats(character, enemy));

            if (!enemy.IsAlive)
            {
                sb.AppendLine($"You defeated the {enemy.Name}! 🎉");
                character.GainExperience(enemy.ExperienceReward);
                character.Coins += enemy.GetCoinDrop();
                HandleLootDrop(character, enemy, sb);
                SaveManager.SaveCharacter(character.UserId, character);
                OngoingBattles.Remove(character.UserId);
                return Task.FromResult(sb.ToString());
            }

            ProcessEnemyTurn(character, enemy, sb);

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

        private static int AttackAction(RpgCharacter character, Enemy enemy)
        {
            int damage = character.GetAttackDamage();
            enemy.Health -= damage;
            return damage;
        }

        private static string DefendAction(RpgCharacter character)
        {
            const int defenseBoost = 10;
            character.Defense += defenseBoost;
            return $"{character.Name} braces and boosts defense by {defenseBoost} this turn!";
        }

        private static string MagicAction(RpgCharacter character, Enemy enemy)
        {
            var sb = new StringBuilder();
            string magicUsed = string.Empty;
            bool magicCast = false;

            foreach (var ability in character.MagicAbilities)
            {
                if (MagicManager.Spells.ContainsKey(ability))
                {
                    var spell = MagicManager.Spells[ability];
                    int manaCost = spell.ManaCost;

                    if (character.Mana >= manaCost)
                    {
                        sb.AppendLine(spell.Effect(character, enemy));
                        magicUsed += $"{ability}, ";
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

            return sb.ToString();
        }

        private static string ShowUpdatedStats(RpgCharacter character, Enemy enemy)
        {
            return $"\nCurrent Stats: HP: {character.Health}/{character.MaxHealth} | Mana: {character.Mana}/{character.MaxMana}" +
                   $"\n**{enemy.Name}** HP: {enemy.Health}/{enemy.MaxHealth}";
        }

        private static void HandleLootDrop(RpgCharacter character, Enemy enemy, StringBuilder sb)
        {
            var loot = enemy.GetLootDrop();
            if (loot != null)
            {
                character.Inventory.Add(loot.Item);
                sb.AppendLine($"You found a **{loot.Item}**!");
            }
        }

        private static void ProcessEnemyTurn(RpgCharacter character, Enemy enemy, StringBuilder sb)
        {
            enemy.UpdateStun();

            if (enemy.IsStunned)
            {
                sb.AppendLine($"**{enemy.Name}** is stunned and cannot act this turn!");
            }
            else
            {
                sb.AppendLine("\nIt's the enemy's turn!");
                int enemyDamage = Math.Max(1, enemy.Attack - character.Defense);
                character.Health -= enemyDamage;
                sb.AppendLine($"{enemy.Name} strikes you for {enemyDamage} damage!");
            }
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

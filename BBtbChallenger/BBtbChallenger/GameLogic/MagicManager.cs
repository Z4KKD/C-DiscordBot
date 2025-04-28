using BBtbChallenger.GameLogic;
using System;
using System.Collections.Generic;

namespace BBtbChallenger.GameLogic
{
    public static class MagicManager
    {
        public static readonly Dictionary<string, (string Description, int ManaCost, Func<RpgCharacter, Enemy, string>)> Spells = new()
        {
            {
                "fireball",
                (
                    "Deals 30 damage to an enemy",
                    20,
                    (RpgCharacter character, Enemy enemy) =>
                    {
                        if (character.Mana >= 20)
                        {
                            character.Mana -= 20;
                            enemy.Health -= 30;
                            return $"{character.Name} casts Fireball! It deals 30 damage to {enemy.Name}.";
                        }
                        return "Not enough mana to cast Fireball.";
                    }
                )
            },

            {
                "stun",
                (
                    "Stuns an enemy for one turn",
                    15,
                    (RpgCharacter character, Enemy enemy) =>
                    {
                        if (character.Mana >= 15)
                        {
                            character.Mana -= 15;
                            enemy.IsStunned = true;  // Assuming Enemy has an `IsStunned` property
                            return $"{character.Name} casts Stun! {enemy.Name} is stunned and cannot act this turn.";
                        }
                        return "Not enough mana to cast Stun.";
                    }
                )
            },

            {
                "powerup",
                (
                    "Doubles the player's attack damage for the next turn",
                    25,
                    (RpgCharacter character, Enemy enemy) =>
                    {
                        if (character.Mana >= 25)
                        {
                            character.Mana -= 25;
                            character.IsPowerUpActive = true;  // Assuming RpgCharacter has this property
                            return $"{character.Name} casts Power Up! Your attack damage is doubled for the next turn.";
                        }
                        return "Not enough mana to cast Power Up.";
                    }
                )
            },
        };
    }
}

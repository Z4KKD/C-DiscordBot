using BBtbChallenger.GameLogic;

public static class MagicManager
{
    public static readonly Dictionary<string, (string Description, int ManaCost, Func<RpgCharacter, Enemy, string>)> Spells = new()
    {
        {
            "rock",
            (
                "Deals 15 damage to an enemy",
                10,
                (RpgCharacter character, Enemy enemy) =>
                {
                    if (character.Mana >= 10)
                    {
                        character.Mana -= 10;
                        enemy.Health -= 15;
                        return $"{character.Name} casts Rock! It deals 15 damage to {enemy.Name}.";
                    }
                    return "Not enough mana to cast Rock.";
                }
            )
        },

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
                "Stuns an enemy for two turn",
                30,
                (RpgCharacter character, Enemy enemy) =>
                {
                    if (character.Mana >= 30)
                    {
                        character.Mana -= 30;
                        enemy.IsStunned = true;
                        enemy.TurnsStunned = 2;
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
                40,
                (RpgCharacter character, Enemy enemy) =>
                {
                    if (character.Mana >= 25)
                    {
                        character.Mana -= 25;
                        character.IsPowerUpActive = true;
                        return $"{character.Name} casts Power Up! Your attack damage is doubled for the next turn.";
                    }
                    return "Not enough mana to cast Power Up.";
                }
            )
        },
    };
}

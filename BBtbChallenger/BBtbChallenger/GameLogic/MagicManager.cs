using BBtbChallenger.GameLogic;

namespace BBtbChallenger.GameLogic;

public static class MagicManager
{
    public static readonly Dictionary<string, Spell> Spells = new()
    {
        ["rock"] = new Spell("Deals 15 damage to an enemy", 10, (c, e) =>
        {
            if (c.Mana < 10) return "Not enough mana to cast Rock.";
            c.Mana -= 10;
            e.Health -= 15;
            return $"{c.Name} casts Rock! It deals 15 damage to {e.Name}.";
        }),

        ["fireball"] = new Spell("Deals 30 damage to an enemy", 20, (c, e) =>
        {
            if (c.Mana < 20) return "Not enough mana to cast Fireball.";
            c.Mana -= 20;
            e.Health -= 30;
            return $"{c.Name} casts Fireball! It deals 30 damage to {e.Name}.";
        }),

        ["stun"] = new Spell("Stuns an enemy for two turns", 30, (c, e) =>
        {
            if (c.Mana < 30) return "Not enough mana to cast Stun.";
            c.Mana -= 30;
            e.IsStunned = true;
            e.TurnsStunned = 2;
            return $"{c.Name} casts Stun! {e.Name} is stunned.";
        }),

        ["powerup"] = new Spell("Doubles attack damage for next turn", 40, (c, _) =>
        {
            if (c.Mana < 40) return "Not enough mana to cast Power Up.";
            c.Mana -= 40;
            c.IsPowerUpActive = true;
            return $"{c.Name} casts Power Up! Attack damage doubled next turn.";
        })
    };
}

public record Spell(string Description, int ManaCost, Func<RpgCharacter, Enemy, string> Effect);

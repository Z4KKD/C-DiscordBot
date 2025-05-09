﻿public class RpgCharacter
{
    public ulong UserId { get; set; }  // Add UserId property to associate character with a Discord user
    public string? Name { get; set; }
    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    public int Health { get; set; } = 100;
    public int Attack { get; set; } = 10;
    public int Defense { get; set; } = 5;
    public int Mana { get; set; } = 50;
    public int MaxMana => 50 + (Level - 1) * 10;
    public int Coins { get; set; } = 100;
    public List<string> Inventory { get; set; } = new();
    public string? Weapon { get; set; }
    public string? Armor { get; set; }
    public string? Helmet { get; set; }
    public string? Shield { get; set; }
    public DateTime LastDeathTime { get; set; }

    public int WeaponBonus { get; set; }
    public int ArmorBonus { get; set; }
    public int HelmetBonus { get; set; }
    public int ShieldBonus { get; set; }

    public bool IsAlive => Health > 0;
    public int MaxHealth => 100 + (Level - 1) * 10;
    public bool IsPowerUpActive { get; set; } = false;
    public int FishingLevel { get; set; } = 1;
    public int FishingExperience { get; set; } = 0;
    public int FishingExpToNextLevel() => 50 + (FishingLevel - 1) * 25;

    public void GainFishingExperience(int amount)
    {
        FishingExperience += amount;
        while (FishingExperience >= FishingExpToNextLevel())
        {
            FishingExperience -= FishingExpToNextLevel();
            FishingLevel++;
        }
    }
    // Add UserId to associate character with Discord user
    public RpgCharacter(ulong userId, string name)
    {
        UserId = userId;
        Name = name;
    }
    public List<string> MagicAbilities
    {
        get
        {
            List<string> abilities = new List<string>();

            if (Level >= 0) abilities.Add("rock");
            if (Level >= 5) abilities.Add("fireball");
            if (Level >= 10) abilities.Add("stun");
            if (Level >= 15) abilities.Add("powerup");

            return abilities;
        }
    }


    public void GainExperience(int amount)
    {
        Experience += amount;
        while (Experience >= ExperienceToNextLevel())
        {
            Experience -= ExperienceToNextLevel();
            Level++;
            Health = MaxHealth;
            Mana = MaxMana;
            Attack += 1;
            Defense += 1;
        }
    }

    public int ExperienceToNextLevel() => 100 + (Level - 1) * 50;

    public void EquipWeapon(string weapon, int bonus, bool isTwoHanded)
    {
        if (Weapon != null) UnequipWeapon();
        if (isTwoHanded && Shield != null) UnequipShield();

        Weapon = weapon.ToLower();
        WeaponBonus = bonus;
        Attack += bonus;
    }

    public void EquipShield(string shield, int bonus)
    {

        if (Shield != null) UnequipShield();

        Shield = shield.ToLower();
        ShieldBonus = bonus;
        Defense += bonus;
    }

    public void EquipHelmet(string helmet, int bonus)
    {
        if (Helmet != null) UnequipHelmet();

        Helmet = helmet.ToLower();
        HelmetBonus = bonus;
        Defense += bonus;
    }

    public void EquipArmor(string armor, int bonus)
    {
        if (Armor != null) UnequipArmor();

        Armor = armor.ToLower();
        ArmorBonus = bonus;
        Defense += bonus;
    }

    public void UnequipWeapon()
    {
        Attack -= WeaponBonus;
        Weapon = null;
        WeaponBonus = 0;
    }

    public void UnequipArmor()
    {
        Defense -= ArmorBonus;
        Armor = null;
        ArmorBonus = 0;
    }

    public void UnequipShield()
    {
        Defense -= ShieldBonus;
        Shield = null;
        ShieldBonus = 0;
    }

    public void UnequipHelmet()
    {
        Defense -= HelmetBonus;
        Helmet = null;
        HelmetBonus = 0;
    }

    public int GetAttackDamage()
    {
        int totalAttack = Attack + WeaponBonus;

        if (IsPowerUpActive)
        {
            totalAttack *= 2;
            IsPowerUpActive = false; 
        }

        return totalAttack;
    }

    public void Die()
    {
        Health = MaxHealth;
        Mana = MaxMana;
        LastDeathTime = DateTime.UtcNow;
    }
}

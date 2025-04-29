using System;
using System.Collections.Generic;

namespace BBtbChallenger.GameLogic
{
    public class Enemy
    {
        public string Name { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Attack { get; set; }
        public int ExperienceReward { get; set; }
        public int CoinDropMin { get; set; }
        public int CoinDropMax { get; set; }
        public List<Drop> Drops { get; set; } = new List<Drop>();
        public bool IsStunned { get; set; } = false;
        public int TurnsStunned { get; set; } = 0;


        public bool IsAlive => Health > 0;

        public Enemy(string name, int maxHealth, int attack, int expReward, int coinDropMin, int coinDropMax, List<Drop>? drops = null)
        {
            Name = name;
            MaxHealth = maxHealth;
            Health = maxHealth;
            Attack = attack;
            ExperienceReward = expReward;
            CoinDropMin = coinDropMin;
            CoinDropMax = coinDropMax;
            Drops = drops ?? new List<Drop>();
        }
        public Enemy Clone()
        {
            return new Enemy(
                Name,
                MaxHealth,
                Attack,
                ExperienceReward,
                CoinDropMin,
                CoinDropMax,
                new List<Drop>(Drops) // make a copy of the Drops list
            );
        }

        public int GetCoinDrop()
        {
            Random rand = new Random();
            return rand.Next(CoinDropMin, CoinDropMax);
        }

        public Drop GetLootDrop()
        {
            Random rand = new Random();
            int roll = rand.Next(1, 101);

            foreach (var drop in Drops)
            {
                if (roll <= drop.Chance)
                    return drop;
            }
            return null!;
        }

        public void UpdateStun()
        {
            if (IsStunned)
            {
                TurnsStunned--;
                if (TurnsStunned <= 0)
                {
                    IsStunned = false;
                }
            }
        }

    }

    public class Drop
    {
        public string Item { get; set; }
        public int Chance { get; set; } 

        public Drop(string item, int chance)
        {
            Item = item;
            Chance = chance;
        }
    }

    public static class EnemyFactory
    {
        private static Enemy CreateEnemy(string name, int baseHealth, int baseAttack, int baseExp, int minDrop, int maxDrop, List<Drop> drops, float healthScalingFactor, float attackScalingFactor, float expScalingFactor)
        {
            // Apply scaling
            int scaledHealth = (int)(baseHealth * healthScalingFactor);
            int scaledAttack = (int)(baseAttack * attackScalingFactor);
            int scaledExp = (int)(baseExp * expScalingFactor);

            return new Enemy(name, scaledHealth, scaledAttack, scaledExp, minDrop, maxDrop, GetDrops(drops));
        }

        public static List<Enemy> GetEnemiesForLevel(int level)
        {
            List<Enemy> enemies = new List<Enemy>();

            // Scale factors for health, attack, and experience based on level
            float healthScalingFactor = 1 + (level - 1) * 0.05f; // 5% increase per level
            float attackScalingFactor = 1 + (level - 1) * 0.03f; // 3% increase per level
            float expScalingFactor = 1 + (level - 1) * 0.1f; // 10% increase in exp reward per level

            if (level < 10)
            {
                enemies.Add(CreateEnemy("Goblin", 30, 8, 50, 5, 10, new List<Drop>
            {
                new Drop("potion", 65),
                new Drop("bronze sword", 50),
                new Drop("bronze helmet", 40),
                new Drop("small mana potion", 30)
            }, healthScalingFactor, attackScalingFactor, expScalingFactor));

                enemies.Add(CreateEnemy("Rat", 20, 5, 30, 3, 8, new List<Drop>
            {
                new Drop("potion", 55),
                new Drop("bronze shield", 35),
                new Drop("small mana potion", 30)
            }, healthScalingFactor, attackScalingFactor, expScalingFactor));
            }
            else if (level < 15)
            {
                enemies.Add(CreateEnemy("Skeleton", 50, 12, 75, 10, 20, new List<Drop>
            {
                new Drop("iron sword", 40),
                new Drop("iron armor", 35),
                new Drop("potion", 60),
                new Drop("small mana potion", 30)
            }, healthScalingFactor, attackScalingFactor, expScalingFactor));

                enemies.Add(CreateEnemy("Orc", 65, 15, 90, 15, 25, new List<Drop>
            {
                new Drop("iron shield", 35),
                new Drop("iron helmet", 30),
                new Drop("small mana potion", 30)
            }, healthScalingFactor, attackScalingFactor, expScalingFactor));
            }
            else if (level < 20)
            {
                enemies.Add(CreateEnemy("Dark Knight", 100, 20, 150, 30, 50, new List<Drop>
            {
                new Drop("steel armor", 40),
                new Drop("steel sword", 30),
                new Drop("steel shield", 30),
                new Drop("large mana potion", 20)
            }, healthScalingFactor, attackScalingFactor, expScalingFactor));

                enemies.Add(CreateEnemy("Fire Elemental", 85, 25, 170, 25, 40, new List<Drop>
            {
                new Drop("steel helmet", 35),
                new Drop("silver sword", 30),
                new Drop("large mana potion", 20)
            }, healthScalingFactor, attackScalingFactor, expScalingFactor));

                enemies.Add(CreateEnemy("Troll", 90, 22, 160, 20, 35, new List<Drop>
            {
                new Drop("steel armor", 40),
                new Drop("steel sword", 30),
                new Drop("elixir", 40)
            }, healthScalingFactor, attackScalingFactor, expScalingFactor));
            }
            else if (level < 25)
            {
                enemies.Add(CreateEnemy("Vampire Lord", 200, 50, 300, 70, 120, new List<Drop>
            {
                new Drop("silver sword", 50),
                new Drop("silver shield", 40),
                new Drop("silver armor", 35),
                new Drop("large mana potion", 20)
            }, healthScalingFactor, attackScalingFactor, expScalingFactor));

                enemies.Add(CreateEnemy("Inferno Dragon", 250, 60, 400, 100, 200, new List<Drop>
            {
                new Drop("silver armor", 45),
                new Drop("gold sword", 35),
                new Drop("elixir", 40)
            }, healthScalingFactor, attackScalingFactor, expScalingFactor));

                enemies.Add(CreateEnemy("Necromancer", 180, 45, 350, 80, 150, new List<Drop>
            {
                new Drop("silver armor", 45),
                new Drop("gold shield", 35),
                new Drop("large mana potion", 20)
            }, healthScalingFactor, attackScalingFactor, expScalingFactor));
            }
            else if (level < 30)
            {
                enemies.Add(CreateEnemy("Dragon", 150, 30, 200, 50, 100, new List<Drop>
            {
                new Drop("gold sword", 50),
                new Drop("gold armor", 40),
                new Drop("gold shield", 35)
            }, healthScalingFactor, attackScalingFactor, expScalingFactor));

                enemies.Add(CreateEnemy("Lich", 130, 35, 220, 60, 120, new List<Drop>
            {
                new Drop("gold helmet", 40),
                new Drop("mithril sword", 35),
                new Drop("large mana potion", 20)
            }, healthScalingFactor, attackScalingFactor, expScalingFactor));

                enemies.Add(CreateEnemy("Witch", 120, 30, 180, 45, 75, new List<Drop>
            {
                new Drop("gold armor", 45),
                new Drop("elixir", 40)
            }, healthScalingFactor, attackScalingFactor, expScalingFactor));
            }
            else if (level < 35)
            {
                enemies.Add(CreateEnemy("Mithril Golem", 200, 50, 300, 100, 200, new List<Drop>
            {
                new Drop("mithril sword", 40),
                new Drop("mithril armor", 35),
                new Drop("mithril shield", 30),
                new Drop("elixir", 40)
            }, healthScalingFactor, attackScalingFactor, expScalingFactor));

                enemies.Add(CreateEnemy("Fire Dragon", 250, 60, 400, 150, 250, new List<Drop>
            {
                new Drop("mithril sword", 35),
                new Drop("mithril armor", 30),
                new Drop("large mana potion", 20)
            }, healthScalingFactor, attackScalingFactor, expScalingFactor));
            }
            else if (level < 40)
            {
                enemies.Add(CreateEnemy("Adamantite Dragon", 300, 80, 500, 200, 350, new List<Drop>
            {
                new Drop("adamantite sword", 40),
                new Drop("adamantite armor", 35),
                new Drop("adamantite shield", 30),
                new Drop("elixir", 40)
            }, healthScalingFactor, attackScalingFactor, expScalingFactor));

                enemies.Add(CreateEnemy("Shadow King", 350, 90, 600, 250, 400, new List<Drop>
            {
                new Drop("adamantite sword", 40),
                new Drop("adamantite armor", 35),
                new Drop("adamantite shield", 30),
                new Drop("large mana potion", 20)
            }, healthScalingFactor, attackScalingFactor, expScalingFactor));
            }
            else
            {
                enemies.Add(CreateEnemy("Ancient Dragon", 350, 100, 700, 300, 500, new List<Drop>
            {
                new Drop("platinum sword", 40),
                new Drop("platinum armor", 35),
                new Drop("platinum shield", 30)
            }, healthScalingFactor, attackScalingFactor, expScalingFactor));

                enemies.Add(CreateEnemy("Elder Lich", 400, 120, 800, 350, 600, new List<Drop>
            {
                new Drop("platinum sword", 45),
                new Drop("platinum armor", 40),
                new Drop("platinum shield", 35)
            }, healthScalingFactor, attackScalingFactor, expScalingFactor));
            }

            return enemies;
        }

        private static readonly Random _random = new Random();
        private static List<Drop> GetDrops(List<Drop> possibleDrops)
        {
            List<Drop> drops = new List<Drop>();

            // Define a multiplier to increase all chances (e.g., 1.5x the original chance)
            float chanceMultiplier = 3f;

            foreach (var drop in possibleDrops)
            {
                // Increase the chance by the multiplier
                int adjustedChance = (int)(drop.Chance * chanceMultiplier);

                // Make sure the chance doesn't exceed 100
                adjustedChance = Math.Min(adjustedChance, 100);

                // Roll for each item drop
                if (_random.Next(100) < adjustedChance)
                {
                    drops.Add(drop); // Add to the loot if it passes the adjusted chance
                }
            }

            return drops;
        }

    }

}

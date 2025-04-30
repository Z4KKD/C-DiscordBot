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
                new List<Drop>(Drops)
            );
        }

        public int GetCoinDrop()
        {
            Random rand = new Random();
            return rand.Next(CoinDropMin, CoinDropMax + 1);
        }

        public Drop? GetLootDrop()
        {
            Random rand = new Random();

            List<Drop> possibleDrops = new List<Drop>();

            foreach (var drop in Drops)
            {
                if (rand.Next(1, 101) <= drop.Chance)
                {
                    possibleDrops.Add(drop);
                }
            }

            if (possibleDrops.Count > 0)
            {
                return possibleDrops[rand.Next(possibleDrops.Count)];
            }

            return null;
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
        private static readonly Random _random = new Random();

        private static Enemy CreateEnemy(string name, int baseHealth, int baseAttack, int baseExp, int minDrop, int maxDrop, List<Drop> drops, float healthScalingFactor, float attackScalingFactor, float expScalingFactor)
        {
            int scaledHealth = (int)(baseHealth * healthScalingFactor);
            int scaledAttack = (int)(baseAttack * attackScalingFactor);
            int scaledExp = (int)(baseExp * expScalingFactor);

            return new Enemy(name, scaledHealth, scaledAttack, scaledExp, minDrop, maxDrop, GetDrops(drops));
        }

        public static List<Enemy> GetEnemiesForLevel(int level)
        {
            List<Enemy> enemies = new List<Enemy>();

            float healthScalingFactor = 1 + (level - 1) * 0.05f;
            float attackScalingFactor = 1 + (level - 1) * 0.05f;
            float expScalingFactor = 1 + (level - 1) * 0.1f;

            if (level < 10)
            {
                enemies.Add(CreateEnemy("Goblin", 30, 8, 50, 5, 10, new List<Drop>
                {
                    new Drop("potion", 90),
                    new Drop("bronze sword", 80),
                    new Drop("bronze helmet", 70),
                    new Drop("small mana potion", 60)
                }, healthScalingFactor, attackScalingFactor, expScalingFactor));

                enemies.Add(CreateEnemy("Rat", 20, 5, 30, 3, 8, new List<Drop>
                {
                    new Drop("potion", 85),
                    new Drop("bronze shield", 75),
                    new Drop("small mana potion", 65)
                }, healthScalingFactor, attackScalingFactor, expScalingFactor));
            }
            else if (level < 15)
            {
                enemies.Add(CreateEnemy("Skeleton", 50, 12, 75, 10, 20, new List<Drop>
                {
                    new Drop("iron sword", 75),
                    new Drop("iron armor", 70),
                    new Drop("potion", 85),
                    new Drop("small mana potion", 65)
                }, healthScalingFactor, attackScalingFactor, expScalingFactor));

                enemies.Add(CreateEnemy("Orc", 65, 15, 90, 15, 25, new List<Drop>
                {
                    new Drop("iron shield", 70),
                    new Drop("iron helmet", 65),
                    new Drop("small mana potion", 60)
                }, healthScalingFactor, attackScalingFactor, expScalingFactor));
            }
            else if (level < 20)
            {
                enemies.Add(CreateEnemy("Dark Knight", 100, 20, 150, 30, 50, new List<Drop>
                {
                    new Drop("steel armor", 75),
                    new Drop("steel sword", 70),
                    new Drop("steel shield", 65),
                    new Drop("large mana potion", 60)
                }, healthScalingFactor, attackScalingFactor, expScalingFactor));

                enemies.Add(CreateEnemy("Fire Elemental", 85, 25, 170, 25, 40, new List<Drop>
                {
                    new Drop("steel helmet", 70),
                    new Drop("silver sword", 65),
                    new Drop("large mana potion", 60)
                }, healthScalingFactor, attackScalingFactor, expScalingFactor));
            }
            else if (level < 25)
            {
                enemies.Add(CreateEnemy("Vampire Lord", 200, 50, 300, 70, 120, new List<Drop>
                {
                    new Drop("silver sword", 80),
                    new Drop("silver shield", 75),
                    new Drop("silver armor", 70),
                    new Drop("large mana potion", 65)
                }, healthScalingFactor, attackScalingFactor, expScalingFactor));

                enemies.Add(CreateEnemy("Inferno Dragon", 250, 60, 400, 100, 200, new List<Drop>
                {
                    new Drop("silver armor", 75),
                    new Drop("gold sword", 70),
                    new Drop("elixir", 65)
                }, healthScalingFactor, attackScalingFactor, expScalingFactor));
            }
            else if (level < 30)
            {
                enemies.Add(CreateEnemy("Dragon", 150, 30, 200, 50, 100, new List<Drop>
                {
                    new Drop("gold sword", 80),
                    new Drop("gold armor", 75),
                    new Drop("gold shield", 70)
                }, healthScalingFactor, attackScalingFactor, expScalingFactor));

                enemies.Add(CreateEnemy("Lich", 130, 35, 220, 60, 120, new List<Drop>
                {
                    new Drop("gold helmet", 75),
                    new Drop("mithril sword", 70),
                    new Drop("large mana potion", 65)
                }, healthScalingFactor, attackScalingFactor, expScalingFactor));
            }
            else if (level < 35)
            {
                enemies.Add(CreateEnemy("Mithril Golem", 200, 50, 300, 100, 200, new List<Drop>
                {
                    new Drop("mithril sword", 80),
                    new Drop("mithril armor", 75),
                    new Drop("mithril shield", 70),
                    new Drop("elixir", 70)
                }, healthScalingFactor, attackScalingFactor, expScalingFactor));

                enemies.Add(CreateEnemy("Fire Dragon", 250, 60, 400, 150, 250, new List<Drop>
                {
                    new Drop("mithril sword", 75),
                    new Drop("mithril armor", 70),
                    new Drop("large mana potion", 65)
                }, healthScalingFactor, attackScalingFactor, expScalingFactor));
            }
            else if (level < 40)
            {
                enemies.Add(CreateEnemy("Adamantite Dragon", 300, 80, 500, 200, 350, new List<Drop>
                {
                    new Drop("adamantite sword", 80),
                    new Drop("adamantite armor", 75),
                    new Drop("adamantite shield", 70),
                    new Drop("elixir", 70)
                }, healthScalingFactor, attackScalingFactor, expScalingFactor));

                enemies.Add(CreateEnemy("Shadow King", 350, 90, 600, 250, 400, new List<Drop>
                {
                    new Drop("adamantite sword", 80),
                    new Drop("adamantite armor", 75),
                    new Drop("adamantite shield", 70),
                    new Drop("large mana potion", 65)
                }, healthScalingFactor, attackScalingFactor, expScalingFactor));
            }
            else if (level < 45)
            {
                enemies.Add(CreateEnemy("Ancient Dragon", 350, 85, 500, 250, 400, new List<Drop>
            {
                new Drop("platinum sword", 80),
                new Drop("platinum armor", 75),
                new Drop("platinum shield", 70)
            }, healthScalingFactor, attackScalingFactor, expScalingFactor));

                        enemies.Add(CreateEnemy("Elder Lich", 375, 95, 550, 275, 420, new List<Drop>
            {
                new Drop("platinum sword", 80),
                new Drop("platinum armor", 75),
                new Drop("platinum shield", 70)
            }, healthScalingFactor, attackScalingFactor, expScalingFactor));
            }

            return enemies;
        }

        private static List<Drop> GetDrops(List<Drop> possibleDrops)
        {
            List<Drop> drops = new List<Drop>();

            foreach (var drop in possibleDrops)
            {
                if (_random.Next(100) < drop.Chance)
                {
                    drops.Add(drop);
                }
            }

            return drops;
        }
    }
}

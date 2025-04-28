using System;
using System.Collections.Generic;

namespace BBtbChallenger.GameLogic
{
    public class Enemy
    {
        public string Name { get; set; }
        public int Health { get; set; }
        public int Attack { get; set; }
        public int ExperienceReward { get; set; }
        public int CoinDropMin { get; set; }
        public int CoinDropMax { get; set; }
        public List<Drop> Drops { get; set; } = new List<Drop>();
        public bool IsStunned { get; set; } = false; 


        public bool IsAlive => Health > 0;

        public Enemy(string name, int health, int attack, int expReward, int coinDropMin, int coinDropMax, List<Drop>? drops = null)
        {
            Name = name;
            Health = health;
            Attack = attack;
            ExperienceReward = expReward;
            CoinDropMin = coinDropMin;
            CoinDropMax = coinDropMax;
            Drops = drops ?? new List<Drop>();
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
        public static List<Enemy> GetEnemiesForLevel(int level)
        {
            List<Enemy> enemies = new List<Enemy>();

            if (level < 10)
            {
                enemies.Add(new Enemy("Goblin", 30, 8, 50, 5, 10, new List<Drop>
            {
                new Drop("potion", 50), 
                new Drop("bronze sword", 30),  
                new Drop("bronze helmet", 20)  
            }));
                enemies.Add(new Enemy("Rat", 20, 5, 30, 3, 8, new List<Drop>
            {
                new Drop("potion", 35),  
                new Drop("bronze shield", 10) 
            }));
            }

            else if (level < 15)
            {
                enemies.Add(new Enemy("Skeleton", 50, 12, 75, 10, 20, new List<Drop>
            {
                new Drop("iron sword", 25),
                new Drop("iron armor", 20),  
                new Drop("potion", 40)  
            }));
                enemies.Add(new Enemy("Orc", 65, 15, 90, 15, 25, new List<Drop>
            {
                new Drop("iron shield", 20), 
                new Drop("iron helmet", 10)  
            }));
            }
  
            else if (level < 20)
            {
                enemies.Add(new Enemy("Dark Knight", 100, 20, 150, 30, 50, new List<Drop>
            {
                new Drop("steel armor", 20),  
                new Drop("steel sword", 10), 
                new Drop("steel shield", 10)  
            }));
                enemies.Add(new Enemy("Fire Elemental", 85, 25, 170, 25, 40, new List<Drop>
            {
                new Drop("steel helmet", 15),  
                new Drop("silver sword", 10)  
            }));
                enemies.Add(new Enemy("Troll", 90, 22, 160, 20, 35, new List<Drop>
            {
                new Drop("steel armor", 20),  
                new Drop("steel sword", 10) 
            }));
            }

            else if (level < 25)
            {
                enemies.Add(new Enemy("Vampire Lord", 200, 50, 300, 70, 120, new List<Drop>
            {
                new Drop("silver sword", 30), 
                new Drop("silver shield", 20), 
                new Drop("silver armor", 15)  
            }));
                enemies.Add(new Enemy("Inferno Dragon", 250, 60, 400, 100, 200, new List<Drop>
            {
                new Drop("silver armor", 25), 
                new Drop("gold sword", 15)  
            }));
                enemies.Add(new Enemy("Necromancer", 180, 45, 350, 80, 150, new List<Drop>
            {
                new Drop("silver armor", 25), 
                new Drop("gold shield", 15) 
            }));
            }

            else if (level < 30)
            {
                enemies.Add(new Enemy("Dragon", 150, 30, 200, 50, 100, new List<Drop>
            {
                new Drop("gold sword", 30),  
                new Drop("gold armor", 15),  
                new Drop("gold shield", 10)  
            }));
                enemies.Add(new Enemy("Lich", 130, 35, 220, 60, 120, new List<Drop>
            {
                new Drop("gold helmet", 20), 
                new Drop("mithril sword", 15) 
            }));
                enemies.Add(new Enemy("Witch", 120, 30, 180, 45, 75, new List<Drop>
            {
                new Drop("gold armor", 25),  
                new Drop("elixir", 20)  
            }));
            }

            else if (level < 35)
            {
                enemies.Add(new Enemy("Mithril Golem", 200, 50, 300, 100, 200, new List<Drop>
            {
                new Drop("mithril sword", 25),  
                new Drop("mithril armor", 15),  
                new Drop("mithril shield", 10)  
            }));
                enemies.Add(new Enemy("Fire Dragon", 250, 60, 400, 150, 250, new List<Drop>
            {
                new Drop("mithril sword", 20),  
                new Drop("mithril armor", 10)  
            }));
            }

            else if (level < 40)
            {
                enemies.Add(new Enemy("Adamantite Dragon", 300, 80, 500, 200, 350, new List<Drop>
            {
                new Drop("adamantite sword", 25),  
                new Drop("adamantite armor", 20),  
                new Drop("adamantite shield", 15)  
            }));
                enemies.Add(new Enemy("Shadow King", 350, 90, 600, 250, 400, new List<Drop>
            {
                new Drop("adamantite sword", 25),  
                new Drop("adamantite armor", 20),  
                new Drop("adamantite shield", 15)  
            }));
            }

            else
            {
                enemies.Add(new Enemy("Ancient Dragon", 350, 100, 700, 300, 500, new List<Drop>
            {
                new Drop("platinum sword", 25),  
                new Drop("platinum armor", 20),  
                new Drop("platinum shield", 15)  
            }));
                enemies.Add(new Enemy("Elder Lich", 400, 120, 800, 350, 600, new List<Drop>
            {
                new Drop("platinum sword", 30),  
                new Drop("platinum armor", 25),  
                new Drop("platinum shield", 20)  
            }));
            }

            return enemies;
        }
    }

}

using BBtbChallenger.Data;
using BBtbChallenger.GameLogic;
using System.Text;

public class ArenaManager
{
    private RpgCharacter _character;
    private List<Enemy> _enemies;
    private Random _random;
    private int _totalExperience;
    private int _totalCoins;
    private int _rewardMultiplier;
    private int _difficultyScaling; // increases enemy power each round
    private int _enemiesDefeatedInArena;
    private Action<ulong> _removePlayerFromArena;


    public ArenaManager(RpgCharacter character, List<Enemy> enemies, Action<ulong> removePlayerFromArena)
    {
        _character = character;
        _enemies = enemies;
        _random = new Random();
        _totalExperience = 0;
        _totalCoins = 0;
        _rewardMultiplier = 1;
        _difficultyScaling = 0;
        _enemiesDefeatedInArena = 0;
        _removePlayerFromArena = removePlayerFromArena;

    }

    public async Task<string> StartArena()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Welcome to the Arena, {_character.Name}!");
        sb.AppendLine($"Prepare to face 5 enemies.");
        sb.AppendLine($"**Starting Stats**: Health: {_character.Health}/{_character.MaxHealth}, Mana: {_character.Mana}/{_character.MaxMana}");
        sb.AppendLine($"**Reward Multiplier**: x{_rewardMultiplier}");

        int totalEnemiesDefeated = 0;

        // Do not reset health here! We should use current health
        //int originalHealth = _character.Health; // Remove this
        //int originalMana = _character.Mana;   // Remove this

        for (int i = 0; i < 5; i++)
        {
            var enemy = GenerateScaledEnemy();
            var result = await FightEnemy(enemy);

            if (!result.Item1)
            {
                sb.AppendLine("You were defeated in the arena...");
                sb.AppendLine("You lost all rewards. Better luck next time!");
                _removePlayerFromArena(_character.UserId);
                _character.Die();
                return sb.ToString();
            }

            totalEnemiesDefeated++;
            _enemiesDefeatedInArena++;
            _totalExperience += enemy.ExperienceReward;
            _totalCoins += enemy.GetCoinDrop();
        }

        sb.AppendLine($"--- Fight Summary ---");
        sb.AppendLine($"Total Enemies Defeated: {totalEnemiesDefeated}");
        sb.AppendLine($"Total XP Earned: {_totalExperience}, Total Coins Earned: {_totalCoins}");
        sb.AppendLine($"Reward Multiplier: x{_rewardMultiplier}");
        sb.AppendLine();
        sb.AppendLine($"**Current Stats**: Health: {_character.Health}/{_character.MaxHealth}, Mana: {_character.Mana}/{_character.MaxMana}");
        sb.AppendLine("You completed all 5 fights!");
        sb.AppendLine("Do you want to leave with your rewards or continue for bigger rewards?");

        return sb.ToString();
    }



    public async Task<string> ContinueArena()
    {
        // Check if the player is still alive
        if (!_character.IsAlive)
        {
            return "You cannot continue because you were defeated in the arena. You lost all rewards. Better luck next time!";
        }

        _difficultyScaling += 1;

        var sb = new StringBuilder();

        // Check if enough enemies defeated to increase reward multiplier
        if (_enemiesDefeatedInArena % 15 == 0 && _enemiesDefeatedInArena != 0)
        {
            _rewardMultiplier *= 1;
            sb.AppendLine($"🎉 **Bonus Unlocked!** Your reward multiplier has doubled to x{_rewardMultiplier}!");
        }
        else
        {
            sb.AppendLine($"You chose to continue! Your reward multiplier is still x{_rewardMultiplier}.");
        }

        sb.AppendLine($"Enemies are now stronger!");
        sb.AppendLine($"**Starting Stats**: Health: {_character.Health}/{_character.MaxHealth}, Mana: {_character.Mana}/{_character.MaxMana}");
        sb.AppendLine($"**Reward Multiplier**: x{_rewardMultiplier}");

        int totalEnemiesDefeated = 0;

        for (int i = 0; i < 5; i++)
        {
            var enemy = GenerateScaledEnemy();
            var result = await FightEnemy(enemy);

            if (!result.Item1)
            {
                sb.AppendLine("You were defeated in the arena...");
                sb.AppendLine("You lost all rewards. Better luck next time!");
                _removePlayerFromArena(_character.UserId);
                _character.Die();
                return sb.ToString();
            }

            totalEnemiesDefeated++;
            _enemiesDefeatedInArena++;
            _totalExperience += enemy.ExperienceReward;
            _totalCoins += enemy.GetCoinDrop();
        }

        sb.AppendLine($"--- Fight Summary ---");
        sb.AppendLine($"Total Enemies Defeated This Round: {totalEnemiesDefeated}");
        sb.AppendLine($"Total XP Earned: {_totalExperience}, Total Coins Earned: {_totalCoins}");
        sb.AppendLine($"Reward Multiplier: x{_rewardMultiplier}");
        sb.AppendLine();
        sb.AppendLine($"**Current Stats**: Health: {_character.Health}/{_character.MaxHealth}, Mana: {_character.Mana}/{_character.MaxMana}");
        sb.AppendLine("You completed another 5 fights!");
        sb.AppendLine("Do you want to leave with your rewards or continue for even bigger rewards?");

        return sb.ToString();
    }


    private async Task<Tuple<bool, string>> FightEnemy(Enemy enemy)
    {
        while (_character.IsAlive && enemy.IsAlive)
        {
            // Attack the enemy
            enemy.Health -= _character.GetAttackDamage();
            if (enemy.IsAlive)
            {
                // Calculate enemy damage and apply to the character
                int enemyDamage = Math.Max(1, enemy.Attack - (int)(0.5 * _character.Defense)); // Ensure there's at least 1 damage
                _character.Health -= enemyDamage;

                // Add debug logs for health reduction
                Console.WriteLine($"Enemy dealt {enemyDamage} damage to {_character.Name}. Current Health: {_character.Health}");

                // Check if character is still alive
                if (_character.Health <= 0)
                {
                    Console.WriteLine($"{_character.Name} has been defeated!");
                    _character.Die();
                    SaveManager.SaveCharacter(_character.UserId, _character);

                    // Automatically leave the arena when defeated
                    LeaveArena();  // Ensure the player is removed from the arena when defeated

                    return new Tuple<bool, string>(false, "You lost. You lost all rewards. Better luck next time!");
                }
            }
        }

        // If the enemy is defeated
        return _character.IsAlive
            ? new Tuple<bool, string>(true, "You won!")
            : new Tuple<bool, string>(false, "You lost.");
    }





    private Enemy GenerateScaledEnemy()
    {
        var baseEnemy = _enemies[_random.Next(_enemies.Count)];
        var scaledEnemy = baseEnemy.Clone();

        scaledEnemy.MaxHealth += 10 * _difficultyScaling;
        scaledEnemy.Health = scaledEnemy.MaxHealth; 
        scaledEnemy.Attack += 2 * _difficultyScaling;
        scaledEnemy.ExperienceReward += 5 * _difficultyScaling;
        scaledEnemy.CoinDropMin += 2 * _difficultyScaling;
        scaledEnemy.CoinDropMax += 4 * _difficultyScaling;

        return scaledEnemy;
    }


    public void LeaveArena()
    {
        // Finalize and apply the rewards when leaving
        // Only apply rewards if the player is still alive (i.e., not defeated)
        if (_character.IsAlive)
        {
            _character.Coins += (_totalCoins * _rewardMultiplier);
            _character.GainExperience(_totalExperience);
        }
        else
        {
            // Player lost, don't give any rewards
            Console.WriteLine($"{_character.Name} lost, no rewards will be given.");
        }

        // Save character data
        SaveManager.SaveCharacter(_character.UserId, _character);
    }

}

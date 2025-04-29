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
        return await StartOrContinueArena("Welcome to the Arena, {_character.Name}!", false);
    }

    public async Task<string> ContinueArena()
    {
        return await StartOrContinueArena($"You chose to continue!", true);
    }

    private async Task<string> StartOrContinueArena(string introMessage, bool isContinuing)
    {
        var sb = new StringBuilder();
        sb.AppendLine(introMessage);
        sb.AppendLine($"**Starting Stats**: Health: {_character.Health}/{_character.MaxHealth}, Mana: {_character.Mana}/{_character.MaxMana}");

        int totalEnemiesDefeated = 0;

        if (isContinuing)
        {
            _difficultyScaling += 1;  // Increase difficulty
        }

        if (_enemiesDefeatedInArena % 15 == 0 && _enemiesDefeatedInArena != 0)
        {
            _rewardMultiplier += 1;
            sb.AppendLine($"🎉 **Bonus Unlocked!** Your reward multiplier is now x{_rewardMultiplier}!");
        }

        for (int i = 0; i < 5; i++)
        {
            var enemy = GenerateScaledEnemy();
            var result = await FightEnemy(enemy);

            if (!result.IsSuccessful)
            {
                sb.AppendLine(result.Message);
                ResetArena();
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
        sb.AppendLine($"**Current Stats**: Health: {_character.Health}/{_character.MaxHealth}, Mana: {_character.Mana}/{_character.MaxMana}");
        sb.AppendLine("You completed all 5 fights!");
        sb.AppendLine("Do you want to leave with your rewards or continue for bigger rewards?");

        return sb.ToString();
    }

    private async Task<BattleResult> FightEnemy(Enemy enemy)
    {
        while (_character.IsAlive && enemy.IsAlive)
        {
            enemy.Health -= _character.GetAttackDamage();
            if (enemy.IsAlive)
            {
                int enemyDamage = Math.Max(1, enemy.Attack - (int)(0.5 * _character.Defense));
                _character.Health -= enemyDamage;
                if (_character.Health <= 0)
                {
                    return new BattleResult(false, "You lost. You lost all rewards. Better luck next time!");
                }
            }
        }

        return new BattleResult(true, "You won!");
    }

    private void ResetArena()
    {
        _removePlayerFromArena(_character.UserId);
        _character.Die();
        _totalCoins = 0;
        _totalExperience = 0;
    }

    private Enemy GenerateScaledEnemy()
    {
        var baseEnemy = _enemies[_random.Next(_enemies.Count)];
        var scaledEnemy = baseEnemy.Clone();

        scaledEnemy.MaxHealth += 10 * _difficultyScaling;
        scaledEnemy.Health = scaledEnemy.MaxHealth;
        scaledEnemy.Attack += 2 * _difficultyScaling;
        scaledEnemy.ExperienceReward += 5;
        scaledEnemy.CoinDropMin += 2;
        scaledEnemy.CoinDropMax += 4;

        return scaledEnemy;
    }

    public void LeaveArena()
    {
        if (_character.IsAlive)
        {
            _character.Coins += (_totalCoins * _rewardMultiplier);
            _character.GainExperience(_totalExperience);
        }
        SaveManager.SaveCharacter(_character.UserId, _character);
    }
}

public class BattleResult
{
    public bool IsSuccessful { get; }
    public string Message { get; }

    public BattleResult(bool isSuccessful, string message)
    {
        IsSuccessful = isSuccessful;
        Message = message;
    }
}

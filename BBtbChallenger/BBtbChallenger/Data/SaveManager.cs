using BBtbChallenger.GameLogic;
using System.Text.Json;

namespace BBtbChallenger.Data;

public static class SaveManager
{
    private static readonly string SaveDirectory = Path.Combine(AppContext.BaseDirectory, "Saves");

    static SaveManager()
    {
        Directory.CreateDirectory(SaveDirectory);
    }

    public static void SaveCharacter(ulong userId, RpgCharacter character)
    {
        var json = JsonSerializer.Serialize(character, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(GetPath(userId), json);
    }

    public static RpgCharacter? LoadCharacter(ulong userId)
    {
        var path = GetPath(userId);
        if (!File.Exists(path)) return null;

        var json = File.ReadAllText(path);
        var character = JsonSerializer.Deserialize<RpgCharacter>(json);
        if (character != null) character.UserId = userId;
        return character;
    }

    private static string GetPath(ulong userId) => Path.Combine(SaveDirectory, $"{userId}.json");
}

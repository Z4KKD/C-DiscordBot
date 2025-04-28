using BBtbChallenger.GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BBtbChallenger.Data
{
    public static class SaveManager
    {
        private static readonly string SaveDirectory = Path.Combine(AppContext.BaseDirectory, "Saves");

        static SaveManager()
        {
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }
        }

        public static void SaveCharacter(ulong userId, RpgCharacter character)
        {
            string path = GetPath(userId);
            var json = JsonSerializer.Serialize(character, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        public static RpgCharacter LoadCharacter(ulong userId)
        {
            string path = GetPath(userId);
            if (!File.Exists(path)) return null!;

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<RpgCharacter>(json)!;
        }

        private static string GetPath(ulong userId)
        {
            return Path.Combine(SaveDirectory, $"{userId}.json");
        }
    }
}

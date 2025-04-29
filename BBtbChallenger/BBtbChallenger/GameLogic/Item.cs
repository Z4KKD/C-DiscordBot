namespace BBtbChallenger.GameLogic;

public class Item
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public Action<RpgCharacter>? UseEffect { get; set; }

    public void Use(RpgCharacter character) => UseEffect?.Invoke(character);
}

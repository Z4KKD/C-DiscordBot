# Detailed RPG Combat System

This project is a simple RPG combat system for Discord bots, featuring character progression, inventory management, battles, and arena challenges. The system includes various commands for interacting with the game world, managing inventory, engaging in combat, and more.

## Features

- **Character Progression**: Players can start their adventure, view stats, and manage their inventory.
- **Combat System**: Engage in random battles with enemies.
- **Arena**: Fight in an arena to test your strength and claim rewards.
- **Item Management**: Equip, sell, and use items like weapons, armor, potions, and elixirs.
- **Fishing & Shop**: Catch fish for rewards and buy/sell items in the shop.

## Commands

### General Commands

- `!help`: Displays the command guide for the RPG system.
- `!start`: Begin your adventure or revive your character.
- `!stats`: View your character's stats (e.g., health, mana, level).
- `!bag`: Check your inventory for items you have.
  
### Combat & Battle Commands

- `!fight`: Fight a random enemy.
- `!a [a|d|m]`: Perform an action in battle: `a` for attack, `d` for defend, `m` for magic.
- `!use [item]`: Use an item from your inventory during battle (e.g., healing potions or mana potions).

### Inventory & Equipment Management

- `!equip [sword|shield|armor|helmet]`: Equip your best available item automatically from your inventory based on predefined rankings.
- `!sell [item]`: Sell an item from your inventory and receive coins.

### Arena Commands

- `!arena`: Start an arena challenge with enemies suitable for your level.
- `!continue`: Continue an ongoing arena battle.
- `!leave`: Leave the arena and claim your rewards.

### Fishing & Shop

- `!fish`: Begin a fishing session to catch small rewards.
- `!shop`: View available items to buy from the shop.
- `!buy [item name]`: Purchase an item from the shop.
- `!sell [item name]`: Sell an item from your inventory to the shop.

## Item System

Items in the game are categorized by type (e.g., sword, shield, armor, helmet) and ranked by power. The `!equip` command will automatically equip the best available item from your inventory based on its power rank.

### Item Types and Power Rankings

- **Weapons**: Swords, with ranks ranging from `bronze sword` to `platinum sword`.
- **Armor**: Protective gear like armors and helmets, with ranks ranging from `iron armor` to `platinum armor`.
- **Shields**: Defenses to protect you during combat, ranging from `bronze shield` to `platinum shield`.

Each item has a predefined strength, and the `!equip` command will automatically select the best available option for each slot.

## Arena System

The arena feature allows players to battle stronger enemies. Players can enter the arena using the `!arena` command, where they'll face enemies based on their level. Players can also continue ongoing arena fights using `!continue` and leave the arena using `!leave`, claiming rewards for their victories.

### Arena Management

- Active arena sessions are tracked using a dictionary where the key is the player's user ID.
- Players can only have one arena session active at a time.

## Item Usage

Items like potions and elixirs can be used in battle or during normal gameplay to restore health or mana.

- **Potion**: Restores 30 HP.
- **Elixir**: Fully restores HP.
- **Mana Potion**: Restores 30 mana.
- **Mana Elixir**: Fully restores mana.

## Installation & Setup

### Requirements

- A Discord bot running on your server.
- The bot must have the necessary permissions to interact with users and send messages.

### Dependencies

- **Discord.Net**: The Discord library used for creating the bot.
- **C# 9.0** or later: Required for the language features.

### Setup

1. Clone the repository to your local machine.
2. Ensure that your bot has the required dependencies and is configured properly to interact with Discord.
3. Implement the necessary bot logic to load the RPG system into your Discord bot.
4. Use a persistent storage system to save character data, inventory, and arena status (this can be a simple JSON file or database).
5. Ensure that your bot has permission to send and receive messages in the channels it will interact with.


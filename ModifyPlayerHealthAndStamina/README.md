# Modify Player Health and Stamina for Valheim

Modify the base health points (HP) and Stamina. Set your HP to 1 or 1000.

This mod uses a transpiler approach to be compatible with other mods like Glutton that modify the same function.

Each patch is applied separately and only if it differs from the default, so you may enable one or both.

## Installation

1. Download and install BepInEx Valheim
2. Download this mod and move the `ModifyPlayerHealthAndStamina.dll` into `<GameLocation>\BepInEx\plugins`
3. Launching the game will generate a config file at `<GameLocation>\BepInEx\config`

## Configuration
Configuration may be done in game using: https://github.com/BepInEx/BepInEx.ConfigurationManager

Make sure your inventory is open before opening ConfigurationManager to display tooltips on options when you mouse over them.

You may also edit the config file manually.
```
[Base stats]

## Modify player base health points.
# Setting type: Single
# Default value: 25
Player Health = 25

## Modify player base stamina points.
# Setting type: Single
# Default value: 75
Player Stamina = 75

```

## Changelog
- v0.0.1
	- initial release
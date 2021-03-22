# Glutton Food Manager for Valheim
Automatically eat food from your inventory with a configurable keyboard shortcut to toggle this behavior. 

With the default settings, Glutton will eat the best food you have available in your inventory whenever a food buff begins to flash.

There are many options available to customize how Glutton behaves.

## Features
* Eat the same food when the remaining duration falls under a specified threshold
* Eat until maximum food buffs are applied based on available inventory
* Toggle auto eating on or off with customizable keyboard shortcut
* Auto eat best or worst food as priority
* Normalize the benefits so that you always get a static amount of hp or stamina
* Completely ignore inventory requirements to eat the best food in game
* Adjust the amount of foods that you can have active
* Prevent the food from being removed from inventory
* Adjust food score calculation to favor a particular stat

## Installation
1. Download and install BepInEx Valheim
2. Download this mod and move the `Glutton.dll` into `<GameLocation>\BepInEx\plugins`
3. Launching the game will generate a config file at `<GameLocation>\BepInEx\config`

## Default Keyboard shortcuts
Toggle Automatic Eating: LeftShift + T

## Configuration
Configuration may be done in game using: https://github.com/BepInEx/BepInEx.ConfigurationManager

Make sure your inventory is open before opening ConfigurationManager to display tooltips on options when you mouse over them.

You may also edit the config file manually.

**PLEASE NOTE:**
Options that require a game restart to take effect are marked with a double asterisk (**).
```
## Settings file was created by plugin Glutton v1.0.0
## Plugin GUID: org.tonsit.glutton

[General]

## Allow Glutton to eat food.
# Setting type: Boolean
# Default value: true
Auto Eat = true

## The percentage of the food duration at which to attempt to eat the same food. Range: 1-50
# Setting type: UInt32
# Default value: 50
# Acceptable value range: From 1 to 50
Refresh Food at Percentage = 50

## Whether Glutton should try to keep maximum food buffs applied.
# Setting type: Boolean
# Default value: true
Eat Maximum Foods = true

## When 'Eat Maximum Foods' is enabled, eat foods with best scores first. Set this to false to eat the lowest scored foods first.
# Setting type: Boolean
# Default value: true
Eat Best Foods First = true

## Serve any food available in the game, regardless of your inventory
# Setting type: Boolean
# Default value: false
Ignore Inventory = false

## How many foods can be active at one time. Note: The UI does not support more than 3 buffs, however the increased stamina and health will still be applied. Restart required.
# Setting type: UInt32
# Default value: 3
# Acceptable value range: From 0 to 25
**Maximum Food Count = 3

## When enabled food will provide a static buff rather than provide less benefit over time. Defaults to 50% of maximum food value (Average benefit over its duration). HP and Stamina benefits may be adjusted in advanced options. Restart Required
# Setting type: Boolean
# Default value: false
**Normalize Food Benefits = false

## Set the percentage for the food health. Defaults to 50% of the foods maximum benefit, which is the average of its duration. Setting this value to 100 makes food provide its maximum benefit. Setting this to 0 would make food provide no health benefit. Ignored when Normalize Food is disabled. Restart required.
# Setting type: UInt32
# Default value: 50
# Acceptable value range: From 0 to 100
**Food Health Benefit = 50

## Set the percentage for the food stamina. Defaults to 50% of the foods maximum benefit, which is the average of its duration. Setting this value to 100 makes food provide its maximum stamina benefit. Setting this to 0 would make food provide no stamina benefit. Ignored when Normalize Food is disabled. Restart required.
# Setting type: UInt32
# Default value: 50
# Acceptable value range: From 0 to 100
**Food Stamina Benefit = 50

## Set the duration for the food timer. This also impacts the rate at which the food benefits decay unless Normalize Food Benefits is enabled. Restart required. Shorter: 5%, Short: 50%, Normal: 100%, Long: 200%, Longer: 1000%
# Setting type: DurationTypes
# Default value: Normal
# Acceptable values: Infinite, Longer, Long, Normal, Short, Shorter
**Food Duration = Normal

## Should eating an item reduce the stack count in your inventory? IgnoreInventory overrides this to false.
# Setting type: Boolean
# Default value: true
Remove Eaten Item From Inventory = true

[Keyboard shortcuts]

## Keyboard shortcut to enable/disable Glutton
# Setting type: KeyboardShortcut
# Default value: T + LeftShift
Toggle Automatic Eating = T + LeftShift

[Weighted Food Score]

## The multiplier to use when calculating the Food Score.
# Setting type: Int32
# Default value: 50
# Acceptable value range: From 0 to 100
Health Score Weight = 50

## The multiplier to use when calculating the Food Score.
# Setting type: Int32
# Default value: 50
# Acceptable value range: From 0 to 100
Stamina Score Weight = 50

## The multiplier to use when calculating the Food Score.
# Setting type: Int32
# Default value: 50
# Acceptable value range: From 0 to 100
Burn Time Score Weight = 50

## The multiplier to use when calculating the Food Score.
# Setting type: Int32
# Default value: 50
# Acceptable value range: From 0 to 100
Regen Score Weight = 50
```
## Example config options

There is an option to override the health and stamina values to be static. When "Normalize Food Benefit" = true, it will provide 50% of the maximum benefit of food (The average over its duration).
The benefit % may be adjusted further in the advanced options.

**Give yourself 100% of the maximum health and stamina benefits and refresh at the end:**
```
Refresh Food at Percentage = 1
**Normalize Food Benefit = true
**Food Health Benefit = 100
**Food Stamina Benefit = 100
```

**Refresh foods only after you have manually eaten them:**
```
Auto Eat = true
Refresh Food at Percentage = 50
Eat Maximum Foods = false
```

**Be a Glutton - Consume the best food in game**
```
Auto Eat = true
Refresh Food At Percentage = 50
Ignore Inventory = true
**Maximum Food Count = 22
```

## Changelog
- v1.0.3
    - Update description in food duration to include percentages
	- Add verbosity level for logging to lower the default output
- v1.0.2
	- Fixed a bug where food would not refresh if the buff was already active when logged in
    - Fixed a bug where food would not refresh if the buff fell off in between update checks
- v1.0.1
	- Fixed a bug where pots were consumed from inventory
	- Fixed a bug where the keyboard shortcut was spamming other players in multiplayer
    - Prevent hotkey from triggering while various text boxes are open
- v1.0.0
	- Add keyboard shortcut to toggle mod behavior
	- Adjust Config for better BepInEx Configuration Manager support
	- Add throttling to UpdateFood patch to improve performance
	- Add option to adjust active food buff count beyond game limit of 3
	- Add option to consume best/worst food choice from inventory automatically
	- Add option to try and keep maximum food buffs active based on available inventory
	- Add option to ignore inventory and retrieve food from a magic kitchen
	- Add option to normalize food benefits
	- Add option to adjust buff duration
	- Add adjustable options for weighting food scores
- v0.0.3
	- Add toggle to remove the consumed item from stack count
- v0.0.2
	- Update readme
- v0.0.1
	- initial release


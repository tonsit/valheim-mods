# Glutton Plugin for Valheim
Manage your food!
Automatically eat the best food you have in your inventory.
Option to ignore inventory requirements completely

Features that may be configured:
*Consume the same food when the remaining duration falls under a specified threshhold
*Consume until 3 food buffs are applied based on available inventory
*Prevent the food from being removed from inventory
*Completely ignore inventory requirements to consume the best food in game


The threshhold to trigger food consumption is expressed as a whole number representing a percentage of the overall duration of the food.
The defaul is 50 which is when the icon begins to flash.

The amount of stamina and health provided by food is tied to its remaining duration. Consuming food at 50% of its duration will give you the maximum effects of food while consuming food relatively quick.
Lower this setting to conserve food.


The original version had a bug where the food would not be removed from your inventory. This has been turned into a feature.
To restore the behavior, set RemoveItemConsumedFromInventory = false

## Installation

1. Download and install BepInEx Valheim
2. Download this mod and move the `Glutton.dll` into `<GameLocation>\BepInEx\plugins`
3. Launching the game will generate a config file at `<GameLocation>\BepInEx\config`

## Configuration
```
[Glutton]

## The percentage of the food duration at which to attempt to consume the same food. Range: 1-50
# Setting type: UInt32
# Default value: 50
FoodDurationRemainingPercent = 50

## Whether Glutton should try to keep maximum food buffs applied.
# Setting type: Boolean
# Default value: true
Consume Maximum Foods = true

## How many foods can be active at one time?
# Setting type: Int32
# Default value: 3
MaximumFoodCount = 3

## Whether Glutton should consume the best food available in the game regardless of your inventory.
# Setting type: Boolean
# Default value: false
IgnoreInventory = true

## Whether Glutton should reduce the stack count in your inventory when consuming food. IgnoreInventory overrides this setting to false.
# Setting type: Boolean
# Default value: true
RemoveItemConsumedFromInventory = true

ConsumeMaximumFoods = true
```

## Changelog
- v0.1.0
	- Add throttling to UpdateFood patch to improve performance
	- Add option to adjust active food buff count
	- Add option to consume best food choice from inventory automatically
	- Add option to try and keep 3 food buffs active based on available inventory
	- Add option to ignore inventory when consuming food
- v0.0.3
	- Add toggle to remove the consumed item from stack count
- v0.0.2
	- Update readme
- v0.0.1
	- initial release


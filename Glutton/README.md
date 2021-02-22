# Glutton Plugin for Valheim
Automatically consume the same food when the remaining duration falls under a specified threshhold. Optionally reapplies the food buff without consuming an item.

The threshhold is expressed as a whole number representing a percentage of the overall duration of the food.

The default is 50 which is when the icon begins to flash.

The amount of stamina and health provided by food is tied to its duration. Consuming food at 50 will give you the maximum effects of food while consuming food relatively quick.

Lower this setting to conserve food.

The original version had a bug where the food would not be removed from your inventory. This has been turned into a feature.

To restore the behavior, set RemoveItemConsumedFromInventory = false

## Configuration
```
[Glutton]

## The percentage of the food duration at which to attempt to consume the same food. Range: 1-50
# Setting type: UInt32
# Default value: 50
FoodDurationRemainingPercent = 50

## Whether the food consumed by Glutton should reduce the stack count in your inventory.
# Setting type: Boolean
# Default value: true
RemoveItemConsumedFromInventory = true
```

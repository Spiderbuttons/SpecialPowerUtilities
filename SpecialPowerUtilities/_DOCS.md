# CONTENT PATCHER AUTHOR DOCUMENTATION

## Special Items & Powers Tabs
If your mod does not add support specifically for Special Power Utilities, your mod will still function as normal for your users. Special Power Utilities does not alter the unlocked conditions of your powers in any way. However, you are able to designate a tab for your mod if you so choose and assign it a custom icon that Special Power Utilities will utilize. If the IDs of your special items & powers follow the standard naming convention (i.e. they are prefixed with your mod ID, such as `Author.ModName_CoolPower`) then Special Power Utilities will create a tab for your mod automatically, but you will not get to choose the icon or how the name is displayed without specifying it on your end.

If you _would_ like to specify which tab your modded powers are placed in and what the icon should be, the following example showcases how this is done:

```jsonc
{
    "Action": "Load",
    "FromFile": "assets/TabIcon.png", // This can be any image so long as it is square. 16x16, 32x32, 64x64... etc. I wouldn't go overboard, though...
    "Target": "Mods/{{ModId}}/TabIcon"  
},

{  
    "Action": "EditData",  
    "Target": "Spiderbuttons.SpecialPowerUtilities/PowerTabs",  
    "Entries": {  
        "{{ModId}}": { // It's important that this is your Mod Id!
            "TabDisplayName": "Example Power Mod",  
            "IconPath": "Mods/{{ModId}}/TabIcon", // You can also target any other image, such as LooseSprites/Cursors, if you prefer. You'll want to use IconSourceRect if you do.
            "IconSourceRect": { // Optional. If not specified, the whole image will be used.
                "X": 0,
                "Y": 0,
                "Width": 16,
                "Height": 16
            }  
        }  
    }
}
```
> You must use your unique ID as the ID for your PowerTabs entry, as this determines which tab modded powers are potentially automatically placed into. However, if you have both a Content Patcher component and a C# component in your mod, you can use the ModId of either one. If doing so causes your powers to have a different prefix, however, you will need to manually assign them to your tab.

If your modded powers are prefixed with your mod ID as mentioned, then there is nothing else you need to do. They will be placed into your new tab automagically. If this is the only part of Special Power Utilities that you make use of, then your powers will function as normal even if users do not have Special Power Utilities installed.

Do note that if your powers are _not_ prefixed with your mod ID, or if you have no modded powers at all, this will lead to an empty tab in the Special Items & Powers menu. **This includes any powers that are disabled via Content Patcher `When` conditions, as they are considered not loaded at all.** An empty tab won't break anything, but it will look weird and likely be reported as a bug by your users.

If your modded powers are not prefixed with your mod ID or otherwise need manual assignment, read onward.

## Specifying / Changing Power Tab Assignment

If you need to change what tab your modded powers are placed into (or need to specify it in the first place because they are not prefixed with your mod ID), then you do not need to make any changes to their names. Instead, you will assign their tab via the `CustomFields` field of your power in Content Patcher, like so:

```jsonc
{
    "Action": "EditData",
    "Target": "Data/Powers",
    "Entries": {
        "CoolPower": {
            "DisplayName": "Cool Power!",
            "Description": "Cool Description!",
            "TexturePath": "LooseSprites\\Cursors",
            "TexturePosition": { "X": 224, "Y": 320 },
            "UnlockedCondition": "PLAYER_STAT Current CoolPower 1",
            "CustomFields": {
                "Spiderbuttons.SpecialPowerUtilities/Tab": "{{ModId}}"
            }
        }
    }
}
```
You will need to add this field to all of your powers that need manual assignment, or else they will be placed into either a "Misc." category or the vanilla "Stardew Valley" category, depending on a user's personal configuration settings. You can also use this field to move a (custom) power into a tab that does _not_ match your mod ID, including moving that power into the "Stardew Valley" category, if you really need to.

##  Reordering Powers
Content Patcher's `MoveEntries` field will not work with the vanilla Special Items & Powers menu because it is a dictionary, not a list. Therefore, any modded powers that are added (without their own mod tab) will more than likely appear at the end of the list of powers, past the mastery stars and such, and vanilla powers are locked in place. With Special Power Utilities, you can change the ordering of the powers in the Special Items & Powers menu. This is done via the `CustomFields` field in your Data/Powers entry, with formats similar to the aforementioned `MoveEntries`, like below:
```jsonc
    "Action": "EditData",
    "Target": "Data/Powers",
    "Fields": {
        "Book_Horse": {
            "CustomFields": {
                "Spiderbuttons.SpecialPowerUtilities/Placement/ToPosition": "Top", // The front of the list i.e. index 0.
                "Spiderbuttons.SpecialPowerUtilities/Placement/ToPosition": "Bottom", // The back of the list.
                "Spiderbuttons.SpecialPowerUtilities/Placement/ToPosition": "7", // Index position 7
                "Spiderbuttons.SpecialPowerUtilities/Placement/BeforeID": "KeyToTheTown", // Just before KeyToTheTown
                "Spiderbuttons.SpecialPowerUtilities/Placement/AfterID": "KeyToTheTown", // Just after KeyToTheTown
            }
        }
    }
}
```
> **IMPORTANT**: You may only use one of `ToPosition`, `BeforeID`, or `AfterID`! They are shown together in the example in order to demonstrate how each is formatted, but only one entry for changing the position of a power is permitted.

These positions are calculated with the full list of every power in the game **before** they are separated into different tabs. What this means is that if you change the position of your _own_ power to, say, index position 7, it is not guaranteed to be placed 7th in your mod's custom tab where your powers are shown (in fact, it's likely to be placed first, since the indices of modded powers come after the index of the last vanilla power). However, if your power is specifically placed into the Stardew Valley category, or if a user has the tab functionality disabled, then your power will be placed in the 7th position, between the Magnifying Glass and the Dark Talisman.

Therefore, this feature is not very useful for custom powers unless you particularly care about where your power is placed among the vanilla powers in the event that a user has Special Power Utilities installed but the tabs disabled. If you care about the ordering of your own powers, simply change the order of your powers in your `content.json`. Otherwise, the intended use of this feature is for reordering the vanilla powers.

# BOOKS

Although Special Power Utilities was made with general powers in mind, it does offer some ways to make working with books specifically easier. Bear in mind that anything that Special Power Utilities does for books is either superficial in nature (and thus unnecessary, in the grand scheme of things) or can be achieved in other ways. If you are not keen on relying on a dependency mod, I would highly recommend against requiring Special Power Utilities just for these options.

## Customizable Message

By default, when you read a book (that isn't the Queen of Sauce recipe book) in Stardew Valley, a message will pop up with the text "You've learned a new power!" on the side of your screen. With Special Power Utilities, you can specify what text will appear instead of that message by following the below example:
```jsonc
{
    "Action": "EditData",
    "Target": "Data/Objects",
    "Entries": {
        "{{ModId}}_BookID": {
            ...
            "CustomFields": {
                "Spiderbuttons.SpecialPowerUtilities/Books/Message": "Your text goes here!"
            }
        }
    }
}
```
If you do not specify a message in `CustomFields`, then the vanilla behaviour will play out (i.e. your book will cause the usual "You've learned a new power!" text to appear when read). Alternatively, you can choose to prevent any message from popping up entirely by adding the context tag `spu_book_no_message`, like so:
```jsonc
{
    "Action": "EditData",
    "Target": "Data/Objects",
    "Entries": {
        "{{ModId}}_BookID": {
            ...
            "ContextTags": [
                "color_blue",
                "book_item",
                "spu_book_no_message"
            ]
        }
    }
}
```
Reading a book with the `spu_book_no_message` context tag, whether it's a vanilla book or a custom book, will not show a message on the side of your screen, regardless of whether or not you also specify a message in `CustomFields`. It is one or the other, not both.

## Animation Colour

When the animation for reading a book is displayed over your farmers head, it will take on the colour corresponding to the `color_` context tag that you added to your book item. This limits you to only choosing colours that already exist in the game for use in the dye system. With Special Power Utilities, however, you are able to specify any arbitrary colour you'd like with a context tag and a colour hex code.
    
```jsonc
{
    "Action": "EditData",
    "Target": "Data/Objects",
    "Entries": {
        "{{ModId}}_BookID": {
            ...
            "ContextTags": [
                "color_blue",
                "book_item",
                "spu_book_color"
            ],
            "CustomFields": {
                "Spiderbuttons.SpecialPowerUtilities/Books/Color": "#D000FF"
            }
        }
    }
}
```
> **IMPORTANT**: Because the colour is being multiplied on top of an off-white base colour, the resulting colour in game might not exactly match the hex code you specify. This is more evident the closer your individual RGB values are to 255 (FF). Since there is a maximum level of white you can achieve, sometimes it is simply not possible to get the exact colour you want after the multiply blending. Like, 99% of colours should be fine, though, and in any case, you can always choose to use the vanilla colour system if you're not happy with the result.
 
The hex code must follow that format: A `#` symbol followed by six hexadecimal digits. Anything past the first six digits will not be parsed, so transparency is not supported. If Special Power Utilities is unable to parse the hex code you provide, if you do not provide a hex code at all, or if you do not add the `spu_book_color` context tag, the book will default to the vanilla behaviour of using the `color_` context tag to determine the colour of the animation. For this reason, and to ensure the book still displays a proper colour when a user does not have Special Power Utilities installed, you should always still include a `color_` context tag for your book object.

You may also use `spu_book_colour` for the context tag and `Colour` for the CustomFields key if you prefer to spell it that way.

## Recipe Books

If you would like to grant a player a list of cooking recipes when you read a book, like the Queen of Sauce Cookbook, Special Power Utilities can handle this for you. You will need to follow at least one of these two steps:

- Add the context tag `spu_recipe_book` to your book object.
- (Optional) Specify a prefix for your recipe IDs in the `CustomFields` field of your book object.

The context tag is _required_ for any recipe book functionality—if it is not present, the game will treat your book like a normal book and no recipes will be granted. If all of the recipe IDs you wish to give to the player are prefixed with your mod ID, you do not need to specify a prefix; Special Power Utilities will automatically find and grant any recipes added by your mod. If your cooking recipe IDs are not prefixed with your mod ID, or if you would like to specify only some recipes with a specific prefix to be granted by this book while ignoring any others, you will need to specify a prefix. The following example will show how to follow both steps:
```jsonc
{
    "Action": "EditData",
    "Target": "Data/Objects",
    "Entries": {
        "{{ModId}}_RecipeBookID": {
            ...
            "ContextTags": [
                "color_blue",
                "book_item",
                "spu_recipe_book"
            ],
            "CustomFields": {
                "Spiderbuttons.SpecialPowerUtilities/Books/RecipePrefix": "EMR_"
            }
        }
    }
}

{
    "Action": "EditData",
    "Target": "Data/CookingRecipes",
    "Entries": {
        "EMR_Recipe1": ...,
        "EMR_Recipe2": ...,
        ...
        "EMR_Recipe10": ...
    }
}
```
Reading the book added in this example will grant the player who read it every recipe whose ID starts with "EMR_," in this case 10 recipes total. The message that will be displayed on screen when the player reads the book will match the Queen of Sauce Cookbook message, displaying how many recipes the player learned from reading your book. However, you can specify a custom message for your recipe book in the same way you would for a normal book, as shown in the previous section.

Additionally, as long as Special Power Utilities remains installed and your book still exists with the same ID, Special Power Utilities will keep track of which recipe books a player has already read. Upon loading a save or connecting to a multiplayer world, it will go through its tracked list of already read recipe books and grant any new recipes it finds. This means that if you add more recipes to your mod later on that are still meant to be granted by your recipe book, a player who has already read the book will already know them without needing to re-read the recipe book.

This is a permanent addition, however—if a player reads a recipe book, Special Power Utilities will always check if that book still exists and what its prefix is before granting the recipes. If a prefix was previously specified, but you have changed it in an update, Special Power Utilities will use that new prefix when granting the recipes again. If there was previously a prefix, but you remove the prefix entirely in an update, Special Power Utilities will try and grab your mod ID from the book ID. If it can, it will grant all recipes beginning with your mod ID. If it cannot figure out your mod ID, no recipes will be granted. In effect, this means that in order to not break your existing recipe book in future updates, you must, at the very least, keep the book ID the same. It also means that while you can make a book either more or less specific in a future update to your mod, you cannot entirely revoke its ability to try granting recipes unless you change your book ID (effectively replacing it with an entirely new book) or remove your book entirely.

# UTILITIES

The titular "utilities" of Special Power Utilities, in the form of Game State Queries, TriggerAction Actions, and Content Patcher tokens.

## Game State Queries

|Condition                  |Effect                       |Example|
|--------------------------|-----------------------------|----------------|
|PLAYER_HAS_POWER \<who> \<powerID> |Returns true if the [Target Player](https://stardewvalleywiki.com/Modding:Game_state_queries#Target_player) has unlocked the specified power, false otherwise.|`PLAYER_HAS_POWER Current KeyToTheTown`

## TriggerAction Actions


| Action                                                                           |Effect             |
|----------------------------------------------------------------------------------|-----------------------------|
| Spiderbuttons.SpecialPowerUtilities/Actions/SetPowerUnavailable [who] \<powerID> |Sets the specified power to be unavailable for the [Target Player](https://stardewvalleywiki.com/Modding:Game_state_queries#Target_player). If no target is specified, it defaults to `Current`.
| Spiderbuttons.SpecialPowerUtilities/Actions/SetPowerAvailable [who] \<powerID>   |Sets the specified power to be available for the [Target Player](https://stardewvalleywiki.com/Modding:Game_state_queries#Target_player). If no target is specified, it defaults to `Current`.

## Content Patcher Tokens

|Token|Effect                       | Example                                                                                      
|--------------------------|-----------------------------|----------------------------------------------------------------------------------------------|
|Spiderbuttons.SpecialPowerUtilities/HasPower|Similar to `HasProfession` for checking whether or not powers are unlocked. Accepts an optional input argument of `Any` to check whether any connected player has a power.| `"When": { "Spiderbuttons.SpecialPowerUtilities/HasPower \|contains=Book_Mystery": "true" }` 
|Spiderbuttons.SpecialPowerUtilities/UnavailablePowers|Similar to `HasPower` above for checking whether or not powers are available. Accepts an optional input argument of `Any` to check whether any player has a power unavailable to them.| `"When": { "Spiderbuttons.SpecialPowerUtilities/UnavailablePowers:Any": "Book_Horse" }`      

# POWER AVAILABILITY

With Special Power Utilities, special items and powers can now be marked as "unavailable" and will appear faded to the player on their special items and powers tab. Marking a power as "unavailable" does not do anything on its own—it would be up to you to determine what effects occur when a player's power is unavailable. It is intended for temporarily revoking the benefits granted by a power under circumstances you choose and then giving those benefits back later. It will _not_ decrement any stats or remove any mail flags or anything like that. Players will still have the special items or powers _unlocked_ (i.e. they will still meet the UnlockedCondition as described in the Data/Powers entry), but the effects of the special items or powers may just be temporarily suppressed. If a player uninstalls Special Power Utilities, their special items and powers will once more be made available to them.

It is important to note that setting a power from the base game (such as the Key to the Town) to be unavailable will do nothing on its own. Setting a power unavailable is simply setting a flag. Nothing will check that flag in the base game without you (or me) deeming it so, but for the base game powers, there are simply far too many places where each power would be checked for me to consider that a worthwhile time investment. Power (un)availability is intended for use with modded powers where you can precisely control these checks. Niche? Terribly. Useful to me? Possibly. Therefore, it's included, because why not.

It's important to note that setting a power as available **does not grant the player that power** if they do not already have it. A power being available is not the same as a power being unlocked. If a player does not have a power but you set it to available, it simply means that it won't be unavailable when they _do_ get it, but they will not get it before meeting the original unlock requirements.

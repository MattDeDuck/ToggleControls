# Toggle Controls
Toggle Controls is a mod made with BepInEx for use with the Potion Craft: Alchemist Simulator game. It gives you the ability to use a toggle key to stir your cauldron, grind your ingredient and use your ladle!

Now updated to work with the Potion Craft: Aclhemist Simulator v1.0.2.1

### BepInEx
- Download the latest BepInEx 5 release from [here](https://github.com/BepInEx/BepInEx/releases)
Note: You need the `BepInEx_64` version for use with Potion Craft
- You can read the installation for BepInEx [here](https://docs.bepinex.dev/articles/user_guide/installation/index.html)
- Once installed run once to generate the config files and folders

### Toggle Controls installation
- Download the latest version from the [releases page](https://github.com/MattDeDuck/ToggleControls/releases)
- Unzip the folder
- Copy the folder into `Potion Craft/BepInEx/plugins/`
- Run the game

### Customise the settings
The settings are found in `settings.json`
- You can customise the speed of the stirring/pouring/grinding (float value) This is ranging from slowest `0.1` - fastest `1.0`
- You can also customise the key you use for the toggles. You can see the naming conventions for the inputs [here](https://docs.unity3d.com/Manual/class-InputManager.html)

Note: The grind speed setting needs to be quite low (for example: 0.005)

### Example

![Stir with no spoon in the cauldron!](https://github.com/MattDeDuck/ToggleControls/blob/master/togglestirexample.gif)
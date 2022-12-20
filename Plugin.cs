using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.RecipeMap;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.IndicatorMapItem;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapObject;
using PotionCraft.ObjectBased.UIElements.Books.GoalsBook;
using PotionCraft.ObjectBased.UIElements.FloatingText;
using PotionCraft.ObjectBased.Stack;
using PotionCraft.ScriptableObjects.Ingredient;
using PotionCraft.ScriptableObjects.Potion;
using PotionCraft.Settings;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;


namespace ToggleControls
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, "1.0.2.1")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; set; }
        public static string pluginLoc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public JObject settingsObj;

        public static bool isAwake = false;

        // Cauldron - stirring
        public static string toggleButtonStir;
        public static bool isStirring = false;
        public static float stirringSpeed;

        // Mortar - grinding
        public static string toggleButtonGrind;
        public static bool isGrinding = false;
        public static float grindingSpeed;

        // Ladle - pouring
        public static string toggleButtonPour;
        public static bool isPouring = false;
        public static float pouringSpeed;

        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            Log = this.Logger;

            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        public void Update()
        {
            if (isAwake)
            {
                // Cauldron
                List<PotionUsedComponent> cauldronIngredients = Managers.Potion.usedComponents;

                // Pouring and stirring
                if (cauldronIngredients.Count > 0)
                {
                    settingsObj = JObject.Parse(File.ReadAllText(pluginLoc + "/settings.json"));

                    // Get the pouring toggle button
                    toggleButtonPour = (string)settingsObj["toggleButtonPour"];

                    if (Input.GetKeyDown(toggleButtonPour))
                    {
                        isPouring = !isPouring;
                        string text = isPouring == true ? "Toggle Pouring on" : "Toggle Pouring off";
                        CollectedFloatingText.SpawnNewText(Managers.Ingredient.alchemyMachine.floatingTextPrefab.gameObject, new Vector3(-2f, -2.5f, 0f), new CollectedFloatingText.FloatingTextContent(text, CollectedFloatingText.FloatingTextContent.Type.Text, 0f), Managers.Game.Cam.transform, false, false);
                    }

                    // Get the stirring toggle button
                    toggleButtonStir = (string)settingsObj["toggleButtonStir"];

                    if (Input.GetKeyDown(toggleButtonStir))
                    {
                        isStirring = !isStirring;
                        string text = isStirring == true ? "Toggle Stir on" : "Toggle Stir off";
                        CollectedFloatingText.SpawnNewText(Managers.Ingredient.alchemyMachine.floatingTextPrefab.gameObject, new Vector3(1f, -2.5f, 0f), new CollectedFloatingText.FloatingTextContent(text, CollectedFloatingText.FloatingTextContent.Type.Text, 0f), Managers.Game.Cam.transform, false, false);
                    }
                }

                // Grinding
                if (Managers.Ingredient.mortar.containedStack != null)
                {
                    settingsObj = JObject.Parse(File.ReadAllText(pluginLoc + "/settings.json"));
                    toggleButtonGrind = (string)settingsObj["toggleButtonGrind"];

                    if (Input.GetKeyDown(toggleButtonGrind))
                    {
                        isGrinding = !isGrinding;
                        string text = isGrinding == true ? "Toggle Grind on" : "Toggle Grind off";
                        CollectedFloatingText.SpawnNewText(Managers.Ingredient.alchemyMachine.floatingTextPrefab.gameObject, new Vector3(2f, -2.5f, 0f), new CollectedFloatingText.FloatingTextContent(text, CollectedFloatingText.FloatingTextContent.Type.Text, 0f), Managers.Game.Cam.transform, false, false);
                    }
                }

                // If the Cauldron is being stirred by toggle button
                if (isStirring)
                {
                    isGrinding = false;
                    isPouring = false;

                    // Get the stirring speed setting
                    settingsObj = JObject.Parse(File.ReadAllText(pluginLoc + "/settings.json"));
                    stirringSpeed = (float)settingsObj["stirringSpeed"];

                    // Stir the cauldron
                    RecipeMapManagerIndicatorSettings asset = Settings<RecipeMapManagerIndicatorSettings>.Asset;
                    float num = stirringSpeed * asset.indicatorSpeed;
                    if (num > Mathf.Epsilon)
                    {
                        Managers.RecipeMap.indicator.lengthToDeleteFromPath += num;
                        if (Managers.Potion.potionStartedAt == null)
                        {
                            Managers.Potion.potionStartedAt = Managers.RecipeMap.currentMap;
                        }
                        if (Managers.Potion.usedComponents.Count > 0)
                        {
                            GoalsLoader.GetGoalByName("MoveIndicator", true).ProgressIncrement(1);
                        }
                    }
                }

                // If the ladle is being used
                if (isPouring)
                {
                    isStirring = false;
                    isGrinding = false;

                    // Get the stirring speed setting
                    settingsObj = JObject.Parse(File.ReadAllText(pluginLoc + "/settings.json"));
                    pouringSpeed = (float)settingsObj["pouringSpeed"];

                    Managers.RecipeMap.MoveIndicatorTowardsBase(pouringSpeed);
                }

                if (isGrinding)
                {
                    isStirring = false;
                    isPouring = false;

                    // Get the grinding speed setting
                    settingsObj = JObject.Parse(File.ReadAllText(pluginLoc + "/settings.json"));
                    grindingSpeed = (float)settingsObj["grindingSpeed"];

                    Stack mortarStack = Managers.Ingredient.mortar.containedStack;
                    mortarStack.substanceGrinding.CurrentGrindStatus += grindingSpeed;
                    mortarStack.UpdateOverallGrindStatus();
                    mortarStack.UpdateGrindedSubstance();

                    // Get ingredient that's in the mortar
                    Ingredient mortarIngredient = (Ingredient)mortarStack.inventoryItem;
                    Managers.RecipeMap.path.ShowPath(mortarIngredient, Managers.Ingredient.mortar.containedStack.overallGrindStatus, 0f, 0f, true);
                }
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(IndicatorMapItem), "PotionFailed")]
        public static void OnIndicatorRuined_Prefix()
        {
            // If potion fails...turn the toggles off
            isStirring = false;
            isGrinding = false;
            isPouring = false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(RecipeMapObject), "Awake")]
        public static void Awake_Postfix()
        {
            isAwake = true;
        }
    }
}
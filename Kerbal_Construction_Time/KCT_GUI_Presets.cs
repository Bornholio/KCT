﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalConstructionTime
{
    public static partial class KCT_GUI
    {
        private static int presetsWidth = 900, presetsHeight = 600;
        private static Rect presetPosition = new Rect((Screen.width-presetsWidth) / 2, (Screen.height-presetsHeight) / 2, presetsWidth, presetsHeight);
        private static Rect presetNamingWindowPosition = new Rect((Screen.width - 250) / 2, (Screen.height - 50) / 2, 250, 50);
        private static int presetIndex = -1;
        private static KCT_Preset WorkingPreset;
        private static Vector2 presetScrollView, presetMainScroll;
        private static bool changed = false, showFormula = false;
        private static string OMultTmp = "", BEffTmp = "", IEffTmp = "", ReEffTmp = "", MaxReTmp = "";
        public static void DrawPresetWindow(int windowID)
        {
            GUIStyle yellowText = new GUIStyle(GUI.skin.label);
            yellowText.normal.textColor = Color.yellow;

            if (WorkingPreset == null)
            {
                SetNewWorkingPreset(new KCT_Preset(KCT_PresetManager.Instance.ActivePreset), false); //might need to copy instead of assign here
                presetIndex = KCT_PresetManager.Instance.GetIndex(WorkingPreset);
            }

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();

            //preset selector
            GUILayout.BeginVertical();
            GUILayout.Label("Presets", yellowText, GUILayout.ExpandHeight(false));
            //preset toolbar in a scrollview
            presetScrollView = GUILayout.BeginScrollView(presetScrollView, HighLogic.Skin.textArea, GUILayout.Width(presetPosition.width/6.0f));
            string[] presetShortNames = KCT_PresetManager.Instance.PresetShortNames(true);
            if (presetIndex == -1)
            {
                SetNewWorkingPreset(null, true);
                /*presetIndex = presetShortNames.Length - 1;
                WorkingPreset.name = "Custom";
                WorkingPreset.shortName = "Custom";
                WorkingPreset.description = "A custom set of configs.";
                WorkingPreset.author = HighLogic.SaveFolder;*/
            }
            if (changed && presetIndex < presetShortNames.Length - 1 && !KCT_Utilities.ConfigNodesAreEquivalent(WorkingPreset.AsConfigNode(), KCT_PresetManager.Instance.Presets[presetIndex].AsConfigNode())) //!KCT_PresetManager.Instance.PresetsEqual(WorkingPreset, KCT_PresetManager.Instance.Presets[presetIndex], true)
            {
                SetNewWorkingPreset(null, true);
                /*presetIndex = presetShortNames.Length - 1; //Custom preset
                WorkingPreset.name = "Custom";
                WorkingPreset.shortName = "Custom";
                WorkingPreset.description = "A custom set of configs.";
                WorkingPreset.author = HighLogic.SaveFolder;*/
            }
           /* presetIndex = KCT_PresetManager.Instance.GetIndex(WorkingPreset); //Check that the current preset is equal to the expected one
            if (presetIndex == -1) 
                presetIndex = presetNames.Length - 1;*/
            int prev = presetIndex;
            presetIndex = GUILayout.SelectionGrid(presetIndex, presetShortNames, 1);
            if (prev != presetIndex) //If a new preset was selected
            {
                if (presetIndex != presetShortNames.Length - 1)
                {
                    SetNewWorkingPreset(new KCT_Preset(KCT_PresetManager.Instance.Presets[presetIndex]), false);
                }
                else
                {
                    SetNewWorkingPreset(null, true);
                    /*WorkingPreset.name = "Custom";
                    WorkingPreset.shortName = "Custom";
                    WorkingPreset.description = "A custom set of configs.";
                    WorkingPreset.author = HighLogic.SaveFolder;*/
                }
            }

            //presetIndex = GUILayout.Toolbar(presetIndex, presetNames);

            GUILayout.EndScrollView();
            if (GUILayout.Button("Save as\nNew Preset", GUILayout.ExpandHeight(false)))
            {
                //create new preset
                SaveAsNewPreset(WorkingPreset);
            }
            if (WorkingPreset.AllowDeletion && presetIndex != presetShortNames.Length - 1 && GUILayout.Button("Delete Preset")) //allowed to be deleted and isn't Custom
            {
                DialogOption[] options = new DialogOption[2];
                options[0] = new DialogOption("Delete File", DeleteActivePreset);
                options[1] = new DialogOption("Cancel", DummyVoid);
                MultiOptionDialog dialog = new MultiOptionDialog("Are you sure you want to delete the selected Preset, file and all? This cannot be undone!", windowTitle: "Confirm Deletion", options: options);
                PopupDialog.SpawnPopupDialog(dialog, false, HighLogic.Skin);
            }
            GUILayout.EndVertical();

            //Main sections
            GUILayout.BeginVertical();
            presetMainScroll = GUILayout.BeginScrollView(presetMainScroll);
            //Preset info section
            GUILayout.BeginVertical(HighLogic.Skin.textArea);
            GUILayout.Label("Preset Name: " + WorkingPreset.name);
            GUILayout.Label("Description: " + WorkingPreset.description);
            GUILayout.Label("Author(s): " + WorkingPreset.author);
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            //Features section
            GUILayout.BeginVertical();
            GUILayout.Label("Features", yellowText);
            GUILayout.BeginVertical(HighLogic.Skin.textArea);
            WorkingPreset.generalSettings.Enabled= GUILayout.Toggle(WorkingPreset.generalSettings.Enabled, "Mod Enabled", HighLogic.Skin.button);
            WorkingPreset.generalSettings.BuildTimes = GUILayout.Toggle(WorkingPreset.generalSettings.BuildTimes, "Build Times", HighLogic.Skin.button);
            WorkingPreset.generalSettings.ReconditioningTimes = GUILayout.Toggle(WorkingPreset.generalSettings.ReconditioningTimes, "Launchpad Reconditioning", HighLogic.Skin.button);
            WorkingPreset.generalSettings.TechUnlockTimes = GUILayout.Toggle(WorkingPreset.generalSettings.TechUnlockTimes, "Tech Unlock Times", HighLogic.Skin.button);
            WorkingPreset.generalSettings.KSCUpgradeTimes = GUILayout.Toggle(WorkingPreset.generalSettings.KSCUpgradeTimes, "KSC Upgrade Times", HighLogic.Skin.button);
            WorkingPreset.generalSettings.Simulations = GUILayout.Toggle(WorkingPreset.generalSettings.Simulations, "Allow Simulations", HighLogic.Skin.button);
            WorkingPreset.generalSettings.SimulationCosts = GUILayout.Toggle(WorkingPreset.generalSettings.SimulationCosts, "Simulation Costs", HighLogic.Skin.button);
            WorkingPreset.generalSettings.RequireVisitsForSimulations = GUILayout.Toggle(WorkingPreset.generalSettings.RequireVisitsForSimulations, "Must Visit Planets", HighLogic.Skin.button);
            WorkingPreset.generalSettings.TechUpgrades = GUILayout.Toggle(WorkingPreset.generalSettings.TechUpgrades, "Upgrades From Tech Tree", HighLogic.Skin.button);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Starting Upgrades:");
            WorkingPreset.generalSettings.StartingPoints = GUILayout.TextField(WorkingPreset.generalSettings.StartingPoints, GUILayout.Width(100));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndVertical(); //end Features


            GUILayout.BeginVertical(); //Begin time settings
            GUILayout.Label("Time Settings", yellowText);
            GUILayout.BeginVertical(HighLogic.Skin.textArea);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Overall Multiplier: ");
            double.TryParse(OMultTmp = GUILayout.TextField(OMultTmp, 10, GUILayout.Width(80)), out WorkingPreset.timeSettings.OverallMultiplier);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Build Effect: ");
            double.TryParse(BEffTmp = GUILayout.TextField(BEffTmp, 10, GUILayout.Width(80)), out WorkingPreset.timeSettings.BuildEffect);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Inventory Effect: ");
            double.TryParse(IEffTmp = GUILayout.TextField(IEffTmp, 10, GUILayout.Width(80)), out WorkingPreset.timeSettings.InventoryEffect);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Reconditioning Effect: ");
            double.TryParse(ReEffTmp = GUILayout.TextField(ReEffTmp, 10, GUILayout.Width(80)), out WorkingPreset.timeSettings.ReconditioningEffect);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Reocnditioning: ");
            double.TryParse(MaxReTmp = GUILayout.TextField(MaxReTmp, 10, GUILayout.Width(80)), out WorkingPreset.timeSettings.MaxReconditioning);
            GUILayout.EndHorizontal();
            GUILayout.Label("Rollout-Reconditioning Split:");
            GUILayout.BeginHorizontal();
            //GUILayout.Label("Rollout", GUILayout.ExpandWidth(false));
            WorkingPreset.timeSettings.RolloutReconSplit = GUILayout.HorizontalSlider((float)Math.Floor(WorkingPreset.timeSettings.RolloutReconSplit * 100f), 0, 100)/100.0;
            //GUILayout.Label("Recon.", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
            GUILayout.Label((Math.Floor(WorkingPreset.timeSettings.RolloutReconSplit*100))+"% Rollout, "+(100-Math.Floor(WorkingPreset.timeSettings.RolloutReconSplit*100))+"% Reconditioning");
            GUILayout.EndVertical(); //end time settings
            GUILayout.EndVertical();
            GUILayout.EndHorizontal(); //end feature/time setting split

            //begin formula settings
            GUILayout.BeginVertical();
            GUILayout.Label("Formula Settings (Advanced)", yellowText);
            GUILayout.BeginVertical(HighLogic.Skin.textArea);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Show/Hide Formulas"))
            {
                showFormula = !showFormula;
            }
            if (GUILayout.Button("View Wiki in Browser"))
            {
                Application.OpenURL("https://github.com/magico13/KCT/wiki");
            }
            GUILayout.EndHorizontal();

            if (showFormula)
            {
                //show half here, half on other side? Or all in one big list
                int textWidth = 350;
                GUILayout.BeginHorizontal();
                GUILayout.Label("NodeFormula: ");
                WorkingPreset.formulaSettings.NodeFormula = GUILayout.TextField(WorkingPreset.formulaSettings.NodeFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("UpgradeFunds: ");
                WorkingPreset.formulaSettings.UpgradeFundsFormula = GUILayout.TextField(WorkingPreset.formulaSettings.UpgradeFundsFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("UpgradeScience: ");
                WorkingPreset.formulaSettings.UpgradeScienceFormula = GUILayout.TextField(WorkingPreset.formulaSettings.UpgradeScienceFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("ResearchFormula: ");
                WorkingPreset.formulaSettings.ResearchFormula = GUILayout.TextField(WorkingPreset.formulaSettings.ResearchFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("EffectivePart: ");
                WorkingPreset.formulaSettings.EffectivePartFormula = GUILayout.TextField(WorkingPreset.formulaSettings.EffectivePartFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("ProceduralPart: ");
                WorkingPreset.formulaSettings.ProceduralPartFormula = GUILayout.TextField(WorkingPreset.formulaSettings.ProceduralPartFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("BPFormula: ");
                WorkingPreset.formulaSettings.BPFormula = GUILayout.TextField(WorkingPreset.formulaSettings.BPFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("KSCUpgrade: ");
                WorkingPreset.formulaSettings.KSCUpgradeFormula = GUILayout.TextField(WorkingPreset.formulaSettings.KSCUpgradeFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Reconditioning: ");
                WorkingPreset.formulaSettings.ReconditioningFormula = GUILayout.TextField(WorkingPreset.formulaSettings.ReconditioningFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("BuildRate: ");
                WorkingPreset.formulaSettings.BuildRateFormula = GUILayout.TextField(WorkingPreset.formulaSettings.BuildRateFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("SimCost: ");
                WorkingPreset.formulaSettings.SimCostFormula = GUILayout.TextField(WorkingPreset.formulaSettings.SimCostFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("KerbinSimCost: ");
                WorkingPreset.formulaSettings.KerbinSimCostFormula = GUILayout.TextField(WorkingPreset.formulaSettings.KerbinSimCostFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("UpgradeReset: ");
                WorkingPreset.formulaSettings.UpgradeResetFormula = GUILayout.TextField(WorkingPreset.formulaSettings.UpgradeResetFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("InventorySale: ");
                WorkingPreset.formulaSettings.InventorySaleFormula = GUILayout.TextField(WorkingPreset.formulaSettings.InventorySaleFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical(); 
            GUILayout.EndVertical(); //end formula settings

            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Save", GUILayout.ExpandWidth(false)))
            {
                KCT_PresetManager.Instance.ActivePreset = WorkingPreset;
                KCT_PresetManager.Instance.SaveActiveToSaveData();
                WorkingPreset = null;
                showSettings = false;


                if (!KCT_PresetManager.Instance.ActivePreset.generalSettings.Enabled && KCT_GameStates.settings.enabledForSave)
                    KCT_Utilities.DisableModFunctionality();
                KCT_GameStates.settings.enabledForSave = KCT_PresetManager.Instance.ActivePreset.generalSettings.Enabled;
                KCT_GameStates.settings.MaxTimeWarp = newTimewarp;
                KCT_GameStates.settings.ForceStopWarp = forceStopWarp;
                KCT_GameStates.settings.DisableAllMessages = disableAllMsgs;
                KCT_GameStates.settings.OverrideLaunchButton = overrideLaunchBtn;
                KCT_GameStates.settings.Debug = debug;
                KCT_GameStates.settings.AutoKACAlarams = autoAlarms;
                KCT_GameStates.settings.PreferBlizzyToolbar = useBlizzyToolbar;

                KCT_GameStates.settings.Save();
                showSettings = false;
                if (!PrimarilyDisabled && !showFirstRun)
                {
                    if (KCT_Events.instance.KCTButtonStock != null)
                        KCT_Events.instance.KCTButtonStock.SetTrue();
                    else
                        showBuildList = true;
                }
                if (!KCT_PresetManager.Instance.ActivePreset.generalSettings.Enabled) InputLockManager.RemoveControlLock("KCTKSCLock");
            }
            if (GUILayout.Button("Cancel", GUILayout.ExpandWidth(false)))
            {
                WorkingPreset = null;
                showSettings = false;
                if (!PrimarilyDisabled && !showFirstRun)
                {
                    if (KCT_Events.instance.KCTButtonStock != null)
                        KCT_Events.instance.KCTButtonStock.SetTrue();
                    else
                        showBuildList = true;
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical(); //end column 2

            GUILayout.BeginVertical(GUILayout.Width(100)); //Start general settings
            GUILayout.Label("General Settings", yellowText);
            GUILayout.Label("NOTE: Affects all saves!", yellowText);
            GUILayout.BeginVertical(HighLogic.Skin.textArea);
            GUILayout.Label("Max Timewarp");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
            {
                newTimewarp = Math.Max(newTimewarp - 1, 0);
            }
            //current warp setting
            GUILayout.Label(TimeWarp.fetch.warpRates[newTimewarp] + "x");
            if (GUILayout.Button("+", GUILayout.ExpandWidth(false)))
            {
                newTimewarp = Math.Min(newTimewarp + 1, TimeWarp.fetch.warpRates.Length - 1);
            }
            GUILayout.EndHorizontal();

            forceStopWarp = GUILayout.Toggle(forceStopWarp, "Auto Stop TimeWarp", HighLogic.Skin.button);
            autoAlarms = GUILayout.Toggle(autoAlarms, "Auto KAC Alarms", HighLogic.Skin.button);
            overrideLaunchBtn = GUILayout.Toggle(overrideLaunchBtn, "Override Launch Button", HighLogic.Skin.button);
            useBlizzyToolbar = GUILayout.Toggle(useBlizzyToolbar, "Use Toolbar Mod", HighLogic.Skin.button);
            disableAllMsgs = !GUILayout.Toggle(!disableAllMsgs, "Use Message System", HighLogic.Skin.button);
            debug = GUILayout.Toggle(debug, "Debug Logging", HighLogic.Skin.button);
            GUILayout.EndVertical();
            GUILayout.EndVertical();
            
            GUILayout.EndHorizontal(); //end main split
            GUILayout.EndVertical(); //end window

            changed = GUI.changed;

            if (!Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2))
                GUI.DragWindow();
        }

        public static void SetNewWorkingPreset(KCT_Preset preset, bool setCustom)
        {
            if (preset != null)
                WorkingPreset = preset;
            if (setCustom)
            {
                presetIndex = KCT_PresetManager.Instance.PresetShortNames(true).Length - 1; //Custom preset
                WorkingPreset.name = "Custom";
                WorkingPreset.shortName = "Custom";
                WorkingPreset.description = "A custom set of configs.";
                WorkingPreset.author = HighLogic.SaveFolder;
            }

            OMultTmp = WorkingPreset.timeSettings.OverallMultiplier.ToString();
            BEffTmp = WorkingPreset.timeSettings.BuildEffect.ToString();
            IEffTmp = WorkingPreset.timeSettings.InventoryEffect.ToString();
            ReEffTmp = WorkingPreset.timeSettings.ReconditioningEffect.ToString();
            MaxReTmp = WorkingPreset.timeSettings.MaxReconditioning.ToString();
        }

        private static string saveName, saveShort, saveDesc, saveAuthor;
        private static bool saveCareer, saveScience, saveSandbox;
        private static KCT_Preset toSave;
        public static bool showPresetSaver = false;
        public static void DrawPresetSaveWindow(int windowID)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Preset name:");
            saveName = GUILayout.TextField(saveName, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Preset short name:");
            saveShort = GUILayout.TextField(saveShort, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Preset author(s):");
            saveAuthor = GUILayout.TextField(saveAuthor, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal();
            GUILayout.Label("Preset description:");
            saveDesc = GUILayout.TextField(saveDesc, GUILayout.Width(220));
            //GUILayout.EndHorizontal();

            saveCareer = GUILayout.Toggle(saveCareer, " Show in Career Games");
            saveScience = GUILayout.Toggle(saveScience, " Show in Science Games");
            saveSandbox = GUILayout.Toggle(saveSandbox, " Show in Sandbox Games");

            KCT_Preset existing = KCT_PresetManager.Instance.FindPresetByShortName(saveShort);
            bool AlreadyExists = existing != null;
            bool CanOverwrite = AlreadyExists ? existing.AllowDeletion : true;

            if (AlreadyExists)
                GUILayout.Label("Warning: A preset with that short name already exists!");

            GUILayout.BeginHorizontal();
            if (CanOverwrite && GUILayout.Button("Save"))
            {
                toSave.name = saveName;
                toSave.shortName = saveShort;
                toSave.description = saveDesc;
                toSave.author = saveAuthor;

                toSave.CareerEnabled = saveCareer;
                toSave.ScienceEnabled = saveScience;
                toSave.SandboxEnabled = saveSandbox;

                toSave.AllowDeletion = true;

                toSave.SaveToFile(KSPUtil.ApplicationRootPath + "/GameData/KerbalConstructionTime/KCT_Presets/"+toSave.shortName+".cfg");
                showPresetSaver = false;
                KCT_PresetManager.Instance.FindPresetFiles();
                KCT_PresetManager.Instance.LoadPresets();
            }
            if (GUILayout.Button("Cancel"))
            {
                showPresetSaver = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            CenterWindow(ref presetNamingWindowPosition);
        }

        public static void SaveAsNewPreset(KCT_Preset newPreset)
        {
            toSave = newPreset;
            saveCareer = newPreset.CareerEnabled;
            saveScience = newPreset.ScienceEnabled;
            saveSandbox = newPreset.SandboxEnabled;

            saveName = newPreset.name;
            saveShort = newPreset.shortName;
            saveDesc = newPreset.description;
            saveAuthor = newPreset.author;

            showPresetSaver = true;
        }

        public static void DeleteActivePreset()
        {
            KCT_PresetManager.Instance.DeletePresetFile(WorkingPreset.shortName);
        }

    }
}

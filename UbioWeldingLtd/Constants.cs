using UnityEngine;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace UbioWeldingLtd
{
	/*
	 * Constants Definition
	 */
	static class Constants
	{
		//Logs/debug constants
		public const string logVersion		  = "v2.0pt5-0.24.2";
		public const string logWarning		  = "WARNING ";
		public const string logError			= "ERROR ";
		public const string logPrefix		   = "[WeldingTool] ";
		public const string logStartWeld		= "----- Starting Welding -----";
		public const string logEndWeld		  = " ----- End Welding -----";
		public const string logNbPart		   = " parts welded";
		public const string logWeldingPart	  = "Welding this part: ";
		public const string logModelUrl		 = "MODEL url: ";
		public const string logResMerge		 = "RESOURCE add: ";
		public const string logResAdd		   = "RESOURCE new: ";
		public const string logModMerge		 = "MODULE merge: ";
		public const string logModAdd		   = "MODULE add: ";
		public const string logModIgnore		= "MODULE ignore duplicate: ";
		public const string logFxAdd			= "FX Add: ";
		public const string logNodeAdd		  = "Stack Node Add: ";
		public const string logWritingFile	  = "Writing File: ";
		public const string logWarnNoMesh	   = "Mesh value does not link to a mesh file";

		//GUI
		public const string guiWeldLabel		= "Weld it";
		public const string guiCancel		   = " Cancel ";
		public const string guiOK			   = " OK ";
		public const string guiSave			 = " Save ";
		public const int guiWeldButWidth		= 70;
		public const int guiWeldButHeight	   = 36;
		public const int guiDialogX			 = 200;
		public const int guiDialogY			 = 100;
		public const int guiDialogW			 = 400;
		public const int guiDialogH			 = 200;
		public const int guiInfoWindowX		 = 300;
		public const int guiInfoWindowY		 = 150;
		public const int guiInfoWindowW		 = 700;
		public const int guiInfoWindowH		 = 300;
        public const int guiMainWindowW     = 300;
		public const int guiMainWindowH		= 200;
		public const int guiMainWindowHSettingsExpanded = 620;
		public const int guiScreenEdgeClearance = 80;
		public const string guiDialFail		 = "We are sorry to announce that our engineer could not perform this weld.\n Please read the report (ALt+F2 or ksp.log) for more details)";
		public const string guiDialWarn		 = "After welding everything, out Engineer had some extra feature that they didn't knew where to put.\n Please read the report (ALt+F2 or ksp.log) for more details)";
		public const string guiNameUsed		 = "Name already used by another part!";
		public const string guiDialOverwrite	= "File already exist, Do you want to overwrite it?";
		public const string guiDialSaved		= "New part saved and shipped!";

        //Main&settings window GUI labels
//        public const string guiMainWindowDescription = "Press \"" + guiWeldItButtonLabel + "\" button to weld all craft selected part of it";
        public const string guiDbAutoReloadLabel = "Database autoreload";
        public const string guiDbAutoReloadTip = "Auto reload game Database after welding";
        public const string guiAllNodesLabel = "Include all nodes";
        public const string guiAllNodesTip = "Create all attach node, included those already attached";
        public const string guiDontProcessMasslessPartsLabel = "Don't process massless parts";
        public const string guiDontProcessMasslessPartsTip = "Don't take into account mass of massless parts (with PhysicsSignificance = 1)";
        public const string guiUseNamedCfgFileLabel = "Use named part's file";
        public const string guiUseNamedCfgFileTip = "Use for welded part name of file like \"BigPod.cfg\", not \"part.cfg\"";
        public const string guiSaveSettingsButtonLabel = "Save settings";
        public const string guiSaveSettingsButtonTip = "Save settings to config file";
        public const string guiWeldItButtonLabel = "Weld it";
        public const string guiWeldItButtonTip = "Press \"" + guiWeldItButtonLabel + "\" button to weld whole craft or selected part of it";
        
		//Settings
		public const string settingWeldingLock = "UBILOCK9213";
        public const string settingPreventClickThroughLock = "UBILOCKClick";
		public const string settingEdiButX	  = "editorButtonXPosition";
		public const string settingEdiButY	  = "editorButtonYPosition";
		public const string settingDbAutoReload = "dataBaseAutoReload";
		public const string settingAllNodes	 = "includeAllNodes";
		public const string settingAllowCareer  = "allowCareerMode";
		public const string settingDontProcessMasslessParts = "dontProcessMasslessParts";
		public const string settingIconGetPath = "UbioWeldingLtd/Textures/ToolbarIcon";
		public const string settingXmlFilePath = "/PluginData/UbioWeldingLtd/";
		public const string settingXmlConfigFileName = "new-config.xml";
		public const string settingXmlOldConfigFileName = "config.xml";
		public const string settingXmlListFileName = "moduleAttributeList.xml";
		public const string underline = "_";
		public static readonly string settingRuntimeDirectory = Assembly.GetExecutingAssembly().Location.Replace(new FileInfo(Assembly.GetExecutingAssembly().Location).Name, "");

		//Readme
		public const string setupGeneralLine1 = "The ModuleAttributeList.xml gives you the ability to edit how the Weldingtool will process the merging of certain modules and their attributes.";
		public const string setupGeneralLine2 = "Caution editing this file may result in broken welded parts or even not welded parts at all.";
		public const string setupGeneralLine3 = "If you delete this file and the config.xml, then the welding tool will provide you with fresh generated files that contain the default values.";
		public const string setupVector2Line1 = "This is the list of modules that have to be read as vector2 curves, as an example the ISP of an RCS thruster.";
		public const string setupVector4Line1 = "In this list are modules that will be read as vector4Curves that means they will create curves from floatpoints and tangents. There is a thread in the forum just about the magic of floatpoint tangents.";
		public const string setupSubmoduleLine1 = "This list contains the Submodules that otherwise would be ignored and not merged, Adding an entry here will give the tool the ability to merge the attributes in it.";
		public const string setupModulesToIgnoreLine1 = "Modules in this list will be completly ignored from the tool and not get added to the new part.";
		public const string setupAveragedAttribtesLine1 = "Entries here will make sure that the tool will not simply add the values of this attribute from the different parts and modules up, but will calculate it as aritmetric mean (average).";
		public const string setupUnchangedAttribtesLine1 = "Entries in this list will give the tool the order to not merge the values for this attribute.";
		public const string setupBreakingAttribtesLine1 = "This might be the most important list of Attributes, the entries here will give the tool the order to check if the values of these attributes are equal, and only allow then the merging or the module, otherwise a new module would be added.";
		public const string setupAddingAttributeEntryLine1 = "To add a new entry to the list so that the welding process will make use of it, you need to name it the way the toll can read, currently that means 'name of the module'+'_'+'name of the attribute'.";
		public const string setupCommentBegin = "<!-- ";
		public const string setupCommentEnd = " -->";

		//Messages
		public const string msgSuccess		  = "Welding is a success";
		public const string msgFail			 = "Welding is a failure";
		public const string msgCfgMissing	   = "Missing Config File";
		public const string msgModelMissing	 = "Missing MODEL{} information";
		public const string msgSaveSuccess	  = "File save Success";
		public const string msgSaveFailed	   = "File save Failed";
		public const string msgPartNameUsed	 = "Part name already used";
		public const string msgWarnModInternal  = "Multiple Internal not managed";
		public const string msgWarnModSeat	  = "Multiple Seats not managed";
		public const string msgWarnModSolPan	= "Multiple deployable Solar Panel not managed";
		public const string msgWarnModJetttison = "Multiple Jettison (Engine Fairing) does not work, only one fairing will act as such.";
		public const string msgWarnModAnimHeat  = "Multiple Animate Heat not managed";
		public const string msgWarnModEngine	= "Multiple Engine can cause proplem if not pointed to the same direction";
		public const string msgWarnModIntake	= "Multiple Resource Intake not managed";
		public const string msgWarnModAnimGen   = "Multiple Animate Generic with the same animationName is not supported by the game.";
		public const string msgWarnModDecouple  = "Multiple Decoupler not fully managed";
		public const string msgWarnModDocking   = "Multiple Docking port can bring issues";
		public const string msgWarnModRcs	   = "Multiple RCS welded cannot be use for rotation, Working well for translation.";
		public const string msgWarnModParachute = "Multiple Parachutes not managed";
		public const string msgWarnModLight	 = "Multiple Light not managed";
		public const string msgWarnModRetLadder = "Multiple Retractable Ladder not managed";
		public const string msgWarnModWheel	 = "Multiple Wheels not managed";
		public const string msgWarnModFxLookAt  = "Multiple Fx Look At Constraint not managed";
		public const string msgWarnModFxPos	 = "Multiple Fx Constraint Position not managed";
		public const string msgWarnModLaunClamp = "Multiple Launching Clamp will probably never be managed!";
		public const string msgWarnModUnknown   = "Module unknown so multiple not managed (if from a ksp update, let me know for a fix)";
		public const string msgWarnModFxAnimTh  = "Multiple FXModuleAnimateThrottle Does not work, only one animation will work";
		public const string msgWarnModScieExp   = "Multiple ModuleScienceExperiment with the same experimentID is not supported";
		public const string msgWarnModLandLegs  = "Multiple ModuleLandingLeg with the same animName is not supported by the game";

		//Weld
		public const string weldPartPath		= "GameData/UbioWeldingLtd/Parts/";
		public const string weldPartDefaultFile	= "/part.cfg";
		public const string weldAuthor		  = "UbioZurWeldingLtd";
		public const string weldManufacturer	= "UbioZur Welding Ltd";
		public const string weldDefaultName	 = "weldedpart";
		public const string weldDefaultTitle	= "My welded part";
		public const string weldDefaultDesc	 = "Warranty void during re-entry.";
		public const string weldPartNode		= "PART";
		public const string weldModelNode	   = "MODEL";
		public const string weldResNode		 = "RESOURCE";
		public const string weldModuleNode	  = "MODULE";
		public const string weldOutResNode	  = "OUTPUT_RESOURCE";
		public const string weldEngineProp	  = "PROPELLANT";
		public const string weldEngineAtmCurve  = "atmosphereCurve";
		public const string weldEngineVelCurve = "velocityCurve";
		public const string weldSubcat		  = "0";
		public const float weldRescaleFactor	= 1.0f;
		public const int weldDefaultPysicsSign  = -1;
		public const int weldDefaultEntryCost   = 0;

		//module name
		public const string modStockSas		 = "ModuleSAS";
		public const string modStockGear		= "ModuleLandingGear";
		public const string modStockReacWheel   = "ModuleReactionWheel";
		public const string modStockCommand	 = "ModuleCommand";
		public const string modStockGen		 = "ModuleGenerator";
		public const string modStockAltern	  = "ModuleAlternator";
		public const string modStockGimbal	  = "ModuleGimbal";
		public const string modStockSensor	  = "ModuleEnviroSensor";
		public const string modStockInternal	= "INTERNAL";
		public const string modStockSeat		= "KerbalSeat";
		public const string modStockSolarPan	= "ModuleDeployableSolarPanel";
		public const string modStockJettison	= "ModuleJettison";
		public const string modStockAnimHeat	= "ModuleAnimateHeat";
		public const string modStockEngine	  = "ModuleEngines";
		public const string modStockIntake	  = "ModuleResourceIntake";
		public const string modStockAnimGen	 = "ModuleAnimateGeneric";
		public const string modStockDecouple	= "ModuleDecouple";
		public const string modStockAnchdec	 = "ModuleAnchoredDecoupler";
		public const string modStockDocking	 = "ModuleDockingNode";
		public const string modStockRCS		 = "ModuleRCS";
		public const string modStockParachutes  = "ModuleParachute";
		public const string modStockLight	   = "ModuleLight";
		public const string modStockRetLadder   = "RetractableLadder";
		public const string modStockWheel	   = "ModuleWheel";
		public const string modStockFxLookAt	= "FXModuleLookAtConstraint"; //Come with wheels
		public const string modStockFxPos	   = "FXModuleConstrainPosition"; //come with wheels7
		public const string modStockFxAnimThro  = "FXModuleAnimateThrottle"; //ION animation throttle
		public const string modStockLaunchClamp = "LaunchClamp";
		public const string modStockScienceExp  = "ModuleScienceExperiment"; //.22 Science Experiment modules
		public const string modstockTransData   = "ModuleDataTransmitter";	//.22 Anteena
		public const string modStockLandingLegs = "ModuleLandingLeg"; // .22 Lanfding legs
		public const string modStockScienceCont = "ModuleScienceContainer"; // .22 Science Container

		//RDNodes
		public const string rdNodeExpRocket	 = "experimentalRocketry"; //For welded Propulsion
		public const string rdNodeNanoLaching   = "nanolathing"; //for structural
		public const string rdNodeExpAero	   = "experimentalAerodynamics"; //for pods
		public const string rdNodeAeroSpace	 = "aerospaceTech"; //for Aero
		public const string rdNodeExpElec	   = "experimentalElectrics"; //For Utility
		public const string rdNodeExpScience	= "experimentalScience"; //For Science
		public const string rdNodeAutomation	= "automation"; //Automation
		public const string rdNodeExpMotors	 = "experimentalsMotors"; //Experimental Motors
		public const string rdNodeByPass		= "advRocketry"; // bypass to make welding show up early
		public const string rdNodeSandboxWeld   = "sandboxWeld"; //For sandbox

		public const string curveKey = "key";

		public static string[] basicVector2CurveModules =
		{
			"atmosphereCurve"
		};

		public static string[] basicVector4CurveModules =
		{
			"velocityCurve"
		};

		public static string[] basicSubModules =
		{
			"RESOURCE",
			"OUTPUT_RESOURCE",
			"PROPELLANT"
		};

		public static string[] basicModulesToIgnore =
		{
			"TweakScale"
		};

		public static string[] basicAveragedModuleAttributes =
		{
			"FNAntimatterReactor_ReactorTemp",
			"FNAntimatterReactor_upgradedReactorTemp",
			"FNAntimatterReactor_radius",
			"ModuleScienceExperiment_interactionRange",
			"ModuleScienceExperiment_xmitDataScalar",
			"ModuleScienceLab_interactionRange",
			"ModuleScienceLab_dataTransmissionBoost",
			"ModuleScienceContainer_storageRange",
			"ModuleCommand_minimumCrew",
			"ModuleRCS_thrusterPower"
		};

		public static string[] basicUnchangedModuleAttributes =
		{
			"ModuleScienceLab_crewsRequired",
			"ModuleScienceLab_containerModuleIndex",
			"ModuleTestSubject_environments"
		};

		public static string[] basicBreakingModuleAttributes = 
		{
			"FNAntimatterReactor_radius",
			"ModuleScienceExperiment_experimentID"
		};

		public static string CommentOutText(string text)
		{
			return string.Concat(setupCommentBegin, text, setupCommentEnd);
		}

		public static GUIContent[] StrengthParamsCalcMethodsGUIContent = 
		{
			new GUIContent("Legacy", "Use UbioZur's method"),
			new GUIContent("Arithmetic mean", "Arithmetic mean between values of all parts"),
			new GUIContent("Weighted average", "Weighted average by mass of parts")
		};

		public static GUIContent[] MaxTempCalcMethodsGUIContent = 
		{
			new GUIContent("Arithmetic mean", "Arithmetic mean between values of all parts"),
			new GUIContent("Weighted average", "Weighted average by mass of parts"),
			new GUIContent("Lowest", "Lowest between MaxTemp values of parts")
		};
	}
}

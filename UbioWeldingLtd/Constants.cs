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
        public const string logVersion          = "v2.0pt5";
        public const string logWarning          = "WARNING ";
        public const string logError            = "ERROR ";
        public const string logPrefix           = "[WeldingTool] ";
        public const string logStartWeld        = "----- Starting Welding -----";
        public const string logEndWeld          = " ----- End Welding -----";
        public const string logNbPart           = " parts welded";
        public const string logWeldingPart      = "Welding this part: ";
        public const string logModelUrl         = "MODEL url: ";
        public const string logResMerge         = "RESOURCE add: ";
        public const string logResAdd           = "RESOURCE new: ";
        public const string logModMerge         = "MODULE merge: ";
        public const string logModAdd           = "MODULE add: ";
        public const string logModIgnore        = "MODULE ignore duplicate: ";
        public const string logFxAdd            = "FX Add: ";
        public const string logNodeAdd          = "Stack Node Add: ";
        public const string logWritingFile      = "Writing File: ";
        public const string logWarnNoMesh       = "Mesh value does not link to a mesh file";

        //GUI
        public const string guiWeldLabel        = "Weld it";
        public const string guiCancel           = " Cancel ";
        public const string guiOK               = " OK ";
        public const string guiSave             = " Save ";
        public const int guiWeldButWidth        = 70;
        public const int guiWeldButHeight       = 36;
        public const int guiDialogX             = 200;
        public const int guiDialogY             = 100;
        public const int guiDialogW             = 400;
        public const int guiDialogH             = 200;
        public const int guiInfoWindowX         = 300;
        public const int guiInfoWindowY         = 150;
        public const int guiInfoWindowW         = 600;
        public const int guiInfoWindowH         = 300;
        public const string guiDialFail         = "We are sorry to announce that our engineer could not perform this weld.\n Please read the report (ALt+F2 or ksp.log) for more details)";
        public const string guiDialWarn         = "After welding everything, out Engineer had some extra feature that they didn't knew where to put.\n Please read the report (ALt+F2 or ksp.log) for more details)";
        public const string guiNameUsed         = "Name already used by another part!";
        public const string guiDialOverwrite    = "File already exist, Do you want to overwrite it?";
        public const string guiDialSaved        = "New part saved and shipped!";

        //Settings
        public const string settingEdiButX      = "editorButtonXPosition";
        public const string settingEdiButY      = "editorButtonYPosition";
        public const string settingDbAutoReload = "dataBaseAutoReload";
        public const string settingAllNodes     = "includeAllNodes";
        public const string settingAllowCareer  = "allowCareerMode";
        public const string settingDontProcessMasslessParts = "dontProcessMasslessParts";
		public const string settingXmlFilePath = "/PluginData/UbioWeldingLtd/";
		public const string settingXmlConfigFileName = "config.xml";
		public static readonly string settingRuntimeDirectory = Assembly.GetExecutingAssembly().Location.Replace(new FileInfo(Assembly.GetExecutingAssembly().Location).Name, "");
        
        //Messages
        public const string msgSuccess          = "Welding is a success";
        public const string msgFail             = "Welding is a failure";
        public const string msgCfgMissing       = "Missing Config File";
        public const string msgModelMissing     = "Missing MODEL{} information";
        public const string msgSaveSuccess      = "File save Success";
        public const string msgSaveFailed       = "File save Failed";
        public const string msgPartNameUsed     = "Part name already used";
        public const string msgWarnModInternal  = "Multiple Internal not managed";
        public const string msgWarnModSeat      = "Multiple Seats not managed";
        public const string msgWarnModSolPan    = "Multiple deployable Solar Panel not managed";
        public const string msgWarnModJetttison = "Multiple Jettison (Engine Fairing) does not work, only one fairing will act as such.";
        public const string msgWarnModAnimHeat  = "Multiple Animate Heat not managed";
        public const string msgWarnModEngine    = "Multiple Engine can cause proplem if not pointed to the same direction";
        public const string msgWarnModIntake    = "Multiple Resource Intake not managed";
        public const string msgWarnModAnimGen   = "Multiple Animate Generic with the same animationName is not supported by the game.";
        public const string msgWarnModDecouple  = "Multiple Decoupler not fully managed";
        public const string msgWarnModDocking   = "Multiple Docking port can bring issues";
        public const string msgWarnModRcs       = "Multiple RCS welded cannot be use for rotation, Working well for translation.";
        public const string msgWarnModParachute = "Multiple Parachutes not managed";
        public const string msgWarnModLight     = "Multiple Light not managed";
        public const string msgWarnModRetLadder = "Multiple Retractable Ladder not managed";
        public const string msgWarnModWheel     = "Multiple Wheels not managed";
        public const string msgWarnModFxLookAt  = "Multiple Fx Look At Constraint not managed";
        public const string msgWarnModFxPos     = "Multiple Fx Constraint Position not managed";
        public const string msgWarnModLaunClamp = "Multiple Launching Clamp will probably never be managed!";
        public const string msgWarnModUnknown   = "Module unknown so multiple not managed (if from a ksp update, let me know for a fix)";
        public const string msgWarnModFxAnimTh  = "Multiple FXModuleAnimateThrottle Does not work, only one animation will work";
        public const string msgWarnModScieExp   = "Multiple ModuleScienceExperiment with the same experimentID is not supported";
        public const string msgWarnModLandLegs  = "Multiple ModuleLandingLeg with the same animName is not supported by the game";

        //Weld
        public const string weldPartPath        = "GameData/UbioWeldingLtd/Parts/";
        public const string weldPartFile        = "/part.cfg";
        public const string weldAuthor          = "UbioZurWeldingLtd";
        public const string weldManufacturer    = "UbioZur Welding Ltd";
        public const string weldDefaultName     = "weldedpart";
        public const string weldDefaultTitle    = "My welded part";
        public const string weldDefaultDesc     = "Warranty void during re-entry.";
        public const string weldPartNode        = "PART";
        public const string weldModelNode       = "MODEL";
        public const string weldResNode         = "RESOURCE";
        public const string weldModuleNode      = "MODULE";
        public const string weldOutResNode      = "OUTPUT_RESOURCE";
        public const string weldEngineProp      = "PROPELLANT";
        public const string weldEngineAtmCurve  = "atmosphereCurve";
        public const string weldEngineVelCurve = "velocityCurve";
        public const string weldSubcat          = "0";
        public const float weldRescaleFactor    = 1.0f;
        public const int weldDefaultPysicsSign  = -1;
        public const int weldDefaultEntryCost   = 0;

        //module name
        public const string modStockSas         = "ModuleSAS";
        public const string modStockGear        = "ModuleLandingGear";
        public const string modStockReacWheel   = "ModuleReactionWheel";
        public const string modStockCommand     = "ModuleCommand";
        public const string modStockGen         = "ModuleGenerator";
        public const string modStockAltern      = "ModuleAlternator";
        public const string modStockGimbal      = "ModuleGimbal";
        public const string modStockSensor      = "ModuleEnviroSensor";
        public const string modStockInternal    = "INTERNAL";
        public const string modStockSeat        = "KerbalSeat";
        public const string modStockSolarPan    = "ModuleDeployableSolarPanel";
        public const string modStockJettison    = "ModuleJettison";
        public const string modStockAnimHeat    = "ModuleAnimateHeat";
        public const string modStockEngine      = "ModuleEngines";
        public const string modStockIntake      = "ModuleResourceIntake";
        public const string modStockAnimGen     = "ModuleAnimateGeneric";
        public const string modStockDecouple    = "ModuleDecouple";
        public const string modStockAnchdec     = "ModuleAnchoredDecoupler";
        public const string modStockDocking     = "ModuleDockingNode";
        public const string modStockRCS         = "ModuleRCS";
        public const string modStockParachutes  = "ModuleParachute";
        public const string modStockLight       = "ModuleLight";
        public const string modStockRetLadder   = "RetractableLadder";
        public const string modStockWheel       = "ModuleWheel";
        public const string modStockFxLookAt    = "FXModuleLookAtConstraint"; //Come with wheels
        public const string modStockFxPos       = "FXModuleConstrainPosition"; //come with wheels7
        public const string modStockFxAnimThro  = "FXModuleAnimateThrottle"; //ION animation throttle
        public const string modStockLaunchClamp = "LaunchClamp";
        public const string modStockScienceExp  = "ModuleScienceExperiment"; //.22 Science Experiment modules
        public const string modstockTransData   = "ModuleDataTransmitter";    //.22 Anteena
        public const string modStockLandingLegs = "ModuleLandingLeg"; // .22 Lanfding legs
        public const string modStockScienceCont = "ModuleScienceContainer"; // .22 Science Container

        //RDNodes
        public const string rdNodeExpRocket     = "experimentalRocketry"; //For welded Propulsion
        public const string rdNodeNanoLaching   = "nanolathing"; //for structural
        public const string rdNodeExpAero       = "experimentalAerodynamics"; //for pods
        public const string rdNodeAeroSpace     = "aerospaceTech"; //for Aero
        public const string rdNodeExpElec       = "experimentalElectrics"; //For Utility
        public const string rdNodeExpScience    = "experimentalScience"; //For Science
        public const string rdNodeAutomation    = "automation"; //Automation
        public const string rdNodeExpMotors     = "experimentalsMotors"; //Experimental Motors
        public const string rdNodeByPass        = "advRocketry"; // bypass to make welding show up early
        public const string rdNodeSandboxWeld   = "sandboxWeld"; //For sandbox
    }
}

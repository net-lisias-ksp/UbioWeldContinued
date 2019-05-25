using System;
using System.Collections.Generic;
using UnityEngine;
using KSP.UI.Screens;

using KSPe.IO;
using D = KSPe.IO.Data;     // KSP/PluginData/net.lisias.ksp/UbioWeldingLtd/*
using L = KSPe.IO.Local;    // KSP/GameData/__LOCAL/net.lisias.ksp/UbioWeldingLtd/*
using GUILayout = KSPe.UI.GUILayout;
using GUI = KSPe.UI.GUI;

namespace UbioWeldingLtd
{
	//UbioZurWeldingLtd Class
	[KSPAddon(KSPAddon.Startup.EditorAny, false)]
	public class UbioZurWeldingLtd : MonoBehaviour
	{

		enum DisplayState
		{
			none,
			weldError,
			weldWarning,
			infoWindow,
			savedWindow,
			overwriteDial,
			mainWindow,
			partSelection
		}

		private DisplayState __state;
		private DisplayState state {
			set {
				Log.dbgGui(this, "New Main Window State: {0}", __state.ToString());
				__state = value;
			}
			get { return __state; }
		}

		public static UbioZurWeldingLtd instance { get; private set; }
		private Rect[] _guiInfoWindowColoumns = new Rect[4];
		private Rect _editorErrorDial;
		private Rect _editorWarningDial;
		private Rect _editorInfoWindow;
		private Rect _editorOverwriteDial;
		private Rect _editorSavedDial;
		private Rect _editorMainWindow;
		private Welder _welder;
		private List<GUIContent> _catNames = new List<GUIContent>();
		private GUIDropdown _catDropdown;
		private GUIDropdown _techDropdown;
		private GUIDropdown _vesselTypeDropdown;
		private GUIStyle _guiStyle = new GUIStyle();
		private GUISkin _guiskin = HighLogic.Skin;
		private Vector2 _scrollRes;
		private Vector2 _scrollMod;
		private Vector2 _settingsScrollPosition = Vector2.zero;
		private Part _currentSelectedPartbranch;
		private Part _previousSelectedPartbranch;
		private Part _selectedPartbranch;
		private RaycastHit _hit;
		private Ray _ray;
		private EditorFacility _editorFacility;
		private EditorToolbar _editorToolbar;

		private AdvancedGUITextArea _textAreaDescription = new AdvancedGUITextArea();
		private AdvancedGUITextField _textFieldTitle = new AdvancedGUITextField();
		private AdvancedGUITextField _textFieldName = new AdvancedGUITextField();
		private WeldingConfiguration _config;
		private bool _guiVisible = false;
		private bool _mainWindowsSettingsMode = false;
		static public bool isReloading = false;
		private string welding_pathname => _config.useNamedCfgFile
						  ? string.Format("{0}/{1}/{1}.cfg", _welder.Category.ToString(), _welder.Name)
						  : string.Format("{0}/{1}/{2}.cfg", _welder.Category.ToString(), _welder.Name, Constants.weldPartDefaultFile)
			  ;

		public Rect editorInfoWindow
		{
			get { return _editorInfoWindow; }
		}

		public GUISkin guiskin
		{
			get { return _guiskin; }
		}

		public GUIStyle guistyle
		{
			get { return _guiStyle; }
		}

		/// <summary>
		/// access to the config of the whole tool
		/// </summary>
		public WeldingConfiguration config
		{
			get { return _config; }
		}

		public Part selectedPartBranch
		{
			get { return _selectedPartbranch; }
		}

		/*
		 * Called when plug in loaded
		 */
		public void Awake()
		{
			try {
				this.HandleAwake();
				this._editorToolbar = new EditorToolbar(this);
			} catch (Exception e) {
				Log.ex(this, e);
			} finally {
				Log.dbgGui(this, "Awake handled.");
			}
		}

		private void HandleAwake()
		{
			instance = this;
			Log.dbg("Platform is {0}", Application.platform);

			initConfig();
			this.state = DisplayState.none;
			_editorErrorDial = new Rect(Screen.width / 2 - Constants.guiDialogX, Screen.height / 2 - Constants.guiDialogY, Constants.guiDialogW, Constants.guiDialogH);
			_editorWarningDial = new Rect(Screen.width / 2 - Constants.guiDialogX, Screen.height / 2 - Constants.guiDialogY, Constants.guiDialogW, Constants.guiDialogH);
			_editorInfoWindow = new Rect(Screen.width / 2 - Constants.guiInfoWindowX, Screen.height / 2 - Constants.guiInfoWindowY, Constants.guiInfoWindowW, Constants.guiInfoWindowH);
			_editorOverwriteDial = new Rect(Screen.width / 2 - Constants.guiDialogX, Screen.height / 2 - Constants.guiDialogY, Constants.guiDialogW, Constants.guiDialogH);
			_editorSavedDial = new Rect(Screen.width / 2 - Constants.guiDialogX, Screen.height / 2 - Constants.guiDialogY, Constants.guiDialogW, Constants.guiDialogH);
			_editorMainWindow = new Rect(_config.MainWindowXPosition, _config.MainWindowYPosition, Constants.guiMainWindowW, Constants.guiMainWindowH);

			_catNames = WeldingHelpers.initPartCategories(_catNames);
			_guiStyle = WeldingHelpers.initGuiStyle(_guiStyle);
			_catDropdown = WeldingHelpers.initDropDown(_catNames, _guiStyle, _catDropdown);
			DatabaseHandler.initMMAssembly();
		}


		/*
		 * Called when plug in is unloaded
		 */
		public void OnDisable()
		{
			try {
				this.HandleDisable();
				this._editorToolbar.OnDisable(); this._editorToolbar = null;
			} catch (Exception e) {
				Log.ex(this, e);
			} finally {
				Log.dbgGui(this, "OnDisable handled.");
			}
		}

		private void HandleDisable()
		{
		}

		/// <summary>
		/// Welds the whole active craft in the scene in case the stocktoolbar is used
		/// </summary>
		public void HandleToolbarButtonUsed()
		{
			try
			{
				if (!EditorLockManager.isEditorLocked())
				{
					if (EditorLogic.RootPart != null)
					{
						if (this.state != DisplayState.mainWindow)
						{
							this.state = DisplayState.mainWindow;
						}
						else
						{
							closeMainwindow();
						}
					} else {
						Log.dbgGui(this, "EditorLogic.RootPart == null!");
					}
				} else {
					Log.dbgGui(this, "EditorLockManager.isEditorLocked()!");
				}
			}
			catch (Exception e)
			{
				Log.ex(this, e);
			}
			finally
			{
				Log.dbgGui(this, "HandleToolbarButtonUsed handled.");
			}
		}

		/// <summary>
		/// closes the mainwindow clean
		/// </summary>
		private void closeMainwindow()
		{
			this.state = DisplayState.none;
			disablePartHighlight(_selectedPartbranch);
			_selectedPartbranch = null;
		}

		/// <summary>
		/// Loads the config for the Welding or prepares default values and generates a new config
		/// </summary>
		private void initConfig()
		{
			D.PluginConfiguration oldConfig = D.PluginConfiguration.CreateForType<OldWeldingPluginConfig>(Constants.settingXmlOldConfigFileName);
			bool oldConfigFound = oldConfig.exists();
			if (oldConfigFound)
			{
				oldConfig = D.PluginConfiguration.CreateForType<OldWeldingPluginConfig>();
				oldConfig.load();
				oldConfig.delete();
				Log.dbg("old configfile found and deleted");
			}

			D.PluginConfiguration config = D.PluginConfiguration.CreateForType<OldWeldingPluginConfig>(Constants.settingXmlConfigFileName);
			if (!config.exists())
			{
				_config = new WeldingConfiguration();
				FileManager.saveConfig(_config);
				_config.vector2CurveModules = Constants.basicVector2CurveModules;
				_config.vector4CurveModules = Constants.basicVector4CurveModules;
				_config.subModules = Constants.basicSubModules;
				_config.modulesToIgnore = Constants.basicModulesToIgnore;
				_config.averagedModuleAttributes = Constants.basicAveragedModuleAttributes;
				_config.unchangedModuleAttributes = Constants.basicUnchangedModuleAttributes;
				_config.breakingModuleAttributes = Constants.basicBreakingModuleAttributes;
			}
			else
			{
				_config = FileManager.loadConfig();
			}

			_config.dataBaseAutoReload = oldConfigFound ? oldConfig.GetValue<bool>(Constants.settingDbAutoReload) : _config.dataBaseAutoReload;
			_config.allowCareerMode = oldConfigFound ? oldConfig.GetValue<bool>(Constants.settingAllowCareer) : _config.allowCareerMode;
			Welder.includeAllNodes = oldConfigFound ? oldConfig.GetValue<bool>(Constants.settingAllNodes) : _config.includeAllNodes;
			Welder.dontProcessMasslessParts = oldConfigFound ? oldConfig.GetValue<bool>(Constants.settingDontProcessMasslessParts) : _config.dontProcessMasslessParts;

			Welder.StrengthCalcMethod = oldConfigFound ? StrengthParamsCalcMethod.ArithmeticMean : _config.StrengthCalcMethod;
			Welder.MaxTempCalcMethod = oldConfigFound ? MaxTempCalcMethod.ArithmeticMean : _config.MaxTempCalcMethod;
			Welder.runInTestMode = oldConfigFound ? false : _config.runInTestMode;
			Welder.precisionDigits = oldConfigFound ? 6 : _config.precisionDigits;
			Welder.fileSimplification = oldConfigFound ? false : _config.fileSimplification;
		}

		/*
		 * Called once everything in scene is loaded
		 */
		public void Start()
		{
			try {
				this._guiVisible = (
					null != EditorLogic.fetch 
					&& (_config.allowCareerMode || !_config.allowCareerMode && HighLogic.fetch.currentGame.Mode != Game.Modes.CAREER)
				);
				EditorLockManager.resetEditorLocks();
				_editorFacility = EditorDriver.editorFacility;
				this._editorToolbar.Start();
			} catch (Exception e) {
				Log.ex(this, e);
			} finally {
				Log.dbgGui(this, "Start handled");
			}
		}

		/// <summary>
		/// Unity default function for stuff that happens every frame
		/// </summary>
		public void Update() {
			try {
				this.HandleUpdate();
				this._editorToolbar.Update();
			} catch (Exception e) {
				Log.ex(this, e);
			} finally {
				Log.dbgGui(this, "Update handled.");
			}
		}

		private void HandleUpdate()
		{
			switch (this.state)
			{
				case DisplayState.none:
					break;

				case DisplayState.partSelection:
					_ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					if (Physics.Raycast(_ray, out _hit))
					{
						_currentSelectedPartbranch = _hit.transform.gameObject.GetComponent<Part>() as Part;
						if (_previousSelectedPartbranch != null && _previousSelectedPartbranch != _currentSelectedPartbranch)
						{
							disablePartHighlight(_previousSelectedPartbranch);
						}
						enablePartHighlight(_currentSelectedPartbranch);
						_previousSelectedPartbranch = _currentSelectedPartbranch;
						if (Input.GetKeyUp(KeyCode.Mouse0) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
						{
							_selectedPartbranch = _currentSelectedPartbranch;
							_selectedPartbranch.SetHighlightType(Part.HighlightType.AlwaysOn);
							_currentSelectedPartbranch = null;
							_previousSelectedPartbranch = null;
							this.state = DisplayState.mainWindow;
						}
					}
				    break;
				
				default:
					if (_selectedPartbranch != null)
					{
						enablePartHighlight(_selectedPartbranch);
					}
					break;
			}
		}


		/// <summary>
		/// the Unity default method to draw any GUI on the Screen.
		/// </summary>
		public void OnGUI()
		{
			try
			{
				this.HandleGUI();
			}
			catch (Exception e)
			{
				Log.ex(this, e);
			}
			finally
			{
				Log.dbgGui(this, "OnGUI handled.");
			}
		}

		/// <summary>
		/// highlights the part at mouseover
		/// </summary>
		/// <param name="part"></param>
		private void enablePartHighlight(Part part)
		{
			if (part != null)
			{
				part.SetHighlightType(Part.HighlightType.OnMouseOver);
				part.SetHighlightColor(Color.magenta);
				part.SetHighlight(true, true);
			}
		}

		private void disablePartHighlight(Part part)
		{
			if (part != null)
			{
				part.SetHighlightDefault();
			}
		}


		/// <summary>
		/// Public Eventcall at the GuiDraw
		/// </summary>
		private void HandleGUI()
		{
			if (!_guiVisible) {
				//TODO: Play a "uh-uh" (nops) sound.
				Log.dbgGui(this, "GUI is set to invisible!");
				return;
			}

			GUI.skin = _guiskin;
			switch (this.state)
			{
				case DisplayState.none:
					EditorLockManager.unlockEditor(Constants.settingPreventClickThroughLock);
					EditorLockManager.unlockEditor(Constants.settingWeldingLock);
					break;
				case DisplayState.weldError:
					_editorErrorDial = GUILayout.Window((int)this.state, _editorErrorDial, OnErrorDisplay, Constants.weldManufacturer);
					break;
				case DisplayState.weldWarning:
					_editorWarningDial = GUILayout.Window((int)this.state, _editorWarningDial, OnWarningDisplay, Constants.weldManufacturer);
					break;
				case DisplayState.infoWindow:
					_editorInfoWindow =  GUI.Window((int)this.state, _editorInfoWindow, OnInfoWindow, Constants.weldManufacturer);
					PreventClickThrough(_editorInfoWindow);
					break;
				case DisplayState.savedWindow:
					_editorSavedDial = GUILayout.Window((int)this.state, _editorSavedDial, OnSavedDisplay, Constants.weldManufacturer);
					break;
				case DisplayState.overwriteDial:
					_editorOverwriteDial = GUILayout.Window((int)this.state, _editorOverwriteDial, OnOverwriteDisplay, Constants.weldManufacturer);
					break;
				case DisplayState.mainWindow:
					_editorMainWindow =  GUI.Window((int)this.state, _editorMainWindow, OnMainWindow, Constants.weldManufacturer);
					PreventClickThrough(_editorMainWindow);
					break;
				case DisplayState.partSelection:
					ScreenMessages.PostScreenMessage(Constants.guiScreenMessagePartSelection, Time.deltaTime, ScreenMessageStyle.UPPER_CENTER);
					break;
			}
		} //private void OnDraw()

		private void weldPart(Part partToWeld)
		{
			//Lock editor
			EditorLockManager.lockEditor(Constants.settingWeldingLock);

			//process the welding
#if (DEBUG)
			Debug.ClearDeveloperConsole();

			Log.info("{0}", Constants.logVersion);
			Log.info("{0}", Constants.logStartWeld);
#endif
			bool warning = false;
			_welder = new Welder();
			_welder.init();

			partToWeld.transform.eulerAngles = Vector3.zero;
			WeldingReturn ret = 0;

			if (!WeldingHelpers.DoesTextContainRegex(partToWeld.name, "strutConnector"))
			{
				ret = _welder.weldThisPart(partToWeld);
			}

			if (ret < 0)
			{
				Log.dbg("{0}", Constants.logEndWeld);
				this.state = DisplayState.weldError;
				return;
			}
			else
			{
				warning = warning || (0 < ret);
			}

			Part[] children = partToWeld.FindChildParts<Part>(true);

			if (children != null)
			{
				foreach (Part child in children)
				{
					if (!WeldingHelpers.DoesTextContainRegex(child.name, "strutConnector"))
					{
						ret = _welder.weldThisPart(child);
					}

					if (ret< 0)
					{
						Log.dbg("{0}", Constants.logEndWeld);
						this.state = DisplayState.weldError;
						return;
					}
					else
					{
						warning = warning || (0 < ret);
					}
				}
			}
			_welder.processNewCoM();

			_welder.prepDecals(_welder.moduleList);
			if (_welder.isMeshSwitchRequired)
			{
				_welder.prepareWeldedMeshSwitchModule(_welder.moduleList);
			}

			_techDropdown = WeldingHelpers.initTechDropDown(_welder.techList, _guiStyle, _techDropdown);

			if (_welder.vesselTypeList.Count > 0)
			{
				_vesselTypeDropdown = WeldingHelpers.initVesselTypeDropDown(_welder.vesselTypeList, _guiStyle, _vesselTypeDropdown);
			}

			_scrollMod = Vector2.zero;
			_scrollRes = Vector2.zero;

			Log.dbg("| {0} Parts welded", _welder.NbParts);

			if (warning)
			{
				Log.dbg(Constants.logEndWeld);
				this.state = DisplayState.weldWarning;
			}
			else
			{
				Log.dbg("welder.Category: {0}", (int)_welder.Category);
				_catDropdown.SelectedItemIndex = (int)_welder.Category;
				this.state = DisplayState.infoWindow;
			}
		}


		private void OnMainWindow(int windowID) {
			try {
				this.HandleMainWindow(windowID);
			} catch (Exception e) {
				Log.ex(this, e);
			} finally {
				Log.dbgGui(this, "OnMainWindow handled.");
			}
		}

		/*
		 * Main window
		 */
		private void HandleMainWindow(int windowID)
		{
			GUIStyle _settingsToggleGroupStyle = new GUIStyle(GUI.skin.toggle);
			_settingsToggleGroupStyle.margin.left += 40;

			//save Window Position
			_config.MainWindowXPosition = (int)_editorMainWindow.xMin;
			_config.MainWindowYPosition = (int)_editorMainWindow.yMin;

			GUILayout.BeginVertical();
			GUILayout.EndVertical();
			GUILayout.BeginVertical();

			if (GUILayout.Button(Constants.guiSettingsGUIContent, GUILayout.MaxWidth(160)))
			{
				_mainWindowsSettingsMode = !_mainWindowsSettingsMode;
			}
			//Settings
			if (_mainWindowsSettingsMode)
			{
				_editorMainWindow.height = Constants.guiMainWindowHSettingsExpanded;
				_settingsScrollPosition = GUILayout.BeginScrollView(_settingsScrollPosition);
				_config.includeAllNodes = GUILayout.Toggle(_config.includeAllNodes, Constants.guiAllNodesGUIContent);
				Welder.includeAllNodes = _config.includeAllNodes;
				_config.dontProcessMasslessParts = GUILayout.Toggle(_config.dontProcessMasslessParts, Constants.guiDontProcessMasslessPartsGUIContent);
				Welder.dontProcessMasslessParts = _config.dontProcessMasslessParts;
				_config.dataBaseAutoReload = GUILayout.Toggle(_config.dataBaseAutoReload, Constants.guiDbAutoReloadGUIContent);
				_config.useNamedCfgFile = GUILayout.Toggle(_config.useNamedCfgFile, Constants.guiUseNamedCfgFileGUIContent);
				_config.advancedDebug = GUILayout.Toggle(_config.advancedDebug, Constants.guiAdvancedDebugGUIContent);
				_config.clearEditor = GUILayout.Toggle(_config.clearEditor, Constants.guiClearEditorGUIContent);
				_config.fileSimplification = GUILayout.Toggle(_config.fileSimplification, Constants.guiFileSimplificationGUIContent);
				Welder.fileSimplification = _config.fileSimplification;
				GUILayout.Space(10.0f);
				GUILayout.Label(" Vector Precision: " + _config.precisionDigits);
				_config.precisionDigits = (int)GUILayout.HorizontalSlider(_config.precisionDigits, 1, 6);
				Welder.precisionDigits = _config.precisionDigits;
				GUILayout.Space(10.0f);
				GUILayout.Label("Strength params calculation method");
//				_config.StrengthCalcMethod = (StrengthParamsCalcMethod)GUILayout.SelectionGrid((int)_config.StrengthCalcMethod, Constants.StrengthParamsCalcMethodsGUIContent, 1, GUILayout.MaxWidth(140));
				foreach (StrengthParamsCalcMethod method in Enum.GetValues(typeof(StrengthParamsCalcMethod)))
				{
					if (GUILayout.Toggle((_config.StrengthCalcMethod == method), Constants.StrengthParamsCalcMethodsGUIContent[(int)method], _settingsToggleGroupStyle))
					{
						_config.StrengthCalcMethod = method;
						Welder.StrengthCalcMethod = method;
					}
				}
				GUILayout.Space(10.0f);
				GUILayout.Label("MaxTemp calculation method");
//				_config.MaxTempCalcMethod = (MaxTempCalcMethod)GUILayout.SelectionGrid((int)_config.MaxTempCalcMethod, Constants.MaxTempCalcMethodsGUIContent, 1, GUILayout.MaxWidth(140));
				foreach (MaxTempCalcMethod method in Enum.GetValues(typeof(MaxTempCalcMethod)))
				{
					if (GUILayout.Toggle((_config.MaxTempCalcMethod == method), Constants.MaxTempCalcMethodsGUIContent[(int)method], _settingsToggleGroupStyle))
					{
						_config.MaxTempCalcMethod = method;
						Welder.MaxTempCalcMethod = method;
					}
				}
				GUILayout.EndScrollView();

//				GUILayout.Space(10.0f);
				if (GUILayout.Button(Constants.guiSaveSettingsButtonGUIContent, GUILayout.MaxWidth(160)))
				{
					FileManager.saveConfig(_config);
					_config = FileManager.loadConfig();
				}
			}
			else
			{
				_editorMainWindow.height = Constants.guiMainWindowH;
				GUILayout.Space(20.0f);
			}

			//SelectPArtbranch button
			if (GUILayout.RepeatButton(Constants.guiSelectPartGUIContent, GUILayout.MaxWidth(160)))
			{
				this.state = DisplayState.partSelection;
			}

			//Weld button
			if (GUILayout.Button(Constants.guiWeldItButtonGUIContent, GUILayout.MaxWidth(160)))
			{
				FileManager.saveConfig(_config);

				if (EditorLockManager.isEditorLocked())
				{
					if (_selectedPartbranch == null)
					{
						_selectedPartbranch = EditorLogic.RootPart;
					}
					repositionPreWeldment(_selectedPartbranch);
					weldPart(_selectedPartbranch);
				}
			}
			if (GUILayout.Button(Constants.guiCloseGUIContent, GUILayout.MaxWidth(160)))
			{
				closeMainwindow();
			}
			//Hints area
			GUILayout.TextArea(GUI.tooltip, GUILayout.ExpandHeight(true), GUILayout.MaxHeight(100));
			GUIStyle VersionLabelGUIStyle = new GUIStyle(GUI.skin.label);
			VersionLabelGUIStyle.fontSize = 12;
			GUILayout.Label(Constants.logVersion, VersionLabelGUIStyle);
			GUILayout.EndVertical();

			GUI.DragWindow();
		} //private void OnMainWindow()



		/// <summary>
		/// resets the partbranch in the editor to a position that is centered at x and z
		/// </summary>
		/// <param name="rootpart"></param>
		private static void repositionPreWeldment(Part rootpart)
		{
			float yPos = EditorLogic.fetch.ship.shipSize.y + EditorLogic.fetch.initialPodPosition.y;
			rootpart.transform.position = new Vector3(0, yPos, 0);
		}


		private void OnErrorDisplay(int windowID) {
			try {
				this.HandleErrorDisplay(windowID);
			} catch (Exception e) {
				Log.ex(this, e);
			} finally {
				Log.dbgGui(this, "OnErrorDisplay handled.");
			}
		}

		/*
		 * Error Message
		 */
		private void HandleErrorDisplay(int windowID)
		{
			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
			GUILayout.Label(Constants.guiDialFail);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button(Constants.guiOK))
			{
				EditorLockManager.unlockEditor(Constants.settingWeldingLock);
				this.state = DisplayState.none;
			}
			GUILayout.EndVertical();

			GUI.DragWindow();
		} //private void OnErrorDisplay()

		private void OnWarningDisplay(int windowID) {
			try {
				this.HandleWarningDisplay(windowID);
			} catch (Exception e) {
				Log.ex(this, e);
			} finally {
				Log.dbgGui(this, "OnWarningDislay handled.");
			}
		}

		/*
		 * Warning Message
		 */
		private void HandleWarningDisplay(int windowID)
		{
			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
			GUILayout.Label(Constants.guiDialWarn);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button(Constants.guiOK))
			{
				this.state = DisplayState.infoWindow;
			}
			GUILayout.EndVertical();

			GUI.DragWindow();
		} //private void OnErrorDisplay()

		private void OnOverwriteDisplay(int windowID) {
			try {
				this.OnOverwriteDisplay(windowID);
			} catch (Exception e) {
				Log.ex(this, e);
			} finally {
				Log.dbgGui(this, "OnOverwriteDisplay handled.");
			}
		}

		/*
		 * Overwrite Message
		 */
		private void HandleOverwriteDisplay(int windowID)
		{
			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
			GUILayout.Label(Constants.guiDialOverwrite);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label(this.welding_pathname);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button(Constants.guiOK))
			{
				WriteCfg();
				this.state = DisplayState.savedWindow;
			}
			if (GUILayout.Button(Constants.guiCancel))
			{
				this.state = DisplayState.infoWindow;
			}
			GUILayout.EndVertical();

			GUI.DragWindow();
		} //private void OnErrorDisplay()

		private void OnSavedDisplay(int windowID) {
			try {
				this.HandleSavedDisplay(windowID);
			} catch (Exception e) {
				Log.ex(this, e);
			} finally {
				Log.dbgGui(this, "OnSavedDisplay handled.");
			}
		}

		/*
		 * Saved Message
		 */
		private void HandleSavedDisplay(int windowID)
		{
			bool MMPathLoaderIsReady = DatabaseHandler.isModuleManagerInstalled ? (bool)DatabaseHandler.DynaInvokeMMPatchLoaderMethod("IsReady") : false;
			GUILayout.BeginVertical();
			if (DatabaseHandler.isReloading)
			{
				ScreenMessages.PostScreenMessage(string.Concat(Constants.guiDBReloading1, "\n", Constants.guiDBReloading2), Time.deltaTime, ScreenMessageStyle.UPPER_CENTER);
				GUILayout.Label(Constants.guiDBReloading1);
				GUILayout.Label(Constants.guiDBReloading2);
				if (!MMPathLoaderIsReady)
				{
					GUILayout.Label(String.Format("ModuleManager progress: {0:P0}", (float)DatabaseHandler.DynaInvokeMMPatchLoaderMethod("ProgressFraction")));
//					GUILayout.Label(String.Format("{0}", (string)DatabaseHandler.DynaInvokeMMPatchLoaderMethod("ProgressTitle")));
				}
			}
			else
			{
				GUILayout.Label(Constants.guiDialSaved);
				GUILayout.FlexibleSpace();
				if (GUILayout.Button(Constants.guiOK))
				{
					this.state = DisplayState.none;
					ClearEditor();
				}
			}
			GUILayout.EndVertical();

			GUI.DragWindow();
		} //private void OnErrorDisplay()

		private void OnInfoWindow(int windowID) {
			try {
				this.HandleInfoWindow(windowID);
			} catch (Exception e) {
				Log.ex(this, e);
			} finally {
				Log.dbgGui(this, "OnHandleInfoWindow handled.");
			}
		}

		/// <summary>
		/// Darws the Info window where the new saved partfile can be configured
		/// </summary>
		/// <param name="windowID"></param>
		private void HandleInfoWindow(int windowID)
		{
			float margin = 5f;
			float height = 20;
			float posH = height;
			float columnWidth = (Constants.guiInfoWindowW - (margin * 5)) / 4;
			float columnHeight = (Constants.guiInfoWindowH - ((height + margin) * 2));
			float scrollwidth = columnWidth - 20.0f;
			GUIStyle style = new GUIStyle();

			for (int i = 0; i < 4; i++)
			{
				_guiInfoWindowColoumns[i] = new Rect((margin * (i + 1)) + (columnWidth * i), height, columnWidth, columnHeight);
				posH = height;
				switch (i)
				{
					case 0:
						{
							GUI.Label(new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, height), "Name:");
							posH += height + margin;
							//_welder.Name = _textFieldTitle.DrawAdvancedGUITextField(new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, height), _welder.Name, 100, (int)this.state);
							_welder.Name = GUI.TextField(new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, height), _welder.Name, 100);
							posH += height + margin;
							GUI.Label(new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, height), "Title:");
							posH += height + margin;
							//_welder.Title = _textFieldTitle.DrawAdvancedGUITextField(new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, height), _welder.Title, 100, (int)this.state);
							_welder.Title = GUI.TextField(new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, height), _welder.Title, 100);
							posH += height + margin;
							GUI.Label(new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, height), "Description:");
							posH += height + margin;
							//_welder.Description = _textAreaDescription.DrawAdvancedGUITextArea(new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, 7 * height + 6 * margin), _welder.Description, 600, (int)this.state);
							_welder.Description = GUI.TextArea(new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, 8 * height + 7 * margin), _welder.Description, 600);
							posH += 8 * height + 8 * margin;
							GUI.Label(new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, height), "Symmetry:");
							posH += height + margin;
							_welder.stringStackSymmetry = GUI.TextField(new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, height), _welder.stringStackSymmetry, 100);
						}
						break;
					case 1:
						{
							GUI.Label(new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, height), "Category:");
							posH += height + margin;
							Rect _cetegoryBox = new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, height);
							posH += height + margin;
							GUI.Label(new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, height), "RequiredTech:");
							posH += height + margin;
							Rect _techBox = new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, height);
							posH += height + margin;
							Rect _vesselTypeBox = new Rect();
							if (_welder.vesselTypeList.Count > 0)
							{
								GUI.Label(new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, height), "VesselType:");
								posH += height + margin;
								_vesselTypeBox = new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, height);
								posH += height + margin;
							}
							GUI.Label(new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, height), string.Format("Nb Parts: {0}", _welder.NbParts));
							posH += height + margin;
							GUI.Label(new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, height), string.Format("Cost: {0:F2}", _welder.Cost));
							posH += height + margin;
							GUI.Label(new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, height), string.Format("Mass: {0:F3} / {1:F3}", _welder.Mass, _welder.WetMass));
							posH += height + margin;
							GUI.Label(new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, height), string.Format("Temp: {0:F1}", _welder.MaxTemp));
							posH += height + margin;
							GUI.Label(new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, height), string.Format("B Force: {0:F3}", _welder.BreakingForce));
							posH += height + margin;
							GUI.Label(new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, height), string.Format("B Torque: {0:F3}", _welder.BreakingTorque));
							posH += height + margin;
							GUI.Label(new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, height), string.Format("Drag: {0:F3} / {1:F3}", _welder.MinDrag, _welder.MaxDrag));

							if (_techDropdown.IsOpen || _catDropdown.IsOpen)
							{
								GUI.Box(_vesselTypeBox, _welder.vesselType);
							}
							else
							{
								if (_welder.vesselTypeList.Count > 0)
								{
									_welder.vesselType = _welder.vesselTypeList[_vesselTypeDropdown.Show(_vesselTypeBox)];
								}
							}

							if (_catDropdown.IsOpen)
							{
								GUI.Box(_techBox, _welder.techRequire);
							}
							else
							{
								_welder.techRequire = _welder.techList[_techDropdown.Show(_techBox)];
							}
							_catDropdown.SelectedItemIndex = (int)_welder.Category;
							_welder.Category = (PartCategories)_catDropdown.Show(_cetegoryBox);
						}
						break;
					case 2:
						{
							GUI.Label(new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, height), "Modules:");
							posH += height + margin;
							string[] modulenames = _welder.Modules;
							_scrollMod = GUI.BeginScrollView(new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, (height * 14 + margin * 13)), _scrollMod, new Rect(0, 0, scrollwidth, modulenames.Length > 14 ? 345 + (modulenames.Length - 14) * (height + margin) : 345), false, true);
							style.wordWrap = false;
							style.normal.textColor = Color.white;
							posH = 0;
							foreach (string modulename in modulenames)
							{
								GUI.Label(new Rect(2, posH, scrollwidth, height), modulename, style);
								posH += height + margin;
							}
							GUI.EndScrollView();
						}
						break;
					case 3:
						{
							GUI.Label(new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, height), "Resources:");
							posH += height + margin;
							string[] resourcesdata = _welder.Resources;
							_scrollRes = GUI.BeginScrollView(new Rect(_guiInfoWindowColoumns[i].x, posH, columnWidth, (height * 14 + margin * 13)), _scrollRes, new Rect(0, 0, scrollwidth, resourcesdata.Length > 14 ? 345 + (resourcesdata.Length - 14) * (height + margin) : 345), false, true);
							style.wordWrap = false;
							style.normal.textColor = Color.white;
							posH = 0;
							foreach (string resname in resourcesdata)
							{
								GUI.Label(new Rect(2, posH, scrollwidth, height), resname, style);
								posH += height + margin;
							}
							GUI.EndScrollView();
						}
						break;
					default:
						{
							GUI.Label(new Rect(0, posH, columnWidth, height), "Broken GUI Column:");
						}
						break;
				}
			}

			if (!WelderNameNotUsed())
			{
				style.normal.textColor = Color.red;
				GUI.Label(new Rect(margin, columnHeight + margin, columnWidth * 1.5f, height), Constants.guiNameUsed, style);
			}
			if (!isSymmetryNumber())
			{
				style.normal.textColor = Color.red;
				GUI.Label(new Rect(margin, height + columnHeight + margin, columnWidth * 1.5f, height), Constants.guiSymmetryNotNumbered, style);
			}


			if (!string.IsNullOrEmpty(_welder.Name))
			{
				if (GUI.Button(new Rect(_guiInfoWindowColoumns[1].x + columnWidth * 0.5f, height + columnHeight + margin, columnWidth * 0.5f, height), Constants.guiSave))
				{
					//check if the file exist
					string welding_pathname = this.welding_pathname; // Cache the built pathname
					if (!File<UbioZurWeldingLtd>.Local.Exists(welding_pathname))
					{
						//create the file
						L.StreamWriter partfile = File<UbioZurWeldingLtd>.Local.CreateText(welding_pathname);
						partfile.Close();
						WriteCfg();
						this.state = DisplayState.savedWindow;
					}
					else
					{
						this.state = DisplayState.overwriteDial;
					}
				}
			}
			else
			{
				GUI.Box(new Rect(_guiInfoWindowColoumns[1].x + columnWidth * 0.5f, height + columnHeight + margin, columnWidth * 0.5f, height), Constants.guiSave);
			}
			if (GUI.Button(new Rect(_guiInfoWindowColoumns[2].x, height + columnHeight + margin, columnWidth * 0.5f, height), Constants.guiCancel))
			{
				this.state = DisplayState.none;
				ClearEditor();
			}
			GUI.DragWindow();
		}


		/*
		 * Test if the name is not already in use
		 */
		private bool WelderNameNotUsed()
		{
			foreach (UrlDir.UrlConfig part in GameDatabase.Instance.GetConfigs(Constants.weldPartNode))
			{
				if ( string.Equals(part.name, _welder.Name) )
				{
					return false;
				}
			}
			return true;
		}


		private bool isSymmetryNumber()
		{
			int result;
			if (int.TryParse(_welder.stringStackSymmetry, out result))
			{
				_welder.stackSymmetry = result-1;
				return true;
			}
			else
			{
				return false;
			}
		}


		/*
		 * Writing the cfg File
		 */
		private void WriteCfg()
		{
			string filename = this.welding_pathname;
			Log.dbg("{0}{1}", Constants.logWritingFile, filename);
			
			_welder.CreateFullConfigNode();
			_welder.FullConfigNode.Save(filename);
			Log.dbg("{0}{1} successful", Constants.logWritingFile, filename);
			
			filename = filename.Replace(".cfg", "-Internal.cfg");
			_welder.FullInternalNode.Save(filename);
			Log.dbg("{0}{1} successful", Constants.logWritingFile, filename);
			
			if (_config.dataBaseAutoReload)
			{
				StartCoroutine(DatabaseHandler.DatabaseReloadWithMM());
			}
		}


		/*
		 * Free and clear the editor
		 */
		private void ClearEditor()
		{
			if (_config.clearEditor)
			{
				EditorLockManager.resetEditorLocks();
				EditorPartList.Instance.Refresh();
				if (_selectedPartbranch != null)
				{
					disablePartHighlight(_selectedPartbranch);
					EditorLogic.fetch.OnSubassemblyDialogDismiss(EditorLogic.RootPart);
					Log.dbg("{0} {1} - {2}", _config.clearEditor, _selectedPartbranch, EditorLogic.SelectedPart);
					EditorLogic.DeletePart(EditorLogic.RootPart);
					_selectedPartbranch = null;
				}
			}
		}


		/*
		 * Lock editor if mouse pointer is inside window rect
		 */
		private void PreventClickThrough(Rect rect)
		{
			Vector2 pointerPos = Mouse.screenPos;
			//			  if (rect.Contains(pointerPos) && !EditorLogic.softLock)
			if (rect.Contains(pointerPos))
			{
				EditorLockManager.lockEditor(Constants.settingPreventClickThroughLock);
			}
			//			  else if (!rect.Contains(pointerPos) && EditorLogic.softLock)
			else if (!rect.Contains(pointerPos))
			{
				EditorLockManager.unlockEditor(Constants.settingPreventClickThroughLock);
			}
		}
	} //public class UbioZurWeldingLtd : MonoBehaviour


} //namespace UbioWeldingLtd

using System;
using UnityEngine;
using KSP.UI.Screens;
using ToolbarControl_NS;

namespace UbioWeldingLtd
{

	[KSPAddon(KSPAddon.Startup.EditorAny, false)]
	public class EditorToolbar : MonoBehaviour
	{
		public static EditorToolbar instance { get; private set; }

		private ApplicationLauncher _toolbar = ApplicationLauncher.Instance;
		private ToolbarControl _toolbarControl;
		private bool _isEnabled = false;

		/// <summary>
		/// public access for initializing the toolbar
		/// </summary>
		public void initToolbar()
		{
			Debug.Log(string.Format("{0}- {1} => initToolbar", Constants.logPrefix, instance.GetType()));
			if (!this._isEnabled)
			{
				GameEvents.onGUIApplicationLauncherReady.Add(this.OnGuiAppLauncherReady);
				this._isEnabled = true;
			}
		}


		/// <summary>
		/// Function is called everytime the Editor is loaded as a scene. a quite primitive fallback as the Applauncher seems to not react on the GameEvents.onGUIApplicationLauncherReady
		/// </summary>
		private void Start()
		{
			if (null == this._toolbarControl)
			{
				OnGuiAppLauncherReady();
			}
		}


		/// <summary>
		/// the initial start of the class with preparing of the toolbar
		/// </summary>
		private void Awake()
		{
			try
			{
				instance = this;
				Debug.Log(string.Format("{0}- {1} => Awake", Constants.logPrefix, instance.GetType()));
				if (UbioZurWeldingLtd.instance != null)
				{
					if (UbioZurWeldingLtd.instance.config.useStockToolbar)
					{
						initToolbar();
					}
				}
			}
			catch (Exception exception)
			{
				Debug.LogError(string.Format("{0}- {1} => Awake", Constants.logPrefix, instance.GetType()));
				Debug.LogException(exception, this);
			}
		}


		/// <summary>
		/// adds the toolbar to the applauncher
		/// </summary>
		private void OnGuiAppLauncherReady()
		{
			try
			{
				this._toolbarControl = gameObject.AddComponent<ToolbarControl>();
				this._toolbarControl.AddToAllToolbars(
					useToolbarButton, null,
					ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH,
					"UbioWeldingLtd",
					"UbioWeldingLtdButton",				
					Constants.settingLargeIconGetPath,
				    Constants.settingSmallIconGetPath,
					Constants.weldManufacturer
					);
				this._isEnabled = true;
				Debug.Log(string.Format("{0}-> OnGuiAppLauncherReady done", Constants.logPrefix));
			}
			catch (Exception exception)
			{
				Debug.LogError(string.Format("{0}- {1} => OnGuiAppLauncherReady", Constants.logPrefix, instance.GetType()));
				Debug.LogException(exception, this);
			}
		}


		/// <summary>
		/// used when the toolbar button is clicked
		/// starts the welding process and resets the buttonstate
		/// </summary>
		private void useToolbarButton()
		{
			try
			{
				UbioZurWeldingLtd.instance.stockToolbarButtonUsed();
				this._toolbarControl.SetFalse();
			}
			catch (Exception exception)
			{
				Debug.LogError(string.Format("{0}- {1} => useToolbarButton", Constants.logPrefix, instance.GetType()));
				Debug.LogException(exception, this);
			}
		}


		/// <summary>
		/// removes the buttom from the toolbar
		/// </summary>
		private void OnDestroy()
		{
			try
			{
				GameEvents.onGUIApplicationLauncherReady.Remove(this.OnGuiAppLauncherReady);
				if (null != this._toolbarControl)
				{
					this._isEnabled = false;
					this._toolbarControl.OnDestroy();
					Destroy(this._toolbar);
					this._toolbarControl = null;
				}
				Debug.Log("BuildToolbar->OnDestroy");
			}
			catch (Exception exception)
			{
				Debug.LogError(string.Format("{0}- {1} => OnDestroy", Constants.logPrefix, instance.GetType()));
				Debug.LogException(exception, this);
			}
		}


		/// <summary>
		/// regular update of the toolbarbutton
		/// </summary>
		private void Update()
		{
			try
			{
				if (null == this._toolbarControl)
				{
					return;
				}

				this._toolbarControl.Enabled = (null != EditorLogic.fetch && EditorLogic.fetch.ship.parts.Count > 0);
			}
			catch (Exception exception)
			{
				Debug.LogError(string.Format("{0}- {1} => Update", Constants.logPrefix, instance.GetType()));
				Debug.LogException(exception, this);
			}
		}


	}
}

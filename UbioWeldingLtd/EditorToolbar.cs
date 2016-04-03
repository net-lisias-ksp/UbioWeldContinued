using System;
using UnityEngine;
using KSP.UI.Screens;

namespace UbioWeldingLtd
{

	[KSPAddon(KSPAddon.Startup.EditorAny, false)]
	public class EditorToolbar : MonoBehaviour
	{
		public static EditorToolbar instance { get; private set; }

		private ApplicationLauncher _toolbar = ApplicationLauncher.Instance;
		private ApplicationLauncherButton _toolbarButton;
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
			if (_toolbarButton == null)
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
				//this._iconTexture = GameDatabase.Instance.GetTexture(Constants.settingIconGetPath, false);
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
				this._toolbarButton = this._toolbar.AddModApplication(
					useToolbarButton,
					null,
					null,
					null,
					null,
					null,
					ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH,
					GameDatabase.Instance.GetTexture(Constants.settingIconGetPath, false)
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
				this._toolbarButton.SetFalse();
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
				if (_toolbarButton != null)
				{
					ApplicationLauncher.Instance.RemoveModApplication(_toolbarButton);
					this._isEnabled = false;
					_toolbarButton = null;
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
				if (this._toolbarButton == null)
				{
					return;
				}

				if (EditorLogic.fetch != null && EditorLogic.fetch.ship.parts.Count > 0)
				{
					this._toolbarButton.Enable();
				}
				else
				{
					this._toolbarButton.Disable();
				}
			}
			catch (Exception exception)
			{
				Debug.LogError(string.Format("{0}- {1} => Update", Constants.logPrefix, instance.GetType()));
				Debug.LogException(exception, this);
			}
		}


	}
}

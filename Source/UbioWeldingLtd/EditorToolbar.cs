using System;
using UnityEngine;
using KSP.UI.Screens;
using ToolbarControl_NS;

namespace UbioWeldingLtd
{
	public class EditorToolbar
	{
		private ToolbarControl _toolbarControl;
		private readonly MonoBehaviour owner;

		public EditorToolbar(MonoBehaviour owner) {
			this.owner = owner;	

			this._toolbarControl = this.owner.gameObject.AddComponent<ToolbarControl>();
			this._toolbarControl.AddToAllToolbars(
				OnClickToolbarButton, null,
				ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH,
				this.GetType().Namespace,
				this.GetType().Namespace+"Button",
				Constants.settingLargeIconGetPath,
				Constants.settingSmallIconGetPath,
				Constants.weldManufacturer
				);
		}

		/// <summary>
		/// used when the toolbar button is clicked
		/// starts the welding process and resets the buttonstate
		/// </summary>
		private void OnClickToolbarButton()
		{
			UbioZurWeldingLtd.instance.HandleToolbarButtonUsed();
			this._toolbarControl.SetFalse();
		}

		public void Start()
		{
			ToolbarControl.RegisterMod(this.GetType().Namespace, Constants.weldManufacturer);
		}

		public void Update()
		{
			if (null == this._toolbarControl) return;

			this._toolbarControl.Enabled = (null != EditorLogic.fetch && EditorLogic.fetch.ship.parts.Count > 0);
		}

		public void OnDisable()
		{
			this._toolbarControl.OnDestroy();
			this._toolbarControl = null;
		}

		public void OnDestroy()
		{
		}
	}
}

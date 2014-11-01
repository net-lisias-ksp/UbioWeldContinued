using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace UbioWeldingLtd
{
	class DatabaseHandler
	{
		static private bool _isStartingReloading = false;
		static public bool isReloading
		{
			get { return (_isStartingReloading || !(GameDatabase.Instance.IsReady()) || !(PartLoader.Instance.IsReady())); }
		}
		/// <summary>
		/// reloads the gamedatabase with the modulemanager to keep the modified parts intact
		/// </summary>
		/// <param name="dump"></param>
		/// <returns></returns>
		static public IEnumerator DatabaseReloadWithMM(bool dump = false)
		{
//			_isStartingReloading = true;
			while (!GameDatabase.Instance.IsReady())
			{
				yield return null;
			}
			ModuleManager.MMPatchLoader.Instance.StartLoad();

			while (!ModuleManager.MMPatchLoader.Instance.IsReady())
			{
				yield return null;
			}
			PartResourceLibrary.Instance.LoadDefinitions();

			PartLoader.Instance.StartLoad();

			while (!PartLoader.Instance.IsReady())
			{
				yield return null;
			}
//			_isStartingReloading = false;
		}

		/// <summary>
		/// simple pure stock reloading of the gamedatabase
		/// </summary>
		static public void ReloadDatabase()
		{
			//reload database Big thanks to AncientGammoner (KSP Forum)
			_isStartingReloading = true;
			GameDatabase.Instance.Recompile = true;
			GameDatabase.Instance.StartLoad();
			PartLoader.Instance.Recompile = true;
			PartLoader.Instance.StartLoad();
			_isStartingReloading = false;
		}

	}
}

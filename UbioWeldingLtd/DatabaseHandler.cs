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
	//	ModuleManager support works with any ModuleManager version greater than 2.3.1
	//	thank to Gary Dryden for idea how to of dynamically invoke MM methods http://www.codeproject.com/Articles/13747/Dynamically-load-a-class-and-execute-a-method-in-N
	class DatabaseHandler : MonoBehaviour
	{
		private static object _MMPatchLoader;
		private static Type _MMPatchLoaderType;

		static private bool _isStartingReloading = false;

		static public bool isReloading
		{
			get { return (_isStartingReloading || !(GameDatabase.Instance.IsReady()) || !(PartLoader.Instance.IsReady())); }
		}


		/// <summary>
		/// initializes ModuleManager support
		/// must bu called before DatabaseReloadWithMM
		/// </summary>
		/// <returns></returns>
		public static void initMMAssembly()
		{
			Assembly _MMAssembly = null;
			// Walk through asseblies looking for ModuleManager* name
			foreach (AssemblyLoader.LoadedAssembly lAssembly in AssemblyLoader.loadedAssemblies)
			{
				Assembly assembly = lAssembly.assembly;
				System.Version aVersion = assembly.GetName().Version;

				if (assembly.GetName().Name.StartsWith("ModuleManager") &&
						(((_MMAssembly == null) && (aVersion >= Constants.minModuleManagerVersion)) || //first instance of ModuleManager
						(_MMAssembly.GetName().Version < aVersion)))	//in case was loaded multiple versions of ModuleManager
				{
					_MMAssembly = assembly;
				}
			}
			if (_MMAssembly != null)
			{
				Debug.Log(string.Format("{0} ModuleManager assembly was found: {1} (version {2})", Constants.logPrefix, _MMAssembly.GetName().Name, _MMAssembly.GetName().Version));

				// Walk through each type in the assembly looking for MMPatchLoader class
				foreach (Type type in _MMAssembly.GetTypes())
				{
					if (type.IsClass)
					{
						if (type.FullName.EndsWith(".MMPatchLoader"))
						{
							_MMPatchLoader = UnityEngine.Object.FindObjectOfType(type);
							_MMPatchLoaderType = type;
						}
					}
				}
				Debug.Log(string.Format("{0} ModuleManager.MMPatchLoader object is found: {1}", Constants.logPrefix, (_MMPatchLoader != null)));
			}
			else
			{
				Debug.Log(string.Format("{0} ModuleManager assembly was not found!", Constants.logPrefix));
			}
		}


		/// <summary>
		/// this will dynamically invoke _MethodName_ method of ModuleManager.MMPatchLoader
		/// </summary>
		/// <returns></returns>
		private static object DynaInvokeMMPatchLoaderMethod(string MethodName)
		{
			if (_MMPatchLoader != null)
			{
				// Dynamically invoke the method
				return _MMPatchLoaderType.InvokeMember(MethodName,
														BindingFlags.Default | BindingFlags.InvokeMethod,
														null,
														_MMPatchLoader,
														null);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// this will check for an installed module manager for a cleaner reload of the database
		/// remember that this is NOT bound to a specific version of ModuleManager
		/// </summary>
		/// <returns></returns>
		public static bool isModuleManagerInstalled()
		{
			bool mmInstalled = (_MMPatchLoader != null);
			Debug.Log(string.Format("{0} MM installed = {1}", Constants.logPrefix, mmInstalled));
			return mmInstalled;
		}


		/// <summary>
		/// reloads the gamedatabase with the modulemanager to keep the modified parts intact
		/// remember that this is NOT bound to a specific version of ModuleManager
		/// </summary>
		/// <param name="dump"></param>
		/// <returns></returns>
		static public IEnumerator DatabaseReloadWithMM(bool dump = false)
		{
			_isStartingReloading = true;
			while (!GameDatabase.Instance.IsReady())
			{
				yield return null;
			}
			DynaInvokeMMPatchLoaderMethod("StartLoad");

			while (!(bool)DynaInvokeMMPatchLoaderMethod("IsReady"))
			{
				yield return null;
			}
			PartResourceLibrary.Instance.LoadDefinitions();

			PartLoader.Instance.StartLoad();

			while (!PartLoader.Instance.IsReady())
			{
				yield return null;
			}
			_isStartingReloading = false;
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

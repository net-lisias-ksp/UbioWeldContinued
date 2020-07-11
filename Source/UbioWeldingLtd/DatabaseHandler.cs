﻿using System;
using System.Collections;
using System.Collections.Generic;
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

		static private bool _isReloading = false;

		static public bool isReloading
		{
			get { return _isReloading; }
		}


		/// <summary>
		/// initializes ModuleManager support
		/// must bu called before DatabaseReloadWithMM
		/// </summary>
		/// <returns></returns>
		public static void initMMAssembly()
		{
			Assembly _MMAssembly = null;
			Assembly _TBAssembly = null;

			Log.dbg("DatabaseHandler.initMMAssembly()");

			// Walk through asseblies looking for ModuleManager* name
			foreach (AssemblyLoader.LoadedAssembly lAssembly in AssemblyLoader.loadedAssemblies)
			{
				if (null == lAssembly || null == lAssembly.assembly)
					Log.warn("Holly crap, a null entry on the AssemblyLoader.loadedAssemblies!");

				Assembly assembly = lAssembly.assembly;
				Log.dbg("{0} on {1}", assembly.GetName().Name, assembly.Location);

				System.Version aVersion = assembly.GetName().Version;

				if (	assembly.GetName().Name.Equals(Dependencies.ModuleManager.name)
					&& (
						( (_MMAssembly == null) && (aVersion >= Dependencies.ModuleManager.minVersion) )	//first instance of ModuleManager
						|| (_MMAssembly.GetName().Version < aVersion)										//in case was loaded multiple versions of ModuleManager
						)
					)
				{
					_MMAssembly = assembly;
				}
				else if (	assembly.GetName().Name.Equals(Dependencies.ToobarControl.name)
					&& (
						( (_TBAssembly == null) && (aVersion >= Dependencies.ToobarControl.minVersion) )
						|| (_TBAssembly.GetName().Version < aVersion)
						)
					)
				{
					_TBAssembly = assembly;
				}

				Log.dbg("{0} checked.", assembly.GetName().Name);
			}

			if (_MMAssembly != null)
			{
				Log.info("ModuleManager assembly was found: {0} (version {1})", _MMAssembly.GetName().Name, _MMAssembly.GetName().Version);

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
				Log.info("ModuleManager.MMPatchLoader object is found: {0}", (_MMPatchLoader != null));
			}
			else
			{
				Log.info(string.Format("ModuleManager assembly was not found!"));
			}

			if (_TBAssembly != null)
			{
				Log.info("ToolbarControl assembly was found: {0} (version {1})", _TBAssembly.GetName().Name, _TBAssembly.GetName().Version);
			}
			else
			{
				Log.info("ToolbarControl assembly was not found!");
			}
		}

		/// <summary>
		/// this will dynamically invoke _MethodName_ method of ModuleManager.MMPatchLoader
		/// </summary>
		/// <returns></returns>
		public static object DynaInvokeMMPatchLoaderMethod(string MethodName)
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
		public static bool isModuleManagerInstalled
		{
			get { return (_MMPatchLoader != null); }
		}


		/// <summary>
		/// reloads the gamedatabase with the modulemanager (if installed) to keep the modified parts intact
		/// this works with version of ModuleManager 2.3.1 and higher
		/// </summary>
		/// <param name="dump"></param>
		/// <returns></returns>
		static public IEnumerator DatabaseReloadWithMM()
		{
			// this code is mostly borrowed from Sarbian's ModuleManager sources (https://github.com/sarbian/ModuleManager/blob/master/moduleManager.cs)
			try // just in case - to waiting for the reloading was completed sometime
			{
				_isReloading = true;
				yield return null;

				GameDatabase.Instance.Recompile = true;
				GameDatabase.Instance.StartLoad();

				while (!GameDatabase.Instance.IsReady())
				{
					yield return null;
				}

				if (isModuleManagerInstalled)
				{
					DynaInvokeMMPatchLoaderMethod("StartLoad");

					while (!(bool)DynaInvokeMMPatchLoaderMethod("IsReady"))
					{
						yield return null;
					}
				}
				PartResourceLibrary.Instance.LoadDefinitions();

				PartLoader.Instance.Recompile = true;
				PartLoader.Instance.StartLoad();

				while (!PartLoader.Instance.IsReady())
				{
					yield return null;
				}
			}
			finally
			{
				_isReloading = false;
			}
		}
	}
}

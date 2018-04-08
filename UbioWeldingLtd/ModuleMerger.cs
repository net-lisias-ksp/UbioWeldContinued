﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UbioWeldingLtd
{
	public class ModuleMerger
	{

		/// <summary>
		/// merges the modules in a almost generic way.
		/// </summary>
		/// <param name="partname"></param>
		/// <param name="configuration"></param>
		public void mergeModules(string partname, UrlDir.UrlConfig configuration, List<ConfigNode> modulelist)
		{
			ConfigNode[] originalModules = configuration.config.GetNodes(Constants.weldModuleNode);
			string newModuleName = "";
			bool exist = false;

			ConfigNode newModule;

			foreach (ConfigNode originalModule in originalModules)
			{
				newModule = originalModule.CreateCopy();
				newModuleName = newModule.GetValue(newModule.values.DistinctNames()[0]);
				exist = false;

				foreach (ConfigNode existingNewModule in modulelist)
				{
					if (newModuleName.Equals(existingNewModule.GetValue(existingNewModule.values.DistinctNames()[0])))
					{
						if (!WeldingHelpers.isArrayContaining(newModuleName, UbioZurWeldingLtd.instance.config.modulesToMultiply))
						{
							Log.dbg("| {0} Module already exists!!!", existingNewModule.GetValue(existingNewModule.values.DistinctNames()[0]));
							if (newModule.values.DistinctNames().Length < 2)
							{
								// making shure that the MODULE gets not duplicated in case it has no attributes
								exist = true;
								break;
							}
							else
							{
								string[] breakingAttributes = new string[newModule.values.DistinctNames().Count()];
								for (int i = 0; i < newModule.values.DistinctNames().Count(); i++)
								{
									breakingAttributes[i] = string.Concat(newModuleName, Constants.nameSeparator, newModule.values.DistinctNames()[i]);
								}

								breakingAttributes = WeldingHelpers.getSharedArrayValues(breakingAttributes, UbioZurWeldingLtd.instance.config.breakingModuleAttributes);
								Log.dbg("| BreakingAttributes found = {0} ", breakingAttributes.Length);

								if (breakingAttributes.Length > 0)
								{
									foreach (string s in breakingAttributes)
									{
										string breakingAttribute = s.Replace(string.Concat(newModuleName, Constants.nameSeparator), "");
										var existingValue = existingNewModule.GetValue(breakingAttribute);
										var newValue = newModule.GetValue(breakingAttribute);
										Log.dbg("| BreakingAttributes found | current one is {0} | ExistingValue = {1} - NewValue = {2}", breakingAttribute, existingValue, newValue);
										exist = Equals(existingValue, newValue);
										if (!exist)
										{
											break;
										}
									}
									if (exist)
									{
										mergeContents(newModuleName, newModule, existingNewModule);
										exist = true;
										break;
									}
								}
								else
								{
									mergeContents(newModuleName, newModule, existingNewModule);
									exist = true;
									break;
								}
							}
						}
					}
					Log.dbg("| Module {1} ready to add = {0}", !exist, existingNewModule.GetValue("name"));
				}//foreach (ConfigNode existingNewModule in _modulelist)
				if ( ! (exist || WeldingHelpers.isArrayContaining(newModuleName, UbioZurWeldingLtd.instance.config.modulesToIgnore)) )
				{
					addNewModule(partname, newModuleName, newModule, modulelist);
				}
			} //foreach (ConfigNode mod in modules)
		}


		/// <summary>
		/// merges any module that is of the Vector4 kind
		/// </summary>
		/// <param name="newModule"></param>
		/// <param name="existingNewModule"></param>
		private void mergeVector4Modules(ConfigNode newModule, ConfigNode existingNewModule)
		{
			Log.dbg("| Merging Vector4Modules Start");
			foreach (string subModule in UbioZurWeldingLtd.instance.config.vector4CurveModules)
			{
				if (newModule.HasNode(subModule))
				{
					if (existingNewModule.HasNode(subModule))
					{
						string[] curve = newModule.GetNode(subModule).GetValues(Constants.curveKey);
						string[] cfgcurve = existingNewModule.GetNode(subModule).GetValues(Constants.curveKey);
						Vector4[] cfgcurvevect = MergeVelCurve(curve, cfgcurve);
						existingNewModule.GetNode(subModule).RemoveValues(Constants.curveKey);
						foreach (Vector4 vec in cfgcurvevect)
						{
							existingNewModule.GetNode(subModule).AddValue(Constants.curveKey, ConfigNode.WriteVector(vec));
						}
					}
					else
					{
						existingNewModule.AddNode(newModule.GetNode(subModule));
					}
				}
			}
			Log.dbg("| Merging Vector4Modules End");
		}


		/// <summary>
		/// merges the parts of a module that are an vector2
		/// </summary>
		/// <param name="newModule"></param>
		/// <param name="existingNewModule"></param>
		private void mergeVector2Modules(ConfigNode newModule, ConfigNode existingNewModule)
		{
			Log.dbg("| Merging Vector2Modules Start");
			foreach (string subModule in UbioZurWeldingLtd.instance.config.vector2CurveModules)
			{
				if (newModule.HasNode(subModule))
				{
					if (existingNewModule.HasNode(subModule))
					{
						string[] curve = newModule.GetNode(subModule).GetValues(Constants.curveKey);
						string[] cfgcurve = existingNewModule.GetNode(subModule).GetValues(Constants.curveKey);
						Vector2[] cfgcurvevect = MergeAtmCurve(curve, cfgcurve);
						existingNewModule.GetNode(subModule).RemoveValues(Constants.curveKey);
						foreach (Vector2 vec in cfgcurvevect)
						{
							existingNewModule.GetNode(subModule).AddValue(Constants.curveKey, ConfigNode.WriteVector(vec));
						}
					}
					else
					{
						existingNewModule.AddNode(newModule.GetNode(subModule));
					}
				}
			}
			Log.dbg("| Merging Vector2Modules End");
		}



		/// <summary>
		/// manages the merging of submodules inside the module
		/// </summary>
		/// <param name="newModule"></param>
		/// <param name="existingNewModule"></param>
		private void mergeSubModules(ConfigNode newModule, ConfigNode existingNewModule)
		{
			bool exist = false;
			string newSubModuleName = "";
			Log.dbg("| Merging SubModules Start");

			foreach (string subModule in UbioZurWeldingLtd.instance.config.subModules)
			{
				ConfigNode[] newNodes = newModule.GetNodes(subModule);
				ConfigNode[] existingNodes = existingNewModule.GetNodes(subModule);

				foreach (ConfigNode newNode in newNodes)
				{
					newSubModuleName = newNode.GetValue(newNode.values.DistinctNames()[0]);

					foreach (ConfigNode existingNode in existingNodes)
					{
						exist = false;
						string[] breakingAttributes = new string[newNode.values.DistinctNames().Count()];
						for (int i = 0; i < newNode.values.DistinctNames().Count(); i++)
						{
							breakingAttributes[i] = string.Concat(subModule, Constants.nameSeparator, newNode.values.DistinctNames()[i]);
						}
						breakingAttributes = WeldingHelpers.getSharedArrayValues(breakingAttributes, UbioZurWeldingLtd.instance.config.breakingModuleAttributes);
						Log.dbg("| SubModule BreakingAttributes found = {0} ", breakingAttributes.Length);

						if (breakingAttributes.Length > 0)
						{
							foreach (string s in breakingAttributes)
							{
								string breakingAttribute = s.Replace(string.Concat(subModule, Constants.nameSeparator), "");
								var existingValue = existingNode.GetValue(breakingAttribute);
								var newValue = newNode.GetValue(breakingAttribute);
								Log.dbg("| SubModule BreakingAttributes found | current one is {0} | ExistingValue = {1} - NewValue = {2}", breakingAttribute, existingValue, newValue);
								exist = Equals(existingValue, newValue);
								if (!exist)
								{
									break;
								}
							}
							if (exist)
							{
								mergeContents(newSubModuleName, newNode, existingNode);
								exist = true;
								break;
							}
						}
						else
						{
							mergeContents(newSubModuleName, newNode, existingNode);
							exist = true;
							break;
						}
					}
					if (!exist)
					{
						if (!WeldingHelpers.isArrayContaining(newSubModuleName, UbioZurWeldingLtd.instance.config.modulesToIgnore))
						{
							existingNewModule.AddNode(newNode);
						}
					}
				}
			}
			Log.dbg("| Merging SubModules End");
		}

		private void mergeContents(string newSubModuleName, ConfigNode newNode, ConfigNode existingNode)
		{
			string dbgname = existingNode.GetValue("name");
			Log.dbg("Pre mergeModuleAttributes({0})", dbgname);
			mergeModuleAttributes(newSubModuleName, newNode, existingNode);
			Log.dbg("Pre mergeSubModules({0})", dbgname);
			mergeSubModules(newNode, existingNode);
			Log.dbg("Pre mergeVector2Modules({0})", dbgname);
			mergeVector2Modules(newNode, existingNode);
			Log.dbg("Pre mergeVector4Modules({0})", dbgname);
			mergeVector4Modules(newNode, existingNode);
			Log.dbg("Values done");
		}

		/// <summary>
		/// managed the merging of the attributes of a module
		/// </summary>
		/// <param name="newModuleName"></param>
		/// <param name="exist"></param>
		/// <param name="boolResult"></param>
		/// <param name="floatResult"></param>
		/// <param name="newModule"></param>
		/// <param name="existingNewModule"></param>
		private void mergeModuleAttributes(string newModuleName, ConfigNode newModule, ConfigNode existingNewModule)
		{
			bool boolResult;
			float floatResult;

			List<string> moduleNames = newModule.values.DistinctNames().ToList<string>();
			foreach (string name in existingNewModule.values.DistinctNames())
			{
				if (!moduleNames.Contains(name))
				{
					moduleNames.Add(name);
				}
			}

			foreach (string ModuleAttribute in moduleNames)
			{
				boolResult = false;
				floatResult = 0f;
				if (bool.TryParse(existingNewModule.GetValue(ModuleAttribute), out boolResult) || bool.TryParse(newModule.GetValue(ModuleAttribute), out boolResult))
				{
					Log.dbg("| {0} - {1} is bool", newModuleName, ModuleAttribute);
					mergeModuleBoolValues(newModuleName, newModule, existingNewModule, ModuleAttribute);
				}
				else
				{
					if (float.TryParse(existingNewModule.GetValue(ModuleAttribute), out floatResult) || float.TryParse(newModule.GetValue(ModuleAttribute), out floatResult))
					{
						Log.dbg("| {0} - {1} is float", newModuleName, ModuleAttribute);
						mergeModuleFloatValues(newModuleName, newModule, existingNewModule, ModuleAttribute);
					}
					else
					{
						Log.dbg("| {0} - {1} is string", newModuleName, ModuleAttribute);
						mergeModuleStringValues(newModuleName, newModule, existingNewModule, ModuleAttribute);
					}
				}
				Log.dbg("- Alex Modulemerger - {0} | {1} = {2}", existingNewModule.GetValue(existingNewModule.values.DistinctNames()[0]), ModuleAttribute, existingNewModule.GetValue(ModuleAttribute));
			}
		}


		/// <summary>
		/// handles the correct merging of bool values in the modules
		/// </summary>
		/// <param name="newModuleName"></param>
		/// <param name="newModule"></param>
		/// <param name="existingNewModule"></param>
		/// <param name="ModuleAttribute"></param>
		private void mergeModuleBoolValues(string newModuleName, ConfigNode newModule, ConfigNode existingNewModule, string ModuleAttribute)
		{
			Log.dbg("| {0} - {1} is bool", newModuleName, ModuleAttribute);
			if (newModule.HasValue(ModuleAttribute))
			{
				if (existingNewModule.HasValue(ModuleAttribute))
				{
					existingNewModule.SetValue(ModuleAttribute, (bool.Parse(newModule.GetValue(ModuleAttribute)) || bool.Parse(existingNewModule.GetValue(ModuleAttribute))).ToString());
				}
				else
				{
					existingNewModule.AddValue(ModuleAttribute, bool.Parse(newModule.GetValue(ModuleAttribute)).ToString());
				}
			}
			Log.dbg("| {0} - {1} is merged with value {2}", newModuleName, ModuleAttribute, bool.Parse(existingNewModule.GetValue(ModuleAttribute)));
		}


		/// <summary>
		/// handles the correct merging of float values in the modules
		/// </summary>
		/// <param name="newModuleName"></param>
		/// <param name="newModule"></param>
		/// <param name="existingNewModule"></param>
		/// <param name="ModuleAttribute"></param>
		private void mergeModuleFloatValues(string newModuleName, ConfigNode newModule, ConfigNode existingNewModule, string ModuleAttribute)
		{
			Log.dbg("| {0} - {1} is float", newModuleName, ModuleAttribute);
			//merge float values if they are allowed
			if (!WeldingHelpers.isArrayContaining(string.Concat(newModuleName, Constants.nameSeparator, ModuleAttribute), UbioZurWeldingLtd.instance.config.unchangedModuleAttributes))
			{
				if (newModule.HasValue(ModuleAttribute) || existingNewModule.HasValue(ModuleAttribute))
				{
					float newValue = float.TryParse(newModule.GetValue(ModuleAttribute), out newValue) == false ? float.Parse(existingNewModule.GetValue(ModuleAttribute)) : float.Parse(newModule.GetValue(ModuleAttribute));
					Log.dbg("| {0} - newValue - {1} = {2}", newModuleName, ModuleAttribute, newValue);

					if (existingNewModule.HasValue(ModuleAttribute) && newModule.HasValue(ModuleAttribute))
					{
						float existingValue = float.Parse(existingNewModule.GetValue(ModuleAttribute));
						Log.dbg("| {0} - existingValue - {1} = {2}", newModuleName, ModuleAttribute, existingValue);

						if (WeldingHelpers.isArrayContaining(string.Concat(newModuleName, Constants.nameSeparator, ModuleAttribute), UbioZurWeldingLtd.instance.config.maximizedModuleAttributes))
						{
							Log.dbg("| {0} - {1} - maximized", newModuleName, ModuleAttribute);
							existingNewModule.SetValue(ModuleAttribute, (existingValue > newValue ? existingValue : newValue).ToString());
						}
						else
						{
							if (WeldingHelpers.isArrayContaining(string.Concat(newModuleName, Constants.nameSeparator, ModuleAttribute), UbioZurWeldingLtd.instance.config.minimizedModuleAttributes))
							{
								Log.dbg("| {0} - {1} - minimized", newModuleName, ModuleAttribute);
								existingNewModule.SetValue(ModuleAttribute, (existingValue < newValue ? existingValue : newValue).ToString());
							}
							else
							{

								if (WeldingHelpers.isArrayContaining(string.Concat(newModuleName, Constants.nameSeparator, ModuleAttribute), UbioZurWeldingLtd.instance.config.averagedModuleAttributes) && (newValue != 0 && existingValue != 0))
								{
									Log.dbg("| {0} - {1} - averaged", newModuleName, ModuleAttribute);
									existingNewModule.SetValue(ModuleAttribute, ((newValue + existingValue) * 0.5f).ToString());
								}
								else
								{
									Log.dbg("| {0} - {1} - added", newModuleName, ModuleAttribute);
									existingNewModule.SetValue(ModuleAttribute, (newValue + existingValue).ToString());
								}
							}
						}
					}
					else if (!existingNewModule.HasValue(ModuleAttribute) && newModule.HasValue(ModuleAttribute))
					{
						Log.dbg("| {0} - setNewValue - {1} = {2}", newModuleName, ModuleAttribute, newValue);
						existingNewModule.AddValue(ModuleAttribute, newValue.ToString());
					}
				}
			}
			Log.dbg("| {0} - {1} is merged with value {2}", newModuleName, ModuleAttribute, float.Parse(existingNewModule.GetValue(ModuleAttribute)));
		}


		/// <summary>
		/// handles the correct merging of string values in the modules
		/// </summary>
		/// <param name="newModuleName"></param>
		/// <param name="newModule"></param>
		/// <param name="existingNewModule"></param>
		/// <param name="ModuleAttribute"></param>
		private void mergeModuleStringValues(string newModuleName, ConfigNode newModule, ConfigNode existingNewModule, string ModuleAttribute)
		{
			Log.dbg("| {0} - {1} is string", newModuleName, ModuleAttribute);
			if (newModule.HasValue(ModuleAttribute))
			{
				if (existingNewModule.HasValue(ModuleAttribute))
				{
					existingNewModule.SetValue(ModuleAttribute, newModule.GetValue(ModuleAttribute));
				}
				else
				{
					existingNewModule.AddValue(ModuleAttribute, newModule.GetValue(ModuleAttribute));
				}
			}
			Log.dbg("| {0} - {1} is merged with value {2}", newModuleName, ModuleAttribute, existingNewModule.GetValue(ModuleAttribute));
		}


		/// <summary>
		/// Merges values that are of the type Vector4 for curves
		/// </summary>
		/// <param name="set1"></param>
		/// <param name="set2"></param>
		/// <returns></returns>
		public Vector4[] MergeVelCurve(string[] set1, string[] set2)
		{
			Vector4[] curvevect = new Vector4[(set1.Length >= set2.Length) ? set1.Length : set2.Length];
			for (int i = 0; i < curvevect.Length; ++i)
			{
				curvevect[i] = ConfigNode.ParseVector4(set2[i]);
			}
			for (int i = 0; i < set1.Length; ++i)
			{
				Vector4 vect = ConfigNode.ParseVector4(set1[i]);
				int j = 0;
				while (vect.x != curvevect[j].x && j < curvevect.Length && j < set2.Length)
				{
					++j;
				}
				if (j >= curvevect.Length)
				{
					//didn't find it, should add more
				}
				else if (j >= set2.Length)
				{
					curvevect[j] = vect;
				}
				else
				{
					curvevect[j].y = (curvevect[j].y + vect.y) * 0.5f;
				}
			}
			return curvevect;
		}


		/// <summary>
		/// Merges of the type Vector2 for atmosphericCurves
		/// </summary>
		/// <param name="set1"></param>
		/// <param name="set2"></param>
		/// <returns></returns>
		public Vector2[] MergeAtmCurve(string[] set1, string[] set2)
		{
			Vector2[] curvevect = new Vector2[(set1.Length >= set2.Length) ? set1.Length : set2.Length];
			for (int i = 0; i < curvevect.Length; ++i)
			{
				curvevect[i] = ConfigNode.ParseVector2(set2[i]);
			}
			for (int i = 0; i < set1.Length; ++i)
			{
				Vector2 vect = ConfigNode.ParseVector2(set1[i]);
				int j = 0;
				while (vect.x != curvevect[j].x && j < curvevect.Length && j < set2.Length)
				{
					++j;
				}
				if (j >= curvevect.Length)
				{
					//didn't find it, should add more
				}
				else if (j >= set2.Length)
				{
					curvevect[j] = vect;
				}
				else
				{
					curvevect[j].y = (curvevect[j].y + vect.y) * 0.5f;
				}
			}
			return curvevect;
		}


		/// <summary>
		/// handles the correct addition of Modules to the Modulelist of the new Part
		/// </summary>
		/// <param name="partname"></param>
		/// <param name="newModuleName"></param>
		/// <param name="newModule"></param>
		private void addNewModule(string partname, string newModuleName, ConfigNode newModule, List<ConfigNode> modulelist)
		{
			switch (newModule.GetValue(newModule.values.DistinctNames()[0]))
			{
				case Constants.modStockAnchdec:
					{
						//Decoupler: Change node name
						string decouplename = newModule.GetValue("explosiveNodeID") + partname + modulelist.Count;
						newModule.SetValue("explosiveNodeID", decouplename);
						break;
					}
				case Constants.modStockDocking:
					{
						//Docking port: Change node name if any TODO: FIX This
						if (newModule.HasValue("referenceAttachNode"))
						{
							string dockname = newModule.GetValue("referenceAttachNode") + partname + modulelist.Count;
							newModule.SetValue("referenceAttachNode", dockname);
						}
						break;
					}
				case Constants.modStockJettison:
					{
						//Fairing/Jetisson, change node name
						string jetissonname = newModule.GetValue("bottomNodeName") + partname + modulelist.Count;
						newModule.SetValue("bottomNodeName", jetissonname);
						break;
					}
			}
			modulelist.Add(newModule);
			Log.dbg("{0}{1}", Constants.logModAdd, newModuleName);
		}
	}
}
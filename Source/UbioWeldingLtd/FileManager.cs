using System;
using System.Xml.Serialization;
using UnityEngine;

using KSPe.IO;
using KSPe.IO.Data;     // KSP/PluginData/net.lisias.ksp/UbioWeldingLtd/*

namespace UbioWeldingLtd
{
	public static class FileManager
	{

		/// <summary>
		/// catches unknown nodes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void serializer_UnknownNode
		(object sender, XmlNodeEventArgs e)
		{
			Log.dbg("Unknown Node:" + e.Name + "\t" + e.Text);
		}


		/// <summary>
		/// catches unknown attribute nodes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void serializer_UnknownAttribute
		(object sender, XmlAttributeEventArgs e)
		{
			System.Xml.XmlAttribute attr = e.Attr;
			Log.dbg("Unknown attribute " + attr.Name + "='" + attr.Value + "'");
		}


		/// <summary>
		/// loads configfile and prepares it with all needed lists
		/// </summary>
		/// <returns></returns>
		public static WeldingConfiguration loadConfig()
		{

			WeldingConfiguration configuration = new WeldingConfiguration();
			ModuleLists moduleList = new ModuleLists();
			FileStream FileStream = null;

			try {
				XmlSerializer configSerializer = new XmlSerializer(typeof(WeldingConfiguration));
				configSerializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
				configSerializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
				FileStream = FileStream.CreateForType<UbioZurWeldingLtd>(Constants.settingXmlConfigFileName, FileMode.Open);
				configuration = (WeldingConfiguration)configSerializer.Deserialize(FileStream);

				if (configuration.MainWindowXPosition > (Screen.width - Constants.guiScreenEdgeClearance))
				{
					configuration.MainWindowXPosition = Screen.width - Constants.guiScreenEdgeClearance;
				}
				if (configuration.MainWindowYPosition > (Screen.height - Constants.guiScreenEdgeClearance))
				{
					configuration.MainWindowYPosition = Screen.height - Constants.guiScreenEdgeClearance;
				}
			} catch (Exception e) {
				configuration = new WeldingConfiguration();
				Log.warn(String.Format("{0} : {1}", Constants.settingXmlConfigFileName, e.Message));
			} finally {
				if (null != FileStream) FileStream.Close();
				FileStream = null;
			}

			try {
				XmlSerializer moduleListSerializer = new XmlSerializer(typeof(ModuleLists));
				moduleListSerializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
				moduleListSerializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
				FileStream = FileStream.CreateForType<UbioZurWeldingLtd>(Constants.settingXmlListFileName, FileMode.Open);
				moduleList = (ModuleLists)moduleListSerializer.Deserialize(FileStream);

				configuration.vector2CurveModules = moduleList.vector2CurveModules != null ? WeldingHelpers.convertFromToStringArray(moduleList.vector2CurveModules) : new string[0];
				configuration.vector4CurveModules = moduleList.vector4CurveModules != null ? WeldingHelpers.convertFromToStringArray(moduleList.vector4CurveModules) : new string[0];
				configuration.subModules = moduleList.subModules != null ? WeldingHelpers.convertFromToStringArray(moduleList.subModules) : new string[0];
				configuration.modulesToIgnore = moduleList.modulesToIgnore != null ? WeldingHelpers.convertFromToStringArray(moduleList.modulesToIgnore) : new string[0];
				configuration.modulesToMultiply = moduleList.modulesToMultiply != null ? WeldingHelpers.convertFromToStringArray(moduleList.modulesToMultiply) : new string[0];
				configuration.maximizedModuleAttributes = moduleList.maximizedModuleAttributes != null ? WeldingHelpers.convertFromToStringArray(moduleList.maximizedModuleAttributes) : new string[0];
				configuration.minimizedModuleAttributes = moduleList.minimizedModuleAttributes != null ? WeldingHelpers.convertFromToStringArray(moduleList.minimizedModuleAttributes) : new string[0];
				configuration.averagedModuleAttributes = moduleList.averagedModuleAttributes != null ? WeldingHelpers.convertFromToStringArray(moduleList.averagedModuleAttributes) : new string[0];
				configuration.unchangedModuleAttributes = moduleList.unchangedModuleAttributes != null ? WeldingHelpers.convertFromToStringArray(moduleList.unchangedModuleAttributes) : new string[0];
				configuration.breakingModuleAttributes = moduleList.breakingModuleAttributes != null ? WeldingHelpers.convertFromToStringArray(moduleList.breakingModuleAttributes) : new string[0];
			} catch (Exception e) {
				configuration.vector2CurveModules = Constants.basicVector2CurveModules;
				configuration.vector4CurveModules = Constants.basicVector4CurveModules;
				configuration.subModules = Constants.basicSubModules;
				configuration.modulesToIgnore = Constants.basicModulesToIgnore;
				configuration.modulesToMultiply = Constants.basicModulesToMultiply;
				configuration.maximizedModuleAttributes = Constants.basicMaximizedModuleAttributes;
				configuration.minimizedModuleAttributes = Constants.basicMinimizedModuleAttributes;
				configuration.averagedModuleAttributes = Constants.basicAveragedModuleAttributes;
				configuration.unchangedModuleAttributes = Constants.basicUnchangedModuleAttributes;
				configuration.breakingModuleAttributes = Constants.basicBreakingModuleAttributes;
				Log.warn(String.Format("{0} : {1}", Constants.settingXmlConfigFileName, e.Message));
			} finally {
				if (null != FileStream) FileStream.Close();
				FileStream = null;
			}

			Log.dbg("Config was loaded");
			return configuration;
		}

		/// <summary>
		/// saves the config file and the modulelist file so that it is possible to change them without the need to recompile
		/// </summary>
		/// <param name="configToSave"></param>
		public static void saveConfig(WeldingConfiguration configToSave)
		{
			try {
				saveConfig(configToSave, Constants.settingXmlConfigFileName, Constants.settingXmlListFileName);
			} catch (Exception e) {
				Log.warn(e.Message);
				saveConfig(configToSave, Constants.settingXmlConfigFileName, Constants.settingXmlListFileName);
			}
		}

		private static void saveConfig(WeldingConfiguration configToSave, String configFilename, String moduleFilename)
		{
			WeldingConfiguration configuration = (WeldingConfiguration)configToSave.clone();
			ModuleLists moduleList = new ModuleLists();
			StreamWriter fileStreamWriter;
			if (configuration == null)
			{
				configuration = new WeldingConfiguration();
			}
			configuration.vector2CurveModules = null;
			configuration.vector4CurveModules = null;
			configuration.subModules = null;
			configuration.modulesToIgnore = null;
			configuration.modulesToMultiply = null;
			configuration.maximizedModuleAttributes = null;
			configuration.minimizedModuleAttributes = null;
			configuration.averagedModuleAttributes = null;
			configuration.unchangedModuleAttributes = null;
			configuration.breakingModuleAttributes = null;

			XmlSerializer configSerializer = new XmlSerializer(typeof(WeldingConfiguration));
			fileStreamWriter = StreamWriter.CreateForType<UbioZurWeldingLtd>(configFilename);
			configSerializer.Serialize(fileStreamWriter, configuration);
			fileStreamWriter.Close();

			//ModuleList from the constants or the actual config, depending on length of the array, in case user did add some entries
			moduleList.vector2CurveModules = WeldingHelpers.convertStringFromToArray(configToSave.vector2CurveModules != null && (configToSave.vector2CurveModules.Length > Constants.basicVector2CurveModules.Length) ? configToSave.vector2CurveModules : Constants.basicVector2CurveModules);
			moduleList.vector4CurveModules = WeldingHelpers.convertStringFromToArray(configToSave.vector4CurveModules != null && (configToSave.vector4CurveModules.Length > Constants.basicVector4CurveModules.Length) ? configToSave.vector4CurveModules : Constants.basicVector4CurveModules);
			moduleList.subModules = WeldingHelpers.convertStringFromToArray(configToSave.subModules != null && (configToSave.subModules.Length > Constants.basicSubModules.Length) ? configToSave.subModules : Constants.basicSubModules);
			moduleList.modulesToIgnore = WeldingHelpers.convertStringFromToArray(configToSave.modulesToIgnore != null && (configToSave.modulesToIgnore.Length > Constants.basicModulesToIgnore.Length) ? configToSave.modulesToIgnore : Constants.basicModulesToIgnore);
			moduleList.modulesToMultiply = WeldingHelpers.convertStringFromToArray(configToSave.modulesToMultiply != null && (configToSave.modulesToMultiply.Length > Constants.basicModulesToMultiply.Length) ? configToSave.modulesToMultiply : Constants.basicModulesToMultiply);
			moduleList.maximizedModuleAttributes = WeldingHelpers.convertStringFromToArray(configToSave.maximizedModuleAttributes != null && (configToSave.maximizedModuleAttributes.Length > Constants.basicMaximizedModuleAttributes.Length) ? configToSave.maximizedModuleAttributes : Constants.basicMaximizedModuleAttributes);
			moduleList.minimizedModuleAttributes = WeldingHelpers.convertStringFromToArray(configToSave.minimizedModuleAttributes != null && (configToSave.minimizedModuleAttributes.Length > Constants.basicMinimizedModuleAttributes.Length) ? configToSave.minimizedModuleAttributes : Constants.basicMinimizedModuleAttributes);
			moduleList.averagedModuleAttributes = WeldingHelpers.convertStringFromToArray(configToSave.averagedModuleAttributes != null && (configToSave.averagedModuleAttributes.Length > Constants.basicAveragedModuleAttributes.Length) ? configToSave.averagedModuleAttributes : Constants.basicAveragedModuleAttributes);
			moduleList.unchangedModuleAttributes = WeldingHelpers.convertStringFromToArray(configToSave.unchangedModuleAttributes != null && (configToSave.unchangedModuleAttributes.Length > Constants.basicUnchangedModuleAttributes.Length) ? configToSave.unchangedModuleAttributes : Constants.basicUnchangedModuleAttributes);
			moduleList.breakingModuleAttributes = WeldingHelpers.convertStringFromToArray(configToSave.breakingModuleAttributes != null && (configToSave.breakingModuleAttributes.Length > Constants.basicBreakingModuleAttributes.Length) ? configToSave.breakingModuleAttributes : Constants.basicBreakingModuleAttributes);

			XmlSerializer moduleListSerializer = new XmlSerializer(typeof(ModuleLists));
			fileStreamWriter = StreamWriter.CreateForType<UbioZurWeldingLtd>(moduleFilename);

			moduleListSerializer.Serialize(fileStreamWriter, moduleList);

			fileStreamWriter.Close();
			Log.dbg("Config was saved");
		}
	}
}

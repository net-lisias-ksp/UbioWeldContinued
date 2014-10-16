using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace UbioWeldingLtd
{
	public static class FileManager
	{

		private static string _configFile = string.Concat(Constants.settingRuntimeDirectory, Constants.settingXmlFilePath, Constants.settingXmlConfigFileName);
		private static string _moduleListFile = string.Concat(Constants.settingRuntimeDirectory, Constants.settingXmlFilePath, Constants.settingXmlListFileName);

		private static string[] comments =
			{
				Constants.CommentOutText(Constants.setupGeneralLine1),
				Constants.CommentOutText(Constants.setupGeneralLine2),
				Constants.CommentOutText(Constants.setupGeneralLine3),
				
				Constants.CommentOutText(Constants.setupVector2Line1),
				Constants.CommentOutText(Constants.setupVector4Line1),
				Constants.CommentOutText(Constants.setupSubmoduleLine1),
				Constants.CommentOutText(Constants.setupModulesToIgnoreLine1),
				Constants.CommentOutText(Constants.setupAveragedAttribtesLine1),
				Constants.CommentOutText(Constants.setupUnchangedAttribtesLine1),
				Constants.CommentOutText(Constants.setupBreakingAttribtesLine1),
				Constants.CommentOutText(Constants.setupAddingAttributeEntryLine1),
			};


		/// <summary>
		/// catches unknown nodes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void serializer_UnknownNode
		(object sender, XmlNodeEventArgs e)
		{
			Console.WriteLine("Unknown Node:" + e.Name + "\t" + e.Text);
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
			Console.WriteLine("Unknown attribute " +
			attr.Name + "='" + attr.Value + "'");
		}


		/// <summary>
		/// loads configfile and prepares it with all needed lists
		/// </summary>
		/// <returns></returns>
		public static WeldingConfiguration loadConfig()
		{

			WeldingConfiguration configuration = new WeldingConfiguration();
			ModuleLists moduleList = new ModuleLists();
			FileStream FileStream;

			if (System.IO.File.Exists(_configFile))
			{
				XmlSerializer configSerializer = new XmlSerializer(typeof(WeldingConfiguration));
				configSerializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
				configSerializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
				FileStream = new FileStream(_configFile, FileMode.Open);
				configuration = (WeldingConfiguration)configSerializer.Deserialize(FileStream);
				FileStream.Close();

				if (configuration.MainWindowXPosition > (Screen.width - Constants.guiScreenEdgeClearance))
				{
					configuration.MainWindowXPosition = Screen.width - Constants.guiScreenEdgeClearance;
				}
				if (configuration.MainWindowYPosition > (Screen.height - Constants.guiScreenEdgeClearance))
				{
					configuration.MainWindowYPosition = Screen.height - Constants.guiScreenEdgeClearance;
				}
			}

			if (System.IO.File.Exists(_moduleListFile))
			{
				XmlSerializer moduleListSerializer = new XmlSerializer(typeof(ModuleLists));
				moduleListSerializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
				moduleListSerializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
				FileStream = new FileStream(_moduleListFile, FileMode.Open);
				moduleList = (ModuleLists)moduleListSerializer.Deserialize(FileStream);
				FileStream.Close();

				configuration.vector2CurveModules = WeldingHelpers.convertFromToStringArray(moduleList.vector2CurveModules);
				configuration.vector4CurveModules = WeldingHelpers.convertFromToStringArray(moduleList.vector4CurveModules);
				configuration.subModules = WeldingHelpers.convertFromToStringArray(moduleList.subModules);
				configuration.modulesToIgnore = WeldingHelpers.convertFromToStringArray(moduleList.modulesToIgnore);
				configuration.averagedModuleAttributes = WeldingHelpers.convertFromToStringArray(moduleList.averagedModuleAttributes);
				configuration.unchangedModuleAttributes = WeldingHelpers.convertFromToStringArray(moduleList.unchangedModuleAttributes);
				configuration.breakingModuleAttributes = WeldingHelpers.convertFromToStringArray(moduleList.breakingModuleAttributes);
			}
			else
			{
				configuration.vector2CurveModules = Constants.basicVector2CurveModules;
				configuration.vector4CurveModules = Constants.basicVector4CurveModules;
				configuration.subModules = Constants.basicSubModules;
				configuration.modulesToIgnore = Constants.basicModulesToIgnore;
				configuration.averagedModuleAttributes = Constants.basicAveragedModuleAttributes;
				configuration.unchangedModuleAttributes = Constants.basicUnchangedModuleAttributes;
				configuration.breakingModuleAttributes = Constants.basicBreakingModuleAttributes;
			}
			return configuration;
		}

		/// <summary>
		/// saves the config file and the modulelist file so that it is possible to change them without the need to recompile
		/// </summary>
		/// <param name="configToSave"></param>
		public static void saveConfig(WeldingConfiguration configToSave)
		{
			WeldingConfiguration configuration = configToSave; //this is not object copy but reference TO object copy operation
			ModuleLists moduleList = new ModuleLists();
			TextWriter fileStreamWriter;
			if (configuration == null)
			{
				configuration = new WeldingConfiguration();
			}
/*
			configuration.vector2CurveModules = null;
			configuration.vector4CurveModules = null;
			configuration.subModules = null;
			configuration.modulesToIgnore = null;
			configuration.averagedModuleAttributes = null;
			configuration.unchangedModuleAttributes = null;
			configuration.breakingModuleAttributes = null;
*/
			moduleList.vector2CurveModules = WeldingHelpers.convertStringFromToArray(Constants.basicVector2CurveModules);
			moduleList.vector4CurveModules = WeldingHelpers.convertStringFromToArray(Constants.basicVector4CurveModules);
			moduleList.subModules = WeldingHelpers.convertStringFromToArray(Constants.basicSubModules);
			moduleList.modulesToIgnore = WeldingHelpers.convertStringFromToArray(Constants.basicModulesToIgnore);
			moduleList.averagedModuleAttributes = WeldingHelpers.convertStringFromToArray(Constants.basicAveragedModuleAttributes);
			moduleList.unchangedModuleAttributes = WeldingHelpers.convertStringFromToArray(Constants.basicUnchangedModuleAttributes);
			moduleList.breakingModuleAttributes = WeldingHelpers.convertStringFromToArray(Constants.basicBreakingModuleAttributes);

			XmlSerializer configSerializer = new XmlSerializer(typeof(WeldingConfiguration));
			fileStreamWriter = new StreamWriter(_configFile);
			configSerializer.Serialize(fileStreamWriter, configuration);
			fileStreamWriter.Close();

			XmlSerializer moduleListSerializer = new XmlSerializer(typeof(ModuleLists));
			fileStreamWriter = new StreamWriter(_moduleListFile);
			moduleListSerializer.Serialize(fileStreamWriter, moduleList);
			
			foreach(string s in comments)
			{
				fileStreamWriter.WriteLine(s);
			}
			fileStreamWriter.Close();
		}

	}
}

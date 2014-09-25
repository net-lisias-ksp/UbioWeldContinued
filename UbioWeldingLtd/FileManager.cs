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
            }

			if (System.IO.File.Exists(_moduleListFile))
			{
				XmlSerializer moduleListSerializer = new XmlSerializer(typeof(ModuleLists));
				moduleListSerializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
				moduleListSerializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
				FileStream = new FileStream(_moduleListFile, FileMode.Open);
				moduleList = (ModuleLists)moduleListSerializer.Deserialize(FileStream);

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
            WeldingConfiguration configuration = configToSave;
			ModuleLists moduleList = new ModuleLists();
            TextWriter fileStreamWriter;
            if (configuration == null)
            {
                configuration = new WeldingConfiguration();
            }

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
			fileStreamWriter.Close();
        }

    }
}

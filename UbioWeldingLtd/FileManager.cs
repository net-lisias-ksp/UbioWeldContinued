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
            FileStream FileStream;

            if (System.IO.File.Exists(_configFile))
            {
                XmlSerializer configSerializer = new XmlSerializer(typeof(WeldingConfiguration));
                configSerializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
                configSerializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
                FileStream = new FileStream(_configFile, FileMode.Open);
                configuration = (WeldingConfiguration)configSerializer.Deserialize(FileStream);
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
            TextWriter fileStreamWriter;
            if (configuration == null)
            {
                configuration = new WeldingConfiguration();
            }

            XmlSerializer configSerializer = new XmlSerializer(typeof(WeldingConfiguration));
            fileStreamWriter = new StreamWriter(_configFile);
            configSerializer.Serialize(fileStreamWriter, configuration);
            fileStreamWriter.Close();
        }

    }
}

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UbioWeldingLtd
{

    [XmlRootAttribute("WeldingConfiguration", Namespace = "KSP-Forum", IsNullable = false)]
    public class WeldingConfiguration
    {
        private int _editorButtonXPosition = 190;
        private int _editorButtonYPosition = 100;
        private bool _dataBaseAutoReload = false;
        private bool _includeAllNodes = false;
        private bool _allowCareerMode = true;
        private bool _dontProcessMasslessParts = true;

        public int editorButtonX
        {
            get { return _editorButtonXPosition; }
            set { _editorButtonXPosition = value; }
        }

        public int editorButtonY
        {
            get { return _editorButtonYPosition; }
            set { _editorButtonYPosition = value; }
        }

        public bool dataBaseAutoReload
        {
            get { return _dataBaseAutoReload; }
            set { _dataBaseAutoReload = value; }
        }

        public bool includeAllNodes
        {
            get { return _includeAllNodes; }
            set { _includeAllNodes = value; }
        }

        public bool allowCareerMode
        {
            get { return _allowCareerMode; }
            set { _allowCareerMode = value; }
        }

        public bool dontProcessMasslessParts
        {
            get { return _dontProcessMasslessParts; }
            set { _dontProcessMasslessParts = value; }
        }
    }
}
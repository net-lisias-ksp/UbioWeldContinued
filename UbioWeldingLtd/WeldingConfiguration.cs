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
		private bool _runInTestMode = true;
		private bool _useStockToolbar = true;
		private string[] _vector2CurveModules;
		private string[] _vector4CurveModules;
		private string[] _subModules;
		private string[] _modulesToIgnore;
		private string[] _averagedModuleAttributes;
		private string[] _unchangedModuleAttributes;
		private string[] _breakingModuleAttributes;

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

		public bool runInTestMode
		{
			get { return _runInTestMode; }
			set { _runInTestMode = value; }
		}

		public bool useStockToolbar
		{
			get { return _useStockToolbar; }
			set { _useStockToolbar = value; }
		}

		public string[] vector2CurveModules
		{
			get { return _vector2CurveModules; }
			set { _vector2CurveModules = value; }
		}

		public string[] vector4CurveModules
		{
			get { return _vector4CurveModules; }
			set { _vector4CurveModules = value; }
		}

		public string[] subModules
		{
			get { return _subModules; }
			set { _subModules = value; }
		}

		public string[] modulesToIgnore
		{
			get { return _modulesToIgnore; }
			set { _modulesToIgnore = value; }
		}

		public string[] averagedModuleAttributes
		{
			get { return _averagedModuleAttributes; }
			set { _averagedModuleAttributes = value; }
		}

		public string[] unchangedModuleAttributes
		{
			get { return _unchangedModuleAttributes; }
			set { _unchangedModuleAttributes = value; }
		}

		public string[] breakingModuleAttributes
		{
			get { return _breakingModuleAttributes; }
			set { _breakingModuleAttributes = value; }
		}
	}
}
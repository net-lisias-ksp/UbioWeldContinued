using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UbioWeldingLtd
{
	public enum StrengthParamsCalcMethod
	{
		Legacy,
		ArithmeticMean,
		WeightedAverage
	}
	public enum MaxTempCalcMethod
	{
		ArithmeticMean,
		WeightedAverage,
		Lowest
	}

	[XmlRootAttribute("WeldingConfiguration", Namespace = "KSP-Forum", IsNullable = false)]
	public class WeldingConfiguration
	{

		public object clone()
		{
			return this.MemberwiseClone();
		}

		private int _MainWindowXPosition = (Screen.width - Constants.guiMainWindowW) / 2;
		private int _MainWindowYPosition = (Screen.height - Constants.guiMainWindowH) / 2;
		private bool _dataBaseAutoReload = false;
		private bool _includeAllNodes = false;
		private bool _allowCareerMode = true;
		private bool _dontProcessMasslessParts = true;
		private bool _runInTestMode = true;
		private bool _useStockToolbar = true;
		private bool _useNamedCfgFile = true; //save the part in the named file like "WeldedPod.cfg", not "part.cfg"
		private StrengthParamsCalcMethod _StrengthCalcMethod = StrengthParamsCalcMethod.WeightedAverage;
		private MaxTempCalcMethod _MaxTempCalcMethod = MaxTempCalcMethod.Lowest;
		private bool _clearEditor = true;
		private string[] _vector2CurveModules;
		private string[] _vector4CurveModules;
		private string[] _subModules;
		private string[] _modulesToIgnore;
		private string[] _modulesToMultiply;
		private string[] _averagedModuleAttributes;
		private string[] _unchangedModuleAttributes;
		private string[] _breakingModuleAttributes;

		public int MainWindowXPosition
		{
			get { return _MainWindowXPosition; }
			set { _MainWindowXPosition = value; }
		}

		public int MainWindowYPosition
		{
			get { return _MainWindowYPosition; }
			set { _MainWindowYPosition = value; }
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

		public bool useNamedCfgFile
		{
			get { return _useNamedCfgFile; }
			set { _useNamedCfgFile = value; }
		}

		public StrengthParamsCalcMethod StrengthCalcMethod
		{
			get { return _StrengthCalcMethod; }
			set { _StrengthCalcMethod = value; }
		}

		public MaxTempCalcMethod MaxTempCalcMethod
		{
			get { return _MaxTempCalcMethod; }
			set { _MaxTempCalcMethod = value; }
		}

		public bool clearEditor
		{
			get { return _clearEditor; }
			set { _clearEditor = value; }
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

		public string[] modulesToMultiply
		{
			get { return _modulesToMultiply; }
			set { _modulesToMultiply = value; }
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
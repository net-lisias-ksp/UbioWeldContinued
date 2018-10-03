﻿using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KSPe.IO;
using System.Text;

namespace UbioWeldingLtd
{

	/// <summary>
	/// class for the clean and readable serialsation
	/// </summary>
	public class ModuleAttribute
	{
		private string _name;

		[XmlAttribute("AttributeName")]
		public string attributeName
		{
			get { return _name; }
			set { _name = value; }
		}

		/// <summary>
		/// constructor for direct name assigning
		/// </summary>
		/// <param name="attributeName"></param>
		public ModuleAttribute(string attributeName)
		{
			this._name = attributeName;
		}

		/// <summary>
		/// default constructor
		/// </summary>
		public ModuleAttribute()
		{
		}
	}


	/// <summary>
	/// used as container for storing user generated module configs
	/// </summary>
	[XmlRootAttribute("ModuleAttributeLists", Namespace = "KSP-Forum", IsNullable = false)]
	public class ModuleLists
	{
		private static readonly string[] comments =
		{
			Constants.CommentOutText(Constants.setupGeneralLine1+"\n"),
			Constants.CommentOutText(Constants.setupGeneralLine2+"\n"),
			Constants.CommentOutText(Constants.setupGeneralLine3+"\n"),
			
			Constants.CommentOutText(Constants.setupVector2Line1+"\n"),
			Constants.CommentOutText(Constants.setupVector4Line1+"\n"),
			Constants.CommentOutText(Constants.setupSubmoduleLine1+"\n"),
			Constants.CommentOutText(Constants.setupModulesToIgnoreLine1+"\n"),
			Constants.CommentOutText(Constants.setupModulesToMultiplyLine1+"\n"),
			Constants.CommentOutText(Constants.setupMaximizedAttribtesLine1+"\n"),
			Constants.CommentOutText(Constants.setupAveragedAttribtesLine1+"\n"),
			Constants.CommentOutText(Constants.setupUnchangedAttribtesLine1+"\n"),
			Constants.CommentOutText(Constants.setupBreakingAttribtesLine1+"\n"),
			Constants.CommentOutText(Constants.setupAddingAttributeEntryLine1+"\n"),
		};
	

		[XmlAnyElement("Comment")]
		private XmlComment Comment
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				foreach (string s in comments) sb.AppendLine(s);
				return new XmlDocument().CreateComment(sb.ToString());
			}
			set { }
		}

		private ModuleAttribute[] _vector2CurveModules;
		private ModuleAttribute[] _vector4CurveModules;
		private ModuleAttribute[] _subModules;
		private ModuleAttribute[] _modulesToIgnore;
		private ModuleAttribute[] _modulesToMultiply;
		private ModuleAttribute[] _maximizedModuleAttributes;
		private ModuleAttribute[] _minimizedModuleAttributes;
		private ModuleAttribute[] _averagedModuleAttributes;
		private ModuleAttribute[] _unchangedModuleAttributes;
		private ModuleAttribute[] _breakingModuleAttributes;

		[XmlArrayAttribute("Vector2CurveModules")]
		public ModuleAttribute[] vector2CurveModules
		{
			get { return _vector2CurveModules; }
			set { _vector2CurveModules = value; }
		}

		[XmlArrayAttribute("Vector4CurveModules")]
		public ModuleAttribute[] vector4CurveModules
		{
			get { return _vector4CurveModules; }
			set { _vector4CurveModules = value; }
		}

		[XmlArrayAttribute("SubModules")]
		public ModuleAttribute[] subModules
		{
			get { return _subModules; }
			set { _subModules = value; }
		}

		[XmlArrayAttribute("ModulesToIgnore")]
		public ModuleAttribute[] modulesToIgnore
		{
			get { return _modulesToIgnore; }
			set { _modulesToIgnore = value; }
		}

		[XmlArrayAttribute("ModulesToMultiply")]
		public ModuleAttribute[] modulesToMultiply
		{
			get { return _modulesToMultiply; }
			set { _modulesToMultiply = value; }
		}

		[XmlArrayAttribute("MaximizedModuleAttributes")]
		public ModuleAttribute[] maximizedModuleAttributes
		{
			get { return _maximizedModuleAttributes; }
			set { _maximizedModuleAttributes = value; }
		}

		[XmlArrayAttribute("MinimizedModuleAttributes")]
		public ModuleAttribute[] minimizedModuleAttributes
		{
			get { return _minimizedModuleAttributes; }
			set { _minimizedModuleAttributes = value; }
		}

		[XmlArrayAttribute("AveragedModuleAttributes")]
		public ModuleAttribute[] averagedModuleAttributes
		{
			get { return _averagedModuleAttributes; }
			set { _averagedModuleAttributes = value; }
		}

		[XmlArrayAttribute("UnchangedModuleAttributes")]
		public ModuleAttribute[] unchangedModuleAttributes
		{
			get { return _unchangedModuleAttributes; }
			set { _unchangedModuleAttributes = value; }
		}

		[XmlArrayAttribute("BreakingModuleAttributes")]
		public ModuleAttribute[] breakingModuleAttributes
		{
			get { return _breakingModuleAttributes; }
			set { _breakingModuleAttributes = value; }
		}

	}
}

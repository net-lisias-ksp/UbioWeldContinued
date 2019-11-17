using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;

namespace UbioWeldingLtd
{
	class ModelInfo
	{
		public string url = string.Empty;
		public Vector3 position = Vector3.zero;
		public Vector3 rotation = Vector3.zero;
		public Vector3 scale = Vector3.one;
		public List<string> textures = new List<string>();
		public string parent = string.Empty;
	}

	enum WeldingReturn
	{
		// Warning
		MultipleLandingLegs = 22,
		MultipleScienceExp = 21,
		MultipleFXAnimateThrottle = 20,
		ModuleUnknown = 19,
		MultipleLaunchClamp = 18,
		MultipleFxPos = 17,
		MultipleFxLookAt = 16,
		MultipleWheel = 15,
		MultipleRetLadder = 14,
		MultipleLight = 13,
		MultipleParachutes = 12,
		MultipleRcs = 11,
		MultipleDocking = 10,
		MultipleDecouple = 9,
		MultipleAnimGen = 8,
		MultipleIntake = 7,
		MultipleEngine = 6,
		MultipleAnimHeat = 5,
		MultipleJettison = 4,
		MultipleSolarPan = 3,
		MultipleSeats = 2,
		MultipleInternal = 1,
		//Success
		Success = 0,
		//error
		MissingCfg = -1,
		MissingModel = -2
	}

	class Welder : ModuleMerger
	{
		private int _partNumber = 0;
		private string _name = Constants.weldDefaultName;
		private string _module = string.Empty;
		private List<ModelInfo> _models = new List<ModelInfo>();
		private float _rescaleFactor = Constants.weldRescaleFactor;
		private int _physicsSignificance = -1;

		private List<AttachNode> _attachNodes = new List<AttachNode>();
		private AttachNode _srfAttachNode = new AttachNode();

		private int _cost = 0;
		private int _crewCapacity = 0;
		private PartCategories _category = PartCategories.none;
		private string _subcat = Constants.weldSubcat;

		private string _title = Constants.weldDefaultTitle;
		private string _description = Constants.weldDefaultDesc;
		private AttachRules _attachrules = new AttachRules();
		private string _techRequire = string.Empty;
		private string _vesselType = string.Empty;
		private List<string> _listedTechs = new List<string>();
		private List<string> _listedVesselTypes = new List<string>();
		private int _entryCost = Constants.weldDefaultEntryCost;

		private double _mass = 0.0d;
		private double _fullmass = 0.0d;
		private string _dragModel = string.Empty;
		private float _minimumDrag = 0.0f;
		private float _maximumDrag = 0.0f;
		private float _angularDrag = 0.0f;
		private float _crashTolerance = 0.0f;
		private float _breakingForce = 0.0f;
		private float _breakingTorque = 0.0f;
		private float _maxTemp = 0.0f;

		private bool _fuelCrossFeed = false;

		private List<ConfigNode> _resourceslist = new List<ConfigNode>();
		private List<ConfigNode> _moduleList = new List<ConfigNode>();
		private ConfigNode _internalStorageNode = new ConfigNode();
		private ConfigNode _fxData = new ConfigNode();

		private Vector3 _coMOffset = Vector3.zero;
		private Vector3 _com = Vector3.zero;

        private Char _filePathDelimiter;
		private bool _advancedDebug = false;
		public ConfigNode FullConfigNode = new ConfigNode(Constants.weldPartNode);
		public ConfigNode FullInternalNode = new ConfigNode(Constants.weldInternalNode);
		private static bool _includeAllNodes = false;
		private static bool _dontProcessMasslessParts = false;
		private static bool _runInTestMode = false;
		private static StrengthParamsCalcMethod _StrengthCalcMethod = StrengthParamsCalcMethod.WeightedAverage;
		private static MaxTempCalcMethod _MaxTempCalcMethod = MaxTempCalcMethod.Lowest;
		private int[] partsHashMap;
		private static int _precisionDigits;
		private static bool _fileSimplification;
		private string _Internal = string.Empty;

		private float _explosionPotential = 0;
		private double _thermalMassModifier = 0;
		private double _heatConductivity = 0;
		private double _emissiveConstant = 0;
		private double _radiatorHeadroom = 0;
		private Vector3 _CoLOffset = Vector3.zero;
		private Vector3 _CoPOffset = Vector3.zero;
		private string _bulkheadProfiles = string.Empty;
		private int _stackSymmetry = 0;
		private string _stringStackSymmetry = "0";

		private int _modelIndex = 0;
		private bool _meshSwitchRequired = false;
		private List<int> _meshSwitchModelIndicies = new List<int>();
		private List<string> _meshSwitchTransformNames = new List<string>();

		public string bulkheadProfiles
		{
			get { return _bulkheadProfiles; }
		}

		public Vector3 CoLOffset
		{
			get { return _CoLOffset; }
		}

		public Vector3 CoPOffset
		{
			get { return _CoPOffset; }
		}

		public float explosionPotential
		{
			get { return _explosionPotential; }
		}

		public double thermalMassModifier
		{
			get { return _thermalMassModifier; }
		}

		public double heatConductivity
		{
			get { return _heatConductivity; }
		}

		public double emissiveConstant
		{
			get { return _emissiveConstant; }
		}

		public double radiatorHeadroom
		{
			get { return _radiatorHeadroom; }
		}

		public bool isMeshSwitchRequired
		{
			get { return _meshSwitchRequired; }
		}

		public static bool includeAllNodes
		{
			get { return _includeAllNodes; }
			set { _includeAllNodes = value; }
		}

		public static bool dontProcessMasslessParts
		{
			get { return _dontProcessMasslessParts; }
			set { _dontProcessMasslessParts = value; }
		}

		public static bool runInTestMode
		{
			get { return _runInTestMode; }
			set { _runInTestMode = value; }
		}

		public static bool fileSimplification
		{
			get { return _fileSimplification; }
			set { _fileSimplification = value; }
		}

		public static int precisionDigits
		{
			get { return _precisionDigits; }
			set { _precisionDigits = value; }
		}

		public static StrengthParamsCalcMethod StrengthCalcMethod
		{
			get { return _StrengthCalcMethod; }
			set { _StrengthCalcMethod = value; }
		}

		public static MaxTempCalcMethod MaxTempCalcMethod
		{
			get { return _MaxTempCalcMethod; }
			set { _MaxTempCalcMethod = value; }
		}

		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				_name = _name.Replace(' ', '-');
				_name = _name.Replace('.', '-');
				_name = _name.Replace('\\', '-');
				_name = _name.Replace('/', '-');
				_name = _name.Replace(':', '-');
				_name = _name.Replace('*', '-');
				_name = _name.Replace('?', '-');
				_name = _name.Replace('<', '-');
				_name = _name.Replace('>', '-');
				_name = _name.Replace('|', '-');
				_name = _name.Replace('_', '-');
			}
		}
		public string Title { get { return _title; } set { _title = value; } }
		public string Description { get { return _description; } set { _description = value; } }
		public int Cost { get { return _cost; } }
		public double Mass { get { return _mass; } }
		public double WetMass { get { return _fullmass; } }
		public bool FuelCrossFeed { get { return _fuelCrossFeed; } set { _fuelCrossFeed = value; } }
		public float MinDrag { get { return _minimumDrag; } }
		public float MaxDrag { get { return _maximumDrag; } }
		public float CrashTolerance { get { return _crashTolerance; } }
		public float BreakingForce { get { return _breakingForce; } }
		public float BreakingTorque { get { return _breakingTorque; } }
		public float MaxTemp { get { return _maxTemp; } }
		public float NbParts { get { return _partNumber; } }
		public string Internal { get { return _Internal; } set { _Internal = value; } }

		public string[] Modules
		{
			get
			{
				string[] moduleslist = new string[_moduleList.Count];
				int index = 0;
				foreach (ConfigNode cfgnode in _moduleList)
				{
					moduleslist[index] = cfgnode.GetValue(Constants.weldModuleNodeName);
					++index;
				}
				return moduleslist;
			}
		}

		public List<ConfigNode> moduleList
		{
			get { return _moduleList; }
		}

		public string[] Resources
		{
			get
			{
				string[] resourceslist = new string[_resourceslist.Count * 2];
				int index = 0;
				foreach (ConfigNode cfgnode in _resourceslist)
				{
					resourceslist[index++] = cfgnode.GetValue(Constants.weldModuleNodeName);
					resourceslist[index++] = string.Format("{0} / {1}", cfgnode.GetValue("amount"), cfgnode.GetValue("maxAmount"));
				}
				return resourceslist;
			}
		}

		public PartCategories Category
		{
//PartCategories.none must be replaced with something else
            get { return (_category != PartCategories.none) ? _category : PartCategories.Utility; }
			set { _category = value; }
		}

		public string vesselType
		{
			get { return _vesselType; }
			set { _vesselType = value; }
		}

		public string techRequire
		{
			get { return _techRequire; }
			set { _techRequire = value; }
		}

		public List<string> techList
		{
			get { return _listedTechs; }
		}

		public List<string> vesselTypeList
		{
			get { return _listedVesselTypes; }
		}

		public int stackSymmetry
		{
			get { return _stackSymmetry; }
			set { _stackSymmetry = value; }
		}

		public string stringStackSymmetry
		{
			get { return _stringStackSymmetry; }
			set { _stringStackSymmetry = value; }
		}


		/*
		 * Constructor
		 */
		public Welder(bool advancedDebug)
        {
            //in Linux and OSX delimiters in file path are '/', not '\'
            if ((Application.platform == RuntimePlatform.LinuxPlayer) || (Application.platform == RuntimePlatform.OSXPlayer))
            {
                _filePathDelimiter = '/';
            }
            else
            {
                _filePathDelimiter = '\\';
            }
			_advancedDebug = advancedDebug;
			loadPartHashMap();
        }


		private Quaternion worldOrientation = new Quaternion();
		private Quaternion rotationMatrix;

		public void init()
		{
			rotationMatrix = Quaternion.Inverse(worldOrientation);
		}


		/*
		 * Set relative position
		 */
		private void setRelativePosition(Part part, ref Vector3 position)
		{
			Debugger.AdvDebug(string.Format("..set relative position ={0} | localroot={1}", part.transform.position.ToString("F3"), part.localRoot.transform.position.ToString("F3")), _advancedDebug);
			//position += part.transform.position - part.localRoot.transform.position;
			position += rotationMatrix * part.transform.position;
		}


		/*
		 * Set relative rotation
		 */
		private void setRelativeRotation(Part part, ref Vector3 rotation)
		{
			rotation += part.transform.eulerAngles - part.localRoot.transform.eulerAngles;

			rotation.x = WeldingHelpers.angleClamp(rotation.x, 0, 360);
			rotation.y = WeldingHelpers.angleClamp(rotation.y, 0, 360);
			rotation.z = WeldingHelpers.angleClamp(rotation.z, 0, 360);
		}


		/*
		 * Process the new center of mass to the models and node
		 */
		public void processNewCoM()
		{
			foreach (ModelInfo model in _models)
			{
				model.position -= _com;
			}
			foreach (AttachNode node in _attachNodes)
			{
				node.position -= _com;
			}
			processInternalToCoM(_internalStorageNode);
		}



		public void processInternalToCoM(ConfigNode controlNode)
		{
			Vector3 pos;
			foreach (ConfigNode node in controlNode.GetNodes())
			{
				if(node.HasValue("position"))
				{
					pos = ConfigNode.ParseVector3(node.GetValue("position"));
					node.SetValue("position", new Vector3(pos.x - _com.x, pos.y - _com.z, pos.z - _com.y));
				}
			}
		}


		/*
		 * Get the mesh name
		 */
		private string GetMeshurl(UrlDir.UrlConfig cfgdir)
		{
			string mesh = "model";
			//in case the mesh is not model.mu
			if (cfgdir.config.HasValue("mesh"))
			{
				mesh = cfgdir.config.GetValue("mesh");
				char[] sep = { '.' };
				string[] words = mesh.Split(sep);
				mesh = words[0];
			}
            string filename = string.Format("{0}" + _filePathDelimiter + "{1}.mu", cfgdir.parent.parent.path, mesh);
			string url = string.Format("{0}/{1}", cfgdir.parent.parent.url, mesh);

			//in case the mesh name does not exist (.22 bug)
			if (!File.Exists(filename))
			{
				Debug.LogWarning(string.Format("{0}{1}.!{2} {3}", Constants.logWarning, Constants.logPrefix, Constants.logWarnNoMesh, filename));
				string[] files = Directory.GetFiles(cfgdir.parent.parent.path, "*.mu");
				if (files.Length != 0)
				{
					Debugger.AdvDebug(string.Format("..cfgdir.parent.parent.path {0}", cfgdir.parent.parent.path), _advancedDebug);
					Debugger.AdvDebug(string.Format("..files[0] {0}", files[0]), _advancedDebug);
					Debugger.AdvDebug(string.Format("..cfgdir.parent.parent.path.Length {0}", cfgdir.parent.parent.path.Length), _advancedDebug);
					files[0] = files[0].Remove(0, cfgdir.parent.parent.path.Length);

					Debugger.AdvDebug(string.Format("{0}.New mesh name: {1}", Constants.logPrefix, files[0]), _advancedDebug);

					char[] sep = { '\\','.', '/' };
					string[] words = files[0].Split(sep);

					Debugger.AdvDebug(string.Format("..words[1] {0}", words[1]), _advancedDebug);
					Debugger.AdvDebug(string.Format("..mesh {0}", mesh), _advancedDebug);

//					url = url.Replace(string.Format(@"{0}", mesh), words[1]);
					url = url.Substring(0, url.LastIndexOf('/') + 1) + words[1];

					Debugger.AdvDebug(string.Format("..url {0}", url), _advancedDebug);

				}
				else
				{
					Debug.LogWarning(string.Format("{0}{1}.No mesh found, using default", Constants.logWarning, Constants.logPrefix));
				}
			}

			return url;
		}


		public void prepareWeldedMeshSwitchModule(List<ConfigNode> moduleList)
		{
			ConfigNode newWeldedMeshSwitch = new ConfigNode(Constants.weldModuleNode);

			string indexString = string.Empty;
			string transformNamesString = string.Empty;

			indexString = WeldingHelpers.loadListIntoString(indexString, _meshSwitchModelIndicies, Constants.weldedMeshSwitchSplitter);
			transformNamesString = WeldingHelpers.loadListIntoString(transformNamesString, _meshSwitchTransformNames, Constants.weldedMeshSwitchSplitter);

			newWeldedMeshSwitch.AddValue(Constants.weldModuleNodeName, Constants.weldedmeshSwitchModule);
			newWeldedMeshSwitch.AddValue("objectIndicies", indexString);
			newWeldedMeshSwitch.AddValue("objects", transformNamesString);
			newWeldedMeshSwitch.AddValue("advancedDebug", _advancedDebug);
			newWeldedMeshSwitch.AddValue("destroyUnusedParts", true);

			moduleList.Add(newWeldedMeshSwitch);
		}


		/// <summary>
		/// loads the names for the transforms and the Index into the weldedMeshSwitch
		/// </summary>
		/// <param name="weldingPart"></param>
		private void loadMeshSwitchValues(Part weldingPart)
		{
			Transform modelTransform = weldingPart.FindModelTransform(Constants.weldModelNode.ToLower()).GetChild(0);
			List<string> transformList = new List<string>();
			string listEntry = string.Empty;

			while (modelTransform.childCount < 2 && modelTransform.childCount != 0)
			{
				if (modelTransform.GetChild(0) != null)
				{
					modelTransform = modelTransform.GetChild(0);
				}
			}

			foreach (Transform t in modelTransform)
			{
				if (t.gameObject.activeSelf)
				{
					if (string.IsNullOrEmpty(listEntry))
					{
						listEntry = t.name;
					}
					else
					{
						listEntry += Constants.weldedMeshSwitchSubSplitter + t.name;
					}
				}
			}
			if (listEntry.Length > 0)
			{
				_meshSwitchModelIndicies.Add(_modelIndex);
				_meshSwitchTransformNames.Add(listEntry);
			}
		}


		/// <summary>
		/// adds the actual modelinfo to the partconfig and checks if there is a meshswitch integrated so that it prepares the meshes from the part
		/// </summary>
		/// <param name="modelToAdd"></param>
		/// <param name="containsMeshSwitch"></param>
		/// <param name="weldingPart"></param>
		private void addNewModel(ModelInfo modelToAdd, bool containsMeshSwitch, Part weldingPart)
		{
			if (containsMeshSwitch)
			{
				_meshSwitchRequired = true;
				loadMeshSwitchValues(weldingPart);
			}
			_models.Add(modelToAdd);
			_modelIndex++;
		}


		/// <summary>
		/// a generell check of the partconfig if it contains a InterstellarMeshSwitch
		/// </summary>
		/// <param name="partconfig"></param>
		/// <returns></returns>
		private bool doesPartContainMeshSwitch(UrlDir.UrlConfig partconfig)
		{
			ConfigNode[] originalModules = partconfig.config.GetNodes(Constants.weldModuleNode);
			string moduleName = string.Empty;
			foreach (ConfigNode module in originalModules)
			{
				moduleName = module.GetValue(module.values.DistinctNames()[0]);
				if (WeldingHelpers.isArrayContaining(moduleName, Constants.interstellarMeshSwitchModule))
				{
					return true;
				}
			}
			return false;
		}


		/// <summary>
		/// a method to check for decals and replaces them with the welded ones
		/// </summary>
		/// <param name="moduleList"></param>
		public void prepDecals(List<ConfigNode> moduleList)
		{
			int decals = 0;
			List<ConfigNode> decalsmodules = new List<ConfigNode>();
			List<int> moduleIndicies = new List<int>();

			for (int i =0; i < moduleList.Count;i++)
			{
				ConfigNode n = moduleList[i];
				if(n.GetValue(n.values.DistinctNames()[0]) == Constants.originalDecalModule)
				{
					decals++;
					decalsmodules.Add(n);
					moduleIndicies.Add(i);
				}
			}

			if(decals > 0)
			{
				for(int i =0; i<decalsmodules.Count; i++)
				{
					ConfigNode node = new ConfigNode(Constants.weldModuleNode);
					node.AddValue(Constants.weldModuleNodeName, Constants.weldedDecalModule);
					node.AddValue(Constants.weldedDecalModuleValueName, decalsmodules[i].GetValue(Constants.weldedDecalModuleValueName));

					//moduleList.RemoveAt(moduleIndicies[i]);
					moduleList.Add(node);
				}
			}
		}


		/*
		 * Weld a new part
		 */
		public WeldingReturn weldThisPart(Part newpart)
		{
			_coMOffset = Vector3.zero;
			WeldingReturn ret = WeldingReturn.Success;
			string partname = (string)newpart.partInfo.partPrefab.name.Clone();
			WeldingHelpers.removeTextRegex(ref partname, "(Clone)");

			Debug.Log(string.Format("{0}{1}{2}",Constants.logPrefix,Constants.logWeldingPart,partname));
			Debugger.AdvDebug(string.Format("..part rescaleFactor {0:F}", newpart.rescaleFactor), _advancedDebug);
			Debugger.AdvDebug(string.Format("..part scaleFactor {0:F}", newpart.scaleFactor), _advancedDebug);
			getAttachmentType(newpart);

			//--- Find all the config file with the name
			List<UrlDir.UrlConfig> matchingPartConfigs = new List<UrlDir.UrlConfig>();
			List<UrlDir.UrlConfig> matchingInternalConfigs = new List<UrlDir.UrlConfig>();
			foreach (UrlDir.UrlConfig config in GameDatabase.Instance.root.GetConfigs(Constants.weldPartNode))
			{
				string newconfigname = config.name.Replace('_', '.');

				if (System.String.Equals(partname, newconfigname, System.StringComparison.Ordinal))
				{
					matchingPartConfigs.Add(config);
					if (config.config.HasNode(Constants.weldInternalNode))
					{
						foreach (UrlDir.UrlConfig cfg in GameDatabase.Instance.root.GetConfigs(Constants.weldInternalNode))
						{
							foreach (ConfigNode node in config.config.GetNodes(Constants.weldInternalNode))
							{
								if (System.String.Equals(node.GetValues()[0], cfg.name, System.StringComparison.Ordinal))
								{
									matchingInternalConfigs.Add(cfg);
								}
							}
						}
					}
				}
			}

			Debugger.AdvDebug(string.Format(".Found {0} config files", matchingPartConfigs.Count), _advancedDebug);

			if (matchingPartConfigs.Count < 1)
			{
				//Missing Config File: Error
				Debug.LogError(string.Format("{0}{1}.{2} {3}", Constants.logError, Constants.logPrefix, Constants.msgCfgMissing, partname));
				return WeldingReturn.MissingCfg;
			}
			else // 0 < matchingPartConfigs.Count
			{
				//Process Config Files
				ModelInfo info = new ModelInfo();
				foreach (UrlDir.UrlConfig cfg in matchingPartConfigs)
				{
					//MODEL
					if (!cfg.config.HasNode(Constants.weldModelNode))
					{
						//Missing Model node
						Debugger.AdvDebug(string.Format("..Config {0} has no {1} node", cfg.name, Constants.weldModelNode), _advancedDebug);

						info = new ModelInfo();
						info.url = GetMeshurl(cfg);
						Debugger.AdvDebug(string.Format("..{0}{1}", Constants.logModelUrl, info.url), _advancedDebug);

						Vector3 position = Vector3.zero;
						position = newpart.transform.position;
						info.position = WeldingHelpers.RoundVector3(position, _precisionDigits);

						Vector3 rotation = newpart.localRoot.transform.eulerAngles;
						setRelativeRotation(newpart, ref rotation);
						info.rotation = WeldingHelpers.RoundVector3(WeldingHelpers.limitRotationAngle(rotation), _precisionDigits);


						Debugger.AdvDebug(string.Format("scaling info: rescaleFactor={0}| vector={1}", newpart.rescaleFactor, newpart.transform.GetChild(0).localScale.ToString("F3")), _advancedDebug);
						Transform modelTransform = newpart.partTransform.Find(Constants.weldModelNode.ToLower());
						Vector3 scale = modelTransform.localScale;
						//Debugger.AdvDebug(string.Format("scale = {0} | localScale = {1} | lossyScale = {0}", scale,modelTransform.GetChild(subModelIndex).localScale, modelTransform.GetChild(subModelIndex).lossyScale), true);
						Transform parentTransform = modelTransform.parent;
						while (parentTransform != newpart.transform)
						{
							scale = Vector3.Scale(scale, parentTransform.localScale);
							parentTransform = parentTransform.parent;
							//Debugger.AdvDebug(string.Format("scale = {0} | localScale = {1} | lossyScale = {0}", scale, parentTransform.localScale, parentTransform.lossyScale), true);
						}
						scale = Vector3.Scale(scale, newpart.transform.localScale);
						//Debugger.AdvDebug(string.Format("scale = {0} | localScale = {1} | lossyScale = {0}", scale, newpart.transform.localScale, newpart.transform.lossyScale), true);
						info.scale = WeldingHelpers.RoundVector3(scale, _precisionDigits);

						//Debugger.AdvDebug("fileSimplification = " + _fileSimplification, true);
						if (_fileSimplification)
						{
							//Debugger.AdvDebug("simplified", true);
							if (newpart.rescaleFactor == _rescaleFactor)
							{
								//Debugger.AdvDebug("rescaleFactor", true);
								if (newpart.scaleFactor == 1)
								{
									//Debugger.AdvDebug("scaleFactor", true);
									if (info.scale == Vector3.one)
									{
										//Debugger.AdvDebug("Vector3.one", true);
										info.scale = Vector3.zero;
									}
								}
							}
						}

						Debugger.AdvDebug(string.Format("..newpart position {0}", newpart.transform.position.ToString("F3")), _advancedDebug);
						Debugger.AdvDebug(string.Format("..newpart rotation {0}", newpart.transform.rotation.ToString("F3")), _advancedDebug);
						Debugger.AdvDebug(string.Format("..newpart rotation.eulerAngles {0}", newpart.transform.rotation.eulerAngles.ToString("F3")), _advancedDebug);
						Debugger.AdvDebug(string.Format("..newpart rotation.localEulerAngles {0}", newpart.transform.localEulerAngles.ToString("F3")), _advancedDebug);
						Debugger.AdvDebug(string.Format("..newpart localRoot.rotation {0}", newpart.localRoot.transform.rotation.ToString("F3")), _advancedDebug);
						Debugger.AdvDebug(string.Format("..newpart localRoot.rotation.eulerAngles {0}", newpart.localRoot.transform.rotation.eulerAngles.ToString("F3")), _advancedDebug);
						Debugger.AdvDebug(string.Format("..position {0}", info.position.ToString("F3")), _advancedDebug);
						Debugger.AdvDebug(string.Format("..rotation {0}", info.rotation.ToString("F3")), _advancedDebug);
						Debugger.AdvDebug(string.Format("..scale {0}", info.scale.ToString("F3")), _advancedDebug);

						addNewModel(info, doesPartContainMeshSwitch(cfg), newpart);
					}
					else //cfg.config.HasNode(Constants.weldModelNode)
					{
						ConfigNode[] modelnodes = cfg.config.GetNodes(Constants.weldModelNode);
						Debugger.AdvDebug(string.Format("..Config {0} has {1} {2} node", cfg.name, modelnodes.Length, Constants.weldModelNode), _advancedDebug);

						int subModelIndex = 0;
						foreach (ConfigNode node in modelnodes)
						{
							info = new ModelInfo();

							if (node.HasValue("model"))
							{
								info.url = node.GetValue("model");
							}
							else
							{
								info.url = GetMeshurl(cfg);
							}
							Debugger.AdvDebug(string.Format("..{0}{1}", Constants.logModelUrl, info.url), _advancedDebug);
							Vector3 position = (node.HasValue("position")) ? (ConfigNode.ParseVector3(node.GetValue("position")) * newpart.rescaleFactor) : Vector3.zero;
							Debugger.AdvDebug(string.Format("..node.HasValue(\"position\") {0}", node.HasValue("position")), _advancedDebug);
							Debugger.AdvDebug(string.Format("..node position {0}", position.ToString("F3")), _advancedDebug);
							position = newpart.FindModelTransform(Constants.weldModelNode.ToLower()).GetChild(subModelIndex).position;
							info.position = WeldingHelpers.RoundVector3(position,_precisionDigits);

							Vector3 rotation = (node.HasValue("rotation")) ? ConfigNode.ParseVector3(node.GetValue("rotation")) : Vector3.zero;
							Debugger.AdvDebug(string.Format("..node.HasValue(\"rotation\") {0}", node.HasValue("rotation")), _advancedDebug);
							Debugger.AdvDebug(string.Format("..node rotation {0}", rotation.ToString("F3")), _advancedDebug);
							rotation = newpart.FindModelTransform(Constants.weldModelNode.ToLower()).GetChild(subModelIndex).rotation.eulerAngles;
							info.rotation = WeldingHelpers.RoundVector3(WeldingHelpers.limitRotationAngle(rotation),_precisionDigits);

							Debugger.AdvDebug(string.Format("..node.HasValue(\"scale\") {0}", node.HasValue("scale")), _advancedDebug);
							if (node.HasValue("scale"))
							{
								Debugger.AdvDebug(string.Format("..node scale {0}", node.GetValue("scale")), _advancedDebug);
							}
							Debugger.AdvDebug(string.Format("..Childs count {0}", newpart.transform.childCount), _advancedDebug);

							Debugger.AdvDebug(string.Format("scaling info: rescaleFactor={0}| scale={1}| config.scale={2}", newpart.rescaleFactor, newpart.scaleFactor, node.HasValue("scale") ? ConfigNode.ParseVector3(node.GetValue("scale")).ToString("F3") : Vector3.zero.ToString("F3")), _advancedDebug);
							Transform modelTransform = newpart.partTransform.Find(Constants.weldModelNode.ToLower());
							Vector3 scale = modelTransform.GetChild(subModelIndex).localScale;
							//Debugger.AdvDebug(string.Format("scale = {0} | localScale = {1} | lossyScale = {0}", scale,modelTransform.GetChild(subModelIndex).localScale, modelTransform.GetChild(subModelIndex).lossyScale), true);
							Transform parentTransform = modelTransform;
							while (parentTransform != newpart.transform)
							{
								scale = Vector3.Scale(scale, parentTransform.localScale);
								parentTransform = parentTransform.parent;
								//Debugger.AdvDebug(string.Format("scale = {0} | localScale = {1} | lossyScale = {0}", scale, parentTransform.localScale, parentTransform.lossyScale), true);
							}
							scale = Vector3.Scale(scale, newpart.transform.localScale);
							//Debugger.AdvDebug(string.Format("scale = {0} | localScale = {1} | lossyScale = {0}", scale, newpart.transform.localScale, newpart.transform.lossyScale), true);
							info.scale = WeldingHelpers.RoundVector3(scale, _precisionDigits);

							//Debugger.AdvDebug("fileSimplification = " + _fileSimplification, true);
							if (_fileSimplification)
							{
								//Debugger.AdvDebug("simplified", true);
								if (newpart.rescaleFactor == _rescaleFactor)
								{
									//Debugger.AdvDebug("rescaleFactor", true);
									if (newpart.scaleFactor == 1)
									{
										//Debugger.AdvDebug("scaleFactor", true);
										if (info.scale == Vector3.one || (node.HasValue("scale") && ConfigNode.ParseVector3(node.GetValue("scale")) * newpart.scaleFactor * newpart.rescaleFactor == info.scale))
										{
											//Debugger.AdvDebug("Vector3.one", true);
											info.scale = Vector3.zero;
										}
									}
								}
							}

							Debugger.AdvDebug(string.Format("..newpart position {0}", newpart.transform.position.ToString("F3")), _advancedDebug);
							Debugger.AdvDebug(string.Format("..newpart rotation {0}", newpart.transform.rotation.ToString("F3")), _advancedDebug);
							Debugger.AdvDebug(string.Format("..newpart rotation.eulerAngles {0}", newpart.transform.rotation.eulerAngles.ToString("F3")), _advancedDebug);
							Debugger.AdvDebug(string.Format("..newpart rotation.localEulerAngles {0}", newpart.transform.localEulerAngles.ToString("F3")), _advancedDebug);
							Debugger.AdvDebug(string.Format("..newpart localRoot.rotation {0}", newpart.localRoot.transform.rotation.ToString("F3")), _advancedDebug);
							Debugger.AdvDebug(string.Format("..newpart localRoot.rotation.eulerAngles {0}", newpart.localRoot.transform.rotation.eulerAngles.ToString("F3")), _advancedDebug);
							Debugger.AdvDebug(string.Format("..position {0}", info.position.ToString("F3")), _advancedDebug);
							Debugger.AdvDebug(string.Format("..rotation {0}", info.rotation.ToString("F3")), _advancedDebug);
							Debugger.AdvDebug(string.Format("..scale {0}", info.scale.ToString("F3")), _advancedDebug);

							if (node.HasValue("texture"))
							{
								foreach (string tex in node.GetValues("texture"))
								{
									info.textures.Add(tex);
									Debugger.AdvDebug(string.Format("..texture {0}", tex), _advancedDebug);
								}
							}
							if (node.HasValue("parent"))
							{
								info.parent = node.GetValue("parent");
							}
							addNewModel(info, doesPartContainMeshSwitch(cfg), newpart);
							subModelIndex++;
						}
					}
					_coMOffset += newpart.transform.position;

					mergeResources(newpart, _resourceslist);

					//MODULE
					ConfigNode[] originalModules = cfg.config.GetNodes(Constants.weldModuleNode);
					Debugger.AdvDebug(string.Format("..Config {0} has {1} {2} node", cfg.name, originalModules.Length, Constants.weldModuleNode), _advancedDebug);
					Debugger.AdvDebug(string.Format(".. running in Alewx Testmode = {0}", runInTestMode), _advancedDebug);

					if (runInTestMode)
					{
						mergeModules(partname, cfg, _moduleList, _advancedDebug);

						newpart.CreateInternalModel();

						if (cfg.config.HasNode(Constants.weldInternalNode))
						{
							mergeInternals(newpart, matchingInternalConfigs);
						}
					}
					else
					{
						ret = OldModuleMerge(ret, partname, cfg);
					}

					//manage the fx group
					//foreach (fxgroup fx in newpart.fxgroups)
					//{
					//	debugger.advdebug(string.format("..config {0} has {1} fxemitters and {2} sound in {3} fxgroups", cfg.name, fx.fxemitters.count, (null != fx.sfx) ? "1" : "0", fx.name), _advanceddebug);

					//	if (!fx.name.contains("rcsgroup")) //rcs fx are not store in the config file
					//	{
					//		foreach (particleemitter gobj in fx.fxemitters)
					//		{
					//			string fxname = gobj.name;
					//			weldinghelpers.removetextregex(ref fxname, "(clone)");
					//			string fxvalue = cfg.config.getvalue(fxname);
					//			string[] allvalue = regex.split(fxvalue, ", ");
					//			vector3 pos = new vector3(float.parse(allvalue[0]), float.parse(allvalue[1]), float.parse(allvalue[2]));
					//			vector3 ang = new vector3(float.parse(allvalue[3]), float.parse(allvalue[4]), float.parse(allvalue[5]));
					//			setrelativeposition(newpart, ref pos);
					//			fxvalue = string.format("{0}, {1}, {2}, {3}, {4}, {5}, {6}", pos.x, pos.y, pos.z, ang.x, ang.y, ang.z, allvalue[6]);
					//			for (int i = 7; i < allvalue.length; ++i)
					//			{
					//				fxvalue = string.format("{0}, {1}", fxvalue, allvalue[i]);
					//			}
					//			_fxdata.addvalue(fxname, fxvalue);
					//			debugger.advdebug(string.format("..{0}{1}", constants.logfxadd, fxname), _advanceddebug);
					//		}
					//		if (fx.sfx != null)
					//		{
					//			_fxdata.addvalue(fx.sfx.name, fx.name);
					//			debugger.advdebug(string.format("..{0}{1}", constants.logfxadd, fx.sfx.name), _advanceddebug);
					//		}
					//	}
					//} //foreach (FXGroup fx in newpart.fxGroups)
				} //foreach (UrlDir.UrlConfig cfg in matchingPartConfigs)
			} //else of if (0 >= matchingPartConfigs.Count)

			//ATTACHNODE
			Debugger.AdvDebug(string.Format(".Part {0} has {1} Stack attach node(s)", partname, newpart.attachNodes.Count), _advancedDebug);

			foreach (AttachNode partnode in newpart.attachNodes)
			{
				//only add node if not attached to another part (or if requested in the condig file)
				if (_includeAllNodes || partnode.attachedPart == null || (partnode.attachedPart != null && !isChildPart(newpart,partnode.attachedPart)))
				{
					AttachNode node = partnode; //make sure we don't overwrite the part node
					node.id += partname + _partNumber;
					Matrix4x4 rot = Matrix4x4.TRS(Vector3.zero, newpart.transform.rotation, Vector3.one);
					node.position = rot.MultiplyVector(node.position);
					node.orientation = rot.MultiplyVector(node.orientation);
					setRelativePosition(newpart, ref node.position);

					_attachNodes.Add(node);
					Debugger.AdvDebug(string.Format(".{0}{1}", Constants.logNodeAdd, node.id), _advancedDebug);
				}
			} //foreach (AttachNode node in newpart.attachNodes)

			//reads the techtreenodes
			if (!_listedTechs.Contains(newpart.partInfo.TechRequired))
			{
				_listedTechs.Add(newpart.partInfo.TechRequired);
			}

			if (!string.IsNullOrEmpty(newpart.partInfo.bulkheadProfiles))
			{
				_bulkheadProfiles = newpart.partInfo.bulkheadProfiles;
			}

			if (newpart.CoLOffset != null && newpart.CoLOffset != Vector3.zero)
			{
				_CoLOffset += newpart.CoLOffset;
			}

			if (newpart.CoPOffset != null && newpart.CoPOffset != Vector3.zero)
			{
				_CoPOffset += newpart.CoPOffset;
			}

			if (newpart.explosionPotential != 0)
			{
				if (_explosionPotential == 0)
				{
					_explosionPotential = newpart.explosionPotential;
				}
				else
				{
					_explosionPotential = (_explosionPotential + newpart.explosionPotential) / 2;
				}
			}

			if (newpart.heatConductivity != 0)
			{
				if (heatConductivity == 0)
				{
					_heatConductivity = newpart.heatConductivity;
				}
				else
				{
					_heatConductivity = (_heatConductivity + newpart.heatConductivity) / 2;
				}
			}

			if (newpart.emissiveConstant != 0)
			{
				if (_emissiveConstant == 0)
				{
					_emissiveConstant = newpart.emissiveConstant;
				}
				else
				{
					_emissiveConstant = (_emissiveConstant + newpart.emissiveConstant) / 2;
				}
			}

			if (newpart.thermalMassModifier != 0)
			{
				if (_thermalMassModifier == 0)
				{
					_thermalMassModifier = newpart.thermalMassModifier;
				}
				else
				{
					_thermalMassModifier = (_thermalMassModifier + newpart.thermalMassModifier) / 2;
				}
			}

			if (newpart.radiatorHeadroom != 0)
			{
				if (_radiatorHeadroom == 0)
				{
					_radiatorHeadroom = newpart.radiatorHeadroom;
				}
				else
				{
					_radiatorHeadroom = (_radiatorHeadroom + newpart.radiatorHeadroom) / 2;
				}
			}

			//reads the vesseltype if that exists
			Debugger.AdvDebug(string.Format(".. VesselType - {0}", newpart.vesselType), _advancedDebug);
			if (newpart.vesselType != VesselType.Debris && newpart.vesselType != VesselType.Flag && newpart.vesselType != VesselType.Unknown)
			{
				if (!_listedVesselTypes.Contains(newpart.vesselType.ToString()))
				{
					_listedVesselTypes.Add(newpart.vesselType.ToString());
				}
			}

			//Cost
			_cost += (int)newpart.partInfo.cost;
			_entryCost += (int)newpart.partInfo.entryCost;
			_crewCapacity += newpart.CrewCapacity;

			// srfAttachNode Rules
			_attachrules.allowDock = _attachrules.allowDock || newpart.attachRules.allowDock;
			_attachrules.allowRotate = _attachrules.allowRotate || newpart.attachRules.allowRotate;
			_attachrules.allowSrfAttach = _attachrules.allowSrfAttach || newpart.attachRules.allowSrfAttach;
			_attachrules.allowStack = _attachrules.allowStack || newpart.attachRules.allowStack;
			_attachrules.srfAttach = _attachrules.srfAttach || newpart.attachRules.srfAttach;
			_attachrules.stack = _attachrules.stack || newpart.attachRules.stack;

			//mass
			double oldmass = _fullmass;
			double olddrymass = _mass;
			double partdrymass = 0.0f;
			// if part's PhysicsSignificance = 1, then this part is "massless" and its mass would be ignored in stock KSP
			//if ((!dontProcessMasslessParts) || (newpart.PhysicsSignificance != 1))
			//{
				partdrymass = newpart.mass;
			//}

			double partwetmass = partdrymass + newpart.GetResourceMass();

			_mass += partdrymass;
			_fullmass += partwetmass;

			if (_fullmass > 0)
			{
				if ((!dontProcessMasslessParts) || (newpart.PhysicsSignificance != 1))
				{
					_com = ((_com * (float)oldmass) + (_coMOffset * (float)partwetmass)) / (float)_fullmass;
				}
			}

			Debugger.AdvDebug(string.Format("New Center of Mass: {0}", _com.ToString()), _advancedDebug);
			//Drag (Add)
			_minimumDrag = (_minimumDrag + newpart.minimum_drag) * 0.5f;
			_maximumDrag = (_maximumDrag + newpart.maximum_drag) * 0.5f;
			_angularDrag = (_angularDrag + newpart.angularDrag) * 0.5f;
			//TODO: modify type
			//completly outdates as it looks
			_dragModel = "default";

			//average crash, breaking and temp
			switch (_StrengthCalcMethod)
			{
				case StrengthParamsCalcMethod.Legacy:
					_crashTolerance = (_partNumber == 0) ? newpart.crashTolerance : (_crashTolerance + newpart.crashTolerance) * 0.75f;
					_breakingForce = (_partNumber == 0) ? newpart.breakingForce : (_breakingForce + newpart.breakingForce) * 0.75f;
					_breakingTorque = (_partNumber == 0) ? newpart.breakingTorque : (_breakingTorque + newpart.breakingTorque) * 0.75f;
					break;
				case StrengthParamsCalcMethod.WeightedAverage:
					_crashTolerance = (_partNumber == 0) ? newpart.crashTolerance : (_crashTolerance * (float)olddrymass + newpart.crashTolerance * newpart.mass) / ((float)olddrymass + newpart.mass);
					_breakingForce = (_partNumber == 0) ? newpart.breakingForce : (_breakingForce * (float)olddrymass + newpart.breakingForce * newpart.mass) / ((float)olddrymass + newpart.mass);
					_breakingTorque = (_partNumber == 0) ? newpart.breakingTorque : (_breakingTorque * (float)olddrymass + newpart.breakingTorque * newpart.mass) / ((float)olddrymass + newpart.mass);
					break;
				case StrengthParamsCalcMethod.ArithmeticMean:
					_crashTolerance = (_partNumber == 0) ? newpart.crashTolerance : (_crashTolerance + newpart.crashTolerance) * 0.5f;
					_breakingForce = (_partNumber == 0) ? newpart.breakingForce : (_breakingForce + newpart.breakingForce) * 0.5f;
					_breakingTorque = (_partNumber == 0) ? newpart.breakingTorque : (_breakingTorque + newpart.breakingTorque) * 0.5f;
					break;
			}
			Debugger.AdvDebug(string.Format("Part crashTolerance: {0} - Global crashTolerance: {1} - method: {2}", newpart.crashTolerance, _crashTolerance, _StrengthCalcMethod), _advancedDebug);
			Debugger.AdvDebug(string.Format("Part crashTolerance: {0} - Global crashTolerance: {1} - method: {2}", newpart.breakingForce, _breakingForce, _StrengthCalcMethod), _advancedDebug);
			Debugger.AdvDebug(string.Format("Part breakingTorque: {0} - Global breakingTorque: {1} - method: {2}", newpart.breakingTorque, _breakingTorque, _StrengthCalcMethod), _advancedDebug);

			switch (_MaxTempCalcMethod)
			{
				case MaxTempCalcMethod.ArithmeticMean:
					_maxTemp = (_partNumber == 0) ? (float)newpart.maxTemp : (float)(_maxTemp + newpart.maxTemp) * 0.5f;
					break;
				case MaxTempCalcMethod.Lowest:
					_maxTemp = (_partNumber == 0) ? (float)newpart.maxTemp : (float)Math.Min(_maxTemp, newpart.maxTemp);
					break;
				case MaxTempCalcMethod.WeightedAverage:
					_maxTemp = (_partNumber == 0) ? (float)newpart.maxTemp : (float)(_maxTemp * (float)olddrymass + newpart.maxTemp * newpart.mass) / ((float)olddrymass + newpart.mass);
					break;
			}
			Debugger.AdvDebug(string.Format("Part maxTemp: {0} - Global maxTemp: {1} - method: {2}", newpart.maxTemp, _maxTemp, _MaxTempCalcMethod), _advancedDebug);

			//Phisics signifance
			if (newpart.PhysicsSignificance != 0 && _physicsSignificance != -1)
			{
				_physicsSignificance = newpart.PhysicsSignificance;
			}

			if (_partNumber == 0)
			{
				//TODO: Find where to find it in game. Would that be pre .15 stuff? http://forum.kerbalspaceprogram.com/threads/7529-Plugin-Posting-Rules-And-Official-Documentation?p=156430&viewfull=1#post156430
				_module = "Part";
				//
				Debugger.AdvDebug(string.Format("weldThisPart - newpart.partInfo.category: {0}", newpart.partInfo.category.ToString()), _advancedDebug);
				_category = newpart.partInfo.category;
				//TODO: better surface node managment
				_srfAttachNode = newpart.srfAttachNode;
				//Fuel crossfeed: TODO: test different ways to managed it
				_fuelCrossFeed = newpart.fuelCrossFeed;
				//
				_physicsSignificance = newpart.PhysicsSignificance;
			}
			_partNumber++;
			return ret;
		}


		/// <summary>
		/// merges all the internal model merging
		/// </summary>
		/// <param name="_newpart"></param>
		/// <param name="_matchingInternalConfigs"></param>
		private void mergeInternals(Part _newpart, List<UrlDir.UrlConfig> _matchingInternalConfigs)
		{
			foreach (UrlDir.UrlConfig internalNode in _matchingInternalConfigs)
			{
				Vector3 relPos = (rotationMatrix * _newpart.transform.position);
				Quaternion relRot = _newpart.transform.rotation;
				relRot.eulerAngles += new Vector3(0, 0, 180);
				relRot.eulerAngles = new Vector3(relRot.eulerAngles.x, relRot.eulerAngles.z, relRot.eulerAngles.y);
				Debugger.AdvDebug("pos = " + relPos + " | rot = " + relRot, _advancedDebug);

				if (!internalNode.config.HasNode(Constants.weldModelNode))
				{
					Debugger.AdvDebug("- No Model in Internal found, locating modelfile", _advancedDebug);
					Debugger.AdvDebug(string.Format("- {0}{1}", Constants.logModelUrl, GetMeshurl(internalNode)), _advancedDebug);
					ConfigNode newNode = new ConfigNode(Constants.weldModelNode);
					newNode.AddValue("model", GetMeshurl(internalNode));
					if (relPos != Vector3.zero)
					{
						newNode.AddValue("position", new Vector3(relPos.x, relPos.z, relPos.y));
						Debugger.AdvDebug("- position = " + (relPos - _com), _advancedDebug);
					}
					if (relRot.eulerAngles != Vector3.zero)
					{
						newNode.AddValue("rotation", relRot.eulerAngles);
						Debugger.AdvDebug("- rotation = " + relRot.eulerAngles, _advancedDebug);
					}
					_internalStorageNode.AddNode(newNode);
				}
				foreach (ConfigNode n in internalNode.config.GetNodes())
				{
					ConfigNode node = n.CreateCopy();
					Debugger.AdvDebug("- " + node.name, _advancedDebug);

					if (!node.name.Equals(Constants.weldModuleNode))
					{
						assignInternalRotation(relRot, node);
						assignInternalPosition(relPos, relRot, node);
					}
					_internalStorageNode.AddNode(node);
				}
			}
		}


		/// <summary>
		/// manages the rotating of the internal models
		/// </summary>
		/// <param name="_relRot"></param>
		/// <param name="_node"></param>
		private void assignInternalRotation(Quaternion _relRot, ConfigNode _node)
		{
			if (_node.name.Equals(Constants.weldModelNode))
			{
				if (_node.HasValue("rotation"))
				{
					Vector3 eul = ConfigNode.ParseVector3(_node.GetValue("rotation"));
					_node.SetValue("rotation", WeldingHelpers.limitRotationAngle(eul + _relRot.eulerAngles));
					Debugger.AdvDebug("- rotation found | euler in config " + eul + " | _relative euler " + _relRot.eulerAngles + " | result " + WeldingHelpers.limitRotationAngle(eul + _relRot.eulerAngles), _advancedDebug);
				}
				else
				{
					Debugger.AdvDebug("- no rotation in config " + _relRot.eulerAngles, _advancedDebug);
					_node.AddValue("rotation", WeldingHelpers.limitRotationAngle(_relRot.eulerAngles));
				}
			}
			if (_node.name.Equals(Constants.weldPropNode))
			{
				if (_node.HasValue("rotation"))
				{
					Quaternion rot = ConfigNode.ParseQuaternion(_node.GetValue("rotation"));
					rot.eulerAngles += _relRot.eulerAngles;
					_node.SetValue("rotation", rot);
				}
				else
				{
					_node.AddValue("rotation", _relRot);
				}
			}
		}


		/// <summary>
		/// manages the positioning of the internal models
		/// </summary>
		/// <param name="_relPos"></param>
		/// <param name="_node"></param>
		private void assignInternalPosition(Vector3 _relPos, Quaternion _relRot, ConfigNode _node)
		{
			if (_node.name.Equals(Constants.weldModelNode))
			{
				_relPos = new Vector3(_relPos.x, _relPos.z, _relPos.y);
				if (_node.HasValue("position"))
				{
					if (_relPos != Vector3.zero)
					{
						Vector3 _pos = ConfigNode.ParseVector3(_node.GetValue("position"));
						_node.SetValue("position", (_pos + _relPos));
						Debugger.AdvDebug("- updated internal position = " + (_pos + _relPos), _advancedDebug);
					}
				}
				else
				{
					if (_relPos != Vector3.zero)
					{
						_node.AddValue("position", _relPos);
						Debugger.AdvDebug("- new internal position = " + _relPos, _advancedDebug);
					}
				}
			}
			if (_node.name.Equals(Constants.weldPropNode))
			{
				Vector3 _newPos = (Quaternion.Inverse(_relRot) * _relPos);
				_newPos = new Vector3(_newPos.x, _newPos.z, _newPos.y);
				if (_node.HasValue("position"))
				{
					if (_relPos != Vector3.zero)
					{
						Vector3 _pos = ConfigNode.ParseVector3(_node.GetValue("position"));
						_node.SetValue("position", (_pos + _newPos));
						Debugger.AdvDebug("- updated internal position = " + (_pos + _newPos), _advancedDebug);
					}
				}
				else
				{
					if (_relPos != Vector3.zero)
					{
						_node.AddValue("position", _newPos);
						Debugger.AdvDebug("- new internal position = " + _newPos, _advancedDebug);
					}
				}
			}
		}


		/// <summary>
		/// manages the merging of the resources of all parts
		/// </summary>
		/// <param name="newpart"></param>
		/// <param name="resourcesList"></param>
		private void mergeResources(Part newpart, List<ConfigNode> resourcesList)
		{
			List<PartResource> newPartResourcesList = newpart.Resources.ToList();
			Debugger.AdvDebug(string.Format("..Part {0} has {1} {2} node", newpart.partName, newPartResourcesList.Count, Constants.weldResNode), _advancedDebug);
			foreach (PartResource partRes in newPartResourcesList)
			{
				string resourceName = partRes.resourceName;
				float resourceAmount = float.Parse(partRes.amount.ToString());
				float resourceMax = float.Parse(partRes.maxAmount.ToString());
				bool exist = false;
				foreach (ConfigNode rescfg in resourcesList)
				{
					if (string.Equals(resourceName, rescfg.GetValue(Constants.weldModuleNodeName)))
					{
						rescfg.SetValue("amount", (resourceAmount + float.Parse(rescfg.GetValue("amount"))).ToString());
						rescfg.SetValue("maxAmount", (resourceMax + float.Parse(rescfg.GetValue("maxAmount"))).ToString());
						exist = true;
						Debugger.AdvDebug(string.Format("..{0}{1} {2}/{3}", Constants.logResMerge, resourceName, resourceAmount, resourceMax), _advancedDebug);
						break;
					}
				}
				if (!exist)
				{
					ConfigNode resourceNode = new ConfigNode(Constants.weldResNode);
					resourceNode.AddValue(Constants.weldModuleNodeName, resourceName);
					resourceNode.AddValue("amount", resourceAmount.ToString());
					resourceNode.AddValue("maxAmount", resourceMax.ToString());
					resourcesList.Add(resourceNode);
					Debugger.AdvDebug(string.Format("..{0}{1} {2}/{3}", Constants.logResAdd, resourceName, resourceAmount, resourceMax), _advancedDebug);
				}
			}
		}


		/// <summary>
		/// checks if the given Module is included in the part.
		/// returns true when a wanted module is found.
		/// </summary>
		/// <param name="moduleSearchArray"></param>
		/// <param name="moduleType"></param>
		/// <returns></returns>
		private bool hasModuleType(ConfigNode[] moduleSearchArray, string moduleType)
		{
			foreach (ConfigNode singleModule in moduleSearchArray)
			{
				if (moduleType.Equals(singleModule.GetValue(singleModule.values.DistinctNames()[0])))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// provides the wanted module from a inserted list of modules.
		/// returns the first found module of the given type, if no module was found retursn null
		/// </summary>
		/// <param name="moduleSearchArray"></param>
		/// <param name="moduleType"></param>
		/// <returns></returns>
		private ConfigNode getModuleOfType(ConfigNode[] moduleSearchArray, string moduleType)
		{
			foreach (ConfigNode singleModule in moduleSearchArray)
			{
				if (moduleType.Equals(singleModule.GetValue(singleModule.values.DistinctNames()[0])))
				{
					return singleModule;
				}
			}
			return null;
		}


		/// <summary>
		/// the original module merging method.
		/// </summary>
		/// <param name="ret"></param>
		/// <param name="partname"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		private WeldingReturn OldModuleMerge(WeldingReturn ret, string partname, UrlDir.UrlConfig configuration)
		{
			ConfigNode[] originalModules = configuration.config.GetNodes(Constants.weldModuleNode);

			foreach (ConfigNode originalModule in originalModules)
			{
				ConfigNode newModule = originalModule.CreateCopy();
				string newModuleName = newModule.GetValue(newModule.values.DistinctNames()[0]);
				bool exist = false;

				foreach (ConfigNode existingNewModule in _moduleList)
				{
					if (string.Equals(newModuleName, existingNewModule.GetValue(existingNewModule.values.DistinctNames()[0])))
					{
						switch (newModuleName)
						{
							//case Constants.modStockSas:
							//	{
							//		// don't add SAS modules together.
							//		break;
							//	}
							case Constants.modStockGear:			//Don't add (.21)
#if (DEBUG)
								Debug.Log(string.Format("{0}..{1}{2}", Constants.logPrefix, Constants.logModIgnore, newModuleName));
#endif
								exist = true;
								break;
							case Constants.modStockReacWheel:	   //Add reaction wheel force
								float pitch = float.Parse(existingNewModule.GetValue("PitchTorque")) + float.Parse(newModule.GetValue("PitchTorque"));
								float yaw = float.Parse(existingNewModule.GetValue("YawTorque")) + float.Parse(newModule.GetValue("YawTorque"));
								float roll = float.Parse(existingNewModule.GetValue("RollTorque")) + float.Parse(newModule.GetValue("RollTorque"));
								float wheelrate = float.Parse(existingNewModule.GetNode(Constants.weldResNode).GetValue("rate")) + float.Parse(newModule.GetNode(Constants.weldResNode).GetValue("rate"));
								existingNewModule.SetValue("PitchTorque", pitch.ToString());
								existingNewModule.SetValue("YawTorque", yaw.ToString());
								existingNewModule.SetValue("RollTorque", roll.ToString());
								existingNewModule.GetNode(Constants.weldResNode).SetValue("rate", wheelrate.ToString());
#if (DEBUG)
								Debug.Log(string.Format("{0}..{1}{2}", Constants.logPrefix, Constants.logModMerge, newModuleName));
#endif
								exist = true;
								break;
							case Constants.modStockCommand:		// Add Crew and Electricity ressources //TODO: Manage all used ressources
								int crew = int.Parse(newModule.GetValue("minimumCrew")) + int.Parse(existingNewModule.GetValue("minimumCrew"));
								existingNewModule.SetValue("minimumCrew", crew.ToString());
								if (newModule.HasNode(Constants.weldResNode))
								{
									if (existingNewModule.HasNode(Constants.weldResNode))
									{
										float comrate = float.Parse(newModule.GetNode(Constants.weldResNode).GetValue("rate")) + float.Parse(existingNewModule.GetNode(Constants.weldResNode).GetValue("rate"));
										existingNewModule.GetNode(Constants.weldResNode).SetValue("rate", comrate.ToString());
									}
									else
									{
										existingNewModule.AddNode(newModule.GetNode(Constants.weldResNode));
									}
								}
#if(DEBUG)
								Debug.Log(string.Format("{0}..{1}{2}", Constants.logPrefix, Constants.logModMerge, newModuleName));
#endif
								exist = true;
								break;
							case Constants.modStockGen:			// Add Generator Values //TODO: Manage output ressource name.
								bool active = bool.Parse(newModule.GetValue("isAlwaysActive")) && bool.Parse(existingNewModule.GetValue("isAlwaysActive"));
								float genrate = float.Parse(newModule.GetNode(Constants.weldOutResNode).GetValue("rate")) + float.Parse(existingNewModule.GetNode(Constants.weldOutResNode).GetValue("rate"));
								existingNewModule.SetValue("isAlwaysActive", active.ToString());
								existingNewModule.GetNode(Constants.weldOutResNode).SetValue("rate", genrate.ToString());
#if(DEBUG)
								Debug.Log(string.Format("{0}..{1}{2}", Constants.logPrefix, Constants.logModMerge, newModuleName));
#endif
								exist = true;
								break;
							case Constants.modStockAltern:		 //add the alternator value
								float altrate = float.Parse(newModule.GetNode(Constants.weldResNode).GetValue("rate")) + float.Parse(existingNewModule.GetNode(Constants.weldResNode).GetValue("rate"));
								existingNewModule.GetNode(Constants.weldResNode).SetValue("rate", altrate.ToString());
#if(DEBUG)
								Debug.Log(string.Format("{0}..{1}{2}", Constants.logPrefix, Constants.logModMerge, newModuleName));
#endif
								exist = true;
								break;
							case Constants.modStockGimbal:	  //average the gimbal range TODO: test the gimbal
								int gimbal = (int.Parse(newModule.GetValue("gimbalRange")) + int.Parse(existingNewModule.GetValue("gimbalRange"))) / 2;
								existingNewModule.SetValue("gimbalRange", gimbal.ToString());
#if(DEBUG)
								Debug.Log(string.Format("{0}..{1}{2}", Constants.logPrefix, Constants.logModMerge, newModuleName));
#endif
								exist = true;
								break;
							case Constants.modStockSensor:	 // Allow one sensor module per different sensor
								exist = string.Equals(newModule.GetValue("sensorType"), existingNewModule.GetValue("sensorType"));
#if(DEBUG)
								Debug.Log(string.Format("{0}..{1}{2}", Constants.logPrefix, (exist) ? Constants.logModIgnore : Constants.logModMerge, newModuleName));
#endif
								break;
							case Constants.modStockEngine:		// Average/add value and warning
								bool exhaustDamage = bool.Parse(newModule.GetValue("exhaustDamage")) || bool.Parse(existingNewModule.GetValue("exhaustDamage"));
								float ignitionThreshold = (float.Parse(newModule.GetValue("ignitionThreshold")) + float.Parse(existingNewModule.GetValue("ignitionThreshold"))) * 0.5f;
								float minThrust = float.Parse(newModule.GetValue("minThrust")) + float.Parse(existingNewModule.GetValue("minThrust"));
								float maxThrust = float.Parse(newModule.GetValue("maxThrust")) + float.Parse(existingNewModule.GetValue("maxThrust"));
								int heatProduction = (int.Parse(newModule.GetValue("heatProduction")) + int.Parse(existingNewModule.GetValue("heatProduction"))) / 2;
								existingNewModule.SetValue("exhaustDamage", exhaustDamage.ToString());
								existingNewModule.SetValue("ignitionThreshold", ignitionThreshold.ToString());
								existingNewModule.SetValue("minThrust", minThrust.ToString());
								existingNewModule.SetValue("maxThrust", maxThrust.ToString());
								existingNewModule.SetValue("heatProduction", heatProduction.ToString());
								//fx offset
								if (newModule.HasValue("fxOffset"))
								{
									Vector3 fxOffset = ConfigNode.ParseVector3(newModule.GetValue("fxOffset"));
									//setRelativePosition(newpart, ref fxOffset);
									//Vector3 cfgFxOffset = ConfigNode.ParseVector3(existingNewModule.GetValue("fxOffset")) + fxOffset;
									newModule.SetValue("fxOffset", ConfigNode.WriteVector(fxOffset));
								}
								//Propellant nodes
								ConfigNode[] Propellant = newModule.GetNodes(Constants.weldEngineProp);
								foreach (ConfigNode prop in Propellant)
								{
									//look if one exist
									ConfigNode[] cfgPropellant = existingNewModule.GetNodes(Constants.weldEngineProp);
									bool propexist = false;
									foreach (ConfigNode cfgprop in cfgPropellant)
									{
										if (string.Equals(cfgprop.GetValue(Constants.weldModuleNodeName), prop.GetValue(Constants.weldModuleNodeName)))
										{
											float ratio = float.Parse(prop.GetValue("ratio")) + float.Parse(cfgprop.GetValue("ratio"));
											cfgprop.SetValue("ratio", ratio.ToString());
											propexist = true;
											break;
										}
									}
									if (!propexist)
									{
										existingNewModule.SetNode(Constants.weldEngineProp, prop);
									}
								}
								if (newModule.HasNode(Constants.weldEngineAtmCurve))
								{
									if (existingNewModule.HasNode(Constants.weldEngineAtmCurve))
									{
										//merge
										string[] curve = newModule.GetNode(Constants.weldEngineAtmCurve).GetValues("key");
										string[] cfgcurve = existingNewModule.GetNode(Constants.weldEngineAtmCurve).GetValues("key");
										Vector2[] cfgcurvevect = MergeAtmCurve(curve, cfgcurve);
										existingNewModule.GetNode(Constants.weldEngineAtmCurve).RemoveValues("key");
										foreach (Vector2 vec in cfgcurvevect)
										{
											existingNewModule.GetNode(Constants.weldEngineAtmCurve).AddValue("key", ConfigNode.WriteVector(vec));
										}
									}
									else
									{
										existingNewModule.AddNode(newModule.GetNode(Constants.weldEngineAtmCurve));
									}
								}
								if (newModule.HasNode(Constants.weldEngineVelCurve))
								{
									if (existingNewModule.HasNode(Constants.weldEngineVelCurve))
									{
										//merge
										string[] curve = newModule.GetNode(Constants.weldEngineVelCurve).GetValues("key");
										string[] cfgcurve = existingNewModule.GetNode(Constants.weldEngineVelCurve).GetValues("key");
										Vector4[] cfgcurvevect = MergeVelCurve(curve, cfgcurve);
										existingNewModule.GetNode(Constants.weldEngineVelCurve).RemoveValues("key");
										foreach (Vector4 vec in cfgcurvevect)
										{
											existingNewModule.GetNode(Constants.weldEngineVelCurve).AddValue("key", ConfigNode.WriteVector(vec));
										}
									}
									else
									{
										existingNewModule.AddNode(newModule.GetNode(Constants.weldEngineVelCurve));
									}
								}
#if (DEBUG)
								Debug.Log(string.Format("{0}..{1}{2} !{3}", Constants.logPrefix, Constants.logModMerge, newModuleName, Constants.msgWarnModEngine));
#endif
								exist = true;
								break;
							case Constants.modStockAnimHeat:
								exist = string.Equals(existingNewModule.GetValue("ThermalAnim"), newModule.GetValue("ThermalAnim"));
#if (DEBUG)
								Debug.Log(string.Format("{0}..{1}{2}", Constants.logPrefix, (exist) ? Constants.logModIgnore : Constants.logModMerge, newModuleName));
#endif
								break;
							case Constants.modStockAnimGen:		// Warning for Multiple Animate Generic
								exist = string.Equals(existingNewModule.GetValue("animationName"), newModule.GetValue("animationName"));
								if (exist)
								{
#if (DEBUG)
									Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModAnimGen));
#endif
									ret = WeldingReturn.MultipleAnimGen;
								}
								break;
							case Constants.modStockInternal:   // Warning for multiple interal and ignore
#if (DEBUG)
								Debug.LogWarning(string.Format("{0}{1}..{2}{3} !{4}", Constants.logWarning, Constants.logPrefix, Constants.logModIgnore, newModuleName, Constants.msgWarnModInternal));
#endif
								ret = WeldingReturn.MultipleInternal;
								exist = true;
								break;
							case Constants.modStockSeat:	   // Warning for Multiple seats //TODO: Test
#if (DEBUG)
								Debug.LogWarning(string.Format("{0}{1}..{2}{3} !{4}", Constants.logWarning, Constants.logPrefix, Constants.logModIgnore, newModuleName, Constants.msgWarnModSeat));
#endif
								ret = WeldingReturn.MultipleSeats;
								exist = true;
								break;
							case Constants.modStockSolarPan:	   // Warning for Multiple Deployable Solar Panel //TODO: Test
#if (DEBUG)
								Debug.LogWarning(string.Format("{0}{1}..{2}{3} !{4}", Constants.logWarning, Constants.logPrefix, Constants.logModIgnore, newModuleName, Constants.msgWarnModSolPan));
#endif
								ret = WeldingReturn.MultipleSolarPan;
								exist = true;
								break;
							case Constants.modStockJettison:	   // Warning for Multiple Jetison //Only one is working fairing is working.
#if (DEBUG)
								Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModJetttison));
#endif
								ret = WeldingReturn.MultipleJettison;
								exist = false;
								break;
							case Constants.modStockFxAnimThro:	   // Warning for Multiple FX animate. // Only the first one is working, the other are ignore
#if (DEBUG)
								Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModFxAnimTh));
#endif
								ret = WeldingReturn.MultipleFXAnimateThrottle;
								exist = false;
								break;
							case Constants.modStockIntake:		// Warning for Multiple Intake //TODO: Test
#if (DEBUG)
								Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModIntake));
#endif
								ret = WeldingReturn.MultipleIntake;
								exist = false;
								break;
							case Constants.modStockDecouple:
							case Constants.modStockAnchdec:		//Warning for Multiple Decoupler, change the node //TODO: Test
#if (DEBUG)
								Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModDecouple));
#endif
								ret = WeldingReturn.MultipleDecouple;
								exist = false;
								break;
							case Constants.modStockDocking:		//Warning for Multiple Dockingport
#if (DEBUG)
								Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModDocking));
#endif
								ret = WeldingReturn.MultipleDocking;
								exist = false;
								break;
							case Constants.modStockRCS:		//Warning for Multiple RCS
#if (DEBUG)
								Debug.LogWarning(string.Format("{0}{1}..{2}{3} !{4}", Constants.logWarning, Constants.logPrefix, Constants.logModIgnore, newModuleName, Constants.msgWarnModRcs));
#endif
								//ret = WeldingReturn.MultipleRcs;
								exist = true;
								break;
							case Constants.modStockParachutes:		//Warning for Multiple Parachutes //TODO: Test
#if (DEBUG)
								Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModParachute));
#endif
								ret = WeldingReturn.MultipleParachutes;
								exist = false;
								break;
							case Constants.modStockLight:		//Warning for Multiple Light //TODO: Test
#if (DEBUG)
								Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModLight));
#endif
								ret = WeldingReturn.MultipleLight;
								exist = false;
								break;
							case Constants.modStockRetLadder:		//Warning for Multiple Retractable ladder //TODO: Test
#if (DEBUG)
								Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModRetLadder));
#endif
								ret = WeldingReturn.MultipleRetLadder;
								exist = false;
								break;
							case Constants.modStockWheel:		//Warning for Multiple Wheels //TODO: Test
#if (DEBUG)
								Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModWheel));
#endif
								ret = WeldingReturn.MultipleWheel;
								exist = false;
								break;
							case Constants.modStockFxLookAt:		//Warning for Multiple FxLookAt Constraint (wome with wheels) //TODO: Test
#if (DEBUG)
								Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModFxLookAt));
#endif
								ret = WeldingReturn.MultipleFxLookAt;
								exist = false;
								break;
							case Constants.modStockFxPos:		//Warning for Multiple Constraint Position (wome with wheels) //TODO: Test
#if (DEBUG)
								Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModFxPos));
#endif
								ret = WeldingReturn.MultipleFxPos;
								exist = false;
								break;
							case Constants.modStockLaunchClamp:		//Warning for Multiple Launching Clamp (I don't even why would it be needed
#if (DEBUG)
								Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModLaunClamp));
#endif
								ret = WeldingReturn.MultipleLaunchClamp;
								exist = false;
								break;
							case Constants.modStockScienceExp:		// Warning for Multiple Science Experiments (.22)
								exist = string.Equals(existingNewModule.GetValue("experimentID"), newModule.GetValue("experimentID"));
								if (exist)
								{
#if (DEBUG)
									Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModScieExp));
#endif
									ret = WeldingReturn.MultipleScienceExp;
								}
								break;
							case Constants.modstockTransData:		// Merge transmition data (.22)
								float packetInterval = (float.Parse(newModule.GetValue("packetInterval")) + float.Parse(existingNewModule.GetValue("packetInterval"))) * 0.5f;
								float packetSize = (float.Parse(newModule.GetValue("packetSize")) + float.Parse(existingNewModule.GetValue("packetSize")));
								float packetResourceCost = (float.Parse(newModule.GetValue("packetResourceCost")) + float.Parse(existingNewModule.GetValue("packetResourceCost")));
								//TODO: requiredResource / DeployFxModules 

								existingNewModule.SetValue("packetInterval", packetInterval.ToString());
								existingNewModule.SetValue("packetSize", packetSize.ToString());
								existingNewModule.SetValue("packetResourceCost", packetResourceCost.ToString());
#if (DEBUG)
								Debug.Log(string.Format("{0}..{1}{2}", Constants.logPrefix, Constants.logModMerge, newModuleName));
#endif
								exist = true;
								break;
							case Constants.modStockLandingLegs:		// Waring Multiple same landing legs
								exist = string.Equals(existingNewModule.GetValue("animationName"), newModule.GetValue("animationName"));
								if (exist)
								{
#if (DEBUG)
									Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModLandLegs));
#endif
									ret = WeldingReturn.MultipleLandingLegs;
								}
								break;
							case Constants.modStockScienceCont:		// Merge Science Container (.22)
								bool evaOnlyStorage = bool.Parse(newModule.GetValue("evaOnlyStorage")) || bool.Parse(existingNewModule.GetValue("evaOnlyStorage"));
								float storageRange = (float.Parse(newModule.GetValue("storageRange")) + float.Parse(existingNewModule.GetValue("storageRange")));
								//TODO: requiredResource / DeployFxModules 

								existingNewModule.SetValue("evaOnlyStorage", evaOnlyStorage.ToString());
								existingNewModule.SetValue("storageRange", storageRange.ToString());
#if (DEBUG)
								Debug.Log(string.Format("{0}..{1}{2}", Constants.logPrefix, Constants.logModMerge, newModuleName));
#endif
								exist = true;
								break;
							default:
								{
									// New update module or mods! not managed
#if (DEBUG)
									Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModUnknown));
#endif
									ret = WeldingReturn.ModuleUnknown;
									exist = false;
									break;
								}
						}
					}
				}//foreach (ConfigNode existingNewModule in _modulelist)
				if (!exist)
				{
					switch (newModule.GetValue(Constants.weldModuleNodeName))
					{
						//case Constants.modStockDecouple:
						//	{
						//		break;
						//	}
						case Constants.modStockAnchdec:
							{//Decoupler: Change node name
								string decouplename = newModule.GetValue("explosiveNodeID") + partname + _partNumber;
								newModule.SetValue("explosiveNodeID", decouplename);
								break;
							}
						case Constants.modStockDocking:
							{//Docking port: Change node name if any TODO: FIX This
								if (newModule.HasValue("referenceAttachNode"))
								{
									string dockname = newModule.GetValue("referenceAttachNode") + partname + _partNumber;
									newModule.SetValue("referenceAttachNode", dockname);
								}
								break;
							}
						case Constants.modStockJettison:
							{//Fairing/Jetisson, change node name
								string jetissonname = newModule.GetValue("bottomNodeName") + partname + _partNumber;
								newModule.SetValue("bottomNodeName", jetissonname);
								break;
							}
					}
					_moduleList.Add(newModule);
#if (DEBUG)
					Debug.Log(string.Format("{0}..{1}{2}", Constants.logPrefix, Constants.logModAdd, newModuleName));
#endif
				} //if (!exist)
			} //foreach (ConfigNode mod in modules)
			return ret;
		}

		/*
		 * Get the full ConfigNode
		 */
		public void CreateFullConfigNode()
		{
			FullConfigNode = new ConfigNode(_name);
			FullConfigNode.AddNode(Constants.weldPartNode);
			ConfigNode partconfig = FullConfigNode.GetNode(Constants.weldPartNode);
			// add name, module and author
			partconfig.AddValue(Constants.weldModuleNodeName, _name);
			partconfig.AddValue("module", _module);
			partconfig.AddValue("author", Constants.weldAuthor);

			//add model information
			foreach (ModelInfo model in _models)
			{
				ConfigNode node = new ConfigNode(Constants.weldModelNode);
				node.AddValue("model", model.url);
				foreach (string tex in model.textures)
				{
					node.AddValue("texture", tex);
				}
				if (!model.position.Equals(Vector3.zero))
				{
					node.AddValue("position", WeldingHelpers.writeVector(WeldingHelpers.RoundVector3(model.position,_precisionDigits)));
				}
				if (!model.scale.Equals(Vector3.zero))
				{
					node.AddValue("scale", WeldingHelpers.writeVector(WeldingHelpers.RoundVector3(model.scale, _precisionDigits)));
				}
				if (!model.rotation.Equals(Vector3.zero))
				{
					node.AddValue("rotation", WeldingHelpers.writeVector(WeldingHelpers.RoundVector3(model.rotation, _precisionDigits)));
				}
				if (!string.IsNullOrEmpty(model.parent))
				{
					node.AddValue("parent", model.parent);
				}
				partconfig.AddNode(node);
			}

			//add rescale factor
			partconfig.AddValue("rescaleFactor", WeldingHelpers.RoundFloat(_rescaleFactor, _precisionDigits));

			//add PhysicsSignificance
			partconfig.AddValue("PhysicsSignificance", _physicsSignificance);

			//add nodes stack
			if (_attachNodes.Count() > 2)
			{
				float topmostMark = float.MinValue;
				float lowestMark = float.MaxValue;
				AttachNode topmostNode = _attachNodes[0];
				AttachNode lowestNode = _attachNodes[1];
				foreach (AttachNode node in _attachNodes)
				{
					if (node.position.y > topmostMark)
					{
						topmostMark = node.position.y;
						topmostNode = node;
					}
					if (node.position.y < lowestMark)
					{
						lowestMark = node.position.y;
						lowestNode = node;
					}
				}
				//				_attachNodes.Add(_attachNodes[0]);
				//				_attachNodes.Insert(_attachNodes.Count-1, _attachNodes[0]);
				_attachNodes.Add(topmostNode);
				_attachNodes.Add(lowestNode);
				_attachNodes.Remove(topmostNode);
				_attachNodes.Remove(lowestNode);
			}
			foreach (AttachNode node in _attachNodes)
			{
				//Make sure the orintation is an int
				Vector3 orientation = Vector3.zero;
				orientation.x = node.orientation.x;// (int)Mathf.FloorToInt(node.orientation.x + 0.5f);
				orientation.y = node.orientation.y;// (int)Mathf.FloorToInt(node.orientation.y + 0.5f);
				orientation.z = node.orientation.z;// (int)Mathf.FloorToInt(node.orientation.z + 0.5f);
				if (orientation == Vector3.zero)
				{
					orientation = Vector3.up;
				}
				orientation.Normalize();

				//partconfig.AddValue(string.Format("node_stack_{0}", node.id), string.Format("{0}, {1}, {2}", ConfigNode.WriteVector(WeldingHelpers.RoundVector3(node.position, _precisionDigits)), ConfigNode.WriteVector(WeldingHelpers.RoundVector3(orientation, _precisionDigits)), node.size));
				partconfig.AddValue(string.Format("node_stack_{0}", node.id), string.Format("{0}, {1}, {2}", WeldingHelpers.writeVector(WeldingHelpers.RoundVector3(node.position, _precisionDigits)), WeldingHelpers.writeVector(WeldingHelpers.RoundVector3(orientation, _precisionDigits)), node.size));
			}
			//add surface attach node
			partconfig.AddValue("node_attach", string.Format("{0}, {1}, {2}", WeldingHelpers.writeVector(WeldingHelpers.RoundVector3(_srfAttachNode.originalPosition, _precisionDigits)), WeldingHelpers.writeVector(WeldingHelpers.RoundVector3(_srfAttachNode.originalOrientation, _precisionDigits)), _srfAttachNode.size));

			//merge fx
			ConfigNode.Merge(partconfig, _fxData);
			partconfig.name = Constants.weldPartNode; //Because it get removed during the merge!?
			//Add CrewCapacity
			partconfig.AddValue("CrewCapacity", _crewCapacity);
			// Add stackSymmetry
			if (_stackSymmetry > 0)
			{
				partconfig.AddValue("stackSymmetry", _stackSymmetry);
			}

			// Add Lifting Offsets
			if (_CoLOffset != Vector3.zero)
			{
				partconfig.AddValue("CoLOffset", WeldingHelpers.writeVector(WeldingHelpers.RoundVector3(_CoLOffset, _precisionDigits)));
			}
			if (_CoPOffset != Vector3.zero)
			{
				partconfig.AddValue("CoPOffset", WeldingHelpers.writeVector(WeldingHelpers.RoundVector3(_CoPOffset, _precisionDigits)));
			}


			//Add R&D (.22)
			partconfig.AddValue("TechRequired", _techRequire);
			partconfig.AddValue("entryCost", _entryCost);

			//add cost
			partconfig.AddValue("cost", _cost);

			//add category
			partconfig.AddValue("category", _category.ToString());
			partconfig.AddValue("subcategory", _subcat);

			//add title desc and manufacturer
			partconfig.AddValue("title", _title);
			partconfig.AddValue("manufacturer", Constants.weldManufacturer);
			partconfig.AddValue("description", _description);

			//add attachement rules
			partconfig.AddValue("attachRules", _attachrules.String());

			//Add the mass
			partconfig.AddValue("mass", Math.Round(_mass,9,MidpointRounding.ToEven));

			//Add the vesseltype if there is one
			if (vesselTypeList.Count > 0)
			{
				partconfig.AddValue("vesselType", _vesselType);
			}

			//add drag
			partconfig.AddValue("dragModelType", _dragModel);
			partconfig.AddValue("maximum_drag", WeldingHelpers.RoundFloat(_maximumDrag, _precisionDigits));
			partconfig.AddValue("minimum_drag", WeldingHelpers.RoundFloat(_minimumDrag,_precisionDigits));
			partconfig.AddValue("angularDrag", WeldingHelpers.RoundFloat(_angularDrag,_precisionDigits));

			//add crash and breaking data
			partconfig.AddValue("crashTolerance", WeldingHelpers.RoundFloat(_crashTolerance, _precisionDigits));
			partconfig.AddValue("breakingForce", WeldingHelpers.RoundFloat(_breakingForce, _precisionDigits));
			partconfig.AddValue("breakingTorque", WeldingHelpers.RoundFloat(_breakingTorque, _precisionDigits));
			partconfig.AddValue("maxTemp", WeldingHelpers.RoundFloat(_maxTemp, _precisionDigits));

			//add if crossfeed
			partconfig.AddValue("fuelCrossFeed", _fuelCrossFeed);

			// Add expolsionpotential
			if (_explosionPotential > 0)
			{
				partconfig.AddValue("explosionPotential", _explosionPotential);
			}

			// Add temperature Values
			if (_thermalMassModifier > 0)
			{
				partconfig.AddValue("thermalMassModifier", _thermalMassModifier);
			}
			if (_heatConductivity > 0)
			{
				partconfig.AddValue("heatConductivity", _heatConductivity);
			}
			if (_emissiveConstant > 0)
			{
				partconfig.AddValue("emissiveConstant", _emissiveConstant);
			}
			if (_radiatorHeadroom > 0)
			{
				partconfig.AddValue("radiatorHeadroom", _radiatorHeadroom);
			}

			// Add bulkheadProfile
			partconfig.AddValue("bulkheadProfiles", _bulkheadProfiles);

			//add INTERNAL

			if (_internalStorageNode.CountNodes > 0)
			{
				ConfigNode internalNode = new ConfigNode(Constants.weldInternalNode);
				if (_internalStorageNode.CountNodes > 1)
				{
					internalNode.AddValue(Constants.weldModuleNodeName, _name + "Internal");
					CreateFullInternalNode();
				}
				else
				{
					internalNode.AddValue(Constants.weldModuleNodeName, _internalStorageNode.GetNodes()[0].GetValue(_internalStorageNode.GetNodes()[0].GetValues()[0]));
				}
				partconfig.AddNode(internalNode);
			}

			//add RESOURCE
			foreach (ConfigNode res in _resourceslist)
			{
				partconfig.AddNode(res);
			}

			//add MODULE
			foreach (ConfigNode mod in _moduleList)
			{
				partconfig.AddNode(mod);
			}
		}


		public void CreateFullInternalNode()
		{
			FullInternalNode = new ConfigNode(_name + "Internal");
			FullInternalNode.AddNode(Constants.weldInternalNode);
			ConfigNode internalConfig = FullInternalNode.GetNode(Constants.weldInternalNode);
			internalConfig.AddValue(Constants.weldModuleNodeName, _name + "Internal");

			List<ConfigNode> tempList = new List<ConfigNode>();
			//add MODEL
			Debugger.AdvDebug("sorting Internal MODELs",_advancedDebug);
			if (_internalStorageNode.HasNode(Constants.weldModelNode))
			{
				foreach (ConfigNode c in _internalStorageNode.GetNodes(Constants.weldModelNode))
				{
					tempList.Add(c);
				}
			}
			foreach (ConfigNode n in tempList)
			{
				internalConfig.AddNode(n);
			}
			Debugger.AdvDebug("sorting Internal MODELs complete", _advancedDebug);
			tempList.Clear();

			//add MODULE
			Debugger.AdvDebug("sorting Internal MODULEs", _advancedDebug);
			if (_internalStorageNode.HasNode(Constants.weldModuleNode))
			{
				foreach (ConfigNode c in _internalStorageNode.GetNodes(Constants.weldModuleNode))
				{
					tempList.Add(c);
				}
			}
			foreach (ConfigNode n in tempList)
			{
				internalConfig.AddNode(n);
			}
			Debugger.AdvDebug("sorting Internal MODULEs complete", _advancedDebug);
			tempList.Clear();

			//add PROP
			Debugger.AdvDebug("sorting Internal PROPs", _advancedDebug);
			if (_internalStorageNode.HasNode(Constants.weldPropNode))
			{
				foreach (ConfigNode c in _internalStorageNode.GetNodes(Constants.weldPropNode))
				{
					tempList.Add(c);
				}
			}
			foreach (ConfigNode n in tempList)
			{
				internalConfig.AddNode(n);
			}
			Debugger.AdvDebug("sorting Internal PROPs complete", _advancedDebug);
			tempList.Clear();
		}


		private void loadPartHashMap()
		{
			Part[] children = UbioZurWeldingLtd.instance.selectedPartBranch.FindChildParts<Part>(true);
			partsHashMap = new int[children.Length + 1];

			for (int i = 0; i < children.Length; i++)
			{
				partsHashMap[i] = children[i].GetHashCode();
			}
			partsHashMap[children.Length] = UbioZurWeldingLtd.instance.selectedPartBranch.GetHashCode();
		}


		private bool isChildPart(Part parentPart, Part partToSearch)
		{
			return partsHashMap.Contains<int>(partToSearch.GetHashCode());
		}


		private void getAttachmentType(Part part)
		{
			Debugger.AdvDebug(part.name + " Attache mode " + part.attachMode+" Attach method "+ part.attachMethod,_advancedDebug);
		}


	} //class Welder
}


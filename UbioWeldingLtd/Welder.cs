using UnityEngine;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
    
    class Welder
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
        private int _entryCost = Constants.weldDefaultEntryCost;

        private float _mass = 0.0f;
        private float _fullmass = 0.0f;
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
        private List<ConfigNode> _modulelist = new List<ConfigNode>();
        private ConfigNode _fxData = new ConfigNode();

        private Vector3 _coMOffset = Vector3.zero;
        private Vector3 _com = Vector3.zero;

        public ConfigNode FullConfigNode = new ConfigNode(Constants.weldPartNode);
        public static bool IncludeAllNodes = false;
        //Do not process mass of massless parts (with PhysicsSignificance = 1)
        public static bool dontProcessMasslessParts = false;

        /*
         * 
         */
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
        public float Mass { get { return _mass; } }
        public float WetMass { get { return _fullmass; } }
        public bool FuelCrossFeed { get { return _fuelCrossFeed; } set { _fuelCrossFeed = value; } }
        public float MinDrag { get { return _minimumDrag; } }
        public float MaxDrag { get { return _maximumDrag; } }
        public float CrashTolerance { get { return _crashTolerance; } }
        public float BreakingForce { get { return _breakingForce; } }
        public float BreakingTorque { get { return _breakingTorque; } }
        public float MaxTemp { get { return _maxTemp; } }
        public float NbParts { get { return _partNumber; } }
        public string[] Modules
        {
            get
            {
                string[] moduleslist = new string[_modulelist.Count];
                int index = 0;
                foreach (ConfigNode cfgnode in _modulelist)
                {
                    moduleslist[index] = cfgnode.GetValue("name");
                    ++index;
                }
                return moduleslist;
            }
        }
        public string[] Resources
        {
            get
            {
                string[] resourceslist = new string[_resourceslist.Count * 2];
                int index = 0;
                foreach (ConfigNode cfgnode in _resourceslist)
                {
                    resourceslist[index++] = cfgnode.GetValue("name");
                    resourceslist[index++] = string.Format("{0} / {1}", cfgnode.GetValue("amount"), cfgnode.GetValue("maxAmount"));
                }
                return resourceslist;
            }
        }
        public PartCategories Category
        { 
            get { return _category; }
            set 
            { 
                _category = value;
                if (HighLogic.fetch.currentGame.Mode == Game.Modes.CAREER)
                {
                    switch (value)
                    {
                        case PartCategories.Pods:
                            _techRequire = Constants.rdNodeByPass;
                            break;
                        case PartCategories.Propulsion:
                            _techRequire = Constants.rdNodeByPass;
                            break;
                        case PartCategories.Control:
                            _techRequire = Constants.rdNodeByPass;
                            break;
                        case PartCategories.Structural:
                            _techRequire = Constants.rdNodeByPass;
                            break;
                        case PartCategories.Aero:
                            _techRequire = Constants.rdNodeByPass;
                            break;
                        case PartCategories.Utility:
                            _techRequire = Constants.rdNodeByPass;
                            break;
                        case PartCategories.Science:
                            _techRequire = Constants.rdNodeByPass;
                            break;
                    }
                }
                else
                {
                    _techRequire = Constants.rdNodeSandboxWeld;
                }
            }
        }
        
        
        /*
         * Constructor
         */
        public Welder() { }

        /*
         * Remove all the (Clone) at the end of the names
         */
        private void removecClone(ref string name)
        {
            const string clone = "(Clone)";
            while (new Regex(clone).IsMatch(name))
            {
                name = name.Substring(0, name.Length - clone.Length);
            }
        } //private void removecClone(ref string name)

        /*
         * Set relative position
         */
        private void setRelativePosition(Part part, ref Vector3 position)
        {
            position += part.transform.position - part.localRoot.transform.position;
        } //private void setRelativePosition(Part part, ref Vector3 position)

        /*
         * Set relative rotation
         */
        private void setRelativeRotation(Part part, ref Vector3 rotation)
        {
            rotation += part.transform.eulerAngles - part.localRoot.transform.eulerAngles;

            if (360.0f <= rotation.x) rotation.x -= 360.0f;
            else if (0 > rotation.x) rotation.x += 360.0f;

            if (360.0f <= rotation.y) rotation.y -= 360.0f;
            else if (0 > rotation.y) rotation.y += 360.0f;

            if (360.0f <= rotation.y) rotation.y -= 360.0f;
            else if (0 > rotation.y) rotation.y += 360.0f;
        } //private void setRelativeRotation(Part part, ref Vector3 rotation)
        
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
        }

        /*
         * Merge Curve Vector 2
         */
        private Vector2[] MergeAtmCurve(string[] set1, string[] set2)
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

        /*
         * Merge Curve Vector 4
         */
        private Vector4[] MergeVelCurve(string[] set1, string[] set2)
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
                    curvevect[j].y = ( curvevect[j].y + vect.y ) * 0.5f;
                }
            }
            return curvevect;
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
            string filename = string.Format("{0}\\{1}.mu", cfgdir.parent.parent.path, mesh);
            string url = string.Format("{0}/{1}", cfgdir.parent.parent.url, mesh);

            //in case the mesh name does not exist (.22 bug)
            if (!File.Exists(filename))
            {
                Debug.LogWarning(string.Format("{0}{1}.!{2} {3}", Constants.logWarning, Constants.logPrefix, Constants.logWarnNoMesh, filename));
                string[] files = Directory.GetFiles(cfgdir.parent.parent.path, "*.mu");
                if (files.Length != 0)
                {
                    files[0] = files[0].Remove(0, cfgdir.parent.parent.path.Length);
#if (DEBUG)
                    Debug.LogWarning(string.Format("{0}{1}.New mesh name: {2}", Constants.logWarning, Constants.logPrefix, files[0]));
#endif
                    char[] sep = { '\\','.' };
                    string[] words = files[0].Split(sep);
                    url = url.Replace(string.Format(@"{0}", mesh), words[1]);
                }
                else
                {
#if (DEBUG)
                    Debug.LogWarning(string.Format("{0}{1}.No mesh found, using default", Constants.logWarning, Constants.logPrefix));
#endif
                }
            }

            return url;
        }

        /*
         * Weld a new part
         */
        public WeldingReturn weldThisPart(Part newpart)
        {
            _coMOffset = Vector3.zero;
            WeldingReturn ret = WeldingReturn.Success;
            string partname = (string)newpart.partInfo.partPrefab.name.Clone();
            removecClone(ref partname);
            //KSP Squad specific hardcode :@
//            if (string.Equals(partname, "fuelTank.long") || string.Equals(partname, "science.module"))
//            {
//                partname = partname.Replace('.', '_'); //Fixed the name change for the fuelTank_long (stored as fuelTank.long), science_module (stored as science.module)
//            }
            Debug.Log(string.Format("{0}{1}{2}",Constants.logPrefix,Constants.logWeldingPart,partname));
#if (DEBUG)
            Debug.Log(string.Format("{0}..part rescaleFactor {1:F}", Constants.logPrefix, newpart.rescaleFactor));
            Debug.Log(string.Format("{0}..part scaleFactor {1:F}", Constants.logPrefix, newpart.scaleFactor));
#endif

            //--- Find all the config file with the name
            List<UrlDir.UrlConfig> matchingPartConfigs = new List<UrlDir.UrlConfig>();
            foreach (UrlDir.UrlConfig config in GameDatabase.Instance.GetConfigs(Constants.weldPartNode))
            {
                string newconfigname = config.name.Replace('_', '.');
#if (DEBUG)
//Girka2K - too many spam in LOG from here
//                Debug.Log(string.Format("{0}.config name {1}", Constants.logPrefix, newconfigname));
#endif
                if (System.String.Equals(partname, newconfigname, System.StringComparison.Ordinal))
                {
                    matchingPartConfigs.Add(config);
                }
            }
#if (DEBUG)
            Debug.Log(string.Format("{0}.Found {1} config files", Constants.logPrefix, matchingPartConfigs.Count));
#endif
            if (0 >= matchingPartConfigs.Count)
            {
                //Missing Config File: Error
                Debug.LogError(string.Format("{0}{1}.{2} {3}",Constants.logError, Constants.logPrefix, Constants.msgCfgMissing, partname));
                return WeldingReturn.MissingCfg;
            }
            else // 0 < matchingPartConfigs.Count
            {
                //Process Config Files
                foreach (UrlDir.UrlConfig cfg in matchingPartConfigs)
                {
                    //MODEL
                    if ( !cfg.config.HasNode(Constants.weldModelNode) )
                    {
                        //Missing Model node
#if (DEBUG)
                        Debug.Log(string.Format("{0}.. Config {1} has no {2} node",Constants.logPrefix,cfg.name,Constants.weldModelNode));
#endif
                        ModelInfo info = new ModelInfo();
                        info.url = GetMeshurl(cfg);
                        Debug.Log(string.Format("{0}..{1}{2}", Constants.logPrefix,Constants.logModelUrl,info.url));

                        Vector3 position = Vector3.zero;
                        setRelativePosition(newpart, ref position);
                        info.position = position;

                        Vector3 rotation = newpart.localRoot.transform.eulerAngles;
                        setRelativeRotation(newpart, ref rotation);
                        info.rotation = rotation;

                        info.scale = new Vector3(newpart.rescaleFactor, newpart.rescaleFactor, newpart.rescaleFactor);

#if (DEBUG)
                        Debug.Log(string.Format("{0}..position {1:F3}", Constants.logPrefix, info.position));
                        Debug.Log(string.Format("{0}..rotation {1:F3}", Constants.logPrefix, info.rotation));
                        Debug.Log(string.Format("{0}..scale {1:F3}", Constants.logPrefix, info.scale));
#endif
                        _models.Add(info);
                        _coMOffset += info.position;
                    }
                    else //cfg.config.HasNode(Constants.weldModelNode)
                    {
                        ConfigNode[] modelnodes = cfg.config.GetNodes(Constants.weldModelNode);
#if (DEBUG)
                        Debug.Log(string.Format("{0}..Config {1} has {2} {3} node", Constants.logPrefix, cfg.name, modelnodes.Length, Constants.weldModelNode));
#endif
                        Vector3 _coMOffsetSum = Vector3.zero;
                        foreach (ConfigNode node in modelnodes)
                        {
                            ModelInfo info = new ModelInfo();

                            if (node.HasValue("model"))
                            {
                                info.url = node.GetValue("model");
                            }
                            else
                            {
                                info.url = GetMeshurl(cfg);
                            }
                            Debug.Log(string.Format("{0}..{1}{2}", Constants.logPrefix, Constants.logModelUrl, info.url));

                            Vector3 position = (node.HasValue("position")) ? (ConfigNode.ParseVector3(node.GetValue("position")) * newpart.rescaleFactor) : Vector3.zero;
                            setRelativePosition(newpart, ref position);

                            info.position = position;

                            Vector3 rotation = (node.HasValue("rotation")) ? ConfigNode.ParseVector3(node.GetValue("rotation")) : Vector3.zero;
                            setRelativeRotation(newpart, ref rotation);
                            info.rotation = rotation;
                            info.scale = (node.HasValue("scale")) ? (ConfigNode.ParseVector3(node.GetValue("scale")) * newpart.rescaleFactor * (newpart.rescaleFactor / _rescaleFactor)) : new Vector3((newpart.rescaleFactor / _rescaleFactor), (newpart.rescaleFactor / _rescaleFactor), (newpart.rescaleFactor / _rescaleFactor));
#if (DEBUG)
                            Debug.Log(string.Format("{0}..position {1:F3}", Constants.logPrefix, info.position));
                            Debug.Log(string.Format("{0}..rotation {1:F3}", Constants.logPrefix, info.rotation));
                            Debug.Log(string.Format("{0}..scale {1:F3}", Constants.logPrefix, info.scale));
#endif
                            if (node.HasValue("texture"))
                            {
                                foreach (string tex in node.GetValues("texture"))
                                {
                                    info.textures.Add(tex);
#if (DEBUG)
                                    Debug.Log(string.Format("{0}..texture {1}", Constants.logPrefix, tex));
#endif
                                }
                            }
                            if (node.HasValue("parent"))
                            {
                                info.parent = node.GetValue("parent");
                            }
                            _models.Add(info);
                            _coMOffsetSum += info.position;
                        } //foreach (ConfigNode node in modelnodes)
                        _coMOffset = _coMOffsetSum / modelnodes.Length;
                    } // else of if ( !cfg.config.HasNode(Constants.weldModelNode) )
                    
                    //RESSOURCE
                    ConfigNode[] ressources = cfg.config.GetNodes(Constants.weldResNode);
#if (DEBUG)
                    Debug.Log(string.Format("{0}..Config {1} has {2} {3} node", Constants.logPrefix, cfg.name, ressources.Length, Constants.weldResNode));
#endif
                    foreach (ConfigNode orires in ressources)
                    {
                        ConfigNode res = orires.CreateCopy();
                        string resname = res.GetValue("name");
                        bool exist = false;
                        foreach (ConfigNode rescfg in _resourceslist)
                        {
                            if (string.Equals(resname, rescfg.GetValue("name")))
                            {
                                //add the ressource
                                float amount = float.Parse(res.GetValue("amount")) + float.Parse(rescfg.GetValue("amount"));
                                float max = float.Parse(res.GetValue("maxAmount")) + float.Parse(rescfg.GetValue("maxAmount"));
                                rescfg.SetValue("amount", amount.ToString());
                                rescfg.SetValue("maxAmount", max.ToString());
                                exist = true;
                                Debug.Log(string.Format("{0}..{1}{2} {3}/{4}", Constants.logPrefix, Constants.logResMerge, resname, amount, max));
                                break;
                            }
                        }
                        if (!exist)
                        {
                            _resourceslist.Add(res);
                            float amount = float.Parse(res.GetValue("amount"));
                            float max = float.Parse(res.GetValue("maxAmount"));
                            Debug.Log(string.Format("{0}..{1}{2} {3}/{4}", Constants.logPrefix, Constants.logResAdd, resname, amount, max));
                        }
                    } //foreach (ConfigNode res in ressources)

                    //MODULE
                    ConfigNode[] modules = cfg.config.GetNodes(Constants.weldModuleNode);
#if (DEBUG)
                    Debug.Log(string.Format("{0}..Config {1} has {2} {3} node", Constants.logPrefix, cfg.name, modules.Length, Constants.weldModuleNode));
#endif
                    foreach (ConfigNode orimod in modules)
                    {
                        ConfigNode mod = orimod.CreateCopy();
                        string modname = mod.GetValue("name");
                        bool exist = false;
                        foreach (ConfigNode modcfg in _modulelist)
                        {
                            if (string.Equals(modname, modcfg.GetValue("name")))
                            {
                                switch (modname)
                                {
                                    case Constants.modStockSas:             // don't add SAS modules together.
                                    case Constants.modStockGear:            //Don't add (.21)
                                        Debug.Log(string.Format("{0}..{1}{2}", Constants.logPrefix, Constants.logModIgnore, modname));
                                        exist = true;
                                        break;
                                    case Constants.modStockReacWheel:       //Add reaction wheel force
                                        float pitch = float.Parse(modcfg.GetValue("PitchTorque")) + float.Parse(mod.GetValue("PitchTorque"));
                                        float yaw = float.Parse(modcfg.GetValue("YawTorque")) + float.Parse(mod.GetValue("YawTorque"));
                                        float roll = float.Parse(modcfg.GetValue("RollTorque")) + float.Parse(mod.GetValue("RollTorque"));
                                        float wheelrate = float.Parse(modcfg.GetNode(Constants.weldResNode).GetValue("rate")) + float.Parse(mod.GetNode(Constants.weldResNode).GetValue("rate"));
                                        modcfg.SetValue("PitchTorque", pitch.ToString());
                                        modcfg.SetValue("YawTorque", yaw.ToString());
                                        modcfg.SetValue("RollTorque", roll.ToString());
                                        modcfg.GetNode(Constants.weldResNode).SetValue("rate", wheelrate.ToString());
                                        Debug.Log(string.Format("{0}..{1}{2}", Constants.logPrefix, Constants.logModMerge, modname));
                                        exist = true;
                                        break;
                                    case Constants.modStockCommand:        // Add Crew and Electricity ressources //TODO: Manage all used ressources
                                        int crew = int.Parse(mod.GetValue("minimumCrew")) + int.Parse(modcfg.GetValue("minimumCrew"));
                                        modcfg.SetValue("minimumCrew", crew.ToString());
                                        if (mod.HasNode(Constants.weldResNode))
                                        {
                                            if (modcfg.HasNode(Constants.weldResNode))
                                            {
                                                float comrate = float.Parse(mod.GetNode(Constants.weldResNode).GetValue("rate")) + float.Parse(modcfg.GetNode(Constants.weldResNode).GetValue("rate"));
                                                modcfg.GetNode(Constants.weldResNode).SetValue("rate", comrate.ToString());
                                            }
                                            else
                                            {
                                                modcfg.AddNode(mod.GetNode(Constants.weldResNode));
                                            }
                                        }
                                        Debug.Log(string.Format("{0}..{1}{2}", Constants.logPrefix, Constants.logModMerge, modname));
                                        exist = true;
                                        break;
                                    case Constants.modStockGen:            // Add Generator Values //TODO: Manage output ressource name.
                                        bool active = bool.Parse(mod.GetValue("isAlwaysActive")) && bool.Parse(modcfg.GetValue("isAlwaysActive"));
                                        float genrate = float.Parse(mod.GetNode(Constants.weldOutResNode).GetValue("rate")) + float.Parse(modcfg.GetNode(Constants.weldOutResNode).GetValue("rate"));
                                        modcfg.SetValue("isAlwaysActive", active.ToString());
                                        modcfg.GetNode(Constants.weldOutResNode).SetValue("rate", genrate.ToString());
                                        Debug.Log(string.Format("{0}..{1}{2}", Constants.logPrefix, Constants.logModMerge, modname));
                                        exist = true;
                                        break;
                                    case Constants.modStockAltern:         //add the alternator value
                                        float altrate = float.Parse(mod.GetNode(Constants.weldResNode).GetValue("rate")) + float.Parse(modcfg.GetNode(Constants.weldResNode).GetValue("rate"));
                                        modcfg.GetNode(Constants.weldResNode).SetValue("rate", altrate.ToString());
                                        Debug.Log(string.Format("{0}..{1}{2}", Constants.logPrefix, Constants.logModMerge, modname));
                                        exist = true;
                                        break;
                                    case Constants.modStockGimbal:      //average the gimbal range TODO: test the gimbal
                                        int gimbal = (int.Parse(mod.GetValue("gimbalRange")) + int.Parse(modcfg.GetValue("gimbalRange"))) / 2;
                                        modcfg.SetValue("gimbalRange", gimbal.ToString());
                                        Debug.Log(string.Format("{0}..{1}{2}", Constants.logPrefix, Constants.logModMerge, modname));
                                        exist = true;
                                        break;
                                    case Constants.modStockSensor:     // Allow one sensor module per different sensor
                                        exist = string.Equals(mod.GetValue("sensorType"), modcfg.GetValue("sensorType"));
                                        Debug.Log(string.Format("{0}..{1}{2}", Constants.logPrefix, (exist) ? Constants.logModIgnore : Constants.logModMerge, modname));
                                        break;
                                    case Constants.modStockEngine:        // Average/add value and warning
                                        bool exhaustDamage = bool.Parse(mod.GetValue("exhaustDamage")) || bool.Parse(modcfg.GetValue("exhaustDamage"));
                                        float ignitionThreshold = (float.Parse(mod.GetValue("ignitionThreshold")) + float.Parse(modcfg.GetValue("ignitionThreshold")))*0.5f;
                                        float minThrust = float.Parse(mod.GetValue("minThrust")) + float.Parse(modcfg.GetValue("minThrust"));
                                        float maxThrust = float.Parse(mod.GetValue("maxThrust")) + float.Parse(modcfg.GetValue("maxThrust"));
                                        int heatProduction = (int.Parse(mod.GetValue("heatProduction")) + int.Parse(modcfg.GetValue("heatProduction"))) / 2;
                                        modcfg.SetValue("exhaustDamage", exhaustDamage.ToString());
                                        modcfg.SetValue("ignitionThreshold", ignitionThreshold.ToString());
                                        modcfg.SetValue("minThrust", minThrust.ToString());
                                        modcfg.SetValue("maxThrust", maxThrust.ToString());
                                        modcfg.SetValue("heatProduction", heatProduction.ToString());
                                        //fx offset
                                        if (mod.HasValue("fxOffset"))
                                        {
                                            Vector3 fxOffset = ConfigNode.ParseVector3(mod.GetValue("fxOffset"));
                                            //setRelativePosition(newpart, ref fxOffset);
                                            //Vector3 cfgFxOffset = ConfigNode.ParseVector3(modcfg.GetValue("fxOffset")) + fxOffset;
                                            mod.SetValue("fxOffset", ConfigNode.WriteVector(fxOffset));
                                        }
                                        //Propellant nodes
                                        ConfigNode[] Propellant = mod.GetNodes(Constants.weldEngineProp);
                                        foreach (ConfigNode prop in Propellant)
                                        {
                                            //look if one exist
                                            ConfigNode[] cfgPropellant = modcfg.GetNodes(Constants.weldEngineProp);
                                            bool propexist = false;
                                            foreach (ConfigNode cfgprop in cfgPropellant)
                                            {
                                                if (string.Equals(cfgprop.GetValue("name"), prop.GetValue("name")))
                                                {
                                                    float ratio = float.Parse(prop.GetValue("ratio")) + float.Parse(cfgprop.GetValue("ratio"));
                                                    cfgprop.SetValue("ratio", ratio.ToString());
                                                    propexist = true;
                                                    break;
                                                }
                                            }
                                            if (!propexist)
                                            {
                                                modcfg.SetNode(Constants.weldEngineProp, prop);
                                            }
                                        }
                                        if (mod.HasNode(Constants.weldEngineAtmCurve))
                                        {
                                            if (modcfg.HasNode(Constants.weldEngineAtmCurve))
                                            {
                                                //merge
                                                string[] curve = mod.GetNode(Constants.weldEngineAtmCurve).GetValues("key");
                                                string[] cfgcurve = modcfg.GetNode(Constants.weldEngineAtmCurve).GetValues("key");
                                                Vector2[] cfgcurvevect = MergeAtmCurve(curve, cfgcurve);
                                                modcfg.GetNode(Constants.weldEngineAtmCurve).RemoveValues("key");
                                                foreach (Vector2 vec in cfgcurvevect)
                                                {
                                                    modcfg.GetNode(Constants.weldEngineAtmCurve).AddValue("key", ConfigNode.WriteVector(vec));
                                                }
                                            }
                                            else
                                            {
                                                modcfg.AddNode(mod.GetNode(Constants.weldEngineAtmCurve));
                                            }
                                        }
                                        if (mod.HasNode(Constants.weldEngineVelCurve))
                                        {
                                            if (modcfg.HasNode(Constants.weldEngineVelCurve))
                                            {
                                                //merge
                                                string[] curve = mod.GetNode(Constants.weldEngineVelCurve).GetValues("key");
                                                string[] cfgcurve = modcfg.GetNode(Constants.weldEngineVelCurve).GetValues("key");
                                                Vector4[] cfgcurvevect = MergeVelCurve(curve, cfgcurve);
                                                modcfg.GetNode(Constants.weldEngineVelCurve).RemoveValues("key");
                                                foreach (Vector4 vec in cfgcurvevect)
                                                {
                                                    modcfg.GetNode(Constants.weldEngineVelCurve).AddValue("key", ConfigNode.WriteVector(vec));
                                                }
                                            }
                                            else
                                            {
                                                modcfg.AddNode(mod.GetNode(Constants.weldEngineVelCurve));
                                            }
                                        }
                                        Debug.Log(string.Format("{0}..{1}{2} !{3}", Constants.logPrefix, Constants.logModMerge, modname, Constants.msgWarnModEngine));
                                        exist = true;
                                        break;
                                    case Constants.modStockAnimHeat:
                                        exist = string.Equals(modcfg.GetValue("ThermalAnim"), mod.GetValue("ThermalAnim"));
                                        Debug.Log(string.Format("{0}..{1}{2}", Constants.logPrefix, (exist) ? Constants.logModIgnore : Constants.logModMerge, modname));
                                        break;
                                    case Constants.modStockAnimGen:        // Warning for Multiple Animate Generic
                                        exist = string.Equals(modcfg.GetValue("animationName"), mod.GetValue("animationName"));
                                        if (exist)
                                        {
                                            Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModAnimGen));
                                            ret = WeldingReturn.MultipleAnimGen;
                                        }
                                        break;
                                    case Constants.modStockInternal:   // Warning for multiple interal and ignore
                                        Debug.LogWarning(string.Format("{0}{1}..{2}{3} !{4}", Constants.logWarning, Constants.logPrefix, Constants.logModIgnore, modname, Constants.msgWarnModInternal));
                                        ret = WeldingReturn.MultipleInternal;
                                        exist = true;
                                        break;
                                    case Constants.modStockSeat:       // Warning for Multiple seats //TODO: Test
                                        Debug.LogWarning(string.Format("{0}{1}..{2}{3} !{4}", Constants.logWarning, Constants.logPrefix, Constants.logModIgnore, modname, Constants.msgWarnModSeat));
                                        ret = WeldingReturn.MultipleSeats;
                                        exist = true;
                                        break;
                                    case Constants.modStockSolarPan:       // Warning for Multiple Deployable Solar Panel //TODO: Test
                                        Debug.LogWarning(string.Format("{0}{1}..{2}{3} !{4}", Constants.logWarning, Constants.logPrefix, Constants.logModIgnore, modname, Constants.msgWarnModSolPan));
                                        ret = WeldingReturn.MultipleSolarPan;
                                        exist = true;
                                        break;
                                    case Constants.modStockJettison:       // Warning for Multiple Jetison //Only one is working fairing is working.
                                        Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModJetttison));
                                        ret = WeldingReturn.MultipleJettison;
                                        exist = false;
                                        break;
                                    case Constants.modStockFxAnimThro:       // Warning for Multiple FX animate. // Only the first one is working, the other are ignore
                                        Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModFxAnimTh));
                                        ret = WeldingReturn.MultipleFXAnimateThrottle;
                                        exist = false;
                                        break;
                                    case Constants.modStockIntake:        // Warning for Multiple Intake //TODO: Test
                                        Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModIntake));
                                        ret = WeldingReturn.MultipleIntake;
                                        exist = false;
                                        break;
                                    case Constants.modStockDecouple:
                                    case Constants.modStockAnchdec:        //Warning for Multiple Decoupler, change the node //TODO: Test
                                        Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModDecouple));
                                        ret = WeldingReturn.MultipleDecouple;
                                        exist = false;
                                        break;
                                    case Constants.modStockDocking:        //Warning for Multiple Dockingport
                                        Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModDocking));
                                        ret = WeldingReturn.MultipleDocking;
                                        exist = false;
                                        break;
                                    case Constants.modStockRCS:        //Warning for Multiple RCS
                                        Debug.LogWarning(string.Format("{0}{1}..{2}{3} !{4}", Constants.logWarning, Constants.logPrefix, Constants.logModIgnore, modname, Constants.msgWarnModRcs));
                                        //ret = WeldingReturn.MultipleRcs;
                                        exist = true;
                                        break;
                                    case Constants.modStockParachutes:        //Warning for Multiple Parachutes //TODO: Test
                                        Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModParachute));
                                        ret = WeldingReturn.MultipleParachutes;
                                        exist = false;
                                        break;
                                    case Constants.modStockLight:        //Warning for Multiple Light //TODO: Test
                                        Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModLight));
                                        ret = WeldingReturn.MultipleLight;
                                        exist = false;
                                        break;
                                    case Constants.modStockRetLadder:        //Warning for Multiple Retractable ladder //TODO: Test
                                        Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModRetLadder));
                                        ret = WeldingReturn.MultipleRetLadder;
                                        exist = false;
                                        break;
                                    case Constants.modStockWheel:        //Warning for Multiple Wheels //TODO: Test
                                        Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModWheel));
                                        ret = WeldingReturn.MultipleWheel;
                                        exist = false;
                                        break;
                                    case Constants.modStockFxLookAt:        //Warning for Multiple FxLookAt Constraint (wome with wheels) //TODO: Test
                                        Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModFxLookAt));
                                        ret = WeldingReturn.MultipleFxLookAt;
                                        exist = false;
                                        break;
                                    case Constants.modStockFxPos:        //Warning for Multiple Constraint Position (wome with wheels) //TODO: Test
                                        Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModFxPos));
                                        ret = WeldingReturn.MultipleFxPos;
                                        exist = false;
                                        break;
                                    case Constants.modStockLaunchClamp:        //Warning for Multiple Launching Clamp (I don't even why would it be needed
                                        Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModLaunClamp));
                                        ret = WeldingReturn.MultipleLaunchClamp;
                                        exist = false;
                                        break;
                                    case Constants.modStockScienceExp:        // Warning for Multiple Science Experiments (.22)
                                        exist = string.Equals(modcfg.GetValue("experimentID"), mod.GetValue("experimentID"));
                                        if (exist)
                                        {
                                            Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModScieExp));
                                            ret = WeldingReturn.MultipleScienceExp;
                                        }
                                        break;
                                    case Constants.modstockTransData:        // Merge transmition data (.22)
                                        float packetInterval = (float.Parse(mod.GetValue("packetInterval")) + float.Parse(modcfg.GetValue("packetInterval"))) * 0.5f;
                                        float packetSize = (float.Parse(mod.GetValue("packetSize")) + float.Parse(modcfg.GetValue("packetSize")));
                                        float packetResourceCost = (float.Parse(mod.GetValue("packetResourceCost")) + float.Parse(modcfg.GetValue("packetResourceCost")));
                                        //TODO: requiredResource / DeployFxModules 

                                        modcfg.SetValue("packetInterval", packetInterval.ToString());
                                        modcfg.SetValue("packetSize", packetSize.ToString());
                                        modcfg.SetValue("packetResourceCost", packetResourceCost.ToString());

                                        Debug.Log(string.Format("{0}..{1}{2}", Constants.logPrefix, Constants.logModMerge, modname));
                                        exist = true;
                                        break;
                                    case Constants.modStockLandingLegs:        // Waring Multiple same landing legs
                                        exist = string.Equals(modcfg.GetValue("animationName"), mod.GetValue("animationName"));
                                        if (exist)
                                        {
                                            Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModLandLegs));
                                            ret = WeldingReturn.MultipleLandingLegs;
                                        }
                                        break;
                                    case Constants.modStockScienceCont:        // Merge Science Container (.22)
                                        bool evaOnlyStorage = bool.Parse(mod.GetValue("evaOnlyStorage")) || bool.Parse(modcfg.GetValue("evaOnlyStorage"));
                                        float storageRange = (float.Parse(mod.GetValue("storageRange")) + float.Parse(modcfg.GetValue("storageRange")));
                                        //TODO: requiredResource / DeployFxModules 

                                        modcfg.SetValue("storageRange", storageRange.ToString());
                                        modcfg.SetValue("storageRange", storageRange.ToString());

                                        Debug.Log(string.Format("{0}..{1}{2}", Constants.logPrefix, Constants.logModMerge, modname));
                                        exist = true;
                                        break;
                                    default:                               // New update module or mods! not managed
                                        Debug.LogWarning(string.Format("{0}{1}.. !{2}", Constants.logWarning, Constants.logPrefix, Constants.msgWarnModUnknown));
                                        ret = WeldingReturn.ModuleUnknown;
                                        exist = false;
                                        break;
                                }
                            }
                        } //foreach (ConfigNode modcfg in _modulelist)
                        if (!exist)
                        {
                            switch (mod.GetValue("name"))
                            {
                                case Constants.modStockDecouple:
                                case Constants.modStockAnchdec:         //Decoupler: Change node name
                                    string decouplename = mod.GetValue("explosiveNodeID") + partname + _partNumber;
                                    mod.SetValue("explosiveNodeID", decouplename);
                                    break;
                                case Constants.modStockDocking:         //Docking port: Change node name if any TODO: FIX This
                                    if (mod.HasValue("referenceAttachNode"))
                                    {
                                        string dockname = mod.GetValue("referenceAttachNode") + partname + _partNumber;
                                        mod.SetValue("referenceAttachNode", dockname);
                                    }
                                    break;
                                case Constants.modStockJettison :       //Fairing/Jetisson, change node name
                                    string jetissonname = mod.GetValue("bottomNodeName") + partname + _partNumber;
                                    mod.SetValue("bottomNodeName", jetissonname);
                                    break;
                            }
                            _modulelist.Add(mod);
                            Debug.Log(string.Format("{0}..{1}{2}", Constants.logPrefix, Constants.logModAdd, modname));
                        } //if (!exist)
                    } //foreach (ConfigNode mod in modules)

                    //manage the fx group
                    foreach (FXGroup fx in newpart.fxGroups)
                    {
#if (DEBUG)
                        Debug.Log(string.Format("{0}..Config {1} has {2} FXEmitters and {3} Sound in {4} FxGroups", Constants.logPrefix, cfg.name, fx.fxEmitters.Count, (null != fx.sfx) ? "1" : "0", fx.name));
#endif

                        if (!fx.name.Contains("rcsGroup")) //RCS Fx are not store in the config file
                        {
                            foreach (ParticleEmitter gobj in fx.fxEmitters)
                            {
                                string fxname = gobj.name;
                                removecClone(ref fxname);
                                string fxvalue = cfg.config.GetValue(fxname);
                                string[] allvalue = Regex.Split(fxvalue, ", ");
                                Vector3 pos = new Vector3(float.Parse(allvalue[0]), float.Parse(allvalue[1]), float.Parse(allvalue[2]));
                                Vector3 ang = new Vector3(float.Parse(allvalue[3]), float.Parse(allvalue[4]), float.Parse(allvalue[5]));
                                setRelativePosition(newpart, ref pos);
                                fxvalue = string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}", pos.x, pos.y, pos.z, ang.x, ang.y, ang.z, allvalue[6]);
                                for (int i = 7; i < allvalue.Length; ++i)
                                {
                                    fxvalue = string.Format("{0}, {1}", fxvalue, allvalue[i]);
                                }
                                _fxData.AddValue(fxname, fxvalue);
                                Debug.Log(string.Format("{0}..{1}{2}", Constants.logPrefix, Constants.logFxAdd, fxname));
                            }
                            if (null != fx.sfx)
                            {
                                _fxData.AddValue(fx.sfx.name, fx.name);
                                Debug.Log(string.Format("{0}..{1}{2}", Constants.logPrefix, Constants.logFxAdd, fx.sfx.name));
                            }
                        }
                    } //foreach (FXGroup fx in newpart.fxGroups)
                } //foreach (UrlDir.UrlConfig cfg in matchingPartConfigs)
            } //else of if (0 >= matchingPartConfigs.Count)
            
            //ATTACHNODE
#if (DEBUG)
            Debug.Log(string.Format("{0}.Part {1} has {2} Stack attach node(s)", Constants.logPrefix, partname, newpart.attachNodes.Count));
#endif
            foreach (AttachNode partnode in newpart.attachNodes)
            {
                //only add node if not attached to another part (or if requested in the condig file)
                if (IncludeAllNodes || null == partnode.attachedPart)
                {
                    AttachNode node = partnode; //make sure we don't overwrite the part node
                    node.id += partname + _partNumber;
                    Matrix4x4 rot = Matrix4x4.TRS(Vector3.zero, newpart.transform.rotation, Vector3.one);
                    node.position = rot.MultiplyVector(node.position);
                    node.orientation = rot.MultiplyVector(node.orientation);
                    setRelativePosition(newpart, ref node.position);

                    _attachNodes.Add(node);
                    Debug.Log(string.Format("{0}.{1}{2}", Constants.logPrefix, Constants.logNodeAdd, node.id));
                }
            } //foreach (AttachNode node in newpart.attachNodes)
            
            //TODO: Tech tree stuff
            //newpart.partInfo.TechRequired

            //Cost
            _cost += (int)newpart.partInfo.cost;
            _crewCapacity += newpart.CrewCapacity;

            // srfAttachNode Rules
            _attachrules.allowDock = _attachrules.allowDock || newpart.attachRules.allowDock;
            _attachrules.allowRotate = _attachrules.allowRotate || newpart.attachRules.allowRotate;
            _attachrules.allowSrfAttach = _attachrules.allowSrfAttach || newpart.attachRules.allowSrfAttach;
            _attachrules.allowStack = _attachrules.allowStack || newpart.attachRules.allowStack;
            _attachrules.srfAttach = _attachrules.srfAttach || newpart.attachRules.srfAttach;
            _attachrules.stack = _attachrules.stack || newpart.attachRules.stack;

            //mass
            float oldmass = _fullmass;
            float partdrymass = 0.0f;
            // if part's PhysicsSignificance = 1, then this part is "massless" and its mass would be ignored in stock KSP
            if ((!dontProcessMasslessParts) || (newpart.PhysicsSignificance != 1))
            {
                partdrymass = newpart.mass;
            }

            float partwetmass = partdrymass + newpart.GetResourceMass();

            _mass += partdrymass;
            _fullmass += partwetmass;
            _com = ((_com * oldmass) + (_coMOffset * partwetmass)) / _fullmass;
#if (DEBUG)
            Debug.Log(string.Format("{0}.New Center of Mass: {1}", Constants.logPrefix, _com.ToString()));
#endif

            //Drag (Add)
            _minimumDrag = ( _minimumDrag + newpart.minimum_drag ) *0.5f;
            _maximumDrag = ( _maximumDrag+ newpart.maximum_drag ) * 0.5f;
            _angularDrag = ( _angularDrag + newpart.angularDrag ) * 0.5f;
            //TODO: modify type
            _dragModel = newpart.dragModelType;

            //average crash, breaking and temp
            _crashTolerance = (0 == _partNumber) ? newpart.crashTolerance : (_crashTolerance + newpart.crashTolerance) * 0.75f;
            _breakingForce = (0 == _partNumber) ? newpart.breakingForce : (_breakingForce + newpart.breakingForce) * 0.75f;
            _breakingTorque = (0 == _partNumber) ? newpart.breakingTorque : (_breakingTorque + newpart.breakingTorque) * 0.75f;
            _maxTemp = (0 == _partNumber) ? newpart.maxTemp : (_maxTemp + newpart.maxTemp) * 0.5f;

            //Phisics signifance
            if (newpart.PhysicsSignificance != 0 && _physicsSignificance != -1)
            {
                _physicsSignificance = newpart.PhysicsSignificance;
            }

            if (0 == _partNumber)
            {
                //TODO: Find where to find it in game. Would that be pre .15 stuff? http://forum.kerbalspaceprogram.com/threads/7529-Plugin-Posting-Rules-And-Official-Documentation?p=156430&viewfull=1#post156430
                _module = "Part";
                //
                _category = newpart.partInfo.category;
                //TODO: better surface node managment
                _srfAttachNode = newpart.srfAttachNode;
                //Fuel crossfeed: TODO: test different ways to managed it
                _fuelCrossFeed = newpart.fuelCrossFeed;
                //
                _physicsSignificance = newpart.PhysicsSignificance;
            }
            ++_partNumber;
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
            partconfig.AddValue("name", _name);
            partconfig.AddValue("module", _module);
            partconfig.AddValue("author", Constants.weldAuthor);

            //add model information
            foreach (ModelInfo model in _models)
            {
                ConfigNode node = new ConfigNode(Constants.weldModelNode);
                node.AddValue("model", model.url);
                node.AddValue("position", ConfigNode.WriteVector(model.position)); ;
                node.AddValue("scale", ConfigNode.WriteVector(model.scale));
                node.AddValue("rotation", ConfigNode.WriteVector(model.rotation));
                foreach (string tex in model.textures)
                {
                    node.AddValue("texture", tex);
                }
                if (!string.IsNullOrEmpty(model.parent))
                {
                    node.AddValue("parent", model.parent);
                }
                partconfig.AddNode(node);
            }

            //add rescale factor
            partconfig.AddValue("rescaleFactor", _rescaleFactor);

            //add PhysicsSignificance
            partconfig.AddValue("PhysicsSignificance", _physicsSignificance);

            //add nodes stack
            foreach (AttachNode node in _attachNodes)
            {
                //Make sure the orintation is an int
                Vector3 orientation = Vector3.zero;
                orientation.x = (int)Mathf.RoundToInt(node.orientation.x);
                orientation.y = (int)Mathf.RoundToInt(node.orientation.y);
                orientation.z = (int)Mathf.RoundToInt(node.orientation.z);
                partconfig.AddValue(string.Format("node_stack_{0}", node.id), string.Format("{0},{1},{2}", ConfigNode.WriteVector(node.position), ConfigNode.WriteVector(orientation), node.size));
            }
            //add surface attach node
            partconfig.AddValue("node_attach", string.Format("{0},{1},{2}", ConfigNode.WriteVector(_srfAttachNode.originalPosition), ConfigNode.WriteVector(_srfAttachNode.originalOrientation), _srfAttachNode.size));

            //merge fx
            ConfigNode.Merge(partconfig, _fxData);
            partconfig.name = Constants.weldPartNode; //Because it get removed during the merge!?
            //Add CrewCapacity
            partconfig.AddValue("CrewCapacity", _crewCapacity);

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
            partconfig.AddValue("mass", _mass);

            //add drag
            partconfig.AddValue("dragModelType", _dragModel);
            partconfig.AddValue("maximum_drag", _maximumDrag);
            partconfig.AddValue("minimum_drag", _minimumDrag);
            partconfig.AddValue("angularDrag", _angularDrag);

            //add crash and breaking data
            partconfig.AddValue("crashTolerance", _crashTolerance);
            partconfig.AddValue("breakingForce", _breakingForce);
            partconfig.AddValue("breakingTorque", _breakingTorque);
            partconfig.AddValue("maxTemp", _maxTemp);

            //add if crossfeed
            partconfig.AddValue("fuelCrossFeed", _fuelCrossFeed);

            //add RESOURCE
            foreach (ConfigNode res in _resourceslist)
            {
                partconfig.AddNode(res);
            }

            //add MODULE
            foreach (ConfigNode mod in _modulelist)
            {
                partconfig.AddNode(mod);
            }
        }
    } //class Welder
}

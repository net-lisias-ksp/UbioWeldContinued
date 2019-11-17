using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UbioWeldingLtd
{
	public class WeldedMeshSwitch : PartModule
	{

		[KSPField]
		public string objectIndicies = string.Empty;
		[KSPField]
		public string objects = string.Empty;
		[KSPField]
		public bool advancedDebug = false;
		[KSPField]
		public bool destroyUnusedParts = true;


		private List<Transform> parentTransforms = new List<Transform>();
		private List<List<Transform>> objectTransforms = new List<List<Transform>>();
		private bool initialized = false;
		private string[] dangerousModules = { "ModuleAblator" };


		public override void OnStart(StartState state)
		{
			initModule();
		}


		public void initModule()
		{
			parseTranseforms();
			checkForDangerousModules();
			activateTransforms();
		}


		public void checkForDangerousModules()
		{
			if (destroyUnusedParts)
			{
				foreach (string s in dangerousModules)
				{
					if (part.Modules.Contains(s))
					{
						Debugger.AdvDebug("[WeldedMeshSwitch] Found a dangerous Module = " + s, advancedDebug);
						destroyUnusedParts = false;
						break;
					}
				}
			}

		}


		public void parseTranseforms()
		{
			int[] batchedTransformIndicies = WeldedParseTools.ParseIntegers(objectIndicies).ToArray();
			if (batchedTransformIndicies.Length < 1)
			{
				Debugger.AdvDebug("[WeldedMeshSwitch] Found no Transforms in the TransformIndicies list", advancedDebug);
			}
			else
			{
				string[] batchedTransformNames = objects.Split(';');
				if (batchedTransformNames.Length < 1)
				{
					Debugger.AdvDebug("[WeldedMeshSwitch] Found no Transforms in the TransformNames list", advancedDebug);
				}
				else
				{
					if (batchedTransformIndicies.Length != batchedTransformNames.Length)
					{
						Debugger.AdvDebug("[WeldedMeshSwitch] List lengths for TransformIndicies = " + batchedTransformIndicies.Length + " and TransformNames = " + batchedTransformNames.Length + " are not equal", advancedDebug);
					}
					else
					{
						parentTransforms.Clear();
						for (int index = 0; index < batchedTransformIndicies.Length; index++)
						{
							Transform newTransform = part.FindModelTransform(Constants.weldModelNode.ToLower()).GetChild(batchedTransformIndicies[index]);
							if (newTransform == null)
							{
								Debugger.AdvDebug("[WeldedMeshSwitch] could not find Transform at index " + index, advancedDebug);
							}
							else
							{
								parentTransforms.Add(newTransform);
								Debugger.AdvDebug("[WeldedMeshSwitch] added Transform = " + newTransform.name + " from index " + index + "  to list", advancedDebug);

								List<Transform> newfinalTransforms = new List<Transform>();
								string[] finalTransformNames = batchedTransformNames[index].Split(',');
								foreach (string finalName in finalTransformNames)
								{
									while (newTransform.childCount < 2)
									{
										newTransform = newTransform.GetChild(0);
									}
									Transform newFinalTransform = newTransform.Find(finalName.Trim(' '));
									if (newFinalTransform == null)
									{
										Debugger.AdvDebug("[WeldedMeshSwitch] could not find Transform " + finalName, advancedDebug);
									}
									else
									{
										newfinalTransforms.Add(newFinalTransform);
										Debugger.AdvDebug("[WeldedMeshSwitch] added find Transform" + finalName + " to list", advancedDebug);
									}
								}
								if (newfinalTransforms.Count > 0)
								{
									objectTransforms.Add(newfinalTransforms);
								}
							}
						}
						initialized = true;
					}
				}
			}
		}


		public void activateTransforms()
		{
			if (initialized)
			{
				if (objectTransforms.Count >0)
				{
					for (int index = 0; index < parentTransforms.Count; index++)
					{
						Transform parent = parentTransforms[index];
						while (parent.childCount < 2)
						{
							parent = parent.GetChild(0);
						}
						foreach (Transform t in parent)
						{
							if (t != null)
							{
								if (objectTransforms[index].Contains(t))
								{
									t.gameObject.SetActive(true);
								}
								else
								{
									if (destroyUnusedParts)
									{
										Destroy(t.gameObject);
									}
									else
									{
										t.gameObject.SetActive(false);
									}
								}
							}
						}
					}
				}
			}
			else
			{
				Debugger.AdvDebug("[WeldedMeshSwitch] can not activate Transforms init failed", advancedDebug);
			}
		}


	}
}

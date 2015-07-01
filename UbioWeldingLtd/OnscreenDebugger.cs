using System.Collections.Generic;
using UnityEngine;

namespace UbioWeldingLtd
{

	[KSPAddon(KSPAddon.Startup.EditorAny, false)]
	public class OnscreenDebugger : MonoBehaviour
	{

		public void Update()
		{			
			if (Input.GetKeyUp(KeyCode.Return))
			{
				//advancedPartsListing(EditorLogic.RootPart.transform);
				//loadSceneRoots();
			}
		}


		private void advancedPartsListing(Transform target)
		{
			string message = "Part " + target.name + " active = " + target.gameObject.activeSelf + " | Childs = " + target.childCount;

			if (target.GetComponent<Collider>() != null)
			{
				message += " | Collider = " + target.GetComponent<Collider>().enabled;
			}
			else
			{
				message += " | Collider = none";
			}

			if(target.GetComponent<MeshRenderer>() != null)
			{
				message += " | Renderer = " + target.GetComponent<MeshRenderer>().enabled;
			}
			else
			{
				message += " | Renderer = none";
			}

			Debug.Log(message);

			foreach (Transform t in target)
			{
				advancedPartsListing(t);
			}
		}


		private void partsListing(Transform target)
		{
			string message = "Part " + target.name + " active = " + target.gameObject.activeSelf + " | Childs = " + target.childCount;
			Debug.Log(message);
			foreach (Transform t in target)
			{
				partsListing(t);
			}
		}

		private void loadSceneRoots()
		{
			List<Transform> Roots = new List<Transform>();
			foreach (Transform xform in UnityEngine.Object.FindObjectsOfType<Transform>())
			{
				Transform newTransform = xform.root;
				while (newTransform.parent != null)
				{
					newTransform = newTransform.parent;
				}
				if (!Roots.Contains(newTransform))
				{
					Roots.Add(newTransform);

					string[] scenestuff = { "vabscenery", "sphscenery", "vablvl2", "vablvl3", "vabmodern", "sphlvl1", "sphlvl2", "sphmodern" };

					foreach (string s in scenestuff)
					{
						if (string.Equals(newTransform.name.ToLower(), s))
						{
							Transform tempParent = newTransform;
							List<Transform> unattachedChildren = new List<Transform>();
							List<Transform> attachedChildren = new List<Transform>();

							while (tempParent.childCount < 2)
							{
								tempParent = tempParent.GetChild(0);
							}

							foreach (Transform t in tempParent)
							{
								Debug.Log(t.name + " active = " + t.gameObject.activeSelf + " | Childs = " + t.childCount);
								if (t.name.ToLower().Contains("crew"))
								{
									t.localScale /= 10;
									unattachedChildren.Add(t);
								}
								else
								{
									attachedChildren.Add(t);
								}
							}
							//tempParent.DetachChildren();
							//foreach (Transform t in attachedChildren)
							//{
								//t.parent = tempParent;
							//}

							newTransform.localScale = new Vector3(10, 10, 10);

							//foreach (Transform t in unattachedChildren)
							//{
								//t.parent = tempParent;
							//}
						}
					}
				}
			}

			foreach (Transform t in Roots)
			{
				string message = "Part " + t.name + " active = " + t.gameObject.activeSelf + " | Childs = " + t.childCount;
				Debug.Log(message);
			}
		}


	}

}

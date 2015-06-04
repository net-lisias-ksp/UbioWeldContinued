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
				listingParts(EditorLogic.RootPart.transform);
			}
		}


		private void listingParts(Transform target)
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
				listingParts(t);
			}
		}


	}

}

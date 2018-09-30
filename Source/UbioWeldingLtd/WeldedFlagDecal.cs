using System;
using System.Collections.Generic;
using UnityEngine;

namespace UbioWeldingLtd
{

	public class WeldedFlagDecal : PartModule
	{

		[KSPField]
		public string textureQuadName = string.Empty;
		[KSPField(isPersistant = true)]
		public bool flagDisplayed = true;


		private List<MeshRenderer> renderers = new List<MeshRenderer>();
		private Texture texture;
		private Shader originalDecalShader;
		private Material originalDecalMaterial;


		[KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "Toggle Flag")]
		public void ToggleFlag()
		{
			flagDisplayed = !flagDisplayed;
			switchDecalRenderMode();
		}


		private void switchDecalRenderMode()
		{
			if (renderers != null && renderers.Count > 0)
			{
				foreach (MeshRenderer r in renderers)
				{
					r.enabled = flagDisplayed;
				}
			}
		}


		public void Start()
		{
			initModule();
			switchDecalRenderMode();
		}
		

		public void initModule()
		{
			Transform originalT = part.FindModelTransform(textureQuadName);
			originalDecalMaterial = originalT.gameObject.GetComponent<MeshRenderer>().material;
			originalDecalShader = originalDecalMaterial.shader;

			Transform[] Transforms = part.FindModelTransforms(textureQuadName);
			foreach(Transform t in Transforms)
			{
				renderers.Add(t.GetComponent<MeshRenderer>());
			}
			UpdateFlagTexture(HighLogic.LoadedSceneIsEditor ? EditorLogic.FlagURL : part.flagURL);
			GameEvents.onFlagSelect.Add(onFlagSelect);
			clearDecalModule();
		}


		public void clearDecalModule()
		{
			Log.dbg("clearDecalModule start");
			PartModuleList modules = part.Modules;
			PartModule pm;
			for(int i =0; i < part.Modules.Count; i++)
			{
				pm = part.Modules[i];
				if (pm is FlagDecal)
				{
					if (((FlagDecal)pm).textureQuadName == textureQuadName)
					{
						Log.dbg("clearDecalModule flagDecal found");
						part.Modules.Remove(pm);
						Destroy(pm);
						Log.dbg("clearDecalModule flagDecal marked for destroy");
						break;
					}
				}
			}
			if(HighLogic.LoadedSceneIsEditor)
			{
				Log.dbg("clearDecalModule LoadedSceneIsEditor");
				GameEvents.onEditorPartEvent.Fire(ConstructionEventType.PartTweaked, part);
				GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
			}
			else if(HighLogic.LoadedSceneIsFlight)
			{
				Log.dbg("clearDecalModule LoadedSceneIsFlight");
				GameEvents.onVesselWasModified.Fire(vessel);
			}
			Log.dbg("clearDecalModule complete");
		}


		private void onFlagSelect(string selected)
		{
			UpdateFlagTexture(selected);
		}


		public void UpdateFlagTexture(string selection)
		{
			texture = GameDatabase.Instance.GetTexture(selection, false);
			part.flagURL = selection;
			if (renderers != null && renderers.Count > 0)
			{
				foreach (MeshRenderer r in renderers)
				{
					if (texture != null && originalDecalMaterial != null && originalDecalShader != null)
					{
						r.material = originalDecalMaterial;
						r.material.shader = originalDecalShader;
						r.material.mainTexture = texture;
					}
				}
			}
		}
	}
}


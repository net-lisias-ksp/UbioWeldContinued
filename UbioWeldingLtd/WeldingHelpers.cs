using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UbioWeldingLtd
{
	public static class WeldingHelpers
	{

		/// <summary>
		/// prepares the Categories for the saveing window
		/// </summary>
		/// <param name="inputList"></param>
		public static List<GUIContent> initPartCategories(List<GUIContent> inputList)
		{
			List<string> catlist = new List<string>(System.Enum.GetNames(typeof(PartCategories)));
			catlist.Remove(PartCategories.none.ToString());
			foreach (string cat in catlist)
			{
				inputList.Add(new GUIContent(cat));
			}
			return inputList;
		}


		/// <summary>
		/// creates a new dropdownmenu list for the techs that are required for the current welding part.
		/// </summary>
		/// <param name="techList"></param>
		/// <param name="guiStyle"></param>
		/// <param name="dropDown"></param>
		/// <returns></returns>
		public static GUIDropdown initVesselTypeDropDown(List<string> vesselTypeList, GUIStyle guiStyle, GUIDropdown dropDown)
		{
			List<GUIContent> contentList = new List<GUIContent>();

			List<string> allVesselTypes = new List<string>(System.Enum.GetNames(typeof(VesselType)));

			Debug.Log(string.Format("{0} vessel types Count = {1}", Constants.logPrefix, vesselTypeList.Count()));
			foreach (string vesselTypeID in vesselTypeList)
			{
				string vesselTypeTitle = vesselTypeID; //for case, when techID will not be found - the length of the list of titles always must match the length of the list of techID's.
				foreach (String vesselType in allVesselTypes)
				{
					if (vesselType == vesselTypeID)
					{
						vesselTypeTitle = vesselType;
						break;
					}
				}
				contentList.Add(new GUIContent(vesselTypeTitle));
			}
			dropDown = new GUIDropdown(contentList[0], contentList.ToArray(), "button", "box", guiStyle, 3);
			return dropDown;
		}


		/// <summary>
		/// creates a new dropdownmenu list for the techs that are required for the current welding part.
		/// </summary>
		/// <param name="techList"></param>
		/// <param name="guiStyle"></param>
		/// <param name="dropDown"></param>
		/// <returns></returns>
		public static GUIDropdown initTechDropDown(List<string> techList, GUIStyle guiStyle, GUIDropdown dropDown)
		{
			List<GUIContent> contentList = new List<GUIContent>();

			List<ProtoTechNode> rdTechs = AssetBase.RnDTechTree.GetTreeTechs().ToList();

			Debug.Log(string.Format("{0} rdTechs.Count = {1}", Constants.logPrefix, rdTechs.Count()));
			foreach (string techID in techList)
			{
				string techTitle = techID; //for case, when techID will not be found - the length of the list of titles always must match the length of the list of techID's.
				foreach (ProtoTechNode rdTech in rdTechs)
				{
					if (rdTech.techID == techID)
					{
						techTitle = rdTech.techID;
						break;
					}
				}
				contentList.Add(new GUIContent(techTitle));
			}
			dropDown = new GUIDropdown(contentList[0], contentList.ToArray(), "button", "box", guiStyle, 3);
			return dropDown;
		}


		/// <summary>
		/// prepares the given Huistyle with the default values of the tool
		/// </summary>
		/// <param name="inputGUIStyle"></param>
		/// <returns></returns>
		public static GUIStyle initGuiStyle(GUIStyle inputGUIStyle)
		{
			inputGUIStyle.normal.textColor = Color.white;
			inputGUIStyle.onHover.background =
			inputGUIStyle.hover.background = new Texture2D(2, 2);
			inputGUIStyle.padding.left =
			inputGUIStyle.padding.right =
			inputGUIStyle.padding.top =
			inputGUIStyle.padding.bottom = 1;
			return inputGUIStyle;
		}


		/// <summary>
		/// prepares the dropdownlist for a given list of GUIContents
		/// </summary>
		/// <param name="categoryList"></param>
		/// <param name="guiStyle"></param>
		/// <param name="dropDown"></param>
		/// <returns></returns>
		public static GUIDropdown initDropDown(List<GUIContent> categoryList, GUIStyle guiStyle, GUIDropdown dropDown)
		{
			dropDown = new GUIDropdown(categoryList[0], categoryList.ToArray(), "button", "box", guiStyle, 3);
			return dropDown;
		}


		/// <summary>
		/// checks an array for a dedicated value
		/// </summary>
		/// <param name="attributeToCheck"></param>
		/// <param name="arrayToCompare"></param>
		/// <returns></returns>
		public static bool isArrayContaining(string attributeToCheck, string[] arrayToCompare)
		{

			foreach (string attributeEntry in arrayToCompare)
			{
				if (string.Equals(attributeEntry.Trim(), attributeToCheck.Trim()))
				{
					return true;
				}
			}
			return false;
		}


		/// <summary>
		/// checks two arrays for shared values and returns them in a new array
		/// </summary>
		/// <param name="attributesToCheck"></param>
		/// <param name="arrayToCompare"></param>
		/// <returns></returns>
		public static string[] getSharedArrayValues(string[] attributesToCheck, string[] arrayToCompare)
		{
			//Debug.Log(string.Format("{0}| Checking Arrays - Array1 contains {1} Values - Array2 Contains {2} Values", Constants.logPrefix, attributesToCheck.Length, arrayToCompare.Length));
			if (attributesToCheck.Length > 0 && arrayToCompare.Length > 0)
			{
				List<string> sharedValues = new List<string>();
				foreach (string attributeToCheck in attributesToCheck)
				{
					foreach (string arrayEntryToCompare in arrayToCompare)
					{
						//Debug.Log(string.Format("{0}| Checking Arrays - Entry1 {1} - Entry2 {2}", Constants.logPrefix, attributeToCheck.Trim(), arrayEntryToCompare.Trim()));
						if (string.Equals(attributeToCheck.Trim(), arrayEntryToCompare.Trim()))
						{
							//Debug.Log(string.Format("{0}| Checking Arrays - shared Value found | {1}", Constants.logPrefix, attributeToCheck));
							if (!sharedValues.Contains(attributeToCheck))
							{
								sharedValues.Add(attributeToCheck);
								//Debug.Log(string.Format("{0}| Checking Arrays - Value {1} added", Constants.logPrefix, attributeToCheck));
							}
						}
					}
				}

				string[] cleanedList = new string[sharedValues.Count];
				for (int i = 0; i < sharedValues.Count; i++)
				{
					cleanedList[i] = sharedValues[i];
				}

				return cleanedList;
			}
			else
			{
				return new string[0];
			}
		}

		/// <summary>
		/// converts an array into a string array
		/// TODO make it generic to convert everything into String array
		/// </summary>
		/// <param name="genericArray"></param>
		/// <returns></returns>
		public static string[] convertFromToStringArray(ModuleAttribute[] genericArray)
		{
			if (genericArray.Length > 0)
			{
				string[] stringArray = new string[genericArray.Length];
				for (int i = 0; i < genericArray.Length; i++)
				{
					stringArray[i] = genericArray[i].attributeName;
				}
				return stringArray;
			}
			else
			{
				return new string[0];
			}
		}


		/// <summary>
		/// converts a string array into a moduleattribute Array
		/// TODO make it generic to convert everything into ModuleAttribute array
		/// </summary>
		/// <param name="stringArray"></param>
		/// <returns></returns>
		public static ModuleAttribute[] convertStringFromToArray(string[] stringArray)
		{
			ModuleAttribute[] genericArray = new ModuleAttribute[stringArray.Length];
			for (int i = 0; i < stringArray.Length; i++)
			{
				genericArray[i] = new ModuleAttribute(stringArray[i]);
			}
			return genericArray;
		}


		/// <summary>
		/// this rounds Vector3 coordinates to --Constants.weldNumberOfFractionalDigits-- fractional digits
		/// </summary>
		/// <returns></returns>
		public static Vector3 RoundVector3(Vector3 inVector, int digits)
		{
			float x = (float)(Math.Round(inVector.x, digits));
			float y = (float)(Math.Round(inVector.y, digits));
			float z = (float)(Math.Round(inVector.z, digits));
			return new Vector3(x, y, z);
		}


		/// <summary>
		/// this rounds float value to --Constants.weldNumberOfFractionalDigits-- fractional digits
		/// </summary>
		/// <returns></returns>
		public static float RoundFloat(float inValue, int digits)
		{
			return (float)(Math.Round(inValue, digits));
		}


		/// <summary>
		/// checks if a wanted text is inside a string
		/// </summary>
		/// <param name="text"></param>
		/// <param name="searchText"></param>
		/// <returns></returns>
		public static bool DoesTextContainRegex(string text, string searchText)
		{
			if (new Regex(searchText).IsMatch(text))
			{
				return true;
			}
			return false;
		}


		/// <summary>
		/// shortens a given string by the length of a searched text, in case that it is contained inside the given text
		/// </summary>
		/// <param name="text"></param>
		/// <param name="textToRemove"></param>
		public static void removeTextRegex(ref string text, string textToRemove)
		{
			while (new Regex(textToRemove).IsMatch(text))
			{
				text = text.Replace(textToRemove, "");
				//text = text.Substring(0, text.Length - textToRemove.Length);
			}
		}



		public static float angleClamp(float input, float min, float max)
		{
			while (input >= max)
			{
				input -= 360;
			}

			while (input < min)
			{
				input += 360;
			}
			return input;
		}


		/// <summary>
		/// limits a rotationvector to a value between 0 and 359 as 360 is just 0
		/// </summary>
		/// <param name="inputVector"></param>
		/// <returns></returns>
		public static Vector3 limitRotationAngle(Vector3 inputVector)
		{
			bool[] angleLimiting = { false, false, false };
			if (RoundFloat(inputVector.x, 3).Equals(360))
			{
				angleLimiting[0] = true;
			}
			if (RoundFloat(inputVector.y, 3).Equals(360))
			{
				angleLimiting[1] = true;
			}
			if (RoundFloat(inputVector.z, 3).Equals(360))
			{
				angleLimiting[2] = true;
			}
			return new Vector3(angleLimiting[0] ? 0 : inputVector.x, angleLimiting[1] ? 0 : inputVector.y, angleLimiting[2] ? 0 : inputVector.z);
		}


		public static bool isVectorEqualFactor(Vector3 inputVector, float factor)
		{
			bool[] isVectorEqual = { false, false, false };
			if (RoundFloat(inputVector.x, 3).Equals(RoundFloat(factor, 3)))
			{
				isVectorEqual[0] = true;
			}
			if (RoundFloat(inputVector.y, 3).Equals(RoundFloat(factor, 3)))
			{
				isVectorEqual[1] = true;
			}
			if (RoundFloat(inputVector.z, 3).Equals(RoundFloat(factor, 3)))
			{
				isVectorEqual[2] = true;
			}
			foreach (bool b in isVectorEqual)
			{
				if (b == false)
				{
					return false;
				}
			}
			return true;
		}


		public static Vector3 multiplyVector3(Vector3 VectorOne, Vector3 VectorTwo)
		{
			return new Vector3(VectorOne.x * VectorTwo.x, VectorOne.y * VectorTwo.y, VectorOne.z * VectorTwo.z);
		}


		public static string loadListIntoString<T>(string buildingResult, List<T> list, string seperator)
		{
			foreach (T obj in list)
			{
				if (string.IsNullOrEmpty(buildingResult))
				{
					buildingResult = obj.ToString();
				}
				else
				{
					buildingResult += string.Format("{0} {1}", seperator, obj.ToString());
				}
			}
			return buildingResult;
		}


		public static string writeVector(Vector3 inputvector)
		{
			List<float> components = new List<float>();
			string output = string.Empty;

			components.Add(inputvector.x);
			components.Add(inputvector.y);
			components.Add(inputvector.z);

			output = loadListIntoString(output, components, Constants.weldedMeshSwitchSubSplitter);

			return output;
		}


	}
}

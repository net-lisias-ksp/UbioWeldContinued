using System;
using System.Collections.Generic;
using UnityEngine;

namespace UbioWeldingLtd
{
    public static class WeldingHelpers
    {

        /// <summary>
        /// prepares the Categories for the saveing window
        /// </summary>
        /// <param name="inputList"></param>
        /// <param name="inputDropDown"></param>
        /// <param name="inputGUIStyle"></param>
        public static List<GUIContent> initPartCategories(List<GUIContent> inputList)
        {
            //inputList = new List<GUIContent>();
            List<string> catlist = new List<string>(System.Enum.GetNames(typeof(PartCategories)));
            catlist.Remove(PartCategories.none.ToString());
            foreach (string cat in catlist)
            {
                inputList.Add(new GUIContent(cat));
            }
            return inputList;
        }


        /// <summary>
        /// prepares the given Huistyle with the default values of the tool
        /// </summary>
        /// <param name="inputGUIStyle"></param>
        /// <returns></returns>
        public static GUIStyle initGuiStyle(GUIStyle inputGUIStyle)
        {
            //inputGUIStyle = new GUIStyle();
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
            dropDown = new GUIDropdown(categoryList[0], categoryList.ToArray(), "button", "box", guiStyle);
            return dropDown;
        }


        /// <summary>
        /// checks an array for a dedicated value
        /// </summary>
        /// <param name="attributeToCheck"></param>
        /// <param name="arrayToCompare"></param>
        /// <returns></returns>
        public static bool isArrayContaing(string attributeToCheck, string[] arrayToCompare)
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
            string[] stringArray = new string[genericArray.Length];
            for (int i = 0; i < genericArray.Length; i++)
            {
                stringArray[i] = genericArray[i].attributeName;
            }
            return stringArray;
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


    }
}

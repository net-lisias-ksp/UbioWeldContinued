using System;
using UnityEngine;

namespace UbioWeldingLtd
{
	class AdvancedGUITextField
	{
		private int _ControlID = -1;

		public string DrawAdvancedGUITextField(Rect rect, string text, int length, int windowID)
		{
			int activeID = GUIUtility.GetControlID(FocusType.Passive);

			if (_ControlID == -1)
			{
				_ControlID = activeID;
			}

			if (_ControlID != activeID && Event.current.type == EventType.MouseDown && !rect.Contains(Event.current.mousePosition))
			{
				GUI.FocusControl(GUI.GetNameOfFocusedControl());
			}
			return GUI.TextField(rect, text, length);
		}
	}
}

using System;
using UnityEngine;

namespace UbioWeldingLtd
{
	public class AdvancedGUITextArea
	{
		private int _ControlID = -1;

		public string DrawAdvancedGUITextArea(Rect rect, string text, int length, int windowID)
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
			return GUI.TextArea(rect, text, length);
		}
	}
}

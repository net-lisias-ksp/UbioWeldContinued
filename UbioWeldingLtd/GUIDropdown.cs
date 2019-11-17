using UnityEngine;

/*
 * Create a dropdown list
 * based on http://wiki.unity3d.com/index.php?title=PopupList
 */

namespace UbioWeldingLtd
{
	public class GUIDropdown
	{
		private static int controlID = -1;
		private bool isClickedComboButton = false;
		private int selectedItemIndex = 0;
 
		private GUIContent buttonContent;
		private GUIContent[] listContent;
		private string buttonStyle;
		private string boxStyle;
		private GUIStyle listStyle;
		private int _darknessFactor;
 
		public GUIDropdown( GUIContent buttonContent, GUIContent[] listContent, GUIStyle listStyle )
		{
			this.buttonContent = buttonContent;
			this.listContent = listContent;
			this.buttonStyle = "button";
			this.boxStyle = "box";
			this.listStyle = listStyle;
		}

		public GUIDropdown(GUIContent buttonContent, GUIContent[] listContent, string buttonStyle, string boxStyle, GUIStyle listStyle, int darkness)
		{
			this.buttonContent = buttonContent;
			this.listContent = listContent;
			this.buttonStyle = buttonStyle;
			this.boxStyle = boxStyle;
			this.listStyle = listStyle;
			this._darknessFactor = darkness;
		}

		public int Show(Rect rect)
		{
			bool done = false;
			int activeID = GUIUtility.GetControlID(FocusType.Passive);

			switch (Event.current.GetTypeForControl(controlID))
			{
				case EventType.MouseUp:
					{
						if (isClickedComboButton)
						{
							done = true;
						}
					}
					break;
			}

			if( GUI.Button( rect, buttonContent, buttonStyle ))
			{
				if( controlID == -1 )
				{
					controlID = activeID;
					isClickedComboButton = false;
				}
				isClickedComboButton = !isClickedComboButton;
				GUI.FocusControl(null);
			}

			if (isClickedComboButton)
			{
				Rect listRect = new Rect(rect.x, rect.y + listStyle.CalcHeight(listContent[0], 1.0f), rect.width, listStyle.CalcHeight(listContent[0], 1.0f) * listContent.Length);
				if (!closeOnOutsideClick(listRect))
				{
					for (int i = 0; i < _darknessFactor; i++)
					{
						GUI.Box(listRect, "", boxStyle);
					}
					int newSelectedItemIndex = GUI.SelectionGrid(listRect, selectedItemIndex, listContent, 1, listStyle);
					if (newSelectedItemIndex != selectedItemIndex)
					{
						selectedItemIndex = newSelectedItemIndex;
						buttonContent = listContent[selectedItemIndex];
					}
				}

				if (done)
				{
					isClickedComboButton = false;
				}
			}
			return selectedItemIndex;
		}

		public int SelectedItemIndex
		{
			get { return selectedItemIndex; }
			set { selectedItemIndex = value; buttonContent = listContent[selectedItemIndex]; }
		}

		public bool IsOpen
		{
			get { return isClickedComboButton; }
		}

		public void hide()
		{
			isClickedComboButton = false;
		}

		internal bool closeOnOutsideClick(Rect dropdownRect)
		{
			if (IsOpen && (Event.current.type == EventType.MouseDown) && !dropdownRect.Contains(Event.current.mousePosition))
			{
				hide();
				return true;
			}
			else
			{
				return false;
			}
		}

	} //class GUIDropdown
}

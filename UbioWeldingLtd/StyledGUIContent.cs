using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UbioWeldingLtd
{
	public class StyledGUIContent
	{
			private GUIContent _content = null;
			private GUIStyle _style = null;


			public StyledGUIContent()
			{
			}


			public StyledGUIContent(String text, GUIStyle Style)
			{
				this._content = new GUIContent(text);
				this._style = new GUIStyle(Style);
			}


			public StyledGUIContent(GUIContent src, GUIStyle Style)
			{
				this._content = new GUIContent(src);
				this._style = new GUIStyle(Style);
			}


			public StyledGUIContent(Texture image, GUIStyle Style)
			{
				this._content = new GUIContent(image);
				this._style = new GUIStyle(Style);
			}


			public StyledGUIContent(String text, Texture image, GUIStyle Style)
			{
				this._content = new GUIContent(text, image);
				this._style = new GUIStyle(Style);
			}


			public StyledGUIContent(String text)
			{
				this._content = new GUIContent(text);
			}


			public StyledGUIContent(GUIContent src)
			{
				this._content = new GUIContent(src);
			}


			public StyledGUIContent(Texture image)
			{
				this._content = new GUIContent(image);
			}


			public StyledGUIContent(String text, Texture image)
			{
				this._content = new GUIContent(text, image);
			}


			public Single calculateWidth
			{
				get
				{
					Single RunningTotal = 0;
					if (_style != null)
					{
						RunningTotal = _style.CalcSize(_content).x;
					}
					if (_content.image != null)
					{
						RunningTotal += _content.image.width;
					}
					if (RunningTotal == 0)
					{
						RunningTotal = 10;
					}
					return RunningTotal;
				}
			}


			public Single calculateHeight
			{
				get { if (_style != null) return _style.CalcSize(_content).x; else return 20; }
			}


		public  GUIContent content
		{
			get {return _content;}
			set {_content = value;}
		}
		public GUIStyle style
		{
			get { return _style; }
			set { _style = value; }
		}

	}
}

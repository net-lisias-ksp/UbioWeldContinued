using UnityEngine;

namespace UbioWeldingLtd
{
	public static class Debugger
	{

		/// <summary>
		/// provides additional logging information if enabled
		/// </summary>
		/// <param name="debugText"></param>
		/// <param name="advancedDebugging"></param>
		public static void AdvDebug(string debugText, bool advancedDebugging)
		{
			if (advancedDebugging)
			{
				Debug.Log(string.Format("{0} {1}", Constants.logPrefix, debugText));
			}
		}

	}
}

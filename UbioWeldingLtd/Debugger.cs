using UnityEngine;

namespace UbioWeldingLtd
{
	public static class Dbg
	{
		/// <summary>
		/// provides additional logging information if enabled
		/// </summary>
		/// <param name="debugText"></param>
		/// <param name="advancedDebugging"></param>
		public static void log(string text, bool verbose=true)
		{
			if (verbose) Debug.Log(string.Format("{0} {1}", Constants.logPrefix, text));
		}
	}
}

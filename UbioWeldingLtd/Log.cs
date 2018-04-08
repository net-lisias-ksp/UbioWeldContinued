using UnityEngine;

namespace UbioWeldingLtd
{
	public static class Log
	{
		public static bool debug = false;

		public static void info(string format, params object[] parms)
		{
			Debug.Log(string.Format("[{0}] {1}", Constants.logPrefix, string.Format(format, parms)));
		}

		public static void warn(string format, params object[] parms)
		{
			Debug.LogWarning(string.Format("[{0}] WARNING {1}", Constants.logPrefix, string.Format(format, parms)));
		}

		public static void err(string format, params object[] parms)
		{
			Debug.LogError(string.Format("[{0}] ERROR {1}", Constants.logPrefix, string.Format(format, parms)));
		}

		public static void ex(System.Exception e, MonoBehaviour offended)
		{
			Debug.LogException(e, offended);
		}

		public static void dbg(string format, params object[] parms)
		{
			if (debug) Debug.Log(string.Format("[{0}] DEBUG {1}", Constants.logPrefix, string.Format(format, parms)));
		}

		public static void dbgWarn(string format, params object[] parms)
		{
			if (debug) Debug.Log(string.Format("[{0}] DEBUG WARNING {1}", Constants.logPrefix, string.Format(format, parms)));
		}
	}
}

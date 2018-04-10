using UnityEngine;

namespace UbioWeldingLtd
{
	public static class Log
	{
		public static int debug = 0;

		public static void info(string format, params object[] parms)
		{
			Debug.Log(string.Format("[{0}] {1}", Constants.logPrefix, string.Format(format, parms)));
		}
		public static void info(string text)
		{
			Debug.Log(string.Format("[{0}] {1}", Constants.logPrefix, text));
		}

		public static void warn(string format, params object[] parms)
		{
			Debug.LogWarning(string.Format("[{0}] WARNING {1}", Constants.logPrefix, string.Format(format, parms)));
		}
		public static void warn(string text)
		{
			Debug.LogWarning(string.Format("[{0}] WARNING {1}", Constants.logPrefix, text));
		}

		public static void err(string format, params object[] parms)
		{
			Debug.LogError(string.Format("[{0}] ERROR {1}", Constants.logPrefix, string.Format(format, parms)));
		}
		public static void err(string text)
		{
			Debug.LogError(string.Format("[{0}] ERROR {1}", Constants.logPrefix, text));
		}

		public static void ex(MonoBehaviour offended, System.Exception e)
		{
			Debug.LogError(string.Format("[{0}] ERROR {1}", Constants.logPrefix, string.Format("{0} raised Exception {1}", offended.name, e.ToString())));
			Debug.LogException(e, offended);
		}

		public static void dbg(string format, params object[] parms)
		{
			if (debug > 0) Debug.Log(string.Format("[{0}] DEBUG {1}", Constants.logPrefix, string.Format(format, parms)));
		}
		public static void dbg(string text)
		{
			if (debug > 0) Debug.Log(string.Format("[{0}] DEBUG {1}", Constants.logPrefix, text));
		}

		public static void dbgWarn(string format, params object[] parms)
		{
			if (debug > 1) Debug.Log(string.Format("[{0}] DEBUG WARNING {1}", Constants.logPrefix, string.Format(format, parms)));
		}
		public static void dbgWarn(string text)
		{
			if (debug > 1) Debug.Log(string.Format("[{0}] DEBUG WARNING {1}", Constants.logPrefix, text));
		}

		public static void dbgGui(MonoBehaviour logger, string format, object[] parms)
		{
			if (debug > 2) Debug.Log(string.Format("[{0}] DEBUG GUI {1} - {2}", Constants.logPrefix, logger.name, string.Format(format, parms)));
		}
		public static void dbgGui(MonoBehaviour logger, string text)
		{
			if (debug > 2) Debug.Log(string.Format("[{0}] DEBUG GUI {1} - {2}", Constants.logPrefix, logger.name, text));
		}
	}
}

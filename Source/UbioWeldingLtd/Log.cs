using System.Diagnostics;
using UnityEngine;

using KSPe.Util.Log;
using Logger = KSPe.Util.Log.Logger;

namespace UbioWeldingLtd
{
	public static class Log
	{
		private static readonly Logger LOG = Logger.CreateForType<UbioZurWeldingLtd>(Constants.logPrefix);

		public static int debuglevel {
			get => (int)LOG.level;
			set => LOG.level = (KSPe.Util.Log.Level)(value % 6);
		}

		public static void log(string format, params object[] @parms)
		{
			LOG.force(format, parms);
		}

		public static void info(string format, params object[] @parms)
		{
			LOG.info(format, parms);
		}

		public static void warn(string format, params object[] @parms)
		{
			LOG.warn(format, parms);
		}

		public static void err(string format, params object[] parms)
		{
			LOG.error(format, parms);
		}

		public static void ex(MonoBehaviour offended, System.Exception e)
		{
			LOG.error(offended, e);
		}

		public static void dbg(string format, params object[] @parms)
		{
			LOG.dbg(format, parms);
		}

		public static void dbgWarn(string format, params object[] @parms)
		{
			LOG.warn(format, parms);
		}

		[ConditionalAttribute("DEBUG")]
		public static void dbgGui(MonoBehaviour logger, string format, params object[] @parms)
		{
			LOG.trace(format, parms);
		}
	}
}

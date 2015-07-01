using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbioWeldingLtd
{
	public static class WeldedParseTools
	{


		public static List<int> ParseIntegers(string stringOfInts)
		{
			List<int> newIntList = new List<int>();
			string[] valueArray = stringOfInts.Split(';');
			for (int i = 0; i < valueArray.Length; i++)
			{
				int newValue = 0;
				if (int.TryParse(valueArray[i], out newValue))
				{
					newIntList.Add(newValue);
				}
				else
				{
					Debugger.AdvDebug("[WeldedParseTools] invalid integer: " + valueArray[i], true);
				}
			}
			return newIntList;
		}


	}
}

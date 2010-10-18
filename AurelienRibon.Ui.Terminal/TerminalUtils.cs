using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AurelienRibon.Ui.Terminal {
	public static class TerminalUtils {
		public static Command ParseCommandLine(string line) {
			string command = "";
			List<string> args = new List<string>();

			Match m = Regex.Match(line.Trim() + " ", @"^(.+?\s+|.+?$)(.*)");
			if (m.Success) {
				command = m.Groups[1].Value.Trim();
				string argsLine = m.Groups[2].Value.Trim();
				Match m2 = Regex.Match(argsLine + " ", @""".*?""\s+|.+?\s+");
				while (m2.Success) {
					string arg = Regex.Replace(m2.Value.Trim(), @"^""(.*?)""$", "$1");
					args.Add(arg);
					m2 = m2.NextMatch();
				}
			}

			return new Command(line, command, args.ToArray());
		}
	}
}

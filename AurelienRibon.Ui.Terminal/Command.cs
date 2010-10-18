using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AurelienRibon.Ui.Terminal {
	public class Command {
		public string Raw { get; private set; }
		public string Name { get; private set; }
		public string[] Args { get; private set; }

		public Command(string raw, string name, string[] args) {
			Raw = raw;
			Name = name;
			Args = args;
		}
	}
}

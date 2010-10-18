using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using System.Media;

namespace AurelienRibon.Ui.Terminal {
	public class Terminal : TextBox {
		public enum Modes { SIMPLE, COMMAND }
		public Modes Mode { get; set; }
		public List<Command> CommandLog { get; private set; }

		private bool isInputEnabled = false;
		private int lastPomptIndex = -1;
		private int indexInLog = 0;

		public Terminal() {
			IsUndoEnabled = false;
			AcceptsReturn = false;
			AcceptsTab = true;
			Mode = Modes.SIMPLE;
			CommandLog = new List<Command>();
			PreviewKeyDown += new KeyEventHandler(OnPreviewKeyDown);
			Loaded += (s, e) => { InsertNewPrompt(); };
		}

		public void InsertNewPrompt() {
			Text += "\n" + "> ";
			CaretIndex = Text.Length;
			lastPomptIndex = CaretIndex;
			isInputEnabled = true;
			Focus();
		}

		public string GetLastCommandDescription(string commandFormat, string firstArgFormat, string otherArgsFormat, string end) {
			if (CommandLog.Count == 0)
				return "Command log is empty";
			Command cmd = CommandLog[CommandLog.Count - 1];
			string ret = string.Format(commandFormat, cmd.Name);
			if (cmd.Args.Length > 0)
				ret += string.Format(firstArgFormat, cmd.Args[0]);
			for (int i = 1; i < cmd.Args.Length; i++)
				ret += string.Format(otherArgsFormat, cmd.Args[i]);
			return ret + end;
		}

		// --------------------------------------------------------------------

		private void OnPreviewKeyDown(object sender, KeyEventArgs e) {
			Terminal term = (Terminal)sender;
			int initialLength = term.Text.Length;

			if (!isInputEnabled) {
				e.Handled = true;
				SystemSounds.Beep.Play();
				return;
			}

			if (CaretIndex < lastPomptIndex
				|| (CaretIndex == lastPomptIndex && e.Key == Key.Back)
				|| (CaretIndex == lastPomptIndex && e.Key == Key.Left)) {
				CaretIndex = Text.Length;
				e.Handled = true;
				SystemSounds.Beep.Play();
				return;
			} else if (CaretIndex >= lastPomptIndex && e.Key == Key.Up) {
				if (indexInLog > 0)
					indexInLog--;
				if (CommandLog.Count > 0) {
					Text = GetTextWithPromptSuffix(CommandLog[indexInLog].Raw);
					CaretIndex = Text.Length;
				}
				e.Handled = true;
			} else if (CaretIndex >= lastPomptIndex && e.Key == Key.Down) {
				if (indexInLog < CommandLog.Count - 1)
					indexInLog++;
				if (CommandLog.Count > 0) {
					Text = GetTextWithPromptSuffix(CommandLog[indexInLog].Raw);
					CaretIndex = Text.Length;
				}
				e.Handled = true;
			}

			if (e.Key == Key.Enter) {
				string line = Text.Substring(lastPomptIndex);
				Text += "\n";
				isInputEnabled = false;
				lastPomptIndex = int.MaxValue;
				switch (Mode) {
					case Modes.SIMPLE:
						InsertNewPrompt();
						break;
					case Modes.COMMAND:
						Command cmd = TerminalUtils.ParseCommandLine(line);
						CommandLog.Add(cmd);
						indexInLog = CommandLog.Count;
						RaiseCommandEntered(cmd);
						break;
				}
			}
		}

		private string GetTextWithPromptSuffix(string suffix) {
			string ret = Text.Substring(0, lastPomptIndex);
			return ret + suffix;
		}

		// --------------------------------------------------------------------

		public event EventHandler<CommandEventArgs> CommandEntered;
		public class CommandEventArgs : EventArgs {
			public Command Command { get; private set; }
			public CommandEventArgs(Command command) {
				Command = command;
			}
		}

		private void RaiseCommandEntered(Command command) {
			if (CommandEntered != null)
				CommandEntered(this, new CommandEventArgs(command));
		}
	}
}

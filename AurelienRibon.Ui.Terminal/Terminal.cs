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
		public List<string> RegisteredCommands { get; private set; }
		public List<Command> CommandLog { get; private set; }
		public bool IsPromptInsertedAtLaunch { get; set; }
		public bool IsSystemBeepEnabled { get; set; }

		private bool isInputEnabled = false;
		private int lastPomptIndex = -1;
		private int indexInLog = 0;

		public Terminal() {
			IsUndoEnabled = false;
			AcceptsReturn = false;
			AcceptsTab = false;

			RegisteredCommands = new List<string>();
			CommandLog = new List<Command>();
			IsPromptInsertedAtLaunch = true;
			IsSystemBeepEnabled = true;

			PreviewKeyDown += new KeyEventHandler(OnPreviewKeyDown);
			Loaded += (s, e) => {
				if (IsPromptInsertedAtLaunch)
					InsertNewPrompt();
			};
		}

		public void InsertNewPrompt() {
			if (Text.Length == 0)
				Text += "> ";
			else
				Text += Text.EndsWith("\n") ? "\n> " : "\n\n> ";
			CaretIndex = Text.Length;
			lastPomptIndex = Text.Length;
			isInputEnabled = true;
		}

		// --------------------------------------------------------------------
		// EVENT HANDLERS
		// --------------------------------------------------------------------

		private void OnPreviewKeyDown(object sender, KeyEventArgs e) {
			Terminal term = (Terminal)sender;
			int initialLength = term.Text.Length;

			if (!isInputEnabled) {
				e.Handled = true;
				if (IsSystemBeepEnabled)
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

			// Push en ENTER key
			if (e.Key == Key.Enter) {
				string line = Text.Substring(lastPomptIndex);
				Text += "\n";
				isInputEnabled = false;
				lastPomptIndex = int.MaxValue;

				Command cmd = TerminalUtils.ParseCommandLine(line);
				CommandLog.Add(cmd);
				indexInLog = CommandLog.Count;
				RaiseCommandEntered(cmd);
				e.Handled = true;

			// Push on TAB key
			} else if (e.Key == Key.Tab) {
				if (CaretIndex != Text.Length) {
					e.Handled = true;
					return;
				}

				string line = Text.Substring(lastPomptIndex);
				string[] commands = GetAssociatedCommands(line);

				if (commands.Length > 0) {
					Text = Text.Remove(lastPomptIndex);
					Text += GetCommonPrefix(commands);
					CaretIndex = Text.Length;
				}

				e.Handled = true;
			}
		}

		private string GetTextWithPromptSuffix(string suffix) {
			string ret = Text.Substring(0, lastPomptIndex);
			return ret + suffix;
		}

		private string[] GetAssociatedCommands(string prefix) {
			List<string> ret = new List<string>();
			foreach (var cmd in RegisteredCommands)
				if (cmd.StartsWith(prefix))
					ret.Add(cmd);
			return ret.ToArray();
		}

		private string GetShortestString(string[] strs) {
			string ret = strs[0];
			foreach (string str in strs)
				ret = str.Length < ret.Length ? str : ret;
			return ret;
		}

		private string GetCommonPrefix(string[] strs) {
			string shortestStr = GetShortestString(strs);
			for (int i = 0; i < shortestStr.Length; i++)
				foreach (string str in strs)
					if (str[i] != shortestStr[i])
						return shortestStr.Substring(0, i);
			return shortestStr;
		}

		// --------------------------------------------------------------------
		// CUSTOM EVENTS
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

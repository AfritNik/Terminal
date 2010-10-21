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
		public int LastPomptIndex { get; private set; }
		public string Prompt { get; set; }
		public bool IsInputEnabled { get; private set; }

		private int indexInLog = 0;

		public Terminal() {
			IsUndoEnabled = false;
			AcceptsReturn = false;
			AcceptsTab = false;

			RegisteredCommands = new List<string>();
			CommandLog = new List<Command>();
			IsPromptInsertedAtLaunch = true;
			IsSystemBeepEnabled = true;
			LastPomptIndex = -1;
			Prompt = "> ";
			IsInputEnabled = false;

			PreviewKeyDown += new KeyEventHandler(OnPreviewKeyDown);
			Loaded += (s, e) => {
				if (IsPromptInsertedAtLaunch)
					InsertNewPrompt();
			};
		}

		public void InsertNewPrompt() {
			if (Text.Length > 0)
				Text += Text.EndsWith("\n") ? "\n" : "\n\n";
			Text += Prompt;
			CaretIndex = Text.Length;
			LastPomptIndex = Text.Length;
			IsInputEnabled = true;
		}

		public void InsertTextBeforePrompt(string text) {
			Text = Text.Insert(LastPomptIndex - Prompt.Length - 1, text);
		}

		// --------------------------------------------------------------------
		// EVENT HANDLERS
		// --------------------------------------------------------------------

		private void OnPreviewKeyDown(object sender, KeyEventArgs e) {
			Terminal term = (Terminal)sender;
			int initialLength = term.Text.Length;

			if (!IsInputEnabled) {
				e.Handled = true;
				if (IsSystemBeepEnabled)
					SystemSounds.Beep.Play();
				return;
			}

			if (CaretIndex < LastPomptIndex
				|| (CaretIndex == LastPomptIndex && e.Key == Key.Back)
				|| (CaretIndex == LastPomptIndex && e.Key == Key.Left)) {
				CaretIndex = Text.Length;
				e.Handled = true;
				SystemSounds.Beep.Play();
				return;
			} else if (CaretIndex >= LastPomptIndex && e.Key == Key.Up) {
				if (indexInLog > 0)
					indexInLog--;
				if (CommandLog.Count > 0) {
					Text = GetTextWithPromptSuffix(CommandLog[indexInLog].Raw);
					CaretIndex = Text.Length;
				}
				e.Handled = true;
			} else if (CaretIndex >= LastPomptIndex && e.Key == Key.Down) {
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
				string line = Text.Substring(LastPomptIndex);
				Text += "\n";
				IsInputEnabled = false;
				LastPomptIndex = int.MaxValue;

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

				string line = Text.Substring(LastPomptIndex);
				string[] commands = GetAssociatedCommands(line);

				if (commands.Length > 0) {
					if (CaretIndex > LastPomptIndex)
						Text = Text.Remove(LastPomptIndex);
					Text += GetCommonPrefix(commands);
					CaretIndex = Text.Length;
				}

				e.Handled = true;
			}
		}

		private string GetTextWithPromptSuffix(string suffix) {
			string ret = Text.Substring(0, LastPomptIndex);
			return ret + suffix;
		}

		private string[] GetAssociatedCommands(string prefix) {
			List<string> ret = new List<string>();
			foreach (var cmd in RegisteredCommands)
				if (cmd.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
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
					if (char.ToLower(str[i]) != char.ToLower(shortestStr[i]))
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

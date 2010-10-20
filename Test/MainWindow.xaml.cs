using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AurelienRibon.Ui.Terminal;

namespace Test {
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();

			Loaded += (s, e) => {
				Term.CommandEntered += (ss, ee) => {
					string msg = ee.Command.GetDescription("Command is '{0}'", " with args '{0}'", ", '{0}'", ".");
					Term.Text += msg;
					Term.InsertNewPrompt();
				};
				Term.RegisteredCommands.Add("hello");
				Term.RegisteredCommands.Add("world");
				Term.RegisteredCommands.Add("helloworld");
			};
		}
	}
}

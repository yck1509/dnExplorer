using System;
using System.Drawing;
using System.Windows.Forms;

namespace dnExplorer {
	public static class InputBox {
		class InputBoxDialog : Form {
			TextBox txtBox;

			public InputBoxDialog(string title, string message) {
				Font = new Font("Segoe UI", 9);
				Text = title;
				StartPosition = FormStartPosition.CenterParent;
				FormBorderStyle = FormBorderStyle.FixedSingle;

				var size = TextRenderer.MeasureText(message, Font);
				ClientSize = new Size(Math.Max(size.Width + 20, 250), size.Height + 75);

				var label = new Label {
					Location = new Point(10, 10),
					Size = size,
					Text = message
				};
				Controls.Add(label);

				txtBox = new TextBox {
					Location = new Point(10, 15 + size.Height),
					Width = ClientSize.Width - 20
				};
				Controls.Add(txtBox);

				var btnX = (int)((Width - 150) / 2f);

				var btnOk = new Button {
					Text = "OK",
					Location = new Point(btnX, 20 + size.Height + txtBox.Height),
					Width = 70,
					Height = 25
				};
				Controls.Add(btnOk);

				var btnCancel = new Button {
					Text = "Cancel",
					Location = new Point(btnX + 80, 20 + size.Height + txtBox.Height),
					Width = 70,
					Height = 25
				};
				Controls.Add(btnCancel);

				AcceptButton = btnOk;
				CancelButton = btnCancel;

				btnOk.Click += (sender, e) => { DialogResult = DialogResult.OK; };

				btnCancel.Click += (sender, e) => { DialogResult = DialogResult.Cancel; };
			}

			public string Result {
				get { return txtBox.Text; }
			}
		}

		public static string Show(string message) {
			var dialog = new InputBoxDialog(Main.AppName, message);
			if (dialog.ShowDialog() == DialogResult.OK) {
				return dialog.Result;
			}
			return null;
		}

		public static string Show(string title, string message) {
			var dialog = new InputBoxDialog(title, message);
			if (dialog.ShowDialog() == DialogResult.OK) {
				return dialog.Result;
			}
			return null;
		}
	}
}
using System;
using System.Drawing;
using System.Windows.Forms;

namespace dnExplorer.Views {
	public class ModuleView : ViewBase {
		public ModuleView() {
			var label = new Label {
				Text = "This page is intentionally left blank\r\nI'm thinking of what to put here :P",
				TextAlign = ContentAlignment.MiddleCenter,
				Dock = DockStyle.Fill,
				AutoSize = false
			};
			Controls.Add(label);
		}

		protected override void OnModelUpdated() {
		}
	}
}
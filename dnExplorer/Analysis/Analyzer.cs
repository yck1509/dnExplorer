using System;
using System.Windows.Forms;
using dnExplorer.Trees;
using dnlib.DotNet;
using WeifenLuo.WinFormsUI.Docking;

namespace dnExplorer.Analysis {
	public class Analyzer : DockContent {
		TreeViewX treeView;

		public Analyzer(IApp app) {
			Text = "Analyzer";
			HideOnClose = true;

			treeView = new TreeViewX(app) {
				Dock = DockStyle.Fill,
				BorderStyle = BorderStyle.None
			};
			Controls.Add(treeView);

			app.DockArea.DockWindows[DockState.DockBottom].BringToFront();
			Show(app.DockArea, DockState.DockBottom);
			Hide();
		}

		public void Display(IDnlibDef item = null) {
			treeView.App.DockArea.DockWindows[DockState.DockBottom].BringToFront();
			Show(treeView.App.DockArea, DockState.DockBottom);
		}
	}
}
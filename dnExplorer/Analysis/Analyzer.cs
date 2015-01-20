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

			if (item != null) {
				if (item is MethodDef)
					treeView.Nodes.Add(MethodAnalysis.CreateAnalysis((MethodDef)item).ToNode());
			}
		}

		public static IAnalysis[] GetChildren(object item) {
			if (item is IMethod && ((IMethod)item).IsMethod) {
				var def = ((IMethod)item).ResolveMethodDef();
				if (def == null)
					return new IAnalysis[0];

				return new IAnalysis[] {
					new MethodUsedByAnalysis(def),
					new MethodUsesAnalysis(def)
				};
			}
			if (item is IField && ((IField)item).IsField) {
				var def = ((IField)item).ResolveFieldDef();
				if (def == null)
					return new IAnalysis[0];

				return new IAnalysis[0];
			}
			if (item is ITypeDefOrRef) {
				var def = ((ITypeDefOrRef)item).ResolveTypeDef();
				if (def == null)
					return new IAnalysis[0];

				return new IAnalysis[0];
			}
			return new IAnalysis[0];
		}
	}
}
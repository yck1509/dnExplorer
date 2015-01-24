using System;
using System.Windows.Forms;
using dnExplorer.Models;
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
			treeView.KeyDown += OnTreeViewKeyDown;
			Controls.Add(treeView);

			app.DockArea.DockWindows[DockState.DockBottom].BringToFront();
			Show(app.DockArea, DockState.DockBottom);
			Hide();
		}

		public void Display(IDnlibDef item = null) {
			treeView.App.DockArea.DockWindows[DockState.DockBottom].BringToFront();
			Show(treeView.App.DockArea, DockState.DockBottom);

			if (item != null) {
				if (item is ModuleDef && ((ModuleDef)item).Assembly != null)
					item = ((ModuleDef)item).Assembly;

				treeView.Nodes.Add(new MultipleAnalysesModel(item, GetChildren(item)).ToNode());
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

				return new IAnalysis[] {
					new FieldUsedByAnalysis(def),
					new FieldAssignedByAnalysis(def)
				};
			}
			if (item is ITypeDefOrRef) {
				var def = ((ITypeDefOrRef)item).ResolveTypeDef();
				if (def == null)
					return new IAnalysis[0];

				return new IAnalysis[] {
					new TypeUsedByAnalysis(def),
					new TypeInstantiatedByAnalysis(def)
				};
			}
			if (item is AssemblyDef) {
				return GetChildren(((AssemblyDef)item).ManifestModule);
			}
			if (item is ModuleDefMD) {
				return new IAnalysis[] {
					new ModuleReferencesAnalysis((ModuleDefMD)item),
					new ModulePInvokeAnalysis((ModuleDefMD)item)
				};
			}
			return new IAnalysis[0];
		}

		void OnTreeViewKeyDown(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.Delete) {
				var selectedNode = treeView.SelectedNode;
				if (selectedNode != null && selectedNode.Parent == null)
					treeView.Nodes.Remove(selectedNode);
			}
		}

		protected override void OnDockStateChanged(EventArgs e) {
			base.OnDockStateChanged(e);

			if (DockState == DockState.Hidden)
				treeView.Nodes.Clear();
		}
	}
}
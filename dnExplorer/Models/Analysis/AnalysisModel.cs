using System;
using System.Collections.Generic;
using System.Drawing;
using dnExplorer.Analysis;
using dnExplorer.Trees;

namespace dnExplorer.Models {
	public class AnalysisModel : LazyModel {
		IAnalysis analysis;
		bool root;

		public AnalysisModel(IAnalysis analysis, bool root = true) {
			this.analysis = analysis;
			this.root = root;
			Text = root ? analysis.Name : Utils.EscapeString(analysis.TargetObject.FullName, false);
		}

		protected override bool HasChildren {
			get { return analysis.HasResult; }
		}

		protected override bool IsVolatile {
			get { return true; }
		}

		protected override IEnumerable<IDataModel> PopulateChildren() {
			if (Node == null)
				yield break;

			foreach (var item in analysis.Run(((TreeViewX)Node.TreeView).App)) {
				if (item is AnalysisError)
					yield return new ErrorModel(((AnalysisError)item).Message);
				else if (item is IDataModel)
					yield return (IDataModel)item;
				else
					yield return new AnalysisModel(analysis.GetChildAnalysis(item), false);
			}
		}

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
			if (root) {
				g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.arrow.png"), bounds);
			}
			else {
				ObjectIconRenderer.Render(analysis.TargetObject, g, bounds);
			}
		}
	}
}
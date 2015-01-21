using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using dnExplorer.Analysis;
using dnExplorer.Trees;
using dnlib.DotNet;

namespace dnExplorer.Models {
	public class AnalysisModel : LazyModel {
		IAnalysis analysis;

		public IFullName Item {
			get { return analysis.TargetObject; }
		}

		public bool IsRoot { get; set; }

		public AnalysisModel(IAnalysis analysis, bool root = true) {
			this.analysis = analysis;
			IsRoot = root;
			Text = root ? analysis.Name : Utils.EscapeString(DisplayNameCreator.CreateFullName(analysis.TargetObject), false);
		}

		protected override bool HasChildren {
			get { return analysis.HasResult; }
		}

		protected override bool IsVolatile {
			get { return true; }
		}

		public override bool RecordHistory {
			get { return false; }
		}

		public override bool ShowViews {
			get { return false; }
		}

		protected override IEnumerable<IDataModel> PopulateChildren() {
			var models = new List<IDataModel>();
			if (Node == null)
				return models;

			foreach (var item in analysis.Run(((TreeViewX)Node.TreeView).App, CancellationToken.Value)) {
				if (item is AnalysisError)
					models.Add(new ErrorModel(((AnalysisError)item).Message));
				else {
					var analyses = analysis.GetChildAnalyses(item).ToArray();
					if (analyses.Length == 1)
						models.Add(new AnalysisModel(analyses[0], false));
					else
						models.Add(new MultipleAnalysesModel((IFullName)item, analyses));
				}
			}
			models.Sort((a, b) => Comparer<string>.Default.Compare(a.Text, b.Text));
			return models;
		}

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
			if (IsRoot) {
				g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.arrow.png"), bounds);
			}
			else {
				ObjectIconRenderer.Render(analysis.TargetObject, g, bounds);
			}
		}
	}
}
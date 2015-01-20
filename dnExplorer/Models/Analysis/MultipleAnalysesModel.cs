using System;
using System.Collections.Generic;
using System.Drawing;
using dnExplorer.Analysis;
using dnExplorer.Trees;
using dnlib.DotNet;

namespace dnExplorer.Models {
	public class MultipleAnalysesModel : LazyModel {
		IAnalysis[] analyses;

		public IFullName Item { get; set; }

		public MultipleAnalysesModel(IFullName item, IAnalysis[] analyses) {
			this.analyses = analyses;
			Item = item;
			Text = Utils.EscapeString(DisplayNameCreator.CreateFullName(item), false);
		}

		protected override bool HasChildren {
			get { return true; }
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

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
			ObjectIconRenderer.Render(Item, g, bounds);
		}

		protected override IEnumerable<IDataModel> PopulateChildren() {
			foreach (var child in analyses)
				yield return new AnalysisModel(child, true);
		}
	}
}
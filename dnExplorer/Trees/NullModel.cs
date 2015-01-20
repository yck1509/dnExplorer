using System;
using System.ComponentModel;
using System.Drawing;

namespace dnExplorer.Trees {
	public class NullModel : IDataModel {
		public static readonly NullModel Instance = new NullModel();

		NullModel() {
		}

		BatchObservableCollection<IDataModel> models;

		public void OnCollapse() {
		}

		public void OnExpand() {
		}

		public BatchObservableCollection<IDataModel> Children {
			get { return models ?? (models = new BatchObservableCollection<IDataModel>()); }
		}

		public DataTreeNodeX Node { get; set; }

		public Color ForeColor {
			get { return Color.Black; }
		}

		public string Text {
			get { return ""; }
		}

		public bool HasIcon {
			get { return false; }
		}

		public bool RecordHistory {
			get { return false; }
		}

		public bool ShowViews {
			get { return false; }
		}

		public void DrawIcon(Graphics g, Rectangle bounds) {
		}

		public void Refresh(bool refreshChildren) {
		}

		public event EventHandler CollapseRequested;

		public event EventHandler ExpandRequested;

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
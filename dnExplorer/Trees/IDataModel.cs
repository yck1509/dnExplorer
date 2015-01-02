using System;
using System.ComponentModel;
using System.Drawing;

namespace dnExplorer.Trees {
	public interface IDataModel : INotifyPropertyChanged {
		void OnCollapse();
		void OnExpand();

		BatchObservableCollection<IDataModel> Children { get; }

		DataTreeNodeX Node { get; set; }

		Color ForeColor { get; }
		string Text { get; }

		bool HasIcon { get; }
		void DrawIcon(Graphics g, Rectangle bounds);
		void Refresh(bool refreshChildren);

		event EventHandler CollapseRequested;
		event EventHandler ExpandRequested;
	}
}
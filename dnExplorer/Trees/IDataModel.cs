﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace dnExplorer.Trees {
	public interface IDataModel : INotifyPropertyChanged {
		void OnCollapse();
		void OnExpand();

		BatchObservableCollection<IDataModel> Children { get; }

		DataTreeNodeX Node { get; set; }
		ContextMenuStrip ContextMenu { get; }

		Color ForeColor { get; }
		string Text { get; }

		bool HasIcon { get; }
		void DrawIcon(Graphics g, Rectangle bounds);
		void Refresh(bool refreshChildren);

		event EventHandler CollapseRequested;
		event EventHandler ExpandRequested;
	}
}
using System;
using System.Collections.Generic;
using dnExplorer.Trees;
using dnExplorer.Views;

namespace dnExplorer {
	public class NavigationHistory {
		class HistoryNode {
			public HistoryNode(HistoryItem item) {
				Item = item;
			}

			public HistoryNode Previous;
			public readonly HistoryItem Item;
			public HistoryNode Next;
		}

		HistoryNode current;

		public HistoryItem Current {
			get { return current == null ? null : current.Item; }
		}

		public bool CanGoBack {
			get { return current != null && current.Previous != null; }
		}

		public bool CanGoForward {
			get { return current != null && current.Next != null; }
		}

		public HistoryItem Record(IDataModel model) {
			var item = new HistoryItem(model);
			var newNode = new HistoryNode(item);
			if (current != null) {
				current.Next = newNode;
				newNode.Previous = current;
			}
			current = newNode;
			return item;
		}

		public void GoBack() {
			if (!CanGoBack)
				return;

			current = current.Previous;
			if (Navigated != null)
				Navigated(this, EventArgs.Empty);
		}

		public void GoForward() {
			if (!CanGoForward)
				return;

			current = current.Next;
			if (Navigated != null)
				Navigated(this, EventArgs.Empty);
		}

		public event EventHandler Navigated;
	}

	public class HistoryItem {
		internal HistoryItem(IDataModel model) {
			Model = model;
			States = new Dictionary<IView, object>();
		}

		public IDataModel Model { get; private set; }
		public Dictionary<IView, object> States { get; private set; }
	}
}
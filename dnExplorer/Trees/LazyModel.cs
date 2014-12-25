using System;
using System.Collections.Generic;

namespace dnExplorer.Trees {
	public abstract class LazyModel : DataModel {
		protected abstract bool HasChildren { get; }
		protected abstract bool IsVolatile { get; }

		protected abstract IEnumerable<IDataModel> PopulateChildren();

		public override void OnCollapse() {
			if (IsVolatile && Children.Count > 0 && Children[0] != NullModel.Instance) {
				Children.Clear();
				Children.Add(NullModel.Instance);
			}
		}

		public override void OnExpand() {
			if (HasChildren && Children[0] == NullModel.Instance) {
				using (Children.BeginUpdate()) {
					Children.Clear();
					foreach (var child in PopulateChildren()) {
						Children.Add(child);
					}
				}
			}
		}

		protected virtual void Refresh() {
		}

		public override void Refresh(bool refreshChildren) {
			Refresh();
			if (refreshChildren) {
				OnCollapseRequested();
				Children.Clear();
				if (HasChildren) {
					Children.Add(NullModel.Instance);
				}
			}
		}
	}
}
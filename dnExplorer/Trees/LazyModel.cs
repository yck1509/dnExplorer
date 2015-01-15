using System;
using System.Collections.Generic;
using System.Drawing;

namespace dnExplorer.Trees {
	public abstract class LazyModel : DataModel {
		class Loading : DataModel {
			public Loading() {
				Text = "Loading... ";
			}

			public override bool HasIcon {
				get { return true; }
			}

			public override void DrawIcon(Graphics g, Rectangle bounds) {
				g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.loading.png"), bounds);
			}

			public override void Refresh(bool refreshChildren) {
			}
		}

		object sync = new object();
		ResponsiveOperation<ICollection<IDataModel>> loadOp;

		protected abstract bool HasChildren { get; }
		protected abstract bool IsVolatile { get; }

		public override void OnCollapse() {
			lock (sync) {
				if (loadOp != null)
					return;

				if (Children.Count > 0 && (
					(IsVolatile && Children[0] != NullModel.Instance) ||
					Children[0] is ErrorModel)) {
					Children.Clear();
					Children.Add(NullModel.Instance);
				}
			}
		}

		public override void OnExpand() {
			if (Children.Count > 0 && Children[0] == NullModel.Instance) {
				Load().Begin();
			}
		}

		public ResponsiveOperation<ICollection<IDataModel>> Load() {
			if (loadOp != null)
				return loadOp;

			lock (sync) {
				if (loadOp != null || (Children.Count == 0 || Children[0] != NullModel.Instance))
					return loadOp;
			}

			loadOp = new ResponsiveOperation<ICollection<IDataModel>>(PopulateChildrenInternal);
			loadOp.Loading += (sender, e) => {
				using (Children.BeginUpdate()) {
					Children.Clear();
					Children.Add(new Loading());
				}
			};
			loadOp.Completed += SetChildren;

			return loadOp;
		}

		ICollection<IDataModel> PopulateChildrenInternal() {
			ICollection<IDataModel> children;
			try {
				children = new List<IDataModel>(PopulateChildren());
			}
			catch (Exception ex) {
				children = new IDataModel[] {
					new ErrorModel(
						string.Format("Error while loading:{0}{1}{0}{0}",
							Environment.NewLine, ex))
				};
			}

			return children;
		}

		void SetChildren(object sender, OperationResultEventArgs<ICollection<IDataModel>> e) {
			lock (sync) {
				if (e.Result.Count > 0x200) {
					using (Children.BeginUpdate()) {
						Children.Clear();
						Children.Add(new Loading());
					}
				}
				using (Children.BeginUpdate()) {
					Children.Clear();
					foreach (var child in e.Result)
						Children.Add(child);
				}
				loadOp = null;
			}
		}

		protected abstract IEnumerable<IDataModel> PopulateChildren();

		protected virtual void Refresh() {
		}

		public override void Refresh(bool refreshChildren) {
			Refresh();
			if (refreshChildren) {
				OnCollapseRequested();

				lock (sync) {
					if (loadOp != null)
						loadOp.Cancel();
					Children.Clear();
					if (HasChildren) {
						Children.Add(NullModel.Instance);
					}
				}
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

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

		const int STATE_IDLE = 0;
		const int STATE_LOADING = 1;
		const int STATE_CANCEL = 2;
		const int STATE_COMPLETE = 3;
		const int LOADING_THRESHOLD = 200;

		object loadLock = new object();
		Task loadChildren;
		int loadState = STATE_IDLE;
		ManualResetEvent waitHnd = new ManualResetEvent(false);

		protected abstract bool HasChildren { get; }
		protected abstract bool IsVolatile { get; }

		public override void OnCollapse() {
			lock (loadLock) {
				if (loadState == STATE_LOADING)
					return;

				if (IsVolatile && Children.Count > 0 && Children[0] != NullModel.Instance) {
					Children.Clear();
					Children.Add(NullModel.Instance);
				}
			}
		}

		public override void OnExpand() {
			lock (loadLock) {
				if (loadState == STATE_LOADING)
					return;

				if (HasChildren && Children[0] == NullModel.Instance) {
					loadState = STATE_LOADING;
					waitHnd.Reset();

					loadChildren = Task.Factory.StartNew(PopulateChildrenInternal);
					if (!waitHnd.WaitOne(LOADING_THRESHOLD)) {
						using (Children.BeginUpdate()) {
							Children.Clear();
							Children.Add(new Loading());
						}
					}
				}
			}
		}

		void PopulateChildrenInternal() {
			var children = new List<IDataModel>(PopulateChildren());
			waitHnd.Set();
			lock (loadLock) {
				if (loadState == STATE_CANCEL)
					return;
				if (children.Count > 0x200) {
					using (Children.BeginUpdate()) {
						Children.Clear();
						Children.Add(new Loading());
					}
				}
				using (Children.BeginUpdate()) {
					Children.Clear();
					foreach (var child in children)
						Children.Add(child);
				}
				loadState = STATE_COMPLETE;
			}
		}

		protected abstract IEnumerable<IDataModel> PopulateChildren();

		protected virtual void Refresh() {
		}

		public override void Refresh(bool refreshChildren) {
			Refresh();
			if (refreshChildren) {
				OnCollapseRequested();
				lock (loadLock) {
					if (loadState == STATE_LOADING)
						loadState = STATE_CANCEL;

					Children.Clear();
					if (HasChildren) {
						Children.Add(NullModel.Instance);
					}
				}
			}
		}
	}
}
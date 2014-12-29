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
		const int LOADING_THRESHOLD = 100;

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

				if (Children.Count > 0 && (
					(IsVolatile && Children[0] != NullModel.Instance) ||
					Children[0] is ErrorModel)) {
					Children.Clear();
					Children.Add(NullModel.Instance);
				}
			}
		}

		public override void OnExpand() {
			LoadImmediate();
		}

		public Task LoadImmediate() {
			if (loadState == STATE_LOADING)
				return null;

			lock (loadLock) {
				if (loadState == STATE_LOADING)
					return null;
				if (!HasChildren || Children[0] != NullModel.Instance)
					return null;
				loadState = STATE_LOADING;
			}

			waitHnd.Reset();

			loadChildren = Task.Factory.StartNew(PopulateChildrenInternal);
			if (!waitHnd.WaitOne(LOADING_THRESHOLD)) {
				lock (loadLock) {
					using (Children.BeginUpdate()) {
						Children.Clear();
						Children.Add(new Loading());
					}
				}
			}
			return loadChildren;
		}

		void PopulateChildrenInternal() {
			ICollection<IDataModel> children;
			try {
				children = new List<IDataModel>(PopulateChildren());
			}
			catch (Exception ex) {
				children = new IDataModel[] {
					new ErrorModel(
						string.Format("Error while loading ModuleDef:{0}{1}{0}{0}",
							Environment.NewLine, ex))
				};
			}

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
			loadChildren = null;
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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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

				if (Children.Count > 0 && (
					(IsVolatile && Children[0] != NullModel.Instance) ||
					Children[0] is ErrorModel)) {
					Children.Clear();
					Children.Add(NullModel.Instance);
				}
			}
		}

		public override void OnExpand() {
			if (loadState == STATE_LOADING)
				return;

			lock (loadLock) {
				if (loadState == STATE_LOADING)
					return;
				if (!HasChildren || Children[0] != NullModel.Instance)
					return;
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
		}

		public void LoadImmediate() {
			// This method should runs on UI thread.
			if (Node != null && Node.TreeView != null && Node.TreeView.InvokeRequired)
				throw new InvalidOperationException();

			// I seldom uses DoEvents, but since DataTreeNodeX.OnChildrenUpdated
			// calls BeginInvoke, I need to use it to ensure all TreeNodes 
			// are created before returning.
			// (Invoke cannot be used, as it will deadlock. Well, not with 
			// DoEvents, but then I'd better use BeginInvoke)
			if (loadState == STATE_LOADING) {
				while (!waitHnd.WaitOne(LOADING_THRESHOLD))
					Application.DoEvents();
				while (loadState != STATE_COMPLETE)
					Application.DoEvents();
				return;
			}

			lock (loadLock) {
				if (loadState == STATE_LOADING)
					return;
				if (!HasChildren || Children[0] != NullModel.Instance)
					return;
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
				while (!waitHnd.WaitOne(LOADING_THRESHOLD))
					Application.DoEvents();
			}
			while (loadState != STATE_COMPLETE)
				Application.DoEvents();
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
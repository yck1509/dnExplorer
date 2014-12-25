using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace dnExplorer.Trees {
	public class BatchObservableCollection<T> : ObservableCollection<T> {
		class Updater : IDisposable {
			BatchObservableCollection<T> collection;

			public Updater(BatchObservableCollection<T> collection) {
				this.collection = collection;
			}

			public void Dispose() {
				collection.EndUpdate();
			}
		}

		bool updating;

		public IDisposable BeginUpdate() {
			updating = true;
			return new Updater(this);
		}

		void EndUpdate() {
			updating = false;
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}


		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
			if (updating)
				return;
			base.OnCollectionChanged(e);
		}
	}
}
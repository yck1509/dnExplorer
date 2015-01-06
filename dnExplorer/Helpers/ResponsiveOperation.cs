using System;
using System.Threading;
using System.Threading.Tasks;

namespace dnExplorer {
	public class ResponsiveOperation<T> {
		const int STATE_IDLE = 0;
		const int STATE_LOADING = 1;
		const int STATE_CANCEL = 2;
		const int STATE_COMPLETE = 3;
		const int LOADING_THRESHOLD = 250;

		Func<T> operation;
		object sync = new object();
		int state = STATE_IDLE;
		Task<T> task;
		ManualResetEvent waitHnd = new ManualResetEvent(false);

		public event Action<T> Completed;
		public event Action Loading;

		public ResponsiveOperation(Func<T> operation) {
			this.operation = operation;
		}

		public void Begin() {
			if (state == STATE_LOADING)
				return;

			lock (sync) {
				if (state == STATE_LOADING)
					return;
				state = STATE_LOADING;
			}

			waitHnd.Reset();

			task = Task.Factory.StartNew<T>(DoOperation);

			if (!waitHnd.WaitOne(LOADING_THRESHOLD)) {
				lock (sync) {
					if (!waitHnd.WaitOne(0)) {
						if (Loading != null)
							Loading();
						task.ContinueWith(OnCompleted, TaskScheduler.FromCurrentSynchronizationContext());
						return;
					}
				}
				OnCompleted(task);
			}
			else
				OnCompleted(task);
		}

		public void Cancel() {
			lock (sync) {
				if (state == STATE_LOADING)
					state = STATE_CANCEL;
			}
		}

		T DoOperation() {
			var result = operation();

			lock (sync) {
				waitHnd.Set();
			}
			return result;
		}

		void OnCompleted(Task<T> t) {
			lock (sync) {
				if (state == STATE_CANCEL)
					return;

				task = null;
				while (!t.IsCompleted)
					Thread.Sleep(10);

				if (Completed != null)
					Completed(t.Result);

				state = STATE_COMPLETE;
			}
		}
	}
}
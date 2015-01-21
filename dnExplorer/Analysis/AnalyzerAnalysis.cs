using System;
using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet;

namespace dnExplorer.Analysis {
	public abstract class AnalyzerAnalysis<T> : IAnalysis where T : IFullName {
		public bool HasResult {
			get { return true; }
		}

		protected AnalyzerAnalysis(T item) {
			Item = item;
		}

		protected T Item { get; private set; }

		public IFullName TargetObject {
			get { return Item; }
		}

		public IEnumerable<IAnalysis> GetChildAnalyses(object child) {
			return Analyzer.GetChildren(child);
		}

		public abstract string Name { get; }

		public abstract IEnumerable<object> Run(IApp app, CancellationToken token);
	}
}
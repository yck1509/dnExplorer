using System;
using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet;

namespace dnExplorer.Analysis {
	public interface IAnalysis {
		string Name { get; }
		bool HasResult { get; }
		IFullName TargetObject { get; }

		IEnumerable<object> Run(IApp app, CancellationToken token);
		IEnumerable<IAnalysis> GetChildAnalyses(object child);
	}
}
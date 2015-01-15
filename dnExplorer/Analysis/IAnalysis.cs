using System;
using System.Collections.Generic;
using dnlib.DotNet;

namespace dnExplorer.Analysis {
	public interface IAnalysis {
		string Name { get; }
		bool HasResult { get; }
		IFullName TargetObject { get; }

		IEnumerable<object> Run();
		IAnalysis GetChildAnalysis(object child);
	}
}
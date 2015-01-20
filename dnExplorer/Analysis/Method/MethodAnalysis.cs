using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using dnExplorer.Models;
using dnExplorer.Trees;
using dnlib.DotNet;

namespace dnExplorer.Analysis {
	public abstract class MethodAnalysis : IAnalysis {
		public bool HasResult {
			get { return true; }
		}

		public abstract IFullName TargetObject { get; }

		public IEnumerable<IAnalysis> GetChildAnalyses(object child) {
			if (child is IMethod)
				return GetChildren(((IMethod)child).ResolveMethodDef());
			return Enumerable.Empty<IAnalysis>();
		}

		static IAnalysis[] GetChildren(MethodDef item) {
			return Analyzer.GetChildren(item);
		}

		public static IDataModel CreateAnalysis(MethodDef method) {
			return new MultipleAnalysesModel(method, GetChildren(method));
		}

		public abstract string Name { get; }

		public abstract IEnumerable<object> Run(IApp app, CancellationToken token);
	}
}
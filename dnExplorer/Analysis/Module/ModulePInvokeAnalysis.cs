using System;
using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet;

namespace dnExplorer.Analysis {
	public class ModulePInvokeAnalysis : AnalyzerAnalysis<ModuleDefMD> {
		public ModulePInvokeAnalysis(ModuleDefMD targetModule)
			: base(targetModule) {
		}

		public override string Name {
			get { return "P-Invokes"; }
		}

		public override IEnumerable<object> Run(IApp app, CancellationToken token) {
			foreach (var typeDef in Item.GetTypes()) {
				foreach (var method in typeDef.Methods) {
					token.ThrowIfCancellationRequested();

					if (method.IsPinvokeImpl)
						yield return method;
				}
			}
		}
	}
}
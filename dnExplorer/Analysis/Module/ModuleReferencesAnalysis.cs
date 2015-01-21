using System;
using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet;

namespace dnExplorer.Analysis {
	public class ModuleReferencesAnalysis : AnalyzerAnalysis<ModuleDefMD> {
		public ModuleReferencesAnalysis(ModuleDefMD targetModule)
			: base(targetModule) {
		}

		public override string Name {
			get { return "References"; }
		}

		public override IEnumerable<object> Run(IApp app, CancellationToken token) {
			var result = new HashSet<object>();

			foreach (var asmRef in Item.GetAssemblyRefs()) {
				var asmDef = app.Modules.Resolver.Resolve(asmRef, Item);
				if (asmDef != null)
					result.Add(asmDef);
				else
					result.Add(new AnalysisError(asmDef.FullName, asmDef));
			}

			foreach (var modRef in Item.GetModuleRefs()) {
				result.Add(modRef);
			}

			return result;
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using dnlib.DotNet;

namespace dnExplorer.Analysis {
	public class DerivedTypesAnalysis : IAnalysis {
		public string Name {
			get { return "Derived Types"; }
		}

		ITypeDefOrRef type;
		TypeDef typeDef;

		public DerivedTypesAnalysis(ITypeDefOrRef type) {
			this.type = type;
			typeDef = type.ResolveTypeDef();
		}

		public IFullName TargetObject {
			get { return typeDef ?? type; }
		}

		public bool HasResult {
			get { return !typeDef.IsSealed; }
		}

		public IEnumerable<object> Run(IApp app, CancellationToken token) {
			if (typeDef == null) {
				yield return new AnalysisError("Failed to resolve '" + type.FullName + "'.");
				yield break;
			}

			var comparer = new SigComparer();
			if (typeDef.IsInterface) {
				foreach (var type in app.Modules.GetModules().SelectMany(module => module.GetTypes())) {
					token.ThrowIfCancellationRequested();

					if (!type.HasInterfaces)
						continue;
					foreach (var iface in type.Interfaces) {
						if (comparer.Equals(iface.Interface, typeDef))
							yield return type;
					}
				}
			}
			else {
				foreach (var type in app.Modules.GetModules().SelectMany(module => module.GetTypes())) {
					token.ThrowIfCancellationRequested();

					if (comparer.Equals(type.BaseType, typeDef))
						yield return type;
				}
			}
		}

		public IEnumerable<IAnalysis> GetChildAnalyses(object child) {
			if (child is ITypeDefOrRef)
				yield return new DerivedTypesAnalysis((ITypeDefOrRef)child);
		}
	}
}
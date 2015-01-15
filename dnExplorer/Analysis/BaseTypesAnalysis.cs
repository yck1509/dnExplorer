using System;
using System.Collections.Generic;
using dnlib.DotNet;

namespace dnExplorer.Analysis {
	public class BaseTypesAnalysis : IAnalysis {
		public string Name {
			get { return "Base Types"; }
		}

		ITypeDefOrRef type;
		TypeDef typeDef;

		public BaseTypesAnalysis(ITypeDefOrRef type) {
			this.type = type;
			typeDef = type.ResolveTypeDef();
		}

		public IFullName TargetObject {
			get { return typeDef ?? type; }
		}

		public bool HasResult {
			get {
				if (typeDef == null)
					return true;
				return typeDef.BaseType != null || typeDef.HasInterfaces;
			}
		}

		public IEnumerable<object> Run(IApp app) {
			if (typeDef == null) {
				yield return new AnalysisError("Failed to resolve '" + type.FullName + "'.");
			}
			else {
				if (typeDef.BaseType != null)
					yield return typeDef.BaseType;
				foreach (var impl in typeDef.Interfaces)
					yield return impl.Interface;
			}
		}

		public IAnalysis GetChildAnalysis(object child) {
			if (child is ITypeDefOrRef)
				return new BaseTypesAnalysis((ITypeDefOrRef)child);
			return null;
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
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

		public IEnumerable<object> Run(IApp app) {
			if (typeDef == null) {
				yield return new AnalysisError("Failed to resolve '" + type.FullName + "'.");
				yield break;
			}

			var comparer = new SigComparer();
			if (typeDef.IsInterface) {
				foreach (var type in app.Modules.GetModules().SelectMany(module => module.GetTypes())) {
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
					if (comparer.Equals(type.BaseType, typeDef))
						yield return type;
				}
			}
		}

		bool IsDerivedTypeInterface(HashSet<TypeDef> derivedTypes, TypeDef type, RecursionCounter counter) {
			if (!counter.Increment())
				return false;

			if (type.HasInterfaces) {
				foreach (var iface in type.Interfaces)
					if (new SigComparer().Equals(this.type, iface.Interface)) {
						derivedTypes.Add(type);

						counter.Decrement();
						return true;
					}
			}

			if (type.BaseType != null) {
				var baseType = type.BaseType.ResolveTypeDef();
				if (baseType != null) {
					if (derivedTypes.Contains(baseType) || IsDerivedTypeInterface(derivedTypes, baseType, counter)) {
						derivedTypes.Add(type);

						counter.Decrement();
						return true;
					}
				}
			}

			counter.Decrement();
			return false;
		}

		bool IsDerivedType(HashSet<TypeDef> derivedTypes, TypeDef type, RecursionCounter counter) {
			if (!counter.Increment())
				return false;

			if (type.BaseType != null) {
				if (new SigComparer().Equals(this.type, type.BaseType)) {
					derivedTypes.Add(type);

					counter.Decrement();
					return true;
				}
				var baseType = type.BaseType.ResolveTypeDef();
				if (baseType != null) {
					if (derivedTypes.Contains(baseType) || IsDerivedType(derivedTypes, baseType, counter)) {
						derivedTypes.Add(type);

						counter.Decrement();
						return true;
					}
				}
			}
			counter.Decrement();
			return false;
		}

		public IAnalysis GetChildAnalysis(object child) {
			if (child is ITypeDefOrRef)
				return new DerivedTypesAnalysis((ITypeDefOrRef)child);
			return null;
		}
	}
}
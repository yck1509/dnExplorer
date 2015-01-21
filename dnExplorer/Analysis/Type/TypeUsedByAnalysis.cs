using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using dnlib.DotNet;

namespace dnExplorer.Analysis {
	public class TypeUsedByAnalysis : AnalyzerAnalysis<TypeDef> {
		public TypeUsedByAnalysis(TypeDef targetType)
			: base(targetType) {
		}

		public override string Name {
			get { return "Used By"; }
		}

		public override IEnumerable<object> Run(IApp app, CancellationToken token) {
			return new ScopedAnalyzer<IMemberDef>(app, Item, AnalyzeType).Run(token);
		}

		IEnumerable<IMemberDef> AnalyzeType(TypeDef type) {
			var comparer = new SigComparer(SigComparerOptions.CompareMethodFieldDeclaringType);

			if (comparer.Equals(type.BaseType, Item))
				yield return type;
			else if (type.Interfaces.Any(iface => comparer.Equals(iface.Interface, Item)))
				yield return type;

			foreach (var method in type.Methods) {
				if (method.Parameters.Any(param => comparer.Equals(param.Type, Item))) {
					yield return method;
					continue;
				}
				if (comparer.Equals(method.ReturnType, Item)) {
					yield return method;
					continue;
				}
				if (!method.HasBody)
					continue;

				if (method.Body.Variables.Any(var => comparer.Equals(var.Type, Item))) {
					yield return method;
					continue;
				}

				foreach (var instr in method.Body.Instructions)
					if (instr.Operand is ITypeDefOrRef && comparer.Equals((ITypeDefOrRef)instr.Operand, Item)) {
						yield return method;
						break;
					}
					else if (instr.Operand is IMemberRef && comparer.Equals(((IMemberRef)instr.Operand).DeclaringType, Item)) {
						yield return method;
						break;
					}
			}
		}
	}
}
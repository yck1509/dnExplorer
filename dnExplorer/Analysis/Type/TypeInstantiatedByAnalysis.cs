using System;
using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace dnExplorer.Analysis {
	public class TypeInstantiatedByAnalysis : AnalyzerAnalysis<TypeDef> {
		public TypeInstantiatedByAnalysis(TypeDef targetType)
			: base(targetType) {
		}

		public override string Name {
			get { return "Instantiated By"; }
		}

		public override IEnumerable<object> Run(IApp app, CancellationToken token) {
			return new ScopedAnalyzer<IMemberDef>(app, Item, AnalyzeType).Run(token);
		}

		IEnumerable<IMemberDef> AnalyzeType(TypeDef type) {
			var comparer = new SigComparer(SigComparerOptions.CompareMethodFieldDeclaringType);

			foreach (var method in type.Methods) {
				if (!method.HasBody)
					continue;

				foreach (var instr in method.Body.Instructions)
					if (instr.OpCode.Code == Code.Newobj &&
					    comparer.Equals(((IMethod)instr.Operand).DeclaringType.ScopeType, Item)) {
						yield return method;
						break;
					}
			}
		}
	}
}
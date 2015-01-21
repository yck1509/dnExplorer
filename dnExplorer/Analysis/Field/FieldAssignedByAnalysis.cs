using System;
using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace dnExplorer.Analysis {
	public class FieldAssignedByAnalysis : AnalyzerAnalysis<FieldDef> {
		public FieldAssignedByAnalysis(FieldDef targetField)
			: base(targetField) {
		}

		public override string Name {
			get { return "Assigned By"; }
		}

		public override IEnumerable<object> Run(IApp app, CancellationToken token) {
			return new ScopedAnalyzer<MethodDef>(app, Item, AnalyzeType).Run(token);
		}

		IEnumerable<MethodDef> AnalyzeType(TypeDef type) {
			var comparer = new SigComparer(SigComparerOptions.CompareMethodFieldDeclaringType);
			foreach (var method in type.Methods) {
				if (!method.HasBody)
					continue;

				foreach (var instr in method.Body.Instructions) {
					switch (instr.OpCode.Code) {
						case Code.Stfld:
						case Code.Stsfld:
						case Code.Ldflda: // Taking address of field -> potentially assign indirectly
						case Code.Ldsflda:
							break;

						default:
							continue;
					}
					if (instr.Operand is IField && comparer.Equals((IField)instr.Operand, Item)) {
						yield return method;
						break;
					}
				}
			}
		}
	}
}
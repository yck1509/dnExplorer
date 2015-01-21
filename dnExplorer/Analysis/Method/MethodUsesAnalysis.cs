using System;
using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet;

namespace dnExplorer.Analysis {
	public class MethodUsesAnalysis : AnalyzerAnalysis<MethodDef> {
		public MethodUsesAnalysis(MethodDef targetMethod)
			: base(targetMethod) {
		}

		public override string Name {
			get { return "Uses"; }
		}

		public override IEnumerable<object> Run(IApp app, CancellationToken token) {
			var result = new HashSet<object>();
			if (!Item.HasBody)
				return result;

			foreach (var instr in Item.Body.Instructions) {
				if (instr.Operand is IMethod) {
					var def = ((IMethod)instr.Operand).ResolveMethodDef();
					if (def == null)
						result.Add(instr.Operand);
					else
						result.Add(def);
				}
				else if (instr.Operand is IField) {
					var def = ((IField)instr.Operand).ResolveFieldDef();
					if (def == null)
						result.Add(instr.Operand);
					else
						result.Add(def);
				}
				else if (instr.Operand is ITypeDefOrRef) {
					var def = ((ITypeDefOrRef)instr.Operand).ResolveTypeDef();
					if (def == null)
						result.Add(instr.Operand);
					else
						result.Add(def);
				}
			}
			return result;
		}
	}
}
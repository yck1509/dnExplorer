using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using dnlib.DotNet;

namespace dnExplorer.Analysis {
	public class ScopedAnalyzer<T> {
		AccessChecker checker;
		ModuleDefMD[] scopeModules;
		object targetItem;
		Func<TypeDef, IEnumerable<T>> analyzeFunc;

		public ScopedAnalyzer(IApp app, object targetItem, Func<TypeDef, IEnumerable<T>> analyzeFunc) {
			scopeModules = GetModules(app, targetItem).ToArray();
			this.targetItem = targetItem;
			this.analyzeFunc = analyzeFunc;
		}

		IEnumerable<ModuleDefMD> GetModules(IApp app, object targetItem) {
			var type = targetItem as TypeDef;
			if (type == null)
				type = ((IMemberDef)targetItem).DeclaringType;

			var asmComparer = new AssemblyNameComparer(AssemblyNameComparerFlags.All);
			foreach (var module in app.Modules.GetModules()) {
				if (module.GlobalType == null)
					continue;

				bool hasRef = asmComparer.Equals(module.Assembly, type.DefinitionAssembly);
				if (!hasRef) {
					foreach (var reference in module.GetAssemblyRefs())
						if (asmComparer.Equals(reference, type.DefinitionAssembly)) {
							hasRef = true;
							break;
						}
				}
				if (!hasRef)
					continue;

				if (type.Module == module)
					yield return module;
				else {
					checker.UserType = module.GlobalType;
					if (checker.CanAccess(type) ?? true)
						yield return module;
				}
			}
		}

		public IEnumerable<T> Run(CancellationToken token) {
			foreach (var modules in scopeModules) {
				foreach (var type in modules.GetTypes()) {
					token.ThrowIfCancellationRequested();

					checker.UserType = type;
					if (!(checker.CanAccess(targetItem) ?? true))
						continue;

					foreach (var result in analyzeFunc(type))
						yield return result;
				}
			}
		}
	}
}
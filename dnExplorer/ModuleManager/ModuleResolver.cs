using System;
using System.Collections.Generic;
using System.Diagnostics;
using dnlib.DotNet;

namespace dnExplorer {
	internal class ModuleResolver : IAssemblyResolver {
		public ModuleResolver(ModuleManager manager) {
			Manager = manager;
			Modules = new HashSet<ModuleDefMD>();
			NetModules = new Dictionary<UTF8String, ModuleDefMD>();
			LoadedAssemblies =
				new Dictionary<AssemblyNameInfo, AssemblyDef>(new AssemblyNameComparer(AssemblyNameComparerFlags.All));
		}

		object sync = new object();
		AssemblyResolver defaultResolver;
		public ModuleManager Manager { get; private set; }
		public AssemblyResolver Resolver { get; set; }
		public HashSet<ModuleDefMD> Modules { get; private set; }
		public Dictionary<UTF8String, ModuleDefMD> NetModules { get; private set; }
		public Dictionary<AssemblyNameInfo, AssemblyDef> LoadedAssemblies { get; private set; }

		public AssemblyResolver CreateResolver() {
			var resolver = new AssemblyResolver(new ModuleContext(this));
			foreach (var assembly in LoadedAssemblies.Values)
				resolver.AddToCache(assembly);
			return resolver;
		}

		AssemblyResolver GetDefaultResolver() {
			lock (sync) {
				if (defaultResolver == null)
					defaultResolver = new AssemblyResolver(new ModuleContext(this));
				foreach (var asm in new List<AssemblyDef>(defaultResolver.GetCachedAssemblies()))
					defaultResolver.Remove(asm);
				foreach (var assembly in LoadedAssemblies.Values)
					defaultResolver.AddToCache(assembly);
				return defaultResolver;
			}
		}

		public bool AddModuleDef(ModuleDefMD module, out ModuleDefMD cached) {
			module.EnableTypeDefFindCache = true;
			if (module.Assembly == null) {
				foreach (var assembly in LoadedAssemblies) {
					foreach (var m in assembly.Value.Modules)
						if (m.Name == module.Name) {
							cached = (ModuleDefMD)m;
							return false;
						}

					var asmModule = (ModuleDefMD)assembly.Value.ManifestModule;
					var rows = asmModule.TablesStream.FileTable.Rows;
					for (uint i = 1; i <= rows; i++) {
						var file = asmModule.TablesStream.ReadFileRow(i);
						var name = asmModule.StringsStream.ReadNoNull(file.Name);
						if (name == module.Name) {
							assembly.Value.Modules.Add(module);
							Modules.Add(module);
							cached = null;
							return true;
						}
					}
				}
				cached = null;
				if (NetModules.ContainsKey(module.Name)) {
					return false;
				}
				NetModules.Add(module.Name, module);
				Modules.Add(module);
				return true;
			}
			var asmName = new AssemblyNameInfo(module.Assembly);
			AssemblyDef cachedAsm;
			if (LoadedAssemblies.TryGetValue(asmName, out cachedAsm)) {
				cached = (ModuleDefMD)cachedAsm.ManifestModule;
				return false;
			}
			LoadedAssemblies[asmName] = module.Assembly;
			Modules.Add(module);
			cached = null;
			return true;
		}

		public bool RemoveModuleDef(ModuleDefMD module) {
			if (!NetModules.Remove(module.Name)) {
				foreach (var assembly in LoadedAssemblies.Values)
					foreach (var asmModule in assembly.Modules)
						if (asmModule == module) {
							if (assembly.Modules.Count == 1)
								LoadedAssemblies.Remove(new AssemblyNameInfo(assembly));
							else
								assembly.Modules.Remove(asmModule);
							Modules.Remove(module);
							return true;
						}
				return false;
			}
			Modules.Remove(module);
			return true;
		}

		void ResolveNetModules(AssemblyDef assembly) {
			while (assembly.Modules.Count > 1)
				assembly.Modules.RemoveAt(1);

			var asmModule = (ModuleDefMD)assembly.ManifestModule;
			var rows = asmModule.TablesStream.FileTable.Rows;
			for (uint i = 1; i <= rows; i++) {
				var file = asmModule.TablesStream.ReadFileRow(i);
				var name = asmModule.StringsStream.ReadNoNull(file.Name);
				ModuleDefMD loneModule;
				if (NetModules.TryGetValue(name, out loneModule)) {
					assembly.Modules.Add(loneModule);
					NetModules.Remove(name);
				}
			}
		}

		public AssemblyDef Resolve(IAssembly assembly, ModuleDef sourceModule) {
			var resolver = Resolver;
			if (resolver == null)
				resolver = GetDefaultResolver();

			AssemblyDef result = resolver.Resolve(assembly, sourceModule);

			var comparer = new AssemblyNameComparer(AssemblyNameComparerFlags.All);
			AssemblyDef cachedResult;
			if (!LoadedAssemblies.TryGetValue(new AssemblyNameInfo(result), out cachedResult)) {
				Debug.Assert(!string.IsNullOrEmpty(result.ManifestModule.Location));
				Manager.LoadModule(result.ManifestModule.Location).Wait();
				cachedResult = LoadedAssemblies[new AssemblyNameInfo(result)];
				ResolveNetModules(cachedResult);
			}
			return cachedResult;
		}

		public bool AddToCache(AssemblyDef asm) {
			return false;
		}

		public bool Remove(AssemblyDef asm) {
			return false;
		}

		public void Clear() {
		}
	}
}
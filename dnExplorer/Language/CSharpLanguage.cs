using System;
using System.Threading;
using dnExplorer.Controls;
using dnlib.DotNet;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;

namespace dnExplorer.Language {
	public class CSharpLanguage : ILanguage {
		public string Name {
			get { return "C#"; }
		}

		AstBuilder CreateBuilder(IDnlibDef item, CancellationToken token) {
			ModuleDef moduleDef;

			DecompilerContext ctx;
			AstBuilder builder;

			if (item is ModuleDef) {
				var def = (ModuleDef)item;
				moduleDef = def;
				builder = new AstBuilder(ctx = new DecompilerContext(moduleDef) { CancellationToken = token });
				builder.AddAssembly(def, true);
			}
			else if (item is TypeDef) {
				var def = (TypeDef)item;
				moduleDef = def.Module;
				builder = new AstBuilder(ctx = new DecompilerContext(moduleDef) { CancellationToken = token });
				builder.DecompileMethodBodies = false;
				ctx.CurrentType = def;
				builder.AddType(def);
			}
			else if (item is MethodDef) {
				var def = (MethodDef)item;
				moduleDef = def.Module;
				builder = new AstBuilder(ctx = new DecompilerContext(moduleDef) { CancellationToken = token });
				ctx.CurrentType = def.DeclaringType;
				builder.AddMethod(def);
			}
			else if (item is FieldDef) {
				var def = (FieldDef)item;
				moduleDef = def.Module;
				builder = new AstBuilder(ctx = new DecompilerContext(moduleDef) { CancellationToken = token });
				ctx.CurrentType = def.DeclaringType;
				builder.AddField(def);
			}
			else if (item is PropertyDef) {
				var def = (PropertyDef)item;
				moduleDef = def.Module;
				builder = new AstBuilder(ctx = new DecompilerContext(moduleDef) { CancellationToken = token });
				ctx.CurrentType = def.DeclaringType;
				builder.AddProperty(def);
			}
			else if (item is EventDef) {
				var def = (EventDef)item;
				moduleDef = def.Module;
				builder = new AstBuilder(ctx = new DecompilerContext(moduleDef) { CancellationToken = token });
				ctx.CurrentType = def.DeclaringType;
				builder.AddEvent(def);
			}
			else
				return null;

			ctx.Settings = new DecompilerSettings {
				UsingDeclarations = false
			};
			return builder;
		}

		public CodeViewData Run(IDnlibDef item, CancellationToken token) {
			var builder = CreateBuilder(item, token);
			if (builder == null)
				return new CodeViewData("");
			builder.RunTransformations();
			var output = new CodeViewOutput();
			builder.GenerateCode(output);
			return output.GetResult();
		}
	}
}
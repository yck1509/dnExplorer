using System;
using System.Drawing;
using dnExplorer.Controls;
using dnExplorer.Models;
using dnlib.DotNet;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;

namespace dnExplorer.Views {
	public class CSharpView : ViewBase<ObjModel> {
		CodeView view;

		public CSharpView() {
			view = new CodeView();
			Controls.Add(view);
			Text = "C#";
		}

		object sync = new object();
		ResponsiveOperation<CodeViewData> op;

		protected override void OnModelUpdated() {
			if (op != null)
				op.Cancel();

			if (Model == null) {
				view.Clear();
			}
			else {
				op = new ResponsiveOperation<CodeViewData>(RunDecompilation);
				op.Completed += OnCompleted;
				op.Loading += OnLoading;
				op.Begin();
			}
		}

		AstBuilder CreateBuilder() {
			ModuleDef moduleDef;

			DecompilerContext ctx;
			AstBuilder builder;

			if (Model.Definition is ModuleDef) {
				var def = (ModuleDef)Model.Definition;
				moduleDef = def;
				builder = new AstBuilder(ctx = new DecompilerContext(moduleDef));
				builder.AddAssembly(def, true);
			}
			else if (Model.Definition is TypeDef) {
				var def = (TypeDef)Model.Definition;
				moduleDef = def.Module;
				builder = new AstBuilder(ctx = new DecompilerContext(moduleDef));
				builder.DecompileMethodBodies = false;
				ctx.CurrentType = def;
				builder.AddType(def);
			}
			else if (Model.Definition is MethodDef) {
				var def = (MethodDef)Model.Definition;
				moduleDef = def.Module;
				builder = new AstBuilder(ctx = new DecompilerContext(moduleDef));
				ctx.CurrentType = def.DeclaringType;
				builder.AddMethod(def);
			}
			else if (Model.Definition is FieldDef) {
				var def = (FieldDef)Model.Definition;
				moduleDef = def.Module;
				builder = new AstBuilder(ctx = new DecompilerContext(moduleDef));
				ctx.CurrentType = def.DeclaringType;
				builder.AddField(def);
			}
			else if (Model.Definition is PropertyDef) {
				var def = (PropertyDef)Model.Definition;
				moduleDef = def.Module;
				builder = new AstBuilder(ctx = new DecompilerContext(moduleDef));
				ctx.CurrentType = def.DeclaringType;
				builder.AddProperty(def);
			}
			else if (Model.Definition is EventDef) {
				var def = (EventDef)Model.Definition;
				moduleDef = def.Module;
				builder = new AstBuilder(ctx = new DecompilerContext(moduleDef));
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

		CodeViewData RunDecompilation() {
			try {
				var builder = CreateBuilder();
				if (builder == null)
					return new CodeViewData("");
				builder.RunTransformations();
				var output = new CodeViewOutput();
				builder.GenerateCode(output);
				return output.GetResult();
			}
			catch (Exception ex) {
				return new CodeViewData(string.Format("Error occured in decompilation:{0}{1}", Environment.NewLine, ex));
			}
		}

		void OnLoading(object sender, EventArgs e) {
			view.SetPlainText("Decompiling...");
		}

		void OnCompleted(object sender, OperationResultEventArgs<CodeViewData> e) {
			view.SetData(e.Result);
			op = null;
		}

		public override Icon Icon {
			get { return IconCreator.CreateIcon((Bitmap)Resources.GetResource<Image>("Icons.code.png"), 16); }
		}
	}
}
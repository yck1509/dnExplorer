using System;
using dnExplorer.Controls;
using dnExplorer.Models;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;

namespace dnExplorer.Views {
	public class MethodView : ViewBase<MethodModel> {
		CodeView view;

		public MethodView() {
			view = new CodeView();
			Controls.Add(view);
		}

		protected override void OnModelUpdated() {
			if (Model == null) {
				view.Text = "";
			}
			else {
				AstBuilder astBuilder = new AstBuilder(new DecompilerContext(Model.Method.Module) {
					CurrentType = Model.Method.DeclaringType,
					Settings = new DecompilerSettings {
						UsingDeclarations = false
					}
				});
				astBuilder.AddMethod(Model.Method);
				astBuilder.RunTransformations();
				var output = new CodeViewOutput();
				astBuilder.GenerateCode(output);
				view.SetData(output.GetResult());
			}
		}
	}
}
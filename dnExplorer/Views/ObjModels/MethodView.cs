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

		CodeViewData RunDecompilation() {
			try {
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
	}
}
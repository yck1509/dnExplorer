using System;
using System.Windows.Forms;
using dnExplorer.Models;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;
using ScintillaNET;

namespace dnExplorer.Views {
	public class MethodView : ViewBase<MethodModel> {
		Scintilla view;

		public MethodView() {
			view = new Scintilla {
				Dock = DockStyle.Fill
			};
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
				var output = new PlainTextOutput();
				astBuilder.GenerateCode(output);
				view.Text = output.ToString();
			}
		}
	}
}
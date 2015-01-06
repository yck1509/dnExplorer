using System;
using System.Threading;
using dnExplorer.Controls;
using dnExplorer.Models;
using ICSharpCode.Decompiler.Disassembler;

namespace dnExplorer.Views {
	public class MethodILView : ViewBase<MethodModel> {
		CodeView view;

		public MethodILView() {
			view = new CodeView();
			Controls.Add(view);
			Text = "IL";
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
				op = new ResponsiveOperation<CodeViewData>(RunDisassembler);
				op.Completed += OnCompleted;
				op.Loading += OnLoading;
				op.Begin();
			}
		}

		CodeViewData RunDisassembler() {
			try {
				var output = new CodeViewOutput();
				var disassembler = new ReflectionDisassembler(output, true, CancellationToken.None);
				disassembler.DisassembleMethod(Model.Method);
				return output.GetResult();
			}
			catch (Exception ex) {
				return new CodeViewData(string.Format("Error occured in disassembling:{0}{1}", Environment.NewLine, ex));
			}
		}

		void OnLoading(object sender, EventArgs e) {
			view.SetPlainText("Disassembling...");
		}

		void OnCompleted(object sender, OperationResultEventArgs<CodeViewData> e) {
			view.SetData(e.Result);
			op = null;
		}
	}
}
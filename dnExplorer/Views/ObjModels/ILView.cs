using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using dnExplorer.Controls;
using dnExplorer.Models;
using dnlib.DotNet;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Disassembler;

namespace dnExplorer.Views {
	public class ILView : ViewBase<ObjModel> {
		CodeView view;

		public ILView() {
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

		void DoDisassemble(ITextOutput output, ReflectionDisassembler disassembler) {
			if (Model.Definition is ModuleDef) {
				var module = (ModuleDef)Model.Definition;
				disassembler.WriteAssemblyReferences(module);
				if (module.Assembly != null)
					disassembler.WriteAssemblyHeader(module.Assembly);
				output.WriteLine();
				disassembler.WriteModuleHeader(module);
			}
			else if (Model.Definition is TypeDef) {
				disassembler.DisassembleType((TypeDef)Model.Definition);
			}
			else if (Model.Definition is MethodDef) {
				disassembler.DisassembleMethod((MethodDef)Model.Definition);
			}
			else if (Model.Definition is FieldDef) {
				disassembler.DisassembleField((FieldDef)Model.Definition);
			}
			else if (Model.Definition is PropertyDef) {
				disassembler.DisassembleProperty((PropertyDef)Model.Definition);
			}
			else if (Model.Definition is EventDef) {
				disassembler.DisassembleEvent((EventDef)Model.Definition);
			}
		}

		CodeViewData RunDisassembler() {
			try {
				var output = new CodeViewOutput();
				var disassembler = new ReflectionDisassembler(output, true, CancellationToken.None);
				DoDisassemble(output, disassembler);
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

		static ContextMenuStrip ctxMenu;

		protected internal override ContextMenuStrip GetContextMenu() {
			if (ctxMenu != null)
				return ctxMenu;

			ctxMenu = new ContextMenuStrip();

			var gotoMD = new ToolStripMenuItem("Go To MetaData View");
			gotoMD.Click += GotoMD;
			ctxMenu.Items.Add(gotoMD);

			return ctxMenu;
		}

		static void GotoMD(object sender, EventArgs e) {
			var model = sender.GetContextMenuModel<ObjModel>();
			var module = model.Definition as ModuleDefMD;
			if (module == null)
				module = (ModuleDefMD)((IMemberDef)model.Definition).Module;
			ViewUtils.ShowToken(model, module.MetaData.PEImage, model.Definition.MDToken);
		}

		public override Icon Icon {
			get { return IconCreator.CreateIcon((Bitmap)Resources.GetResource<Image>("Icons.code.png"), 16); }
		}
	}
}
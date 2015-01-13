using System;
using System.Threading;
using System.Windows.Forms;
using dnExplorer.Controls;
using dnExplorer.Language;
using dnExplorer.Models;
using dnlib.DotNet;

namespace dnExplorer.Views {
	public class ObjCodeView : ViewBase<ObjModel> {
		CodeView view;

		public ObjCodeView() {
			view = new CodeView();
			Controls.Add(view);
		}

		object sync = new object();
		CancellationTokenSource cancellation;
		ResponsiveOperation<CodeViewData> op;

		protected override void OnModelUpdated() {
			InitApp();
			GenerateCode();
		}

		bool appInited;

		void InitApp() {
			if (appInited)
				return;

			App.Language.PropertyChanged += (sender, e) => GenerateCode();
			appInited = true;
		}

		void GenerateCode() {
			if (InvokeRequired) {
				Invoke(new Action(GenerateCode));
				return;
			}

			if (op != null) {
				op.Cancel();
				cancellation.Cancel();
			}

			if (Model == null) {
				view.Clear();
			}
			else {
				cancellation = new CancellationTokenSource();
				var state = new RunState(Model.Definition, App.Language.ActiveLanguage, cancellation.Token);
				op = new ResponsiveOperation<CodeViewData>(state.Run);
				op.LoadingThreshold = 50;
				op.Completed += OnCompleted;
				op.Loading += OnLoading;
				op.Begin();
			}
		}

		struct RunState {
			IDnlibDef item;
			ILanguage lang;
			CancellationToken token;

			public RunState(IDnlibDef item, ILanguage lang, CancellationToken token) {
				this.item = item;
				this.lang = lang;
				this.token = token;
			}

			public CodeViewData Run() {
				try {
					return lang.Run(item, token);
				}
				catch (Exception ex) {
					return new CodeViewData(string.Format("Error occured in decompiling:{0}{1}", Environment.NewLine, ex));
				}
			}
		}

		void OnLoading(object sender, EventArgs e) {
			view.SetPlainText("Decompiling...");
		}

		void OnCompleted(object sender, OperationResultEventArgs<CodeViewData> e) {
			view.SetData(e.Result);
			op = null;
			cancellation = null;
		}

		ContextMenuStrip ctxMenu;

		protected internal override ContextMenuStrip GetContextMenu() {
			if (ctxMenu != null)
				return ctxMenu;

			ctxMenu = new ContextMenuStrip();

			var gotoMD = new ToolStripMenuItem("Go To MetaData View");
			gotoMD.Click += GotoMD;
			ctxMenu.Items.Add(gotoMD);

			return ctxMenu;
		}

		void GotoMD(object sender, EventArgs e) {
			var model = sender.GetContextMenuModel<ObjModel>();
			var module = model.Definition as ModuleDefMD;
			if (module == null)
				module = (ModuleDefMD)((IMemberDef)model.Definition).Module;
			ViewUtils.ShowToken(App, model, module.MetaData.PEImage, model.Definition.MDToken);
		}
	}
}
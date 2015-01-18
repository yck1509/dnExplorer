using System;
using System.Threading;
using System.Windows.Forms;
using dnExplorer.Controls;
using dnExplorer.Language;
using dnExplorer.Models;
using dnlib.DotNet;
using ScintillaNET;

namespace dnExplorer.Views {
	public class ObjCodeView : ViewBase<ObjModel> {
		CodeView view;

		public ObjCodeView() {
			view = new CodeView();
			view.Navigate += OnNavigateTarget;
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

			App.Languages.PropertyChanged += (sender, e) => GenerateCode();
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
				var state = new RunState(Model.Definition, App.Languages.ActiveLanguage, cancellation.Token);
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

		void OnNavigateTarget(object sender, CodeViewNavigateEventArgs e) {
			if (!e.IsLocal) {
				if (e.Target is IMemberDef)
					ViewUtils.ShowMember(Model, (IMemberDef)e.Target);
				else if (e.Target is IAssembly) {
					var resolver = App.Modules.Resolver;
					var assemblyRef = (IAssembly)e.Target;
					var assemblyDef = App.Modules.Resolver.Resolve(assemblyRef, null);
					if (assemblyDef != null)
						ViewUtils.ShowModule(Model, assemblyDef.ManifestModule);
					else
						MessageBox.Show("Failed to resolve '" + assemblyRef.FullName + "'.", App.AppName, MessageBoxButtons.OK,
							MessageBoxIcon.Error);
				}
				else if (e.Target is MemberRef) {
					var memberRef = (MemberRef)e.Target;
					var memberDef = (IMemberDef)memberRef.Resolve();
					if (memberDef != null)
						ViewUtils.ShowMember(Model, memberDef);
					else
						MessageBox.Show("Failed to resolve '" + memberRef.FullName + "'.", App.AppName, MessageBoxButtons.OK,
							MessageBoxIcon.Error);
				}
				else if (e.Target is TypeRef) {
					var typeRef = (TypeRef)e.Target;
					var typeDef = typeRef.Resolve();
					if (typeDef != null)
						ViewUtils.ShowMember(Model, typeDef);
					else
						MessageBox.Show("Failed to resolve '" + typeRef.FullName + "'.", App.AppName, MessageBoxButtons.OK,
							MessageBoxIcon.Error);
				}
				else if (e.Target is MethodSpec) {
					var methodRef = ((MethodSpec)e.Target).Method;
					var methodDef = methodRef.ResolveMethodDef();
					if (methodDef != null)
						ViewUtils.ShowMember(Model, methodDef);
					else
						MessageBox.Show("Failed to resolve '" + methodRef.FullName + "'.", App.AppName, MessageBoxButtons.OK,
							MessageBoxIcon.Error);
				}
				else
					MessageBox.Show("Unsupported navigation target '" + e.Target.GetType().FullName + "'.", App.AppName,
						MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else if (!e.IsDefinition)
				NavigateLocal(e.Target);
		}

		void NavigateLocal(object target) {
			foreach (var textRef in view.Data.References) {
				if (textRef.Value.Reference.Equals(target) && textRef.Value.IsDefinition) {
					new Range(textRef.Key, textRef.Key + textRef.Value.Length, view).Select();
					return;
				}
			}
			MessageBox.Show("Cannot find definition of '" + target + "'.", App.AppName, MessageBoxButtons.OK,
				MessageBoxIcon.Error);
		}

		ContextMenuStrip ctxMenu;

		protected internal override ContextMenuStrip GetContextMenu() {
			if (ctxMenu != null)
				return ctxMenu;

			ctxMenu = new ContextMenuStrip();

			var gotoMD = new ToolStripMenuItem("Go To MetaData View");
			gotoMD.Click += GotoMD;
			ctxMenu.Items.Add(gotoMD);

			var analyze = new ToolStripMenuItem("Analyze");
			analyze.Click += Analyze;
			ctxMenu.Items.Add(analyze);

			return ctxMenu;
		}

		void GotoMD(object sender, EventArgs e) {
			var model = sender.GetContextMenuModel<ObjModel>();
			var module = model.Definition as ModuleDefMD;
			if (module == null)
				module = (ModuleDefMD)((IMemberDef)model.Definition).Module;
			ViewUtils.ShowToken(App, model, module.MetaData.PEImage, model.Definition.MDToken);
		}

		void Analyze(object sender, EventArgs e) {
			var model = sender.GetContextMenuModel<ObjModel>();
			App.Analyzer.Display(model.Definition);
		}
	}
}
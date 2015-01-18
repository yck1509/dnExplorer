using System;
using dnExplorer.Analysis;
using dnExplorer.Language;
using dnExplorer.Views;
using WeifenLuo.WinFormsUI.Docking;

namespace dnExplorer {
	public interface IApp {
		string AppName { get; }
		DockPanel DockArea { get; }

		ModuleManager Modules { get; }
		LanguageManager Languages { get; }
		Analyzer Analyzer { get; }

		ViewLocator Views { get; }
	}
}
using System;
using dnExplorer.Language;
using dnExplorer.Views;

namespace dnExplorer {
	public interface IApp {
		string AppName { get; }

		ModuleManager Modules { get; }
		LanguageManager Languages { get; }

		ViewLocator Views { get; }
	}
}
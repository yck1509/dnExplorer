using System;
using dnExplorer.Language;
using dnExplorer.Views;

namespace dnExplorer {
	public interface IApp {
		string AppName { get; }

		ModuleManager Module { get; }
		LanguageManager Language { get; }

		ViewLocator Views { get; }
	}
}
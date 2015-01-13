using System;
using System.Threading;
using dnExplorer.Controls;
using dnlib.DotNet;

namespace dnExplorer.Language {
	public interface ILanguage {
		string Name { get; }
		CodeViewData Run(IDnlibDef item, CancellationToken token);
	}
}
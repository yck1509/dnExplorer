using System;
using dnlib.DotNet;

namespace dnExplorer.Analysis {
	public class AnalysisError {
		public string Message { get; private set; }
		public IFullName ErrorObject { get; private set; }

		public AnalysisError(string message, IFullName errObj = null) {
			Message = message;
			ErrorObject = errObj;
		}
	}
}
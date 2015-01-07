using System;
using dnExplorer.Trees;
using dnlib.DotNet;

namespace dnExplorer.Models {
	public abstract class ObjModel : LazyModel {
		public abstract IDnlibDef Definition { get; }
	}
}
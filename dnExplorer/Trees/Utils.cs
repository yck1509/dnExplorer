using System;

namespace dnExplorer.Trees {
	public static class Utils {
		public static DataTreeNodeX ToNode(this IDataModel model) {
			return new DataTreeNodeX(model);
		}
	}
}
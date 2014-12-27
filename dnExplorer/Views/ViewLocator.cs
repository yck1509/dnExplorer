using System;
using System.Collections.Generic;
using dnExplorer.Nodes;
using dnExplorer.Trees;

namespace dnExplorer.Views {
	public class ViewLocator {
		static readonly Dictionary<Type, ViewBase> views = new Dictionary<Type, ViewBase>();

		public static ViewBase LocateView(IDataModel model) {
			ViewBase view;
			if (!views.TryGetValue(model.GetType(), out view)) {
				if (model is ModuleModel)
					view = new ModuleView();

				else if (model is PEImageModel)
					view = new PEImageView();
				else if (model is PESectionsModel)
					view = new PESectionsView();
				else if (model is PESectionModel)
					view = new PESectionView();
				else if (model is PEDDModel)
					view = new PEDDView();
				else if (model is PECLIModel)
					view = new PECLIView();

				else if (model is MetaDataModel)
					view = new MetaDataView();
				else if (model is MDStreamModel)
					view = new MDStreamView();
				else if (model is MDTablesStreamModel)
					view = new MDTablesStreamView();

				else
					view = null;
				views[model.GetType()] = view;
			}
			return view;
		}
	}
}
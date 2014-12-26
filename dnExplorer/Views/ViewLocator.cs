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
				if (model is PEImageModel)
					view = new PEImageView();
				else if (model is PESectionsModel)
					view = new PESectionsView();
				else if (model is PESectionModel)
					view = new PESectionView();
				else if (model is PEDDModel)
					view = new PEDDView();
				else
					view = null;
				views[model.GetType()] = view;
			}
			return view;
		}
	}
}
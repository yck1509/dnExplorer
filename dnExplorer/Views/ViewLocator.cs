using System;
using System.Collections.Generic;
using dnExplorer.Trees;

namespace dnExplorer.Views {
	public class ViewLocator {
		internal ViewLocator(IApp app) {
			viewMap = new Dictionary<Type, IList<IView>>();
			foreach (var type in typeof(IView).Assembly.GetTypes()) {
				if (!type.IsAbstract && typeof(IView).IsAssignableFrom(type)) {
					var baseType = type.BaseType;

					if (baseType != null && baseType.IsGenericType &&
					    baseType.GetGenericTypeDefinition() == typeof(ViewBase<>)) {
						var view = (IView)Activator.CreateInstance(type);
						type.GetProperty("App").SetValue(view, app, null);

						var modelType = baseType.GetGenericArguments()[0];
						viewMap.AddListEntry(modelType, view);
					}
				}
			}
		}

		readonly Dictionary<Type, IList<IView>> viewMap;

		public IEnumerable<IView> LocateViews(IDataModel model) {
			var modelType = model.GetType();
			foreach (var viewType in viewMap) {
				if (!viewType.Key.IsAssignableFrom(modelType))
					continue;
				foreach (var view in viewType.Value)
					yield return view;
			}
		}
	}
}
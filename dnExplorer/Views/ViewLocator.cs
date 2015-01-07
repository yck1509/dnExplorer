using System;
using System.Collections.Generic;
using dnExplorer.Trees;

namespace dnExplorer.Views {
	public class ViewLocator {
		static ViewLocator() {
			viewMap = new Dictionary<Type, IList<IView>>();
			foreach (var type in typeof(IView).Assembly.GetTypes()) {
				if (!type.IsAbstract && typeof(IView).IsAssignableFrom(type)) {
					var baseType = type.BaseType;

					if (baseType != null && baseType.IsGenericType &&
					    baseType.GetGenericTypeDefinition() == typeof(ViewBase<>)) {
						var modelType = baseType.GetGenericArguments()[0];
						var view = (IView)Activator.CreateInstance(type);

						viewMap.AddListEntry(modelType, view);
					}
				}
			}
		}

		static readonly Dictionary<Type, IList<IView>> viewMap;

		public static IEnumerable<IView> LocateViews(IDataModel model) {
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
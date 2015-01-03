using System;
using System.Collections.Generic;
using System.Linq;
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
			IList<IView> views;
			if (!viewMap.TryGetValue(model.GetType(), out views))
				return Enumerable.Empty<IView>();
			return views;
		}
	}
}
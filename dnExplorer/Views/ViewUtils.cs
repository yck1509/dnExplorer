using System;
using dnExplorer.Nodes;
using dnExplorer.Trees;
using dnlib.DotNet;
using dnlib.PE;

namespace dnExplorer.Views {
	public static class ViewUtils {
		public static void ShowRawData(IDataModel model, IPEImage image, long begin, long end) {
			TreeNavigator.Create()
				.Path<ModuleModel>(m => m.Module.Image == image ? NavigationState.In : NavigationState.Next)
				.Path<RawDataModel>(m => NavigationState.Done)
				.Handler(node => {
					var targetView = (RawDataView)ViewLocator.LocateView(node.Model);
					targetView.Select(begin, end);
				})
				.Navigate(model);
		}

		public static void ShowToken(IDataModel model, IPEImage image, MDToken token) {
			TreeNavigator.Create()
				.Path<ModuleModel>(m => m.Module.Image == image ? NavigationState.In : NavigationState.Next)
				.Path<MetaDataModel>(m => NavigationState.In)
				.Path<MDTablesStreamModel>(m => NavigationState.In)
				.Path<MDTableHeapModel>(m => NavigationState.Done)
				.Handler(node => {
					var targetView = (MDTableHeapView)ViewLocator.LocateView(node.Model);
					targetView.SelectItem(token);
				})
				.Navigate(model);
		}
	}
}
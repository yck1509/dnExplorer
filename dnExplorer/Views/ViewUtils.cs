using System;
using System.Windows.Forms;
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

		public static TModel GetContextMenuModel<TModel>(this object sender)
			where TModel : class, IDataModel {
			ToolStrip menu;
			if (sender is ToolStripItem) {
				var item = (ToolStripItem)sender;

				menu = item.Owner;
				while (menu != null && !(menu is ContextMenuStrip)) {
					if (menu is ToolStripDropDown)
						menu = ((ToolStripDropDown)menu).OwnerItem.Owner;
					else
						menu = null;
				}
				if (menu == null)
					return null;
			}
			else if (sender is ContextMenuStrip)
				menu = (ContextMenuStrip)sender;
			else
				return null;

			var node = menu.Tag as DataTreeNodeX;
			if (node == null)
				return null;

			return (TModel)node.Model;
		}
	}
}
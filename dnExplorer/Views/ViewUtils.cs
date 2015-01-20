using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using dnExplorer.Models;
using dnExplorer.Trees;
using dnlib.DotNet;
using dnlib.DotNet.MD;
using dnlib.PE;

namespace dnExplorer.Views {
	public static class ViewUtils {
		public static void ShowRawData(IApp app, IDataModel model, IPEImage image, long begin, long end) {
			TreeNavigator.Create()
				.Path<dnModuleModel>(m => m.Module.Image == image ? NavigationState.In : NavigationState.Next)
				.Path<RawDataModel>(m => NavigationState.Done)
				.Handler(node => {
					var targetView = (RawDataView)app.Views.LocateViews(node.Model).Single();
					targetView.Select(begin, end);
				})
				.Goto(model);
		}

		public static void ShowToken(IApp app, IDataModel model, IPEImage image, MDToken token) {
			TreeNavigator.Create()
				.Path<dnModuleModel>(m => m.Module.Image == image ? NavigationState.In : NavigationState.Next)
				.Path<MetaDataModel>(m => NavigationState.In)
				.Path<MDTablesStreamModel>(m => NavigationState.In)
				.Path<MDTableHeapModel>(m => NavigationState.Done)
				.Handler(node => {
					var targetView = (MDTableHeapView)app.Views.LocateViews(node.Model).Single();
					targetView.SelectItem(token);
				})
				.Goto(model);
		}

		public static void ShowStream(IApp app, IDataModel model, IPEImage image, DotNetStream stream, uint begin, uint size) {
			TreeNavigator.Create()
				.Path<dnModuleModel>(m => m.Module.Image == image ? NavigationState.In : NavigationState.Next)
				.Path<MetaDataModel>(m => NavigationState.In)
				.Path<MDStreamModel>(m => m.Stream == stream ? NavigationState.Done : NavigationState.Next)
				.Path<MDTablesStreamModel>(m => m.Stream == stream ? NavigationState.Done : NavigationState.Next)
				.Handler(node => {
					if (node.Model is MDStreamModel) {
						var targetView = (MDStreamView)app.Views.LocateViews(node.Model).Single();
						targetView.SelectHexRange(begin, begin + size - 1);
					}
					else if (node.Model is MDTablesStreamModel) {
						var targetView = (MDTablesStreamView)app.Views.LocateViews(node.Model).Single();
						targetView.SelectHexRange(begin, begin + size - 1);
					}
				})
				.Goto(model);
		}

		public static void ShowMember(IDataModel model, IMemberDef member) {
			ShowMember((TreeViewX)model.Node.TreeView, member);
		}

		public static void ShowMember(TreeViewX treeView, IMemberDef member) {
			var declTypes = new HashSet<TypeDef>();
			var declType = member.DeclaringType;
			var ns = (member is TypeDef) ? ((TypeDef)member).Namespace : null;
			while (declType != null) {
				ns = declType.Namespace;
				declTypes.Add(declType);
				declType = declType.DeclaringType;
			}

			TreeNavigator.Create()
				.Path<dnModuleModel>(m => m.Module.ModuleDef == member.Module ? NavigationState.In : NavigationState.Next)
				.Path<ModuleModel>(m => NavigationState.In)
				.Path<NamespaceModel>(m => m.Namespace == ns ? NavigationState.In : NavigationState.Next)
				.Path<TypeModel>(
					m =>
						m.Type == member ? NavigationState.Done : (declTypes.Contains(m.Type) ? NavigationState.In : NavigationState.Next))
				.Path<MethodModel>(m => m.Method == member ? NavigationState.Done : NavigationState.Next)
				.Path<FieldModel>(m => m.Field == member ? NavigationState.Done : NavigationState.Next)
				.Path<PropertyModel>(
					m =>
						m.Property == member
							? NavigationState.Done
							: (m.Property.GetAccessors().Contains(member) ? NavigationState.In : NavigationState.Next))
				.Path<EventModel>(
					m =>
						m.Event == member
							? NavigationState.Done
							: (m.Event.GetAccessors().Contains(member) ? NavigationState.In : NavigationState.Next))
				.Goto(treeView);
		}

		public static void ShowModule(IDataModel model, ModuleDef module) {
			ShowModule((TreeViewX)model.Node.TreeView, module);
		}

		public static void ShowModule(TreeViewX treeView, ModuleDef module) {
			TreeNavigator.Create()
				.Path<dnModuleModel>(m => m.Module.ModuleDef == module ? NavigationState.Done : NavigationState.Next)
				.Goto(treeView);
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

			var view = ((ContextMenuStrip)menu).SourceControl as TreeViewX;
			if (view == null)
				return null;

			var node = view.SelectedNode as DataTreeNodeX;
			if (node != null)
				return (TModel)node.Model;
			return null;
		}
	}
}
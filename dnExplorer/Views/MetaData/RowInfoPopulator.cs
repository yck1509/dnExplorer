using System;
using System.Drawing;
using System.Windows.Forms;
using dnExplorer.Controls;
using dnExplorer.Models;
using dnlib.DotNet;
using dnlib.DotNet.MD;
using dnlib.IO;
using SR = System.Reflection;

namespace dnExplorer.Views {
	internal class RowInfoPopulator {
		static readonly object InvalidValue = "<<INVALID>>\0";

		static readonly Func<TablesStream, MDTable, uint, IBinaryReader> getReader;

		static RowInfoPopulator() {
			var getReaderInfo = typeof(TablesStream).GetMethod("GetReader_NoLock",
				SR.BindingFlags.NonPublic | SR.BindingFlags.Instance);
			getReader =
				(Func<TablesStream, MDTable, uint, IBinaryReader>)
					Delegate.CreateDelegate(typeof(Func<TablesStream, MDTable, uint, IBinaryReader>), getReaderInfo);
		}

		public static void PopulateGridView(MDTableHeapView view, GridView gridView, MDRowModel row) {
			var reader = getReader(row.Parent.Tables, row.Parent.MDTable, row.Rid);

			var beginPos = reader.Position;
			gridView.Clear();
			foreach (var column in row.Parent.MDTable.Columns) {
				reader.Position = beginPos + column.Offset;
				var offset = (uint)row.Parent.MDTable.StartOffset + (uint)reader.Position;

				object value = null;
				uint rawValue = 0;
				switch (column.Size) {
					case 1:
						byte u1 = reader.ReadByte();
						value = u1;
						rawValue = u1;
						break;
					case 2:
						ushort u2 = reader.ReadUInt16();
						value = u2;
						rawValue = u2;
						break;
					case 4:
						uint u4 = reader.ReadUInt32();
						value = u4;
						rawValue = u4;
						break;
					default:
						throw new Exception("Unexpected column size.");
				}

				var desc = GetDescription(row.Parent.MetaData, row.Parent.MDTable, column, rawValue);
				var ctxMenu = GetContextMenu(view, row, row.Parent.MetaData, column.ColumnSize);
				var cell = new GridView.Cell(desc,
					back: desc == InvalidValue ? ControlPaint.Light(Color.Red) : SystemColors.ControlLight);
				gridView.AddRow(column.Name, column.ColumnSize.ToString(), offset, value, cell, ctxMenu);
			}
		}

		static object GetDescription(IMetaData metadata, MDTable table, ColumnInfo column, uint rowValue) {
			if (ColumnSize.Module <= column.ColumnSize && column.ColumnSize <= ColumnSize.GenericParamConstraint) {
				Table targetTable = (Table)(column.ColumnSize - ColumnSize.Module);

				if (rowValue == 0)
					return "";
				return new MDToken(targetTable, rowValue).ToDescription();
			}
			if (ColumnSize.TypeDefOrRef <= column.ColumnSize && column.ColumnSize <= ColumnSize.TypeOrMethodDef) {
				CodedToken info;
				switch (column.ColumnSize) {
					case ColumnSize.TypeDefOrRef:
						info = CodedToken.TypeDefOrRef;
						break;
					case ColumnSize.HasConstant:
						info = CodedToken.HasConstant;
						break;
					case ColumnSize.HasCustomAttribute:
						info = CodedToken.HasCustomAttribute;
						break;
					case ColumnSize.HasFieldMarshal:
						info = CodedToken.HasFieldMarshal;
						break;
					case ColumnSize.HasDeclSecurity:
						info = CodedToken.HasDeclSecurity;
						break;
					case ColumnSize.MemberRefParent:
						info = CodedToken.MemberRefParent;
						break;
					case ColumnSize.HasSemantic:
						info = CodedToken.HasSemantic;
						break;
					case ColumnSize.MethodDefOrRef:
						info = CodedToken.MethodDefOrRef;
						break;
					case ColumnSize.MemberForwarded:
						info = CodedToken.MemberForwarded;
						break;
					case ColumnSize.Implementation:
						info = CodedToken.Implementation;
						break;
					case ColumnSize.CustomAttributeType:
						info = CodedToken.CustomAttributeType;
						break;
					case ColumnSize.ResolutionScope:
						info = CodedToken.ResolutionScope;
						break;
					case ColumnSize.TypeOrMethodDef:
						info = CodedToken.TypeOrMethodDef;
						break;
					default:
						throw new InvalidOperationException(string.Format("Invalid ColumnSize: {0}", column.ColumnSize));
				}

				MDToken decodedToken;
				if (!info.Decode(rowValue, out decodedToken))
					return InvalidValue;

				if (decodedToken.Rid == 0)
					return "";
				return decodedToken.ToDescription();
			}
			if (column.ColumnSize == ColumnSize.Strings) {
				if (rowValue == 0)
					return "";

				var value = metadata.StringsStream.Read(rowValue);
				if (value == (UTF8String)null)
					return InvalidValue;
				return value;
			}
			if (column.ColumnSize == ColumnSize.Blob) {
				if (rowValue == 0)
					return "";

				if (!metadata.BlobStream.IsValidOffset(rowValue))
					return InvalidValue;

				var reader = metadata.BlobStream.GetClonedImageStream();
				reader.Position = rowValue;
				return reader.ReadCompressedUInt32();
			}
			if (column.ColumnSize == ColumnSize.GUID) {
				if (rowValue == 0)
					return "";

				var value = metadata.GuidStream.Read(rowValue);
				if (value == null)
					return InvalidValue;
				return value.Value.ToString("D");
			}
			return GetLiteralDescription(table.Table, column, rowValue);
		}

		static string GetLiteralDescription(Table table, ColumnInfo column, uint rowValue) {
			object enumValue = null;
			switch (table) {
				case Table.TypeDef:
					if (column.Name == "Flags")
						enumValue = (TypeAttributes)rowValue;
					break;

				case Table.Field:
					if (column.Name == "Flags")
						enumValue = (FieldAttributes)rowValue;
					break;

				case Table.Method:
					if (column.Name == "ImplFlags")
						enumValue = (MethodImplAttributes)rowValue;
					else if (column.Name == "Flags")
						enumValue = (MethodAttributes)rowValue;
					break;

				case Table.Param:
					if (column.Name == "Flags")
						enumValue = (ParamAttributes)rowValue;
					break;

				case Table.Constant:
					if (column.Name == "Type")
						enumValue = (ElementType)rowValue;
					break;

				case Table.DeclSecurity:
					if (column.Name == "Action")
						enumValue = (SecurityAction)rowValue;
					break;

				case Table.Event:
					if (column.Name == "EventFlags")
						enumValue = (EventAttributes)rowValue;
					break;

				case Table.Property:
					if (column.Name == "Flags")
						enumValue = (PropertyAttributes)rowValue;
					break;

				case Table.MethodSemantics:
					if (column.Name == "Semantic")
						enumValue = (MethodSemanticsAttributes)rowValue;
					break;

				case Table.ImplMap:
					if (column.Name == "MappingFlags")
						enumValue = (PInvokeAttributes)rowValue;
					break;

				case Table.Assembly:
					if (column.Name == "HashAlgId")
						enumValue = (AssemblyHashAlgorithm)rowValue;
					else if (column.Name == "Flags")
						enumValue = (AssemblyAttributes)rowValue;
					break;

				case Table.AssemblyRef:
					if (column.Name == "Flags")
						enumValue = (AssemblyAttributes)rowValue;
					break;

				case Table.File:
					if (column.Name == "Flags")
						enumValue = (FileAttributes)rowValue;
					break;

				case Table.ExportedType:
					if (column.Name == "Flags")
						enumValue = (TypeAttributes)rowValue;
					break;

				case Table.ManifestResource:
					if (column.Name == "Flags")
						enumValue = (ManifestResourceAttributes)rowValue;
					break;

				case Table.GenericParam:
					if (column.Name == "Flags")
						enumValue = (GenericParamAttributes)rowValue;
					break;
			}


			if (enumValue == null)
				return "";
			return enumValue.ToString();
		}

		static ContextMenuStrip GetContextMenu(MDTableHeapView view, MDRowModel row, IMetaData metadata, ColumnSize size) {
			ContextMenuStrip ctxMenu;

			if (ColumnSize.Module <= size && size <= ColumnSize.GenericParamConstraint) {
				Table targetTable = (Table)(size - ColumnSize.Module);
				ctxMenu = CreateMDTokenContextMenu(view, targetTable);
			}
			else if (ColumnSize.TypeDefOrRef <= size && size <= ColumnSize.TypeOrMethodDef) {
				CodedToken info;
				switch (size) {
					case ColumnSize.TypeDefOrRef:
						info = CodedToken.TypeDefOrRef;
						break;
					case ColumnSize.HasConstant:
						info = CodedToken.HasConstant;
						break;
					case ColumnSize.HasCustomAttribute:
						info = CodedToken.HasCustomAttribute;
						break;
					case ColumnSize.HasFieldMarshal:
						info = CodedToken.HasFieldMarshal;
						break;
					case ColumnSize.HasDeclSecurity:
						info = CodedToken.HasDeclSecurity;
						break;
					case ColumnSize.MemberRefParent:
						info = CodedToken.MemberRefParent;
						break;
					case ColumnSize.HasSemantic:
						info = CodedToken.HasSemantic;
						break;
					case ColumnSize.MethodDefOrRef:
						info = CodedToken.MethodDefOrRef;
						break;
					case ColumnSize.MemberForwarded:
						info = CodedToken.MemberForwarded;
						break;
					case ColumnSize.Implementation:
						info = CodedToken.Implementation;
						break;
					case ColumnSize.CustomAttributeType:
						info = CodedToken.CustomAttributeType;
						break;
					case ColumnSize.ResolutionScope:
						info = CodedToken.ResolutionScope;
						break;
					case ColumnSize.TypeOrMethodDef:
						info = CodedToken.TypeOrMethodDef;
						break;
					default:
						throw new InvalidOperationException(string.Format("Invalid ColumnSize: {0}", size));
				}

				ctxMenu = CreateCodedTokenContextMenu(view, info);
			}
			else if (size == ColumnSize.Strings) {
				ctxMenu = CreateStreamContextMenu(view, metadata, metadata.StringsStream);
			}
			else if (size == ColumnSize.Blob) {
				ctxMenu = CreateStreamContextMenu(view, metadata, metadata.BlobStream);
			}
			else if (size == ColumnSize.GUID) {
				ctxMenu = CreateStreamContextMenu(view, metadata, metadata.GuidStream);
			}
			else
				ctxMenu = null;

			return ctxMenu;
		}

		static ContextMenuStrip CreateMDTokenContextMenu(MDTableHeapView view, Table table) {
			var menu = new ContextMenuStrip();

			var go = new ToolStripMenuItem("Go To Row");
			go.Click += (sender, e) => {
				var gridView = (GridView)menu.SourceControl;
				var rowIndex = gridView.SelectedCells[0].RowIndex;
				var rid = Convert.ToUInt32(gridView[3, rowIndex].Value);
				view.SelectItem(new MDToken(table, rid));
			};
			menu.Items.Add(go);

			return menu;
		}

		static ContextMenuStrip CreateCodedTokenContextMenu(MDTableHeapView view, CodedToken code) {
			var menu = new ContextMenuStrip();

			var go = new ToolStripMenuItem("Go To Row");
			go.Click += (sender, e) => {
				var gridView = (GridView)menu.SourceControl;
				var rowIndex = gridView.SelectedCells[0].RowIndex;

				if (gridView[4, rowIndex].Value == InvalidValue) {
					MessageBox.Show("Invalid token.", Main.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				var token = code.Decode(Convert.ToUInt32(gridView[3, rowIndex].Value));
				view.SelectItem(new MDToken(token));
			};
			menu.Items.Add(go);

			return menu;
		}

		static ContextMenuStrip CreateStreamContextMenu(MDTableHeapView view, IMetaData metadata, DotNetStream stream) {
			var menu = new ContextMenuStrip();

			var show = new ToolStripMenuItem("Show in " + stream.Name + " Stream");
			show.Click += (sender, e) => {
				var gridView = (GridView)menu.SourceControl;
				var rowIndex = gridView.SelectedCells[0].RowIndex;

				if (gridView[4, rowIndex].Value == InvalidValue) {
					MessageBox.Show("Invalid offset.", Main.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				var offset = Convert.ToUInt32(gridView[3, rowIndex].Value);
				if (offset == 0)
					return;

				var desc = gridView[4, rowIndex].Value;
				uint size = 1;
				if (stream is StringsStream) {
					size = (uint)((UTF8String)desc).DataLength + 1;
				}
				else if (stream is BlobStream) {
					size = (uint)gridView[4, rowIndex].Value;
					size += (uint)Utils.GetCompressedUInt32Length(size);
				}
				else if (stream is GuidStream) {
					offset = (offset - 1) * 0x10;
					size = 0x10;
				}


				ViewUtils.ShowStream(view.Model, metadata.PEImage, stream, offset, size);
			};
			menu.Items.Add(show);

			return menu;
		}
	}
}
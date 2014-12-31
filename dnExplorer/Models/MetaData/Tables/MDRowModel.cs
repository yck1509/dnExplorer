using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using dnExplorer.Trees;
using dnlib.DotNet;
using dnlib.DotNet.MD;

namespace dnExplorer.Models {
	public class MDRowModel : LazyModel {
		public MDTableModel Parent { get; set; }
		public uint Rid { get; set; }

		public MDRowModel(MDTableModel parent, uint rid) {
			Parent = parent;
			Rid = rid;

			var text = new StringBuilder();
			text.AppendFormat("0x{0:x}", Rid);
			var disp = GetDisplayText();
			if (disp != null)
				text.AppendFormat(" - {0}", disp);
			Text = text.ToString();
		}

		protected override bool HasChildren {
			get { return false; }
		}

		protected override bool IsVolatile {
			get { return false; }
		}

		protected override IEnumerable<IDataModel> PopulateChildren() {
			yield break;
		}

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
			g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.entry.png"), bounds);
		}

		string ReadString(uint offset) {
			var value = Parent.MetaData.StringsStream.Read(offset);
			if (value == (UTF8String)null)
				return "<<INVALID>>";
			return Utils.EscapeString(value, true);
		}

		string ToTokenString(Table table, uint rid) {
			if (!Parent.Tables.HasTable(table) || Parent.Tables.Get(table).IsInvalidRID(rid))
				return "<<INVALID>>";

			return new MDToken(table, rid).ToDescription();
		}

		string DecodeToken(CodedToken desc, uint codedToken) {
			MDToken token;
			if (!desc.Decode(codedToken, out token))
				return "<<INVALID>>";

			if (!Parent.Tables.HasTable(token.Table) ||
			    Parent.Tables.Get(token.Table).IsInvalidRID(token.Rid))
				return "<<INVALID>>";

			return token.ToDescription();
		}

		string GetDisplayText() {
			switch (Parent.MDTable.Table) {
				case Table.Module:
					return ReadString(Parent.Tables.ReadModuleRow(Rid).Name);

				case Table.TypeRef:
					var typeRef = Parent.Tables.ReadTypeRefRow(Rid);
					return string.Format("({0}, {1})",
						ReadString(typeRef.Namespace),
						ReadString(typeRef.Name));

				case Table.TypeDef:
					var typeDef = Parent.Tables.ReadTypeDefRow(Rid);
					return string.Format("({0}, {1})",
						ReadString(typeDef.Namespace),
						ReadString(typeDef.Name));

				case Table.FieldPtr:
					return ToTokenString(Table.Field, Parent.Tables.ReadFieldPtrRow(Rid).Field);

				case Table.Field:
					return ReadString(Parent.Tables.ReadFieldRow(Rid).Name);

				case Table.MethodPtr:
					return ToTokenString(Table.Method, Parent.Tables.ReadMethodPtrRow(Rid).Method);

				case Table.Method:
					return ReadString(Parent.Tables.ReadMethodRow(Rid).Name);

				case Table.ParamPtr:
					return ToTokenString(Table.Param, Parent.Tables.ReadParamPtrRow(Rid).Param);

				case Table.Param:
					return ReadString(Parent.Tables.ReadParamRow(Rid).Name);

				case Table.InterfaceImpl:
					var ifaceImpl = Parent.Tables.ReadInterfaceImplRow(Rid);
					return string.Format("({0} : {1})",
						ToTokenString(Table.TypeDef, ifaceImpl.Class),
						DecodeToken(CodedToken.TypeDefOrRef, ifaceImpl.Interface));

				case Table.MemberRef:
					return ReadString(Parent.Tables.ReadMemberRefRow(Rid).Name);

				case Table.Constant:
					return DecodeToken(CodedToken.HasConstant, Parent.Tables.ReadConstantRow(Rid).Parent);

				case Table.CustomAttribute:
					return DecodeToken(CodedToken.HasCustomAttribute, Parent.Tables.ReadCustomAttributeRow(Rid).Parent);

				case Table.FieldMarshal:
					return DecodeToken(CodedToken.HasFieldMarshal, Parent.Tables.ReadFieldMarshalRow(Rid).Parent);

				case Table.DeclSecurity:
					return DecodeToken(CodedToken.HasDeclSecurity, Parent.Tables.ReadDeclSecurityRow(Rid).Parent);

				case Table.ClassLayout:
					return ToTokenString(Table.TypeDef, Parent.Tables.ReadClassLayoutRow(Rid).Parent);

				case Table.FieldLayout:
					return ToTokenString(Table.Field, Parent.Tables.ReadFieldLayoutRow(Rid).Field);

				case Table.StandAloneSig:
					return string.Format("0x{0:x}", Parent.Tables.ReadStandAloneSigRow(Rid).Signature);

				case Table.EventMap:
					var eventMap = Parent.Tables.ReadEventMapRow(Rid);
					return string.Format("({0} : {1})",
						ToTokenString(Table.TypeDef, eventMap.Parent),
						ToTokenString(Table.Event, eventMap.EventList));

				case Table.EventPtr:
					return ToTokenString(Table.Event, Parent.Tables.ReadEventPtrRow(Rid).Event);

				case Table.Event:
					return ReadString(Parent.Tables.ReadEventRow(Rid).Name);

				case Table.PropertyMap:
					var propertyMap = Parent.Tables.ReadPropertyMapRow(Rid);
					return string.Format("({0} : {1})",
						ToTokenString(Table.TypeDef, propertyMap.Parent),
						ToTokenString(Table.Property, propertyMap.PropertyList));

				case Table.PropertyPtr:
					return ToTokenString(Table.Property, Parent.Tables.ReadPropertyPtrRow(Rid).Property);

				case Table.Property:
					return ReadString(Parent.Tables.ReadPropertyRow(Rid).Name);

				case Table.MethodSemantics:
					var methodSemantics = Parent.Tables.ReadMethodSemanticsRow(Rid);
					return string.Format("({0} : {1})",
						DecodeToken(CodedToken.HasSemantic, methodSemantics.Association),
						ToTokenString(Table.Method, methodSemantics.Method));

				case Table.MethodImpl:
					var methodImpl = Parent.Tables.ReadMethodImplRow(Rid);
					return string.Format("({0} : {1})",
						DecodeToken(CodedToken.MethodDefOrRef, methodImpl.MethodBody),
						DecodeToken(CodedToken.MethodDefOrRef, methodImpl.MethodDeclaration));

				case Table.ModuleRef:
					return ReadString(Parent.Tables.ReadModuleRefRow(Rid).Name);

				case Table.TypeSpec:
					return string.Format("0x{0:x}", Parent.Tables.ReadTypeSpecRow(Rid).Signature);

				case Table.ImplMap:
					return ReadString(Parent.Tables.ReadImplMapRow(Rid).ImportName);

				case Table.FieldRVA:
					return ToTokenString(Table.Field, Parent.Tables.ReadFieldRVARow(Rid).Field);

				case Table.ENCLog:
					var encLog = Parent.Tables.ReadENCLogRow(Rid);
					return string.Format("(0x{0:x8}, 0x{1:x8})", encLog.Token, encLog.FuncCode);

				case Table.ENCMap:
					var encMap = Parent.Tables.ReadENCMapRow(Rid);
					return string.Format("0x{0:x8}", encMap.Token);

				case Table.Assembly:
					return ReadString(Parent.Tables.ReadAssemblyRow(Rid).Name);

				case Table.AssemblyProcessor:
				case Table.AssemblyOS:
					break;

				case Table.AssemblyRef:
					return ReadString(Parent.Tables.ReadAssemblyRefRow(Rid).Name);

				case Table.AssemblyRefProcessor:
				case Table.AssemblyRefOS:
					break;

				case Table.File:
					return ReadString(Parent.Tables.ReadFileRow(Rid).Name);

				case Table.ExportedType:
					var exportedType = Parent.Tables.ReadExportedTypeRow(Rid);
					return string.Format("({0}, {1})",
						ReadString(exportedType.TypeNamespace),
						ReadString(exportedType.TypeName));

				case Table.ManifestResource:
					return ReadString(Parent.Tables.ReadManifestResourceRow(Rid).Name);

				case Table.NestedClass:
					var nestedClass = Parent.Tables.ReadNestedClassRow(Rid);
					return string.Format("({0} : {1})",
						ToTokenString(Table.TypeDef, nestedClass.EnclosingClass),
						ToTokenString(Table.TypeDef, nestedClass.NestedClass));

				case Table.GenericParam:
					return ReadString(Parent.Tables.ReadGenericParamRow(Rid).Name);

				case Table.MethodSpec:
					return DecodeToken(CodedToken.MethodDefOrRef, Parent.Tables.ReadMethodSpecRow(Rid).Method);

				case Table.GenericParamConstraint:
					return ToTokenString(Table.GenericParam, Parent.Tables.ReadGenericParamConstraintRow(Rid).Owner);
			}
			return null;
		}
	}
}
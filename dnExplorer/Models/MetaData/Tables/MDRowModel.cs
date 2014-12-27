using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using dnExplorer.Trees;
using dnlib.DotNet;
using dnlib.DotNet.MD;

namespace dnExplorer.Nodes {
	public class MDRowModel : LazyModel {
		public IMetaData MetaData { get; set; }
		public TablesStream Tables { get; set; }
		public MDTable MDTable { get; set; }
		public uint Rid { get; set; }

		public MDRowModel(IMetaData metadata, TablesStream stream, MDTable table, uint rid) {
			MetaData = metadata;
			Tables = stream;
			MDTable = table;
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
			var value = MetaData.StringsStream.Read(offset);
			if (value.Length == 0)
				return "<<EMPTY>>";
			if (value == (UTF8String)null)
				return "<<INVALID>>";
			return value;
		}

		string ToTokenString(Table table, uint rid) {
			if (!Tables.HasTable(table) || Tables.Get(table).IsInvalidRID(rid))
				return "<<INVALID>>";

			return string.Format("[{0} 0x{1:x}]", table, rid);
		}

		string DecodeToken(CodedToken desc, uint codedToken) {
			MDToken token;
			if (!desc.Decode(codedToken, out token))
				return "<<INVALID>>";

			if (!Tables.HasTable(token.Table) || Tables.Get(token.Table).IsInvalidRID(token.Rid))
				return "<<INVALID>>";

			return string.Format("[{0} 0x{1:x}]", token.Table, token.Rid);
		}

		string GetDisplayText() {
			switch (MDTable.Table) {
				case Table.Module:
					return ReadString(Tables.ReadModuleRow(Rid).Name);

				case Table.TypeRef:
					var typeRef = Tables.ReadTypeRefRow(Rid);
					return string.Format("{0}, {1}",
						ReadString(typeRef.Namespace),
						ReadString(typeRef.Name));

				case Table.TypeDef:
					var typeDef = Tables.ReadTypeDefRow(Rid);
					return string.Format("{0}, {1}",
						ReadString(typeDef.Namespace),
						ReadString(typeDef.Name));

				case Table.FieldPtr:
					return ToTokenString(Table.Field, Tables.ReadFieldPtrRow(Rid).Field);

				case Table.Field:
					return ReadString(Tables.ReadFieldRow(Rid).Name);

				case Table.MethodPtr:
					return ToTokenString(Table.Method, Tables.ReadMethodPtrRow(Rid).Method);

				case Table.Method:
					return ReadString(Tables.ReadMethodRow(Rid).Name);

				case Table.ParamPtr:
					return ToTokenString(Table.Param, Tables.ReadParamPtrRow(Rid).Param);

				case Table.Param:
					return ReadString(Tables.ReadParamRow(Rid).Name);

				case Table.InterfaceImpl:
					var ifaceImpl = Tables.ReadInterfaceImplRow(Rid);
					return string.Format("{0} : {1}",
						ToTokenString(Table.TypeDef, ifaceImpl.Class),
						DecodeToken(CodedToken.TypeDefOrRef, ifaceImpl.Interface));

				case Table.MemberRef:
					return ReadString(Tables.ReadMemberRefRow(Rid).Name);

				case Table.Constant:
					return DecodeToken(CodedToken.HasConstant, Tables.ReadConstantRow(Rid).Parent);

				case Table.CustomAttribute:
					return DecodeToken(CodedToken.HasCustomAttribute, Tables.ReadCustomAttributeRow(Rid).Parent);

				case Table.FieldMarshal:
					return DecodeToken(CodedToken.HasFieldMarshal, Tables.ReadFieldMarshalRow(Rid).Parent);

				case Table.DeclSecurity:
					return DecodeToken(CodedToken.HasDeclSecurity, Tables.ReadDeclSecurityRow(Rid).Parent);

				case Table.ClassLayout:
					return ToTokenString(Table.TypeDef, Tables.ReadClassLayoutRow(Rid).Parent);

				case Table.FieldLayout:
					return ToTokenString(Table.Field, Tables.ReadFieldLayoutRow(Rid).Field);

				case Table.StandAloneSig:
					return string.Format("0x{0:x}", Tables.ReadStandAloneSigRow(Rid).Signature);

				case Table.EventMap:
					var eventMap = Tables.ReadEventMapRow(Rid);
					return string.Format("{0} : {1}",
						ToTokenString(Table.TypeDef, eventMap.Parent),
						ToTokenString(Table.Event, eventMap.EventList));

				case Table.EventPtr:
					return ToTokenString(Table.Event, Tables.ReadEventPtrRow(Rid).Event);

				case Table.Event:
					return ReadString(Tables.ReadEventRow(Rid).Name);

				case Table.PropertyMap:
					var propertyMap = Tables.ReadPropertyMapRow(Rid);
					return string.Format("{0} : {1}",
						ToTokenString(Table.TypeDef, propertyMap.Parent),
						ToTokenString(Table.Property, propertyMap.PropertyList));

				case Table.PropertyPtr:
					return ToTokenString(Table.Property, Tables.ReadPropertyPtrRow(Rid).Property);

				case Table.Property:
					return ReadString(Tables.ReadPropertyRow(Rid).Name);

				case Table.MethodSemantics:
					var methodSemantics = Tables.ReadMethodSemanticsRow(Rid);
					return string.Format("{0} : {1}",
						DecodeToken(CodedToken.HasSemantic, methodSemantics.Association),
						ToTokenString(Table.Method, methodSemantics.Method));

				case Table.MethodImpl:
					var methodImpl = Tables.ReadMethodImplRow(Rid);
					return string.Format("{0} : {1}",
						DecodeToken(CodedToken.MethodDefOrRef, methodImpl.MethodBody),
						DecodeToken(CodedToken.MethodDefOrRef, methodImpl.MethodDeclaration));

				case Table.ModuleRef:
					return ReadString(Tables.ReadModuleRefRow(Rid).Name);

				case Table.TypeSpec:
					return string.Format("0x{0:x}", Tables.ReadTypeSpecRow(Rid).Signature);

				case Table.ImplMap:
					return ReadString(Tables.ReadImplMapRow(Rid).ImportName);

				case Table.FieldRVA:
					return ToTokenString(Table.Field, Tables.ReadFieldRVARow(Rid).Field);

				case Table.ENCLog:
					var encLog = Tables.ReadENCLogRow(Rid);
					return string.Format("0x{0:x8}, 0x{1:x8}", encLog.Token, encLog.FuncCode);

				case Table.ENCMap:
					var encMap = Tables.ReadENCMapRow(Rid);
					return string.Format("0x{0:x8}", encMap.Token);

				case Table.Assembly:
					return ReadString(Tables.ReadAssemblyRow(Rid).Name);

				case Table.AssemblyProcessor:
				case Table.AssemblyOS:
					break;

				case Table.AssemblyRef:
					return ReadString(Tables.ReadAssemblyRefRow(Rid).Name);

				case Table.AssemblyRefProcessor:
				case Table.AssemblyRefOS:
					break;

				case Table.File:
					return ReadString(Tables.ReadFileRow(Rid).Name);

				case Table.ExportedType:
					var exportedType = Tables.ReadExportedTypeRow(Rid);
					return string.Format("{0}, {1}",
						ReadString(exportedType.TypeNamespace),
						ReadString(exportedType.TypeName));

				case Table.ManifestResource:
					return ReadString(Tables.ReadManifestResourceRow(Rid).Name);

				case Table.NestedClass:
					var nestedClass = Tables.ReadNestedClassRow(Rid);
					return string.Format("{0} : {1}",
						ToTokenString(Table.TypeDef, nestedClass.EnclosingClass),
						ToTokenString(Table.TypeDef, nestedClass.NestedClass));

				case Table.GenericParam:
					return ReadString(Tables.ReadGenericParamRow(Rid).Name);

				case Table.MethodSpec:
					return DecodeToken(CodedToken.MethodDefOrRef, Tables.ReadMethodSpecRow(Rid).Method);

				case Table.GenericParamConstraint:
					return ToTokenString(Table.GenericParam, Tables.ReadGenericParamConstraintRow(Rid).Owner);
			}
			return null;
		}
	}
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using dnExplorer.Analysis;
using dnExplorer.Trees;
using dnlib.DotNet;

namespace dnExplorer.Models {
	public class TypeModel : ObjModel, IHasInfo {
		public TypeDef Type { get; set; }

		public override IDnlibDef Definition {
			get { return Type; }
		}

		public TypeModel(TypeDef type) {
			Type = type;
		}

		protected override bool HasChildren {
			get {
				return Type.BaseType != null || Type.HasInterfaces || !Type.IsSealed ||
				       Type.HasNestedTypes || Type.HasMethods || Type.HasProperties ||
				       Type.HasEvents || Type.HasFields;
			}
		}

		protected override bool IsVolatile {
			get { return true; }
		}

		protected override IEnumerable<IDataModel> PopulateChildren() {
			IAnalysis analysis;

			analysis = new BaseTypesAnalysis(Type);
			if (analysis.HasResult)
				yield return new AnalysisModel(analysis, true);

			analysis = new DerivedTypesAnalysis(Type);
			if (analysis.HasResult)
				yield return new AnalysisModel(analysis, true);

			foreach (var nestedType in Type.NestedTypes.OrderBy(type => type.Name))
				yield return new TypeModel(nestedType);

			foreach (var method in Type.Methods.OrderBy(method => method.Name)) {
				if (method.SemanticsAttributes != 0)
					continue;
				yield return new MethodModel(method);
			}

			foreach (var property in Type.Properties.OrderBy(property => property.Name))
				yield return new PropertyModel(property);

			foreach (var evnt in Type.Events.OrderBy(evnt => evnt.Name))
				yield return new EventModel(evnt);

			foreach (var field in Type.Fields.OrderBy(field => field.Name))
				yield return new FieldModel(field);
		}

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
			ObjectIconRenderer.RenderType(Type, g, bounds);
		}

		protected override void Refresh() {
			switch (Type.Visibility) {
				case TypeAttributes.NotPublic:
				case TypeAttributes.NestedAssembly:
				case TypeAttributes.NestedFamANDAssem:
				case TypeAttributes.NestedPrivate:
				case TypeAttributes.NestedFamily:
				case TypeAttributes.NestedFamORAssem:
					ForeColor = Color.Gray;
					break;
				default:
					ForeColor = Color.Empty;
					break;
			}
			Text = Utils.EscapeString(DisplayNameCreator.CreateDisplayName(Type), false);
		}

		string IHasInfo.Header {
			get { return Utils.EscapeString(Type.FullName, false); }
		}

		IEnumerable<KeyValuePair<string, string>> IHasInfo.GetInfos() {
			if (Type.DeclaringType != null)
				yield return
					new KeyValuePair<string, string>("Declaring Type", Utils.EscapeString(Type.DeclaringType.FullName, false));
			if (Type.Scope != null)
				yield return new KeyValuePair<string, string>("Scope", Utils.EscapeString(Type.Scope.ToString(), false));

			yield return new KeyValuePair<string, string>("Token", Type.MDToken.ToStringRaw());
		}
	}
}
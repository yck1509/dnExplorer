using System;
using System.Collections.Generic;
using System.Text;
using dnlib.DotNet;
using dnlib.Threading;

namespace dnExplorer {
	public class DisplayNameCreator {
		static void CreateDisplayName(StringBuilder sb, RecursionCounter recu, TypeSig sig) {
			if (sig == null) {
				sb.Append("<<<NULL>>>");
				return;
			}
			if (!recu.Increment()) {
				sb.Append("<<<INFRECURSION>>>");
				return;
			}

			switch (sig.ElementType) {
				case ElementType.Void:
				case ElementType.Boolean:
				case ElementType.Char:
				case ElementType.I1:
				case ElementType.U1:
				case ElementType.I2:
				case ElementType.U2:
				case ElementType.I4:
				case ElementType.U4:
				case ElementType.I8:
				case ElementType.U8:
				case ElementType.R4:
				case ElementType.R8:
				case ElementType.String:
				case ElementType.TypedByRef:
				case ElementType.I:
				case ElementType.U:
				case ElementType.Object:
				case ElementType.ValueType:
				case ElementType.Class:
					var type = ((TypeDefOrRefSig)sig).TypeDefOrRef;
					sb.Append(type.Name ?? "<<<NULL>>>");
					break;

				case ElementType.Ptr:
					CreateDisplayName(sb, recu, sig.Next);
					sb.Append('*');
					break;

				case ElementType.ByRef:
					CreateDisplayName(sb, recu, sig.Next);
					sb.Append('&');
					break;

				case ElementType.Array: {
					CreateDisplayName(sb, recu, sig.Next);
					var arraySig = (ArraySig)sig;
					sb.Append('[');
					uint rank = arraySig.Rank;
					if (rank == 0)
						sb.Append("<RANK0>"); // Not allowed
					else if (rank == 1)
						sb.Append('*');
					else
						for (int i = 0; i < (int)rank; i++) {
							if (i != 0)
								sb.Append(", ");

							const int NO_LOWER = int.MinValue;
							const uint NO_SIZE = uint.MaxValue;
							int lower = arraySig.LowerBounds.Get(i, NO_LOWER);
							uint size = arraySig.Sizes.Get(i, NO_SIZE);
							if (lower != NO_LOWER) {
								sb.Append(lower);
								sb.Append("..");
								if (size != NO_SIZE)
									sb.Append(lower + (int)size - 1);
								else
									sb.Append('.');
							}
						}
					sb.Append(']');
					break;
				}

				case ElementType.SZArray:
					CreateDisplayName(sb, recu, sig.Next);
					sb.Append("[]");
					break;

				case ElementType.CModReqd:
					CreateDisplayName(sb, recu, sig.Next);
					sb.Append(" modreq(");
					sb.Append(((ModifierSig)sig).Modifier.Name ?? "<<<NULL>>>");
					sb.Append(")");
					break;

				case ElementType.CModOpt:
					CreateDisplayName(sb, recu, sig.Next);
					sb.Append(" modopt(");
					sb.Append(((ModifierSig)sig).Modifier.Name ?? "<<<NULL>>>");
					sb.Append(")");
					break;

				case ElementType.Pinned:
					CreateDisplayName(sb, recu, sig.Next);
					break;

				case ElementType.ValueArray:
					CreateDisplayName(sb, recu, sig.Next);
					var valueArraySig = (ValueArraySig)sig;
					sb.Append(" ValueArray(");
					sb.Append(valueArraySig.Size);
					sb.Append(')');
					break;

				case ElementType.Module:
					CreateDisplayName(sb, recu, sig.Next);
					var moduleSig = (ModuleSig)sig;
					sb.Append(" Module(");
					sb.Append(moduleSig.Index);
					sb.Append(')');
					break;

				case ElementType.GenericInst: {
					var genericInstSig = (GenericInstSig)sig;
					var typeGenArgs = genericInstSig.GenericArguments;
					CreateDisplayName(sb, recu, genericInstSig.GenericType);
					sb.Append('<');
					int i = -1;
					foreach (var genArg in typeGenArgs.GetSafeEnumerable()) {
						i++;
						if (i != 0)
							sb.Append(", ");
						CreateDisplayName(sb, recu, genArg);
					}
					sb.Append('>');
				}
					break;

				case ElementType.Var:
				case ElementType.MVar:
					var gs = (GenericSig)sig;
					var gp = gs.GenericParam;
					if (gp == null || UTF8String.IsNullOrEmpty(gp.Name)) {
						sb.Append(gs.IsMethodVar ? "!!" : "!");
						sb.Append(gs.Number);
					}
					else
						sb.Append(gp.Name);
					break;

				case ElementType.FnPtr:
					sb.Append("(fnptr)");
					break;

				case ElementType.Sentinel:
					break;

				case ElementType.End:
				case ElementType.R:
				case ElementType.Internal:
				default:
					break;
			}

			recu.Decrement();
		}

		public static string CreateDisplayName(IType type) {
			var sb = new StringBuilder();
			if (type is ExportedType) {
				sb.Append(FullNameCreator.Name((ExportedType)type, false));
				return sb.ToString();
			}
			if (type is TypeSpec)
				type = ((TypeSpec)type).TypeSig;

			if (type is TypeSig) {
				if (type is TypeDefOrRefSig)
					type = ((TypeDefOrRefSig)type).TypeDefOrRef;
				else {
					CreateDisplayName(sb, new RecursionCounter(), (TypeSig)type);
					return sb.ToString();
				}
			}

			if (type is TypeRef) {
				sb.Append(FullNameCreator.Name((TypeRef)type, false));
			}
			else if (type is TypeDef) {
				var typeDef = (TypeDef)type;
				sb.Append(FullNameCreator.Name(typeDef, false));

				if (typeDef.HasGenericParameters) {
					sb.Append('<');
					uint genParamCount = (uint)typeDef.GenericParameters.Count;
					for (uint i = 0; i < genParamCount; i++) {
						if (i != 0) {
							sb.Append(", ");
						}
						sb.Append(FullNameCreator.Name(new GenericVar(i, typeDef), false));
					}
					sb.Append('>');
				}
			}
			else
				throw new NotSupportedException();
			return sb.ToString();
		}

		public static string CreateDisplayName(MethodDef method) {
			var sb = new StringBuilder();
			if (method.MethodSig == null) {
				sb.Append(method.Name ?? "<<<NULL>>>");
			}
			else {
				if (!UTF8String.IsNullOrEmpty(method.Name)) {
					sb.Append(method.Name);
				}
				if (method.MethodSig.Generic) {
					sb.Append('<');
					uint genParamCount = method.MethodSig.GenParamCount;
					for (uint i = 0; i < genParamCount; i++) {
						if (i != 0) {
							sb.Append(", ");
						}
						sb.Append(FullNameCreator.Name(new GenericMVar(i, method), false));
					}
					sb.Append('>');
				}
				sb.Append('(');
				int count = PrintMethodArgList(sb, method.MethodSig.Params, false, false);
				PrintMethodArgList(sb, method.MethodSig.ParamsAfterSentinel, count > 0, true);
				if (method.IsConstructor) {
					sb.Append(")");
				}
				else {
					sb.Append(") : ");
					sb.Append(CreateDisplayName(method.MethodSig.RetType));
				}
			}
			return sb.ToString();
		}

		static int PrintMethodArgList(StringBuilder sb, IEnumerable<TypeSig> args, bool hasPrintedArgs, bool isAfterSentinel) {
			if (args == null) {
				return 0;
			}
			if (isAfterSentinel) {
				if (hasPrintedArgs) {
					sb.Append(", ");
				}
				sb.Append("...");
				hasPrintedArgs = true;
			}
			int count = 0;
			foreach (TypeSig arg in args.GetSafeEnumerable()) {
				count++;
				if (hasPrintedArgs) {
					sb.Append(", ");
				}
				sb.Append(CreateDisplayName(arg));
				hasPrintedArgs = true;
			}
			return count;
		}

		public static string CreateDisplayName(PropertyDef property) {
			var sb = new StringBuilder();
			if (property.PropertySig == null) {
				sb.Append(property.Name ?? "<<<NULL>>>");
			}
			else {
				if (!UTF8String.IsNullOrEmpty(property.Name)) {
					sb.Append(property.Name);
				}
				if (property.PropertySig.Params.Count > 0) {
					sb.Append('[');
					int count = PrintMethodArgList(sb, property.PropertySig.Params, false, false);
					sb.Append(']');
				}
				sb.Append(" : ");
				sb.Append(CreateDisplayName(property.PropertySig.RetType));
			}
			return sb.ToString();
		}

		public static string CreateDisplayName(EventDef evnt) {
			var sb = new StringBuilder();
			if (evnt.EventType == null) {
				sb.Append(evnt.Name ?? "<<<NULL>>>");
			}
			else {
				if (!UTF8String.IsNullOrEmpty(evnt.Name)) {
					sb.Append(evnt.Name);
				}
			}
			return sb.ToString();
		}

		public static string CreateDisplayName(FieldDef field) {
			var sb = new StringBuilder();
			if (field.FieldSig.Type == null) {
				sb.Append(field.Name ?? "<<<NULL>>>");
			}
			else {
				if (!UTF8String.IsNullOrEmpty(field.Name)) {
					sb.Append(field.Name);
				}
				sb.Append(" : ");
				sb.Append(CreateDisplayName(field.FieldSig.Type));
			}
			return sb.ToString();
		}
	}
}
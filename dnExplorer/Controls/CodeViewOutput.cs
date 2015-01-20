using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using dnlib.DotNet.Emit;
using ICSharpCode.Decompiler;
using ICSharpCode.NRefactory;

namespace dnExplorer.Controls {
	public class CodeViewData {
		public struct TextType {
			public readonly int Length;
			public readonly int Type;

			public TextType(int len, int type) {
				Length = len;
				Type = type;
			}
		}

		public struct TextRef {
			public readonly int Length;
			public readonly object Reference;

			const byte FLAG_LOCAL = 1;
			const byte FLAG_DEF = 2;

			readonly byte Flags;

			public bool IsLocal {
				get { return (Flags & FLAG_LOCAL) != 0; }
			}

			public bool IsDefinition {
				get { return (Flags & FLAG_DEF) != 0; }
			}

			public TextRef(int len, object reference, bool isLocal, bool isDef) {
				Length = len;
				Reference = reference;
				Flags = 0;
				if (isLocal)
					Flags |= FLAG_LOCAL;
				if (isDef)
					Flags |= FLAG_DEF;
			}
		}

		public const int TYPE_PLAIN = 0;
		public const int TYPE_KEYWORD = 1;
		public const int TYPE_COMMENT = 2;
		public const int TYPE_LITERAL = 3;
		public const int TYPE_DEF = 4;
		public const int TYPE_DEF_TARGET = 5;
		public const int TYPE_REF = 6;
		public const int TYPE_REF_TARGET = 7;

		public string Code { get; internal set; }
		public Dictionary<int, TextType> Types { get; internal set; }
		public Dictionary<int, TextRef> References { get; internal set; }

		internal CodeViewData() {
		}

		public CodeViewData(string text) {
			Code = text;
			Types = new Dictionary<int, TextType>();
			References = new Dictionary<int, TextRef>();
		}
	}

	public class CodeViewOutput : ITextOutput {
		MemoryStream result = new MemoryStream();
		StreamWriter writer;
		int indent;
		bool needsIndent;

		Dictionary<int, CodeViewData.TextType> types = new Dictionary<int, CodeViewData.TextType>();
		Dictionary<int, CodeViewData.TextRef> refs = new Dictionary<int, CodeViewData.TextRef>();
		int currentType;
		int lastPos;

		public CodeViewOutput() {
			writer = new StreamWriter(result, Encoding.UTF8);
			writer.AutoFlush = true;
		}

		public TextLocation Location {
			get { return new TextLocation(0, (int)result.Position); }
		}

		public void Indent() {
			indent++;
		}

		public void Unindent() {
			indent--;
		}

		void WriteIndent() {
			if (needsIndent) {
				needsIndent = false;
				for (int i = 0; i < indent; i++) {
					SetType(CodeViewData.TYPE_PLAIN);
					writer.Write('\t');
				}
			}
		}

		void SetType(int type) {
			if (currentType != type) {
				Flush();
				currentType = type;
			}
		}

		public void Flush() {
			if (lastPos == (int)result.Position) return;
			types.Add(lastPos, new CodeViewData.TextType((int)result.Position - lastPos, currentType));
			lastPos = (int)result.Position;
		}

		public void Write(char ch) {
			WriteIndent();
			SetType(CodeViewData.TYPE_PLAIN);
			writer.Write(ch);
		}

		public void Write(string text) {
			WriteIndent();
			SetType(CodeViewData.TYPE_PLAIN);
			writer.Write(text);
		}

		public void WriteLine() {
			SetType(CodeViewData.TYPE_PLAIN);
			writer.WriteLine();
			needsIndent = true;
		}

		public void WriteDefinition(string text, object definition, bool isLocal) {
			WriteIndent();
			Debug.Assert(definition != null);
			SetType(isLocal ? CodeViewData.TYPE_DEF : CodeViewData.TYPE_DEF_TARGET);

			int pos = (int)result.Position;
			writer.Write(text);
			refs.Add(pos, new CodeViewData.TextRef(text.Length, definition, isLocal, true));
		}

		public void WriteReference(string text, object reference, bool isLocal) {
			WriteIndent();
			Debug.Assert(reference != null);
			bool isOpCode = reference is OpCode;

			SetType(isOpCode ? CodeViewData.TYPE_REF : CodeViewData.TYPE_REF_TARGET);

			int pos = (int)result.Position;
			writer.Write(text);
			if (!isOpCode)
				refs.Add(pos, new CodeViewData.TextRef(text.Length, reference, isLocal, false));
		}

		public void WriteKeyword(string text) {
			WriteIndent();
			SetType(CodeViewData.TYPE_KEYWORD);
			writer.Write(text);
		}

		public void WriteComment(string text) {
			WriteIndent();
			SetType(CodeViewData.TYPE_COMMENT);
			writer.Write(text);
		}

		public void WriteLiteral(string text) {
			WriteIndent();
			SetType(CodeViewData.TYPE_LITERAL);
			writer.Write(text);
		}

		public CodeViewData GetResult() {
			Flush();
			return new CodeViewData {
				Code = Encoding.UTF8.GetString(result.ToArray()),
				Types = types,
				References = refs
			};
		}

		void ITextOutput.MarkFoldStart(string collapsedText, bool defaultCollapsed) {
		}

		void ITextOutput.MarkFoldEnd() {
		}

		void ITextOutput.AddDebuggerMemberMapping(MemberMapping memberMapping) {
		}
	}
}
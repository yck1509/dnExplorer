using System;
using System.Threading;
using dnExplorer.Controls;
using dnlib.DotNet;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Disassembler;

namespace dnExplorer.Language {
	public class CILLanguage : ILanguage {
		public string Name {
			get { return "IL"; }
		}

		void DoDisassemble(IDnlibDef item, ITextOutput output, ReflectionDisassembler disassembler) {
			if (item is ModuleDef) {
				var module = (ModuleDef)item;
				disassembler.WriteAssemblyReferences(module);
				if (module.Assembly != null)
					disassembler.WriteAssemblyHeader(module.Assembly);
				output.WriteLine();
				disassembler.WriteModuleHeader(module);
			}
			else if (item is TypeDef) {
				disassembler.DisassembleType((TypeDef)item);
			}
			else if (item is MethodDef) {
				disassembler.DisassembleMethod((MethodDef)item);
			}
			else if (item is FieldDef) {
				disassembler.DisassembleField((FieldDef)item);
			}
			else if (item is PropertyDef) {
				disassembler.DisassembleProperty((PropertyDef)item);
			}
			else if (item is EventDef) {
				disassembler.DisassembleEvent((EventDef)item);
			}
		}

		public CodeViewData Run(IDnlibDef item, CancellationToken token) {
			try {
				var output = new CodeViewOutput();
				var disassembler = new ReflectionDisassembler(output, true, token);
				DoDisassemble(item, output, disassembler);
				return output.GetResult();
			}
			catch {
				// Retry if ILStructure failed.
				var output = new CodeViewOutput();
				output.WriteComment("// ILStructure failed!");
				output.WriteLine();
				var disassembler = new ReflectionDisassembler(output, false, token);
				DoDisassemble(item, output, disassembler);
				return output.GetResult();
			}
		}
	}
}
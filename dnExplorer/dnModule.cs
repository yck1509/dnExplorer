﻿using System;
using System.IO;
using dnlib.DotNet;
using dnlib.DotNet.MD;
using dnlib.IO;
using dnlib.PE;
using SR = System.Reflection;

namespace dnExplorer {
	public class dnModule {
		class dnModuleStreamCreator : IImageStreamCreator {
			byte[] data;

			public dnModuleStreamCreator(byte[] data, string fileName) {
				this.data = data;
				FileName = fileName;
			}

			public string FileName { get; set; }

			public long Length {
				get { return data.Length; }
			}

			public IImageStream Create(FileOffset offset, long length) {
				if (offset < 0 || length < 0)
					return MemoryImageStream.CreateEmpty();

				int offs = (int)Math.Min(data.Length, (long)offset);
				int len = (int)Math.Min((long)data.Length - offs, length);
				return new MemoryImageStream(offset, data, offs, len);
			}

			public IImageStream CreateFull() {
				return new MemoryImageStream(0, data, 0, data.Length);
			}

			public void Dispose() {
				data = null;
				FileName = null;
			}
		}

		static SR.MethodInfo loadInternal = typeof(ModuleDefMD)
			.GetMethod("Load", SR.BindingFlags.NonPublic | SR.BindingFlags.Static);

		public dnModule(string fileName)
			: this(File.ReadAllBytes(fileName), fileName) {
		}

		public dnModule(byte[] module, string fileName) {
			RawData = module;
			ErrorMessage = null;
			Name = Path.GetFileName(fileName);

			var creator = new dnModuleStreamCreator(module, fileName);
			try {
				Image = new PEImage(creator, ImageLayout.File, true);
			}
			catch (Exception ex) {
				ErrorMessage += string.Format("Error while loading PE Image:{0}{1}{0}{0}",
					Environment.NewLine, ex);

				Image = null;
				return;
			}

			try {
				MetaData = MetaDataCreator.CreateMetaData(Image);
			}
			catch (Exception ex) {
				ErrorMessage += string.Format("Error while loading MetaData:{0}{1}{0}{0}",
					Environment.NewLine, ex);

				MetaData = null;
				return;
			}

			try {
				ModuleDef = (ModuleDefMD)loadInternal.Invoke(null, new object[] { MetaData, null });
			}
			catch (Exception ex) {
				ErrorMessage = string.Format("Error while loading ModuleDef:{0}{1}{0}{0}",
					Environment.NewLine, ex);

				ModuleDef = null;
				return;
			}

			Name = ModuleDef.Name;
		}

		public byte[] RawData { get; private set; }
		public IPEImage Image { get; private set; }
		public IMetaData MetaData { get; private set; }
		public ModuleDefMD ModuleDef { get; private set; }

		public string Name { get; private set; }
		public string ErrorMessage { get; private set; }
	}
}
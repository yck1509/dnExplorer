using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ScintillaNET;

namespace dnExplorer {
	public interface IHasInfo {
		string Header { get; }
		IEnumerable<KeyValuePair<string, string>> GetInfos();
	}

	public class InfoPanel : Control {
		Scintilla content;

		public InfoPanel() {
			content = new Scintilla();

			//content.BorderStyle = BorderStyle.None;
			content.Folding.IsEnabled = false;
			foreach (var i in content.Margins)
				i.Width = 0;

			content.BackColor = BackColor = Color.FromArgb(0xff, 0xff, 0xe1);

			content.Styles[1].Font = Font; // Header
			content.Styles[1].Size = 10;
			content.Styles[1].BackColor = BackColor;
			content.Styles[1].ForeColor = Color.FromArgb(0x00, 0x00, 0x00);

			content.Styles[2].Font = Font; // Name
			content.Styles[2].Size = 9;
			content.Styles[2].BackColor = BackColor;
			content.Styles[2].ForeColor = Color.FromArgb(0x00, 0x00, 0x00);
			content.Styles[2].Bold = true;

			content.Styles[3].Font = Font; // Value
			content.Styles[3].Size = 9;
			content.Styles[3].BackColor = BackColor;
			content.Styles[3].ForeColor = Color.FromArgb(0x00, 0x00, 0x00);

			content.Scrolling.HorizontalScrollTracking = true;

			content.Selection.BackColorUnfocused = content.Selection.BackColor;
			content.Selection.ForeColorUnfocused = content.Selection.ForeColor;

			const uint SCI_SETEXTRAASCENT = 2525;
			const uint SCI_SETEXTRADESCENT = 2527;
			content.NativeInterface.SendMessageDirect(SCI_SETEXTRAASCENT, 2);
			content.NativeInterface.SendMessageDirect(SCI_SETEXTRADESCENT, 3);

			content.IsReadOnly = true;

			Controls.Add(content);

			Width = Height = 100;
			content.SetBounds(0, 0, Width, Height);
			content.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
		}

		public void SetInfo(IHasInfo info) {
			content.IsReadOnly = false;

			var header = info.Header;
			var infos = info.GetInfos().ToList();

			int indent = 0;
			var hls = new List<Tuple<long, long, int>>();
			long prev = 0;
			var ms = new MemoryStream();
			using (var bFont = new Font(content.Font, FontStyle.Bold))
			using (var writer = new StreamWriter(ms, Encoding.UTF8)) {
				writer.AutoFlush = true;

				writer.Write(header);
				hls.Add(Tuple.Create(prev, ms.Position, 1));
				prev = ms.Position;

				foreach (var entry in infos) {
					writer.WriteLine();
					prev = ms.Position;

					writer.Write(entry.Key + ":\t");
					hls.Add(Tuple.Create(prev, ms.Position, 2));
					prev = ms.Position;

					writer.Write(entry.Value);
					hls.Add(Tuple.Create(prev, ms.Position, 3));

					var nameWidth = TextRenderer.MeasureText(entry.Key, bFont).Width;
					if (nameWidth > indent)
						indent = nameWidth;
				}
			}
			content.Text = Encoding.UTF8.GetString(ms.ToArray());

			for (int i = 0; i < infos.Count; i++) {
				const int SCI_CLEARTABSTOPS = 2675;
				const int SCI_ADDTABSTOP = 2676;
				content.NativeInterface.SendMessageDirect(SCI_CLEARTABSTOPS, i + 1);
				content.NativeInterface.SendMessageDirect(SCI_ADDTABSTOP, i + 1, indent + 20);
			}

			foreach (var hlEntry in hls)
				content.GetRange((int)hlEntry.Item1, (int)hlEntry.Item2).SetStyle(hlEntry.Item3);

			var width = TextRenderer.MeasureText(content.Text, content.Font).Width;
			content.Scrolling.HorizontalScrollWidth = width + 10;

			content.IsReadOnly = true;
		}

		public void Clear() {
			content.IsReadOnly = false;
			content.Text = "";
			content.IsReadOnly = true;
		}
	}
}
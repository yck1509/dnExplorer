using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace dnExplorer.Controls {
	internal class HexViewerContextMenu : ContextMenuStrip {
		HexViewer hexView;

		ToolStripMenuItem copy;
		ToolStripMenuItem copyBeginOffset;
		ToolStripMenuItem copyEndOffset;
		ToolStripMenuItem copySize;
		ToolStripMenuItem copyValue;
		ToolStripMenuItem copyHex;
		ToolStripMenuItem selAll;
		ToolStripMenuItem gotoOffset;

		public HexViewerContextMenu(HexViewer hexView) {
			this.hexView = hexView;
			InitializeItems();
		}

		void InitializeItems() {
			copy = new ToolStripMenuItem("Copy");
			Items.Add(copy);

			copyBeginOffset = new ToolStripMenuItem("Begin Offset");
			copyBeginOffset.Click += DoCopyBeginOffset;
			copy.DropDownItems.Add(copyBeginOffset);

			copyEndOffset = new ToolStripMenuItem("End Offset");
			copyEndOffset.Click += DoCopyEndOffset;
			copy.DropDownItems.Add(copyEndOffset);

			copySize = new ToolStripMenuItem("Size");
			copySize.Click += DoCopySize;
			copy.DropDownItems.Add(copySize);

			copy.DropDownItems.Add(new ToolStripSeparator());

			copyValue = new ToolStripMenuItem("Value");
			copyValue.Click += DoCopyValue;
			copy.DropDownItems.Add(copyValue);

			copyHex = new ToolStripMenuItem("Hex");
			copyHex.Click += DoCopyHex;
			copy.DropDownItems.Add(copyHex);

			Items.Add(new ToolStripSeparator());

			selAll = new ToolStripMenuItem("Select All");
			selAll.Click += DoSelectAll;
			Items.Add(selAll);

			Items.Add(new ToolStripSeparator());

			gotoOffset = new ToolStripMenuItem("Go To Offset...");
			gotoOffset.Click += DoGoToOffset;
			Items.Add(gotoOffset);
		}

		void UpdateItems() {
			copy.Enabled = hexView.HasSelection;
		}

		protected override void OnOpening(CancelEventArgs e) {
			UpdateItems();
			base.OnOpening(e);
		}

		void DoSelectAll(object sender, EventArgs e) {
			hexView.SelectionStart = 0;
			hexView.SelectionEnd = hexView.Stream.Length - 1;
		}

		void DoGoToOffset(object sender, EventArgs e) {
			var result = InputBox.Show("Go To Offset", "Target offset:");
			if (result == null)
				return;

			var offset = Utils.ParseInputNum(result);
			if (offset == null) {
				MessageBox.Show("Invalid number.", "Go To Offset", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (offset.Value >= hexView.Stream.Length) {
				MessageBox.Show("Offset out of range.", "Go To Offset", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			hexView.Select(offset.Value);
		}

		void DoCopyBeginOffset(object sender, EventArgs e) {
			var offset = ((uint)hexView.SelectionStart).ToString("X8");
			Clipboard.SetText(offset);
		}

		void DoCopyEndOffset(object sender, EventArgs e) {
			var offset = ((uint)hexView.SelectionEnd).ToString("X8");
			Clipboard.SetText(offset);
		}

		void DoCopySize(object sender, EventArgs e) {
			var size = ((uint)(hexView.SelectionEnd - hexView.SelectionStart)).ToString("X8");
			Clipboard.SetText(size);
		}

		void DoCopyValue(object sender, EventArgs e) {
			var data = hexView.GetSelection();
			var dataObj = new DataObject();
			dataObj.SetData(Main.AppName + " Binary", true, new MemoryStream(hexView.GetSelection()));
			Clipboard.SetDataObject(dataObj, true);
		}

		void DoCopyHex(object sender, EventArgs e) {
			var buff = hexView.GetSelection();
			var sb = new StringBuilder();
			for (int i = 0; i < buff.Length; i++) {
				if (i % 8 == 0 && i != 0)
					sb.AppendFormat(" {0:X2}", buff[i]);
				else
					sb.AppendFormat("{0:X2}", buff[i]);
			}
			Clipboard.SetText(sb.ToString());
		}
	}
}
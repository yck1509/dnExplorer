using System;
using System.Drawing;

namespace dnExplorer.Trees {
	public class ErrorModel : DataModel {
		public string Message { get; set; }

		public ErrorModel(string message) {
			Message = message;
			Text = "Error!";
		}

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
		}

		public override void Refresh(bool refreshChildren) {
		}
	}
}
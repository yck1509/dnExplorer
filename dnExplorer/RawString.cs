using System;

namespace dnExplorer {
	internal class RawString {
		readonly string value;

		public RawString(string value) {
			this.value = value;
		}

		public override string ToString() {
			return value;
		}
	}
}
namespace System {
	abstract class ACSVable {
		protected string CreateData(string[] text, char spacer) {
			string ret = "";
			foreach (var s in text)
				ret += s + spacer + ' ';
			return ret.Substring(0, ret.Length - 2);
		}
		public abstract string ToCSV(char spacer);
		public abstract string GetHeader(char spacer);
	}
}

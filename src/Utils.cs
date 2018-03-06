using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcerFanControl {

	public static class Utils {

		public static Int32 ParseInt32(string str, Int32 @default = 0) {
			if (String.IsNullOrEmpty(str)) { return @default; }
			if (str.Length > 2 && str[ 0 ] == '0') {
				char ch = str[1];
				if (ch == 'x' || ch == 'X') { return Convert.ToInt32(str.Substring(2), 16); }
				if (ch == 'b' || ch == 'b') { return Convert.ToInt32(str.Substring(2), 2); }
			}
			return Convert.ToInt32(str);
		}

		public static Int16 ParseInt16(string str, Int16 @default = 0) {
			if (String.IsNullOrEmpty(str)) { return @default; }
			if (str.Length > 2 && str[ 0 ] == '0') {
				char ch = str[1];
				if (ch == 'x' || ch == 'X') { return Convert.ToInt16(str.Substring(2), 16); }
				if (ch == 'b' || ch == 'b') { return Convert.ToInt16(str.Substring(2), 2); }
			}
			return Convert.ToInt16(str);
		}

		public static UInt16 ParseUInt16(string str, UInt16 @default = 0) {
			if (String.IsNullOrEmpty(str)) { return @default; }
			if (str.Length > 2 && str[ 0 ] == '0') {
				char ch = str[1];
				if (ch == 'x' || ch == 'X') { return Convert.ToUInt16(str.Substring(2), 16); }
				if (ch == 'b' || ch == 'b') { return Convert.ToUInt16(str.Substring(2), 2); }
			}
			return Convert.ToUInt16(str);
		}

		public static byte ParseByte(string str, byte @default = 0) {
			if (String.IsNullOrEmpty(str)) { return @default; }
			if (str.Length > 2 && str[ 0 ] == '0') {
				char ch = str[1];
				if (ch == 'x' || ch == 'X') { return Convert.ToByte(str.Substring(2), 16); }
				if (ch == 'b' || ch == 'b') { return Convert.ToByte(str.Substring(2), 2); }
			}
			return Convert.ToByte(str);
		}

	}
}

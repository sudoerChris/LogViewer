using System;
using System.Runtime.InteropServices;

namespace LogViewer {
	internal class Utility {
		[DllImport("dwmapi.dll")]
		private static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);
		public static bool SetDarkMode(IntPtr handle, bool enabled) {
			int pvAttribute = enabled ? 1 : 0;
			return DwmSetWindowAttribute(handle, 20, ref pvAttribute, sizeof(int)) == 0;
		}
	}
}

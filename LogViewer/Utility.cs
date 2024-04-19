using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace LogViewer {
	internal class Utility {
		[DllImport("dwmapi.dll")]
		private static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);
		public static bool SetDarkMode(IntPtr handle, bool enabled) {
			int pvAttribute = enabled ? 1 : 0;
			return DwmSetWindowAttribute(handle, 20, ref pvAttribute, sizeof(int)) == 0;
		}
		private static readonly char[] invalidChars = Path.GetInvalidPathChars();
		public static bool HasInvalidPathCharacters(string path) {
			foreach (char c in path) {
				if (Array.IndexOf(invalidChars, c) >= 0) {
					return true;
				}
			}
			return false;
		}
		public static bool DirectoryExist(string path) {
			bool exist = false;
			try {
				Directory.GetCreationTime(path);
				exist = true;
			}
			catch (UnauthorizedAccessException) {
				exist = true;
			}
			catch { }
			return exist;
		}
		public static bool DirectoryAuthorized(string path) {
			bool auth = false;
			try {
				Directory.GetCreationTime(path);
				auth = true;
			}
			catch (UnauthorizedAccessException) {
				auth = false;
			}
			catch { }
			return auth;
		}
		#region win32 call
		[DllImport("user32.dll")]
		public static extern IntPtr SendMessage(IntPtr hWnd, Int32 wMsg, Int32 wParam, ref Point lParam);

		[DllImport("user32.dll")]
		public static extern IntPtr SendMessage(IntPtr hWnd, Int32 wMsg, Int32 wParam, IntPtr lParam);
		public const int WM_USER = 0x400;
		public const int WM_SETREDRAW = 0x000B;
		public const int WM_PAINT = 0x000F;
		public const int EM_GETEVENTMASK = WM_USER + 59;
		public const int EM_SETEVENTMASK = WM_USER + 69;
		#endregion win32 call
	}
}

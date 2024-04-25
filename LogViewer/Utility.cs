using Microsoft.Win32;
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

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

		// workaround for control created outside UI thread
		// otherwise main thread will try to send event to other thread while that thread is waiting to invoke main thread, lol
		public static void UnsubscribeInvalidSystemEvents() {
			try {
				var handlers = typeof(SystemEvents).GetField("_handlers", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
				var handlersValues = handlers.GetType().GetProperty("Values").GetValue(handlers);
				foreach (var invokeInfos in (handlersValues as IEnumerable).OfType<object>().ToArray())
					foreach (var invokeInfo in (invokeInfos as IEnumerable).OfType<object>().ToArray()) {
						var syncContext = invokeInfo.GetType().GetField("_syncContext", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(invokeInfo);
						if (syncContext == null)
							throw new Exception("syncContext missing");
						if (!(syncContext is WindowsFormsSynchronizationContext))
							continue;
						var threadRef = (WeakReference)syncContext.GetType().GetField("destinationThreadRef", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(syncContext);
						if (!threadRef.IsAlive)
							continue;
						var thread = (System.Threading.Thread)threadRef.Target;
						if (thread.ManagedThreadId == 1)
							continue;  // Change here if you have more valid UI threads to ignore
						var dlg = (Delegate)invokeInfo.GetType().GetField("_delegate", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(invokeInfo);
						var handler = (UserPreferenceChangedEventHandler)Delegate.CreateDelegate(typeof(UserPreferenceChangedEventHandler), dlg.Target, dlg.Method.Name);
						SystemEvents.UserPreferenceChanged -= handler;
					}
			}
			catch {
				//trace here your errors
			}
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

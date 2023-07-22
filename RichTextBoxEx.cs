﻿using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogViewer {
	internal class RichTextBoxEx : RichTextBox {
		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, Int32 wMsg, Int32 wParam, ref Point lParam);

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, Int32 wMsg, Int32 wParam, IntPtr lParam);

		private const int WM_USER = 0x400;
		private const int WM_SETREDRAW = 0x000B;
		private const int EM_GETEVENTMASK = WM_USER + 59;
		private const int EM_SETEVENTMASK = WM_USER + 69;
		
		private SCROLLINFO suspendScrollInfoH, suspendScrollInfoV;
		private bool writing = false;
		private bool suspended = false;
		private IntPtr _EventMask;
		private int suspendSelStart = 0;
		private int suspendSelLength = 0;
		private bool KeepScrollPosAfterWrite = false;
		public void BeginWrite(bool bufferedDrawing = false) {
			if (writing) return;
			writing = true;
			KeepScrollPosAfterWrite = SelectionStart != TextLength;
			_EventMask = SendMessage(Handle, EM_GETEVENTMASK, 0, IntPtr.Zero);
			suspendSelStart = SelectionStart != TextLength ? SelectionStart : -1;
			suspendSelLength = SelectionLength;
			suspendScrollInfoH = GetWin32ScrollInfo(SBOrientation.SB_HORZ);
			if (KeepScrollPosAfterWrite) {
				suspendScrollInfoV = GetWin32ScrollInfo(SBOrientation.SB_VERT);
			}
			if (bufferedDrawing || KeepScrollPosAfterWrite) {
				SuspendPainting();
			}
		}
		public void EndWrite() {
			if (!writing) return;
			if (suspendSelStart == -1)
				Select(TextLength, 0);
			else
				Select(suspendSelStart, suspendSelLength);
			SendMessage(Handle, EM_SETEVENTMASK, 0, _EventMask);
			if (KeepScrollPosAfterWrite) {
				SetScrollPos(suspendScrollInfoH.nPos, suspendScrollInfoV.nPos);
			}
			else {
				SetScrollPos(suspendScrollInfoH.nPos, GetWin32ScrollInfo(SBOrientation.SB_VERT).nPos);
			}
			ResumePainting();
			writing = false;
		}


		private void SuspendPainting() {
			if (suspended) return;
			suspended = true;
			SendMessage(Handle, WM_SETREDRAW, 0, IntPtr.Zero);
		}
		private void ResumePainting() {
			if (!suspended) return;
			SendMessage(Handle, WM_SETREDRAW, 1, IntPtr.Zero);
			Invalidate();
			suspended = false;
		}

		public void RemoveFirst(int count) {
			int selStart = (suspended ? suspendSelStart : SelectionStart) - count;
			int curSelLength = SelectionLength;
			if (selStart < 0) {
				selStart = 0;
				curSelLength = 0;
			}
			int topLeftCharIndex = GetCharIndexFromPosition(new Point(0, 0));
			Select(0, count);
			SelectedText = "";
			if (suspended) {
				suspendSelStart = selStart;
				suspendSelLength = curSelLength;
			}
			else {
				Select(selStart, curSelLength);
			}
		}
		protected override void OnGotFocus(EventArgs e) {
			// prevent rescroll to cursor when set focus
			SetScrollPos(GetWin32ScrollInfo(SBOrientation.SB_HORZ).nPos,GetWin32ScrollInfo(SBOrientation.SB_VERT).nPos);
		}
		#region scroll
		[DllImport("user32.dll")]
		private extern static int GetWindowLong(IntPtr hWnd, int index);

		[DllImport("user32.dll")]
		private static extern bool GetScrollInfo(IntPtr hwnd, SBOrientation fnBar,
				ref SCROLLINFO lpsi);

		private const int EM_SETSCROLLPOS = WM_USER + 222;
		private const int WS_VSCROLL = 0x00200000;
		private const int WS_HSCROLL = 0x00100000;
		public enum ScrollInfoMask : uint {
			SIF_RANGE = 0x1,
			SIF_PAGE = 0x2,
			SIF_POS = 0x4,
			SIF_DISABLENOSCROLL = 0x8,
			SIF_TRACKPOS = 0x10,
			SIF_ALL = (SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS),
		}
		[StructLayout(LayoutKind.Sequential)]
		public struct SCROLLINFO {
			public int cbSize;
			public ScrollInfoMask fMask;
			public int nMin;
			public int nMax;
			public uint nPage;
			public int nPos;
			public int nTrackPos;
		}
		public SCROLLINFO newSCROLLINFO() {
			return new SCROLLINFO() {
				cbSize = Marshal.SizeOf<SCROLLINFO>(),
				fMask = ScrollInfoMask.SIF_ALL,
				nMin = 0,
				nMax = 0,
				nPage = 0,
				nPos = 0,
				nTrackPos = 0
			};
		}
		public enum SBOrientation : int { SB_HORZ = 0x0, SB_VERT = 0x1 }
		public bool VScrollBarVisible() {
			int style = GetWindowLong(Handle, -16);
			return (style & WS_VSCROLL) != 0;
		}
		public bool HScrollBarVisible() {
			int style = GetWindowLong(Handle, -16);
			return (style & WS_HSCROLL) != 0;
		}
		public SCROLLINFO GetWin32ScrollInfo(SBOrientation orientation) {
			var info = newSCROLLINFO();
			if(orientation == SBOrientation.SB_HORZ && HScrollBarVisible()
				|| orientation == SBOrientation.SB_VERT && VScrollBarVisible())
				GetScrollInfo(Handle, orientation, ref info);
			return info;
		}
		public bool ScrollReachBottom {
			get {
				SCROLLINFO info = GetWin32ScrollInfo(SBOrientation.SB_VERT);
				return info.nPos >= info.nMax - info.nPage;
			}
		}
		public void ScrollToBottom() {
			if (suspended) return;
			SCROLLINFO info = GetWin32ScrollInfo(SBOrientation.SB_VERT);
			SetScrollPos(GetWin32ScrollInfo(SBOrientation.SB_HORZ).nPos, (int)(info.nMax - info.nPage));
		}

		private void SetScrollPos(int x, int y) {
			Point p = new Point(x, y);
			SendMessage(Handle, EM_SETSCROLLPOS, 0, ref p);
		}
		#endregion scroll
	}
}

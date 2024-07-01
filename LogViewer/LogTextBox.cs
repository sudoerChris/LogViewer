using System;
using System.Drawing;
using System.Windows.Forms;

public partial class LogTextBox : UserControl {
	// use multiple RTBs to allow pausing and keep rendering on the other RTB
	private readonly RichTextBoxEx[] RTBs = new RichTextBoxEx[2];
	private int CurDisplayRTB = 0, CurWriteRTB = 0;
	public LogTextBox() {
		InitializeComponent();
		InitializeDynamicComponent();
	}

	private void InitializeDynamicComponent() {
		for (int i = 0; i < RTBs.Length; i++) {
			RTBs[i] = new RichTextBoxEx {
				BackColor = Color.Black,
				BorderStyle = BorderStyle.None,
				CausesValidation = false,
				DetectUrls = false,
				Dock = DockStyle.Fill,
				Font = new Font("Consolas", 9F),
				ForeColor = Color.White,
				HideSelection = false,
				Location = new Point(0, 0),
				Margin = new Padding(0),
				Name = $"mainLogText{i}",
				ReadOnly = true,
				Size = Size,
				Visible = false,
			};
			this.Controls.Add(RTBs[i]);
		}
		RTBs[0].Visible = true;
	}
	#region config

	override public Font Font { get { return RTBs[0].Font; } set { foreach (var t in RTBs) { t.Font = value; } } }
	public bool WordWrap { get { return RTBs[0].WordWrap; } set { foreach (var t in RTBs) { t.WordWrap = value; } } }
	#endregion config

	public void ScrollToBottom() {
		if (CurDisplayRTB != CurWriteRTB) {
			RTBs[CurWriteRTB].Visible = true;
			RTBs[CurDisplayRTB].Visible = false;
			RTBs[CurDisplayRTB].Clear();
			CurDisplayRTB = CurWriteRTB;
		}
		RTBs[CurDisplayRTB].ScrollToBottom();
	}

	public void Clear() {
		if (CurDisplayRTB != CurWriteRTB) {
			RTBs[CurWriteRTB].Visible = true;
			RTBs[CurDisplayRTB].Visible = false;
			RTBs[CurDisplayRTB].Clear();
			CurDisplayRTB = CurWriteRTB;
		}
		RTBs[CurDisplayRTB].Clear();
	}

	public int Find(string str) {
		return RTBs[CurDisplayRTB].Find(str);
	}
	public void FindNext(string str) {
		RTBs[CurDisplayRTB].FindNext(str);
	}
	public void FindPrevious(string str) {
		RTBs[CurDisplayRTB].FindPrevious(str);
	}
	public void WriteLine(string msg, Color? c = null) {
		RTBs[CurWriteRTB].WriteLine(msg, c);
	}
	public void WriteRtf(string rtf, bool removeLastNewLine = false) {
		RTBs[CurWriteRTB].WriteRtf(rtf, removeLastNewLine);
		if (CurDisplayRTB == CurWriteRTB && RTBs[CurWriteRTB].KeepScrollPosAfterWrite && RTBs[CurWriteRTB].TextLength > 500_000) {
			// pause current RTB, keep displaying it but switch to another RTB for rendering
			RTBs[CurWriteRTB ^ 1].Rtf = RTBs[CurWriteRTB].Rtf;
			RTBs[CurWriteRTB ^ 1].ScrollToBottom();
			CurWriteRTB ^= 1;
		}
	}
}

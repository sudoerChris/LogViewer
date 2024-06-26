﻿using System.Drawing;
using System.IO;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace LogViewer {
	public partial class MainForm : Form {
		Version appVersion = Assembly.GetExecutingAssembly().GetName().Version;
		private readonly ConfigManager configManager = new ConfigManager();
		private LogFileWatcher watcher;
		private readonly Regex invalidPathChar = new Regex("<|>|:|\"|/|\\\\|\\||\\?|\\*", RegexOptions.Compiled | RegexOptions.Singleline);
		private readonly Regex invalidNonWildcardPathChar = new Regex("<|>|:|\"|/|\\\\|\\|", RegexOptions.Compiled | RegexOptions.Singleline);
		private LogReader logReader;
		private List<HighlightItem> highlightItems = new List<HighlightItem>();
		public MainForm() {
			Utility.SetDarkMode(Handle, true);
			InitializeComponent();
			InitComponent();
			InitSettings();
		}

		#region config
		private void InitSettings() {
			if (Environment.GetCommandLineArgs().Length > 1) {
				LoadConfig(Environment.GetCommandLineArgs()[1]);
			}
			else if (File.Exists("def.LogViewerConfig")) {
				LoadConfig("def.LogViewerConfig");
			}
			else { //default value
				SetFont(mainLogText.Font);
			}
			UpdateIncludeRegex();
			UpdateExcludeRegex();
			UpdateCondition();
		}
		private void InitComponent() {
			Text = $"LogViewer {appVersion.Major}.{appVersion.Minor}.{appVersion.Build}";
			logReader = new LogReader(mainLogText);
			includeTextBox.LostFocus += (s, e) => { UpdateIncludeRegex(); };
			excludeTextBox.LostFocus += (s, e) => { UpdateExcludeRegex(); };
			fontDialog1.Apply += FontDialog1_Apply;
			folderTextBox.LostFocus += pathTextbox_LostFocus;
			filenameTextbox.LostFocus += pathTextbox_LostFocus;
			highlightDataGrid.CellDoubleClick += highlightDataGrid_CellDoubleClick;
			highlightDataGrid.CellValueChanged += highlightDataGrid_CellValueChanged;
			highlightDataGrid.EditingControlShowing += highlightDataGrid_EditingControlShowing;
			highlightDataGrid.LostFocus += highlightDataGrid_LostFocus;
			highlightDataGrid.KeyUp += HighlightDataGrid_KeyUp;
			int[] win32DefColors = new int[defaultHighlightBackColors.Length];
			for (int i = 0; i < defaultHighlightBackColors.Length; i++) {
				win32DefColors[i] = ColorTranslator.ToWin32(defaultHighlightBackColors[i]);
			}
			colorDialog1.CustomColors = win32DefColors;
			saveFileDialog1.AddExtension = true;
			saveFileDialog1.DefaultExt = "LogViewerConfig";
			saveFileDialog1.Title = "Save Config";
			saveFileDialog1.Filter = "Config (*.LogViewerConfig)|*.LogViewerConfig";
			watcher = new LogFileWatcher();
			watcher.TargetChangedHandler += Watcher_TargetChanged;
			watcher.ContentChangedHandler += (s, e) => { UpdateFileContent(); };
			mainLogText.KeyUp += mainLogText_KeyUp;
			findTextbox.KeyUp += findTextbox_KeyUp;
			alwaysOnTopCb.CheckedChanged += (s, e) => { TopMost = alwaysOnTopCb.Checked; };
			scrollToBottomBtn.Click += (s, e) => { mainLogText.ScrollToBottom(); };
			clearBtn.Click += (s, e) => { mainLogText.Clear(); };
			wordWrapCb.CheckedChanged += (s, e) => { mainLogText.WordWrap = wordWrapCb.Checked; };
			saveBtn.Click += (s, e) => { SaveConfig(); };
			bufferedDrawCb.CheckedChanged += (s, e) => { logReader.EnqueueMsg(LogReader.MessageID.SetBufferedPrinting, bufferedDrawCb.Checked); };
			sizeLimitInput.LostFocus += (s, e) => { logReader.EnqueueMsg(LogReader.MessageID.UpdateSizeLimit, (int)sizeLimitInput.Value); };
			Utility.UnsubscribeInvalidSystemEvents();
#if DEBUG
			testBtn.Visible = true;
#endif
		}
		public void SaveConfig() {
			configManager.config.FolderPath = folderTextBox.Text;
			configManager.config.Filename = filenameTextbox.Text;
			configManager.config.FilenameIsRegex = filenameRegexCb.Checked;
			configManager.config.Include = includeTextBox.Text;
			configManager.config.Exclude = excludeTextBox.Text;
			configManager.config.SizeLimit = (int)sizeLimitInput.Value;
			configManager.config.Persistent = persistentCb.Checked;
			configManager.config.BufferedDraw = bufferedDrawCb.Checked;
			configManager.config.WordWrap = wordWrapCb.Checked;
			configManager.config.AlwaysOnTop = alwaysOnTopCb.Checked;
			configManager.config.FontFamilyName = mainLogText.Font.FontFamily.Name;
			configManager.config.FontSize = mainLogText.Font.Size;
			configManager.config.FontStyle = (int)mainLogText.Font.Style;
			configManager.config.HighlightItems = new List<HighlightItem>(highlightItems);
			configManager.config.X = Location.X;
			configManager.config.Y = Location.Y;
			configManager.config.W = Size.Width;
			configManager.config.H = Size.Height;
			if (configManager.path != null) {
				saveFileDialog1.InitialDirectory = Path.GetDirectoryName(configManager.path);
				saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(configManager.path);
			}
			else {
				saveFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
				saveFileDialog1.FileName = invalidPathChar.Replace(filenameTextbox.Text, "");
			}
			DialogResult result = saveFileDialog1.ShowDialog();
			if (result == DialogResult.OK) {
				configManager.path = saveFileDialog1.FileName;
				configManager.SaveConfig();
			}
		}
		public bool LoadConfig(string path) {
			configManager.path = path;
			if (!configManager.LoadConfig()) {
				mainLogText.WriteLine("Read Config File Error!", Color.Red);
				return false;
			}
			folderTextBox.Text = configManager.config.FolderPath;
			filenameTextbox.Text = configManager.config.Filename;
			filenameRegexCb.Checked = configManager.config.FilenameIsRegex;
			includeTextBox.Text = configManager.config.Include;
			excludeTextBox.Text = configManager.config.Exclude;
			sizeLimitInput.Value = configManager.config.SizeLimit;
			logReader.EnqueueMsg(LogReader.MessageID.UpdateSizeLimit, configManager.config.SizeLimit);
			persistentCb.Checked = configManager.config.Persistent;
			wordWrapCb.Checked = configManager.config.WordWrap;
			bufferedDrawCb.Checked = configManager.config.BufferedDraw;
			alwaysOnTopCb.Checked = configManager.config.AlwaysOnTop;
			SetFont(new Font(configManager.config.FontFamilyName, configManager.config.FontSize, (FontStyle)configManager.config.FontStyle));
			highlightItems = new List<HighlightItem>(configManager.config.HighlightItems);
			logReader.EnqueueMsg(LogReader.MessageID.UpdateHighlighter, highlightItems);
			SetGeometry(configManager.config.X, configManager.config.Y, configManager.config.W, configManager.config.H);
			highlightDataGrid.Rows.Clear();
			foreach (HighlightItem item in highlightItems) {
				highlightDataGrid.Rows.Add(GetNewRow(item.PatternRegex.ToString(), item.HighlightColor));
			}
			return true;
		}
		public void SetGeometry(int x, int y, int w, int h) {
			if (x == int.MinValue) {
				x = Bounds.X;
			}
			if (y == int.MinValue) {
				y = Bounds.Y;
			}
			if (w == int.MinValue) {
				w = Bounds.Width;
			}
			if (h == int.MinValue) {
				h = Bounds.Height;
			}
			bool isWithinScreenBounds = false;
			foreach (Screen screen in Screen.AllScreens) {
				Rectangle screenBounds = screen.Bounds;
				isWithinScreenBounds |=
						(x - 10 >= screenBounds.Left && x + 10 <= screenBounds.Right && y >= screenBounds.Top && y + 10 <= screenBounds.Bottom) || // TL
						((x + w - 10) >= screenBounds.Left && (x + w + 10) <= screenBounds.Right && y >= screenBounds.Top && y + 10 <= screenBounds.Bottom); // TR
				if (isWithinScreenBounds) {
					break;
				}
			}
			if (isWithinScreenBounds) {
				Bounds = new Rectangle(x, y, w, h);
			}
		}
		#endregion

		#region print log file
		private void Watcher_TargetChanged(object sender, FileSystemEventArgs e) {
			logReader.EnqueueMsg(LogReader.MessageID.UpdateTargetFile, new LogReader.UpdateTargetFileMsg { path = watcher.CurFilePath, clearTBContent = !persistentCb.Checked });
			Invoke(new Action(() => {
				Text = $"LogViewer {appVersion.Major}.{appVersion.Minor}.{appVersion.Build}: {watcher.CurFileName}";
			}));
		}
		private void UpdateCondition() {
			watcher.SetCondition(folderTextBox.Text, filenameTextbox.Text, filenameRegexCb.Checked);
		}
		private void UpdateFileContent() {
			logReader.EnqueueMsg(LogReader.MessageID.LoadFile);
		}

		#endregion print log file

		#region highlight
		const double highlightGamma = 0.7;
		static readonly Color[] defaultHighlightBackColors = {
			Color.FromArgb((int)(0xE6*highlightGamma), (int)(0x5C*highlightGamma), (int)(0x00*highlightGamma)),//Dark Orange
			Color.FromArgb((int)(0xD3*highlightGamma), (int)(0xA6*highlightGamma), (int)(0x00*highlightGamma)),//Dark Yellow
			Color.FromArgb((int)(0x00*highlightGamma), (int)(0x64*highlightGamma), (int)(0x00*highlightGamma)),//Dark Green
			Color.FromArgb((int)(0x00*highlightGamma), (int)(0x46*highlightGamma), (int)(0x8C*highlightGamma)),//Dark Blue
			Color.FromArgb((int)(0x80*highlightGamma), (int)(0x00*highlightGamma), (int)(0x80*highlightGamma)),//Dark Purple
			Color.FromArgb((int)(0x7B*highlightGamma), (int)(0x5D*highlightGamma), (int)(0x34*highlightGamma)),//Dark Brown
			Color.FromArgb((int)(0xB2*highlightGamma), (int)(0x00*highlightGamma), (int)(0x7F*highlightGamma)),//Dark Pink
			Color.FromArgb((int)(0x00*highlightGamma), (int)(0x68*highlightGamma), (int)(0x65*highlightGamma)),//Dark Teal
			Color.FromArgb((int)(0xB8*highlightGamma), (int)(0x86*highlightGamma), (int)(0x0B*highlightGamma)),//Dark Gold
			Color.FromArgb((int)(0x80*highlightGamma), (int)(0x00*highlightGamma), (int)(0x00*highlightGamma)),//Dark Maroon
			Color.FromArgb((int)(0xFF*highlightGamma), (int)(0x5C*highlightGamma), (int)(0x5C*highlightGamma)),//Dark Coral
			Color.FromArgb((int)(0x4B*highlightGamma), (int)(0x00*highlightGamma), (int)(0x82*highlightGamma)),//Dark Indigo
			Color.FromArgb((int)(0x55*highlightGamma), (int)(0x6B*highlightGamma), (int)(0x2F*highlightGamma)),//Dark Olive
			Color.FromArgb((int)(0x2F*highlightGamma), (int)(0x4F*highlightGamma), (int)(0x4F*highlightGamma)),//Dark Slate
			Color.FromArgb((int)(0x99*highlightGamma), (int)(0x00*highlightGamma), (int)(0x00*highlightGamma)),//Dark Crimson
			Color.FromArgb((int)(0x8B*highlightGamma), (int)(0x66*highlightGamma), (int)(0x8B*highlightGamma)),//Dark Plum
			Color.FromArgb((int)(0x8B*highlightGamma), (int)(0x45*highlightGamma), (int)(0x13*highlightGamma)),//Dark Sienna
			Color.FromArgb((int)(0x99*highlightGamma), (int)(0x32*highlightGamma), (int)(0xCC*highlightGamma)),//Dark Orchid
			Color.FromArgb((int)(0x43*highlightGamma), (int)(0x46*highlightGamma), (int)(0x4B*highlightGamma)),//Dark Steel
		};
		private DataGridViewRow GetNewRow(string s, Color c) {
			DataGridViewRow newRow = (DataGridViewRow)highlightDataGrid.RowTemplate.Clone();
			newRow.Cells.Add((DataGridViewCell)highlightDataGrid.Columns[0].CellTemplate.Clone());
			newRow.Cells.Add((DataGridViewCell)highlightDataGrid.Columns[1].CellTemplate.Clone());
			newRow.Cells[1].Value = s;
			foreach (DataGridViewCell cell in newRow.Cells) {
				cell.Style.BackColor = c;
			}
			return newRow;
		}
		private void addHighlightBtn_Click(object sender, EventArgs e) {
			List<Color> unusedBackColor = new List<Color>(defaultHighlightBackColors);
			foreach (DataGridViewRow row in highlightDataGrid.Rows) {
				unusedBackColor.Remove(row.Cells[0].Style.BackColor);
			}
			if (unusedBackColor.Count == 0) {
				unusedBackColor.Add(defaultHighlightBackColors.First());
			}
			highlightItems.Add(new HighlightItem() { HighlightColor = unusedBackColor.First() });
			int newRow = highlightDataGrid.Rows.Add(GetNewRow("", unusedBackColor.First()));
			highlightDataGrid.CurrentCell = highlightDataGrid.Rows[newRow].Cells[1];
			highlightDataGrid.BeginEdit(false);
		}
		private void highlightDataGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e) {
			if (highlightItems[highlightDataGrid.CurrentCell.RowIndex].SetPatternRegex((string)highlightDataGrid.CurrentCell.Value)) {
				highlightDataGrid.CurrentCell.Style.BackColor = highlightDataGrid.CurrentRow.Cells[0].Style.BackColor;
				logReader.EnqueueMsg(LogReader.MessageID.UpdateHighlighter, highlightItems);
			}
			else {
				highlightDataGrid.CurrentCell.Style.BackColor = Color.Red;
			}
		}
		private void highlightDataGrid_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e) {
			if (highlightDataGrid.CurrentCell.ColumnIndex == 1) {
				((TextBox)e.Control).TextChanged += highlightDataGrid_TextChanged;
				((TextBox)e.Control).LostFocus += highlightDataGrid_EditingControlLostFocus;
			}
		}
		private void highlightDataGrid_TextChanged(object sender, EventArgs e) {
			try {
				new Regex(((TextBox)sender).Text);
				highlightDataGrid.CurrentCell.Style.BackColor = highlightDataGrid.CurrentRow.Cells[0].Style.BackColor;
			}
			catch {
				highlightDataGrid.CurrentCell.Style.BackColor = Color.Red;
			}

		}

		private void highlightDataGrid_LostFocus(object sender, EventArgs e) {
			highlightDataGrid.ClearSelection();
		}
		private void highlightDataGrid_EditingControlLostFocus(object sender, EventArgs e) {
			// calling EndEdit under FocusInternal will cause NullPointerException.....WHY M$ WHY
			bool focusInternal = false;
			StackTrace st = new StackTrace();
			for (int i = 0; i < st.FrameCount; i++) {
				if (st.GetFrame(i).GetMethod().Module.Name == "System.Windows.Forms.dll" && st.GetFrame(i).GetMethod().Name == "FocusInternal") {
					focusInternal = true;
					break;
				}
			}
			if (!focusInternal) {
				highlightDataGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
				highlightDataGrid.EndEdit();
			}
		}

		private void highlightDataGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
			if (e.ColumnIndex == 0) {
				colorDialog1.Color = highlightItems[e.RowIndex].HighlightColor;
				DialogResult result = colorDialog1.ShowDialog();
				if (result == DialogResult.OK) {
					highlightItems[e.RowIndex].HighlightColor = colorDialog1.Color;
					foreach (DataGridViewCell cell in highlightDataGrid.Rows[e.RowIndex].Cells) {
						cell.Style.BackColor = colorDialog1.Color;
					}
				}
			}
			else {
				highlightDataGrid.BeginEdit(false);
			}
		}
		private void HighlightDataGrid_KeyUp(object sender, KeyEventArgs e) {
			SortedSet<int> selectedRows = new SortedSet<int>();
			if (e.KeyData == Keys.Delete) {
				foreach (DataGridViewCell cell in highlightDataGrid.SelectedCells) {
					selectedRows.Add(cell.RowIndex);
				}
			}
			foreach (int rowIndex in selectedRows.Reverse()) {
				highlightDataGrid.Rows.RemoveAt(rowIndex);
				highlightItems.RemoveAt(rowIndex);
				logReader.EnqueueMsg(LogReader.MessageID.UpdateHighlighter, highlightItems);
			}
		}
		#endregion highlight

		#region path
		private bool checkFolder() {
			folderTextBox.BackColor = SystemColors.Window;
			if (Utility.HasInvalidPathCharacters(folderTextBox.Text)) {
				folderTextBox.BackColor = Color.IndianRed;
				return false;
			}
			return true;
		}
		private bool checkFilename() {
			filenameTextbox.BackColor = SystemColors.Window;
			if (filenameTextbox.Text.Length == 0) return true;
			if (filenameRegexCb.Checked) {
				try {
					new Regex(filenameTextbox.Text);
				}
				catch {
					filenameTextbox.BackColor = Color.IndianRed;
					return false;
				}
			}
			else {
				if (invalidNonWildcardPathChar.IsMatch(filenameTextbox.Text)) {
					filenameTextbox.BackColor = Color.IndianRed;
					return false;
				}
			}
			return true;
		}
		private void pathTextbox_LostFocus(object sender, EventArgs e) {
			if (checkFolder() && checkFilename()) {
				UpdateCondition();
			}
		}
		private void filenameRegexCb_CheckedChanged(object sender, EventArgs e) {
			checkFilename();
		}
		private void filenameTextbox_TextChanged(object sender, EventArgs e) {
			checkFilename();
		}
		private void folderTextBox_TextChanged(object sender, EventArgs e) {
			if (folderTextBox.Text.StartsWith(@"\")) return; // skip checking for share folderPath while typing
			checkFolder();
		}
		#endregion path

		#region font
		private void SetFont(Font font) {
			fontBtn.Text = $"{font.Name}, {font.SizeInPoints:0.##} pt";
			fontBtn.Font = new Font(font.FontFamily, fontBtn.Font.Size);
			mainLogText.Font = font;
		}
		private void fontBtn_Click(object sender, EventArgs e) {
			fontDialog1.Font = mainLogText.Font;
			DialogResult result = fontDialog1.ShowDialog();
			if (result == DialogResult.OK) {
				SetFont(fontDialog1.Font);
			}
		}
		private void FontDialog1_Apply(object sender, EventArgs e) {
			SetFont(fontDialog1.Font);
		}
		#endregion font

		#region line filtering
		private void UpdateIncludeRegex() {
			if (includeTextBox.Text.Length == 0) {
				logReader.EnqueueMsg(LogReader.MessageID.UpdateIncludeRegex, null);
			}
			try {
				// test regex first
				new Regex(includeTextBox.Text, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
				logReader.EnqueueMsg(LogReader.MessageID.UpdateIncludeRegex, includeTextBox.Text);
			}
			catch {
				Debug.WriteLine("UpdateIncludeRegex error occur");
			}
		}
		private void UpdateExcludeRegex() {
			if (excludeTextBox.Text.Length == 0) {
				logReader.EnqueueMsg(LogReader.MessageID.UpdateExcludeRegex, null);
			}
			try {
				// test regex first
				new Regex(excludeTextBox.Text, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
				logReader.EnqueueMsg(LogReader.MessageID.UpdateExcludeRegex, excludeTextBox.Text);
			}
			catch {
				Debug.WriteLine("UpdateExcludeRegex error occur");
			}
		}
		private void includeTextBox_TextChanged(object sender, EventArgs e) {
			includeTextBox.BackColor = SystemColors.Window;
			if (includeTextBox.Text.Length == 0) return;
			try {
				new Regex(includeTextBox.Text);
			}
			catch {
				includeTextBox.BackColor = Color.IndianRed;
			}
		}
		private void excludeTextBox_TextChanged(object sender, EventArgs e) {
			excludeTextBox.BackColor = SystemColors.Window;
			if (excludeTextBox.Text.Length == 0) return;
			try {
				new Regex(excludeTextBox.Text);
			}
			catch {
				excludeTextBox.BackColor = Color.IndianRed;
			}
		}
		#endregion line filtering

		#region UI event
		private void toggleOptionPanelBtn_Click(object sender, EventArgs e) {
			optionPanel.Visible = !optionPanel.Visible;
			toggleOptionBtn.Text = optionPanel.Visible ? "-" : "+";
		}
		private void reloadBtn_Click(object sender, EventArgs e) {
			logReader.EnqueueMsg(LogReader.MessageID.UpdateTargetFile, new LogReader.UpdateTargetFileMsg { path = watcher.CurFilePath, clearTBContent = true });
			watcher.Restart();
		}
		private void mainLogText_KeyUp(object sender, KeyEventArgs e) {
			if (e.Control && e.KeyCode == Keys.F) {
				Point pos = findPanel.Location;
				if (!findPanel.Visible) {
					if (optionPanel.Visible) {
						pos.Y = optionPanel.Size.Height + 5;
					}
					else {
						pos.Y = 20;
					}
					findPanel.Location = pos;
				}
				findPanel.Visible = !findPanel.Visible;
				if (findPanel.Visible) {
					findTextbox.Focus();
				}
			}
			else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F3) {
				mainLogText.FindNext(findTextbox.Text);
			}
			else if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.F3) {
				mainLogText.FindPrevious(findTextbox.Text);
			}
		}
		private void findTextbox_KeyUp(object sender, KeyEventArgs e) {
			if (e.Modifiers == Keys.None && e.KeyCode == Keys.F3) {
				mainLogText.FindNext(findTextbox.Text);
			}
			else if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.F3) {
				mainLogText.FindPrevious(findTextbox.Text);
			}
		}
		#endregion UI Event

		private void testBtn_Click(object sender, EventArgs e) {
		}
	}
}

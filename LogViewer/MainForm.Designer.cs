using System.Windows.Forms;

namespace LogViewer
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			this.toggleOptionBtn = new System.Windows.Forms.Button();
			this.optionPanel = new System.Windows.Forms.Panel();
			this.bufferedDrawCb = new System.Windows.Forms.CheckBox();
			this.saveBtn = new System.Windows.Forms.Button();
			this.persistentCb = new System.Windows.Forms.CheckBox();
			this.readLastLinesInput = new System.Windows.Forms.NumericUpDown();
			this.addHighlightBtn = new System.Windows.Forms.Label();
			this.highlightDataGrid = new System.Windows.Forms.DataGridView();
			this.colorColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.textColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.filenameRegexCb = new System.Windows.Forms.CheckBox();
			this.excludeTextBox = new System.Windows.Forms.TextBox();
			this.excludeLabel = new System.Windows.Forms.Label();
			this.readLastLinesLabel = new System.Windows.Forms.Label();
			this.reloadBtn = new System.Windows.Forms.Button();
			this.includeTextBox = new System.Windows.Forms.TextBox();
			this.includeLabel = new System.Windows.Forms.Label();
			this.highlight = new System.Windows.Forms.Label();
			this.filenameTextbox = new System.Windows.Forms.TextBox();
			this.folderTextBox = new System.Windows.Forms.TextBox();
			this.alwaysOnTopCb = new System.Windows.Forms.CheckBox();
			this.filenameLabel = new System.Windows.Forms.Label();
			this.folderLabel = new System.Windows.Forms.Label();
			this.fontDialog1 = new System.Windows.Forms.FontDialog();
			this.colorDialog1 = new System.Windows.Forms.ColorDialog();
			this.scrollToBottomBtn = new System.Windows.Forms.Label();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.testBtn = new System.Windows.Forms.Button();
			this.clearBtn = new System.Windows.Forms.Label();
			this.findPanel = new System.Windows.Forms.Panel();
			this.findTextbox = new System.Windows.Forms.TextBox();
			this.findLabel = new System.Windows.Forms.Label();
			this.wordWrapCb = new System.Windows.Forms.CheckBox();
			this.mainLogText = new LogViewer.RichTextBoxEx();
			this.fontBtn = new System.Windows.Forms.Button();
			this.optionPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.readLastLinesInput)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.highlightDataGrid)).BeginInit();
			this.findPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// toggleOptionBtn
			// 
			this.toggleOptionBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.toggleOptionBtn.AutoSize = true;
			this.toggleOptionBtn.BackColor = System.Drawing.Color.DimGray;
			this.toggleOptionBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.toggleOptionBtn.FlatAppearance.BorderSize = 0;
			this.toggleOptionBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.toggleOptionBtn.Font = new System.Drawing.Font("Segoe UI", 11.25F);
			this.toggleOptionBtn.ForeColor = System.Drawing.Color.White;
			this.toggleOptionBtn.Location = new System.Drawing.Point(942, 10);
			this.toggleOptionBtn.Margin = new System.Windows.Forms.Padding(0);
			this.toggleOptionBtn.Name = "toggleOptionBtn";
			this.toggleOptionBtn.Size = new System.Drawing.Size(29, 30);
			this.toggleOptionBtn.TabIndex = 1;
			this.toggleOptionBtn.Text = "+";
			this.toggleOptionBtn.UseVisualStyleBackColor = false;
			this.toggleOptionBtn.Click += new System.EventHandler(this.toggleOptionPanelBtn_Click);
			// 
			// optionPanel
			// 
			this.optionPanel.BackColor = System.Drawing.Color.DarkGray;
			this.optionPanel.Controls.Add(this.wordWrapCb);
			this.optionPanel.Controls.Add(this.bufferedDrawCb);
			this.optionPanel.Controls.Add(this.saveBtn);
			this.optionPanel.Controls.Add(this.persistentCb);
			this.optionPanel.Controls.Add(this.readLastLinesInput);
			this.optionPanel.Controls.Add(this.addHighlightBtn);
			this.optionPanel.Controls.Add(this.highlightDataGrid);
			this.optionPanel.Controls.Add(this.fontBtn);
			this.optionPanel.Controls.Add(this.filenameRegexCb);
			this.optionPanel.Controls.Add(this.excludeTextBox);
			this.optionPanel.Controls.Add(this.excludeLabel);
			this.optionPanel.Controls.Add(this.readLastLinesLabel);
			this.optionPanel.Controls.Add(this.reloadBtn);
			this.optionPanel.Controls.Add(this.includeTextBox);
			this.optionPanel.Controls.Add(this.includeLabel);
			this.optionPanel.Controls.Add(this.highlight);
			this.optionPanel.Controls.Add(this.filenameTextbox);
			this.optionPanel.Controls.Add(this.folderTextBox);
			this.optionPanel.Controls.Add(this.alwaysOnTopCb);
			this.optionPanel.Controls.Add(this.filenameLabel);
			this.optionPanel.Controls.Add(this.folderLabel);
			this.optionPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.optionPanel.Location = new System.Drawing.Point(0, 0);
			this.optionPanel.Margin = new System.Windows.Forms.Padding(0);
			this.optionPanel.Name = "optionPanel";
			this.optionPanel.Size = new System.Drawing.Size(998, 159);
			this.optionPanel.TabIndex = 2;
			this.optionPanel.Visible = false;
			// 
			// bufferedDrawCb
			// 
			this.bufferedDrawCb.AutoSize = true;
			this.bufferedDrawCb.Location = new System.Drawing.Point(342, 110);
			this.bufferedDrawCb.Name = "bufferedDrawCb";
			this.bufferedDrawCb.Size = new System.Drawing.Size(94, 17);
			this.bufferedDrawCb.TabIndex = 10;
			this.bufferedDrawCb.Text = "Buffered Draw";
			this.bufferedDrawCb.UseVisualStyleBackColor = true;
			// 
			// saveBtn
			// 
			this.saveBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.saveBtn.BackColor = System.Drawing.Color.DimGray;
			this.saveBtn.FlatAppearance.BorderSize = 0;
			this.saveBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.saveBtn.ForeColor = System.Drawing.Color.White;
			this.saveBtn.Location = new System.Drawing.Point(764, 119);
			this.saveBtn.Name = "saveBtn";
			this.saveBtn.Size = new System.Drawing.Size(105, 30);
			this.saveBtn.TabIndex = 13;
			this.saveBtn.Text = "Save Config";
			this.saveBtn.UseVisualStyleBackColor = false;
			// 
			// persistentCb
			// 
			this.persistentCb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.persistentCb.AutoSize = true;
			this.persistentCb.Location = new System.Drawing.Point(717, 86);
			this.persistentCb.Name = "persistentCb";
			this.persistentCb.Size = new System.Drawing.Size(72, 17);
			this.persistentCb.TabIndex = 8;
			this.persistentCb.Text = "Persistent";
			this.persistentCb.UseVisualStyleBackColor = true;
			// 
			// readLastLinesInput
			// 
			this.readLastLinesInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.readLastLinesInput.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.readLastLinesInput.Location = new System.Drawing.Point(432, 88);
			this.readLastLinesInput.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
			this.readLastLinesInput.Name = "readLastLinesInput";
			this.readLastLinesInput.Size = new System.Drawing.Size(279, 16);
			this.readLastLinesInput.TabIndex = 7;
			this.readLastLinesInput.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
			// 
			// addHighlightBtn
			// 
			this.addHighlightBtn.BackColor = System.Drawing.Color.White;
			this.addHighlightBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.addHighlightBtn.Font = new System.Drawing.Font("Consolas", 12F);
			this.addHighlightBtn.ForeColor = System.Drawing.Color.Black;
			this.addHighlightBtn.Location = new System.Drawing.Point(295, 46);
			this.addHighlightBtn.Name = "addHighlightBtn";
			this.addHighlightBtn.Size = new System.Drawing.Size(25, 21);
			this.addHighlightBtn.TabIndex = 15;
			this.addHighlightBtn.Text = "+";
			this.addHighlightBtn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.addHighlightBtn.Click += new System.EventHandler(this.addHighlightBtn_Click);
			// 
			// highlightDataGrid
			// 
			this.highlightDataGrid.AllowUserToAddRows = false;
			this.highlightDataGrid.AllowUserToResizeColumns = false;
			this.highlightDataGrid.AllowUserToResizeRows = false;
			this.highlightDataGrid.BackgroundColor = System.Drawing.Color.DimGray;
			this.highlightDataGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.highlightDataGrid.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
			this.highlightDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.highlightDataGrid.ColumnHeadersVisible = false;
			this.highlightDataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colorColumn,
            this.textColumn});
			this.highlightDataGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnKeystroke;
			this.highlightDataGrid.GridColor = System.Drawing.Color.Black;
			this.highlightDataGrid.Location = new System.Drawing.Point(10, 45);
			this.highlightDataGrid.Name = "highlightDataGrid";
			this.highlightDataGrid.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			this.highlightDataGrid.RowHeadersVisible = false;
			this.highlightDataGrid.RowTemplate.Height = 20;
			this.highlightDataGrid.Size = new System.Drawing.Size(323, 104);
			this.highlightDataGrid.TabIndex = 16;
			// 
			// colorColumn
			// 
			this.colorColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			dataGridViewCellStyle1.BackColor = System.Drawing.Color.Black;
			dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
			this.colorColumn.DefaultCellStyle = dataGridViewCellStyle1;
			this.colorColumn.Frozen = true;
			this.colorColumn.HeaderText = "Highlight Color";
			this.colorColumn.MaxInputLength = 0;
			this.colorColumn.Name = "colorColumn";
			this.colorColumn.ReadOnly = true;
			this.colorColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.colorColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.colorColumn.Width = 30;
			// 
			// textColumn
			// 
			this.textColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			dataGridViewCellStyle2.BackColor = System.Drawing.Color.Black;
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
			this.textColumn.DefaultCellStyle = dataGridViewCellStyle2;
			this.textColumn.HeaderText = "pattern";
			this.textColumn.Name = "textColumn";
			this.textColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.textColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// filenameRegexCb
			// 
			this.filenameRegexCb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.filenameRegexCb.AutoSize = true;
			this.filenameRegexCb.Location = new System.Drawing.Point(875, 8);
			this.filenameRegexCb.Name = "filenameRegexCb";
			this.filenameRegexCb.Size = new System.Drawing.Size(58, 17);
			this.filenameRegexCb.TabIndex = 4;
			this.filenameRegexCb.Text = "RegEx";
			this.filenameRegexCb.UseVisualStyleBackColor = true;
			this.filenameRegexCb.CheckedChanged += new System.EventHandler(this.filenameRegexCb_CheckedChanged);
			// 
			// excludeTextBox
			// 
			this.excludeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.excludeTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.excludeTextBox.Location = new System.Drawing.Point(393, 62);
			this.excludeTextBox.Name = "excludeTextBox";
			this.excludeTextBox.Size = new System.Drawing.Size(540, 13);
			this.excludeTextBox.TabIndex = 6;
			this.excludeTextBox.TextChanged += new System.EventHandler(this.excludeTextBox_TextChanged);
			// 
			// excludeLabel
			// 
			this.excludeLabel.AutoSize = true;
			this.excludeLabel.Location = new System.Drawing.Point(339, 62);
			this.excludeLabel.Name = "excludeLabel";
			this.excludeLabel.Size = new System.Drawing.Size(48, 13);
			this.excludeLabel.TabIndex = 13;
			this.excludeLabel.Text = "Exclude:";
			// 
			// readLastLinesLabel
			// 
			this.readLastLinesLabel.AutoSize = true;
			this.readLastLinesLabel.Location = new System.Drawing.Point(339, 87);
			this.readLastLinesLabel.Name = "readLastLinesLabel";
			this.readLastLinesLabel.Size = new System.Drawing.Size(87, 13);
			this.readLastLinesLabel.TabIndex = 11;
			this.readLastLinesLabel.Text = "Read Last Lines:";
			// 
			// reloadBtn
			// 
			this.reloadBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.reloadBtn.BackColor = System.Drawing.Color.DimGray;
			this.reloadBtn.FlatAppearance.BorderSize = 0;
			this.reloadBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.reloadBtn.ForeColor = System.Drawing.Color.White;
			this.reloadBtn.Location = new System.Drawing.Point(875, 119);
			this.reloadBtn.Name = "reloadBtn";
			this.reloadBtn.Size = new System.Drawing.Size(105, 30);
			this.reloadBtn.TabIndex = 14;
			this.reloadBtn.Text = "Reload";
			this.reloadBtn.UseVisualStyleBackColor = false;
			this.reloadBtn.Click += new System.EventHandler(this.reloadBtn_Click);
			// 
			// includeTextBox
			// 
			this.includeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.includeTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.includeTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.includeTextBox.Location = new System.Drawing.Point(393, 36);
			this.includeTextBox.Name = "includeTextBox";
			this.includeTextBox.Size = new System.Drawing.Size(540, 13);
			this.includeTextBox.TabIndex = 5;
			this.includeTextBox.TextChanged += new System.EventHandler(this.includeTextBox_TextChanged);
			// 
			// includeLabel
			// 
			this.includeLabel.AutoSize = true;
			this.includeLabel.Location = new System.Drawing.Point(339, 36);
			this.includeLabel.Name = "includeLabel";
			this.includeLabel.Size = new System.Drawing.Size(45, 13);
			this.includeLabel.TabIndex = 8;
			this.includeLabel.Text = "Include:";
			// 
			// highlight
			// 
			this.highlight.AutoSize = true;
			this.highlight.Location = new System.Drawing.Point(10, 29);
			this.highlight.Name = "highlight";
			this.highlight.Size = new System.Drawing.Size(51, 13);
			this.highlight.TabIndex = 7;
			this.highlight.Text = "Highlight:";
			// 
			// filenameTextbox
			// 
			this.filenameTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.filenameTextbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.filenameTextbox.Location = new System.Drawing.Point(711, 9);
			this.filenameTextbox.Name = "filenameTextbox";
			this.filenameTextbox.Size = new System.Drawing.Size(158, 13);
			this.filenameTextbox.TabIndex = 3;
			this.filenameTextbox.TextChanged += new System.EventHandler(this.filenameTextbox_TextChanged);
			// 
			// folderTextBox
			// 
			this.folderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.folderTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.folderTextBox.Location = new System.Drawing.Point(93, 9);
			this.folderTextBox.Name = "folderTextBox";
			this.folderTextBox.Size = new System.Drawing.Size(554, 13);
			this.folderTextBox.TabIndex = 2;
			this.folderTextBox.TextChanged += new System.EventHandler(this.folderTextBox_TextChanged);
			// 
			// alwaysOnTopCb
			// 
			this.alwaysOnTopCb.AutoSize = true;
			this.alwaysOnTopCb.Location = new System.Drawing.Point(342, 133);
			this.alwaysOnTopCb.Name = "alwaysOnTopCb";
			this.alwaysOnTopCb.Size = new System.Drawing.Size(98, 17);
			this.alwaysOnTopCb.TabIndex = 12;
			this.alwaysOnTopCb.Text = "Always On Top";
			this.alwaysOnTopCb.UseVisualStyleBackColor = true;
			// 
			// filenameLabel
			// 
			this.filenameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.filenameLabel.AutoSize = true;
			this.filenameLabel.Location = new System.Drawing.Point(653, 9);
			this.filenameLabel.Name = "filenameLabel";
			this.filenameLabel.Size = new System.Drawing.Size(52, 13);
			this.filenameLabel.TabIndex = 2;
			this.filenameLabel.Text = "Filename:";
			// 
			// folderLabel
			// 
			this.folderLabel.AutoSize = true;
			this.folderLabel.Location = new System.Drawing.Point(10, 9);
			this.folderLabel.Name = "folderLabel";
			this.folderLabel.Size = new System.Drawing.Size(77, 13);
			this.folderLabel.TabIndex = 0;
			this.folderLabel.Text = "Monitor Folder:";
			// 
			// fontDialog1
			// 
			this.fontDialog1.AllowScriptChange = false;
			this.fontDialog1.AllowVerticalFonts = false;
			this.fontDialog1.FixedPitchOnly = true;
			this.fontDialog1.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.fontDialog1.FontMustExist = true;
			this.fontDialog1.ShowApply = true;
			this.fontDialog1.ShowColor = true;
			// 
			// colorDialog1
			// 
			this.colorDialog1.AnyColor = true;
			this.colorDialog1.FullOpen = true;
			// 
			// scrollToBottomBtn
			// 
			this.scrollToBottomBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.scrollToBottomBtn.BackColor = System.Drawing.Color.DimGray;
			this.scrollToBottomBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.scrollToBottomBtn.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.scrollToBottomBtn.ForeColor = System.Drawing.Color.White;
			this.scrollToBottomBtn.Location = new System.Drawing.Point(952, 342);
			this.scrollToBottomBtn.Name = "scrollToBottomBtn";
			this.scrollToBottomBtn.Size = new System.Drawing.Size(20, 20);
			this.scrollToBottomBtn.TabIndex = 18;
			this.scrollToBottomBtn.Text = "v";
			this.scrollToBottomBtn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// testBtn
			// 
			this.testBtn.Location = new System.Drawing.Point(484, 160);
			this.testBtn.Name = "testBtn";
			this.testBtn.Size = new System.Drawing.Size(75, 23);
			this.testBtn.TabIndex = 22;
			this.testBtn.Text = "Test";
			this.testBtn.UseVisualStyleBackColor = true;
			this.testBtn.Click += new System.EventHandler(this.testBtn_Click);
			// 
			// clearBtn
			// 
			this.clearBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.clearBtn.BackColor = System.Drawing.Color.DimGray;
			this.clearBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.clearBtn.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.clearBtn.ForeColor = System.Drawing.Color.White;
			this.clearBtn.Location = new System.Drawing.Point(926, 342);
			this.clearBtn.Name = "clearBtn";
			this.clearBtn.Size = new System.Drawing.Size(20, 20);
			this.clearBtn.TabIndex = 17;
			this.clearBtn.Text = "x";
			this.clearBtn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// findPanel
			// 
			this.findPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.findPanel.BackColor = System.Drawing.Color.DimGray;
			this.findPanel.Controls.Add(this.findTextbox);
			this.findPanel.Controls.Add(this.findLabel);
			this.findPanel.Location = new System.Drawing.Point(795, 162);
			this.findPanel.Name = "findPanel";
			this.findPanel.Size = new System.Drawing.Size(138, 19);
			this.findPanel.TabIndex = 23;
			this.findPanel.Visible = false;
			// 
			// findTextbox
			// 
			this.findTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.findTextbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.findTextbox.Location = new System.Drawing.Point(39, 3);
			this.findTextbox.Name = "findTextbox";
			this.findTextbox.Size = new System.Drawing.Size(96, 13);
			this.findTextbox.TabIndex = 15;
			// 
			// findLabel
			// 
			this.findLabel.AutoSize = true;
			this.findLabel.Location = new System.Drawing.Point(3, 3);
			this.findLabel.Name = "findLabel";
			this.findLabel.Size = new System.Drawing.Size(30, 13);
			this.findLabel.TabIndex = 14;
			this.findLabel.Text = "Find:";
			// 
			// wordWrapCb
			// 
			this.wordWrapCb.AutoSize = true;
			this.wordWrapCb.Checked = this.mainLogText.WordWrap;
			this.wordWrapCb.CheckState = System.Windows.Forms.CheckState.Checked;
			this.wordWrapCb.Location = new System.Drawing.Point(442, 110);
			this.wordWrapCb.Name = "wordWrapCb";
			this.wordWrapCb.Size = new System.Drawing.Size(81, 17);
			this.wordWrapCb.TabIndex = 11;
			this.wordWrapCb.Text = "Word Wrap";
			this.wordWrapCb.UseVisualStyleBackColor = true;
			// 
			// mainLogText
			// 
			this.mainLogText.BackColor = System.Drawing.Color.Black;
			this.mainLogText.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.mainLogText.CausesValidation = false;
			this.mainLogText.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainLogText.Font = new System.Drawing.Font("Consolas", 9F);
			this.mainLogText.ForeColor = System.Drawing.Color.White;
			this.mainLogText.HideSelection = false;
			this.mainLogText.Location = new System.Drawing.Point(0, 0);
			this.mainLogText.Margin = new System.Windows.Forms.Padding(0);
			this.mainLogText.Name = "mainLogText";
			this.mainLogText.ReadOnly = true;
			this.mainLogText.Size = new System.Drawing.Size(998, 392);
			this.mainLogText.TabIndex = 0;
			this.mainLogText.Text = "";
			// 
			// fontBtn
			// 
			this.fontBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.fontBtn.BackColor = System.Drawing.Color.DimGray;
			this.fontBtn.FlatAppearance.BorderSize = 0;
			this.fontBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.fontBtn.Font = this.mainLogText.Font;
			this.fontBtn.ForeColor = System.Drawing.Color.White;
			this.fontBtn.Location = new System.Drawing.Point(792, 83);
			this.fontBtn.Name = "fontBtn";
			this.fontBtn.Size = new System.Drawing.Size(141, 23);
			this.fontBtn.TabIndex = 9;
			this.fontBtn.Text = "Font";
			this.fontBtn.UseVisualStyleBackColor = false;
			this.fontBtn.Click += new System.EventHandler(this.fontBtn_Click);
			// 
			// MainForm
			// 
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(998, 392);
			this.Controls.Add(this.findPanel);
			this.Controls.Add(this.clearBtn);
			this.Controls.Add(this.testBtn);
			this.Controls.Add(this.scrollToBottomBtn);
			this.Controls.Add(this.toggleOptionBtn);
			this.Controls.Add(this.optionPanel);
			this.Controls.Add(this.mainLogText);
			this.DoubleBuffered = true;
			this.ForeColor = System.Drawing.Color.Black;
			this.MinimumSize = new System.Drawing.Size(500, 150);
			this.Name = "MainForm";
			this.ShowIcon = false;
			this.Text = "LogViewer";
			this.optionPanel.ResumeLayout(false);
			this.optionPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.readLastLinesInput)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.highlightDataGrid)).EndInit();
			this.findPanel.ResumeLayout(false);
			this.findPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

#endregion

		private RichTextBoxEx mainLogText;
		private Button toggleOptionBtn;
		private Panel optionPanel;
		private Label folderLabel;
		private CheckBox alwaysOnTopCb;
		private Label filenameLabel;
		private TextBox filenameTextbox;
		private TextBox folderTextBox;
		private TextBox includeTextBox;
		private Label includeLabel;
		private Label highlight;
		private TextBox excludeTextBox;
		private Label excludeLabel;
		private Label readLastLinesLabel;
		private Button reloadBtn;
		private CheckBox filenameRegexCb;
		private Button fontBtn;
		private Label addHighlightBtn;
		private DataGridView highlightDataGrid;
		private FontDialog fontDialog1;
		private ColorDialog colorDialog1;
		private NumericUpDown readLastLinesInput;
		private Label scrollToBottomBtn;
		private CheckBox persistentCb;
		private SaveFileDialog saveFileDialog1;
		private Button saveBtn;
#if DEBUG
		private Button testBtn;
#endif
		private Label clearBtn;
		private CheckBox bufferedDrawCb;
		private CheckBox wordWrapCb;
		private DataGridViewTextBoxColumn colorColumn;
		private DataGridViewTextBoxColumn textColumn;
		private Panel findPanel;
		private TextBox findTextbox;
		private Label findLabel;
	}
}


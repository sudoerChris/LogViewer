using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

using LogViewer;
class LogReader {
	private LogTextBox targetTB;
	private ManualResetEventSlim msgQueueEvent = new ManualResetEventSlim(false);
	public LogReader(LogTextBox target) {
		targetTB = target;
		Thread t = new Thread(() => {
			rtfBuilder = new RichTextBox();
			rtfBuilder.ForeColor = targetTB.ForeColor;
			rtfBuilder.ReadOnly = true;
			rtfBuilder.Visible = false;
			rtfBuilder.DetectUrls = false;
			Utility.SendMessage(rtfBuilder.Handle, Utility.WM_SETREDRAW, 0, IntPtr.Zero);
			Utility.SendMessage(rtfBuilder.Handle, Utility.EM_SETEVENTMASK, 0, IntPtr.Zero);
			Utility.UnsubscribeInvalidSystemEvents();
			while (true) {
				if (msgQueue.Count == 0) {
					msgQueueEvent.Wait();
					msgQueueEvent.Reset();
				}
				ProcessMsg();
			}
		});
		t.IsBackground = true;
		t.Start();
	}
	#region Message Queue
	// for this use case I don't need to process all message, just the latest one
	public enum MessageID { UpdateTargetFile, UpdateHighlighter, UpdateIncludeRegex, UpdateExcludeRegex, UpdateSizeLimit, SetBufferedPrinting, LoadFile, Count}
	public class UpdateTargetFileMsg{
		public string path;
		public bool clearTBContent;
	}
	// give an id to each msg, only process the latest message
	private readonly Dictionary<MessageID, object> msgQueue = new Dictionary<MessageID, object>();
	public void EnqueueMsg(MessageID id, object msg = null) {
		lock (msgQueue) {
			if (msgQueue.ContainsKey(id)){
				msgQueue[id] = msg;
			} else {
				msgQueue.Add(id, msg);
			}
			msgQueueEvent.Set();
		}
	}
	private void ProcessMsg() {
		Dictionary<MessageID, object> msgQueueCopy;
		lock (msgQueue) {
			msgQueueCopy = new Dictionary<MessageID, object>(msgQueue);
			msgQueue.Clear();
		}
		foreach (KeyValuePair<MessageID, object> msgEntry in msgQueueCopy) {
			try {
				switch (msgEntry.Key) {
					case MessageID.UpdateTargetFile: {
							UpdateTargetFileMsg msg = (UpdateTargetFileMsg)msgEntry.Value;
							if (msg.clearTBContent) {
								targetTB.Invoke(new Action(() => {
									targetTB.Clear();
								}));
							}
							TargetFilePath = msg.path;
							targetTB.Invoke(new Action(() => {
								targetTB.WriteLine($"Monitoring: {TargetFilePath}");
							}));
							lastFileSize = 0;
							prevLineEnded = true;
							OnLoadFile();
							break;
						}
					case MessageID.UpdateHighlighter:
						HighlightItems = (List<HighlightItem>)msgEntry.Value;
						break;
					case MessageID.UpdateIncludeRegex:
						if (string.IsNullOrEmpty((string)msgEntry.Value)) {
							IncludeRegex = null;
						}
						else {
							IncludeRegex = new Regex((string)msgEntry.Value, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
						}
						break;
					case MessageID.UpdateExcludeRegex:
						if (string.IsNullOrEmpty((string)msgEntry.Value)) {
							ExcludeRegex = null;
						}
						else {
							ExcludeRegex = new Regex((string)msgEntry.Value, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
						}
						break;
					case MessageID.UpdateSizeLimit:
						FileSizeLimit = (int)msgEntry.Value * 1000;
						break;
					case MessageID.SetBufferedPrinting:
						BufferedPrinting = (bool)msgEntry.Value;
						break;
					case MessageID.LoadFile:
						OnLoadFile();
						break;
				}
			}
			catch (Exception ex) {
				Debug.WriteLine($"ProcessMsg {msgEntry.Key} exception: {ex.Message}");
			}
		}
	}
	private bool DropCurrentTask(){
		lock (msgQueue){
			return msgQueue.ContainsKey(MessageID.UpdateTargetFile);
		}
	}
	#endregion Message Queue

	#region Config
	private string TargetFilePath;
	private Regex IncludeRegex = null, ExcludeRegex = null;
	private int FileSizeLimit = 1_000_000;
	private List<HighlightItem> HighlightItems = new List<HighlightItem>();
	public bool BufferedPrinting { get; private set; } = false;
	#endregion Config

	#region Process Text
	private long lastFileSize = 0;
	private bool prevLineEnded = true;
	RichTextBox rtfBuilder;
	private void OnLoadFile() {
		if (string.IsNullOrEmpty(TargetFilePath)) return;
		try {
			FileInfo file = new FileInfo(TargetFilePath);
			long fileLength = file.Length;
			if (fileLength <= lastFileSize) // skip if file size shrink
			{
				lastFileSize = fileLength;
				return;
			}
			if (fileLength - lastFileSize > FileSizeLimit) // limit size to read for better performance
			{
				lastFileSize = fileLength - FileSizeLimit;
			}
			// read file to buffer
			byte[] buf = new byte[fileLength - lastFileSize];
			int readBytes = 0;
			using (FileStream fileStream = new FileStream(TargetFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
				fileStream.Seek(lastFileSize, SeekOrigin.Begin);
				readBytes = fileStream.Read(buf, 0, buf.Length);
			}
			if (readBytes == 0) // read nothing
			{
				return;
			}
			lastFileSize += readBytes;
			rtfBuilder.Font = targetTB.Font;
			// read buffer to string and process
			StreamReader reader = new StreamReader(new MemoryStream(buf, 0, readBytes));
			int printedLine = 0;
			string line;
			while ((line = reader.ReadLine()) != null) {
				// filter line
				if (IncludeRegex != null) {
					if (!IncludeRegex.IsMatch(line)) continue;
				}
				if (ExcludeRegex != null) {
					if (ExcludeRegex.IsMatch(line)) continue;
				}
				// Highlight
				rtfBuilder.Select(rtfBuilder.TextLength, 0);
				int lineStartIndex = rtfBuilder.TextLength;
				rtfBuilder.SelectionBackColor = Color.Black;
				rtfBuilder.SelectedText = line + "\n";
				foreach (HighlightItem item in HighlightItems) {
					if (item.PatternRegex == null) continue;
					foreach (Match m in item.PatternRegex.Matches(line)) {
						if (!m.Success) continue;
						rtfBuilder.Select(lineStartIndex + m.Index, m.Length);
						rtfBuilder.SelectionBackColor = item.HighlightColor;
					}
				}
				printedLine++;
				if (!BufferedPrinting || printedLine % 10 == 0) // print line
				{
					rtfBuilder.SelectAll();
					targetTB.Invoke(new Action<string, bool>(targetTB.WriteRtf), new object[] { rtfBuilder.SelectedRtf, !prevLineEnded });
					prevLineEnded = true;
					rtfBuilder.Clear();
				}
				if (printedLine % 50 == 0 && DropCurrentTask()) {
					return;
				}
			}
			if (rtfBuilder.TextLength > 0) {
				rtfBuilder.SelectAll();
				targetTB.Invoke(new Action<string, bool>(targetTB.WriteRtf), new object[] { rtfBuilder.SelectedRtf, !prevLineEnded });
			}
			prevLineEnded = buf[buf.Length - 1] == '\n';
		}
		catch (Exception ex) {
			targetTB.Invoke(new Action(() => {
				targetTB.WriteLine(ex.Message + "\n", Color.Red);
			}));
			prevLineEnded = true;
		}
		rtfBuilder.Clear();
	}
	#endregion Process Text
}

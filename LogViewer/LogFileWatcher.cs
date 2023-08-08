using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace LogViewer {
	internal class LogFileWatcherWorker: ISynchronizeInvoke {
		#region outgoing event
		public ISynchronizeInvoke SynchronizingObject = null;
		public event FileSystemEventHandler ContentChangedHandler;
		public event FileSystemEventHandler TargetChangedHandler;
		private void OnContentChanged(object sender, FileSystemEventArgs e) {
			if (SynchronizingObject != null && SynchronizingObject.InvokeRequired) {

				SynchronizingObject.BeginInvoke(ContentChangedHandler, new object[] { sender, e });
			}
			else {
				ContentChangedHandler(sender, e);
			}
		}
		private void OnTargetChanged(object sender, FileSystemEventArgs e) {
			if (SynchronizingObject != null && SynchronizingObject.InvokeRequired) {

				SynchronizingObject.BeginInvoke(TargetChangedHandler, new object[] { sender, e });
			}
			else {
				TargetChangedHandler(sender, e);
			}
		}
		#endregion outgoing event
		public string CurFilePath = "";
		public string CurFileName { get { return logFileWatcher.Filter; } }
		private string folderPath;
		private string filePattern;
		private Regex filePatternRegex;
		private bool filePatternIsRegex;
		public LogFileWatcherWorker(LogFileWatcher parent) {
			InitWatcher();
			contextThread = Thread.CurrentThread;
			parent.OnSetCondition += (folder, pattern, regex) => { SetCondition(folder, pattern, regex); };
		}
		private void InitWatcher() {
			logFolderWatcher.BeginInit();
			logFileWatcher.BeginInit();
			fileWatcherTimer.BeginInit();
			waitFolderTimer.BeginInit();

			logFolderWatcher.EnableRaisingEvents = false;
			logFolderWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastWrite;
			logFolderWatcher.SynchronizingObject = this;
			logFolderWatcher.Changed += logFolderWatcher_Created;
			logFolderWatcher.Created += logFolderWatcher_Created;
			logFolderWatcher.Renamed += logFolderWatcher_Created;

			logFileWatcher.EnableRaisingEvents = false;
			logFileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
			logFileWatcher.SynchronizingObject = this;
			logFileWatcher.Changed += logFileWatcher_Changed;
			logFileWatcher.Deleted += logFileWatcher_Deleted;

			fileWatcherTimer.Enabled = false;
			fileWatcherTimer.SynchronizingObject = this;
			fileWatcherTimer.Elapsed += fileWatcherTimer_Elapsed;
			fileWatcherTimer.Interval = 500;
			fileWatcherTimer.AutoReset = false;

			waitFolderTimer.Enabled = false;
			waitFolderTimer.SynchronizingObject = this;
			waitFolderTimer.Elapsed += waitFolderTimer_Elapsed;
			waitFolderTimer.Interval = 1000;
			waitFolderTimer.AutoReset = false;

			logFolderWatcher.EndInit();
			logFileWatcher.EndInit();
			fileWatcherTimer.EndInit();
			waitFolderTimer.EndInit();
		}
		#region start/stop
		private void Restart() {
			Stop();
			Start();
		}
		private void Start() {
			if (StartFolderWatcher()) {
				SelectLastWriteFile();
			}
			else {
				StopFileWatcher();
			}
		}
		private void Stop() {
			StopFolderWatcher();
			StopFileWatcher();
		}
		private bool StartFolderWatcher() {
			if (!Directory.Exists(folderPath)) {
				// folderPath not exist, start waiting timer
				if(!Utility.DirectoryAuthorized(folderPath)) {
					// TODO: implemente username and password
				}
				logFolderWatcher.EnableRaisingEvents = false;
				waitFolderTimer.Start();
				return false;
			}
			// folderPath exist, start logFolderWatcher
			waitFolderTimer.Stop();
			logFolderWatcher.Path = folderPath;
			logFolderWatcher.EnableRaisingEvents = true;
			return true;
		}
		private bool StartFileWatcher() {
			if (!Directory.Exists(folderPath)) return false;
			logFileWatcher.Path = folderPath;
			if (!logFileWatcher.EnableRaisingEvents || !fileWatcherTimer.Enabled) {
				logFileWatcher.EnableRaisingEvents = true;
				fileWatcherTimer.Start();
			}
			return true;
		}
		private void StopFolderWatcher() {
			logFolderWatcher.EnableRaisingEvents = false;
			waitFolderTimer.Stop();
		}
		private void StopFileWatcher() {
			logFileWatcher.EnableRaisingEvents = false;
			fileWatcherTimer.Stop();
		}
		#endregion start/stop

		private void SetCondition(string folder, string pattern, bool regex) {
			if (folderPath == folder && filePattern == pattern && filePatternIsRegex == regex) {
				//nothing changed
				Start();
				return;
			}
			Stop();
			filePattern = pattern;
			filePatternIsRegex = regex;
			folderPath = folder;
			if (Directory.Exists(folder)) {
				logFolderWatcher.Path = logFileWatcher.Path = folder;
			}
			try {
				if (pattern.Length == 0) {
					logFolderWatcher.Filter = "*";
					filePatternRegex = new Regex(".*", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
				}
				else if (regex) {
					logFolderWatcher.Filter = "*";
					filePatternRegex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
				}
				else {
					logFolderWatcher.Filter = pattern;
					string regexStr = "^" + Regex.Escape(pattern)
						.Replace("\\*", ".*")
						.Replace("\\?", ".") + "$";
					filePatternRegex = new Regex(regexStr, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
				}
			}
			catch {
				filePatternRegex = new Regex(".*", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
			}
			Start();
		}
		/// <summary>
		/// final function to set target file and start file monitoring
		/// </summary>
		/// <param name="fullName">full path of log file</param>
		private void SetTarget(string fullName) {
			if (!File.Exists(fullName)) return;
			if (CurFilePath == fullName) { // target unchanged, continue last status
				if (!logFileWatcher.EnableRaisingEvents) { // start watcher if not already running
					StartFileWatcher();
				}
				return;
			}
			StopFileWatcher();
			CurFilePath = fullName;
			if (fullName.Length == 0) return;
			logFolderWatcher.Path = logFileWatcher.Path = Path.GetDirectoryName(fullName);
			logFileWatcher.Filter = Path.GetFileName(fullName);
			fileWatcherTimer_lastWriteTime = DateTime.MinValue;
			fileWatcherTimer_length = 0;
			StartFileWatcher();
			OnTargetChanged(this, new FileSystemEventArgs(WatcherChangeTypes.Created, logFolderWatcher.Path, logFileWatcher.Filter));
		}

		private bool CheckFilenamePattern(string filename) {
			return filePatternRegex.IsMatch(filename);
		}
		private FileInfo GetLastWriteFile() {
			try {
				DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
				IOrderedEnumerable<FileInfo> sortedFiles;
				if (filePattern.Length == 0) {
					sortedFiles = directoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly).OrderByDescending(f => f.LastWriteTime);
				}
				else {
					if (filePatternIsRegex) {
						sortedFiles = directoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly).Where(f => filePatternRegex.IsMatch(f.Name)).OrderByDescending(f => f.LastWriteTime);
					}
					else {
						sortedFiles = directoryInfo.GetFiles(filePattern, SearchOption.TopDirectoryOnly).OrderByDescending(f => f.LastWriteTime);
					}
				}
				if (!sortedFiles.Any()) return null;
				return sortedFiles.ElementAt(0);
			}
			catch (DirectoryNotFoundException ex) {
				System.Diagnostics.Debug.WriteLine("Directory not found: " + ex.Message);
			}
			catch (UnauthorizedAccessException ex) {
				System.Diagnostics.Debug.WriteLine("Access to the directory is denied: " + ex.Message);
			}
			catch (Exception ex) {
				System.Diagnostics.Debug.WriteLine("Error occurred: " + ex.Message);
			}
			return null;
		}
		private void SelectLastWriteFile() {
			string fullName = GetLastWriteFile()?.FullName;
			if (fullName == null) return;
			SetTarget(fullName);
		}
		#region filesystem event
		private readonly FileSystemWatcher logFolderWatcher = new FileSystemWatcher();
		private readonly FileSystemWatcher logFileWatcher = new FileSystemWatcher();
		private readonly System.Timers.Timer fileWatcherTimer = new System.Timers.Timer();
		private readonly System.Timers.Timer waitFolderTimer = new System.Timers.Timer();
		private void logFolderWatcher_Created(object sender, FileSystemEventArgs e) {
			if (e.FullPath == CurFilePath || !CheckFilenamePattern(e.Name)) return;
			SetTarget(e.FullPath);
		}
		private void waitFolderTimer_Elapsed(object sender, ElapsedEventArgs e) {
			if (CurFilePath.Length == 0) {
				Stop();
				return;
			}
			try {
				if (Utility.DirectoryExist(folderPath)) { 
					Start();
					return;
				}
				waitFolderTimer.Start();
			}
			catch (Exception ex) {
				System.Diagnostics.Debug.WriteLine("waitFolderTimer_Elapsed Error occurred: " + ex.Message);
				Start();
			}
		}

		DateTime fileWatcherTimer_lastWriteTime = DateTime.MinValue;
		long fileWatcherTimer_length = 0;
		private void fileWatcherTimer_Elapsed(object sender, ElapsedEventArgs e) {
			bool required = InvokeRequired;
			if (CurFilePath.Length == 0) {
				Stop();
				return;
			}
			try {
				FileInfo info = new FileInfo(CurFilePath);
				using (FileStream fileStream = new FileStream(CurFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
					if (fileWatcherTimer_lastWriteTime != info.LastWriteTime || fileWatcherTimer_length != fileStream.Length) {
						fileWatcherTimer_lastWriteTime = info.LastWriteTime;
						fileWatcherTimer_length = fileStream.Length;
						OnContentChanged(this, new FileSystemEventArgs(WatcherChangeTypes.Changed, info.DirectoryName, info.Name));
					}
				}
				fileWatcherTimer.Start();
				}
			catch (Exception ex) {
				System.Diagnostics.Debug.WriteLine("fileWatcherTimer_Elapsed Error occurred: " + ex.Message);
				Start();
			}
		}
		private void logFileWatcher_Changed(object sender, FileSystemEventArgs e) {
			try {
				FileInfo info = new FileInfo(CurFilePath);
				if (fileWatcherTimer_lastWriteTime != info.LastWriteTime || fileWatcherTimer_length != info.Length) {
					fileWatcherTimer_lastWriteTime = info.LastWriteTime;
					fileWatcherTimer_length = info.Length;
					OnContentChanged(this, e);
					fileWatcherTimer.Start();
				}
			}
			catch (Exception ex) {
				System.Diagnostics.Debug.WriteLine("logFileWatcher_Changed Error occurred: " + ex.Message);
			}
		}
		private void logFileWatcher_Deleted(object sender, FileSystemEventArgs e) {
			StopFileWatcher();
		}


		#endregion filesystem event
		#region ISynchronizeInvoke

		private readonly Thread contextThread;
		public bool InvokeRequired {
			get {
				bool required = Thread.CurrentThread != contextThread;
				return required;
			}
		}
		public IAsyncResult BeginInvoke(Delegate method, object[] args) {
			if (Thread.CurrentThread == contextThread) {
				// If we are already in the context thread, directly invoke the method
				return new MyAsyncResult(method.DynamicInvoke(args));
			}
			else {
				// Otherwise, asynchronously invoke the method in the context thread
				return new MyAsyncResult(contextThread, method, args);
			}
		}

		public object EndInvoke(IAsyncResult result) {
			return ((MyAsyncResult)result).Result;
		}

		public object Invoke(Delegate method, object[] args) {
			if (Thread.CurrentThread == contextThread) {
				// If we are already in the context thread, directly invoke the method
				return method.DynamicInvoke(args);
			}
			else {
				// Otherwise, synchronously invoke the method in the context thread
				MyAsyncResult asyncResult = new MyAsyncResult(contextThread, method, args);
				asyncResult.AsyncWaitHandle.WaitOne();
				return asyncResult.Result;
			}
		}
		private class MyAsyncResult : IAsyncResult {
			private readonly object syncObject = new object();
			private readonly Thread contextThread;
			private readonly Delegate method;
			private readonly object[] args;
			private object result;

			public MyAsyncResult(object result) {
				this.result = result;
			}

			public MyAsyncResult(Thread contextThread, Delegate method, object[] args) {
				this.contextThread = contextThread;
				this.method = method;
				this.args = args;
				ThreadPool.QueueUserWorkItem(InvokeMethod);
			}

			private void InvokeMethod(object state) {
				result = method.DynamicInvoke(args);
				lock (syncObject) {
					Monitor.Pulse(syncObject);
				}
			}

			public object Result {
				get {
					if (Thread.CurrentThread != contextThread) {
						lock (syncObject) {
							Monitor.Wait(syncObject);
						}
					}
					return result;
				}
			}

			// Implementation of the IAsyncResult interface

			public object AsyncState => null;

			public WaitHandle AsyncWaitHandle => new ManualResetEvent(false);

			public bool CompletedSynchronously => true;

			public bool IsCompleted => true;
		}
		#endregion


	}

	internal class LogFileWatcher {
		// for LogFileWatcherWorker
		public delegate void SetConditionHandler(string folder, string pattern, bool regex);
		public event SetConditionHandler OnSetCondition;

		// for Consumer
		public event FileSystemEventHandler ContentChanged;
		public event FileSystemEventHandler TargetChanged;
		private ISynchronizeInvoke synchronizingObject = null;
		public ISynchronizeInvoke SynchronizingObject {
			set {
				synchronizingObject = value;
				if (worker != null)
					worker.SynchronizingObject = value;
			}
			get { return synchronizingObject; }
		}

		public string CurFileName { get { return worker.CurFileName; } }
		public string CurFilePath { get { return worker.CurFilePath; } }

		// private member
		private Thread workerThread = null;
		private LogFileWatcherWorker worker;
		private ManualResetEvent workerCreatedEvent = new ManualResetEvent(false);
		// public method
		public LogFileWatcher() {
		}
		public void Restart() {
			Stop();
			Start();
		}
		public void Start() {
			if (workerThread != null && workerThread.ThreadState == ThreadState.Running) return;
			workerCreatedEvent.Reset();
			workerThread = new Thread(StartWorker);
			workerThread.Name = "LogFileWatcherWorker";
			workerThread.Start();
			workerCreatedEvent.WaitOne();
		}
		public void Stop() {
			if (workerThread == null) return;
			workerThread.Abort();
			workerThread = null;
		}

		public void SetCondition(string folder, string pattern, bool regex) {
			Start();
			if(worker.InvokeRequired)
				worker.BeginInvoke(OnSetCondition, new object[] { folder, pattern, regex });
			else
				OnSetCondition(folder, pattern, regex);
		}

		private void StartWorker() {
			worker = new LogFileWatcherWorker(this);
			worker.TargetChangedHandler += Worker_TargetChanged;
			worker.ContentChangedHandler += Worker_ContentChanged;
			worker.SynchronizingObject = synchronizingObject;
			workerCreatedEvent.Set();
		}

		private void Worker_TargetChanged(object sender, FileSystemEventArgs e) {
			TargetChanged(sender, e);
		}

		private void Worker_ContentChanged(object sender, FileSystemEventArgs e) {
			ContentChanged(sender,e);
		}
	}
}

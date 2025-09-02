using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;

namespace LogViewer {
	/// <summary>
	/// A file FSWatcher that monitors a folder for the latest modified file matching a fileFilter.
	/// </summary>
	internal class FileWatcherEx : ISynchronizeInvoke {
		#region outgoing event
		public event FileSystemEventHandler ContentChangedHandler;
		public event FileSystemEventHandler TargetChangedHandler;
		private void OnFileChanged(object sender, FileSystemEventArgs e) {
			ContentChangedHandler?.Invoke(sender, e);
		}
		private void OnTargetChanged(object sender, FileSystemEventArgs e) { 
			TargetChangedHandler?.Invoke(sender, e);
		}
		#endregion outgoing event
		public string CurFilePath => FileWatcher.FullPath;
		private string FolderPath;
		private string FileNameFilter;
		private Regex FileNameRegex;

		private SingleFileWatcher FileWatcher;
		private SingleFolderWatcher FolderWatcher;

		public FileWatcherEx() {
			workerThread = new Thread(WorkerMethod) {
				IsBackground = true
			};
			workerThread.Start();
		}
		// worker thread
		private void WorkerMethod() {
			InitWatcher();
			while (true) {
				operationPendingEvent.WaitOne();
				lock (syncObject) {
					while (taskQueue.Count > 0) {
						AsyncResult task = (AsyncResult)taskQueue.Dequeue();
						task.RetVal = task.Method.DynamicInvoke(task.Args);
						task.Complete();
					}
					operationPendingEvent.Reset();
				}
			}
		}
		private void InitWatcher() {
			FolderWatcher = new SingleFolderWatcher(this);
			FileWatcher = new SingleFileWatcher(this);
		}
		#region start/stop
		public void Restart() {
			if (InvokeRequired) {
				BeginInvoke(new Action(Restart), null);
				return;
			}
			Stop();
			Start();
		}
		private void Start() {
			FolderWatcher.Start();
		}
		private void Stop() {
			FolderWatcher.Stop();
			FileWatcher.Stop();
		}
		#endregion start/stop

		public void SetCondition(string folderPath, string fileNameFilter = "", string fileNameRegex = null) {
			Regex regex = null;
			if (!string.IsNullOrEmpty(fileNameRegex)) {
				regex = new Regex(fileNameRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled);
			}
			SetCondition(folderPath, fileNameFilter, regex);
		}

		public void SetCondition(string folderPath, string fileNameFilter = "", Regex fileNameRegex = null) {
			if (InvokeRequired) {
				BeginInvoke(new Action<string, string, Regex>(SetCondition), new object[] {folderPath, fileNameFilter, fileNameRegex });
				return;
			}
			string expandedFolder = Environment.ExpandEnvironmentVariables(folderPath);
			if (FolderPath == expandedFolder && FileNameFilter == fileNameFilter && FileNameRegex == fileNameRegex) {
				//nothing changed
				return;
			}
			Stop();
			FolderPath = expandedFolder;
			FileNameFilter = fileNameFilter;
			FileNameRegex = fileNameRegex;
			FolderWatcher.SetCondition(FolderPath, FileNameFilter, FileNameRegex);
		}
		private void SetTarget(string filePath) {
			if (!File.Exists(filePath)) return;
			if (FileWatcher.FullPath != filePath) {
				FileWatcher.SetCondition(filePath);
			}
			if (!FileWatcher.IsRunning) {
				FileWatcher.Start();
				OnTargetChanged(this, new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetDirectoryName(filePath), Path.GetFileName(filePath)));
			}
		}
		private FileInfo GetLastWriteFile() {
			try {
				DirectoryInfo directoryInfo = new DirectoryInfo(FolderPath);
				IOrderedEnumerable<FileInfo> sortedFiles;
				if (string.IsNullOrEmpty(FileNameFilter)) {
					sortedFiles = directoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly).OrderByDescending(f => f.LastWriteTime);
				}
				else {
					sortedFiles = directoryInfo.GetFiles(FileNameFilter, SearchOption.TopDirectoryOnly).OrderByDescending(f => f.LastWriteTime);
				}
				if (FileNameRegex != null) {
					sortedFiles = sortedFiles
						.Where(f => FileNameRegex.IsMatch(f.Name))
						.OrderByDescending(f => f.LastWriteTime);
				}
				if (!sortedFiles.Any()) return null;
				return sortedFiles.ElementAt(0);
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

		#region ISynchronizeInvoke
		private readonly Thread workerThread;
		private readonly object syncObject = new object();
		private readonly ManualResetEvent operationPendingEvent = new ManualResetEvent(false);
		private readonly Queue<AsyncResult> taskQueue = new Queue<AsyncResult>();

		public bool InvokeRequired => Thread.CurrentThread != workerThread;

		public IAsyncResult BeginInvoke(Delegate method, object[] args) {
			lock (syncObject) {
				AsyncResult task = new AsyncResult(method, args);
				taskQueue.Enqueue(task);
				operationPendingEvent.Set();
				return task;
			}
		}

		public object EndInvoke(IAsyncResult result) {
			AsyncResult task = result as AsyncResult;
			task.AsyncWaitHandle.WaitOne();
			return task.RetVal;
		}

		public object Invoke(Delegate method, object[] args) {
			if (InvokeRequired) {
				var asyncResult = BeginInvoke(method, args);
				asyncResult.AsyncWaitHandle.WaitOne();
				return EndInvoke(asyncResult);
			}
			else {
				return method.DynamicInvoke(args);
			}
		}

		private class AsyncResult : IAsyncResult {
			internal Delegate Method;
			internal object[] Args;
			internal object RetVal;
			internal Exception Exception = null;
			internal bool Synchronous = false;
			public bool IsCompleted { get; private set; } = false;
			private ManualResetEvent ResetEvent = new ManualResetEvent(initialState: false);
			private object InvokeSyncObject = new object();
			public object AsyncState => null;

			public WaitHandle AsyncWaitHandle {
				get {
					lock (InvokeSyncObject) {
						if (IsCompleted) {
							ResetEvent.Set();
						}
					}
					return ResetEvent;
				}
			}

			public bool CompletedSynchronously {
				get {
					if (IsCompleted && Synchronous) {
						return true;
					}

					return false;
				}
			}


			internal AsyncResult(Delegate method, object[] args, bool synchronous = false) {
				Method = method;
				Args = args;
				Synchronous = synchronous;
			}

			~AsyncResult() {
				ResetEvent?.Close();
			}

			internal void Complete() {
				lock (InvokeSyncObject) {
					IsCompleted = true;
					ResetEvent?.Set();
				}
			}
		}
		#endregion ISynchronizeInvoke

		private void OnFolderChanged(object sender, FileSystemEventArgs e) {
			SelectLastWriteFile();
		}

		private class SingleFolderWatcher {
			public string FolderPath { get; private set; } = null;
			public string FileNameFilter { get; private set; } = "";
			public Regex FileNameRegex { get; private set; } = null;
			public bool IsRunning => CurState != State.Stopped;
			private enum State { Stopped, Waiting, Monitoring }
			private State CurState = State.Stopped;
			private readonly FileWatcherEx Parent;
			private FileSystemWatcher FSWatcher;
			private System.Timers.Timer WaitTimer; // wait for the folder to appear, or remote system is back online
			private System.Timers.Timer PingTimer; // ping the folder periodically because FileSystemWatcher is not reliable in SMB
			private string LastChangedFilePath = null;
			public SingleFolderWatcher(FileWatcherEx parent) {
				Parent = parent;
				FSWatcher = new FileSystemWatcher();
				FSWatcher.BeginInit();
				FSWatcher.EnableRaisingEvents = false;
				FSWatcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastWrite;
				FSWatcher.SynchronizingObject = parent;
				FSWatcher.Changed += FSWatcherOnChanged;
				FSWatcher.Created += FSWatcherOnChanged;
				FSWatcher.Deleted += FSWatcherOnDeleted;
				FSWatcher.EndInit();
				WaitTimer = new System.Timers.Timer(1000) { AutoReset = false, Enabled = false, SynchronizingObject = parent };
				WaitTimer.Elapsed += OnWaitTimerElapsed;
				PingTimer = new System.Timers.Timer(1000) { AutoReset = false, Enabled = false, SynchronizingObject = parent };
				PingTimer.Elapsed += OnPingTimerElapsed;
			}

			public void SetCondition(string folderPath, string fileNameFilter = "", Regex fileNameRegex = null) {
				Stop();
				FolderPath = folderPath;
				FileNameFilter = fileNameFilter;
				FileNameRegex = fileNameRegex;
				FSWatcher.Path = FolderPath;
				FSWatcher.Filter = FileNameFilter;
			}

			public void Start() {
				if (CurState != State.Stopped) return;
				if (string.IsNullOrEmpty(FolderPath)) return;
				CurState = State.Waiting;
				// use wait timer to check if the folder exists
				OnWaitTimerElapsed(WaitTimer, null);
				return;
			}

			public void Stop() {
				if (CurState == State.Stopped) return;
				CurState = State.Stopped;
				FSWatcher.EnableRaisingEvents = false;
				WaitTimer.Stop();
				PingTimer.Stop();
				LastChangedFilePath = null;
			}

			private void StartMonitoring() {
				if (CurState == State.Monitoring) return;
				CurState = State.Monitoring;
				WaitTimer.Stop();
				FSWatcher.EnableRaisingEvents = true;
				PingTimer.Start();
			}

			private void FSWatcherOnChanged(object sender, FileSystemEventArgs e) {
				if (File.Exists(e.FullPath)) {
					if(FileNameRegex != null && !FileNameRegex.IsMatch(Path.GetFileName(e.FullPath))) {
						// not match the fileFilter
						return;
					}
					lock (this) {
						// skip duplicate events
						if (e.FullPath == LastChangedFilePath) return;
						LastChangedFilePath = e.FullPath;
					}
				}
				Parent.OnFolderChanged(sender, e);
			}

			private void FSWatcherOnDeleted(object sender, FileSystemEventArgs e) {
				Stop();
				Start();
				Parent.OnFolderChanged(sender, e);
			}

			private void OnWaitTimerElapsed(object sender, ElapsedEventArgs e) {
				if (string.IsNullOrEmpty(FolderPath)) {
					Stop();
					return;
				}
				try {
					if (Utility.DirectoryExist(FolderPath)) {
						FSWatcherOnChanged(this, new FileSystemEventArgs(WatcherChangeTypes.Created, FolderPath, ""));
						// switch to monitoring, wait timer is not needed anymore
						StartMonitoring();
						return;
					}
				}
				catch (Exception ex) {
					System.Diagnostics.Debug.WriteLine($"SingleFolderWatcher.OnWaitTimerElapsed Error: {ex.Message}");
				}
				WaitTimer.Start();
			}
			private void OnPingTimerElapsed(object sender, ElapsedEventArgs e) {
				if (string.IsNullOrEmpty(FolderPath)) {
					Stop();
					return;
				}
				try {
					// with SMB sometimes we need to touch the folder for FileSystemWatcher to work
					Utility.DirectoryExist(FolderPath);
					PingTimer.Start();
				}
				catch (Exception ex) {
					System.Diagnostics.Debug.WriteLine($"SingleFolderWatcher.OnPingTimerElapsed Error: {ex.Message}");
					// restart
					Stop();
					Start();
					return;
				}
			}
		}
		private class SingleFileWatcher {
			public string FullPath { get; private set; }
			public bool IsRunning => CurState != State.Stopped;
			private enum State { Stopped, Waiting, Monitoring }
			private State CurState = State.Stopped;
			private readonly FileWatcherEx Parent;
			private FileSystemWatcher FSWatcher;
			private System.Timers.Timer WaitTimer;
			private System.Timers.Timer PingTimer;
			private DateTime LastWriteTime = DateTime.MinValue;
			private long LastLength = 0;

			public SingleFileWatcher(FileWatcherEx parent) {
				Parent = parent;
				FSWatcher = new FileSystemWatcher();
				FSWatcher.BeginInit();
				FSWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName;
				FSWatcher.SynchronizingObject = parent;
				FSWatcher.Created += FSWatcherOnChanged;
				FSWatcher.Changed += FSWatcherOnChanged;
				FSWatcher.Deleted += FSWatcherOnDeleted;
				FSWatcher.EnableRaisingEvents = false;
				FSWatcher.EndInit();
				WaitTimer = new System.Timers.Timer(500) { AutoReset = false, Enabled = false, SynchronizingObject = parent };
				WaitTimer.Elapsed += OnWaitTimerElapsed;
				PingTimer = new System.Timers.Timer(500) { AutoReset = false, Enabled = false, SynchronizingObject = parent };
				PingTimer.Elapsed += OnPingTimerElapsed;
			}
			public void SetCondition(string fullPath) {
				Stop();
				string path = Directory.GetParent(fullPath)?.FullName;
				string fileName = Path.GetFileName(fullPath);
				if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(fileName)) {
					System.Diagnostics.Debug.WriteLine($"SingleFileWatcher.SetCondition({fullPath}) invalid path");
					return;
				}
				FullPath = fullPath;
				FSWatcher.Path = Directory.GetParent(fullPath).FullName;
				FSWatcher.Filter = Path.GetFileName(fullPath);
				LastWriteTime = DateTime.MinValue;
				LastLength = 0;
			}
			public void Start() {
				if (CurState != State.Stopped) return;
				if (string.IsNullOrEmpty(FullPath)) return;
				CurState = State.Waiting;
				// use wait timer to check if the file exists
				OnWaitTimerElapsed(WaitTimer, null);
			}

			public void Stop() {
				if (CurState == State.Stopped) return;
				CurState = State.Stopped;
				FSWatcher.EnableRaisingEvents = false;
				WaitTimer.Stop();
				PingTimer.Stop();
				LastWriteTime = DateTime.MinValue;
				LastLength = 0;
			}

			private void StartMonitoring() {
				if (CurState == State.Monitoring) return;
				CurState = State.Monitoring;
				WaitTimer.Stop();
				FSWatcher.EnableRaisingEvents = true;
				PingTimer.Start();
			}

			private void FSWatcherOnChanged(object sender, FileSystemEventArgs e) {
				try {
					lock (this) {
						FileInfo info = new FileInfo(Path.Combine(FSWatcher.Path, FSWatcher.Filter));
						if (LastWriteTime == info.LastWriteTime && LastLength == info.Length) return;
						LastWriteTime = info.LastWriteTime;
						LastLength = info.Length;
						Parent.OnFileChanged(sender, e);
					}
				}
				catch (Exception ex) {
					System.Diagnostics.Debug.WriteLine($"SingleFileWatcher.FSWatcherOnChanged Error: {ex.Message}");
				}
			}

			private void FSWatcherOnDeleted(object sender, FileSystemEventArgs e) {
				Stop();
				Start();
				Parent.OnFileChanged(sender, e);
			}
			private void OnWaitTimerElapsed(object sender, ElapsedEventArgs e) {
				if (string.IsNullOrEmpty(FullPath)) {
					Stop();
					return;
				}
				try {
					if (File.Exists(FullPath)) {
						FSWatcherOnChanged(this, new FileSystemEventArgs(WatcherChangeTypes.Created, FSWatcher.Path, FSWatcher.Filter));
						// switch to monitoring, wait timer is not needed anymore
						StartMonitoring();
						return;
					}
				}
				catch (Exception ex) {
					System.Diagnostics.Debug.WriteLine($"SingleFileWatcher.OnWaitTimerElapsed Error: {ex.Message}");
				}
				WaitTimer.Start();
			}
			private void OnPingTimerElapsed(object sender, ElapsedEventArgs e) {
				if (string.IsNullOrEmpty(FullPath)) {
					Stop();
					return;
				}
				try {
					// with SMB sometimes we need to touch the file for FileSystemWatcher to work
					if (!File.Exists(FullPath)) {
						Stop();
						Start();
						return;
					}
					PingTimer.Start();
				}
				catch (Exception ex) {
					System.Diagnostics.Debug.WriteLine($"SingleFileWatcher.OnPingTimerElapsed Error: {ex.Message}");
					// restart
					Stop();
					Start();
					return;
				}
			}
		}

	}
}

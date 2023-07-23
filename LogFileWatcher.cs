using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;

namespace LogViewer {
	internal class LogFileWatcher {
		private readonly FileSystemWatcher logFolderWatcher = new FileSystemWatcher();
		private readonly FileSystemWatcher logFileWatcher = new FileSystemWatcher();
		private readonly Timer fileWatcherTimer = new Timer();
		private string pattern;
		private Regex patternRegex;
		private bool patternIsRegex;
		private string curFilePath;
		private ISynchronizeInvoke SynchronizingObject;
		private FileSystemEventHandler onContentChangedHandler;
		private FileSystemEventHandler onTargetChangedHandler;
		public LogFileWatcher(ISynchronizeInvoke synchronizingObject) {
			this.SynchronizingObject = synchronizingObject;
			InitWatcher();
		}
		public void Start() {
			StartFolderWatcher();
			StartFileWatcher();
		}
		private void StartFolderWatcher() {
			if (!logFolderWatcher.EnableRaisingEvents) {
				logFolderWatcher.EnableRaisingEvents = true;
			}
		}
		private void StartFileWatcher() {

			if (!logFileWatcher.EnableRaisingEvents) {
				logFileWatcher.EnableRaisingEvents = true;
			}
			if (!fileWatcherTimer.Enabled) {
				fileWatcherTimer.Enabled = true;
			}
		}
		public void Stop() {
			StopFolderWatcher();
			StopFileWatcher();
		}
		private void StopFolderWatcher() {
			logFolderWatcher.EnableRaisingEvents = false;
		}
		private void StopFileWatcher() {
			logFileWatcher.EnableRaisingEvents = false;
			fileWatcherTimer.Enabled = false;
		}
		public void SetCondition(string folder, string pattern, bool regex) {
			if (logFolderWatcher.Path == folder && this.pattern == pattern && patternIsRegex == regex) {
				return;
			}
			Stop();
			if (!Directory.Exists(folder)) {
				return;
			}
			logFolderWatcher.Path = logFileWatcher.Path = folder;
			this.pattern = pattern;
			patternIsRegex = regex;
			try {
				if (pattern.Length == 0) {
					logFolderWatcher.Filter = "*";
					patternRegex = new Regex(".*", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
				}
				else if (regex) {
					logFolderWatcher.Filter = "*";
					patternRegex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
				}
				else {
					logFolderWatcher.Filter = pattern;
					string regexStr = "^" + Regex.Escape(pattern)
						.Replace("\\*", ".*")
						.Replace("\\?", ".") + "$";
					patternRegex = new Regex(regexStr, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
				}
			}
			catch {
				patternRegex = new Regex(".*", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
			}
			StartFolderWatcher();
			SelectLastWriteFile();
		}
		public string GetCurFolderPath() { return curFilePath; }

		public string GetCurFileName() { return logFileWatcher.Filter; }

		public event FileSystemEventHandler ContentChanged {
			add {
				onContentChangedHandler = (FileSystemEventHandler)Delegate.Combine(onContentChangedHandler, value);
			}
			remove {
				onContentChangedHandler = (FileSystemEventHandler)Delegate.Remove(onContentChangedHandler, value);
			}
		}

		public event FileSystemEventHandler TargetChanged {
			add {
				onTargetChangedHandler = (FileSystemEventHandler)Delegate.Combine(onTargetChangedHandler, value);
			}
			remove {
				onTargetChangedHandler = (FileSystemEventHandler)Delegate.Remove(onTargetChangedHandler, value);
			}
		}
		private bool CheckFilename(string filename) {
			return patternRegex.IsMatch(filename);
		}
		private FileInfo GetLastWriteFile() {
			try {
				DirectoryInfo directoryInfo = new DirectoryInfo(logFolderWatcher.Path);
				IOrderedEnumerable<FileInfo> sortedFiles;
				if (pattern.Length == 0) {
					sortedFiles = directoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly).OrderByDescending(f => f.LastWriteTime);
				}
				else {
					if (patternIsRegex) {
						sortedFiles = directoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly).Where(f => patternRegex.IsMatch(f.Name)).OrderByDescending(f => f.LastWriteTime);
					}
					else {
						sortedFiles = directoryInfo.GetFiles(pattern, SearchOption.TopDirectoryOnly).OrderByDescending(f => f.LastWriteTime);
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
		private void SetTarget(string fullName) {
			if (curFilePath == fullName) return;
			StopFileWatcher();
			curFilePath = fullName;
			if (fullName.Length == 0) return;
			logFolderWatcher.Path = logFileWatcher.Path = Path.GetDirectoryName(fullName);
			logFileWatcher.Filter = Path.GetFileName(fullName);
			onTargetChangedHandler(this, new FileSystemEventArgs(WatcherChangeTypes.Created, logFolderWatcher.Path, logFileWatcher.Filter));
			StartFileWatcher();
		}
		private void InitWatcher() {
			logFolderWatcher.BeginInit();
			logFileWatcher.BeginInit();
			fileWatcherTimer.BeginInit();

			logFolderWatcher.EnableRaisingEvents = false;
			logFolderWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastWrite;
			logFolderWatcher.SynchronizingObject = SynchronizingObject;
			logFolderWatcher.Changed += logFolderWatcher_Created;
			logFolderWatcher.Created += logFolderWatcher_Created;
			logFolderWatcher.Renamed += logFolderWatcher_Created;

			logFileWatcher.EnableRaisingEvents = false;
			logFileWatcher.NotifyFilter = NotifyFilters.LastWrite;
			logFileWatcher.SynchronizingObject = SynchronizingObject;
			logFileWatcher.Changed += logFileWatcher_Changed;
			logFileWatcher.Deleted += logFileWatcher_Deleted;

			fileWatcherTimer.Enabled = false;
			fileWatcherTimer.SynchronizingObject = SynchronizingObject;
			fileWatcherTimer.Elapsed += fileWatcherTimer_Elapsed;

			logFolderWatcher.EndInit();
			logFileWatcher.EndInit();
			fileWatcherTimer.EndInit();
		}
		#region filesystem event
		private void logFolderWatcher_Created(object sender, FileSystemEventArgs e) {
			if (e.FullPath == curFilePath || !CheckFilename(e.Name)) return;
			SetTarget(e.FullPath);
		}

		DateTime fileWatcherTimer_lastWriteTime = DateTime.Now;
		private void fileWatcherTimer_Elapsed(object sender, ElapsedEventArgs e) {
			if (curFilePath.Length == 0) return;
			try {
				FileInfo info = new FileInfo(curFilePath);
				if (fileWatcherTimer_lastWriteTime != info.LastWriteTime) {
					fileWatcherTimer_lastWriteTime = info.LastWriteTime;
					onContentChangedHandler(this, new FileSystemEventArgs(WatcherChangeTypes.Changed, info.DirectoryName, info.Name));
				}
			}
			catch (Exception ex) {
				System.Diagnostics.Debug.WriteLine("Error occurred: " + ex.Message);
			}
		}

		private void logFileWatcher_Changed(object sender, FileSystemEventArgs e) {
			try {
				FileInfo info = new FileInfo(curFilePath);
				if (fileWatcherTimer_lastWriteTime != info.LastWriteTime) {
					fileWatcherTimer_lastWriteTime = info.LastWriteTime;
					onContentChangedHandler(this, new FileSystemEventArgs(WatcherChangeTypes.Changed, info.DirectoryName, info.Name));
				}
			}
			catch (Exception ex) {
				System.Diagnostics.Debug.WriteLine("Error occurred: " + ex.Message);
			}
		}
		private void logFileWatcher_Deleted(object sender, FileSystemEventArgs e) {
			StopFileWatcher();
		}
		#endregion filesystem event
	}
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace LogViewer {
	[Serializable]
	public class HighlightItem : IXmlSerializable {
		public Color HighlightColor = Color.Black;
		public Regex PatternRegex {get; private set;}
		public HighlightItem() { }
		public bool SetPatternRegex(string  pattern) {
			try {
				PatternRegex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
			}
			catch {
				PatternRegex = null;
				return false;
			}
			return true;
		}
		public XmlSchema GetSchema() {
			return null;
		}

		public void ReadXml(XmlReader reader) {
			reader.MoveToContent();
			SetPatternRegex(reader["PatternRegex"]);
			HighlightColor = Color.FromArgb(Convert.ToInt32(reader["HighlightColor"]));
			reader.ReadStartElement();
		}

		public void WriteXml(XmlWriter writer) {
			writer.WriteAttributeString("PatternRegex", PatternRegex.ToString());
			writer.WriteAttributeString("HighlightColor", HighlightColor.ToArgb().ToString());
		}
	}
	[Serializable]
	public class AppConfig {
		public string FolderPath { get; set; }
		public string Filename { get; set; }
		public bool FilenameIsRegex { get; set; }
		public string Include { get; set; }
		public string Exclude { get; set; }
		public int ReadLastLines { get; set; }
		public bool Persistent { get; set; }
		public bool BufferedDraw { get; set; }
		public bool AlwaysOnTop { get; set; }
		public bool WordWrap { get; set; }
		public string FontFamilyName { get; set; }
		public float FontSize { get; set; }
		public int FontStyle { get; set; }
		public int X { get; set; } = -1;
		public int Y { get; set; } = -1;
		public int W { get; set; } = -1;
		public int H { get; set; } = -1;
		public List<HighlightItem> HighlightItems { get; set; }
	}
	internal class ConfigManager {
		public AppConfig config = new AppConfig();
		public string path = null;
		public void SaveConfig() {
			try {
				var serializer = new XmlSerializer(typeof(AppConfig));
				using (var writer = new StreamWriter(path)) {
					serializer.Serialize(writer, config);
				}
			}
			catch (Exception ex) {
				System.Diagnostics.Debug.WriteLine("Error saving config: " + ex.Message);
			}
		}
		public bool LoadConfig() {
			try {
				var serializer = new XmlSerializer(typeof(AppConfig));
				using (var reader = new StreamReader(path)) {
					config = (AppConfig)serializer.Deserialize(reader);
					return true;
				}
			}
			catch (Exception ex) {
				System.Diagnostics.Debug.WriteLine("Error loading config: " + ex.Message);
				return false;
			}
		}
	}
}

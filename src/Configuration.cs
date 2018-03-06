using System;
using System.Text;
using System.Xml;
using System.IO;
using System.Threading.Tasks;

namespace AcerFanControl {


	class Configuration {

		public const string DefaultConfigFileName = "acerfancontrol.config";

		private GeneralOptions _general;
		private TVicPortOptions _ports;
		private FanProfile[] _allProfiles;
		
		public GeneralOptions General => _general;
		public TVicPortOptions Ports => _ports;
		public FanProfile[] AllProfiles => _allProfiles;


		public Configuration() {
			LoadFanConfigXML(Properties.Resources.defaultConfigXML);
		}

		public async Task LoadDefaultConfigFile() {
			string sxml = await Configuration.ReadFanConfigFile(DefaultConfigFileName);
			LoadFanConfigXML(sxml);
		}
		public async Task SaveDefaultConfigFile() {
			await Configuration.SaveFanConfigFile(DefaultConfigFileName, Properties.Resources.defaultConfigXML);
		}

		private void LoadFanConfigXML(string sxml) {
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(sxml);

			GeneralOptions general = new GeneralOptions(doc.SelectSingleNode("//configuration/general"));
			TVicPortOptions ports = new TVicPortOptions(doc.SelectSingleNode("//configuration/tvicports"));

			//List<FanProfile> profiles = new List<FanProfile>();
			XmlNodeList xmlProfileDefs = doc.SelectNodes("//configuration/fanprofiles/profile");
			FanProfile[] profiles = new FanProfile[xmlProfileDefs.Count];
			for (int i = 0; i < profiles.Length; i++) { profiles[i] = (new FanProfile(xmlProfileDefs[ i ], general)); }
			
			//If we got this far, there was no exception.  So it's safe to actually use the values. 
			this._general = general;
			this._ports = ports;
			this._allProfiles = profiles;
		}

		private static async Task<bool> SaveFanConfigFile(string sFilePath, string sxml) {
			using (FileStream fs = new FileStream(sFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)) {
				using(StreamWriter writer = new StreamWriter(fs, Encoding.UTF8)) {
					await writer.WriteAsync(sxml);
					return true;
				}
			}
		}

		private static async Task<string> ReadFanConfigFile(string sFilePath) {
			using (FileStream fs = new FileStream(sFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
				using (StreamReader reader = new StreamReader(fs, System.Text.Encoding.UTF8)) {
					return await reader.ReadToEndAsync();
				}
			}
		}

	}

	class GeneralOptions {
		public readonly Int16 interval;
		public readonly Int16 timeout;
		public readonly Int16 spinwait;
		public readonly byte UpHysteresis;
		public readonly byte DownHysteresis;
		/// <summary> Scale for converting fanspeed percent to 255 values. </summary>
		public readonly float fanspeedscale;
		public readonly float cputempscale;

		public GeneralOptions(XmlNode node) {
			interval = Utils.ParseInt16(node.SelectSingleNode(nameof(interval)).InnerText);
			timeout = Utils.ParseInt16(node.SelectSingleNode(nameof(timeout)).InnerText);
			spinwait = Utils.ParseInt16(node.SelectSingleNode(nameof(spinwait)).InnerText);

			XmlNode hysteresis = node.SelectSingleNode("hysteresis");
			UpHysteresis = Utils.ParseByte(hysteresis?.Attributes["up"]?.Value);
			DownHysteresis = Utils.ParseByte(hysteresis?.Attributes["down"]?.Value);

			fanspeedscale = float.Parse(node.SelectSingleNode(nameof(fanspeedscale)).InnerText);
			cputempscale = float.Parse(node.SelectSingleNode(nameof(cputempscale)).InnerText);
		}
	}

	class TVicPortOptions {
		public struct ByteOperationExpression {
			public byte mask;	
			public string operation;
			public byte Value;
			public bool DoOperation(byte val) {
				switch (operation) {
				case "!=": return (val & mask) != Value;
				case "==": return (val & mask) == Value;
				case ">": return (val & mask) > Value;
				case ">=": return (val & mask) >= Value;
				case "<": return (val & mask) < Value;
				case "<=": return (val & mask) <= Value;			
				}
				return false;
			}
			public ByteOperationExpression(XmlNode node) {
				mask = Utils.ParseByte(node.Attributes[ nameof(mask) ].Value);
				operation = node.Attributes[ nameof(operation) ].Value;
				Value = Utils.ParseByte(node.InnerText);
			}
		}

		public UInt16 LocationAddress;
		public UInt16 DataAddress;

		public UInt16 StatusAddress;
		public ByteOperationExpression StatusRead;
		public ByteOperationExpression StatusWrite;


		public UInt16 InstructionAddress;
		public byte InstructRead;
		public byte InstructWrite;

		public UInt16 ControlAddress;
		public byte ControlManual;
		public byte ControlBIOS;

		public UInt16 CPUTempAddress;
		public UInt16 FanSpeedAddress;


		internal TVicPortOptions(XmlNode node) {
			LocationAddress = Utils.ParseUInt16(node.SelectSingleNode("location").Attributes[ "address" ].Value);
			DataAddress = Utils.ParseUInt16(node.SelectSingleNode("data").Attributes[ "address" ].Value);

			XmlNode tmp = node.SelectSingleNode("status");
			StatusAddress = Utils.ParseUInt16(tmp.Attributes[ "address" ].Value);
			StatusRead = new ByteOperationExpression(tmp.SelectSingleNode("canread"));
			StatusWrite = new ByteOperationExpression(tmp.SelectSingleNode("canwrite"));

			tmp = node.SelectSingleNode("instruction");
			InstructionAddress = Utils.ParseUInt16(tmp.Attributes[ "address" ].Value);
			InstructRead = Utils.ParseByte(tmp.Attributes[ "read" ].Value);
			InstructWrite = Utils.ParseByte(tmp.Attributes[ "write" ].Value);

			tmp = node.SelectSingleNode("control");
			ControlAddress = Utils.ParseUInt16(tmp.Attributes[ "address" ].Value);
			ControlManual = Utils.ParseByte(tmp.Attributes[ "manual" ].Value);
			ControlBIOS = Utils.ParseByte(tmp.Attributes[ "bios" ].Value);

			CPUTempAddress = Utils.ParseUInt16(node.SelectSingleNode("cputemp").Attributes[ "address" ].Value);
			FanSpeedAddress = Utils.ParseUInt16(node.SelectSingleNode("fanspeed").Attributes[ "address" ].Value);
		}

	}


	class FanProfile {
		public struct TemperaturePoint {
			public byte Temperature;
			public byte FanSpeed;
		}

		public string Name;
		public int Interval;
		public byte UpHysteresis;
		public byte DownHysteresis;
		public bool IsDefault;
		public bool IsBIOS;
		public TemperaturePoint[] Points { get; set; }
		public TrayIcon.ProfileMenuItem MenuItem { get; set; }

		internal FanProfile() { }

		internal FanProfile(XmlNode node, GeneralOptions defaults) {
			Name = node.Attributes["name"].Value;
			this.Interval = Utils.ParseInt32(node.SelectSingleNode("interval")?.InnerText, defaults.interval);

			XmlNode hysteresis = node.SelectSingleNode("hysteresis");
			UpHysteresis = Utils.ParseByte(hysteresis?.Attributes["up"]?.Value, defaults.UpHysteresis);
			DownHysteresis = Utils.ParseByte(hysteresis?.Attributes["down"]?.Value, defaults.DownHysteresis);

			bool.TryParse(node.Attributes[ "default" ]?.Value, out IsDefault);

			XmlNodeList cfgPoints = node.SelectNodes("point");
			this.Points = new TemperaturePoint[ cfgPoints.Count ];
			for (var i = 0; i < this.Points.Length; i++) {
				node = cfgPoints[ i ];
				ref TemperaturePoint pt = ref Points[i];
				pt.Temperature = byte.Parse(node.Attributes[ "temp" ].Value);
				pt.FanSpeed = byte.Parse(node.Attributes[ "fan" ].Value);
			}
		}
	}




}

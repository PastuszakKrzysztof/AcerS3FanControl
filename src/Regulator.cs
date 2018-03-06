using System;
using System.Collections.Generic;

namespace AcerFanControl {

	class Regulator : IDisposable {
		private Configuration Config => Program.Config;
		private EmbeddedController EC => Program.FanController;

		private bool _biosControl = true;
		private byte _fanSpeed = 0;
		private byte _priorTemp = 0;

		private System.Windows.Forms.Timer _timer; //Use Windows Forms Timer so we don't have to create another thread or marshall any contexts. 
		private FanProfile _profile;

		public byte CPUTemperature { get; private set; } = 0;

		public bool BiosControl {
			get => _biosControl;
			set {
				if (_biosControl != value) {
					_biosControl = value;
					EC.BiosControl = value;
				}
			}
		}

		public byte FanSpeed {
			get { return _fanSpeed; }
			set {
				if (value > 100) value = 100;
				if (value != _fanSpeed) {
					if (BiosControl) { BiosControl = false; }
					EC.FanSpeed = (byte)((Config.General.fanspeedscale < 0 ? 255 : 0) + (value * Config.General.fanspeedscale));
					_priorTemp = CPUTemperature;
					_fanSpeed = value;
				}
			}
		}

		public Regulator() {
		}

		internal void RunProfile(FanProfile profile) {
			void handleTimerTick(Object ob, EventArgs ev) => RunProfileInternal();

			if (_timer == null) {
				_timer = new System.Windows.Forms.Timer() { Interval = Config.General.interval };
				_timer.Tick += handleTimerTick;
				_timer.Start();
			}
			_profile = profile;
			_priorTemp = 0;
			_timer.Interval = profile.Interval;
			RunProfileInternal();			
		}

		private DateTime[] _exceptionTimes = new DateTime[2];
		private void RunProfileInternal() {
			try {
				CPUTemperature = (byte)((EC.CPUTemperature - (Config.General.cputempscale < 0 ? 255 : 0)) / Config.General.cputempscale);

				if (_profile.IsBIOS) {
					_fanSpeed = (byte)((EC.FanSpeed - (Config.General.fanspeedscale < 0 ? 255 : 0)) / Config.General.fanspeedscale);
					BiosControl = true;
				} else {
					if (CPUTemperature >= _priorTemp + _profile.UpHysteresis || CPUTemperature <= _priorTemp - _profile.DownHysteresis) {
						byte lTemp = 0, lFan = 0;
						byte hTemp = 99, hFan = 100;

						for (var i = 0; i < _profile.Points.Length; i++) {
							byte pTemp = _profile.Points[i].Temperature;
							byte pFan = _profile.Points[i].FanSpeed;
							if (pTemp <= CPUTemperature && pTemp > lTemp) { lTemp = pTemp; lFan = pFan; }
							if (pTemp >= CPUTemperature && pTemp < hTemp) { hTemp = pTemp; hFan = pFan; }
						}

						if (lTemp == hTemp) {
							FanSpeed = hFan;
						} else {
							int divisor = hTemp - lTemp;
							if (divisor == 0) { divisor = 1; }
							float slope = (hFan-lFan)/(float)divisor;
							FanSpeed = (byte)(lFan + slope * (CPUTemperature - lTemp));
						}
					}
				}
				Program.TrayIconCtx.Update(_profile, CPUTemperature, _fanSpeed);
			} catch (Exception ex) {
				//Only throw if 3rd exception over last 10 cycles. 
				DateTime cutoff = DateTime.Now.AddMilliseconds(-(_profile.Interval * 10));		
				if(_exceptionTimes[0] > cutoff  && _exceptionTimes[1] > cutoff) {
					throw ex;
				} else {
					Array.Sort(_exceptionTimes); //So the earlier date is towards the front.
					_exceptionTimes[ 0 ] = DateTime.Now;
				}
			}
		}


		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls
		public void Dispose() {
			if (!disposedValue) {
				_timer.Dispose();
				disposedValue = true;
			}
		}
#endregion

	}

}

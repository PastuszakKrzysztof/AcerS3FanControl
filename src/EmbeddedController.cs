using System;
using System.Runtime.InteropServices;

namespace AcerFanControl {

	class EmbeddedController {

		[DllImport("TVicPort.dll", EntryPoint = "OpenTVicPort", ExactSpelling = false, CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern uint OpenTVicPort();
		[DllImport("TVicPort.dll", EntryPoint = "CloseTVicPort", ExactSpelling = false, CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern void CloseTVicPort();
		[DllImport("TVicPort.dll", EntryPoint = "IsDriverOpened", ExactSpelling = false, CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern uint IsDriverOpened();
		[DllImport("TVicPort.dll", EntryPoint = "WritePort", ExactSpelling = false, CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern void WritePort(ushort PortAddr, byte bValue);
		[DllImport("TVicPort.dll", EntryPoint = "ReadPort", ExactSpelling = false, CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern byte ReadPort(ushort PortAddr);


		private System.Diagnostics.Stopwatch _watch = new System.Diagnostics.Stopwatch();
		private bool IsTVicPortAvailable = false;

		private GeneralOptions General => Program.Config.General;
		private TVicPortOptions Ports => Program.Config.Ports;

		public EmbeddedController() {

		}
		~EmbeddedController() {
			if (IsDriverOpened() == 1) { CloseTVicPort(); }
		}

		public Exception CheckTvicPortAvailability() {
			try {
				if (IsDriverOpened() != 1) { OpenTVicPort(); }
				IsTVicPortAvailable = true;
			} catch(Exception ex) {
				return ex;
			}
			return null;
		}

		private void WaitForReadReadyFlag() {
			while (Ports.StatusRead.DoOperation(ReadPort(Ports.StatusAddress))) {
				if (_watch.ElapsedMilliseconds > General.timeout) {
					throw new TimeoutException("WaitForReadReadyFlag: EC timed out while waiting for RBF to set");
				}
				System.Threading.Thread.SpinWait(General.spinwait);
			}
		}

		private void WaitForWriteReadyFlag() {
			while (Ports.StatusWrite.DoOperation(ReadPort(Ports.StatusAddress))) {
				if (_watch.ElapsedMilliseconds > General.timeout) {
					throw new TimeoutException("WaitForWriteReadyFlag: EC timed out while waiting for WBF to clear");
				}
				System.Threading.Thread.SpinWait(General.spinwait);
			}
		}


		private byte ReadEC(ushort addr) {
			_watch.Restart();
			if (!IsTVicPortAvailable) { return 0; }
			if (IsDriverOpened() != 1) { OpenTVicPort(); }
			WaitForWriteReadyFlag();
			WritePort(Ports.InstructionAddress, Ports.InstructRead);
			WaitForWriteReadyFlag();
			WritePort(Ports.LocationAddress, (byte)addr);
			WaitForReadReadyFlag();
			return ReadPort(Ports.DataAddress);
		}

		private void WriteEC(ushort addr, byte value) {
			_watch.Restart();
			if (!IsTVicPortAvailable) { return; }
			if (IsDriverOpened() != 1) { OpenTVicPort(); }
			WaitForWriteReadyFlag();
			WritePort(Ports.InstructionAddress, Ports.InstructWrite);
			WaitForWriteReadyFlag();
			WritePort(Ports.LocationAddress, (byte)addr);
			WaitForWriteReadyFlag();
			WritePort(Ports.DataAddress, value);
		}


		internal byte CPUTemperature => ReadEC(Ports.CPUTempAddress);

		internal bool BiosControl {
			get => ReadEC(Ports.ControlAddress) == Ports.ControlBIOS;
			set => WriteEC(Ports.ControlAddress, value ? Ports.ControlBIOS : Ports.ControlManual);
		}

		internal byte FanSpeed {
			get => ReadEC(Ports.FanSpeedAddress);
			set => WriteEC(Ports.FanSpeedAddress, value);
		}

	}

}

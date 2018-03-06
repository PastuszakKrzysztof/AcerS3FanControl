using System;
using System.Windows.Forms;


namespace AcerFanControl {
	static class Program {
		public const string ProgramName = "Acer Fan Control";

		public static Configuration Config { get; private set; }
		public static EmbeddedController FanController { get; private set; }
		public static Regulator Regulator { get; private set; }
		public static TrayIcon TrayIconCtx { get; private set; }

		
		[STAThread]
		static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
		
			try {
				Config = new Configuration();
				FanController = new EmbeddedController();
				Regulator = new Regulator();
				TrayIconCtx = new TrayIcon();
				TrayIconCtx.Init();
				Application.Idle += OnIdle;
				Application.ApplicationExit += OnExit;
				Application.Run(TrayIconCtx);

			} catch (Exception e) {
				MessageBox.Show(e.ToString(), ProgramName, MessageBoxButtons.OK, MessageBoxIcon.Error);
			} finally {
				Regulator.BiosControl = true;
			}
		}

		private static void OnIdle(object sender, EventArgs ev) {
			Application.Idle -= OnIdle;
			GC.Collect(2, GCCollectionMode.Forced, true, true);
		}
		private static void OnExit(object sender, EventArgs ev) {
			try {
				FanController.BiosControl = true;
				Regulator.BiosControl = true;
			} catch { }
		}

	}
}

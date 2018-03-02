using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

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

				Application.Run(TrayIconCtx);

			} catch (Exception e) {
				MessageBox.Show(e.ToString(), ProgramName, MessageBoxButtons.OK, MessageBoxIcon.Error);
			} finally {
				Regulator.BiosControl = true;
			}
		}

	}
}

using System;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace VpmfServiceManager
{
	public class Setup
	{
		public string operation { get; set; }
		public DateTime? start_date_time { get; set; }			// Data Avvio prima esecuzione
		public int start_delay { get; set; }					// Ritardo primo avvio rispetto a start_date_time in secondi
		public int restart_delay { get; set; }					// Pausa tra un' esecuzione e la successiva in secondi
		public  string Host { get; set; }
		public string Archivio { get; set; }
		public int DbPort { get; set; }
		public string User { get; set; }
		public string Password { get; set; }

		public Setup()
		{
			operation = "";
			start_date_time = null;
			start_delay = 0;
			restart_delay = 0;
			Host = "";
			Archivio = "";
			DbPort = 0;
			User = "";
			Password = "";
		}
}
}
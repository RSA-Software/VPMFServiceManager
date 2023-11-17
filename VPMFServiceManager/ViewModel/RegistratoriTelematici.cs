using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VpmfServiceManager.ViewModel
{
	public class RegistratoriTelematici
	{
		public string matricola { get; set; }
		public string qrcode { get; set; }

		public RegistratoriTelematici()
		{
			var rt = this;
			rt.matricola = "";
			rt.qrcode = "";
		}
	}
}

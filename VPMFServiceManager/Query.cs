using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VpmfServiceManager
{
	public class Parameter
	{
		public string name { get; set; }
		public OdbcType type { get; set; }
		public object value { get; set; }
	}

	public class Query
	{
		public string query { get; set; }
		public List<Parameter> parameters { get; set; }

	}


}

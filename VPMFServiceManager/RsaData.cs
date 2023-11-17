using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VpmfServiceManager
{

	public class FacileJson<T>
	{
		public int RecordsTotal { get; set; }

		public IList<T> Data { get; set; }
	}

	public class GenericJson
	{
		public int RecordsTotal { get; set; }
		public IList<ExpandoObject> Data { get; set; }
	}

	public class ErrorJson<T>
	{
		public string Description { get; set; }
		public IList<T> Errors { get; set; }
	}
}

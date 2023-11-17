using System.Net;
using RestSharp;
using System.Text.Json;

namespace VpmfServiceManager
{
	public class ResponseHttpError
	{
		public int status_code { get; set; }
		public string Message { get; set; }
	}

	public class Response<T>
	{
		public bool IsSuccessful { get; set; }
		public T json { get; set; }
		public ResponseHttpError error { get; set; }
	}

	public static class RestResponseExstensions
	{
		public static Response<T> GetResponse<T>(this RestResponse restResponse)
		{
			var response = new Response<T>();

			response.IsSuccessful = restResponse.IsSuccessful;
			if (restResponse.StatusCode == HttpStatusCode.OK)
			{
				var options = new JsonSerializerOptions();
				options.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
				response.json = JsonSerializer.Deserialize<T>(restResponse.Content ?? "");
			}
			else if (restResponse.StatusCode == 0)
			{
				response.error = new ResponseHttpError
				{
					status_code = 0,
					Message = restResponse.ErrorMessage ?? ""
				};
			}
			else if (restResponse.StatusCode == HttpStatusCode.NotFound)
			{
				response.error = new ResponseHttpError
				{
					status_code = (int)HttpStatusCode.NotFound,
					Message = restResponse.Content ?? ""
				};
			}
			else
			{
				try
				{
					response.error = JsonSerializer.Deserialize<ResponseHttpError>(restResponse.Content ?? "");
				}
				catch (JsonException)
				{
					response.error = new ResponseHttpError
					{
						Message = restResponse.Content
					};
				}
				if (response.error == null) response.error = new ResponseHttpError();
				response.error.status_code = (int)restResponse.StatusCode;
			}
			return response;
		}
	}
}

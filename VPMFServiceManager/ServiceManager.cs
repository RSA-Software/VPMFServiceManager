using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text.Json;
using System.Threading;
using VpmfServiceManager.ViewModel;
using System.Data.Odbc;
using Chilkat;
using System.Runtime.InteropServices;

namespace VpmfServiceManager
{
	public enum Operation
	{
		AECHECK = 0,
	}


	public partial class VpmfServiceManager : ServiceBase
	{
		private static List<Thread> _thList;
		private static bool _exit;
		private static string _appName;
		private static string _logName;
		private static FacileJson<Setup> _impo;


		public VpmfServiceManager()
		{
			_exit = false;
			_appName = "VpmfServiceManager";
			_logName = "Application";
			_thList = new List<Thread>();
			_impo = new FacileJson<Setup>();
			InitializeComponent();
		}

		public static string GetConnectionString(Setup options)
		{
			var strcon = $"Driver={{PostgreSQL ANSI}}; Server={options.Host}; Database={options.Archivio}; Port={options.DbPort}; UID={options.User}; PWD={options.Password}; QUERY_TIMEOUT=18000; MaxVarcharSize=512; BoolsAsChar=0";
			return (strcon);
		}

		public static string GetUnlockCode()
		{
			return ("BEWASR.CB1032026_PVchMWYK5Rkk");
		}

		public static string GetBetween(string str_source, string str_start, string str_end)
		{
			if (str_source != null)
			{
				if (str_source.Contains(str_start) && str_source.Contains(str_end))
				{
					var start = str_source.IndexOf(str_start, 0, StringComparison.CurrentCulture) + str_start.Length;
					var end = str_source.IndexOf(str_end, start, StringComparison.CurrentCulture);
					return str_source.Substring(start, end - start);
				}
			}
			return "";
		}

		public static string GetLogPath()
		{
			var path = System.Reflection.Assembly.GetExecutingAssembly().Location;
			path = Path.GetDirectoryName(path);
			if (path != null) Directory.SetCurrentDirectory(path);

#if (DEBUG)
			// ReSharper disable once PossibleNullReferenceException
			path = Directory.GetParent(path).FullName;

			// ReSharper disable once PossibleNullReferenceException
			path = Directory.GetParent(path).FullName;
#endif
			return (path);
		}


		public static async void WriteLog(string text)
		{
			if (string.IsNullOrWhiteSpace(text)) return;

			var path = GetLogPath();
			if (!path.EndsWith("\\") && !path.EndsWith("/")) path += "//";
			path += "log";
			Directory.CreateDirectory(path);
			path += "/";

			var fname = "AgenziaEntrateCheck.txt";

			try
			{
				var sw = new StreamWriter(path + fname, true);
				await sw.WriteAsync($"{DateTime.Now} - {text}");
				await sw.WriteAsync("\r\n");
				sw.Close();
			}
			catch (Exception ex)
			{
				EventLog.WriteEntry(GetAppName(), $"Invalid log_path ({path}) - {ex.Message}", EventLogEntryType.Warning);
			}
			Debug.WriteLine($"{DateTime.Now} - {text}");
		}


		public static void Check(ref OdbcCommand cmd, string schema, string matricola, string qrcode)
		{
			var glob = new Global();
			if (!glob.UnlockBundle(GetUnlockCode())) throw new Exception("Chilkat Unlock Code Invalid");

			var path = GetLogPath();
			if (!path.EndsWith("/") && !path.EndsWith("\\")) path += "/";
			path += "log";
			Directory.CreateDirectory(path);

			path += $"/{schema}";
			Directory.CreateDirectory(path);

			var end_path = path;
			end_path += $"/{matricola}";

			path += $"/{matricola}.html";

			var http = new Http();
			var html = http.QuickGetStr(qrcode);
			if (http.LastMethodSuccess)
			{
				var update = false;
				DateTime? cas_data_ult_trasmissione = null;
				DateTime? cas_ult_ver = null;
				var cas_piva_ult_ver = "";
				DateTime? cas_data_agg_firmware = null;
				DateTime? cas_data_release_firmware = null;
				var cas_firmware = "";
				var founded = 0;

				var cks = new CkString();
				cks.append(html);
				cks.saveToFile(path, "utf-8");

				//
				// Estraiamo la data dell'ultima trasmissione
				//
				var search = "<li>Ultima Trasmissione da Dispositivo</li>";
				if (html.Contains(search))
				{
					var start = html.IndexOf(search, 0, StringComparison.CurrentCulture) + search.Length;
					if (start != -1)
					{
						var str = html.Substring(start);
						search = "Data:";
						if (str.Contains(search))
						{
							start = str.IndexOf(search, 0, StringComparison.CurrentCulture) + search.Length;
							if (start != -1)
							{
								start = str.IndexOf(search, 0, StringComparison.CurrentCulture) + search.Length;
								str = str.Substring(start).Trim();
								str = str.Substring(0, 10);
								try
								{
									founded++;
									cas_data_ult_trasmissione = DateTime.ParseExact(str, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
									update = true;
								}
								catch (Exception)
								{
								}
							}
						}
					}
				}

				//
				// Estraiamo la data dell'ultima verificazione periodica
				//
				search = "<li>Ultima Verificazione Periodica</li>";
				if (html.Contains(search))
				{
					var start = html.IndexOf(search, 0, StringComparison.CurrentCulture) + search.Length;
					if (start != -1)
					{
						var str = html.Substring(start);
						search = "Data:";
						if (str.Contains(search))
						{
							start = str.IndexOf(search, 0, StringComparison.CurrentCulture) + search.Length;
							if (start != -1)
							{
								start = str.IndexOf(search, 0, StringComparison.CurrentCulture) + search.Length;
								str = str.Substring(start).Trim();
								str = str.Substring(0, 10);
								try
								{
									founded++;
									cas_ult_ver = DateTime.ParseExact(str, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
									update = true;
								}
								catch (Exception)
								{
								}
							}
						}
					}
				}

				//
				// Estraiamo la partita iva dell'ultima verificazione periodica
				//
				search = "<li>Ultima Verificazione Periodica</li>";
				if (html.Contains(search))
				{
					var start = html.IndexOf(search, 0, StringComparison.CurrentCulture) + search.Length;
					if (start != -1)
					{
						var str = html.Substring(start);
						search = "PIVA Laboratorio:";
						if (str.Contains(search))
						{
							start = str.IndexOf(search, 0, StringComparison.CurrentCulture) + search.Length;
							if (start != -1)
							{
								start = str.IndexOf(search, 0, StringComparison.CurrentCulture) + search.Length;
								str = str.Substring(start).Trim();
								str = str.Substring(0, 11);
								try
								{
									founded++;
									cas_piva_ult_ver = str.Trim();
									update = true;
								}
								catch (Exception)
								{
								}
							}
						}
					}
				}

				//
				// Estraiamo la data dell'ultimo aggiornamento firmware
				//
				search = "<li>Ultima versione software del dispositivo</li>";
				if (html.Contains(search))
				{
					var start = html.IndexOf(search, 0, StringComparison.CurrentCulture) + search.Length;
					if (start != -1)
					{
						var str = html.Substring(start);
						search = "Data Invio Manutenzione:";
						if (str.Contains(search))
						{
							start = str.IndexOf(search, 0, StringComparison.CurrentCulture) + search.Length;
							if (start != -1)
							{
								start = str.IndexOf(search, 0, StringComparison.CurrentCulture) + search.Length;
								str = str.Substring(start).Trim();
								str = str.Substring(0, 10);
								try
								{
									founded++;
									cas_data_agg_firmware = DateTime.ParseExact(str, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
									update = true;
								}
								catch (Exception)
								{
								}
							}
						}
					}
				}

				//
				// Estraiamo la data dell'ultima release firmware
				//
				search = "<li>Ultima versione software del dispositivo</li>";
				if (html.Contains(search))
				{
					var start = html.IndexOf(search, 0, StringComparison.CurrentCulture) + search.Length;
					if (start != -1)
					{
						var str = html.Substring(start);
						search = "Data Rilascio:";
						if (str.Contains(search))
						{
							start = str.IndexOf(search, 0, StringComparison.CurrentCulture) + search.Length;
							if (start != -1)
							{
								start = str.IndexOf(search, 0, StringComparison.CurrentCulture) + search.Length;
								str = str.Substring(start).Trim();
								str = str.Substring(0, 10);
								try
								{
									founded++;
									cas_data_release_firmware = DateTime.ParseExact(str, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
									update = true;
								}
								catch (Exception)
								{
								}
							}
						}
					}
				}

				//
				// Estraiamo la vertsione del firmvare
				//
				search = "<li>Ultima versione software del dispositivo</li>";
				if (html.Contains(search))
				{
					var start = html.IndexOf(search, 0, StringComparison.CurrentCulture) + search.Length;
					if (start != -1)
					{
						var str = html.Substring(start);
						search = "Versione:";
						if (str.Contains(search))
						{
							start = str.IndexOf(search, 0, StringComparison.CurrentCulture) + search.Length;
							if (start != -1)
							{
								//start = str.IndexOf(search, 0, StringComparison.CurrentCulture)/* + search.Length*/;
								str = GetBetween(str, search, "</li>").Trim();
								try
								{
									founded++;
									cas_firmware = str.ToUpper();
									update = true;
								}
								catch (Exception)
								{
								}
							}
						}
					}
				}

				if (update)
				{
					var first = true;
					var sql = $"UPDATE {schema}.casse SET";
					if (cas_data_ult_trasmissione.HasValue)
					{
						first = false;
						sql += " cas_data_ult_trasmissione = ?";
					}
					if (cas_ult_ver.HasValue)
					{
						if (!first)
							sql += ",";
						else
							first = false;
						sql += " cas_ult_ver_ae = ?";
					}
					if (!string.IsNullOrWhiteSpace(cas_piva_ult_ver))
					{
						if (!first)
							sql += ",";
						else
							first = false;
						sql += " cas_piva_ult_ver = ?";
					}
					if (cas_data_agg_firmware.HasValue)
					{
						if (!first)
							sql += ",";
						else
							first = false;
						sql += " cas_data_agg_firmware = ?";
					}
					if (cas_data_release_firmware.HasValue)
					{
						if (!first)
							sql += ",";
						else
							first = false;
						sql += " cas_data_release_firmware = ?";
					}
					if (!string.IsNullOrWhiteSpace(cas_firmware))
					{
						if (!first) sql += ",";
						sql += "  cas_firmware = ?";
					}
					sql += ", cas_last_update = NOW(), cas_last_check = Now() WHERE cas_codice = ?";

					cmd.CommandText = sql;
					cmd.Parameters.Clear();
					if (cas_data_ult_trasmissione.HasValue) cmd.Parameters.Add("@cas_data_ult_trasmissione", OdbcType.DateTime).Value = cas_data_ult_trasmissione;
					if (cas_ult_ver.HasValue) cmd.Parameters.Add("@cas_ult_ver_ae", OdbcType.DateTime).Value = cas_ult_ver;
					if (!string.IsNullOrWhiteSpace(cas_piva_ult_ver)) cmd.Parameters.Add("@cas_piva_ult_ver", OdbcType.VarChar).Value = cas_piva_ult_ver;
					if (cas_data_agg_firmware.HasValue) cmd.Parameters.Add("@cas_data_agg_firmware", OdbcType.DateTime).Value = cas_data_agg_firmware;
					if (cas_data_release_firmware.HasValue) cmd.Parameters.Add("@cas_data_release_firmware", OdbcType.DateTime).Value = cas_data_release_firmware;
					if (!string.IsNullOrWhiteSpace(cas_firmware)) cmd.Parameters.Add("@cas_firmware", OdbcType.VarChar).Value = cas_firmware;
					cmd.Parameters.Add("@cas_codice", OdbcType.VarChar).Value = matricola;
					cmd.ExecuteNonQuery();

					end_path += $"_{founded}.html";
					File.Delete(end_path); 
					File.Move(path, end_path);
				}
			}
			else
			{
				WriteLog($"AgenziaEntrateCheck {schema} - {matricola} : {http.LastErrorText}");
			}
		}


		//
		// Scarico dati fatture elettroniche dal sito di Digithera
		//
		public static void AgenziaEntrateCheck(object info)
		{
			var setup = (Setup)info;
			if (string.IsNullOrWhiteSpace(setup.Host))
			{
				WriteLog("AgenziaEntrateCheck : Host non indicato");
				EventLog.WriteEntry(_appName, "AgenziaEntrateCheck : Host non indicato", EventLogEntryType.Warning);
				return;
			}
			if (string.IsNullOrWhiteSpace(setup.Archivio))
			{
				WriteLog("AgenziaEntrateCheck : Archivio non indicato");
				EventLog.WriteEntry(_appName, "AgenziaEntrateCheck : Archivio non indicato", EventLogEntryType.Warning);
				return;
			}
			if (string.IsNullOrWhiteSpace(setup.User))
			{
				WriteLog("AgenziaEntrateCheck : User non indicato");
				EventLog.WriteEntry(_appName, "AgenziaEntrateCheck : User non indicato", EventLogEntryType.Warning);
				return;
			}
			if (string.IsNullOrWhiteSpace(setup.Password))
			{
				WriteLog("AgenziaEntrateCheck : Password non indicata");
				EventLog.WriteEntry(_appName, "AgenziaEntrateCheck : Password non indicata", EventLogEntryType.Warning);
				return;
			}
			if (setup.DbPort == 0) setup.DbPort = 5432;

			WriteLog($"AgenziaEntrateCheck : Starting {DateTime.Now}");
			Debug.WriteLine($"AgenziaEntrateCheck : Starting {DateTime.Now}");
			if (setup.start_date_time != null)
			{
				if (DateTime.Compare(setup.start_date_time.Value, DateTime.Now) > 0)
				{
					var tsp = (setup.start_date_time.Value - DateTime.Now);
					Thread.Sleep(tsp);
				}
			}
			if (setup.start_delay > 0) Thread.Sleep(setup.start_delay * 1000);

			while (!_exit)
			{ 
				try
				{
					using (var connection = new OdbcConnection(GetConnectionString(setup)))
					{
						connection.Open();
						var cmd = new OdbcCommand { Connection = connection };

						var schema_arr = new List<string>();
						cmd.CommandText = "SELECT aec_schema FROM aecheck ORDER BY aec_codice";
						var reader = cmd.ExecuteReader();
						while (reader.Read())
						{
							if (_exit) break;
							var schema = reader.GetString(0).Trim();
							schema_arr.Add(schema);
						}
						reader.Close();

						foreach (var schema in schema_arr)
						{
							if (_exit) break;
							var matr_arr = new List<RegistratoriTelematici>();
#if DEBUG
							cmd.CommandText = $"SELECT cas_codice, cas_qrcode FROM {schema}.casse WHERE cas_qrcode <> '' AND cas_dismissione IS NULL";
#else
							cmd.CommandText = $"SELECT cas_codice, cas_qrcode FROM {schema}.casse WHERE cas_qrcode <> '' AND cas_dismissione IS NULL AND (cas_last_check IS NULL OR cas_last_check < (NOW() - INTERVAL '12 HOURS'))";
#endif
							reader = cmd.ExecuteReader();
							while (reader.Read())
							{
								if (_exit) break;
								var reg = new RegistratoriTelematici();
								reg.matricola = reader.GetString(0).Trim();
								reg.qrcode = reader.GetString(1).Trim();
								matr_arr.Add(reg);
							}
							reader.Close();

							foreach (var matr in matr_arr)
							{
								if (_exit) break;
								try
								{
									Check(ref cmd, schema, matr.matricola, matr.qrcode);
								}
								catch (Exception ex)
								{
									WriteLog($"AgenziaEntrateCheck {schema} - {matr.matricola} : {ex.Message}");
								}
							}
						}
						connection.Close();
					}
				}
				catch (Exception ex)
				{
					if (_exit) break;
					WriteLog(ex.Message);
					EventLog.WriteEntry(_appName, $"AgenziaEntrateCheck : {ex.Message}", EventLogEntryType.Warning);
					Thread.Sleep(10*1000);
					if (_exit) break;
					continue;
				}
				if (!_exit)
				{
					if (setup.restart_delay >= 0)
					{
						Debug.WriteLine($"AgenziaEntrateCheck - Sleeping {setup.restart_delay} sec");
						Thread.Sleep(setup.restart_delay * 1000);
					}
					else if (setup.start_date_time != null)
					{
						var days = (int)(DateTime.Now - setup.start_date_time.Value).TotalDays;
						var restart = setup.start_date_time.Value.AddDays(days);
						while (DateTime.Compare(restart, DateTime.Now) < 0)
						{
							restart = restart.AddDays(1);
						}

						if (setup.restart_delay != -1) restart = restart.AddDays((-setup.restart_delay) - 1);
						var tsp = (restart - DateTime.Now);
						Debug.WriteLine($"AgenziaEntrateCheck : Sleeping {tsp}");
						Thread.Sleep(tsp);
					}

					Debug.WriteLine("AgenziaEntrateCheck : ReStart Loop");
				}
			}
			WriteLog("AgenziaEntrateCheck : End thread");
			Debug.WriteLine("AgenziaEntrateCheck : End thread");
		}

		public static bool GetExitStatus()
		{
			return _exit;
		}

		public static string GetAppName()
		{
			return _appName;
		}


		protected override void OnStart(string[] args)
		{
			var path = System.Reflection.Assembly.GetExecutingAssembly().Location;
			path = Path.GetDirectoryName(path);
			if (path != null) Directory.SetCurrentDirectory(path);

#if DEBUG
			Debugger.Launch();
#endif

#if (DEBUG)
			path = Directory.GetParent(path).FullName;
			path = Directory.GetParent(path).FullName;
			path += @"\cfg\VpmfServiceManagerD.json";
#else
			path += @"\cfg\VpmfServiceManager.json";
#endif

			if (!EventLog.SourceExists(_appName)) EventLog.CreateEventSource(_appName, _logName);
			try
			{
				var r = new StreamReader(path);
				var json = r.ReadToEnd();
				_impo = JsonSerializer.Deserialize<FacileJson<Setup>>(json);
				if (_impo == null) throw new Exception("File impostazioni non caricato!");
				if (_impo.RecordsTotal == 0 || _impo.Data == null) throw new Exception("Nessun record nel file delle impostazioni!");
				if (_impo.RecordsTotal !=  _impo.Data.Count) throw new Exception("RecordsTotal non coincide con il numero delle impostazioni presenti");
			}
			catch (DirectoryNotFoundException ex)
			{
				EventLog.WriteEntry(_appName, ex.Message + "\n\n" + path, EventLogEntryType.Warning);
				Stop();
				return;
			}
			catch (Exception ex)
			{
				EventLog.WriteEntry(_appName, ex.Message, EventLogEntryType.Warning);
				Debug.Write(ex.Message);
				Stop();
				return;
			}

			foreach (var setup in _impo.Data)
			{
				switch (setup.operation.Trim().ToUpper())
				{
					case "AECHECK":
					{
						var th = new Thread(AgenziaEntrateCheck);
						_thList.Add(th);
						th.Start(setup);
					}
					break;
				}
			}
		}

		protected override void OnStop()
		{
			_exit = true;
			foreach (var th in _thList)
			{
				th.Abort();
			}
		}
	}
}

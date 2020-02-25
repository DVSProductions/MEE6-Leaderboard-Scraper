using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Helpers;
namespace MEE6_Leaderboard_Scraper {
	partial class Program {
		private static string TryGetName(string url) {
			try {
				return new mee6Format(Json.Decode(new WebClient().DownloadString(url))).guild.name;
			}
			catch (WebException wex) {
				if (wex.Response is HttpWebResponse hwr) {
					if ((int)hwr.StatusCode == 429)
						Console.WriteLine("Error. You have been temporairly banned by Cloudflare for sending too many Requests");
					else if ((int)hwr.StatusCode == 404)
						Console.WriteLine("This server does not exist. Only enter the number from the URL");
					else
						Console.WriteLine(wex);
				}
				else
					Console.WriteLine(wex);
				return null;
			}
			catch (Exception ex) {
				Console.WriteLine(ex);
				return null;
			}
		}
		static string GetURL(string id, int page) => GetURL(id, page, 999);
		static string GetURL(string id, int page, int entries) => "https://mee6.xyz/api/plugins/levels/leaderboard/" + id + "?limit=" + entries + "&page=" + page;
		static bool Checkfor(string toCheck, char[] finds) {
			foreach (var c in toCheck)
				foreach (var b in finds)
					if (c == b)
						return true;
			return false;
		}
		static void Main(string[] args) {
			Console.WriteLine("DVSProductions MEE6 Scraper");
			var page = 0;
			var dat = new List<mee6Format>();
			var hasArgs = args.Length > 0 ? 1 : 0;
			string id = hasArgs == 1 ? args[0] : null, serverName;
			do {
				if (hasArgs != 1) {
					Console.Write("Enter Leaderboard ID> ");
					id = Console.ReadLine();
				}
				hasArgs--;
				serverName = TryGetName(GetURL(id, 0, 5));
			} while (string.IsNullOrEmpty(serverName));
			Console.WriteLine("Connecting to " + serverName);

			var rng = new Random();
			var autooffset = 0;
			do {
				Console.Write("Scraping page " + page);
				var ok = false;
				try {
					System.Threading.Thread.Sleep(rng.Next(500, 1500) + autooffset);
					var sw = System.Diagnostics.Stopwatch.StartNew();
					dat.Add(new mee6Format(System.Web.Helpers.Json.Decode(new WebClient().DownloadString(GetURL(id, page++)))));
					autooffset = (int)sw.ElapsedMilliseconds;
					ok = true;
				}
				catch (WebException wex) {
					if (wex.Response is HttpWebResponse hwr && (int)hwr.StatusCode == 429) 
						Console.WriteLine(" Failed. You have been temporairly banned by Cloudflare for sending too many Requests");					
					else
						Console.WriteLine($" Failed with error: {wex.Message}");
				}
				catch (Exception ex) {
					Console.WriteLine($" Failed with error: {ex.Message}");
				}
				if (!ok) 
					page--;
				else
					Console.WriteLine(" done.");
			} while (dat[dat.Count - 1].players.Count != 0);
			var allPlayers = dat[0].players;
			for (var n = 1; n < dat.Count; n++)
				allPlayers.InsertRange(0, dat[n].players);
			allPlayers.Sort();
			for (var n = 0; n < allPlayers.Count; n++)
				allPlayers[n].position = n + 1;
			var sep = ';';
			var file = new List<string>() { allPlayers[0].GetHeader(sep) };
			var filename = (Checkfor(serverName, Path.GetInvalidFileNameChars()) ? id : serverName) + " " + DateTime.Now.ToShortDateString() + ".csv";
			foreach (var p in allPlayers)
				file.Add(p.ToCSV(sep));
			Console.WriteLine("Found " + allPlayers.Count + " users");
			File.WriteAllLines(filename, file);
			Console.WriteLine("Written " + filename);
			if (hasArgs != 0) {
				Console.Write("Press any key to continue...");
				Console.ReadKey(true);
			}
		}
	}
}

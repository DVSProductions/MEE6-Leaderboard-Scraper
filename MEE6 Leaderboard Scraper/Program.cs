using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Helpers;
namespace System {
	interface ICSVable {
		string ToCSV(char spacer);
		string GetHeader(char spacer);
	}
}
namespace MEE6_Leaderboard_Scraper {
	class Program {
		struct mee6Format {
			public struct mee6Guild {
				public string icon { get; set; }
				public string id { get; set; }
				public string name { get; set; }
				public bool premium { get; set; }
				public mee6Guild(dynamic source) {
					icon = source.icon;
					id = source.id;
					name = source.name;
					premium = source.premium;
				}
			}
			public struct mee6player : IComparable<mee6player>, ICSVable {
				public string avatar { get; set; }
				public List<int> detailed_xp { get; set; }
				public string discriminator { get; set; }
				public string guild_id { get; set; }
				public string id { get; set; }
				public int level { get; set; }
				public string username { get; set; }
				public int xp { get; set; }
				public mee6player(dynamic djo) {
					avatar = djo.avatar;
					detailed_xp = new List<int>();
					for (int n = 0; n < djo.detailed_xp.length; n++)
						detailed_xp.Add((int)djo.detailed_xp[n]);
					discriminator = djo.discriminator;
					guild_id = djo.guild_id;
					id = djo.id;
					level = djo.level;
					//var from = Encoding.UTF8;
					//var to = Encoding.GetEncoding(1252);
					//username = to.GetString(Encoding.Convert(from, to, from.GetBytes(djo.username)));
					username = djo.username;
					xp = djo.xp;
				}
				public int CompareTo(mee6player other) {
					return other.xp.CompareTo(xp);
				}
				public string ToCSV(char spacer) {
					return username + spacer + " " + xp;
				}
				public string GetHeader(char spacer) {
					return "username" + spacer + " xp";
				}
			}
			public bool admin { get; set; }
			public object banner_url { get; set; }
			public mee6Guild guild { get; set; }
			public int page { get; set; }
			public object player { get; set; }
			public List<mee6player> players { get; set; }
			public mee6Format(dynamic djo) {
				admin = djo.admin;
				banner_url = djo.banner_url;
				guild = new mee6Guild(djo.guild);
				page = djo.page;
				player = djo.player;
				players = new List<mee6player>();
				foreach (dynamic elem in (DynamicJsonArray)djo.players)
					players.Add(new mee6player(elem));
			}

		}
		private static string TryGetName(string url) {
			try {
				return new mee6Format(Json.Decode(new WebClient().DownloadString(url))).guild.name;
			}
			catch {
				return null;
			}
		}
		static string GetURL(string id, int page) => "https://mee6.xyz/api/plugins/levels/leaderboard/" + id + "?page=" + page;
		static bool Checkfor(string toCheck, char[] finds) {
			foreach (char c in toCheck)
				foreach (char b in finds)
					if (c == b)
						return true;
			return false;
		}
		static void Main(string[] args) {

			Console.WriteLine("DVSProductions MEE6 Scraper");
			int page = 0;
			var dat = new List<mee6Format>();
			var hasInput = args.Length > 0 ? 1 : 0;
			string id = hasInput > 0 ? args[0] : null, serverName;
			do {
				if (hasInput == 0) {
					Console.Write("Enter Leaderboard ID> ");
					id = Console.ReadLine();
				}
				hasInput--;
				serverName = TryGetName(GetURL(id, 0));
			} while (string.IsNullOrEmpty(serverName));
			Console.WriteLine("Connecting to " + serverName);
			do {
				Console.Write("Scraping page " + page);
				dat.Add(new mee6Format(Json.Decode(new WebClient().DownloadString(GetURL(id, page++)))));
				Console.WriteLine(" done.");
			} while (dat[dat.Count - 1].players.Count != 0);
			var allPlayers = dat[0].players;
			for (int n = 1; n < dat.Count; n++)
				allPlayers.InsertRange(0, dat[n].players);
			allPlayers.Sort();
			char sep = ';';
			var file = new List<string>() { allPlayers[0].GetHeader(sep) };
			string filename = (Checkfor(serverName, Path.GetInvalidFileNameChars()) ? id : serverName) + " " + DateTime.Now.ToShortDateString() + ".csv";
			foreach (var p in allPlayers)
				file.Add(p.ToCSV(sep));
			Console.WriteLine("Found " + allPlayers.Count + " users");
			File.WriteAllLines(filename, file);
			Console.WriteLine("Written " + filename);
			if (hasInput != 0) {
				Console.Write("Press any key to continue...");
				Console.ReadKey(true);
			}
		}
	}
}

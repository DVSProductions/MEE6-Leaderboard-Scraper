using System;
using System.Collections.Generic;
using System.Web.Helpers;
namespace MEE6_Leaderboard_Scraper {
	partial class Program {
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
			public class mee6player : ACSVable, IComparable<mee6player> {
				public string avatar { get; set; }
				public List<int> detailed_xp { get; set; }
				public string discriminator { get; set; }
				public string guild_id { get; set; }
				public string id { get; set; }
				public int level { get; set; }
				public string username { get; set; }
				public int xp { get; set; }
				public int position;
				public int messages;
				public mee6player(dynamic djo) {
					avatar = djo.avatar;
					detailed_xp = new List<int>();
					for (int n = 0; n < djo.detailed_xp.length; n++)
						detailed_xp.Add((int)djo.detailed_xp[n]);
					discriminator = djo.discriminator;
					id = djo.id;
					guild_id = djo.guild_id;
					level = djo.level;
					username = djo.username;
					xp = djo.xp;
					messages = xp / 20;
				}
				public int CompareTo(mee6player other) => other.xp.CompareTo(xp);
				public override string ToCSV(char spacer) =>
					 CreateData(new string[] { id, username, xp.ToString(), position.ToString() }, spacer);
				public override string GetHeader(char spacer) =>
					CreateData(new string[] { "id", "Username", "xp", "Position" }, spacer);


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
				foreach (var elem in (DynamicJsonArray)djo.players)
					players.Add(new mee6player(elem));
			}

		}
	}
}

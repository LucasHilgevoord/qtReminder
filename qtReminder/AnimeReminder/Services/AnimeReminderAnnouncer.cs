using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using qtReminder.AnimeReminder.Models;
using qtReminder.ImageSearch;

namespace qtReminder.AnimeReminder.Services
{
    public static class AnimeReminderAnnouncer
    {
        private readonly static Dictionary<int, AnnounceModel> 
            AnnouncedAnime = new Dictionary<int, AnnounceModel>();

        public static void Announce(AnnounceModel model)
        {
            if (!AnnouncedAnime.TryGetValue(model.AnimeGuildModel.AnimeID, out var anime)) return;

            if (anime.AnnouncedMessage != null)
                AnnounceEdit(anime).Wait();
            else if(model.Episode > model.AnimeGuildModel.LastAnnouncedEpisode && anime.AnnouncedMessage == null) AnnounceNew(model).Wait();
        }
        
        private static async Task AnnounceNew(AnnounceModel model)
        {
            var embed = CreateEmbed(model);
            var mentions = GetMentions(model.AnimeGuildModel);
            
            var client = Program.ServiceProvider.GetRequiredService<DiscordSocketClient>();
            var guild = client.GetGuild(model.AnimeGuildModel.Guild);
            var channel = guild?.GetTextChannel(model.AnimeGuildModel.Channel);

            if (channel == null) return; // This would be very bad!!!
            
            var message = await channel.SendMessageAsync(mentions, embed:embed);
            AnnouncedAnime[model.AnimeGuildModel.AnimeID].AnnouncedMessage = message;
            
            Quotes.AnnounceQuotes.AddQuoteWaiter(model.AnimeGuildModel);

        }

        private static async Task AnnounceEdit(AnnounceModel announceModel)
        {
            var embed = CreateEmbed(announceModel);

            if (!AnnouncedAnime.TryGetValue(announceModel.AnimeGuildModel.AnimeID, out var model) ||
                model.AnnouncedMessage == null) return;

            await model.AnnouncedMessage.ModifyAsync(x => x.Embed = embed);
        }

        private static string GetMentions(AnimeGuildModel model)
        {
            return model.SubscribedUsers.Aggregate("", (current, user) => current + $"<@{user}>");
        }
        
        public static void UpdateAnime(AnnounceModel announceModel)
        {
            if (!AnnouncedAnime.ContainsKey(announceModel.AnimeGuildModel.AnimeID))
            {
                // If it doesn't contain the key, it's very simple. Just add it,
                // and then add the self destruct anonymous method.
                
                // check if the subgroup is even correct
                var (subgroup, _, _) = GetSubgroupFromAnnounceModel(announceModel);
                
                if (subgroup == null || !announceModel.AnimeGuildModel
                        .WantedSubgroupTitle.Select(x=>x.ToLower())
                            .Contains(subgroup.ToLower())) return;
                
                AnnouncedAnime.Add(announceModel.AnimeGuildModel.AnimeID, announceModel);
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(TimeSpan.FromMinutes(20));
                    AnnouncedAnime.Remove(announceModel.AnimeGuildModel.AnimeID);
                }).ConfigureAwait(false);
            }
            else
            {
                // If it does contain the key, it's a bit harder..
                var a = AnnouncedAnime[announceModel.AnimeGuildModel.AnimeID];
                
                // Get the important subgroup. the parameter announceModel
                // will have only 1 correct subgroup.
                // So we will get the current subgroup and the "new" subgroup and compare them against each other.
                // If the important subgroup equals the new subgroup, we edit the anime model to be that one. :)) 
                var result  = GetSubgroupFromAnnounceModel(announceModel);
                string newSubgroup = result.Item1;
                if (result.quality == Quality.Unknown) return;
                string currentSubgroup = a.QualityLinks[result.quality].Subgroup;
                string importantSubgroup = GetImportantSubgroup(newSubgroup, currentSubgroup, a.AnimeGuildModel);

                if (string.Equals(importantSubgroup, newSubgroup, StringComparison.CurrentCultureIgnoreCase))
                {
                    a.QualityLinks[result.quality] = new SubgroupTorrentLink(result.link, result.subgroup);
                }
            }
        }

        /// <summary>
        /// This method is only for temporary models, which are created by the checker.
        /// It will find the first correct Quality link where the subgroup is not null, and return it.
        /// </summary>
        private static (string subgroup, Quality quality, string link) GetSubgroupFromAnnounceModel(AnnounceModel announceModel)
        {
            foreach (var q in announceModel.QualityLinks)
            {
                if (q.Value.Link != null) 
                    return (q.Value.Subgroup, q.Key, q.Value.Link);
            }

            return (null, Quality.Unknown, null); // I mean... this should theoratically not happen... but ehh...
        }

        private static string GetImportantSubgroup(string s1, string s2, AnimeGuildModel m)
        {
            if (s1 == null) return s2;
            if (s2 == null) return s1;
            
            // if this is confusing, future me, this lowers all strings in the array
            int is1 = Array.IndexOf(m.WantedSubgroupTitle.Select(x=>x.ToLower()).ToArray(), s1.ToLower());
            int is2 = Array.IndexOf(m.WantedSubgroupTitle.Select(x=>x.ToLower()).ToArray(), s2.ToLower());
            
            is1 = is1 < 0 ? int.MaxValue : is1;
            is2 = is2 < 0 ? int.MaxValue : is2;

            return is1 < is2 ? s1 : s2;
        }

        private static Embed CreateEmbed(AnnounceModel announceModel)
        {
            var title = announceModel.AnimeGuildModel.AnimeTitle;
            var embedBuilder = new EmbedBuilder()
                .WithTitle($"Episode {announceModel.Episode} of {title.EnglishTitle ?? title.RomajiTitle} just came out!");

            var links = new StringBuilder();

            foreach (var q in announceModel.QualityLinks)
            {
                if (q.Value.Link == null) continue;

                string subgroup = q.Value.Subgroup == null ? "" : $"[{q.Value.Subgroup}]";
                links.AppendLine($"[{subgroup} - {QualityString.GetQualityString(q.Key)}]({q.Value.Link})");
            }

            string imageLink = null;
            try
            {
                var images = DuckDuckGoImageSearch.SearchImage(announceModel.AnimeGuildModel.AnimeTitle.RomajiTitle);
                imageLink = images[Program.Randomizer.Next(images.Length)];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var quote = Quotes.AnnounceQuotes.GetRandomQuote(announceModel.AnimeGuildModel.Guild);
            if (quote != null)
            {
                embedBuilder.AddField(x =>
                {
                    x.Name = $"Wise words of {quote.Author}";
                    x.Value = quote.Message;
                });
            }

            if (imageLink != null) embedBuilder.ImageUrl = imageLink;
            embedBuilder.Description = links.ToString();
            embedBuilder.Color = new Color(255, 255, 255);

            return embedBuilder.Build();
        }
    }
}
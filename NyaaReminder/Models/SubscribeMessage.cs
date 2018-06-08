using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using qtReminder.Nyaa;

namespace qtReminder.Models
{
    /// <summary>
    ///     A message that was sent when somebody is subscribed, used to also subscribe to this
    ///     anime when the emoji has been clicked.
    /// </summary>
    public class SubscribeMessage
    {
        public AnimeChannel channel;
        public IUserMessage userMessage;

        public Action Disposed;
        
        public SubscribeMessage(AnimeChannel channel, IUserMessage userMessage)
        {
            if (channel == null || userMessage == null) throw new NoNullAllowedException("Kanker noob.");

            this.channel = channel;
            this.userMessage = userMessage;

            // start a timer that waits 5 minutes before it deletes the emoji so it can no longer be checked.
            var timer = new Timer();
            timer.Interval = TimeSpan.FromMinutes(5).TotalMilliseconds;
            timer.AutoReset = false;
            timer.Elapsed += async (sender, args) =>
            {
                try
                {
                    await userMessage.RemoveAllReactionsAsync();
                    
                }
                catch (Exception)
                {
                    /* ignore dumbass bitch */
                }
                
                Disposed?.Invoke();
            };
        }

        public async Task UserSubscribe(ulong ID)
        {
            channel.SubscribeUser(ID);
            var messageEmbed = userMessage.Embeds.FirstOrDefault();
            var author = messageEmbed?.Author.GetValueOrDefault();
            var embed = await TorrentReminder.MakeAnimeSubscriptionEmbed(channel, null);

            // if the embed has a author, add it to this one as well! !!!!!!!
            if (author.HasValue)
                embed = embed.ToEmbedBuilder()
                    .WithAuthor(author?.Name, author?.IconUrl, author?.Url).Build();

            try
            {
                await userMessage.ModifyAsync(properties => { properties.Embed = embed; });
            }
            catch (Exception)
            {
                // ignore fucking noob Rider.
            }
        }
    }
}
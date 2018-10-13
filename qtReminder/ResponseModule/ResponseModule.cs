using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace qtReminder.ResponseModule
{
    /// <summary>
    /// Base class of the response module.
    /// Commands can subscribe to this, which will be invoked
    /// whenever a message event fires, it will then look who sent the message
    /// and execute the command.
    ///
    /// When a message returns true, it means that the execution has been completed.
    /// If that is the case, it will be deleted from the list and all associated messages with it.
    /// </summary>
    public static class ResponseModule
    {
        private static readonly List<GuildUserWaiter> guildUserWaiterList = new List<GuildUserWaiter>();

        public static void AddWaiter(GuildUserWaiter guildUserWaiter)
        {
            guildUserWaiterList.Add(guildUserWaiter);

            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(TimeSpan.FromMinutes(guildUserWaiter.LifeTime));
                if (guildUserWaiterList.Contains(guildUserWaiter))
                {
                    guildUserWaiterList.Remove(guildUserWaiter);

                    if (!guildUserWaiter.IsReactionWaiter || guildUserWaiter.ParentMessage == null
                        || !(guildUserWaiter.ParentMessage is IUserMessage botMessage)) return;

                    try
                    {
                       await botMessage.RemoveAllReactionsAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            });
        }

        public static async Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, 
            ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (!reaction.User.IsSpecified || reaction.User.Value.IsBot || !(channel is IGuildChannel guildChannel)) return;

            var xxx = guildUserWaiterList;
            var candidates = guildUserWaiterList
                .Where(x => x.GuildId == guildChannel.GuildId
                            && x.IsReactionWaiter).ToList();

            if (candidates.Count == 0) return;

            foreach (var g in candidates)
            {
                if (g.OnlyWaitsForOwner && reaction.UserId != g.UserId) return;

                if (!(await g.ReactionAction(message.Id, reaction, g.Params))) continue;

                try
                {
                    await g.ParentMessage.DeleteAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        
        public static async Task MessageReceived(SocketMessage message)
        {
            // cannot wait for messages in channel
            if (!(message.Channel is IGuildChannel guildChannel)) return;
            if (message.Author.IsBot) return;
            
            var xxx = guildUserWaiterList;
            var candidates = guildUserWaiterList
                .Where(x => x.GuildId == guildChannel.GuildId && 
                            !x.IsReactionWaiter).ToList();

            // no candidates found, return
            if (candidates.Count == 0) return;
            
            foreach (var g in candidates)
            {
                if (g.OnlyWaitsForOwner && message.Author.Id != g.UserId) continue;
                
                // if the message is c, delete it AND continue.
                if (message.Content.ToLower() == "c")
                {
                    g.AddAssociatedMessage(message);
                    await g.Deleting();
                    guildUserWaiterList.Remove(g);
                    continue;
                }
                
                // if the action succeeded, delete it from the list.
                if (!await g.Action(message, g.Params)) continue;
                
                g.AddAssociatedMessage(message);
                await g.Deleting();
                guildUserWaiterList.Remove(g);
            }
        }
    }
}
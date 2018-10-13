using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace qtReminder.ResponseModule
{
    public class GuildUserWaiter
    {
        public GuildUserWaiter(ulong guildId, ulong userId, Func<SocketMessage, object, Task<bool>> action, 
            object @params = null, bool WaitsForOwner = true, bool DeleteOnSuccess = true, double minuteLifeTime = 5.0f)
        {
            GuildId = guildId;
            UserId = userId;
            Action = action;
            CreatedDate = DateTime.Now;
            Params = @params;
            DeleteMessagesOnSuccess = DeleteOnSuccess;
            OnlyWaitsForOwner = WaitsForOwner;
            IsReactionWaiter = false;
            LifeTime = minuteLifeTime;
        }

        public GuildUserWaiter(ulong guildId, ulong userId, Func<ulong, SocketReaction, object, Task<bool>> action,
            object @params = null, bool WaitsForOwner = true, double minuteLifeTime = 5.0f)
        {
            GuildId = guildId;
            UserId = userId;
            ReactionAction = action;
            CreatedDate = DateTime.Now;
            Params = @params;
            OnlyWaitsForOwner = WaitsForOwner;
            IsReactionWaiter = true;
            LifeTime = minuteLifeTime;
        }

        public void AddAssociatedMessage(IMessage message)
        {
            if(AssociatedMessages == null) AssociatedMessages = new List<IMessage>();

            AssociatedMessages.Add(message);
        }

        public async Task Deleting()
        {
            if (!DeleteMessagesOnSuccess) return;

            foreach (var m in AssociatedMessages)
            {
                try
                {
                    m.DeleteAsync().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to delete message.");
                }
            }

            // delete the parent message as well.
            try
            {
                if(ParentMessage != null)
                await ParentMessage.DeleteAsync();
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to delete parent message.");
            }
        }
        
        public ulong GuildId { get; private set; }
        public ulong UserId { get; private set; }
        public object Params { get; private set; }
        public bool IsReactionWaiter { get; set; }
        public bool OnlyWaitsForOwner { get; set; }
        public double LifeTime { get; set; }
        public Func<SocketMessage, object, Task<bool>> Action { get; private set; }
        public Func<ulong, SocketReaction, object, Task<bool>> ReactionAction { get; private set; }

        // Associated messages
        private List<IMessage> AssociatedMessages { get; set; }
        public IMessage ParentMessage { get; set; }
        public bool DeleteMessagesOnSuccess { get; set; }
        
        public DateTime CreatedDate { get; private set; }
    }
}
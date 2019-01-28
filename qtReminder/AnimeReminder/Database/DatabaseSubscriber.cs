using System.Linq;
using qtReminder.AnimeReminder.Models;

namespace qtReminder.AnimeReminder.Database
{
    public class DatabaseSubscriber
    {
        public static bool SubscribeToAnime(ref AnimeGuildModel rAnimeGuildModel, ulong userId)
        {
            var c = Database.GetDatabaseAndSubscriptionCollection();
            var agm = rAnimeGuildModel;
            var animeguildmodel = c.collection
                .FindOne(x => x.Guild == agm.Guild && x.AnimeID == agm.AnimeID);

            bool success = true;
            
            // If the guild subscription does not exist,
            // create it.
            // otherwise, try to subscribe the current user to the anime.
            // if the user is already subscribed, this function will return false.
            // in any other cases, it will return true. (Even when it might have failed theoratically...)
            if (animeguildmodel == null)
            {
                agm.SubscribedUsers = new [] {userId};
                c.collection.Insert(agm);
            }
            else
            {
                rAnimeGuildModel = animeguildmodel;
                
                var subbedUsers = animeguildmodel.SubscribedUsers.ToList();
                if (subbedUsers.Contains(userId)) success = false;
                else
                {
                    subbedUsers.Add(userId);
                    animeguildmodel.SubscribedUsers = subbedUsers.ToArray();
                    c.collection.Update(animeguildmodel);
                }
            }
            
            return success;
        }

        public static bool UnsubscribeFromAnime(ref AnimeGuildModel animeGuildModel, ulong userId)
        {
            animeGuildModel.SubscribedUsers = 
                animeGuildModel.SubscribedUsers.ToList().Where(x=>x != userId)
                    .ToArray();

            var c = Database.GetDatabaseAndSubscriptionCollection();
            bool success = true;
            
            if (animeGuildModel.SubscribedUsers.Length == 0)
            {
                success = c.collection.Delete(animeGuildModel.Id);
            }
            else
            {
                success = c.collection.Update(animeGuildModel);
            }

            return success;
        }
    }
}
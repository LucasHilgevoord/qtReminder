using System.Collections.Generic;
using System.Linq;
using LiteDB;
using qtReminder.AnimeReminder.Models;

namespace qtReminder.AnimeReminder.Database
{
    public static class Database
    {
        private static LiteDatabase _database = null;
        public static LiteDatabase GetDatabase()
        {
            return _database ?? (_database = new LiteDatabase("mode=Exclusive; filename=qtreminder.db"));
        }

        public static (LiteDatabase database, LiteCollection<AnimeGuildModel> collection)
            GetDatabaseAndSubscriptionCollection()
        {
            var database = GetDatabase();
            return (database, database.GetCollection<AnimeGuildModel>("subscribers"));
        }

        public static LiteCollection<QuoteModel> GetQuotes()
        {
            return GetDatabase().GetCollection<QuoteModel>("quotes");
        }

        // FOR SOME REASON I STORE THE LAST CHECKED NYAA VALUES IN THIS DATABASE!
        // I really prefer this method instead of doing it with a JSON file because
        // I hate working with JSON. I hope that is enough reason to do it this way.
        // It might even be faster? Because it doesn't use any FileStreams, kinda.
        
        public static string GetLastChecked()
        {
            var database = GetDatabase();
            var col = database.GetCollection<LastCheckedModel>();
            string lastCheckValue = null;
            if (col.Count() == 0) col.Insert(new LastCheckedModel());
            else lastCheckValue = col.FindAll().ToArray()[0].LastChecked;

            return lastCheckValue;
        }

        public static void SetLastChecked(string lastChecked)
        {
            var database = GetDatabase();
            var col = database.GetCollection<LastCheckedModel>();
            if (col.Count() == 0) col.Insert(new LastCheckedModel() {LastChecked = lastChecked});
            else
            {
                var c = col.FindAll().ToList()[0];
                c.LastChecked = lastChecked;
                col.Update(c);
            }
        }
    }
}
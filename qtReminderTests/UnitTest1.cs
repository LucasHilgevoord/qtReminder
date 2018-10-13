using System;
using System.Threading.Tasks;
using qtReminder.AnimeReminder.Nyaa;
using Xunit;

namespace qtReminderTests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
        {
            // load fake XML first.
            string feed = await NyaaRSSFeed.GetRSSFeed();
            var document = NyaaXMLConverter.ParseXML(feed);
            var torrents = NyaaXMLConverter.GetTorrents(document);

            var parsedTorrents = NyaaParser.ParseNyaaTorrents(torrents);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using qtReminder.AnimeReminder.Models;

namespace qtReminder.AnimeReminder.Nyaa
{
    public static class NyaaXMLConverter
    {
        public static XmlDocument ParseXML(string xmlString)
        {
            var document = new XmlDocument();
            document.LoadXml(xmlString);
            return document;
        }

        public static NyaaTorrentModel[] GetTorrents(XmlDocument xmldoc)
        {
            var channels = xmldoc["rss"]["channel"].ChildNodes;
            var result = new List<NyaaTorrentModel>(); 
            for (var i = 0; i < channels.Count; i++)
            {
                var item = channels[i];
                if (item.Name != "item") continue;
                
                var model = new NyaaTorrentModel()
                {
                    Title = (item["title"].InnerText),
                    Link = (item["guid"].InnerText),
                    InfoHash = (item["nyaa:infoHash"].InnerText),
                };

                result.Add(model);
            }

            return result.ToArray();
        }
    }
}
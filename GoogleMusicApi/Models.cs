using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicApi
{
    public class Item
    {
        public string kind { get; set; }
        public string id { get; set; }
        public string clientId { get; set; }
        public bool deleted { get; set; }
        public string creationTimestamp { get; set; }
        public string lastModifiedTimestamp { get; set; }
    }

    public class PlaylistItem : Item
    {
        
        public string recentTimestamp { get; set; }
        public string name { get; set; }
        public string shareToken { get; set; }
        public string ownerName { get; set; }
        public string ownerProfilePhotoUrl { get; set; }
        public bool accessControlled { get; set; }
        public string type { get; set; }
        public string description { get; set; }
        public List<PlaylistEntryItem> playlistEntries { get; set; }
    }

    public class PlaylistEntryItem : Item
    {
        
        public string playlistId { get; set; }
        public string absolutePosition { get; set; }
        public string trackId { get; set; }
        public string source { get; set; }
        public PlaylistItem playlist { get; set; }
    }

    public class Data
    {
        public List<Item> items { get; set; }
    }

    public class RootObject
    {
        public string kind { get; set; }
        public string nextPageToken { get; set; }
        public Data data { get; set; }
    }
}

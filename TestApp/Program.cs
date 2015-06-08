using GoogleMusicApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp
{
    class Program
    {

        //private static string SongID = "4e8b260f-5a77-3988-848b-39710909c54d";  // '6QfW1gFhbJqyr3qsxOAD_uflsVQ', salt = '1433206642470'
        private static string SongID = "Tqdbjfwrznawbb6ljl3r3r4apja";
        private static string DeviceID = "338202c40679ff5f";
        static void Main(string[] args)
        {
            Api api = new Api();
            //api.Login("thepelhams2013@gmail.com", "4tippsey", "338202c40679ff5f");
            //api.GetPlaylistEntries();
            api.GetStreamUrl(SongID, DeviceID);
            System.Console.ReadKey();
        }
    }
}

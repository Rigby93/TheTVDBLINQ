using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVShowObjects;
using System.Threading;

namespace TheTVDBLINQ
{
    class Program
    {
        static void Main(string[] args)
        {
            var tvdbapi = new TheTVDBAPI("CF16009B834D65E7", "en", "http://thetvdb.com");
            for (int i = 0; i < 30; i++)
            {
                Console.WriteLine(tvdbapi.GetCurrentServerTime());
                Thread.Sleep(1000);
            }

            
            ////var series = new Dictionary<int, Series>(tvdbapi.GetSeries("arrow"));
            //var updates = tvdbapi.GetUpdates("1430838272", new List<int> { 70327, 70328 });
            //foreach (var series in updates.SeriesUpdates)
            //{
            //    Console.WriteLine(series);
            //}
            //foreach (var episode in updates.EpisodeUpdates)
            //{
            //    Console.WriteLine(episode);
            //}
            Console.ReadKey();
        }
    }
}

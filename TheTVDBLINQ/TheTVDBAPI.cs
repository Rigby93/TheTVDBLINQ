using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Globalization;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using TVShowObjects;

namespace TheTVDBLINQ
{
    public class TheTVDBAPI
    {
        private string _key;
        private string _language;
        private string _mirrorpath;

        public string APIKey
        {
            get
            {
                return this._key;
            }
            set
            {
                this._key = value;
            }
        }

        public string Language
        {
            get
            {
                return this._language;
            }
            set
            {
                this._language = value;
            }
        }

        public string MirrorPath
        {
            get
            {
                return this._mirrorpath;
            }
            set
            {
                this._mirrorpath = value;
            }
        }

        public TheTVDBAPI()
        {
        }

        public TheTVDBAPI(string key, string language, string mirrorpath)
        {
            this._key = key;
            this._language = language;
            this._mirrorpath = mirrorpath;
        }

        private Series ReadSeriesXML(string xmlSource)
        {
            Series series = new Series();
            Dictionary<int, Episode> episodes = new Dictionary<int, Episode>();
            Episode episode = null;
            bool newSeries = false;
            bool newEpisode = false;
            string[] formats = new string[4] { "hh:mm tt", "h:mm tt", "HH:mm", "H:mm" };
            bool connection = false;
            while (!connection)
            {
                try
                {
                    WebRequest webRequest = WebRequest.Create(xmlSource);
                    webRequest.Timeout = 15000;
                    using (WebResponse response = webRequest.GetResponse())
                    {
                        XDocument readSeries = XDocument.Load(response.GetResponseStream());
                        var query = from xmlSeries in readSeries.Descendants("series")
                                    select new Series
                                    {
                                        SeriesID = int.Parse(xmlSeries.Element("id").Value),
                                        Actors = xmlSeries.Element("actors").Value.Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList(),
                                        SeriesName = xmlSeries.Element("seriesname").Value
                                    };

                        using (XmlTextReader xmlTextReader = new XmlTextReader(response.GetResponseStream()))
                        {
                            while (xmlTextReader.Read())
                            {
                                if (xmlTextReader.Name == "Series")
                                    newSeries = xmlTextReader.NodeType == XmlNodeType.Element;
                                int num;
                                DateTime date;
                                double dub;
                                if (newSeries)
                                {
                                    switch (xmlTextReader.Name.ToLower())
                                    {
                                        case "id":
                                            if (int.TryParse(xmlTextReader.ReadInnerXml(), out num))
                                            {
                                                series.SeriesID = num;
                                            }
                                            break;
                                        case "actors":
                                            List<string> actors = new List<string>(xmlTextReader.ReadInnerXml().Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries));
                                            series.Actors = actors;
                                            break;
                                        case "airs_dayofweek":
                                            series.DayOfWeekAirs = xmlTextReader.ReadInnerXml();
                                            break;
                                        case "airs_time":
                                            if (DateTime.TryParseExact(xmlTextReader.ReadInnerXml(), formats, (IFormatProvider)CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                                            {
                                                series.TimeAirs = date;
                                            }
                                            break;
                                        case "contentrating":
                                            series.ContentRating = xmlTextReader.ReadInnerXml();
                                            break;
                                        case "firstaired":
                                            if (DateTime.TryParse(xmlTextReader.ReadInnerXml(), out date))
                                            {
                                                series.FirstAired = date;
                                            }
                                            break;
                                        case "genre":
                                            List<string> genres = new List<string>(xmlTextReader.ReadInnerXml().Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries));
                                            series.Genre = genres;
                                            break;
                                        case "imdb_id":
                                            series.IMDB_ID = xmlTextReader.ReadInnerXml();
                                            break;
                                        case "language":
                                            series.Language = xmlTextReader.ReadInnerXml();
                                            break;
                                        case "network":
                                            series.Network = xmlTextReader.ReadInnerXml().Replace("&amp;", "&").Replace("amp;", "&");
                                            break;
                                        case "overview":
                                            series.Overview = xmlTextReader.ReadInnerXml().Replace("&amp;", "&").Replace("amp;", "&");
                                            break;
                                        case "rating":
                                            if (double.TryParse(xmlTextReader.ReadInnerXml(), out dub))
                                            {
                                                series.Rating = dub;
                                            }
                                            break;
                                        case "ratingcount":
                                            if (int.TryParse(xmlTextReader.ReadInnerXml(), out num))
                                            {
                                                series.RatingCount = num;
                                            }
                                            break;
                                        case "runtime":
                                            if (int.TryParse(xmlTextReader.ReadInnerXml(), out num))
                                            {
                                                series.Runtime = num;
                                            }
                                            break;
                                        case "seriesname":
                                            series.SeriesName = xmlTextReader.ReadInnerXml().Replace("&amp;", "&").Replace("amp;", "&");
                                            break;
                                        case "status":
                                            series.Status = xmlTextReader.ReadInnerXml();
                                            break;
                                        case "banner":
                                            series.Banner = xmlTextReader.ReadInnerXml();
                                            break;
                                        case "fanart":
                                            series.Fanart = xmlTextReader.ReadInnerXml();
                                            break;
                                        case "poster":
                                            series.Poster = xmlTextReader.ReadInnerXml();
                                            break;
                                    }
                                }
                                if (xmlTextReader.Name == "Episode")
                                {
                                    if (xmlTextReader.NodeType == XmlNodeType.Element)
                                    {
                                        episode = new Episode();
                                        newEpisode = true;
                                    }
                                    else
                                    {
                                        episodes.Add(episode.EpisodeID, episode);
                                        newEpisode = false;
                                    }
                                }
                                if (newEpisode)
                                {
                                    switch (xmlTextReader.Name.ToLower())
                                    {
                                        case "seriesid":
                                            if (int.TryParse(xmlTextReader.ReadInnerXml(), out num))
                                            {
                                                episode.SeriesID = num;
                                            }
                                            break;
                                        case "id":
                                            if (int.TryParse(xmlTextReader.ReadInnerXml(), out num))
                                            {
                                                episode.EpisodeID = num;
                                            }
                                            break;
                                        case "director":
                                            episode.Director = xmlTextReader.ReadInnerXml();
                                            break;
                                        case "episodename":
                                            episode.Name = xmlTextReader.ReadInnerXml().Replace("&amp;", "&").Replace("amp;", "&");
                                            break;
                                        case "episodenumber":
                                            if (int.TryParse(xmlTextReader.ReadInnerXml(), out num))
                                            {
                                                episode.EpisodeNumber = num;
                                            }
                                            break;
                                        case "firstaired":
                                            if (DateTime.TryParse(xmlTextReader.ReadInnerXml(), out date))
                                            {
                                                episode.FirstAired = date;
                                            }
                                            break;
                                        case "gueststars":
                                            List<string> guestStars = new List<string>(xmlTextReader.ReadInnerXml().Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries));
                                            episode.GuestStars = guestStars;
                                            break;
                                        case "language":
                                            episode.Language = xmlTextReader.ReadInnerXml();
                                            break;
                                        case "overview":
                                            episode.Overview = xmlTextReader.ReadInnerXml().Replace("&amp;", "&").Replace("amp;", "&");
                                            break;
                                        case "rating":
                                            if (double.TryParse(xmlTextReader.ReadInnerXml(), out dub))
                                            {
                                                episode.Rating = dub;
                                            }
                                            break;
                                        case "ratingcount":
                                            if (int.TryParse(xmlTextReader.ReadInnerXml(), out num))
                                            {
                                                episode.RatingCount = num;
                                            }
                                            break;
                                        case "seasonnumber":
                                            if (int.TryParse(xmlTextReader.ReadInnerXml(), out num))
                                            {
                                                episode.Season = num;
                                            }
                                            break;
                                        case "writer":
                                            List<string> writers = new List<string>(xmlTextReader.ReadInnerXml().Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries));
                                            episode.Writer = writers;
                                            break;
                                        case "absolute_number":
                                            if (int.TryParse(xmlTextReader.ReadInnerXml(), out num))
                                            {
                                                episode.AbsoluteNumber = num;
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    series.Episodes = episodes;
                    connection = true;
                }
                catch (WebException ex)
                {
                }
            }
            return series;
        }

        public void UnzipFromStream(Stream zipStream, string outFolder)
        {

            ZipInputStream zipInputStream = new ZipInputStream(zipStream);
            ZipEntry zipEntry = zipInputStream.GetNextEntry();
            while (zipEntry != null)
            {
                String entryFileName = zipEntry.Name;
                // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                // Optionally match entrynames against a selection list here to skip as desired.
                // The unpacked length is available in the zipEntry.Size property.

                byte[] buffer = new byte[4096];     // 4K is optimum

                // Manipulate the output filename here as desired.
                String fullZipToPath = Path.Combine(outFolder, entryFileName);
                string directoryName = Path.GetDirectoryName(fullZipToPath);
                if (directoryName.Length > 0)
                    Directory.CreateDirectory(directoryName);

                // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                // of the file, but does not waste memory.
                // The "using" will close the stream even if an exception occurs.
                using (FileStream streamWriter = File.Create(fullZipToPath))
                {
                    StreamUtils.Copy(zipInputStream, streamWriter, buffer);
                }
                zipEntry = zipInputStream.GetNextEntry();
            }
        }

        public string GetCurrentServerTime()
        {
            string url = _mirrorpath + "/api/Updates.php?type=none";
            string currentTime = "";
            bool connection = false;
            while (!connection)
            {
                try
                {
                    WebRequest webRequest = WebRequest.Create(url);
                    webRequest.Timeout = 15000;
                    using (WebResponse response = webRequest.GetResponse())
                    {
                        using (XmlTextReader xmlTextReader = new XmlTextReader(response.GetResponseStream()))
                        {
                            while (xmlTextReader.Read())
                            {
                                if (xmlTextReader.Name == "Time")
                                    currentTime = xmlTextReader.ReadInnerXml();
                            }
                        }
                    }
                    connection = true;
                }
                catch (WebException ex)
                {
                }
            }
            return currentTime;
        }

        public Update GetUpdates(string previousTime, List<int> favourites)
        {
            string currentTime = GetCurrentServerTime();
            string period = "day";
            string url = this._mirrorpath + "/api/" + this._key + "/updates/updates_" + period + ".zip";
            string tmpFolder = Path.GetTempPath() + "TVDB Updates-" + currentTime;
            Update update = new Update();
            List<int> seriesIDs = new List<int>();
            List<int> episodeIDs = new List<int>();
            bool connection = false;
            while (!connection)
            {
                try
                {
                    using (WebDownload webDownload = new WebDownload(15000))
                        UnzipFromStream(webDownload.OpenRead(url), tmpFolder);
                    XDocument xdoc = XDocument.Load(tmpFolder.Replace(".zip","") + "\\updates_" + period + ".xml");
                    XElement root = xdoc.Root;
                    seriesIDs = (from series in root.Elements("Series")
                                 where favourites.Contains(int.Parse(series.Element("id").Value)) && Int64.Parse(series.Element("time").Value) > Int64.Parse(previousTime)
                                 select int.Parse(series.Element("id").Value)).ToList<int>();
                    episodeIDs = (from series in root.Elements("Episode")
                                  where favourites.Contains(int.Parse(series.Element("Series").Value)) && Int64.Parse(series.Element("time").Value) > Int64.Parse(previousTime)
                                  select int.Parse(series.Element("id").Value)).ToList<int>();
                    update.SeriesUpdates = seriesIDs;
                    update.EpisodeUpdates = episodeIDs;
                    Directory.Delete(tmpFolder, true);
                    connection = true;
                }
                catch (WebException ex)
                {
                }
            }
            return update;
        }

        public Dictionary<int, Series> GetSeries(string seriesName)
        {
            string url = this._mirrorpath + "/api/GetSeries.php?seriesname=" + seriesName.Trim().Replace(" ", "%20");
            Series series = null;
            Dictionary<int, Series> allSeries = new Dictionary<int, Series>();
            bool connection = false;
            while (!connection)
            {
                try
                {
                    WebRequest webRequest = WebRequest.Create(url);
                    webRequest.Timeout = 15000;
                    using (WebResponse response = webRequest.GetResponse())
                    {
                        XDocument searchSeries = XDocument.Load(response.GetResponseStream());
                        IEnumerable<XElement> seriesResults = from xmlSeries in searchSeries.Descendants("Series")
                                    select xmlSeries;
                        foreach (XElement s in seriesResults)
                        {
                            series = new Series();
                            switch (s.Name.ToString().ToLower())
                            {
                                case "seriesid":
                                    int num;
                                    if (int.TryParse(s.Value, out num))
                                    {
                                        series.SeriesID = num;
                                    }
                                    break;
                                case "language":
                                    series.Language = s.Value;
                                    break;
                                case "seriesname":
                                    series.SeriesName = s.Value.Replace("&amp;", "&").Replace("amp;", "&");
                                    break;
                                case "banner":
                                    series.Banner = s.Value;
                                    break;
                                case "overview":
                                    series.Overview = s.Value.Replace("&amp;", "&").Replace("amp;", "&");
                                    break;
                                case "firstaired":
                                    DateTime date;
                                    if (DateTime.TryParse(s.Value, out date))
                                    {
                                        series.FirstAired = date;
                                    }
                                    break;
                                case "network":
                                    series.Network = s.Value.Replace("&amp;", "&").Replace("amp;", "&");
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    connection = true;
                }
                catch (WebException ex)
                {
                }
            }
            return allSeries;
        }

        public Series GetSeriesUpdates(int seriesID)
        {
            string xmlSource = _mirrorpath + "/api/" + _key + "/series/" + seriesID + "/" + _language + ".xml";
            Series series = new Series();
            try
            {
                return ReadSeriesXML(xmlSource);
            }
            catch (XmlException ex)
            {
                return null;
            }
        }

        public Episode GetEpisodeUpdates(int episodeID)
        {
            string xmlSource = _mirrorpath + "/api/" + _key + "/episodes/" + episodeID + "/" + _language + ".xml";
            Series series = new Series();
            Episode episode = new Episode();
            try
            {
                return ReadSeriesXML(xmlSource).Episodes.Values.First();
            }
            catch (XmlException ex)
            {
                return null;
            }
        }

        public Series GetSeriesInfo(int seriesID)
        {
            Series series = new Series();
            string tmpFolder = Path.GetTempPath() + "TVDB-" + seriesID;
            string zipFile = _mirrorpath + "/api/" + _key + "/series/" + seriesID + "/all/" + _language + ".zip";
            bool connection = false;
            while (!connection)
            {
                try
                {
                    using (WebDownload webDownload = new WebDownload(15000))
                        UnzipFromStream(webDownload.OpenRead(zipFile), tmpFolder);
                    series = ReadSeriesXML(tmpFolder + "\\" + _language + ".xml");
                    Directory.Delete(tmpFolder, true);
                    connection = true;
                }
                catch (WebException ex)
                {
                }
            }
            return series;
        }

        public void SaveSeriesPoster(int seriesID, string filename)
        {
            string url = _mirrorpath + "/banners/_cache/" + filename;
            string localAppFolder = string.Concat(new object[4] { Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "\\Personal TV Organiser\\", seriesID, "\\" });
            try
            {
                if (!Directory.Exists(localAppFolder))
                {
                    Directory.CreateDirectory(localAppFolder);
                    Directory.CreateDirectory(localAppFolder + "posters");
                }
                string postersFolder = localAppFolder + filename.Replace("/", "\\");
                if (File.Exists(postersFolder))
                    return;
                using (WebClient webClient = new WebClient())
                    webClient.DownloadFile(url, postersFolder);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveFanartVignette(int seriesID, string filename)
        {
            filename = filename.Replace("original", "vignette");
            string url = _mirrorpath + "/banners/" + filename;
            string localAppFolder = string.Concat(new object[4] { Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "\\Personal TV Organiser\\", seriesID, "\\" });
            try
            {
                if (!Directory.Exists(localAppFolder + "fanart"))
                {
                    if (!Directory.Exists(localAppFolder))
                        Directory.CreateDirectory(localAppFolder);
                    Directory.CreateDirectory(localAppFolder + "fanart");
                }
                string vignetteFolder = localAppFolder + filename.Replace("/vignette", "").Replace("/", "\\");
                if (File.Exists(vignetteFolder))
                    return;
                using (WebClient webClient = new WebClient())
                    webClient.DownloadFile(url, vignetteFolder);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

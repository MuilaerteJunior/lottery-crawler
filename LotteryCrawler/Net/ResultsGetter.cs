using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LotteryCrawler.Net
{
    public class ResultsGetter
    {
        private string url;
        private string dataFileName;
        private int drawnCount;

        public ResultsGetter(string url, string dataFileName, int drawnCount)
        {
            this.url = url;
            this.dataFileName = dataFileName;
            this.drawnCount = drawnCount;
        }

        public List<Sorteio> GetResults()
        {            
            return ParseFileData(ExtractFileToMemory(DownloadMostRecentFile()));
        }

        private string DownloadMostRecentFile()
        {
            var tempFileName = Path.GetTempFileName();         

            HttpWebRequest webReq = (HttpWebRequest)HttpWebRequest.Create(url);

            webReq.CookieContainer = new CookieContainer();
            webReq.Method = "GET";
            using (WebResponse response = webReq.GetResponse())
            {
                using (Stream reader = response.GetResponseStream())
                {
                    using (var writer = File.OpenWrite(tempFileName))
                    {
                        var buffer = new byte[1024 * 16];
                        var read = buffer.Length;
                        while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            writer.Write(buffer, 0, read);
                        }
                    }
                }
            }

            return tempFileName;
        }

        private string ExtractFileToMemory(string filePath)
        {
            using (ZipArchive archive = new ZipArchive(File.OpenRead(filePath)))
            {
                ZipArchiveEntry entry = archive.Entries.Where(x => x.Name.ToUpper() == dataFileName).FirstOrDefault();
                if (entry != null)
                    using (var stream = entry.Open())
                    {
                        return new StreamReader(stream).ReadToEnd();
                    }
            }
            return string.Empty;
        }

        private List<Sorteio> ParseFileData(string fileContent)
        {
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();

            // There are various options, set as needed
            htmlDoc.OptionFixNestedTags = true;

            // filePath is a path to a file containing the html
            htmlDoc.LoadHtml(fileContent);

            var sorteios = new List<Sorteio>();

            foreach (var line in htmlDoc.DocumentNode.SelectNodes("//tr").Where(x=>x.ChildNodes.Count > drawnCount + 2).Skip(1))
            {
                var sorteio = new Sorteio();

                sorteio.Id = int.Parse(line.ChildNodes.Where(x => x.Name == "td").ElementAt(0).InnerText);
                sorteio.Date = DateTime.Parse(line.ChildNodes.Where(x=>x.Name == "td").ElementAt(1).InnerText , CultureInfo.GetCultureInfo("pt-BR"));

                for (var i = 2; i < 2 + drawnCount; i++)
                {
                    var cell = line.ChildNodes.Where(x => x.Name == "td").ElementAt(i);
                    sorteio.Numbers.Add(int.Parse(cell.InnerText));                    
                }

                sorteios.Add(sorteio);
            }

            return sorteios;
        }
    }
}

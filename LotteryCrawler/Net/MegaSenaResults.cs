using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;

namespace LotteryCrawler.Net
{
    public class MegaSenaResults : IResultsGenerator
    {
        private string _url;
        private string _dataFileName;
        private int _drawnCount;

        public MegaSenaResults()
        {
            this._url = "http://www1.caixa.gov.br/loterias/_arquivos/loterias/D_megase.zip";
            this._dataFileName = "D_MEGA.HTM";
            this._drawnCount = 6;
        }

        public IEnumerable<Sorteio> Resultados(int size)
        {
            _drawnCount = size;
            return ParseFileData(ExtractFileToMemory(DownloadMostRecentFile()));
        }

        private string DownloadMostRecentFile()
        {
            var tempFileName = Path.GetTempFileName();         
            HttpWebRequest webReq = (HttpWebRequest)HttpWebRequest.Create(_url);
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
                ZipArchiveEntry entry = archive.Entries.FirstOrDefault(x => x.Name.ToUpper() == _dataFileName);
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
            htmlDoc.OptionFixNestedTags = true;
            htmlDoc.LoadHtml(fileContent);
            List<Sorteio> sorteios = ReadAndExtractInformation(htmlDoc);
            return sorteios;
        }

        private List<Sorteio> ReadAndExtractInformation(HtmlDocument htmlDoc)
        {
            var sorteios = new List<Sorteio>();
            foreach (var line in htmlDoc.DocumentNode.SelectNodes("//tr").Where(x => x.ChildNodes.Count > _drawnCount + 2).Skip(1))
            {
                var sorteio = new Sorteio();

                sorteio.Id = int.Parse(line.ChildNodes.Where(x => x.Name == "td").ElementAt(0).InnerText);
                sorteio.Date = DateTime.Parse(line.ChildNodes.Where(x => x.Name == "td").ElementAt(1).InnerText, CultureInfo.GetCultureInfo("pt-BR"));

                for (var i = 2; i < 2 + _drawnCount; i++)
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

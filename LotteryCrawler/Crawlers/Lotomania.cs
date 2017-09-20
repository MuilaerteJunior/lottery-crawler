using HtmlAgilityPack;
using LotteryCrawler.Net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotteryCrawler.Crawlers
{
    public class Lotomania : Lottery
    {
        public Lotomania():base("http://www1.caixa.gov.br/loterias/_arquivos/loterias/D_lotoma.zip", "D_LOTMAN.HTM", 20, 100)
        {
           
        }        
    }
}

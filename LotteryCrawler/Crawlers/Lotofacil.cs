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
    public class Lotofacil : Lottery
    {
        public Lotofacil():base("http://www1.caixa.gov.br/loterias/_arquivos/loterias/D_lotfac.zip", "D_LOTFAC.HTM", 15, 25)
        {
           
        }        
    }
}

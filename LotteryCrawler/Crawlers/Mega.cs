using LotteryCrawler.Net;
using LotteryCrawler.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LotteryCrawler.Crawlers
{
    public class Mega : Lottery
    {
        public Mega(): base("http://www1.caixa.gov.br/loterias/_arquivos/loterias/D_megase.zip" , "D_MEGA.HTM" , 6, 60)
        {            
        }        
    }
}

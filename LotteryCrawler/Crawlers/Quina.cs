using LotteryCrawler.Net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LotteryCrawler.Crawlers
{   
    public class Quina : Lottery
    {
      
        public Quina() : base("http://www1.caixa.gov.br/loterias/_arquivos/loterias/D_quina.zip", "D_QUINA.HTM", 5, 80)
        {        
        }
    }
}

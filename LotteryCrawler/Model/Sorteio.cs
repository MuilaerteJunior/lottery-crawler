using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotteryCrawler
{
    public class Sorteio
    {
        public Sorteio() {
            this.Numbers = new List<int>();
        }
        public List<int> Numbers { get; set; }
        public DateTime Date { get; set; }
        public int Id { get; set; }
    }
}

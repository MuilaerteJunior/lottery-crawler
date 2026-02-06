using System.Collections.Generic;

namespace LotteryCrawler.Net
{
    public interface IResultsGenerator
    {
        IEnumerable<Sorteio> Resultados(int size);
    }
}

using System.Collections.Generic;

namespace LotteryCrawler.Net
{
    public class NumbersProvider {

        private IResultsGenerator _geradorDeResultados;

        internal IEnumerable<Sorteio> Generate(int size)
        {
            return _geradorDeResultados.Resultados(size);
        }

        internal void Use<T>() where T : IResultsGenerator, new()
        {
            _geradorDeResultados = new T();
        }
    }
}

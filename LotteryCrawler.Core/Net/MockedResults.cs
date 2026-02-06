using System;
using System.Collections.Generic;
using System.Linq;

namespace LotteryCrawler.Net
{
    public abstract class MockedResults : IResultsGenerator
    {
        private int _optionsResult;
        private int _numberOfOptions;
        private static readonly Random _random = new Random();

        public MockedResults(int optsResult, int numberOpts)
        {
            _optionsResult = optsResult;
            _numberOfOptions = numberOpts;
        }
        
        public IEnumerable<Sorteio> Resultados(int size)
        {
            var result = new List<Sorteio>();

            for (var sizeIndex = 0; sizeIndex < size; sizeIndex++)
            {
                var numberTries = 0;
                var triesLimit = 2000;
                var resultados = new List<int>();
                while (resultados.Count < _optionsResult)
                {
                    if (numberTries >= triesLimit)
                        throw new Exception("Number of tries overlimited");

                    var sortedNumber = _random.Next(1, _numberOfOptions);
                    if (!resultados.Contains(sortedNumber))
                        resultados.Add(sortedNumber);

                    numberTries++;
                }

                result.Add(new Sorteio
                {
                    Numbers = resultados.OrderBy(v => v).ToList()
                });
            }
            
            return result;
        }
    }
}

using System;
using System.Diagnostics;
using System.Linq;

namespace LotteryCrawler.Core.Crawlers
{

    public class PredictiveTestMethod {
        /*
        public string PredictiveBet(Sorteio lastResult) {
            if ( this._results.Contains(lastResult) ) {
                var abc = CategorizeResults(new Sorteio[] { lastResult });
                _lastResult = lastResult;
                this._results.RemoveAt(this._results.IndexOf(lastResult));
            }

            var blackListNResults = GenerateBlackListNResults(_numberOfConsiderations);
            var wordSuggestion = NumberFormatSuggestion(_numberOfConsiderations);
            var sw = new Stopwatch();
            sw.Start();
            var bet = GenerateNumberFromString(wordSuggestion, blackListNResults);
            var lastResultNumbers = lastResult.Numbers;
            while ( lastResultNumbers .Intersect(bet).Count() != lastResultNumbers .Count() ) { 
                bet = GenerateNumberFromString(wordSuggestion, blackListNResults);
                 //TODO: Implementar a lógica para gerar a word suggestion dinamica tambem
            }
            sw.Stop();

            return new TimeSpan(sw.ElapsedMilliseconds).ToString(@"hh\:mm\:ss");
        }
        */
    }
}
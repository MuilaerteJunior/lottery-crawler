using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LotteryCrawler.Bet.Crawlers
{
    public class StudyNumbers {
        private List<Sorteio> _results;
        private List<int> RepeatedNumberLastTwoResults;

        private List<string> LastDivisionNumber;
        private int _numberOfConsiderations;
        private int _betNumberLimit = 6;
        private static readonly Random _random = new Random();
        private Sorteio _lastResult;
        private static readonly char[] numbersCategories = { 'A' , 'B', 'C', 'D', 'E', 'F' };
        private IEnumerable<char> _repeatedLetterLastGame;
        private IEnumerable<char> _intersectionLast2Games;

        public StudyNumbers(List<Sorteio> results, int numberOfConsiderations)
        {
            this._results = results.OrderByDescending(s => s.Date).ToList();
            this._numberOfConsiderations = numberOfConsiderations;
        }

        private List<int> CreateBlackListNumbers(int nResults) {
            var lastNResults = this._results.Take(nResults).ToArray();
            var repeatedNumberLastTwoResults = new List<int>();

            if ( lastNResults.Any() ) {
                for ( int indexResult = 0; indexResult < lastNResults.Length - 1; indexResult++) {
                    var lastResult = lastNResults[indexResult].Numbers;
                    var penultimateResult = lastNResults[indexResult + 1].Numbers;

                    var intersect = lastResult.Intersect(penultimateResult);
                    repeatedNumberLastTwoResults.AddRange(intersect);
                }
            }

            return repeatedNumberLastTwoResults;
        }
        /// <summary>
        /// This method returns a word to suggest numbers.
        /// A is FROM 0 TO 10
        /// B is FROM 10 TO 20
        /// AND SO ON
        /// </summary>
        /// <param name="nResults"></param>
        /// <returns></returns>
        private string SuggestCategorizeNumbers(int nResults) {
            var lastNResults = this._results.Take(nResults).ToArray();

            var lastDivisionNumber = CategorizeResults(lastNResults);
            var allResultsAsLetters = string.Join("", lastDivisionNumber);
            var option = string.Join("", allResultsAsLetters
                                        .GroupBy(c => c)
                                        .OrderBy(e => e.Count())
                                        .ThenBy(a => a.Key)
                                        .Select(c => c.Key));

            if ( option.Length < _betNumberLimit )
                option = option + allResultsAsLetters
                                    .Substring((new Random()).Next(0, allResultsAsLetters.Length),
                                                _betNumberLimit - option.Length);

            return option;
        }

        private List<string> CategorizeResults(Sorteio[] lastNResults) {
            var categorizedResults = new List<string>();
            for ( int indexResult = 0; indexResult < lastNResults.Length; indexResult++ ) {
                var lastResult = lastNResults[indexResult].Numbers;

                var stringResultByTen = "";
                var from0To10 =  lastResult.Count(a => a > 0 && a <= 10);
                if ( from0To10 > 0)
                    stringResultByTen += new string('A',from0To10);

                var from10To20 = lastResult.Count(a => a > 10 && a <= 20);
                if ( from10To20 > 0 )
                    stringResultByTen += new string('B', from10To20);
                var from20To30 = lastResult.Count(a => a > 20 && a <= 30);
                if ( from20To30 > 0 )
                    stringResultByTen += new string('C', from20To30);

                var from30To40 = lastResult.Count(a => a > 30 && a <= 40);
                if ( from30To40 > 0 )
                    stringResultByTen += new string('D', from30To40);

                var from40To50 = lastResult.Count(a => a > 40 && a <= 50);
                if ( from40To50 > 0 )
                    stringResultByTen += new string('E', from40To50);

                var from50To60 = lastResult.Count(a => a > 50 && a <= 60);
                if ( from50To60 > 0 )
                    stringResultByTen += new string('F', from50To60);

                categorizedResults.Add(stringResultByTen);
            }

            if ( categorizedResults.Count > 0 ) 
                _repeatedLetterLastGame = categorizedResults[0].GroupBy(c => c).Where(c => c.Count() > 1).Select(c => c.Key);
            
            if ( categorizedResults.Count > 1 ) 
                _intersectionLast2Games = categorizedResults[0].Intersect(categorizedResults[1]);

            return categorizedResults;
        }
        private List<int> GenerateNumberFromString(string word, List<int> notRecomendedNumbers) {
            if ( word.Length != _betNumberLimit )
                throw new Exception("Missing suggestions parameter");

            var finalNumbers = new List<int>();
            foreach ( var letter in word ) {
                switch ( letter ) {
                    case 'A': GenerateNumber(1, 10, notRecomendedNumbers, finalNumbers); break;
                    case 'B': GenerateNumber(10,20, notRecomendedNumbers, finalNumbers); break;
                    case 'C': GenerateNumber(20,30, notRecomendedNumbers, finalNumbers); break;
                    case 'D': GenerateNumber(30,40, notRecomendedNumbers, finalNumbers); break;
                    case 'E': GenerateNumber(40,50, notRecomendedNumbers, finalNumbers); break;
                    case 'F': GenerateNumber(50,60, notRecomendedNumbers, finalNumbers); break;
                    default: break;
                }
            }

            return finalNumbers.OrderBy(a => a).ToList();
        }

        private void GenerateNumber(int minValue, int maxValue, List<int> notRecomendedNumbers, List<int> actualBet) {
            var suggestionNumber  = _random.Next(minValue, maxValue);
            var indexTry = 0;
            while ((notRecomendedNumbers != null && notRecomendedNumbers.Contains(suggestionNumber))
                    || actualBet.Contains(suggestionNumber)
                    || (indexTry <= 100)) {
                suggestionNumber  = _random.Next(minValue, maxValue);
                indexTry++;
            }
            actualBet.Add(suggestionNumber);
        }
        public List<List<int>> GenerateBets(int numberOfBets = 3) {
            var bets = new List<List<int>>();

            var blackListNResults = CreateBlackListNumbers(_numberOfConsiderations);
            var wordSuggestion = SuggestCategorizeNumbers(_numberOfConsiderations);
            for ( var betIndex = 0; betIndex < numberOfBets; betIndex++ ) {
                var bet = GenerateNumberFromString(wordSuggestion, blackListNResults);
                bets.Add(bet);
            }

            return bets;
        }

        public string PredictiveBet(Sorteio lastResult) {
            if ( this._results.Contains(lastResult) ) {
                var abc = CategorizeResults(new Sorteio[] { lastResult });
                _lastResult = lastResult;
                this._results.RemoveAt(this._results.IndexOf(lastResult));
            }

            var blackListNResults = CreateBlackListNumbers(_numberOfConsiderations);
            var wordSuggestion = SuggestCategorizeNumbers(_numberOfConsiderations);
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
    }
}
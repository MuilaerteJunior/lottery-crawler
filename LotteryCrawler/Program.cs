using HtmlAgilityPack;
using LotteryCrawler.Crawlers;
using LotteryCrawler.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace LotteryCrawler
{
    class Program
    {
        //static void Main(string[] args)
        //{
        //    Console.WriteLine("Escolha a quantidade últimos jogos a considerar:");
        //    Console.WriteLine("1 - Os últimos 6 jogos ");
        //    Console.WriteLine("2 - Os últimos 10 jogos ");
        //    Console.WriteLine("3 - Os últimos 16 jogos ");
        //    Console.WriteLine("9 - Predictive mode");

        //    //var inputValue = Console.ReadLine();

        //    //int option = 1;
        //    int option = 9;
        //    //int.TryParse(inputValue, out option);
        //    Lottery lottery = null;
        //    switch (option)
        //    {
        //        case 2: lottery = new Mega(10); break;
        //        case 3: lottery = new Mega(16); break;
        //        case 9: {
        //                lottery = new Mega(16, true); 
        //                Console.WriteLine(lottery.TimeElapsed());
        //                break;
        //            }
        //        case 1:  
        //        default:
        //            lottery = new Mega(6);
        //            break;
        //    }

        //    while (true)
        //    {
        //        var output = string.Empty;
        //        for ( var betIndex = 0; betIndex < 3; betIndex++ ) {
        //            output += string.Join("\t", lottery.Drawn(betIndex)) + Environment.NewLine;
        //        }

        //        File.AppendAllText("output.txt", output + "\n");
        //        Console.WriteLine(output);
        //        Console.ReadKey();
        //    }
        //}

        static void Main(string[] args)
        {
            var numProvider = new NumbersProvider();

            numProvider.Use<Mocked10Results>();
            PrintaResultados(numProvider.Generate(5));

            numProvider.Use<Mocked60Results>();
            PrintaResultados(numProvider.Generate(5));

            Console.ReadKey();
        }

        private static void PrintaResultados(IEnumerable<Sorteio> resultados)
        {
            foreach (var item in resultados)
            {
                Console.WriteLine(string.Join(" - ", item.Numbers.Select(r => r)));
            }
        }
    }
}

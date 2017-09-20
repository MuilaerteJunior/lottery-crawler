using HtmlAgilityPack;
using LotteryCrawler.Crawlers;
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
        static void Main(string[] args)
        {

            int opt = 0;

            Console.WriteLine("Escolha o tipo de sorteio:");
            Console.WriteLine("1 - Megasena");
            Console.WriteLine("2 - Quina");
            Console.WriteLine("3 - Lotomania");
            Console.WriteLine("4 - Lotofacil");

            var k = Console.ReadLine();

            int.TryParse(k, out opt);

            Lottery lottery = null;
            int bids = 0;

            switch(opt)
            {
                case 1:
                    {
                        lottery = new Mega();
                        bids = 6;
                        break;                        
                    }
                case 2:
                    {
                        lottery = new Quina();
                        bids = 5;
                        break;
                    }
                case 3:
                    {
                        lottery = new Lotomania();
                        bids = 50;
                        break;
                    }
                case 4:
                    {
                        lottery = new Lotofacil();
                        bids = 15;
                        break;
                    }
            }

            while (true)
            {
                var output = string.Join("\t", lottery.Drawn(bids));
                File.AppendAllText("output.txt", output + "\n");
                Console.WriteLine(output);
                Console.ReadKey();
            }



        }

        

    }



}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotteryCrawler.Util
{
    public static class DataExtensions
    {
        private static double? avg = null;
        private static double? stDev = null;


        public static double Normalize(this double[] interval , double value)
        {
            if(!avg.HasValue)
                avg = somaMatrix(interval) / interval.Length;
            if(!stDev.HasValue)
                stDev = StDev(interval, avg??0);

            return (value - stDev??0) / avg??0;
        }

        private static double somaMatrix(double[] valores)
        {
            double soma = 0;//variável temporaria
            for (int i = 0; i < valores.Length; i++)
            {
                soma = soma + valores[i];
            }
            return soma;
        }


        private static double StDev(double[] interval, double avg)
        {        
            //Parte1: Cálculo do (x - xMedio)^2
            double[] Parte1 = new double[interval.Length]; //declara a variavel que armanezará (x - xMedio)^2

            for (int i = 0; i < interval.Length; i++)
            {
                Parte1[i] = Math.Pow(avg - interval[i], 2);// Calcula e armazena (x - xMedio)^2;
            }

            double stdDev = Math.Sqrt(somaMatrix(Parte1) / Parte1.Length);//Extrai a raiz quadrada da somatória de (x - xMedio)^2 dividido por N

            return stdDev;
        }
    }
}

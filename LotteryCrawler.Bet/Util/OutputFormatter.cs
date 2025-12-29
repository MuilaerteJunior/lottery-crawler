namespace LotteryCrawler.Util
{
    public static class OutputFormatter
    {
        private const int DELIMITER_COUNT = 50;

        public static void PrintSectionTitle(string title)
        {
            Console.WriteLine(new String('-', DELIMITER_COUNT));
            Console.WriteLine(title);
            Console.WriteLine(new String('-', DELIMITER_COUNT));
        }

        public static void PrintLineSeparator()
        {
            Console.WriteLine(new String('-', DELIMITER_COUNT));
        }

        public static void PrintOut(string title, NumberPresence[] numbers)
        {
            OutputFormatter.PrintSectionTitle(title);
            foreach (var number in numbers)
            {
                if (number.Quantity.HasValue)
                    Console.WriteLine($"Number {number.Number}. Count {number.Quantity.Value}. PossibilityPresence {number.PossibiltyOfPresence}");
                else
                    Console.WriteLine($"Number {number.Number}. PossibilityPresence {number.PossibiltyOfPresence}");

            }
            PrintLineSeparator();
        }

    }
}

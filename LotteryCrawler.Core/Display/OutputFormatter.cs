namespace LotteryCrawler.Core.Display
{
    public static class OutputFormatter
    {
        private const int DELIMITER_COUNT = 50;

        /// <summary>
        /// Example:
        /// --------------------------------------------------
        /// Title
        /// --------------------------------------------------
        /// </summary>
        public static void PrintSectionTitle(string title)
        {
            Console.WriteLine(new string('-', DELIMITER_COUNT));
            Console.WriteLine(title);
            Console.WriteLine(new string('-', DELIMITER_COUNT));
        }

        /// <summary>
        /// Example:
        /// --------------------------------------------------
        /// </summary>
        public static void PrintLineSeparator(int size = DELIMITER_COUNT)
        {
            Console.WriteLine(new string('-', size));
        }

        /// <summary>
        /// Example
        /// --------------------------------------------------
        /// Top Picks
        /// --------------------------------------------------
        /// Number 3. Count 7. PossibilityPresence 0.14
        /// Number 7. PossibilityPresence 0.09
        /// Number 12. Count 2. PossibilityPresence 0.03
        /// Number 25. PossibilityPresence 0.01
        /// Number 40. Count 10. PossibilityPresence 0.20
        /// --------------------------------------------------
        /// </summary>
        /// <param name="title"></param>
        /// <param name="numbers"></param>
        public static void PrintOut(string title, NumberPresence[] numbers)
        {
            PrintSectionTitle(title);
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

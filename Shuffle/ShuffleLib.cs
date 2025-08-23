namespace Shuffle
{
    public class ShuffleLib
    {
        static void Main()
        {
            // list of test items
            List<string> items = new List<string> { "A", "B", "C", "D", "E", "F", "G" };

            int today = DateTime.Today.Day; // today's day of the month

            // Execute based on odd/even
            if (today % 2 == 1)
            {
                Console.WriteLine($"Today is the {today}th → Odd day, running the list!");
                Shuffle(items);
                RunList(items);
            }
            else
            {
                Console.WriteLine($"Today is the {today}th → Even day, running the list!");
                Shuffle(items);
                RunList(items);
            }
        }

        // Fisher-Yates shuffle algorithm
        public static void Shuffle<T>(IList<T> list)
        {
            var rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void RunList(List<string> list)
        {
            foreach (var item in list)
            {
                Console.WriteLine("▶ processing: " + item);
            }
        }
    }
}



using Babble.Core;

namespace Babble.CLI;

internal class Program
{
    static void Main(string[] args)
    {
        BabbleCore.StartInference();
        for (int i = 0; i < 10; i++)
        {
            foreach (var item in BabbleCore.Expressions)
            {
                Console.WriteLine($"{item.Key}: {item.Value}");
            }
            Thread.Sleep(1000);
        }
        BabbleCore.StopInference();
    }
}

using System;
using NBitcoin;

namespace SigningUtil
{
    internal static class Program
    {
        private const string dening14PrivateKey = "KwDiBf89QgGbjEhKnhXJuH7SUWPU37sPj2c2ionJYMnWFj1ZLAiB";
        private const string deningrecoverPrivateKey = "KwDiBf89QgGbjEhKnhXJuH7Ro98XrkJMQ5PFYQ4reorxqRmVKhvL";

        private static void Main(string[] args)
        {
            Console.WriteLine("Enter a message to be signed by dening14@tt.ru and press Enter");
            var message = Console.ReadLine();
            var key = Key.Parse(dening14PrivateKey);
            var signed = key.SignMessage(message);
            Console.WriteLine("Copy a signed message for dening14@tt.ru from a line below and press any key");
            Console.WriteLine(signed);
            Console.WriteLine();


            Console.WriteLine("Enter a message to be signed by deningrecover@tt.ru and press Enter");
            message = Console.ReadLine();
            key = Key.Parse(deningrecoverPrivateKey);
            signed = key.SignMessage(message);
            Console.WriteLine("Copy a signed message for deningrecover@tt.ru from a line below and press any key to exit");
            Console.WriteLine(signed);
            Console.ReadKey();
        }
    }
}

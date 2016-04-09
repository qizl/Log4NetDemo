using Log4NetExtension;
using System;

namespace Log4NetDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            LogHelper.Initialize();
            LogHelper.WriteInfo("hello word!", "localhost", new Random().Next(100, 10000));
        }
    }
}

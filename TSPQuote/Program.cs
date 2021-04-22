using System;
using TSPQuote.Controllers;

namespace TSPQuote
{
    class Program
    {
        static void Main(string[] args)
        {
            ProgramController Controller = new ProgramController();
            Controller.Begin();
        }
    }
}

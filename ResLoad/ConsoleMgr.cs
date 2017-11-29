using System;
using System.Collections.Generic;


namespace ResLoad
{
    class ConsoleMgr
    {
        public static bool IsShowLog = false;

        public static void Init()
        {
            Console.BufferWidth = 500;
            Console.BufferHeight = 2000;
        }

		public static void LogRed(string str, bool isFromControl = false)
        {
            if ((isFromControl) && (!IsShowLog)) return;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.White;
        }

		public static void LogGreen(string str, bool isFromControl = false)
        {
            if ((isFromControl) && (!IsShowLog)) return;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.White;
        }

		public static void LogWhite(string str, bool isFromControl = false)
        {
            if ((isFromControl) && (!IsShowLog)) return;

            Console.WriteLine(str);
        }
    }
}

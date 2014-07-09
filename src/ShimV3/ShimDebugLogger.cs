//using System;
//using System.Diagnostics;
//using System.Globalization;
//using System.IO;
//using System.Threading;

//namespace NuGet.ShimV3
//{
//    internal static class ShimDebugLogger
//    {
//        public static void Log(string message)
//        {
//            #if DEBUG

//            Trace.WriteLine(message);

//            string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NuGet\\http-debug.log");

//            int tries = 0;

//            while (tries < 5)
//            {
//                tries++;

//                try
//                {
//                    using (StreamWriter writer = new StreamWriter(logPath, true))
//                    {
//                        writer.WriteLine(String.Format(CultureInfo.InvariantCulture, "[{0}] {1}", DateTime.Now.ToString(), message));
//                    }

//                    break;
//                }
//                catch (IOException)
//                {
//                    // ignore
//                    Thread.Sleep(100);
//                }
//            }

//            #endif
//        }
//    }
//}

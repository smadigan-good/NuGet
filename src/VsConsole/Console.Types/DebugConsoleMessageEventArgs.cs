using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet
{
    public class DebugConsoleMessageEventArgs : EventArgs
    {
        public ConsoleColor? Color { get; set; }

        public string Message { get; set; }

        public TimeSpan? Elapsed { get; set; }

        public Guid? Context { get; set; }

        public int? Bytes { get; set;}
    }
}

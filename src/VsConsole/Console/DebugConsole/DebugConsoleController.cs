using NuGet;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetConsole
{
    [Export(typeof(IDebugConsoleController))]
    public class DebugConsoleController : IDebugConsoleController
    {
        public DebugConsoleController()
        {

        }

        public void Log(string message)
        {
            Log(message, ConsoleColor.White);
        }

        public void Log(string message, ConsoleColor color)
        {
            Log(message, color, null, null, null);
        }

        public void Log(string message, ConsoleColor color, TimeSpan? span, int? bytes, Guid? context)
        {
            if (OnMessage != null)
            {
                DebugConsoleMessageEventArgs args = new DebugConsoleMessageEventArgs()
                {
                    Bytes = bytes,
                    Color = color,
                    Context = context,
                    Elapsed = span,
                    Message = message
                };

                OnMessage(this, args);
            }
        }

        public event EventHandler<DebugConsoleMessageEventArgs> OnMessage;
    }
}

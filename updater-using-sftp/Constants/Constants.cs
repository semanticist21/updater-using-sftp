using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater.Constants
{
    public enum ErrorLevel
    {
        Info,
        Warning,
        Error,
        Debug
    }

    public enum ConnectionStatus
    {
        Connected,
        Disconnected,
        Warning,
        Error
    }
}

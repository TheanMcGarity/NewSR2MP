using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSR2MP.Networking.Packet
{
    public struct TestLogMessage : NetworkMessage
    {
        public string MessageToLog;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovementPacketGeneratorMOP
{
    public class Packet
    {
        public string Opcode { get; set; }
    }
    public class ClientPacket : Packet
    {
        public ClientPacket(string opcode)
        {
            Opcode = opcode;
        }
    }

    public class ServerPacket : Packet
    {
    }
    public class ServerPacketAttribute : Attribute
    {
        public string Opcode { get; set; }

        public ServerPacketAttribute(string opcode)
        {
            Opcode = opcode;
        }
    }
    public class ClientPacketAttribute : Attribute
    {
        public string Opcode { get; set; }

        public ClientPacketAttribute(string opcode)
        {
            Opcode = opcode;
        }
    }
}

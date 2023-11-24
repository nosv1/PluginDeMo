using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameReaderCommon;
using SimHub.Plugins;

namespace PluginDeMo_v2.F1_2023
{
    // Each packet has the following header:
    // struct PacketHeader
    // {
    //     uint16    m_packetFormat;            // 2023
    //     uint8     m_gameYear;                // Game year - last two digits e.g. 23
    //     uint8     m_gameMajorVersion;        // Game major version - "X.00"
    //     uint8     m_gameMinorVersion;        // Game minor version - "1.XX"
    //     uint8     m_packetVersion;           // Version of this packet type, all start from 1
    //     uint8     m_packetId;                // Identifier for the packet type, see below
    //     uint64    m_sessionUID;              // Unique identifier for the session
    //     float     m_sessionTime;             // Session timestamp
    //     uint32    m_frameIdentifier;         // Identifier for the frame the data was retrieved on
    //     uint32    m_overallFrameIdentifier;  // Overall identifier for the frame the data was retrieved
    //                                         // on, doesn't go back after flashbacks
    //     uint8     m_playerCarIndex;          // Index of player's car in the array
    //     uint8     m_secondaryPlayerCarIndex; // Index of secondary player's car in the array (splitscreen)
    //                                         // 255 if no second player
    // };

    public class Header
    {
        int PacketFormat { get; set; } // 2023
        int GameYear { get; set; } // Game year - last two digits e.g. 23
        int GameMajorVersion { get; set; } // Game major version - "X.00"
        int GameMinorVersion { get; set; } // Game minor version - "1.XX"
        int PacketVersion { get; set; } // Version of this packet type, all start from 1
        int PacketId { get; set; } // Identifier for the packet type, see below
        long SessionUID { get; set; } // Unique identifier for the session
        float SessionTime { get; set; } // Session timestamp
        int FrameIdentifier { get; set; } // Identifier for the frame the data was retrieved on
        int OverallFrameIdentifier { get; set; } // Overall identifier for the frame the data was retrieved on, doesn't go back after flashbacks
        int PlayerCarIndex { get; set; } // Index of player's car in the array
        int SecondaryPlayerCarIndex { get; set; } // Index of secondary player's car in the array (splitscreen) 255 if no second player

        public Header() { }
    }
}

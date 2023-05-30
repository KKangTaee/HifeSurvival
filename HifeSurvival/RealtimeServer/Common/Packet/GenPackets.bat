START ../../PacketGenerator/bin/PacketGenerator.exe ../../PacketGenerator/PDL.xml
XCOPY /Y GenPackets.cs "../../DummyClient/Packet"
XCOPY /Y GenPackets.cs "../../Server/Packet"
XCOPY /Y ClientPacketManager.cs "../../DummyClient/Packet"
XCOPY /Y ServerPacketManager.cs "../../Server/Packet"
XCOPY /Y GenPackets.cs "../../../Assets/Scripts/Realtime/Packet"
XCOPY /Y ClientPacketManager.cs "../../../Assets/Scripts/Realtime/Packet"
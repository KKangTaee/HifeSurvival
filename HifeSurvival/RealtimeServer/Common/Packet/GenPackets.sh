mono ../../PacketGenerator/bin/PacketGenerator.dll ../../PacketGenerator/PDL.xml

cp -f GenPackets.cs "../../DummyClient/Packet"
cp -f GenPackets.cs "../../Server/Packet"
cp -f ClientPacketManager.cs "../../DummyClient/Packet"
cp -f ServerPacketManager.cs "../../Server/Packet"
cp -f GenPackets.cs "../../../Assets/Scripts/Realtime/Packet"
cp -f ClientPacketManager.cs "../../../Assets/Scripts/Realtime/Packet"
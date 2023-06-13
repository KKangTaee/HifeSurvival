START ../../PacketGenerator/bin/PacketGenerator.exe ../../PacketGenerator/PDL.xml
XCOPY /Y GenPackets.cs "../../ServerCore"
XCOPY /Y PacketManager.cs "../../ServerCore"
cd ../../ServerCore
dotnet build
XCOPY /Y "bin/Debug/netcoreapp3.1/ServerCore.dll" "../../../../../Assets/Scripts/Realtime"
pause

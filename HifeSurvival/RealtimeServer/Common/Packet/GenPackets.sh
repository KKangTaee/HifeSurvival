#패킷 생성
mono ../../PacketGenerator/bin/PacketGenerator.dll ../../PacketGenerator/PDL.xml

#ServerCore 프로젝트에 생성된 패킷 복사
cp -f GenPackets.cs "../../ServerCore"
cp -f PacketManager.cs "../../ServerCore"

#DLL 빌드 생성
cd ../../ServerCore
dotnet build

#클라이언트에 생성된 DLL 배포
cp -f bin/Debug/netstandard2.1/ServerCore.dll "../../Assets/Plugins"
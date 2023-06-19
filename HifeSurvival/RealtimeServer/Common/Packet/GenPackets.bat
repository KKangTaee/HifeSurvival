::패킷 생성
START ..\..\PacketGenerator\bin\PacketGenerator.exe ..\..\PacketGenerator\PDL.xml

::ServerCore 프로젝트에 생성된 패킷 복사
XCOPY /Y /C GenPackets.cs "..\..\ServerCore"
XCOPY /Y /C PacketManager.cs "..\..\ServerCore"

::DLL 빌드 생성
cd ..\..\ServerCore
dotnet build
 
::클라이언트에 생성된 DLL 배포
XCOPY /Y "bin\Debug\netstandard2.1\ServerCore.dll" "..\..\Assets\Plugins"
timeout /t 10

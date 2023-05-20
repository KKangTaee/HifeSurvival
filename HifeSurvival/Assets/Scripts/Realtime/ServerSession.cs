using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;
using UnityEngine;


public class ServerSession : PacketSession
{
	public override void OnConnected(EndPoint endPoint)
	{
		Debug.Log($"OnConnected : {endPoint}");
		NetworkManager.Instance.OnConnectResult(true);
	}

	public override void OnDisconnected(EndPoint endPoint)
	{
		Debug.Log($"OnDisConnected : {endPoint}");
		NetworkManager.Instance.OnDisconnectResult(true);
	}

	public override void OnRecvPacket(ArraySegment<byte> buffer)
	{
		PacketManager.Instance.OnRecvPacket(this, buffer,
		(session, packet)=>
		{
			PacketQueue.Instance.Push(packet);
		});
	}

	public override void OnSend(int numOfBytes)
	{
		Debug.Log($"바이트 호출 완료 Transferred bytes: {numOfBytes}");
	}
}
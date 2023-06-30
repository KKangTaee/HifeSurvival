using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;
using UnityEngine;


public class ClientSession : Session
{
	public override void OnConnected(EndPoint endPoint)
	{
		Debug.Log($"OnConnected : {endPoint}");
		NetworkManager.Instance.OnConnectResult(true);
	}

	public override void OnDisconnected(EndPoint endPoint)
	{
		Debug.Log($"OnDisConnected : {endPoint}");
		NetworkManager.Instance.OnDisconnectResult();
	}

	public override void OnRecv(ArraySegment<byte> buffer)
	{
		PacketManager.Instance.OnRecvPacket(this, buffer,
		(session, packet)=>
		{
			PacketQueue.Instance.Push(packet);
		});
	}
}
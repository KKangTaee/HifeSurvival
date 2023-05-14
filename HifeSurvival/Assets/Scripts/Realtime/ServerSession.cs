using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;
using UnityEngine;


class ServerSession : PacketSession
{
	public override void OnConnected(EndPoint endPoint)
	{
		Console.WriteLine($"OnConnected : {endPoint}");			
		Debug.Log($"OnConnected : {endPoint}");
	}

	public override void OnDisconnected(EndPoint endPoint)
	{
		Console.WriteLine($"OnDisconnected : {endPoint}");
		Debug.Log($"OnConnected : {endPoint}");
	}

	public override void OnRecvPacket(ArraySegment<byte> buffer)
	{
		// PacketManager.Instance.OnRecvPacket(this, buffer);
	}

	public override void OnSend(int numOfBytes)
	{
		//Console.WriteLine($"Transferred bytes: {numOfBytes}");
	}
}


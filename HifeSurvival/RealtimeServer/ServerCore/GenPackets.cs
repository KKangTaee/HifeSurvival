using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;



public enum PacketID
{
	C_JoinToGame = 1,
	S_JoinToGame = 2,
	S_LeaveToGame = 3,
	CS_SelectHero = 4,
	CS_ReadyToGame = 5,
	S_Countdown = 6,
	S_StartGame = 7,
	S_SpawnMonster = 8,
	CS_Attack = 9,
	MoveRequest = 10,
	MoveResponse = 11,
	S_Dead = 12,
	S_Respawn = 13,
	IncreaseStatRequest = 14,
	IncreaseStatResponse = 15,
	PickRewardRequest = 16,
	PickRewardResponse = 17,
	UpdateRewardBroadcast = 18,
	UpdateLocationBroadcast = 19,
	UpdateStatBroadcast = 20,
	UpdatePlayerCurrency = 21,
	
}

public interface IPacket
{
	ushort Protocol { get; }
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
}


public class C_JoinToGame : IPacket
{
	public string userId;
	public string userName;

	public ushort Protocol { get { return (ushort)PacketID.C_JoinToGame; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		ushort userIdLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.userId = Encoding.Unicode.GetString(s.Slice(count, userIdLen));
		count += userIdLen;
		ushort userNameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.userName = Encoding.Unicode.GetString(s.Slice(count, userNameLen));
		count += userNameLen;
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_JoinToGame);
		count += sizeof(ushort);
		ushort userIdLen = (ushort)Encoding.Unicode.GetByteCount(this.userId);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), userIdLen);
		count += sizeof(ushort);
		Encoding.Unicode.GetBytes(this.userId, s.Slice(count, s.Length - count));
		count += userIdLen;
		ushort userNameLen = (ushort)Encoding.Unicode.GetByteCount(this.userName);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), userNameLen);
		count += sizeof(ushort);
		Encoding.Unicode.GetBytes(this.userName, s.Slice(count, s.Length - count));
		count += userNameLen;
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_JoinToGame : IPacket
{
	public class JoinPlayer
	{
		public string userId;
		public string userName;
		public int id;
		public int heroKey;
	
		public void Read(ReadOnlySpan<byte> s, ref ushort count)
		{
			ushort userIdLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
			count += sizeof(ushort);
			this.userId = Encoding.Unicode.GetString(s.Slice(count, userIdLen));
			count += userIdLen;
			ushort userNameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
			count += sizeof(ushort);
			this.userName = Encoding.Unicode.GetString(s.Slice(count, userNameLen));
			count += userNameLen;
			this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
			this.heroKey = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
		}
	
		public bool Write(Span<byte> s, ref ushort count)
		{
			bool success = true;
			ushort userIdLen = (ushort)Encoding.Unicode.GetByteCount(this.userId);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), userIdLen);
			count += sizeof(ushort);
			Encoding.Unicode.GetBytes(this.userId, s.Slice(count, s.Length - count));
			count += userIdLen;
			ushort userNameLen = (ushort)Encoding.Unicode.GetByteCount(this.userName);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), userNameLen);
			count += sizeof(ushort);
			Encoding.Unicode.GetBytes(this.userName, s.Slice(count, s.Length - count));
			count += userNameLen;
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
			count += sizeof(int);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.heroKey);
			count += sizeof(int);
			return success;
		}	
	}
	public List<JoinPlayer> joinPlayerList = new List<JoinPlayer>();
	public int roomId;

	public ushort Protocol { get { return (ushort)PacketID.S_JoinToGame; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.joinPlayerList.Clear();
		ushort joinPlayerLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		for (int i = 0; i < joinPlayerLen; i++)
		{
			JoinPlayer joinPlayer = new JoinPlayer();
			joinPlayer.Read(s, ref count);
			joinPlayerList.Add(joinPlayer);
		}
		this.roomId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_JoinToGame);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.joinPlayerList.Count);
		count += sizeof(ushort);
		foreach (JoinPlayer joinPlayer in this.joinPlayerList)
			success &= joinPlayer.Write(s, ref count);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.roomId);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_LeaveToGame : IPacket
{
	public string userId;
	public int id;

	public ushort Protocol { get { return (ushort)PacketID.S_LeaveToGame; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		ushort userIdLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.userId = Encoding.Unicode.GetString(s.Slice(count, userIdLen));
		count += userIdLen;
		this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_LeaveToGame);
		count += sizeof(ushort);
		ushort userIdLen = (ushort)Encoding.Unicode.GetByteCount(this.userId);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), userIdLen);
		count += sizeof(ushort);
		Encoding.Unicode.GetBytes(this.userId, s.Slice(count, s.Length - count));
		count += userIdLen;
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class CS_SelectHero : IPacket
{
	public int id;
	public int heroKey;

	public ushort Protocol { get { return (ushort)PacketID.CS_SelectHero; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.heroKey = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.CS_SelectHero);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.heroKey);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class CS_ReadyToGame : IPacket
{
	public int id;

	public ushort Protocol { get { return (ushort)PacketID.CS_ReadyToGame; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.CS_ReadyToGame);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_Countdown : IPacket
{
	public int countdownSec;

	public ushort Protocol { get { return (ushort)PacketID.S_Countdown; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.countdownSec = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_Countdown);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.countdownSec);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_StartGame : IPacket
{
	public int playTimeSec;
	public List<PlayerSpawn> playerList = new List<PlayerSpawn>();
	public List<MonsterSpawn> monsterList = new List<MonsterSpawn>();

	public ushort Protocol { get { return (ushort)PacketID.S_StartGame; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.playTimeSec = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.playerList.Clear();
		ushort playerLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		for (int i = 0; i < playerLen; i++)
		{
			PlayerSpawn player = new PlayerSpawn();
			player.Read(s, ref count);
			playerList.Add(player);
		}
		this.monsterList.Clear();
		ushort monsterLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		for (int i = 0; i < monsterLen; i++)
		{
			MonsterSpawn monster = new MonsterSpawn();
			monster.Read(s, ref count);
			monsterList.Add(monster);
		}
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_StartGame);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playTimeSec);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.playerList.Count);
		count += sizeof(ushort);
		foreach (PlayerSpawn player in this.playerList)
			success &= player.Write(s, ref count);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.monsterList.Count);
		count += sizeof(ushort);
		foreach (MonsterSpawn monster in this.monsterList)
			success &= monster.Write(s, ref count);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_SpawnMonster : IPacket
{
	public List<MonsterSpawn> monsterList = new List<MonsterSpawn>();

	public ushort Protocol { get { return (ushort)PacketID.S_SpawnMonster; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.monsterList.Clear();
		ushort monsterLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		for (int i = 0; i < monsterLen; i++)
		{
			MonsterSpawn monster = new MonsterSpawn();
			monster.Read(s, ref count);
			monsterList.Add(monster);
		}
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_SpawnMonster);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.monsterList.Count);
		count += sizeof(ushort);
		foreach (MonsterSpawn monster in this.monsterList)
			success &= monster.Write(s, ref count);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class CS_Attack : IPacket
{
	public int id;
	public int targetId;
	public int attackValue;

	public ushort Protocol { get { return (ushort)PacketID.CS_Attack; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.targetId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.attackValue = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.CS_Attack);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.targetId);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.attackValue);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class MoveRequest : IPacket
{
	public int id;
	public PVec3 currentPos;
	public PVec3 targetPos;
	public float speed;
	public long timestamp;

	public ushort Protocol { get { return (ushort)PacketID.MoveRequest; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		currentPos.Read(s, ref count);
		targetPos.Read(s, ref count);
		this.speed = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.timestamp = BitConverter.ToInt64(s.Slice(count, s.Length - count));
		count += sizeof(long);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.MoveRequest);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
		count += sizeof(int);
		success &= currentPos.Write(s,ref count);
		success &= targetPos.Write(s,ref count);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.speed);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.timestamp);
		count += sizeof(long);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class MoveResponse : IPacket
{
	

	public ushort Protocol { get { return (ushort)PacketID.MoveResponse; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.MoveResponse);
		count += sizeof(ushort);
		
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_Dead : IPacket
{
	public int id;
	public int fromId;
	public int respawnTime;

	public ushort Protocol { get { return (ushort)PacketID.S_Dead; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.fromId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.respawnTime = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_Dead);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.fromId);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.respawnTime);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_Respawn : IPacket
{
	public int id;
	public PVec3 pos;
	public PStat stat;

	public ushort Protocol { get { return (ushort)PacketID.S_Respawn; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		pos.Read(s, ref count);
		stat.Read(s, ref count);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_Respawn);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
		count += sizeof(int);
		success &= pos.Write(s,ref count);
		success &= stat.Write(s,ref count);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class IncreaseStatRequest : IPacket
{
	public int id;
	public int type;
	public int increase;

	public ushort Protocol { get { return (ushort)PacketID.IncreaseStatRequest; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.type = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.increase = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.IncreaseStatRequest);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.type);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.increase);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class IncreaseStatResponse : IPacket
{
	public int id;
	public int type;
	public int increase;
	public int usedGold;
	public PStat originStat;
	public PStat addStat;
	public int result;

	public ushort Protocol { get { return (ushort)PacketID.IncreaseStatResponse; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.type = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.increase = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.usedGold = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		originStat.Read(s, ref count);
		addStat.Read(s, ref count);
		this.result = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.IncreaseStatResponse);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.type);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.increase);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.usedGold);
		count += sizeof(int);
		success &= originStat.Write(s,ref count);
		success &= addStat.Write(s,ref count);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.result);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class PickRewardRequest : IPacket
{
	public int id;
	public int worldId;

	public ushort Protocol { get { return (ushort)PacketID.PickRewardRequest; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.worldId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.PickRewardRequest);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.worldId);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class PickRewardResponse : IPacket
{
	public int id;
	public int worldId;
	public int rewardType;
	public int gold;
	public int itemSlotId;
	public PItem item;

	public ushort Protocol { get { return (ushort)PacketID.PickRewardResponse; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.worldId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.rewardType = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.gold = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.itemSlotId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		item.Read(s, ref count);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.PickRewardResponse);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.worldId);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.rewardType);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.gold);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.itemSlotId);
		count += sizeof(int);
		success &= item.Write(s,ref count);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class UpdateRewardBroadcast : IPacket
{
	public int worldId;
	public int status;
	public int rewardType;
	public int gold;
	public PItem item;
	public PVec3 pos;

	public ushort Protocol { get { return (ushort)PacketID.UpdateRewardBroadcast; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.worldId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.status = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.rewardType = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.gold = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		item.Read(s, ref count);
		pos.Read(s, ref count);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.UpdateRewardBroadcast);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.worldId);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.status);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.rewardType);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.gold);
		count += sizeof(int);
		success &= item.Write(s,ref count);
		success &= pos.Write(s,ref count);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class UpdateLocationBroadcast : IPacket
{
	public int id;
	public PVec3 currentPos;
	public PVec3 targetPos;
	public float speed;
	public long timestamp;

	public ushort Protocol { get { return (ushort)PacketID.UpdateLocationBroadcast; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		currentPos.Read(s, ref count);
		targetPos.Read(s, ref count);
		this.speed = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.timestamp = BitConverter.ToInt64(s.Slice(count, s.Length - count));
		count += sizeof(long);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.UpdateLocationBroadcast);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
		count += sizeof(int);
		success &= currentPos.Write(s,ref count);
		success &= targetPos.Write(s,ref count);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.speed);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.timestamp);
		count += sizeof(long);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class UpdateStatBroadcast : IPacket
{
	public int id;
	public PStat originStat;
	public PStat addStat;

	public ushort Protocol { get { return (ushort)PacketID.UpdateStatBroadcast; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		originStat.Read(s, ref count);
		addStat.Read(s, ref count);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.UpdateStatBroadcast);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
		count += sizeof(int);
		success &= originStat.Write(s,ref count);
		success &= addStat.Write(s,ref count);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class UpdatePlayerCurrency : IPacket
{
	public int id;
	public List<PCurrency> currencyList = new List<PCurrency>();

	public ushort Protocol { get { return (ushort)PacketID.UpdatePlayerCurrency; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.currencyList.Clear();
		ushort currencyLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		for (int i = 0; i < currencyLen; i++)
		{
			PCurrency currency = new PCurrency();
			currency.Read(s, ref count);
			currencyList.Add(currency);
		}
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.UpdatePlayerCurrency);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.currencyList.Count);
		count += sizeof(ushort);
		foreach (PCurrency currency in this.currencyList)
			success &= currency.Write(s, ref count);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}
public struct PVec3
{
	public float x;
	public float y;
	public float z;

	public void Read(ReadOnlySpan<byte> s, ref ushort count)
	{
		this.x = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.y = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.z = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
	}

	public bool Write(Span<byte> s, ref ushort count)
	{
		bool success = true;
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.x);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.y);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.z);
		count += sizeof(float);
		return success;
	}	
}
	public struct PStat
{
	public int str;
	public int def;
	public int hp;
	public float moveSpeed;
	public float attackSpeed;

	public void Read(ReadOnlySpan<byte> s, ref ushort count)
	{
		this.str = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.def = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.hp = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.moveSpeed = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.attackSpeed = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
	}

	public bool Write(Span<byte> s, ref ushort count)
	{
		bool success = true;
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.str);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.def);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.hp);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.moveSpeed);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.attackSpeed);
		count += sizeof(float);
		return success;
	}	
}
	public struct PItem
{
	public int itemKey;
	public int level;
	public int str;
	public int def;
	public int hp;
	public int cooltime;
	public bool canUse;

	public void Read(ReadOnlySpan<byte> s, ref ushort count)
	{
		this.itemKey = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.level = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.str = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.def = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.hp = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.cooltime = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.canUse = BitConverter.ToBoolean(s.Slice(count, s.Length - count));
		count += sizeof(bool);
	}

	public bool Write(Span<byte> s, ref ushort count)
	{
		bool success = true;
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.itemKey);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.level);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.str);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.def);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.hp);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.cooltime);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.canUse);
		count += sizeof(bool);
		return success;
	}	
}
	public struct PCurrency
{
	public int currencyType;
	public int count;

	public void Read(ReadOnlySpan<byte> s, ref ushort count)
	{
		this.currencyType = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.count = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
	}

	public bool Write(Span<byte> s, ref ushort count)
	{
		bool success = true;
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.currencyType);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.count);
		count += sizeof(int);
		return success;
	}	
}
	public class PlayerSpawn
{
	public int id;
	public int herosKey;
	public PVec3 pos;

	public void Read(ReadOnlySpan<byte> s, ref ushort count)
	{
		this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.herosKey = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		pos.Read(s, ref count);
	}

	public bool Write(Span<byte> s, ref ushort count)
	{
		bool success = true;
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.herosKey);
		count += sizeof(int);
		success &= pos.Write(s,ref count);
		return success;
	}	
}
	public class MonsterSpawn
{
	public int id;
	public int monstersKey;
	public int groupId;
	public int grade;
	public PVec3 pos;

	public void Read(ReadOnlySpan<byte> s, ref ushort count)
	{
		this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.monstersKey = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.groupId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.grade = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		pos.Read(s, ref count);
	}

	public bool Write(Span<byte> s, ref ushort count)
	{
		bool success = true;
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.monstersKey);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.groupId);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.grade);
		count += sizeof(int);
		success &= pos.Write(s,ref count);
		return success;
	}	
}
	

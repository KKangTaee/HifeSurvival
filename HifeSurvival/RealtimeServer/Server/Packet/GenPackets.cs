using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;

public enum PacketID
{
	C_Chat = 1,
	S_Chat = 2,
	C_JoinToGame = 3,
	S_JoinToGame = 4,
	S_LeaveToGame = 5,
	SelectHero = 6,
	ReadyToGame = 7,
	S_Countdown = 8,
	S_StartGame = 9,
	C_Attack = 10,
	S_JoinOther = 11,
	S_LeaveOther = 12,
	
}

public interface IPacket
{
	ushort Protocol { get; }
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
}


public class C_Chat : IPacket
{
	public string chat;

	public ushort Protocol { get { return (ushort)PacketID.C_Chat; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		ushort chatLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(s.Slice(count, chatLen));
		count += chatLen;
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_Chat);
		count += sizeof(ushort);
		ushort chatLen = (ushort)Encoding.Unicode.GetByteCount(this.chat);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), chatLen);
		count += sizeof(ushort);
		Encoding.Unicode.GetBytes(this.chat, s.Slice(count, s.Length - count));
		count += chatLen;
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_Chat : IPacket
{
	public int playerId;
	public string chat;

	public ushort Protocol { get { return (ushort)PacketID.S_Chat; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.playerId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		ushort chatLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(s.Slice(count, chatLen));
		count += chatLen;
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_Chat);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
		count += sizeof(int);
		ushort chatLen = (ushort)Encoding.Unicode.GetByteCount(this.chat);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), chatLen);
		count += sizeof(ushort);
		Encoding.Unicode.GetBytes(this.chat, s.Slice(count, s.Length - count));
		count += chatLen;
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
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
		public int playerId;
		public int heroId;
	
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
			this.playerId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
			this.heroId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
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
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
			count += sizeof(int);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.heroId);
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
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class SelectHero : IPacket
{
	public int playerId;
	public int heroId;

	public ushort Protocol { get { return (ushort)PacketID.SelectHero; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.playerId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.heroId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.SelectHero);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.heroId);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class ReadyToGame : IPacket
{
	public int playerId;

	public ushort Protocol { get { return (ushort)PacketID.ReadyToGame; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.playerId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.ReadyToGame);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
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
	public class Player
	{
		public int playerId;
		public int heroId;
	
		public void Read(ReadOnlySpan<byte> s, ref ushort count)
		{
			this.playerId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
			this.heroId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
		}
	
		public bool Write(Span<byte> s, ref ushort count)
		{
			bool success = true;
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
			count += sizeof(int);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.heroId);
			count += sizeof(int);
			return success;
		}	
	}
	public List<Player> playerList = new List<Player>();
	public class Monster
	{
		public int monsterId;
		public int monsterType;
		public int groupId;
		public int subId;
	
		public void Read(ReadOnlySpan<byte> s, ref ushort count)
		{
			this.monsterId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
			this.monsterType = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
			this.groupId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
			this.subId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
		}
	
		public bool Write(Span<byte> s, ref ushort count)
		{
			bool success = true;
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.monsterId);
			count += sizeof(int);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.monsterType);
			count += sizeof(int);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.groupId);
			count += sizeof(int);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.subId);
			count += sizeof(int);
			return success;
		}	
	}
	public List<Monster> monsterList = new List<Monster>();

	public ushort Protocol { get { return (ushort)PacketID.S_StartGame; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.playerList.Clear();
		ushort playerLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		for (int i = 0; i < playerLen; i++)
		{
			Player player = new Player();
			player.Read(s, ref count);
			playerList.Add(player);
		}
		this.monsterList.Clear();
		ushort monsterLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		for (int i = 0; i < monsterLen; i++)
		{
			Monster monster = new Monster();
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
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.playerList.Count);
		count += sizeof(ushort);
		foreach (Player player in this.playerList)
			success &= player.Write(s, ref count);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.monsterList.Count);
		count += sizeof(ushort);
		foreach (Monster monster in this.monsterList)
			success &= monster.Write(s, ref count);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class C_Attack : IPacket
{
	public bool toIdIsPlayer;
	public int toId;
	public int fromId;
	public int damageValue;

	public ushort Protocol { get { return (ushort)PacketID.C_Attack; } }

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		this.toIdIsPlayer = BitConverter.ToBoolean(s.Slice(count, s.Length - count));
		count += sizeof(bool);
		this.toId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.fromId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.damageValue = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_Attack);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.toIdIsPlayer);
		count += sizeof(bool);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.toId);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.fromId);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.damageValue);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}
public struct Vec3
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
	
public class S_JoinOther : IPacket
{
	public string userId;
	public string userName;
	public int plaeyrId;
	public int heroId;

	public ushort Protocol { get { return (ushort)PacketID.S_JoinOther; } }

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
		this.plaeyrId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.heroId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_JoinOther);
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
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.plaeyrId);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.heroId);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}

public class S_LeaveOther : IPacket
{
	public string userId;
	public int playerId;

	public ushort Protocol { get { return (ushort)PacketID.S_LeaveOther; } }

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
		this.playerId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
	}

	public ArraySegment<byte> Write()
	{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_LeaveOther);
		count += sizeof(ushort);
		ushort userIdLen = (ushort)Encoding.Unicode.GetByteCount(this.userId);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), userIdLen);
		count += sizeof(ushort);
		Encoding.Unicode.GetBytes(this.userId, s.Slice(count, s.Length - count));
		count += userIdLen;
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}
}


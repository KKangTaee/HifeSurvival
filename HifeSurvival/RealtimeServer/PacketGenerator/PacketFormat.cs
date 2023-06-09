﻿using System;
using System.Collections.Generic;
using System.Text;

namespace PacketGenerator
{
	class PacketFormat
	{
		// {0} 패킷 등록
		// {1} 핸들러 등록
		// {2} 핸들러 바인딩
		public static string managerFormat =
@"using System;
using System.Collections.Generic;
using ServerCore;


public abstract class PacketHandler
{{
{1}
}}

public class PacketManager
{{
	#region Singleton
	static PacketManager _instance = new PacketManager();
	public static PacketManager Instance {{ get {{ return _instance; }} }}
	#endregion

	PacketManager()
	{{
		Register();
	}}


	Dictionary<ushort, Func<Session, ArraySegment<byte>, IPacket>> _makeFunc = new Dictionary<ushort, Func<Session, ArraySegment<byte>, IPacket>>();
	Dictionary<ushort, Action<Session, IPacket>> _handler = new Dictionary<ushort, Action<Session, IPacket>>();
	
	public void BindHandler(PacketHandler handler)
	{{
		_handler.Clear();
{2}
	}}

	public void Register()
	{{
{0}
	}}

	public void OnRecvPacket(Session session, ArraySegment<byte> buffer, Action<Session, IPacket> onRecvCallback = null)
	{{
		ushort count = 0;

		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		if(_makeFunc.TryGetValue(id, out var func) == true)
		{{
			IPacket packet = func.Invoke(session, buffer);

			if(onRecvCallback != null)
			   onRecvCallback.Invoke(session, packet);
			else
				HandlePacket(session,packet);
		}}
	}}

	T MakePacket<T>(Session session, ArraySegment<byte> buffer) where T : IPacket, new()
	{{
		T pkt = new T();
		pkt.Read(buffer);
		return pkt;	
	}}

	public void HandlePacket(Session inSession, IPacket inPacket)
	{{
		Action<Session, IPacket> action = null;
		if (_handler.TryGetValue(inPacket.Protocol, out action))
			action.Invoke(inSession, inPacket);
	}}
}}
";
		// {0} 패킷 이름
		public static string managerHandlerFormat =
@"	public virtual void {0}Handler(Session session, IPacket packet) {{}}";


        // {0} 패킷 이름
        public static string managerRegisterFormat =
@"		_makeFunc.Add((ushort)PacketID.{0}, MakePacket<{0}>);";

		public static string managerBindFormat =
@"		_handler.Add((ushort)PacketID.{0}, handler.{0}Handler);";

        // {0} 패킷 이름/번호 목록
        // {1} 패킷 목록
        public static string fileFormat =
@"using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;



public enum PacketID
{{
	{0}
}}

public interface IPacket
{{
	ushort Protocol {{ get; }}
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
}}

{1}
";

		// {0} 패킷 이름
		// {1} 패킷 번호
		public static string packetEnumFormat =
@"{0} = {1},";

		 

        // {0} 패킷 이름
        // {1} 멤버 변수들
        // {2} 멤버 변수 Read
        // {3} 멤버 변수 Write
        public static string packetFormat =
@"
[Serializable]
public class {0} : IPacket
{{
	{1}

	public ushort Protocol {{ get {{ return (ushort)PacketID.{0}; }} }}

	public void Read(ArraySegment<byte> segment)
	{{
		ushort count = 0;

		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		{2}
	}}

	public ArraySegment<byte> Write()
	{{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.{0});
		count += sizeof(ushort);
		{3}
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
			return null;
		return SendBufferHelper.Close(count);
	}}
}}
";
		// {0} 변수 형식
		// {1} 변수 이름
		public static string memberFormat =
@"public {0} {1} {{ get; set; }}";

        public static string memberFormatStruct =
@"private {0} _{1};
public {0} {1} 
{{ 
	get {{ return _{1}; }} 
	set {{ _{1} = value; }}
}}";

        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        // {2} 멤버 변수들
        // {3} 멤버 변수 Read
        // {4} 멤버 변수 Write
        public static string memberListFormat =
@"
public class {0}
{{
	{2}

	public void Read(ReadOnlySpan<byte> s, ref ushort count)
	{{
		{3}
	}}

	public bool Write(Span<byte> s, ref ushort count)
	{{
		bool success = true;
		{4}
		return success;
	}}	
}}
public List<{0}> {1}List {{ get; set;}} = new List<{0}>();";

			public static string structFormat =
@"
public struct {0}
{{
	{1}

	public {0} Read(ReadOnlySpan<byte> s, ref ushort count)
	{{
		{2}
		return this;
	}}

	public bool Write(Span<byte> s, ref ushort count)
	{{
		bool success = true;
		{3}
		return success;
	}}	
}}";

		public static string classFormat =
@"
public class {0}
{{
	{1}

	public void Read(ReadOnlySpan<byte> s, ref ushort count)
	{{
		{2}
	}}

	public bool Write(Span<byte> s, ref ushort count)
	{{
		bool success = true;
		{3}
		return success;
	}}	
}}";

		// {0} 변수 이름
		// {1} To~ 변수 형식
		// {2} 변수 형식
		public static string readFormat =
@"this.{0} = BitConverter.{1}(s.Slice(count, s.Length - count));
count += sizeof({2});";

		// {0} 변수 이름
		// {1} 변수 형식
		public static string readByteFormat =
@"this.{0} = ({1})segment.Array[segment.Offset + count];
count += sizeof({1});";

		// {0} 변수 이름
		public static string readStringFormat =
@"ushort {0}Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);
this.{0} = Encoding.Unicode.GetString(s.Slice(count, {0}Len));
count += {0}Len;";

		// {0} 리스트 이름 [대문자]
		// {1} 리스트 이름 [소문자]
		public static string readListFormat =
@"this.{1}List.Clear();
ushort {1}Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);
for (int i = 0; i < {1}Len; i++)
{{
	{0} {1} = new {0}();
	{1}.Read(s, ref count);
	{1}List.Add({1});
}}";

		// {0} 변수 이름
		// {1} 변수 형식
		public static string writeFormat =
@"success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.{0});
count += sizeof({1});";

		// {0} 변수 이름
		// {1} 변수 형식
		public static string writeByteFormat =
@"segment.Array[segment.Offset + count] = (byte)this.{0};
count += sizeof({1});";

		// {0} 변수 이름
		public static string writeStringFormat =
@"ushort {0}Len = (ushort)Encoding.Unicode.GetByteCount(this.{0});
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), {0}Len);
count += sizeof(ushort);
Encoding.Unicode.GetBytes(this.{0}, s.Slice(count, s.Length - count));
count += {0}Len;";

		// {0} 리스트 이름 [대문자]
		// {1} 리스트 이름 [소문자]
		public static string writeListFormat =
@"success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.{1}List.Count);
count += sizeof(ushort);
foreach ({0} {1} in this.{1}List)
	success &= {1}.Write(s, ref count);";

		public static string writeStructFormat =
@"success &= {0}.Write(s,ref count);";

		public static string readStructFormat =
@"{0} = {0}.Read(s, ref count);";

        public static string readClassFormat =
@"{0}.Read(s, ref count);";

        public static string memberStructFormat =
@"public List<{0}> {1}List {{ get; set; }} = new List<{0}>();";
	}
}

using System;

namespace BLivehimeNTR;

internal static class BLiveBase
{
    private static byte[] CreatePacket(short version, ServerOperation operation, byte[] body)
    {
        var packetLength = 16 + body.Length;
        var result = new byte[packetLength];
        Buffer.BlockCopy(ToBigEndianBytes(packetLength), 0, result, 0, 4);
        Buffer.BlockCopy(ToBigEndianBytes((short)16), 0, result, 4, 2);
        Buffer.BlockCopy(ToBigEndianBytes(version), 0, result, 6, 2);
        Buffer.BlockCopy(ToBigEndianBytes((int)operation), 0, result, 8, 4);
        Buffer.BlockCopy(ToBigEndianBytes(0), 0, result, 12, 4);
        Buffer.BlockCopy(body, 0, result, 16, body.Length);
        return result;
    }

    internal static byte[] CreateSmsPacket(short version, byte[] body)
    {
        return CreatePacket(version, ServerOperation.OpSendSmsReply, body);
    }

    internal static byte[] CreateHeartbeatPacket(short version)
    {
        return CreatePacket(version, ServerOperation.OpHeartbeatReply, ToBigEndianBytes(1));
    }

    private static byte[] ToBigEndianBytes(int value)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return bytes;
    }

    private static byte[] ToBigEndianBytes(short value)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return bytes;
    }

    private enum ServerOperation
    {
        OpHeartbeatReply = 3,
        OpSendSmsReply = 5
    }
}
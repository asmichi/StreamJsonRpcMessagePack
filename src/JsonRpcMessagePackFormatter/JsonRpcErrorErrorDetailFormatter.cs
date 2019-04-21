// Copyright (c) @asmichi (on github). Licensed under the MIT License. See LICENCE in the project root for details.

using System;
using MessagePack;
using MessagePack.Formatters;
using StreamJsonRpc.Protocol;

namespace Asmichi.StreamJsonRpcAdapters
{
    // Serializes the value as: Serializes the value as [Code, Message, Data].
    internal sealed class JsonRpcErrorErrorDetailFormatter : IMessagePackFormatter<JsonRpcError.ErrorDetail>
    {
        public static readonly IMessagePackFormatter<JsonRpcError.ErrorDetail> Instance = new JsonRpcErrorErrorDetailFormatter();

        public int Serialize(ref byte[] bytes, int offset, JsonRpcError.ErrorDetail value, IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return MessagePackBinary.WriteNil(ref bytes, offset);
            }

            var startOffset = offset;

            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, 3);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, (int)value.Code);
            offset += MessagePackBinary.WriteString(ref bytes, offset, value.Message);
            offset += TypelessFormatter.Instance.Serialize(ref bytes, offset, value.Data, formatterResolver);

            return offset - startOffset;
        }

        public JsonRpcError.ErrorDetail Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            int singleReadSize;

            if (MessagePackBinary.ReadArrayHeader(bytes, offset, out singleReadSize) != 3)
            {
                throw new InvalidOperationException("Invalid JsonRpcError.ErrorDetail format.");
            }

            offset += singleReadSize;

            var code = (JsonRpcErrorCode)MessagePackBinary.ReadInt32(bytes, offset, out singleReadSize);
            offset += singleReadSize;

            var message = MessagePackBinary.ReadString(bytes, offset, out singleReadSize);
            offset += singleReadSize;

            var data = TypelessFormatter.Instance.Deserialize(bytes, offset, formatterResolver, out singleReadSize);
            offset += singleReadSize;

            readSize = offset - startOffset;
            return new JsonRpcError.ErrorDetail()
            {
                Code = code,
                Message = message,
                Data = data,
            };
        }
    }
}

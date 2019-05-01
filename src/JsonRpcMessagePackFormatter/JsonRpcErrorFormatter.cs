// Copyright (c) @asmichi (https://github.com/asmichi). Licensed under the MIT License. See LICENCE in the project root for details.

using System;
using MessagePack;
using MessagePack.Formatters;
using StreamJsonRpc.Protocol;

namespace Asmichi.StreamJsonRpcAdapters
{
    // Serializes the value as: [Version, Id, ErrorDetail]
    internal sealed class JsonRpcErrorFormatter : IMessagePackFormatter<JsonRpcError>
    {
        public static readonly IMessagePackFormatter<JsonRpcError> Instance = new JsonRpcErrorFormatter();

        public int Serialize(ref byte[] bytes, int offset, JsonRpcError value, IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return MessagePackBinary.WriteNil(ref bytes, offset);
            }

            var startOffset = offset;

            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, 3);
            offset += MessagePackBinary.WriteString(ref bytes, offset, value.Version);
            offset += TypelessFormatter.Instance.Serialize(ref bytes, offset, value.Id, formatterResolver);
            offset += JsonRpcErrorErrorDetailFormatter.Instance.Serialize(ref bytes, offset, value.Error, formatterResolver);

            return offset - startOffset;
        }

        public JsonRpcError Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
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
                throw new InvalidOperationException("Invalid JsonRpcError format.");
            }

            offset += singleReadSize;

            var version = MessagePackBinary.ReadString(bytes, offset, out singleReadSize);
            offset += singleReadSize;

            var id = TypelessFormatter.Instance.Deserialize(bytes, offset, formatterResolver, out singleReadSize);
            offset += singleReadSize;

            var error = JsonRpcErrorErrorDetailFormatter.Instance.Deserialize(bytes, offset, formatterResolver, out singleReadSize);
            offset += singleReadSize;

            readSize = offset - startOffset;
            return new JsonRpcError()
            {
                Version = version,
                Id = id,
                Error = error,
            };
        }
    }
}

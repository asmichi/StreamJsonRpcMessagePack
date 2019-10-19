// Copyright (c) @asmichi (https://github.com/asmichi). Licensed under the MIT License. See LICENCE in the project root for details.

using System;
using MessagePack;
using MessagePack.Formatters;
using StreamJsonRpc.Protocol;

namespace Asmichi.StreamJsonRpcAdapters
{
    internal sealed class JsonRpcResultFormatter : IMessagePackFormatter<JsonRpcResult?>
    {
        public static readonly IMessagePackFormatter<JsonRpcResult?> Instance = new JsonRpcResultFormatter();

        private JsonRpcResultFormatter()
        {
        }

        // [Version, Id, Result]
        public int Serialize(ref byte[] bytes, int offset, JsonRpcResult? value, IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return MessagePackBinary.WriteNil(ref bytes, offset);
            }

            var startOffset = offset;

            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, 3);
            offset += MessagePackBinary.WriteString(ref bytes, offset, value.Version);
            offset += TypelessFormatter.Instance.Serialize(ref bytes, offset, value.Id, formatterResolver);
            offset += TypelessFormatter.Instance.Serialize(ref bytes, offset, value.Result, formatterResolver);

            return offset - startOffset;
        }

        public JsonRpcResult? Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
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
                throw new InvalidOperationException("Invalid JsonRpcResult format.");
            }

            offset += singleReadSize;

            var version = MessagePackBinary.ReadString(bytes, offset, out singleReadSize);
            offset += singleReadSize;

            var id = TypelessFormatter.Instance.Deserialize(bytes, offset, formatterResolver, out singleReadSize);
            offset += singleReadSize;

            var result = TypelessFormatter.Instance.Deserialize(bytes, offset, formatterResolver, out singleReadSize);
            offset += singleReadSize;

            readSize = offset - startOffset;
            return new JsonRpcResult()
            {
                Version = version,
                Id = id,
                Result = result,
            };
        }
    }
}

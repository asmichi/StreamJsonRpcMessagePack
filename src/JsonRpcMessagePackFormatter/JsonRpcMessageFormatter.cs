// Copyright (c) @asmichi (on github). Licensed under the MIT License. See LICENCE in the project root for details.

using System;
using MessagePack;
using MessagePack.Formatters;
using StreamJsonRpc.Protocol;

namespace Asmichi.StreamJsonRpcAdapters
{
    // Serializes the value as: [MessageKind, value]
    internal sealed class JsonRpcMessageFormatter : IMessagePackFormatter<JsonRpcMessage>
    {
        public static readonly IMessagePackFormatter<JsonRpcMessage> Instance = new JsonRpcMessageFormatter();

        private JsonRpcMessageFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, JsonRpcMessage value, IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return MessagePackBinary.WriteNil(ref bytes, offset);
            }

            var startOffset = offset;

            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, 2);

            switch (value)
            {
                case JsonRpcRequest request:
                    offset += MessagePackBinary.WritePositiveFixedIntUnsafe(ref bytes, offset, (int)MessageKind.Request);
                    offset += JsonRpcRequestFormatter.Instance.Serialize(ref bytes, offset, request, formatterResolver);
                    break;

                case JsonRpcResult result:
                    offset += MessagePackBinary.WritePositiveFixedIntUnsafe(ref bytes, offset, (int)MessageKind.Result);
                    offset += JsonRpcResultFormatter.Instance.Serialize(ref bytes, offset, result, formatterResolver);
                    break;

                case JsonRpcError error:
                    offset += MessagePackBinary.WritePositiveFixedIntUnsafe(ref bytes, offset, (int)MessageKind.Error);
                    offset += JsonRpcErrorFormatter.Instance.Serialize(ref bytes, offset, error, formatterResolver);
                    break;

                default:
                    throw new ArgumentException(
                        string.Format("Unknown type of JsonRpcMessage: {0}", value.GetType()),
                        nameof(value));
            }

            return offset - startOffset;
        }

        public JsonRpcMessage Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            int singleReadSize;

            if (MessagePackBinary.ReadArrayHeader(bytes, offset, out singleReadSize) != 2)
            {
                throw new InvalidOperationException("Invalid JsonRpcMessage format.");
            }

            offset += singleReadSize;

            var kind = (MessageKind)MessagePackBinary.ReadInt32(bytes, offset, out singleReadSize);
            offset += singleReadSize;

            JsonRpcMessage value;
            switch (kind)
            {
                case MessageKind.Request:
                    value = JsonRpcRequestFormatter.Instance.Deserialize(bytes, offset, formatterResolver, out singleReadSize);
                    break;

                case MessageKind.Result:
                    value = JsonRpcResultFormatter.Instance.Deserialize(bytes, offset, formatterResolver, out singleReadSize);
                    break;

                case MessageKind.Error:
                    value = JsonRpcErrorFormatter.Instance.Deserialize(bytes, offset, formatterResolver, out singleReadSize);
                    break;

                default:
                    throw new InvalidOperationException(string.Format("Invalid value of MessageKind: {0}", kind));
            }

            offset += singleReadSize;

            readSize = offset - startOffset;
            return value;
        }

        // NOTE: Every value must fit into positive fixint [0, 127].
        private enum MessageKind
        {
            Request = 0,
            Result = 1,
            Error = 2,
        }
    }
}

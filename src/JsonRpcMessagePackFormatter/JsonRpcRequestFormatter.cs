// Copyright (c) @asmichi (https://github.com/asmichi). Licensed under the MIT License. See LICENCE in the project root for details.

using System;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;
using StreamJsonRpc.Protocol;

namespace Asmichi.StreamJsonRpcAdapters
{
    // Serializes the value as: [Version, Id, Method, Arguments]
    internal sealed class JsonRpcRequestFormatter : IMessagePackFormatter<JsonRpcRequest>
    {
        public static readonly IMessagePackFormatter<JsonRpcRequest> Instance = new JsonRpcRequestFormatter();

        private JsonRpcRequestFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, JsonRpcRequest value, IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return MessagePackBinary.WriteNil(ref bytes, offset);
            }

            var startOffset = offset;

            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, 4);
            offset += MessagePackBinary.WriteString(ref bytes, offset, value.Version);
            offset += TypelessFormatter.Instance.Serialize(ref bytes, offset, value.Id, formatterResolver);
            offset += MessagePackBinary.WriteString(ref bytes, offset, value.Method);

            switch (value.Arguments)
            {
                case null:
                    offset += MessagePackBinary.WriteNil(ref bytes, offset);
                    break;

                case IReadOnlyList<object> args:
                    offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, args.Count);

                    for (var i = 0; i < args.Count; i++)
                    {
                        offset += TypelessFormatter.Instance.Serialize(ref bytes, offset, args[i], formatterResolver);
                    }

                    break;

                case IReadOnlyDictionary<string, object> argMap:
                    offset += MessagePackBinary.WriteMapHeader(ref bytes, offset, argMap.Count);

                    foreach (var x in argMap)
                    {
                        offset += MessagePackBinary.WriteString(ref bytes, offset, x.Key);
                        offset += TypelessFormatter.Instance.Serialize(ref bytes, offset, x.Value, formatterResolver);
                    }

                    break;

                default:
                    throw new ArgumentException("value.Arguments must be of one of: null, IReadOnlyList<object>, IReadOnlyDictionary<string, object>.", nameof(value));
            }

            return offset - startOffset;
        }

        public JsonRpcRequest Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            int singleReadSize;

            if (MessagePackBinary.ReadArrayHeader(bytes, offset, out singleReadSize) != 4)
            {
                throw new InvalidOperationException("Invalid JsonRpcMessage format.");
            }

            offset += singleReadSize;

            var version = MessagePackBinary.ReadString(bytes, offset, out singleReadSize);
            offset += singleReadSize;

            var id = TypelessFormatter.Instance.Deserialize(bytes, offset, formatterResolver, out singleReadSize);
            offset += singleReadSize;

            var method = MessagePackBinary.ReadString(bytes, offset, out singleReadSize);
            offset += singleReadSize;

            object arguments;
            switch (MessagePackBinary.GetMessagePackType(bytes, offset))
            {
                case MessagePackType.Nil:
                    {
                        arguments = null;
                        offset++;
                        break;
                    }

                case MessagePackType.Array:
                    {
                        var length = MessagePackBinary.ReadArrayHeader(bytes, offset, out singleReadSize);
                        offset += singleReadSize;

                        var array = new object[length];
                        for (var i = 0; i < length; i++)
                        {
                            array[i] = TypelessFormatter.Instance.Deserialize(bytes, offset, formatterResolver, out singleReadSize);
                            offset += singleReadSize;
                        }

                        arguments = array;
                        break;
                    }

                case MessagePackType.Map:
                    {
                        var length = MessagePackBinary.ReadMapHeader(bytes, offset, out singleReadSize);
                        offset += singleReadSize;

                        var map = new Dictionary<string, object>(length);
                        for (var i = 0; i < length; i++)
                        {
                            var key = MessagePackBinary.ReadString(bytes, offset, out singleReadSize);
                            offset += singleReadSize;
                            var value = TypelessFormatter.Instance.Deserialize(bytes, offset, formatterResolver, out singleReadSize);
                            offset += singleReadSize;
                            map.Add(key, value);
                        }

                        arguments = map;
                        break;
                    }

                default:
                    throw new InvalidOperationException(string.Format("Invalid code : {0}", bytes[offset]));
            }

            readSize = offset - startOffset;
            return new JsonRpcRequest()
            {
                Version = version,
                Id = id,
                Method = method,
                Arguments = arguments,
            };
        }
    }
}

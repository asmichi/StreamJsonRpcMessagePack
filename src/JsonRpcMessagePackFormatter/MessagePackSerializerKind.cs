// Copyright (c) @asmichi (on github). Licensed under the MIT License. See LICENCE in the project root for details.

namespace Asmichi.StreamJsonRpcAdapters
{
    /// <summary>
    /// Represents a kind of serializer supplied by MessagePack for C#.
    /// </summary>
    public enum MessagePackSerializerKind
    {
        /// <summary>
        /// <see cref="MessagePack.LZ4MessagePackSerializer"/>.
        /// </summary>
        LZ4MessagePackSerializer,

        /// <summary>
        /// <see cref="MessagePack.MessagePackSerializer"/>.
        /// </summary>
        MessagePackSerializer,
    }
}

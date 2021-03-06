// Copyright (c) @asmichi (https://github.com/asmichi). Licensed under the MIT License. See LICENCE in the project root for details.

using MessagePack;
using StreamJsonRpc;

namespace Asmichi.StreamJsonRpcAdapters
{
    /// <summary>
    /// Represents options of <see cref="JsonRpcMessageFormatter"/>.
    /// </summary>
    public class JsonRpcMessagePackFormatterOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonRpcMessagePackFormatterOptions"/> class.
        /// <see cref="Resolver"/> must be initialized later.
        /// </summary>
        public JsonRpcMessagePackFormatterOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonRpcMessagePackFormatterOptions"/> class.
        /// </summary>
        /// <param name="resolver">An <see cref="IFormatterResolver"/> instance that will be used for (de)serialization.</param>
        public JsonRpcMessagePackFormatterOptions(IFormatterResolver resolver)
        {
            // NOTE: Do not throw ArgumentNullException. Consumers prior to 0.2.0 may pass null to `resolver` because `resolver` had the default value of null before 0.2.0.
            Resolver = resolver;
        }

        /// <summary>
        /// Specifies the <see cref="IFormatterResolver"/> instance that will be used for (de)serialization.
        /// </summary>
        public IFormatterResolver? Resolver { get; set; }

        /// <summary>
        /// Specifies which serializer should be used.
        /// The default is <see cref="MessagePackSerializerKind.LZ4MessagePackSerializer"/>.
        /// </summary>
        public MessagePackSerializerKind SerializerKind { get; set; } = MessagePackSerializerKind.LZ4MessagePackSerializer;

        /// <summary>
        /// <para>
        /// (Experimental) Specifies whether parameter objects (<see cref="JsonRpc.InvokeWithParameterObjectAsync"/>) is allowed.
        /// The default is <see langword="false"/>.
        /// </para>
        /// <para>If <see langword="true"/>, parameter objects are converted to JSON maps using Json.NET. This behavior is subject to change.</para>
        /// </summary>
        public bool AllowParameterObject { get; set; } = false;
    }
}

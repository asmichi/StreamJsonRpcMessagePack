// Copyright (c) @asmichi (https://github.com/asmichi). Licensed under the MIT License. See LICENCE in the project root for details.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Asmichi.StreamJsonRpcAdapters;
using Nerdbank.Streams;
using StreamJsonRpc;

namespace JsonRpcMessagePackFormatterExample
{
    public static class Program
    {
        public static async Task Main()
        {
            var (streamA, streamB) = FullDuplexStream.CreatePair();

            using (var sideA = CreateRpc("A", streamA))
            using (var sideB = CreateRpc("B", streamB))
            {
                var ret = await sideA.InvokeAsync<int>("Mul", 4, 2);
                Console.WriteLine(ret);
            }
        }

        private static JsonRpc CreateRpc(string name, Stream duplexStream)
        {
            // Create a JsonRpcMessagePackFormatter as an IJsonRpcMessageFormatter.
            var formatter = new JsonRpcMessagePackFormatter(MyCompositeResolver.Instance);

            // Create a JsonRpc that uses the IJsonRpcMessageFormatter.
            var handler = new LengthHeaderMessageHandler(duplexStream, duplexStream, formatter);
            var rpc = new JsonRpc(handler)
            {
                TraceSource = new TraceSource(name, SourceLevels.Verbose),
            };

            rpc.AddLocalRpcTarget(new ServerObject());
            rpc.StartListening();
            return rpc;
        }

        private sealed class ServerObject
        {
            public int Mul(int a, int b) => a * b;
        }
    }
}

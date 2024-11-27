﻿using Azure.Core.Serialization;
using Microsoft.Azure.Cosmos;
using System.Text.Json;


namespace NCS.DSS.LearningProgression.Cosmos
{
    /// <remarks>
    /// See: https://github.com/Azure/azure-cosmos-dotnet-v3/blob/master/Microsoft.Azure.Cosmos.Samples/Usage/SystemTextJson/CosmosSystemTextJsonSerializer.cs
    /// </remarks>
    public sealed class CosmosSystemTextJsonSerializer : CosmosSerializer
    {
        private readonly JsonObjectSerializer _systemTextJsonSerializer;

        public CosmosSystemTextJsonSerializer(JsonSerializerOptions jsonSerializerOptions)
        {
            _systemTextJsonSerializer = new JsonObjectSerializer(jsonSerializerOptions);
        }

        public override T FromStream<T>(Stream stream)
        {
            if (stream.CanSeek && stream.Length == 0)
            {
                return default;
            }

            if (typeof(Stream).IsAssignableFrom(typeof(T)))
            {
                return (T) (object) stream;
            }

            using (stream)
            {
                return (T) _systemTextJsonSerializer.Deserialize(stream, typeof(T), default);
            }
        }

        public override Stream ToStream<T>(T input)
        {
            var streamPayload = new MemoryStream();
            _systemTextJsonSerializer.Serialize(streamPayload, input, typeof(T), default);
            streamPayload.Position = 0;
            return streamPayload;
        }
    }
}
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MaiksMixer.Core.Logging;

namespace MaiksMixer.Core.Communication
{
    /// <summary>
    /// Provides serialization and deserialization functionality for messages.
    /// </summary>
    public static class MessageSerializer
    {
        private static readonly JsonSerializerOptions _serializerOptions;
        
        /// <summary>
        /// Initializes the static members of the MessageSerializer class.
        /// </summary>
        static MessageSerializer()
        {
            // Configure serializer options
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true
            };
        }
        
        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>A JSON string representation of the object, or null if serialization failed.</returns>
        public static string? Serialize<T>(T obj)
        {
            if (obj == null)
            {
                LogManager.Warn("Cannot serialize null object");
                return null;
            }
            
            try
            {
                string json = JsonSerializer.Serialize(obj, _serializerOptions);
                return json;
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to serialize object of type {typeof(T).Name}: {ex.Message}", ex);
                return null;
            }
        }
        
        /// <summary>
        /// Deserializes a JSON string to an object.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized object, or default(T) if deserialization failed.</returns>
        public static T? Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                LogManager.Warn("Cannot deserialize null or empty JSON string");
                return default;
            }
            
            try
            {
                T? obj = JsonSerializer.Deserialize<T>(json, _serializerOptions);
                return obj;
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to deserialize JSON to type {typeof(T).Name}: {ex.Message}", ex);
                return default;
            }
        }
        
        /// <summary>
        /// Attempts to deserialize a JSON string to an object.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="result">When this method returns, contains the deserialized object if deserialization succeeded, or default(T) if deserialization failed.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        public static bool TryDeserialize<T>(string json, out T? result)
        {
            result = default;
            
            if (string.IsNullOrEmpty(json))
            {
                LogManager.Warn("Cannot deserialize null or empty JSON string");
                return false;
            }
            
            try
            {
                result = JsonSerializer.Deserialize<T>(json, _serializerOptions);
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to deserialize JSON to type {typeof(T).Name}: {ex.Message}", ex);
                return false;
            }
        }
    }
}

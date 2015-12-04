// Copyright © 2010-2015 Firebrand Technologies

//using JWT;
using Jose;
using ServiceStack.Logging;
using ServiceStack.Text;

namespace Fcs.Framework
{
    public class JoseJWTJsonSerializer : IJsonSerializer
    {
        private static ILog _logger;

        private static ILog Logger
        {
            get { return _logger ?? (_logger = LogManager.GetLogger("FcsClient")); }
        }

        static JoseJWTJsonSerializer()
        {
            JsConfig.TryToParseNumericType = true;
            JsConfig.ParsePrimitiveIntegerTypes = ParseAsType.Int32 | ParseAsType.Int64;
        }

        public string Serialize(object obj)
        {
            var json = JsonSerializer.SerializeToString(obj);
            Logger.Debug("SERIALIZE: " + json);
            return json;
        }

        public T Deserialize<T>(string json)
        {
            Logger.Debug("DESERIALIZE: " + json);
            return JsonSerializer.DeserializeFromString<T>(json);
        }
    }

    /// <summary>
    /// Provides JSON Serialize and Deserialize.  Allows custom serializers used.
    /// </summary>
    public interface IJsonSerializer
    {
        /// <summary>
        /// Serialize an object to JSON string
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>JSON string</returns>
        string Serialize(object obj);

        /// <summary>
        /// Deserialize a JSON string to typed object.
        /// </summary>
        /// <typeparam name="T">type of object</typeparam>
        /// <param name="json">JSON string</param>
        /// <returns>typed object</returns>
        T Deserialize<T>(string json);
    }
}
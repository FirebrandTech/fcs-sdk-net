// Copyright © 2010-2015 Firebrand Technologies

//using JWT;


using ServiceStack.Logging;
using ServiceStack.Text;

namespace Fcs.Framework {

    /// <summary>
    ///     public interface IJsonSerializer {
    ///    T Deserialize<T>(string json);
    ///    string Serialize(object obj);
    ///}
    /// : IJsonSerializer
    /// </summary>
    public class ServiceStackJsonSerializer : IJsonSerializer {
        private static ILog _logger;

        private static ILog Logger {
            get { return _logger ?? (_logger = LogManager.GetLogger("FcsClient")); }
        }

        static ServiceStackJsonSerializer() {
            JsConfig.TryToParseNumericType = true;
            JsConfig.ParsePrimitiveIntegerTypes = ParseAsType.Int32 | ParseAsType.Int64;
        }

        public string Serialize(object obj) {
            var json = JsonSerializer.SerializeToString(obj);
            Logger.Debug("SERIALIZE: " + json);
            return json;
        }

        public T Deserialize<T>(string json) {
            Logger.Debug("DESERIALIZE: " + json);
            return JsonSerializer.DeserializeFromString<T>(json);
        }
    }
}
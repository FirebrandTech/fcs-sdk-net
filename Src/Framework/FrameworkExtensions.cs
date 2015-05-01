// Copyright © 2010-2015 Firebrand Technologies

using System;
using System.Collections.Generic;
using ServiceStack;

namespace Fcs.Framework {
    public class AccessToken {
        public string T { get; set; }
        public DateTime E { get; set; }
        public string U { get; set; }
    }

    public static class FrameworkExtensions {
        public static bool IsNullOrWhiteSpace(this string s) {
            return string.IsNullOrWhiteSpace(s);
        }

        public static bool IsFull(this string s) {
            return !string.IsNullOrWhiteSpace(s);
        }

        public static TValue Value<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) {
            return dictionary.ContainsKey(key) ? dictionary[key] : default(TValue);
        }

        public static DateTime? AsUtc(this DateTime? dateTime) {
            if (dateTime == null) {
                return null;
            }
            return new DateTime(dateTime.Value.Ticks, DateTimeKind.Utc);
        }

        public static DateTime? AsLocal(this DateTime? dateTime) {
            if (dateTime == null) {
                return null;
            }
            return new DateTime(dateTime.Value.Ticks, DateTimeKind.Local);
        }

        public static DateTime? ToUtc(this DateTime? dateTime) {
            if (dateTime == null) {
                return null;
            }
            return dateTime.Value.ToUniversalTime();
        }

        public static string ToUrlSafeBase64(this byte[] bytes) {
            string s = Convert.ToBase64String(bytes);
            s = s.Replace("/", "_")
                 .Replace("+", "-");
            return s.TrimEnd('=');
        }

        public static byte[] FromUrlSafeBase64(this string s) {
            s = s.PadRight(s.Length + (4 - s.Length%4)%4, '=');
            s = s.Replace("_", "/")
                 .Replace("-", "+");
            return Convert.FromBase64String(s);
        }

        public static AccessToken DeserializeToken(this string serialized) {
            return serialized.FromUrlSafeBase64().FromUtf8Bytes().FromJsv<AccessToken>();
        }

        public static string SerializeToken(this AccessToken token) {
            return token.ToJsv().ToUtf8Bytes().ToUrlSafeBase64();
        }

        public static string SerializeToken(this FcsToken token) {
            return new AccessToken
                   {
                       T = token.Value,
                       E = token.Expires,
                       U = token.User
                   }.SerializeToken();
        }
    }
}
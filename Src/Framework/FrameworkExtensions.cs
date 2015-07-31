// Copyright © 2010-2015 Firebrand Technologies

using System;
using System.Collections.Generic;
using System.Web;

namespace Fcs.Framework {
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

        public static string RemoveParam(this Uri uri, string key) {
            // this gets all the query string key value pairs as a collection
            var newQueryString = HttpUtility.ParseQueryString(uri.Query);

            // this removes the key if exists
            newQueryString.Remove(key);

            // this gets the page path from root without QueryString
            var pagePathWithoutQueryString = uri.GetLeftPart(UriPartial.Path);

            return newQueryString.Count > 0
                ? String.Format("{0}?{1}", pagePathWithoutQueryString, newQueryString)
                : pagePathWithoutQueryString;
        }

        public static Guid? ToGuid(this string value) {
            if (string.IsNullOrEmpty(value))
                return null;

            try {
                if (value.Length == 36)
                    return new Guid(value);

                if (value.Length != 22)
                    return null;

                value = value
                    .Replace("_", "/")
                    .Replace("-", "+");
                var buffer = Convert.FromBase64String(value + "==");
                return new Guid(buffer);
            }
            catch {
                return null;
            }
        }

        public static string ToShortString(this Guid? guid) {
            if (guid == null || guid.Value == Guid.Empty) return null;
            var encoded = Convert.ToBase64String(guid.Value.ToByteArray());
            encoded = encoded
                .Replace("/", "_")
                .Replace("+", "-");
            return encoded.Substring(0, 22);
        }
    }
}
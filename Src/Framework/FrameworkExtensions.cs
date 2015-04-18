// Copyright © 2010-2015 Firebrand Technologies

using System;
using System.Collections.Generic;

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

    }
}
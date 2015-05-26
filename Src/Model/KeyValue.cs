// Copyright © 2010 Firebrand Technologies

#region Usings

using System;

#endregion

namespace Fcs.Model {
    public class KeyValue : IKeyValue {
        public KeyValue() {}

        public KeyValue(Guid? id, string value) {
            this.Key = id.ToString();
            this.Value = value;
        }

        public KeyValue(int id, string value) {
            this.Key = id.ToString();
            this.Value = value;
        }

        public KeyValue(string key, string value) {
            this.Key = key;
            this.Value = value;
        }

        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class KeyValue<TKey, TValue> : IKeyValue<TKey, TValue> {
        public KeyValue(TKey key, TValue value) {
            this.Key = key;
            this.Value = value;
        }

        public TKey Key { get; set; }
        public TValue Value { get; set; }
    }

    public class CodeName {
        public CodeName() {}

        public CodeName(int code, string name) {
            this.Code = code;
            this.Name = name;
        }

        public int Code { get; set; }
        public string Name { get; set; }
    }

    public class IdName {
        public IdName(Guid? id, string name) {
            this.Id = id;
            this.Name = name;
        }

        public Guid? Id { get; set; }
        public string Name { get; set; }
    }
}
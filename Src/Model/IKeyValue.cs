// Copyright © 2010-2014 Firebrand Technologies

namespace Fcs.Model {
    public interface IKeyValue {
        string Key { get; }
        string Value { get; }
    }

    public interface IKeyValue<out TKey, out TValue> {
        TKey Key { get; }
        TValue Value { get; }
    }
}
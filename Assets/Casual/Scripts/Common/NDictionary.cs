using System;
using System.Collections;
using System.Collections.Generic;

public class NDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>> where TKey : IComparable<TKey>
{
    private IComparer<TKey> comparer;

    List<TKey> keyOrder = new List<TKey>();
    public List<TKey> KeyOrder { get { return keyOrder; } }

    public new TValue this[TKey key]
    {
        get { return base[key]; }
        set { Add(key, value); }
    }

    public NDictionary() : this(Comparer<TKey>.Default) { }
    public NDictionary(IComparer<TKey> comparer)
    {
        this.comparer = comparer == null ? Comparer<TKey>.Default : comparer;
    }

    public new Enumerator GetEnumerator()
    {
        return new Enumerator(keyOrder, this);
    }

    public new void Add(TKey k, TValue v)
    {
        if (!ContainsKey(k))
        {
            base.Add(k, v);
            keyOrder.Add(k);
        }
        else
        {
            base[k] = v;
        }
    }

    public new void Remove(TKey k)
    {
        base.Remove(k);
        keyOrder.Remove(k);
    }

    public new void Clear()
    {
        base.Clear();
        keyOrder.Clear();
    }

    public void Sort()
    {
        keyOrder.Sort(this.comparer);
    }

    public void Sort(IComparer<TKey> compare)
    {
        keyOrder.Sort(compare);
    }

    public TValue GetTop()
    {
        int index = keyOrder.Count - 1;
        return GetValueByIndex(index);
    }

    public TValue GetBottom()
    {
        return GetValueByIndex(0);
    }

    public int IndexOf(TKey key)
    {
        for (int index = 0, len = keyOrder.Count; index < len; index++)
        {
            if (keyOrder[index].CompareTo(key) == 0)
                return index;
        }
        return -1;
    }

    public TValue GetValueByIndex(int index, bool sort = false)
    {
        if (sort) Sort();
        TValue value = default(TValue);
        TKey key = index < keyOrder.Count && index >= 0 ? keyOrder[index] : default(TKey);
        TryGetValue(key, out value);

        return value;
    }

    public new class Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        List<TKey> order;
        Dictionary<TKey, TValue> dictionary;

        int index = -1;

        public Enumerator(List<TKey> order, Dictionary<TKey, TValue> dictionary)
        {
            this.order = order;
            this.dictionary = dictionary;
        }

        public KeyValuePair<TKey, TValue> Current { get; private set; }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public void Dispose() { }

        public bool MoveNext()
        {
            index++;

            if (index < dictionary.Count)
                Current = new KeyValuePair<TKey, TValue>(order[index], dictionary[order[index]]);

            return index < order.Count;
        }

        public void Reset()
        {
            index = -1;
        }
    }
}

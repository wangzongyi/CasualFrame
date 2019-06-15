using System;
using System.Collections.Generic;

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    public class ObjectPool
    {
        public ObjectPool(System.Func<object> creater)
        {
            Creater = creater;
        }

        private const int MAX_SIZE = 20;
        private Queue<object> objQueue = new Queue<object>();

        public Func<object> Creater;

        /// <summary>
        /// 如果池内已经有了该实例，直接返回
        /// </summary>
        /// <param name="obj"></param>
        public void Enqueue(object obj)
        {
            if (Count >= MAX_SIZE || objQueue.Contains(obj))
                return;

            objQueue.Enqueue(obj);
        }

        public T Dequeue<T>() where T : class
        {
            object instObject = null;

            if (objQueue.Count > 0)
                instObject = objQueue.Dequeue();
            else
                instObject = Creater();

            return instObject == null ? null : instObject as T;
        }

        public bool Contains(object obj)
        {
            return objQueue.Contains(obj);
        }

        public int Count { get { return objQueue.Count; } }

        public void ClearThisPool()
        {
            objQueue.Clear();
        }
    }


    private readonly Dictionary<Type, ObjectPool> _objectPools = new Dictionary<Type, ObjectPool>();
    private const int MAX_SIZE = 20;

    public void RegistCreater<T>(Func<object> creater) where T : class
    {
        Type type = typeof(T);
        if (!_objectPools.ContainsKey(type))
        {
            _objectPools[type] = new ObjectPool(creater);
        }
    }

    public T FetchObject<T>() where T : class
    {
        Type type = typeof(T);
        return _objectPools.ContainsKey(type) ? _objectPools[type].Dequeue<T>() : null;
    }

    public void ReturnObject<T>(T obj) where T : class, IDisposable
    {
        if (obj == null)
            return;

        Type type = typeof(T);
        if (_objectPools.ContainsKey(type))
        {
            obj.Dispose();
            _objectPools[type].Enqueue(obj);
        }
    }

    public void ClearPool<T>() where T : class
    {
        Type type = typeof(T);
        if(_objectPools.ContainsKey(type))
        {
            _objectPools[type].ClearThisPool();
        }
    }

    public void ClearPool()
    {
        foreach (ObjectPool objectPool in _objectPools.Values)
        {
            objectPool.ClearThisPool();
        }
    }
}

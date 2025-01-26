using System;
using System.Collections.Concurrent;

namespace ConsoleGameEngine.Core.GameObjects;

public class ObjectPool<T>(Func<T> objectFactory)
{
    private readonly ConcurrentBag<T> _objects = [];
    private readonly Func<T> _objectGenerator = objectFactory ?? throw new ArgumentNullException(nameof(objectFactory));

    public T Get()
    {
        return _objects.TryTake(out var item) ? item : _objectGenerator();
    }

    public void Return(T obj)
    {
        _objects.Add(obj);
    }
}
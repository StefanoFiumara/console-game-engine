using System;
using System.Collections.Concurrent;

namespace ConsoleGameEngine.Core.GameObjects;

public class ObjectPool<T>(Func<T> objectFactory)
{
    private readonly ConcurrentBag<T> _objects = [];

    public T Get() => _objects.TryTake(out var item) ? item : objectFactory();
    public void Return(T obj) => _objects.Add(obj);
}
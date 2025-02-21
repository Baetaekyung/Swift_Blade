using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    private readonly Dictionary<Type, IEntityComponent> componentDictionary = new();
    protected virtual void Awake()
    {
        var componentList =
            GetComponentsInChildren<IEntityComponent>(true).
            ToList();
        componentList.ForEach(x => InitializeEntityComponent(x));
    }
    protected virtual void Start()
    {
        var componentList = componentDictionary.Values.OfType<IEntityComponentStart>().ToList();
        componentList.ForEach(x => x.EntityComponentStart(this));
    }
    private IEntityComponent InitializeEntityComponent(IEntityComponent component)
    {
        componentDictionary.Add(component.GetType(), component);
        component.EntityComponentAwake(this);
        return component;
    }
    public T GetEntityComponent<T>() where T : Component, IEntityComponent
    {
        if (componentDictionary.TryGetValue(typeof(T), out IEntityComponent value))
            return value as T;

        Debug.LogError($"[ERROR]can't find {typeof(T)}, ReInitializing...");
        T missingInstance = GetComponentInChildren<T>(true);
        if (missingInstance == null)
        {
            Debug.LogError("Can't find Component");
            return null;
        }
        
        return InitializeEntityComponent(missingInstance) as T;
    }
}

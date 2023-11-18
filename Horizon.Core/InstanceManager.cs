using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogz.Logging;
using Bogz.Logging.Loggers;

namespace Horizon.Core;

/// <summary>
/// A niche class that specialises in managing object instances that all decsend from a parent class.
/// </summary>
/// <typeparam name="InstanceType">The base class that all instances have decsended from</typeparam>
public class InstanceManager<InstanceType>
{
    public Dictionary<Type, InstanceType> Instances { get; } = new();

    /// <summary>
    /// Returns the currently selected active instance.
    /// </summary>
    /// <returns>The instance currently active.</returns>
    public InstanceType? CurrentInstance { get; protected set; }

    /// <summary>
    /// Adds a instantiated instance to the manager.
    /// </summary>
    /// <typeparam name="T">The type (has to be descended from InstanceType)</typeparam>
    /// <param name="instance">The instance.</param>
    public virtual void AddInstance<T>(InstanceType instance)
        where T : InstanceType
    {
        if (Instances.Count == 0)
        {
            Instances.Add(typeof(T), instance);
            ChangeInstance(typeof(T));
            return;
        }

        Instances.Add(typeof(T), instance);
    }

    /// <summary>
    /// Instansiates and adds an instance to the manager.
    /// </summary>
    /// <typeparam name="T">The type (has to be descended from InstanceType)</typeparam>
    public void AddInstance<T>()
        where T : InstanceType, new() => AddInstance<T>(Activator.CreateInstance<T>());

    /// <summary>
    /// Adds an instance of the specified type to the manager.
    /// </summary>
    /// <param name="type">The type of the game screen to add.</param>
    public void AddInstance(Type type)
    {
        if (!typeof(InstanceType).IsAssignableFrom(type))
        {
            // FIXME: !we gotta throw some kind of exception or atleast desing a subscription model for these types of errors.
            ConcurrentLogger
                .Instance
                .Log(LogLevel.Error, $"The specified type must implement {nameof(InstanceType)}.");
        }

        var instance = (InstanceType)Activator.CreateInstance(type)!;
        AddInstance<InstanceType>(instance);
    }

    /// <summary>
    /// Removes an instance of a type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void RemoveInstance<T>()
        where T : InstanceType
    {
        Instances.Remove(typeof(T));
    }

    /// <summary>
    /// Changes the current active Instance.
    /// </summary>
    /// <param name="type">The type of the game screen to switch to.</param>
    public virtual void ChangeInstance(Type type)
    {
        if (!Instances.ContainsKey(type))
            AddInstance(type);

        CurrentInstance = Instances[type];
    }
}

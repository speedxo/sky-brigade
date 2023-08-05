using SkyBrigade.Engine.GameEntity.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyBrigade.Engine.GameEntity;

/// <summary>
/// Specifies that a class requires the presence of a specific component type to be added.
/// This attribute can be applied to classes that implement the IGameComponent interface
/// to indicate their dependencies on other components.
/// </summary>
/// <remarks>
/// Use this attribute to define dependencies between components in an entity-component system.
/// When applied to a class, it indicates that the class requires a specific component type
/// to be present in the entity before it can be added.
/// </remarks>
/// <example>
/// The following example shows a component class that requires a TransformComponent
/// to be present in the entity before it can be added.
/// <code>
/// [RequiresComponent(typeof(TransformComponent))]
/// public class MeshRendererComponent : IGameComponent
/// {
///     // Component implementation
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class RequiresComponentAttribute : Attribute
{
    /// <summary>
    /// Gets the required component type that this class depends on.
    /// </summary>
    public Type ComponentType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RequiresComponentAttribute"/> class
    /// with the specified component type.
    /// </summary>
    /// <param name="type">The type of the required component.</param>
    public RequiresComponentAttribute(Type type)
    {
        ComponentType = type;
    }
}

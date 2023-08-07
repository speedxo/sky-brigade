﻿namespace SkyBrigade.Engine.Collections
{
    /// <summary>
    /// A niche class that specialises in managing object instances that all decsend from a parent class.
    /// </summary>
    /// <typeparam name="InstanceType">The base class that all instances have decsended from</typeparam>
    public class InstanceManager<InstanceType>
    {
        protected readonly Dictionary<Type, InstanceType> instances = new();
        protected Type key;

        /// <summary>
        /// Returns the currently selected active instance.
        /// </summary>
        /// <returns>The instance pointed to by the indexer.</returns>
        public InstanceType GetCurrentInstance() => instances[key];

        /// <summary>
        /// Adds a instantiated instance to the manager.
        /// </summary>
        /// <typeparam name="T">The type (has to be descended from InstanceType)</typeparam>
        /// <param name="instance">The instance.</param>
        public void AddInstance<T>(InstanceType instance) where T: InstanceType
        {
            instances[typeof(T)] = instance;
            if (key == null)
                key = typeof(T);
        }
        /// <summary>
        /// Instansiates and adds an instance to the manager.
        /// </summary>
        /// <typeparam name="T">The type (has to be descended from InstanceType)</typeparam>
        public void AddInstance<T>() where T : InstanceType
            => AddInstance<T>(Activator.CreateInstance<T>());

        /// <summary>
        /// Adds an instance of the specified type to the manager.
        /// </summary>
        /// <param name="type">The type of the game screen to add.</param>
        public void AddInstance(Type type)
        {
            if (!typeof(InstanceType).IsAssignableFrom(type))
            {
                throw new ArgumentException("The specified type must implement IGameScreen.");
            }

            var instance = (InstanceType)Activator.CreateInstance(type);
            AddInstance<InstanceType>(instance);
        }

        /// <summary>
        /// Removes an instance of a type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RemoveInstance<T>() where T: InstanceType
        {
            instances.Remove(typeof(T));
        }

        /// <summary>
        /// Changes the current active Instance.
        /// </summary>
        /// <param name="type">The type of the game screen to switch to.</param>
        public void ChangeInstance(Type type)
        {
            if (!instances.ContainsKey(type))
            {
                var newScreen = (InstanceType) Activator.CreateInstance(type);

                if (newScreen == null)
                    throw new NullReferenceException("An impossible scenario has occurred, perhaps a single event upset occurred??");


                instances.Add(type, newScreen);
            }

            key = type;
        }
    }
}
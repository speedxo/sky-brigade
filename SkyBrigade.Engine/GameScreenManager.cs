using Silk.NET.OpenGL;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.Rendering;
using System;
using System.Collections.Generic;

namespace SkyBrigade.Engine
{
    /// <summary>
    /// Manages the game screens and facilitates screen changes.
    /// </summary>
    public class GameScreenManager : Entity, IDisposable
    {
        private readonly Dictionary<Type, IGameScreen> screens = new();
        private Type currentScreenType;


        /// <summary>
        /// Adds a game screen to the manager.
        /// </summary>
        /// <typeparam name="T">The type of the game screen.</typeparam>
        /// <param name="screen">The game screen instance.</param>
        public void AddGameScreen<T>(IGameScreen screen) where T : IGameScreen
        {
            screens[typeof(T)] = screen;

            if (currentScreenType == null)
            {
                currentScreenType = typeof(T);
            }
        }

        /// <summary>
        /// Removes a game screen from the manager.
        /// </summary>
        /// <typeparam name="T">The type of the game screen.</typeparam>
        public void RemoveGameScreen<T>() where T : IGameScreen
        {
            screens.Remove(typeof(T));
        }

        /// <summary>
        /// Changes the current active game screen.
        /// </summary>
        /// <param name="type">The type of the game screen to switch to.</param>
        public void ChangeGameScreen(Type type)
        {
            if (!screens.ContainsKey(type))
            {
                var newScreen = Activator.CreateInstance(type) as IGameScreen;

                if (newScreen == null)
                    throw new NullReferenceException("An impossible scenario has occurred, perhaps a single event upset occurred??");

                newScreen.Initialize(GameManager.Instance.Gl);
                //newScreen.LoadContent();

                currentScreenType = type;
                screens.Add(type, newScreen);
            }

            if (screens.TryGetValue(type, out var screen) && type != currentScreenType)
            {
                if (screens.TryGetValue(currentScreenType, out var currentScreen))
                {
                    /*  TODO: need some kind of system to load and unload larger
                     *  objects for gamescreens we intent to go back to.
                     */
                    //currentScreen.UnloadContent();
                }
            }
        }

        /// <summary>
        /// Updates the current active game screen.
        /// </summary>
        /// <param name="dt">The time since the last update.</param>
        public override void Update(float dt)
        {
            base.Update(dt);

            if (screens.TryGetValue(currentScreenType, out var screen))
            {
                screen.Update(dt);
            }
        }

        /// <summary>
        /// Renders the current active game screen.
        /// </summary>
        /// <param name="gl">The OpenGL context.</param>
        /// <param name="dt">The time since the last render.</param>
        public override void Draw(float dt, RenderOptions? options = null)
        {
            base.Draw(dt, options);

            if (screens.TryGetValue(currentScreenType, out var screen))
            {
                screen.Draw(dt, options);
            }
        }

        /// <summary>
        /// Disposes all the game screens managed by the manager.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);

            foreach (var item in screens.Values)
                item.Dispose();
        }
    }
}

using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;

namespace SkyBrigade.Engine
{
    /// <summary>
    /// Manages the game screens and facilitates screen changes.
    /// </summary>
    public class GameScreenManager : IDisposable
    {
        private Dictionary<Type, IGameScreen> screens;
        private Type currentScreenType;
        private GL gl;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameScreenManager"/> class.
        /// </summary>
        /// <param name="gl">The OpenGL context.</param>
        public GameScreenManager(GL gl)
        {
            this.gl = gl;
            screens = new Dictionary<Type, IGameScreen>();
        }

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

                newScreen.Initialize(gl);
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
        public void Update(float dt)
        {
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
        public void Render(GL gl, float dt)
        {
            if (screens.TryGetValue(currentScreenType, out var screen))
            {
                screen.Render(gl, dt);
            }
        }

        /// <summary>
        /// Disposes all the game screens managed by the manager.
        /// </summary>
        public void Dispose()
        {
            foreach (var item in screens.Values)
                item.Dispose();
        }
    }
}

using Silk.NET.OpenGL;

namespace SkyBrigade.Engine;

public class GameScreenManager : IDisposable
{
    private Dictionary<Type, IGameScreen> screens;
    private Type currentScreenType;
    private GL gl;

    public GameScreenManager(GL gl)
    {
        this.gl = gl;
        screens = new Dictionary<Type, IGameScreen>();
    }

    public void AddGameScreen<T>(IGameScreen screen) where T : IGameScreen
    {
        screens[typeof(T)] = screen;

        if (currentScreenType == null)
        {
            currentScreenType = typeof(T);
        }
    }

    public void RemoveGameScreen<T>() where T : IGameScreen
    {
        screens.Remove(typeof(T));
    }

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

    public void Update(float dt)
    {
        if (screens.TryGetValue(currentScreenType, out var screen))
        {
            screen.Update(dt);
        }
    }

    public void Render(GL gl, float dt)
    {
        if (screens.TryGetValue(currentScreenType, out var screen))
        {
            screen.Render(gl, dt);
        }
    }

    public void Dispose()
    {
        foreach (var item in screens.Values)
            item.Dispose();
    }
}
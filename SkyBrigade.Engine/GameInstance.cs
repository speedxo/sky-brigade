using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using SkyBrigade.Engine.Content;
using SkyBrigade.Engine.Logging;
using Silk.NET.OpenGL.Extensions.APPLE;

// Namespace declaration for the GameManager class
namespace SkyBrigade.Engine
{
    /// <summary>
    /// The GameManager class manages the main game loop and essential components for a game.
    /// </summary>
    public class GameManager
    {
        // Singleton pattern: Lazy initialization of a single instance of the GameManager class
        private static readonly Lazy<GameManager> _instance = new Lazy<GameManager>(() => new GameManager());

        /// <summary>
        /// Gets the singleton instance of the GameManager class.
        /// </summary>
        public static GameManager Instance => _instance.Value;

        #region Public Properties

        /// <summary>
        /// Gets the ContentManager responsible for loading and managing game assets.
        /// </summary>
        public ContentManager ContentManager { get; private set; }

        /// <summary>
        /// Gets the OpenGL context used for rendering.
        /// </summary>
        public GL Gl { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the input is captured (e.g., for controlling the game).
        /// </summary>
        public bool IsInputCaptured { get; private set; } = true;

        /// <summary>
        /// Gets the GameScreenManager responsible for managing different game screens or scenes.
        /// </summary>
        public GameScreenManager GameScreenManager { get; private set; }

        /// <summary>
        /// Gets the input context used for handling user input.
        /// </summary>
        public IInputContext Input { get; private set; }

        /// <summary>
        /// Gets the total memory usage of the application in megabytes (MB).
        /// </summary>
        public long MemoryUsage { get; private set; }

        /// <summary>
        /// Gets the main game window.
        /// </summary>
        public IWindow Window { get; private set; }

        /// <summary>
        /// Gets the logger used for logging application messages.
        /// </summary>
        public Logger Logger { get; private set; }

        #endregion Public Properties

        #region Private Properties

        private ImGuiController imguiController;
        private Type initialGameScreen;
        private float oneSecondTimer;

        #endregion Private Properties

        /// <summary>
        /// Runs the game with the specified parameters and sets up the main game loop.
        /// </summary>
        /// <param name="parameters">Parameters used to initialize the game instance.</param>
        /// <returns>The initialized GameManager instance.</returns>
        public GameManager Run(GameInstanceParameters parameters)
        {
            // Store the initial game screen that we should display when the game starts.
            this.initialGameScreen = parameters.InitialGameScreen;

            // Create a window with the specified options.
            var options = WindowOptions.Default with
            {
                API = new GraphicsAPI()
                {
                    Flags = ContextFlags.ForwardCompatible,
                    API = ContextAPI.OpenGL,
                    Profile = ContextProfile.Core,
                    Version = new APIVersion(4, 1)
                },
                Title = parameters.WindowTitle,
                Size = new Silk.NET.Maths.Vector2D<int>((int)parameters.InitialWindowSize.X, (int)parameters.InitialWindowSize.Y),
                FramesPerSecond = 0,

                VSync = false
            };

            // Create the window.
            this.Window = Silk.NET.Windowing.Window.Create(options);

            // Register event handlers for the window.
            Window.Render += onRender;
            Window.Update += onUpdate;
            Window.Load += onLoad;
            Window.Closing += Window_Closing;

            // Run the window.
            Window.Run();
            Window.Dispose();
            return this;
        }

        private void Window_Closing()
        {
            Dispose();
        }

        // Method called when the game is loaded, before the game loop starts.
        private void onLoad()
        {
            // Initialize ImGui controller for UI rendering.
            imguiController = new ImGuiController(
                Gl = Window.CreateOpenGL(), // Load OpenGL
                Window, // Pass in our window
                Input = Window.CreateInput() // Create an input context
            );
            // Initialize the logger with both file and console outputs.
            Logger = new Logger(LogOutput.Both);

            // Check if at least one keyboard is available for input.
            if (Input.Keyboards.Count < 1)
                throw new Exception("Cannot play without a keyboard. A keyboard is required for this game to function.");

            // Attach key down event handlers for all available keyboards.
            for (int i = 0; i < Input.Keyboards.Count; i++)
            {
                Input.Keyboards[i].KeyDown += onKeyDown;
            }

            // Initialize the ContentManager responsible for loading assets.
            ContentManager = new ContentManager();
            LoadEssentialAssets();

            // Initialize the GameScreenManager and set the initial game screen.
            GameScreenManager = new GameScreenManager(Gl);
            GameScreenManager.ChangeGameScreen(initialGameScreen);

            Gl.Enable(EnableCap.VertexArray);
        }

        // Method to load essential assets required for the game.
        private void LoadEssentialAssets()
        {
            ContentManager.GenerateNamedShader("material_basic", "Assets/material_shader/basic.vert", "Assets/material_shader/basic.frag");
            ContentManager.GenerateNamedShader("basic", "Assets/basic_shader/basic.vert", "Assets/basic_shader/basic.frag");
            ContentManager.GenerateNamedShader("material_advanced", "Assets/material_shader/advanced.vert", "Assets/material_shader/advanced.frag");

            ContentManager.GenerateNamedTexture("debug", "Assets/among.png");
            ContentManager.GenerateNamedTexture("gray", "Assets/gray.png");
            ContentManager.GenerateNamedTexture("white", "Assets/white.png");
        }

        // Event handler for key-down events.
        private void onKeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            // Toggle input capture when the Escape key is pressed.
            if (key == Key.Escape)
            {
                IsInputCaptured = !IsInputCaptured;
                for (int i = 0; i < Input.Mice.Count; i++)
                    Input.Mice[i].Cursor.CursorMode = IsInputCaptured ? CursorMode.Raw : CursorMode.Normal;
            }

            // Close the window when the Q key is pressed.
            if (key == Key.Q)
            {
                Window.Close();
            }
        }

        // Variables and method used for non-essential updates that run once per second.
        private void onUpdate(double delta)
        {
            oneSecondTimer += (float)delta;
            if (oneSecondTimer >= 1.0f)
            {
                oneSecondTimer = 0.0f;
                nonEssentialUpdate();
            }

            GameScreenManager.Update((float)delta);
        }

        // Update method for non-essential tasks, such as measuring memory usage.
        private void nonEssentialUpdate()
        {
            MemoryUsage = GC.GetTotalMemory(false) / 1000000;
        }

        // Method called when rendering a frame.
        private unsafe void onRender(double delta)
        {
            // Make sure ImGui is up-to-date before rendering.
            imguiController.Update((float)delta);

            // Clear the screen buffer before rendering the game screen.
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Render the current game screen.
            GameScreenManager.Render(Gl, (float)delta);

            // Render ImGui UI on top of the game screen.
            imguiController.Render();
        }

        private void Dispose()
        {
            ContentManager.Dispose();
            GameScreenManager.Dispose();

            Logger.Dispose();
        }
    }
}
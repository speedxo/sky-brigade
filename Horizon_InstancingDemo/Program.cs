using Horizon;
using Horizon.Rendering;

namespace Horizon_InstancingDemo
{
    internal class Program : Scene
    {
        static void Main(string[] args)
        {
            var assemName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            var version = assemName.Version;

            var engine = new BasicEngine(GameInstanceParameters.Default with
            {
                InitialGameScreen = typeof(Program),
                WindowTitle = $"{assemName.Name} ({version})"
            });
            engine.Run();
        }
        public override void Initialize()
        {
            base.Initialize();
            InitializeRenderingPipeline();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
        }
        public override void Dispose()
        {

        }

        public override void DrawGui(float dt)
        {

        }

        public override void DrawOther(float dt, ref RenderOptions options)
        {

        }
    }
}
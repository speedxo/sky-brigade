namespace SkyBrigade.Engine.Rendering
{
    public class EmptyMaterial : Material
    {
        public EmptyMaterial()
        {
            Shader = GameManager.Instance.ContentManager.GetShader("basic");
        }

        public override void Load(string path)
        {

        }

        public override void Save(string path)
        {

        }

        public override void Use()
        {
            Shader.Use();
        }
    }
}


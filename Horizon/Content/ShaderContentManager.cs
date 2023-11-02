namespace Horizon.Content
{
    /// <summary>
    /// The content manager dispatched to manage shaders.
    /// </summary>
    public class ShaderContentManager : GameContentManager<string, Shader>
    {
        public override string Name { get; set; } = "ShaderManager";

        public Shader AddFromDefinitions(params ShaderDefinition[] shaderDefinitions) =>
            Add(ShaderFactory.CompileFromDefinitions(shaderDefinitions));

        public Shader AddNamedFromDefinitions(
            string name,
            params ShaderDefinition[] shaderDefinitions
        ) => AddNamed(name, ShaderFactory.CompileFromDefinitions(shaderDefinitions));
    }
}

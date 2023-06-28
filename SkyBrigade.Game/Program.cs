using Silk.NET.OpenGL;
using SkyBrigade.Engine;
using SkyBrigade.Engine.Data;
using SkyBrigade.Engine.OpenGL;


using Shader = SkyBrigade.Engine.OpenGL.Shader;
using Texture = SkyBrigade.Engine.OpenGL.Texture;

namespace SkyBrigade.Game;

class DemoGameScreen : IGameScreen
{
    private Texture testTexture;
    private VertexBufferObject<Vertex> testVbo;
    private Shader testShader;
    private readonly Vertex[] testVertices = {
            new Vertex(-1, -1, 0, 0, 1),
            new Vertex(1, -1, 0, 1, 1),
            new Vertex(1, 1, 0, 1, 0),
            new Vertex(-1, 1, 0, 0, 0),
    };


    public void Initialize(GL gl)
    {
        gl.ClearColor(1.0f, 0.0f, 0.0f, 1.0f);

        // when using the content manager we dont need to worry about managing the object.
        testTexture = GameManager.Instance.ContentManager.GenerateNamedTexture("amongus", "Assets/among.png");

        testVbo = new VertexBufferObject<Vertex>(GameManager.Instance.Gl);
        testVbo.VertexBuffer.BufferData(testVertices);
        testVbo.ElementBuffer.BufferData(new uint[] {
                 0, 1, 3,
                 1, 2, 3
            });

        //Telling the VAO object how to lay out the attribute pointers
        testVbo.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, (uint)Vertex.SizeInBytes, 0);
        testVbo.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, (uint)Vertex.SizeInBytes, 3 * sizeof(float));
        testVbo.VertexAttributePointer(2, 2, VertexAttribPointerType.Float, (uint)Vertex.SizeInBytes, 6 * sizeof(float));

        testShader = GameManager.Instance.ContentManager.GenerateNamedShader("basic_shader", "Assets/shaders/basic/basic.vert", "Assets/shaders/basic/basic.frag");
    }

    public void Render(GL gl, float dt)
    {
        gl.Clear(ClearBufferMask.ColorBufferBit);
        gl.Viewport(0, 0, 1280, 720);

        testShader.Use();

        gl.ActiveTexture(TextureUnit.Texture0);
        gl.BindTexture(TextureTarget.Texture2D, testTexture.Handle);
        testShader.SetUniform("uTexture", 0);

        testVbo.Bind();

        // i really dont wanna make the whole method unsafe
        unsafe
        {
            gl.DrawElements(PrimitiveType.Triangles, (uint)6, DrawElementsType.UnsignedInt, null);
        }
        testVbo.Unbind();
        gl.UseProgram(0);
    }



    public void Update(float dt)
    {

    }

    public void Dispose()
    {
        testVbo.Dispose();
    }

}

class Program
{
    static void Main(string[] args)
    {
        GameManager.Instance.Run(GameInstanceParameters.Default with
        {
            InitialGameScreen = typeof(DemoGameScreen),
            WindowTitle = "vrek",
            InitialWindowSize = new System.Numerics.Vector2(1280, 720)
        });
    }
}


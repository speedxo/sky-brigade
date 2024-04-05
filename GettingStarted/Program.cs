using System.Numerics;

using Bogz.Logging;

using Horizon.Engine;
using Horizon.OpenGL;
using Horizon.OpenGL.Assets;
using Horizon.OpenGL.Buffers;
using Horizon.OpenGL.Descriptions;
using Horizon.Rendering.Primitives;

using Silk.NET.OpenGL;

using Texture = Horizon.OpenGL.Assets.Texture;

namespace GettingStarted;

class Program : Scene
{
    public override Camera ActiveCamera { get; protected set; }

    private VertexBufferObject vbo;
    private Technique technique;
    private Texture texture;

    private Matrix4x4 modelMatrix, viewMatrix;

    private Vector2 spritePos;
    private float spriteScale = 0.1f;

    private PrimitiveRenderer shapeRenderer;

    // Primitive shader class, implemented here to showcase extendability.
    private unsafe class ShaderTeq : Technique
    {
        public ShaderTeq()
        {
            SetShader(GameEngine
                .Instance
                .ObjectManager
                .Shaders
                .CreateOrGet(
                "basic",
                ShaderDescription.FromPath("shader", "basic")));
        }
    }
    public Program()
    {
        shapeRenderer = AddEntity<PrimitiveRenderer>();

        int size = 100;
        float scale = 0.01f;
        for (int x = 0; x < size / 2; x++)
        {
            for (int y = 0; y < size / 2; y++)
            {
                shapeRenderer.Shapes.Add(new ShapePrimitive((PrimitiveShapeType)((x + y * size / 2) % 3), new Vector2((x / (float)size) - (size * scale) + scale * x + 0.5f, (y / (float)size) - (size * scale) + scale * y + 0.5f), Vector2.One * scale, new Vector3(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle()), 0.0f));
            }
        }

        viewMatrix = Matrix4x4.CreateOrthographic(1.0f * (16.0f / 9.0f), 1.0f, 0.01f, 10.0f);
        shapeRenderer.ViewMatrix = viewMatrix;

    }
    public override void Initialize()
    {
        base.Initialize();

        // We need to enable some features which are done through flags, in this case we need Textures.
        Engine.GL.Enable(EnableCap.Texture2D);
        Engine.GL.Enable(EnableCap.Blend);
        Engine.GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        // Likewise we should enable a texture unit to bind our texture to.
        Engine.GL.ActiveTexture(TextureUnit.Texture0);

        /* In OpenGL it is required that you create one or more VBOs (vertex buffer object), then create and bind said buffers to a VAO (vertex array object),
          * a VBO is simply a buffer on the GPU side of which we can upload data to, and do whatever with. Horizon provides an exceptionless asset creation pipeline,
          * through the ObjectManager, while you can create the buffers manually using traditional gl calls, I have automated the process of creating the vao.
         */
        CreateVertexBufferObject();

        /* The next step is to populate our newly acquired buffers with data, for this we need to actually get the data, as well as specify how it is structured. */
        LayoutAndUploadData();

        /* And finally we create the shader :) */
        CreateShaderAndTexture();

        // here we specify the color to clear the screen to, since we will be drawing a fullscreen quad we shouldnt ever see this.
        Engine.GL.ClearColor(0.3f, 0.0f, 0.5f, 1.0f);
    }

    private unsafe void CreateShaderAndTexture()
    {
        /* The technique is just a decorated Shader object with more functionality, here we will use the less verbose inline definition. */
        // side note: to copy source files to the build directory, select them in visual studio and in the properties panel you will see an option for "Copy to output directory"
        technique = new ShaderTeq();

        texture = Engine.ObjectManager.Textures.CreateOrGet("texture", new TextureDescription { Path = "image.png" });

        /* You can F12 on the ShaderDescription.FromPath() to see that it simply is a shorthand for finding the path of all shaders in a folder with the same name, 
         * and creating a program from the shaders in said folder, this also enables my pre processor to use things such as #include :) */
    }

    private readonly struct Vertex(float x, float y, float z, float uvX, float uvY, float cR, float cG, float cB)
    {
        public readonly Vector3 Position { get; init; } = new Vector3(x, y, z);
        public readonly Vector2 TexCoords { get; init; } = new Vector2(uvX, uvY);
        public readonly Vector3 Colour { get; init; } = new Vector3(cR, cG, cB);
    }

    private unsafe void LayoutAndUploadData()
    {
        // Set the model matrix to be a basic scale.
        modelMatrix = Matrix4x4.Identity;

        // We create the 4 unique vertices required to render a square, the left side of the screen is -1 and the right is +1, same goes for the top and bottom.
        Vertex[] vertexData = [
            new Vertex( -1.0f, -1.0f, 0.0f,     0.0f, 1.0f,     0.1f, 0.1f, 1.0f),
            new Vertex( 1.0f, -1.0f, 0.0f,      1.0f, 1.0f,     0.5f, 0.6f, 0.1f),
            new Vertex( 1.0f, 1.0f, 0.0f,       1.0f, 0.0f,     0.2f, 0.4f, 0.8f),
            new Vertex( -1.0f, 1.0f, 0.0f,      0.0f, 0.0f,     0.9f, 0.2f, 0.5f),
        ];


        // as well as the 6 indices.
        uint[] indices = [0, 1, 2, 0, 2, 3];

        // next we upload the data, we can use modern DSA (direct state access) calls to skip having to bind and unbind a buffer to the state machine.
        vbo.VertexBuffer.NamedBufferData(new ReadOnlySpan<Vertex>(vertexData));
        vbo.ElementBuffer.NamedBufferData(new ReadOnlySpan<uint>(indices));

        /* next we need to tell the GPU how the data is layed out in memory, so it can be exposed to the shader correctly. Since data is passed into the shader
         * at binding points (called Attributes), we specify them. */

        // 1. Bind the VAO (this done when calling VBO.Bind() because a VBO is really just a more fancy decorated VAO)
        vbo.Bind();

        // 2. Bind the buffer you want to layout
        vbo.VertexBuffer.Bind();

        // 3. Enable and specify the layout of each vertex attribute
        vbo.VertexBuffer.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, (uint)sizeof(Vertex), 0);
        vbo.VertexBuffer.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, (uint)sizeof(Vertex), sizeof(float) * 3);
        vbo.VertexBuffer.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, (uint)sizeof(Vertex), sizeof(float) * 3);

        /* The index parameter specifies which shader binding point we want to bind to, it is standard to start from 0, the next specifies the size of the vector,
         * if it is 1, then we pass a single value, if it is say 2, we are passing a 2D vector, etc. in this case will be passing 3 floats for each vertex, so we specify that. 
         
         * We then specify the data type (float in this case) and the vertexSize is the total size (also called stride) in bytes of a complete vertex, which for us is simply 3 floats, and finally
         * offset is to access different attributes within the array.
         */

        vbo.ElementBuffer.Bind();

        // 4. Clean up the state machine
        vbo.Unbind();
    }

    private void CreateVertexBufferObject()
    {
        /* Since we are going to be drawing a triangle, we will need an array to store vertices, as well as indices, and here we hit the first of many OpenGL quirks,
         * while all gl buffers are the same object, due to OpenGL's archaic history being the first widely adopted graphics library, the binding points for an array-
         * -buffer (a general purpose buffer) and an element buffer (still an array buffer, just storing indices instead of vertices) are different, here we ask the
         * object manager to construct a VAO with an array buffer, and an element buffer, which are both actually just array buffers.
         */
        var result = Engine.ObjectManager.VertexArrays.Create(new VertexArrayObjectDescription
        {
            Buffers = new()
            {
                { VertexArrayBufferAttachmentType.ArrayBuffer, new BufferObjectDescription { Type = BufferTargetARB.ArrayBuffer } },
                { VertexArrayBufferAttachmentType.ElementBuffer, new BufferObjectDescription { Type = BufferTargetARB.ElementArrayBuffer } }
            }
        });

        /* The likelihood of any failures occurring are slim at best, however for the sake of the tutorial this demo will be relatively verbose. */
        switch (result.Status)
        {
            case Horizon.Content.AssetCreationStatus.Failed:
                Logger.Instance.Log(LogLevel.Error, result.Message);
                throw new Exception(result.Message);
            case Horizon.Content.AssetCreationStatus.Success:

                /* The VAO class is a 'primitive' asset, ie. it simply holds references to buffers stored elsewhere, to get anything done we simply need to create a new VBO-
                 * instance and inject the VAO in. 
                 */
                vbo = new VertexBufferObject(result.Asset);

                /* of course there is a more compact way to do this, i just wanted to be verbose, this can be done in one line:
                vbo = new VertexBufferObject(
                    Engine
                        .ObjectManager
                        .VertexArrays
                        .Create(
                            new VertexArrayObjectDescription
                            {
                                Buffers = new()
                                {
                                    {
                                        VertexArrayBufferAttachmentType.ArrayBuffer,
                                        BufferObjectDescription.ArrayBuffer
                                    },
                                    {
                                        VertexArrayBufferAttachmentType.ElementBuffer,
                                        BufferObjectDescription.ElementArrayBuffer
                                    }
                                }
                            }
                        )
                );
                */

                break;
        }
    }

    public override void UpdateState(float dt)
    {
        base.UpdateState(dt);
        // Update the sprite position by an offset from the input managers virtual controller, which aggregates a variety of input source types into a single virtual controller.
        spritePos += GameEngine.Instance.InputManager.GetVirtualController().MovementAxis * dt;

        for (int i = 0; i < shapeRenderer.Shapes.Count; i++)
        {
            shapeRenderer.Shapes[i] = shapeRenderer.Shapes[i] with
            {
                Rotation = shapeRenderer.Shapes[i].Rotation + Random.Shared.NextSingle() * dt * 100.0f
            };
        }
    }
    float timer = 0.0f;
    public override unsafe void Render(float dt, object? obj = null)
    {
        timer += dt;

        // first we clear the screen and set the viewport size.
        Engine.GL.Clear(ClearBufferMask.ColorBufferBit);
        Engine.GL.Viewport(0, 0, (uint)Engine.WindowManager.ViewportSize.X, (uint)Engine.WindowManager.ViewportSize.Y);

        shapeRenderer.Transform.Rotation += dt * 20.0f;
        shapeRenderer.Transform.Size = new Vector2(MathF.Sin(timer), MathF.Cos(timer));
        base.Render(dt, obj);

        // ensure we update our matrix.
        modelMatrix = viewMatrix * Matrix4x4.CreateScale(spriteScale) * Matrix4x4.CreateTranslation(spritePos.X, spritePos.Y, 0.0f);

        // next we bind the shader, the currently bound program will be used to draw anything.
        technique.Bind();

        // Make sure to upload our matrix.
        technique.SetUniform("uModelMatrix", modelMatrix);

        // Binding the texture to the unit we enabled and setting the shader uniform.
        texture.Bind(0);
        technique.SetUniform("uTexture", 0);

        // next we bind the vertex array object (in this case the VBO)
        vbo.Bind();

        /* since OpenGL draw calls let you specify your indices as a parameter (a pointer specifically) we will need to set a null pointer to tell OpenGL
         * to use the element buffer bound to the vbo, this unfortunatly requires an unsafe context. */
        unsafe
        {
            // Here we call glDrawElements, this call requires us to specify the primitive type, the index type (we created a uint[]) and how many elements we want to draw (we will draw all 6)
            Engine.GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, null);
        }

        // finally we clean up the state machine
        vbo.Unbind();
        technique.Unbind();
    }

    static void Main(string[] args)
    {
        new GameEngine(
            GameEngineConfiguration.Default with
            {
                InitialScene = typeof(Program)
            }
        ).Run();
    }
}

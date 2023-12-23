using Bogz.Logging;

using Horizon.Engine;
using Horizon.OpenGL;
using Horizon.OpenGL.Assets;
using Horizon.OpenGL.Buffers;
using Horizon.OpenGL.Descriptions;

using Silk.NET.OpenGL;

namespace GettingStarted;

class Program : Scene
{
    public override Camera ActiveCamera { get; protected set; }

    private VertexBufferObject vbo;
    private Technique technique;

    public override void Initialize()
    {
        base.Initialize();

        /* In OpenGL it is required that you create one or more VBOs (vertex buffer object), then create and bind said buffers to a VAO (vertex array object),
          * a VBO is simply a buffer on the GPU side of which we can upload data to, and do whatever with. Horizon provides an exceptionless asset creation pipeline,
          * through the ObjectManager, while you can create the buffers manually using traditional gl calls, I have automated the process of creating the vao.
         */
        CreateVertexBufferObject();

        /* The next step is to populate our newly acquired buffers with data, for this we need to actually get the data, as well as specify how it is structured. */
        LayoutAndUploadData();

        /* And finally we create the shader :) */
        CreateShader();

        // here we specify the color to clear the screen to, since we will be drawing a fullscreen quad we shouldnt ever see this.
        Engine.GL.ClearColor(0.3f, 0.0f, 0.5f, 1.0f);
    }

    private void CreateShader()
    {
        /* The technique is just a decorated Shader object with more functionality, here we will use the less verbose inline definition. */

        // side note: to copy source files to the build directory, select them in visual studio and in the properties panel you will see an option for "Copy to output directory"
        technique = new Technique(Engine.ObjectManager.Shaders.Create(ShaderDescription.FromPath("shader", "basic")));

        /* You can F12 on the ShaderDescription.FromPath() to see that it simply is a shorthand for finding the path of all shaders in a folder with the same name, 
         * and creating a program from the shaders in said folder, this also enables my pre processor to use things such as #include :) */
    }
    private void LayoutAndUploadData()
    {
        // We create the 4 unique vertices required to render a square, the left side of the screen is -1 and the right is +1, same goes for the top and bottom.
        float[] vertexData = new float[] {
            -1.0f, -1.0f, 0.0f,
            1.0f, -1.0f, 0.0f,
            1.0f, 1.0f, 0.0f,
            -1.0f, 1.0f, 0.0f,
        };

        // as well as the 6 indices.
        uint[] indices = new uint[] { 0, 1, 2, 0, 2, 3 };

        // next we upload the data, we can use modern DSA (direct state access) calls to skip having to bind and unbind a buffer to the state machine.
        vbo.VertexBuffer.NamedBufferData(new ReadOnlySpan<float>(vertexData));
        vbo.ElementBuffer.NamedBufferData(new ReadOnlySpan<uint>(indices));

        /* next we need to tell the GPU how the data is layed out in memory, so it can be exposed to the shader correctly. Since data is passed into the shader
         * at binding points (called Attributes), we specify them. */

        // 1. Bind the VAO (this done when calling VBO.Bind() because a VBO is really just a more fancy decorated VAO)
        vbo.Bind();

        // 2. Bind the buffer you want to layout
        vbo.VertexBuffer.Bind();

        // 3. Enable and specify the layout of each vertex attribute
        vbo.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, sizeof(float) * 3, 0);

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

    public override void Render(float dt, object? obj = null)
    {
        base.Render(dt, obj);

        // first we clear the screen and set the viewport size.
        Engine.GL.Clear(ClearBufferMask.ColorBufferBit);
        Engine.GL.Viewport(0, 0, (uint)Engine.WindowManager.ViewportSize.X, (uint)Engine.WindowManager.ViewportSize.Y);

        // next we bind the shader, the currently bound program will be used to draw anything.
        technique.Bind();

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

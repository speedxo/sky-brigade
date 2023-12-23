namespace TileBash;



//public class NoiseGenerator
//{
//    private RenderRectangle renderRect;
//    private Technique technique;

//    public uint TextureHandle
//    {
//        get => renderRect.frameBuffer.Attachments[FramebufferAttachment.ColorAttachment0];
//    }

//    public NoiseGenerator(int width, int height, Vector2 scale)
//    {
//        renderRect = new RenderRectangle(
//            technique = new Technique(
//                Entity.Engine.ObjectManager.Shaders.A(
//                    "content/noise/noise.vert",
//                    "content/noise/noise.frag"
//                )
//            ),
//            width,
//            height
//        );
//        renderRect.frameBuffer.AddAttachment(FramebufferAttachment.ColorAttachment0);
//        System.Diagnostics.Debug.Assert(renderRect.frameBuffer.ContructFrameBuffer());

//        renderRect.frameBuffer.Bind();
//        Entity.Engine.GL.Clear(ClearBufferMask.ColorBufferBit);
//        renderRect.frameBuffer.Unbind();

//        technique.Use();
//        technique.SetUniform("uNoiseScale", scale);

//        renderRect.RenderScene(1.0f);

//        Entity.Engine.GL.BindTexture(
//            TextureTarget.Texture2D,
//            renderRect.frameBuffer.Attachments[FramebufferAttachment.ColorAttachment0]
//        );

//        // Allocate array for pixel data
//        byte[] pixelData = new byte[width * height * 4]; // RGBA format, assuming 8 bits per channel

//        unsafe
//        {
//            fixed (void* data = pixelData)
//            {
//                // Get the pixel data from the texture
//                Entity.Engine.GL.GetTexImage(
//                    GLEnum.Texture2D,
//                    0,
//                    GLEnum.Rgba,
//                    GLEnum.UnsignedByte,
//                    data
//                );
//            }
//        }
//        Entity.Engine.GL.BindTexture(TextureTarget.Texture2D, 0);
//        //renderRect.frameBuffer.Dispose();

//        ProcessData(pixelData, width, height);

//        //technique.Dispose();
//    }

//    public float[,] Data { get; private set; }

//    private void ProcessData(byte[] pixelData, int width, int height)
//    {
//        Data = new float[width, height];

//        for (int y = 0; y < height; y++)
//        {
//            for (int x = 0; x < width; x++)
//            {
//                int pixelIndex = (y * width + x) * 4; // RGBA format
//                byte r = pixelData[pixelIndex];

//                Data[x, y] = r / 255.0f;
//            }
//        }
//    }
//}

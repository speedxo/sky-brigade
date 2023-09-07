namespace Horizon.OpenGL;

public static class FrameBufferManager
{
    private static List<FrameBufferObject> frameBuffers = new();

    public static FrameBufferObject CreateFrameBuffer(int width, int height)
    {
        var fbo = new FrameBufferObject(width, height);
        frameBuffers.Add(fbo);
        return fbo;
    }

    public static void ResizeAll(int width, int height)
    {
        foreach (var item in frameBuffers)
        {
            item.Resize(width, height);
        }
    }
}

namespace SkyBrigade.Engine.Collections;

/* The reason this implementation is so primative and sloppy but i need it to
 * be fast
 */

public class CircularBuffer<T>
{
    public T[] Buffer;
    private int startIndex;
    private int endIndex;

    public int Length { get; private set; }

    public CircularBuffer(int capacity)
    {
        Buffer = new T[capacity];
        startIndex = 0;
        endIndex = -1;
        Length = 0;
    }

    public void Append(T value)
    {
        if (Length == Buffer.Length)
        {
            startIndex = (startIndex + 1) % Buffer.Length;
        }

        endIndex = (endIndex + 1) % Buffer.Length;
        Buffer[endIndex] = value;

        if (Length < Buffer.Length)
        {
            Length++;
        }
    }

    public T[] ToArray()
    {
        T[] result = new T[Length];

        if (Length > 0)
        {
            if (startIndex <= endIndex)
            {
                Array.Copy(Buffer, startIndex, result, 0, Length);
            }
            else
            {
                Array.Copy(Buffer, startIndex, result, 0, Buffer.Length - startIndex);
                Array.Copy(Buffer, 0, result, Buffer.Length - startIndex, endIndex + 1);
            }
        }

        return result;
    }
}
namespace SkyBrigade.Engine.Collections;

/* The reason this implementation is so primative and sloppy but i need it to
 * be fast
 */

public class CircularBuffer<T>
{
    private T[] buffer;
    private int startIndex;

    public int Index { get; private set; }

    public int Length { get; private set; }

    public CircularBuffer(int capacity)
    {
        buffer = new T[capacity];
        startIndex = 0;
        Index = -1;
        Length = 0;
    }

    public void Append(T value)
    {
        if (Length == buffer.Length)
        {
            startIndex = (startIndex + 1) % Buffer.Length;
        }

        Index = (Index + 1) % buffer.Length;
        buffer[Index] = value;

        if (Length < buffer.Length)
        {
            Length++;
        }
    }

    public T[] Buffer { get => buffer; }

    public T[] ToArray()
    {
        T[] result = new T[Length];

        if (Length > 0)
        {
            if (startIndex <= Index)
            {
                Array.Copy(buffer, startIndex, result, 0, Length);
            }
            else
            {
                Array.Copy(buffer, startIndex, result, 0, buffer.Length - startIndex);
                Array.Copy(buffer, 0, result, buffer.Length - startIndex, Index + 1);
            }
        }

        return result;
    }
}
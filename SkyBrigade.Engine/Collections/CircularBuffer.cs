using System;
namespace SkyBrigade.Engine.Collections;

/* The reason this implementation is so primative and sloppy but i need it to
 * be fast
 */
public class CircularBuffer<T>
{
    private T[] buffer;
    private int startIndex;
    private int endIndex;

    public int Length { get; private set; }

    public CircularBuffer(int capacity)
    {
        buffer = new T[capacity];
        startIndex = 0;
        endIndex = -1;
        Length = 0;
    }

    public void Append(T value)
    {
        if (Length == buffer.Length)
        {
            startIndex = (startIndex + 1) % buffer.Length;
        }

        endIndex = (endIndex + 1) % buffer.Length;
        buffer[endIndex] = value;

        if (Length < buffer.Length)
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
                Array.Copy(buffer, startIndex, result, 0, Length);
            }
            else
            {
                Array.Copy(buffer, startIndex, result, 0, buffer.Length - startIndex);
                Array.Copy(buffer, 0, result, buffer.Length - startIndex, endIndex + 1);
            }
        }

        return result;
    }
}


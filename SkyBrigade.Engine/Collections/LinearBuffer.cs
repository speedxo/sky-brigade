namespace Horizon.Collections;

/* The reason this implementation is so primative and sloppy but i need it to
 * be fast
 */

public class LinearBuffer<T>
{
    private T[] buffer;
    public int Index { get; private set; }

    public int Length { get; private set; }
    
    public LinearBuffer(int capacity)
    {
        buffer = new T[capacity];
        Index = 0;
        Length = 0;
    }

    public void Append(T value)
    {
        Index = (Index + 1) % buffer.Length;

        buffer[Index] = value;
        if (Length < buffer.Length)
            Length++;
    }

    public T[] Buffer { get => buffer; }
}
using Horizon.Collections;

namespace Horizon.Data;

public class DeltaTracker<T>
{
    private T previousState;
    private Func<T, T, float> calculateDeltaFunc;
    private float delta = 0.0f;
    private CircularBuffer<float> averageBuffer;

    public DeltaTracker(Func<T, T, float> calculateDeltaFunc)
    {
        this.calculateDeltaFunc = calculateDeltaFunc;
        averageBuffer = new CircularBuffer<float>(100);
    }

    // Get the delta value
    public float GetDelta() => delta;

    // forward the buffer
    public float[] Buffer => averageBuffer.Buffer;

    // Get the average value
    public float GetAverage() => averageBuffer.Buffer.Average();

    public void Update(T currentState)
    {
        // The first time this loop runs, previous state will be null
        // so we need to make sure it isnt null.
        previousState ??= currentState;

        delta = calculateDeltaFunc(previousState, currentState);

        averageBuffer.Append(delta);
        previousState = currentState;
    }
}
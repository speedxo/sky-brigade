namespace Horizon;

public static class MathHelper
{
    [System.Diagnostics.Contracts.Pure]
    public static float DegreesToRadians(float degrees)
    {
        return MathF.PI / 180f * degrees;
    }

    [System.Diagnostics.Contracts.Pure]
    public static float RadiansToDegrees(float degrees)
    {
        return degrees * (180f / MathF.PI);
    }
}

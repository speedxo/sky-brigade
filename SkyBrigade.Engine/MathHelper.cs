namespace SkyBrigade.Engine;

public static class MathHelper
{
    [System.Diagnostics.Contracts.Pure]
    public static float DegreesToRadians(float degrees)
    {
        return MathF.PI / 180f * degrees;
    }
}


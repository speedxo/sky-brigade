float exposure(float x)
{
    float exposure = 1.;
    return 1.0 - exp(-x * exposure);
}
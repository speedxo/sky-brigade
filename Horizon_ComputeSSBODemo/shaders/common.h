// PCG hash, see:
// https://www.reedbeta.com/blog/hash-functions-for-gpu-rendering/

// Used as initial seed to the PRNG.
uint pcg_hash(uint seed)
{
  uint state = seed * 747796405u + 2891336453u;
  uint word = ((state >> ((state >> 28u) + 4u)) ^ state) * 277803737u;
  return (word >> 22u) ^ word;
}

// Used to advance the PCG state.
uint rand_pcg(inout uint rng_state)
{
  uint state = rng_state;
  rng_state = rng_state * 747796405u + 2891336453u;
  uint word = ((state >> ((state >> 28u) + 4u)) ^ state) * 277803737u;
  return (word >> 22u) ^ word;
}

// Advances the prng state and returns the corresponding random float.
float rand(inout uint state)
{
  uint x = rand_pcg(state);
  state = x;
  return float(x)*uintBitsToFloat(0x2f800004u);
}
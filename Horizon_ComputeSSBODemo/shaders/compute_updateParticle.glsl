#version 460 core
layout(local_size_x=1024, local_size_y=1) in;

#include "common.h"
#include "particle.h"

uniform float uDt;

layout(std430, binding=0) coherent restrict buffer particle_buffer
{
    Particle particles[];
} Buffer;

layout(std430, binding=1) coherent restrict buffer particle_index_buffer
{
  int count;
  int[] indices;
} IndexBuffer;

layout(location = 0) uniform int uParticlesToSpawn;

void SimulateParticle(inout Particle particle, int index)
{
  particle.age += uDt;
  if (particle.age > 60.0)
  {
    particle.color.a = 0.0; // make the particle invisible
    IndexBuffer.indices[atomicAdd(IndexBuffer.count, 1)] = index;
    return;
  }

  uint seed = pcg_hash(index);
  particle.pos += (particle.dir + (rand(seed) * 0.4)) * uDt * rand(seed) * 5.0;
}

void main()
{
    int index = int(gl_GlobalInvocationID);
    if (index > Buffer.particles.length())
      return;
    SimulateParticle(Buffer.particles[index], index);
}
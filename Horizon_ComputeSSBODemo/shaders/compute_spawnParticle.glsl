#version 460 core
layout(local_size_x=1024, local_size_y=1) in;

#include "common.h"
#include "particle.h"

#define PI 3.1415

uniform float uDt;
uniform vec2 uSpawnPosition;

layout(std430, binding=0) writeonly restrict buffer particle_buffer {
    Particle particles[];
} Buffer;

layout(std430, binding=1) coherent restrict buffer particle_index_buffer {
  int count;
  int[] indices;
} IndexBuffer;

layout(location = 0) uniform int uParticlesToSpawn;

float randF(vec2 co){
    return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453);
}

vec2 randomDir(float noise)
{
  return vec2(cos(noise * PI * 2.0), sin(noise * PI * 2.0));
}

void MakeParticle(in uvec2 ind, out Particle particle) {
  uint seed = ind.x;
  seed = pcg_hash(seed);
  
  vec2 pos = uSpawnPosition;
  vec2 dir = randomDir(rand(seed));

  particle.pos = pos;
  particle.dir = dir;
  particle.color = vec4(rand(seed), rand(seed), rand(seed), 1.0);
}

void main()
{
  int index = int(gl_GlobalInvocationID);

  if (index >= uParticlesToSpawn)
    return;

  // undo decrement and return if we are maxed.
  int freeIndex = atomicAdd(IndexBuffer.count, -1) - 1;
  if (freeIndex < 0) {
    atomicAdd(IndexBuffer.count, 1);
    return;
  }
  int particleIndex = IndexBuffer.indices[freeIndex];
  MakeParticle(gl_GlobalInvocationID.xy, Buffer.particles[freeIndex]);
}
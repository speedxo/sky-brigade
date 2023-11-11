#version 460 core

layout(location = 0) in vec2 vPos;

uniform mat4 uView;
uniform mat4 uModel;
uniform mat4 uProjection;

struct Particle
{
    vec2 pos;   
    vec2 dir;
    vec4 color;
    float age;
    //vec3 _space;
};

layout(std430, binding=0) readonly restrict buffer particle_buffer
{
    Particle particles[];
} Buffer;

out vec4 partColor;

void main() {
  //if (Buffer.particles[gl_InstanceID].color.a == 0.0)
  //  return;

  partColor = Buffer.particles[gl_InstanceID].color;
  gl_Position = uProjection * uView * uModel * vec4(vPos + Buffer.particles[gl_InstanceID].pos, 0.0, 1.0);
}
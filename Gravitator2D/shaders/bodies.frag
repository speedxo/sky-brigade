#version 410

in vec2 fragTexCoord;
uniform int uBodyCount;
#define MAX_POINTS 16

layout(std140) struct Body {
  vec2 Position;
  float Radius;
  vec3 Color;
  int _spacer0;
  int _spacer1;
};

layout(std140) uniform BodyBuffer { Body bodies[MAX_POINTS]; };

out vec4 oFragColor;

void main() {
  vec3 fragColor = vec3(0.0);

  for (int i = 0; i < uBodyCount; i++) {
    vec2 delta = fragTexCoord - bodies[i].Position;
    float distSq = dot(delta, delta);

    if (distSq <= bodies[i].Radius * bodies[i].Radius) {
      fragColor = bodies[i].Color;
      break;
    }
  }

  oFragColor = vec4(fragColor, 1.0f);
}

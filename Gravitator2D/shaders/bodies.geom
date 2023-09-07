#version 410

layout(points) in;
layout(triangle_strip, max_vertices = 64) out;

in vec2 fragTexCoord[];

out vec2 geomTexCoord;

uniform int uPointCount = 2;
#define MAX_POINTS 64
layout(std140) uniform uniformPoints { vec2 points[MAX_POINTS]; };

void main() {
  for (int i = 0; i < uPointCount; ++i) {
    vec2 center = points[i];
    float radius = 0.05; // Adjust the circle radius as needed

    for (float angle = 0.0; angle < 6.2831853;
         angle += 0.1) { // Approximately 2 * pi
      vec2 offset = vec2(cos(angle), sin(angle)) * radius;
      gl_Position = vec4(center + offset, 0.0, 1.0);
      geomTexCoord = fragTexCoord[0];
      EmitVertex();
    }
    EndPrimitive();
  }
}

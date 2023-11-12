#version 410 core

in float alive;
in float age;

out vec4 FragColor;

uniform vec3 uStartColor;
uniform vec3 uEndColor;

void main() {
  if (alive < 0.0) discard; 
  FragColor = vec4(mix(uEndColor, uStartColor, alive), alive + 0.1);
}
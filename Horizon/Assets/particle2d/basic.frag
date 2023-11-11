#version 410 core

in float alive;
out vec4 FragColor;

void main() {
  if (alive < 0.5) discard; 
  FragColor = vec4(1.0f);
}
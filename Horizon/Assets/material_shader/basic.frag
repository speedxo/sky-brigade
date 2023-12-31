#version 410 core

uniform sampler2D uTexture;
uniform vec4 uColor;

out vec4 FragColor;

in vec3 fNorm;
in vec2 fTexCoords;
in vec3 fragPos;

// physical parameters
uniform vec3 camPos;
uniform float uGamma = 2.2f;
uniform float uAmbientStrength = 0.03f;

// material parameters
uniform float uMetallicness;
uniform float uRoughness;
uniform float uAo;

uniform int uDebug_defferedRenderLayer = 0;

// lights
uniform vec3 lightPositions[4];
uniform vec3 lightColors[4];

const float PI = 3.14159265359;
// ----------------------------------------------------------------------------
float DistributionGGX(vec3 N, vec3 H, float roughness) {
  float a = roughness * roughness;
  float a2 = a * a;
  float NdotH = max(dot(N, H), 0.0);
  float NdotH2 = NdotH * NdotH;

  float nom = a2;
  float denom = (NdotH2 * (a2 - 1.0) + 1.0);
  denom = PI * denom * denom;

  return nom / denom;
}
// ----------------------------------------------------------------------------
float GeometrySchlickGGX(float NdotV, float roughness) {
  float r = (roughness + 1.0);
  float k = (r * r) / 8.0;

  float nom = NdotV;
  float denom = NdotV * (1.0 - k) + k;

  return nom / denom;
}
// ----------------------------------------------------------------------------
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness) {
  float NdotV = max(dot(N, V), 0.0);
  float NdotL = max(dot(N, L), 0.0);
  float ggx2 = GeometrySchlickGGX(NdotV, roughness);
  float ggx1 = GeometrySchlickGGX(NdotL, roughness);

  return ggx1 * ggx2;
}
// ----------------------------------------------------------------------------
vec3 fresnelSchlick(float cosTheta, vec3 F0) {
  return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}
// ----------------------------------------------------------------------------

/*
    None = 0,

    Albedo = 1,
    Normal = 2,
    Metallicness = 3,
    Roughness = 4,
    AmbientOcclusion = 5
*/

// adapted for 1D
vec4 renderDefferedLayer() {
  switch (uDebug_defferedRenderLayer) {
  case 1:
    return texture(uTexture, fTexCoords);
  case 3:
    return vec4(vec3(uMetallicness), 1.0f);
  case 4:
    return vec4(vec3(uRoughness), 1.0f);
  case 5:
    return vec4(vec3(uAo), 1.0f);
  default:
    return vec4(1.0);
  }
}

void main() {
  if (uDebug_defferedRenderLayer > 0) {
    FragColor = renderDefferedLayer();
    return;
  }

  vec3 col = texture(uTexture, fTexCoords).rgb;

  vec3 N = normalize(fNorm);
  vec3 V = normalize(camPos - fragPos);

  // calculate reflectance at normal incidence; if dia-electric (like plastic)
  // use F0 of 0.04 and if it's a metal, use the albedo color as F0 (metallic
  // workflow)
  vec3 F0 = vec3(0.04);
  F0 = mix(F0, col, uMetallicness);

  // reflectance equation
  vec3 Lo = vec3(0.0);
  for (int i = 0; i < 4; ++i) {
    // calculate per-light radiance
    vec3 L = normalize(lightPositions[i] - fragPos);
    vec3 H = normalize(V + L);
    float distance = length(lightPositions[i] - fragPos);
    float attenuation = 1.0 / (distance * distance);
    vec3 radiance = lightColors[i] * attenuation;

    // Cook-Torrance BRDF
    float NDF = DistributionGGX(N, H, uRoughness);
    float G = GeometrySmith(N, V, L, uRoughness);
    vec3 F = fresnelSchlick(clamp(dot(H, V), 0.0, 1.0), F0);

    vec3 numerator = NDF * G * F;
    float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) +
                        0.0001; // + 0.0001 to prevent divide by zero
    vec3 specular = numerator / denominator;

    // kS is equal to Fresnel
    vec3 kS = F;
    // for energy conservation, the diffuse and specular light can't
    // be above 1.0 (unless the surface emits light); to preserve this
    // relationship the diffuse component (kD) should equal 1.0 - kS.
    vec3 kD = vec3(1.0) - kS;
    // multiply kD by the inverse metalness such that only non-metals
    // have diffuse lighting, or a linear blend if partly metal (pure metals
    // have no diffuse light).
    kD *= 1.0 - uMetallicness;

    // scale light by NdotL
    float NdotL = max(dot(N, L), 0.0);

    // add to outgoing radiance Lo
    Lo += (kD * col.rgb / PI + specular) * radiance *
          NdotL; // note that we already multiplied the BRDF by the Fresnel (kS)
                 // so we won't multiply by kS again
  }

  // ambient lighting (note that the next IBL tutorial will replace
  // this ambient lighting with environment lighting).
  vec3 ambient = vec3(uAmbientStrength) * col.rgb * uAo;

  vec3 color = ambient + Lo;

  // HDR tonemapping
  color = color / (color + vec3(1.0));
  // gamma correct
  color = pow(color, vec3(1.0 / uGamma));

  FragColor = vec4(color, 1.0);
}
#version 410 core

uniform sampler2D uTexture;

out vec4 FragColor;

in vec3 fNorm;
in vec2 fTexCoords;
in vec3 fragPos;

// physical parameters
uniform vec3 camPos;

uniform bool uWireframeEnabled;

layout(std140) uniform MaterialRenderOptions {
  int DefferedRenderLayer;
  float Gamma;
  float AmbientStrength;
  vec4 Color;
};

// material parameters
uniform sampler2D uAlbedo;
uniform sampler2D uRoughness;
uniform sampler2D uMetallicness;
uniform sampler2D uAo;
uniform sampler2D uNormals;

// lights
uniform vec3 lightPositions[4];
uniform vec3 lightColors[4];

const float PI = 3.14159265359;

// ----------------------------------------------------------------------------
// Easy trick to get tangent-normals to world-space to keep PBR code simplified.
// Don't worry if you don't get what's going on; you generally want to do normal
// mapping the usual way for performance anyways;
vec3 getNormalFromMap() {
  vec3 tangentNormal = texture(uNormals, fTexCoords).xyz * 2.0 - 1.0;

  vec3 Q1 = dFdx(fragPos);
  vec3 Q2 = dFdy(fragPos);
  vec2 st1 = dFdx(fTexCoords);
  vec2 st2 = dFdy(fTexCoords);

  vec3 N = normalize(fNorm);
  vec3 T = normalize(Q1 * st2.t - Q2 * st1.t);
  vec3 B = -normalize(cross(N, T));
  mat3 TBN = mat3(T, B, N);

  return normalize(tangentNormal * TBN);
}

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

vec4 renderDefferedLayer() {
  switch (DefferedRenderLayer) {
  case 1:
    return texture(uAlbedo, fTexCoords);
  case 2:
    return vec4(getNormalFromMap(), 1.0f);
  case 3:
    return texture(uMetallicness, fTexCoords);
  case 4:
    return texture(uRoughness, fTexCoords);
  case 5:
    return texture(uAo, fTexCoords);
  default:
    return vec4(1.0);
  }
}

void main() {
  if (uWireframeEnabled) {
    FragColor = vec4(1.0f);
    return;
  }
  if (DefferedRenderLayer > 0) {
    FragColor = renderDefferedLayer();
    return;
  }

  // gamma correct input
  vec3 col = pow(texture(uAlbedo, fTexCoords).rgb, vec3(Gamma));

  vec3 N = getNormalFromMap();
  vec3 V = normalize(camPos - fragPos);

  // calculate reflectance at normal incidence; if dia-electric (like plastic)
  // use F0 of 0.04 and if it's a metal, use the albedo color as F0 (metallic
  // workflow)
  vec3 F0 = vec3(0.04);
  F0 = mix(F0, col, texture(uMetallicness, fTexCoords).r);

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
    float NDF = DistributionGGX(N, H, texture(uRoughness, fTexCoords).r);
    float G = GeometrySmith(N, V, L, texture(uRoughness, fTexCoords).r);
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
    kD *= 1.0 - texture(uMetallicness, fTexCoords).r;

    // scale light by NdotL
    float NdotL = max(dot(N, L), 0.0);

    // add to outgoing radiance Lo
    Lo += (kD * col.rgb / PI + specular) * radiance *
          NdotL; // note that we already multiplied the BRDF by the Fresnel (kS)
                 // so we won't multiply by kS again
  }

  // ambient lighting (note that the next IBL tutorial will replace
  // this ambient lighting with environment lighting).
  vec3 ambient = vec3(AmbientStrength) * col.rgb * texture(uAo, fTexCoords).r;

  vec3 color = ambient + Lo;

  // HDR tonemapping
  color = color / (color + vec3(1.0));
  // gamma correct
  color = pow(color, vec3(1.0 / Gamma));

  FragColor = vec4(color, 1.0);
}
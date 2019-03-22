﻿#version 330

uniform mat4 Model;
uniform mat4 ViewProj;
uniform mat4 Transform;

uniform vec3[5] LightPosition;
uniform vec3 EyePosition;

uniform float TexScaler;

in vec3 a_Position;
in vec2 a_TexCoord;
in vec3 a_Normal;

out vec2 v_TexCoord;
out vec3 Normal;

out vec3 FragPos;
out vec3[5] posLight;
out vec3 posEye;

void main()
{
    gl_Position = ViewProj * (Model * Transform) * vec4(a_Position, 1);

	FragPos = vec3((Model * Transform) * vec4(a_Position, 1));
	posLight = LightPosition;
	posEye = EyePosition;

	v_TexCoord = a_TexCoord * vec2(TexScaler, TexScaler);
	Normal = mat3(Transform) * a_Normal;
}
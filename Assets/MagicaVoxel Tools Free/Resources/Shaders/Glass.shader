// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X
Shader "MagicaVoxel/Glass"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_Tint("Tint", Color) = (1,1,1,1)
		_Opacity("Opacity", Range(0 , 1)) = 0
		_Smoothness("Smoothness", Range(0 , 1)) = 0.8
		[HDR]_Glow("Glow", Color) = (0,0,0,0)
		_Normal("Normal", 2D) = "white" {}
		_NormalScale("Normal Scale", Range(0 , 1)) = 0.1
		[Header(Refraction)]
		_IOR("IOR", Range(0 , 5)) = 1.2
		_ChromaticAberration("Chromatic Aberration", Range(0 , 0.3)) = 0.1
		[HideInInspector] _texcoord("", 2D) = "white" {}
		[HideInInspector] __dirty("", Int) = 1
	}

		SubShader
		{
			Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
			Cull Back
			GrabPass{ }
			CGINCLUDE
			#include "UnityStandardUtils.cginc"
			#include "UnityPBSLighting.cginc"
			#include "Lighting.cginc"
			#pragma target 3.0
			#pragma multi_compile _ALPHAPREMULTIPLY_ON
			struct Input
			{
				float2 uv_texcoord;
				float4 screenPos;
				float3 worldPos;
			};

			uniform float _NormalScale;
			uniform sampler2D _Normal;
			uniform float4 _Normal_ST;
			uniform sampler2D _Albedo;
			uniform float4 _Albedo_ST;
			uniform float4 _Tint;
			uniform float4 _Glow;
			uniform float _Smoothness;
			uniform float _Opacity;
			uniform sampler2D _GrabTexture;
			uniform float _ChromaticAberration;
			uniform float _IOR;

			inline float4 Refraction(Input i, SurfaceOutputStandard o, float indexOfRefraction, float chomaticAberration) {
				float3 worldNormal = o.Normal;
				float4 screenPos = i.screenPos;
				#if UNITY_UV_STARTS_AT_TOP
					float scale = -1.0;
				#else
					float scale = 1.0;
				#endif
				float halfPosW = screenPos.w * 0.5;
				screenPos.y = (screenPos.y - halfPosW) * _ProjectionParams.x * scale + halfPosW;
				#if SHADER_API_D3D9 || SHADER_API_D3D11
					screenPos.w += 0.00000000001;
				#endif
				float2 projScreenPos = (screenPos / screenPos.w).xy;
				float3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
				float3 refractionOffset = ((((indexOfRefraction - 1.0) * mul(UNITY_MATRIX_V, float4(worldNormal, 0.0))) * (1.0 / (screenPos.z + 1.0))) * (1.0 - dot(worldNormal, worldViewDir)));
				float2 cameraRefraction = float2(refractionOffset.x, -(refractionOffset.y * _ProjectionParams.x));
				float4 redAlpha = tex2D(_GrabTexture, (projScreenPos + cameraRefraction));
				float green = tex2D(_GrabTexture, (projScreenPos + (cameraRefraction * (1.0 - chomaticAberration)))).g;
				float blue = tex2D(_GrabTexture, (projScreenPos + (cameraRefraction * (1.0 + chomaticAberration)))).b;
				return float4(redAlpha.r, green, blue, redAlpha.a);
			}

			void RefractionF(Input i, SurfaceOutputStandard o, inout half4 color)
			{
				#ifdef UNITY_PASS_FORWARDBASE
				color.rgb = color.rgb + Refraction(i, o, _IOR, _ChromaticAberration) * (1 - color.a);
				color.a = 1;
				#endif
			}

			void surf(Input i , inout SurfaceOutputStandard o)
			{
				o.Normal = float3(0,0,1);
				float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
				o.Normal = UnpackScaleNormal(tex2D(_Normal, uv_Normal), _NormalScale);
				float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
				o.Albedo = (tex2D(_Albedo, uv_Albedo) * _Tint).rgb;
				o.Emission = _Glow.rgb;
				o.Smoothness = _Smoothness;
				o.Alpha = _Opacity;
				o.Normal = o.Normal + 0.00001 * i.screenPos * i.worldPos;
			}

			ENDCG
			CGPROGRAM
			#pragma surface surf Standard alpha:fade keepalpha finalcolor:RefractionF fullforwardshadows exclude_path:deferred

			ENDCG
			Pass
			{
				Name "ShadowCaster"
				Tags{ "LightMode" = "ShadowCaster" }
				ZWrite On
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 3.0
				#pragma multi_compile_shadowcaster
				#pragma multi_compile UNITY_PASS_SHADOWCASTER
				#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
				#include "HLSLSupport.cginc"
				#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
					#define CAN_SKIP_VPOS
				#endif
				#include "UnityCG.cginc"
				#include "Lighting.cginc"
				#include "UnityPBSLighting.cginc"
				sampler3D _DitherMaskLOD;
				struct v2f
				{
					V2F_SHADOW_CASTER;
					float2 customPack1 : TEXCOORD1;
					float3 worldPos : TEXCOORD2;
					float4 screenPos : TEXCOORD3;
					float4 tSpace0 : TEXCOORD4;
					float4 tSpace1 : TEXCOORD5;
					float4 tSpace2 : TEXCOORD6;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};
				v2f vert(appdata_full v)
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_OUTPUT(v2f, o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					Input customInputData;
					float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
					half3 worldNormal = UnityObjectToWorldNormal(v.normal);
					half3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
					half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
					half3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
					o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
					o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
					o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
					o.customPack1.xy = customInputData.uv_texcoord;
					o.customPack1.xy = v.texcoord;
					o.worldPos = worldPos;
					TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
					o.screenPos = ComputeScreenPos(o.pos);
					return o;
				}
				half4 frag(v2f IN
				#if !defined( CAN_SKIP_VPOS )
				, UNITY_VPOS_TYPE vpos : VPOS
				#endif
				) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID(IN);
					Input surfIN;
					UNITY_INITIALIZE_OUTPUT(Input, surfIN);
					surfIN.uv_texcoord = IN.customPack1.xy;
					float3 worldPos = IN.worldPos;
					half3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
					surfIN.worldPos = worldPos;
					surfIN.screenPos = IN.screenPos;
					SurfaceOutputStandard o;
					UNITY_INITIALIZE_OUTPUT(SurfaceOutputStandard, o)
					surf(surfIN, o);
					#if defined( CAN_SKIP_VPOS )
					float2 vpos = IN.pos;
					#endif
					half alphaRef = tex3D(_DitherMaskLOD, float3(vpos.xy * 0.25, o.Alpha * 0.9375)).a;
					clip(alphaRef - 0.01);
					SHADOW_CASTER_FRAGMENT(IN)
				}
				ENDCG
			}
		}
			Fallback "Diffuse"
						CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=15500
0;494;1071;524;1254.521;417.0568;1.748601;True;False
Node;AmplifyShaderEditor.ColorNode;2;-678.4642,-235.2645;Float;False;Property;_Tint;Tint;1;0;Create;True;0;0;False;0;1,1,1,1;1,0,0.09197801,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;27;-435.9864,-393.8112;Float;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;25;-723.8242,41.50547;Float;False;Property;_NormalScale;Normal Scale;6;0;Create;True;0;0;False;0;0.1;0.143;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-724.5503,142.8828;Float;False;Property;_Smoothness;Smoothness;3;0;Create;True;0;0;False;0;0.8;0.8;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1;-730.6448,247.6671;Float;False;Property;_IOR;IOR;8;0;Create;True;0;0;False;0;1.2;1.26;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;-51.09309,-246.8141;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-735.1558,346.2893;Float;False;Property;_Opacity;Opacity;2;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;23;-387.6871,-124.0765;Float;True;Property;_Normal;Normal;5;0;Create;True;0;0;False;0;None;9a4a55d8d2e54394d97426434477cdcf;True;0;False;white;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;29;-206.1069,380.5053;Float;False;Property;_Glow;Glow;4;1;[HDR];Create;True;0;0;True;0;0,0,0,0;0,0,0,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;129.5878,-71.59726;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;Glass/DiffuseBumped;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;7;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;28;0;27;0
WireConnection;28;1;2;0
WireConnection;23;5;25;0
WireConnection;0;0;28;0
WireConnection;0;1;23;0
WireConnection;0;2;29;0
WireConnection;0;4;3;0
WireConnection;0;8;1;0
WireConnection;0;9;22;0
ASEEND*/
//CHKSM=8B9FB10C67DA5FA2DBC92A792A8BDED22B6D3258
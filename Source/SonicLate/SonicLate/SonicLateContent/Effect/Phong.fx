float4x4 matWorld;
float4x4 matVP;
float4x4 bones[ 60 ];
float3 lightDir;
float4 ambient;
float4 diffuse;
float3 eyePos;
texture texMesh;

sampler MeshSampler = sampler_state{
	texture = ( texMesh );
	MIPFILTER = LINEAR;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
};

struct VSInput{
	float4 Pos : POSITION0;
	float3 Normal : NORMAL;
	float2 MeshUV : TEXCOORD0;
	int4 Indices : BLENDINDICES0;
	float4 Weights : BLENDWEIGHT0;
};

struct VSOutPut{
	float4 Pos : POSITION; // 射影変換後の座標
	float4 Color : COLOR0; // ディフューズ色
	float2 MeshUV : TEXCOORD0; // UV
	float3 RefValue : TEXCOORD1; // 鏡面反射光
};

//-----------------------------------------------------------------------------
// 頂点シェーダ 通常のメッシュ
//-----------------------------------------------------------------------------
VSOutPut PhongVS( float4 Pos : POSITION, float3 Normal : NORMAL, float2 MeshUV : TEXCOORD0 ){
	VSOutPut Out = ( VSOutPut )0;

	// 通常の射影変換
	Out.Pos = mul( Pos, matWorld );
	Out.Pos = mul( Out.Pos, matVP );

	// 頂点色を算出
	float3 eye = normalize( eyePos - Pos.xyz ); // 頂点から視点へのベクトル
	float3 light = -lightDir;
	float3 n = normalize( mul( Normal.xyz, matWorld ) );
	float3 r = -eye + 2.0f * dot( n, eye ) * n; // 反射ベクトル

	Out.Color = diffuse * max( ambient, dot( n, light ) );
	Out.Color.a = diffuse.a; // アルファ値はディフューズをそのまま使う
	Out.RefValue = pow( max( 0, dot( light, r ) ), 10 );

	// UVはそのまま
	Out.MeshUV = MeshUV;

	return Out;
}

//-----------------------------------------------------------------------------
// 頂点シェーダ スキンメッシュ
//-----------------------------------------------------------------------------
VSOutPut PhongSkinedVS( VSInput Input ){
	VSOutPut Out = ( VSOutPut )0;
	
	// ワールド変換行列をブレンド
	float4x4 matCombWorld = 0.0f;
	for ( int i = 0; i < 4; i++ ){
		matCombWorld += mul( bones[ Input.Indices[ i ] ], Input.Weights[ i ] );
	}

	// 通常の射影変換
	Out.Pos = mul( Input.Pos, matCombWorld );
	Out.Pos = mul( Out.Pos, matVP );

	// 頂点色を算出
	float3 eye = normalize( eyePos - Input.Pos.xyz ); // 頂点から視点へのベクトル
	float3 light = -lightDir;
	float3 n = normalize( mul( Input.Normal, ( float3x3 )matCombWorld ) );
	float3 r = -eye + 2.0f * dot( n, eye ) * n; // 反射ベクトル
	
	Out.Color = diffuse * max( ambient, dot( n, light ) );
	Out.Color.a = diffuse.a; // アルファ値はディフューズをそのまま使う
	Out.RefValue = pow( max( 0, dot( light, r ) ), 10 );

	// UVはそのまま
	Out.MeshUV = Input.MeshUV;

	return Out;
}

//-----------------------------------------------------------------------------
// ピクセルシェーダ
//-----------------------------------------------------------------------------
float4 PhongPS( VSOutPut In ) : COLOR{
	// 頂点色 + テクスチャ
	float4 Out = In.Color * tex2D( MeshSampler, In.MeshUV );
	Out.xyz += In.RefValue;
	
	// アルファ値はディフューズ
	Out.a = In.Color.a;

	return Out;
}

//-----------------------------------------------------------------------------
// テクニック 通常のメッシュ
//-----------------------------------------------------------------------------
technique PhongTec{
	pass P0{
		VertexShader = compile vs_2_0 PhongVS();
		PixelShader = compile ps_2_0 PhongPS();
	}
}

//-----------------------------------------------------------------------------
// テクニック スキンメッシュ
//-----------------------------------------------------------------------------
technique PhongSkinedTec{
	pass P0{
		VertexShader = compile vs_2_0 PhongSkinedVS();
		PixelShader = compile ps_2_0 PhongPS();
	}
}
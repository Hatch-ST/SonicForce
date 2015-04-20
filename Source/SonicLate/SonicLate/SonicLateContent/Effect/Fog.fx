float4x4 matWorld;
float4x4 matVP;
float4x4 bones[ 60 ];
float3 lightDir;
float4 ambient;
float4 diffuse;
float4 fogColor;
float2 fogCoord;
float3 cameraPosition;
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
	float2 Depth : TEXCOORD1;// 深度
};


//-----------------------------------------------------------------------------
// 頂点シェーダ 通常のメッシュ
//-----------------------------------------------------------------------------
VSOutPut FogVS( float4 Pos : POSITION0, float3 Normal : NORMAL, float2 MeshUV : TEXCOORD0 ){
	VSOutPut Out = ( VSOutPut )0;

	// 通常の射影変換
	Out.Pos = mul( Pos, matWorld );
	Out.Pos = mul( Out.Pos, matVP );

	// 頂点色を算出
	float3 n = normalize( mul( Normal, matWorld ) );
	Out.Color = ( max( 0.0, dot( n, -lightDir ) ) + ambient ) * diffuse;
	Out.Color.a = diffuse.a; // アルファ値はディフューズをそのまま使う
	
	// フォグ
	Out.Depth.x = Out.Pos.z; // 深度
	Out.Depth.y = length( Pos - cameraPosition ); // 距離

	// UVはそのまま
	Out.MeshUV = MeshUV;

	return Out;
}

//-----------------------------------------------------------------------------
// 頂点シェーダ スキンメッシュ
//-----------------------------------------------------------------------------
VSOutPut FogSkinVS( VSInput Input ){
	VSOutPut Out = ( VSOutPut )0;

	// ワールド変換行列をブレンド
	float4x4 matCombWorld = 0.0f;
	for ( int i = 0; i < 4; i++ ){
		matCombWorld += mul( bones[ Input.Indices[ i ] ], Input.Weights[ i ] );
	}

	Out.Pos = mul( Input.Pos, matCombWorld );
	Out.Pos = mul( Out.Pos, matVP );
	Input.Normal = mul( Input.Normal, ( float3x3 )matCombWorld );
	
	// 頂点色を算出
	float3 n = mul( Input.Normal, matCombWorld );
	Out.Color = ( max( 0.0, dot( n, -lightDir ) ) + ambient ) * diffuse;
	Out.Color.a = diffuse.a; // アルファ値はディフューズをそのまま使う
	
	// フォグ
	Out.Depth.x = Out.Pos.z; // 深度
	Out.Depth.y = length( Input.Pos - cameraPosition ); // 距離

	// UVはそのまま
	Out.MeshUV = Input.MeshUV;

	return Out;
}

//-----------------------------------------------------------------------------
// ピクセルシェーダ
//-----------------------------------------------------------------------------
float4 FogPS( VSOutPut In ) : COLOR0{
	float4 color = In.Color * tex2D( MeshSampler, In.MeshUV );
	float fog = max( 0.0f, min( 1.0f, fogCoord.y + In.Depth * fogCoord.y ) );
	float4 Out = color * ( 1.0f - fog ) + fogColor * fog;
	Out.a = color.a;

	return Out;
}

//-----------------------------------------------------------------------------
// テクニック 通常のメッシュ
//-----------------------------------------------------------------------------
technique FogTec{
	pass P0{
		VertexShader = compile vs_2_0 FogVS();
		PixelShader = compile ps_2_0 FogPS();
	}
}

//-----------------------------------------------------------------------------
// テクニック スキンメッシュ
//-----------------------------------------------------------------------------
technique SkinnedFogTec{
	pass P0{
		VertexShader = compile vs_2_0 FogSkinVS();
		PixelShader = compile ps_2_0 FogPS();
	}
}

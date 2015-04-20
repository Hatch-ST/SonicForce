float4x4 matWorld;
float4x4 matVP;
float4x4 bones[ 60 ];
float4 ambient;
float4 diffuse;
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
};


//-----------------------------------------------------------------------------
// 頂点シェーダ 通常のメッシュ
//-----------------------------------------------------------------------------
VSOutPut DiffuseVS( float4 Pos : POSITION0, float3 Normal : NORMAL, float2 MeshUV : TEXCOORD0 ){
	VSOutPut Out = ( VSOutPut )0;

	// 通常の射影変換
	Out.Pos = mul( Pos, matWorld );
	Out.Pos = mul( Out.Pos, matVP );

	Out.Color = diffuse;
	Out.MeshUV = MeshUV;

	return Out;
}

//-----------------------------------------------------------------------------
// 頂点シェーダ スキンメッシュ
//-----------------------------------------------------------------------------
VSOutPut DiffuseSkinVS( VSInput Input ){
	VSOutPut Out = ( VSOutPut )0;

	// ワールド変換行列をブレンド
	float4x4 matCombWorld = 0.0f;
	for ( int i = 0; i < 4; i++ ){
		matCombWorld += mul( bones[ Input.Indices[ i ] ], Input.Weights[ i ] );
	}

	Out.Pos = mul( Input.Pos, matCombWorld );
	Out.Pos = mul( Out.Pos, matVP );
	
	Out.Color = diffuse;
	Out.MeshUV = Input.MeshUV;

	return Out;
}

//-----------------------------------------------------------------------------
// ピクセルシェーダ
//-----------------------------------------------------------------------------
float4 DiffusePS( VSOutPut In ) : COLOR0{
	// 頂点色 * テクスチャ
	float4 Out = In.Color * tex2D( MeshSampler, In.MeshUV );

	return Out;
}

//-----------------------------------------------------------------------------
// テクニック 通常のメッシュ
//-----------------------------------------------------------------------------
technique DiffuseTec{
	pass P0{
		VertexShader = compile vs_2_0 DiffuseVS();
		PixelShader = compile ps_2_0 DiffusePS();
	}
}

//-----------------------------------------------------------------------------
// テクニック スキンメッシュ
//-----------------------------------------------------------------------------
technique SkinnedDiffuseTec{
	pass P0{
		VertexShader = compile vs_2_0 DiffuseSkinVS();
		PixelShader = compile ps_2_0 DiffusePS();
	}
}

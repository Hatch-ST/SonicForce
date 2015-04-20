float4x4 matWorld;
float4x4 matVP;
float4x4 bones[ 60 ];
float3 lightDir;
float4 ambient;
float4 diffuse;
float3 viewVec;
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
	float4 Back : COLOR1;
	float2 MeshUV : TEXCOORD0; // UV
};

//-----------------------------------------------------------------------------
// 頂点シェーダ 通常のメッシュ
//-----------------------------------------------------------------------------
VSOutPut WrappedVS( float4 Pos : POSITION, float3 Normal : NORMAL, float2 MeshUV : TEXCOORD0 ){
	VSOutPut Out = ( VSOutPut )0;

	// 通常の射影変換
	Out.Pos = mul( Pos, matWorld );
	Out.Pos = mul( Out.Pos, matVP );
	
	float3 n = normalize( mul( Normal.xyz, matWorld ) );
	float cos = dot( n, lightDir );

	// 基本色
	float4 color = max( ambient, -cos );

	// 回り込み
	float4 wrap = max( 0.0f, cos ) * 0.2f;

	// 疑似散乱光
	float t = min( 0.8f, max( 0.0f, 1.0f - dot( n, -viewVec ) ) );
	Out.Back =  pow( t, 3 );

	Out.Color = diffuse * min( color + wrap, 1.0f );
	Out.Color.a = diffuse.a; // アルファ値はディフューズをそのまま使う

	// UVはそのまま
	Out.MeshUV = MeshUV;

	return Out;
}

//-----------------------------------------------------------------------------
// 頂点シェーダ スキンメッシュ
//-----------------------------------------------------------------------------
VSOutPut WrappedSkinedVS( VSInput Input ){
	VSOutPut Out = ( VSOutPut )0;
		
	// ワールド変換行列をブレンド
	float4x4 matCombWorld = 0.0f;
	for ( int i = 0; i < 4; i++ ){
		matCombWorld += mul( bones[ Input.Indices[ i ] ], Input.Weights[ i ] );
	}

	// 通常の射影変換
	Out.Pos = mul( Input.Pos, matCombWorld );
	Out.Pos = mul( Out.Pos, matVP );

	float3 n = normalize( mul( Input.Normal.xyz, matCombWorld ) );
	float cos = dot( n, lightDir );

	// 基本色
	float4 color = max( ambient, -cos );

	// 回り込み
	float4 wrap = max( 0.0f, cos ) * 0.2f;

	// 疑似散乱光
	float t = min( 0.8f, max( 0.0f, 1.0f - dot( n, -viewVec ) ) );
	Out.Back =  pow( t, 3 );

	Out.Color = diffuse * min( color + wrap, 1.0f );
	Out.Color.a = diffuse.a; // アルファ値はディフューズをそのまま使う

	// UVはそのまま
	Out.MeshUV = Input.MeshUV;

	return Out;
}

//-----------------------------------------------------------------------------
// ピクセルシェーダ
//-----------------------------------------------------------------------------
float4 WrappedPS( VSOutPut In ) : COLOR{
	// 頂点色 + テクスチャ
	float4 Out = In.Color * tex2D( MeshSampler, In.MeshUV ) + In.Back;
	
	// アルファ値はディフューズ
	//Out.a = In.Color.a;

	return Out;
}

//-----------------------------------------------------------------------------
// テクニック 通常のメッシュ
//-----------------------------------------------------------------------------
technique WrappedTec{
	pass P0{
		VertexShader = compile vs_2_0 WrappedVS();
		PixelShader = compile ps_2_0 WrappedPS();
	}
}

//-----------------------------------------------------------------------------
// テクニック スキンメッシュ
//-----------------------------------------------------------------------------
technique WrappedSkinedTec{
	pass P0{
		VertexShader = compile vs_2_0 WrappedSkinedVS();
		PixelShader = compile ps_2_0 WrappedPS();
	}
}
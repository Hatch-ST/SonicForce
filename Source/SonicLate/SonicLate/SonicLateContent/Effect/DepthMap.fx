float4x4 matWorld;
float4x4 matVP;
float4x4 bones[ 60 ];

struct VSInput{
	float4 Pos : POSITION0;
	int4 Indices : BLENDINDICES0;
	float4 Weights : BLENDWEIGHT0;
};

struct VSOutPut{
	float4 Pos : POSITION; // 射影変換座標
	float4 ShadowMapTex : TEXCOORD0; // Zバッファテクスチャ
};

//-----------------------------------------------------------------------------
// 頂点シェーダ スキニングしない
//-----------------------------------------------------------------------------
VSOutPut DepthMapVS( float4 Pos : POSITION ){
	VSOutPut Out = ( VSOutPut )0;

	// 通常の射影変換
	Out.Pos = mul( Pos, matWorld );
	Out.Pos = mul( Out.Pos, matVP );

	// テクスチャ座標を頂点に合わせる
	Out.ShadowMapTex = Out.Pos;

	return Out;
}

//-----------------------------------------------------------------------------
// 頂点シェーダ スキニングする
//-----------------------------------------------------------------------------
VSOutPut DepthMapSkinedVS( VSInput Input ){
	VSOutPut Out = ( VSOutPut )0;

	// ワールド変換行列をブレンド
	float4x4 matCombWorld = 0.0f;
	for ( int i = 0; i < 4; i++ ){
		matCombWorld += mul( bones[ Input.Indices[ i ] ], Input.Weights[ i ] );
	}
	
	// 通常の射影変換
	Out.Pos = mul( Input.Pos, matCombWorld );
	Out.Pos = mul( Out.Pos, matVP );

	// テクスチャ座標を頂点に合わせる
	Out.ShadowMapTex = Out.Pos;

	return Out;
}

//-----------------------------------------------------------------------------
// ピクセルシェーダ
//-----------------------------------------------------------------------------
float4 DepthMapPS( float4 ShadowMapTex : TEXCOORD0 ) : COLOR{
	float z = ShadowMapTex.z / ShadowMapTex.w;
	float4 Depth = float4( 0, 0, 256.0f, 256.0f );
	Depth.g = modf( z * 256.0f, Depth.r ); // 整数部をrに格納
	Depth.b *= modf( Depth.g * 256.0f, Depth.g ); // 整数部をgに格納
	Depth /= 256.0f; // 正規化

	return Depth;
}

//-----------------------------------------------------------------------------
// テクニック 通常のメッシュ
//-----------------------------------------------------------------------------
technique DepthMapTec{
	pass P0{
		VertexShader = compile vs_2_0 DepthMapVS();
		PixelShader = compile ps_2_0 DepthMapPS();
	}
}

//-----------------------------------------------------------------------------
// テクニック スキンメッシュ
//-----------------------------------------------------------------------------
technique DepthMapSkinedTec{
	pass P0{
		VertexShader = compile vs_2_0 DepthMapSkinedVS();
		PixelShader = compile ps_2_0 DepthMapPS();
	}
}
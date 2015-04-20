float4x4 matWorld;
float4x4 matVP;
float4x4 bones[ 50 ];
float4 ambient;
float3 lightDir;
float4 diffuse;
float4 fogColor;
float2 fogCoord;
texture texMesh;

float4x4 matLightView;
float4x4 matLightProj;
texture texShadowMap;

sampler DefSampler = sampler_state{
	texture = ( texShadowMap );
	AddressU = CLAMP;
	AddressV = CLAMP;
	AddressW = CLAMP;
	MIPFILTER = LINEAR;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
};

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
	float4 ZCalcTex : TEXCOORD1; // Zバッファテクスチャ座標
	float2 Depth : TEXCOORD2;// 深度
};

//-----------------------------------------------------------------------------
// 頂点シェーダ スキニングしない
//-----------------------------------------------------------------------------
VSOutPut DepthShadowVS( float4 Pos : POSITION, float3 Normal : NORMAL, float2 MeshUV : TEXCOORD0 ){
	VSOutPut Out = ( VSOutPut )0;

	// 通常の射影変換
	Out.Pos = mul( Pos, matWorld );
	Out.Pos = mul( Out.Pos, matVP );

	// Z値比較のためにライト目線で射影変換
	Out.ZCalcTex = mul( Pos, matWorld );
	Out.ZCalcTex = mul( Out.ZCalcTex, matLightView );
	Out.ZCalcTex = mul( Out.ZCalcTex, matLightProj );

	// 頂点色を算出
	float3 n = normalize( mul( Normal, matWorld ) );
	Out.Color = ( max( 0.0, dot( n, -lightDir ) ) + ambient ) * diffuse;
	Out.Color.a = diffuse.a; // アルファ値はディフューズをそのまま使う
	
	// ライト目線で裏ならZ値を0にする
	float3 l = float3( matLightView._13, matLightView._23, matLightView._33 );
	Out.ZCalcTex.z *= ceil( max( 0.0f, dot( n, l ) ) );

	// UVはそのまま
	Out.MeshUV = MeshUV;
	
	// 深度
	Out.Depth.x = Out.Pos.z;
	Out.Depth.y = 0;

	return Out;
}

//-----------------------------------------------------------------------------
// 頂点シェーダ スキニングする
//-----------------------------------------------------------------------------
VSOutPut DepthShadowSkinedVS( VSInput Input ){
	VSOutPut Out = ( VSOutPut )0;
	
	// ワールド変換行列をブレンド
	float4x4 matCombWorld = 0.0f;
	for ( int i = 0; i < 4; i++ ){
		matCombWorld += mul( bones[ Input.Indices[ i ] ], Input.Weights[ i ] );
	}

	// 通常の射影変換
	Out.Pos = mul( Input.Pos, matCombWorld );
	Out.Pos = mul( Out.Pos, matVP );
	Input.Normal = mul( Input.Normal, ( float3x3 )matCombWorld );

	// Z値比較のためにライト目線で射影変換
	Out.ZCalcTex = mul( Input.Pos, matCombWorld );
	Out.ZCalcTex = mul( Out.ZCalcTex, matLightView );
	Out.ZCalcTex = mul( Out.ZCalcTex, matLightProj );

	// 頂点色を算出
	float3 n = mul( Input.Normal, matCombWorld );
	Out.Color = ( max( 0.0, dot( n, -lightDir ) ) + ambient ) * diffuse;
	Out.Color.a = diffuse.a; // アルファ値はディフューズをそのまま使う
	
	// ライト目線で裏ならZ値を0にする
	float3 l = float3( matLightView._13, matLightView._23, matLightView._33 );
	Out.ZCalcTex.z *= ceil( max( 0.0f, dot( n, l ) ) );

	// UVはそのまま
	Out.MeshUV = Input.MeshUV;

	// 深度
	Out.Depth.x = Out.Pos.z;
	Out.Depth.y = 0;

	return Out;
}

//-----------------------------------------------------------------------------
// ピクセルシェーダ
//-----------------------------------------------------------------------------
float4 DepthShadowPS( VSOutPut In ) : COLOR{
	// 今回の深度
	float zValue = ( In.ZCalcTex.z / In.ZCalcTex.w );
	zValue = min( zValue, 1.0f );

	// 対応するテクセル座標
	float2 transTexCoord;
	transTexCoord.x = ( 1.0f + In.ZCalcTex.x / In.ZCalcTex.w ) * 0.5f;
	transTexCoord.y = ( 1.0f - In.ZCalcTex.y / In.ZCalcTex.w ) * 0.5f;

	 // Zテクスチャの深度を算出
	float4 t = tex2D( DefSampler, transTexCoord );
	float shadowMapZ = t.r + ( t.g + t.b / 256.0f ) / 256.0f;

	// 基本色( 頂点 + テクスチャ )
	float4 Out = In.Color * tex2D( MeshSampler, In.MeshUV );
	
	float shade = ( zValue > shadowMapZ ) ? 1 : 0;
	float dark = shadowMapZ * 0.1f * shade;

	Out.rgb -= dark;
	Out.a = In.Color.a; // アルファ値はディフューズ

	return Out;
}

//-----------------------------------------------------------------------------
// テクニック 通常のメッシュ
//-----------------------------------------------------------------------------
technique DepthShadowTec{
	pass P0{
		VertexShader = compile vs_2_0 DepthShadowVS();
		PixelShader = compile ps_2_0 DepthShadowPS();
	}
}

//-----------------------------------------------------------------------------
// テクニック スキンメッシュ
//-----------------------------------------------------------------------------
technique DepthShadowSkinedTec{
	pass P0{
		VertexShader = compile vs_2_0 DepthShadowSkinedVS();
		PixelShader = compile ps_2_0 DepthShadowPS();
	}
}

//-----------------------------------------------------------------------------
// フォグ
//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
// 頂点シェーダ 通常のメッシュ
//-----------------------------------------------------------------------------
VSOutPut DepthShadowFogVS( float4 Pos : POSITION0, float3 Normal : NORMAL, float2 MeshUV : TEXCOORD0 ){
	VSOutPut Out = ( VSOutPut )0;

	// 通常の射影変換
	Out.Pos = mul( Pos, matWorld );
	Out.Pos = mul( Out.Pos, matVP );

	// Z値比較のためにライト目線で射影変換
	Out.ZCalcTex = mul( Pos, matWorld );
	Out.ZCalcTex = mul( Out.ZCalcTex, matLightView );
	Out.ZCalcTex = mul( Out.ZCalcTex, matLightProj );

	// 頂点色を算出
	float3 n = normalize( mul( Normal, matWorld ) );
	Out.Color = ( max( 0.0f, dot( n, -lightDir ) ) + ambient ) * diffuse;
	Out.Color.a = diffuse.a; // アルファ値はディフューズをそのまま使う
	
	// ライト目線で裏ならZ値を0にする
	float3 l = float3( matLightView._13, matLightView._23, matLightView._33 );
	//Out.ZCalcTex.z *= ceil( max( 0.0f, dot( n, l ) ) );
	
	// 深度
	Out.Depth.x = Out.Pos.z;
	Out.Depth.y = 0;


	// UVはそのまま
	Out.MeshUV = MeshUV;

	return Out;
}

//-----------------------------------------------------------------------------
// ピクセルシェーダ
//-----------------------------------------------------------------------------
float4 DepthShadowFogPS( VSOutPut In ) : COLOR0{
	// 今回の深度
	float zValue = ( In.ZCalcTex.z / In.ZCalcTex.w );
	zValue = min( zValue, 1.0f );

	// 対応するテクセル座標
	float2 transTexCoord;
	transTexCoord.x = ( 1.0f + In.ZCalcTex.x / In.ZCalcTex.w ) * 0.5f;
	transTexCoord.y = ( 1.0f - In.ZCalcTex.y / In.ZCalcTex.w ) * 0.5f;

	 // Zテクスチャの深度を算出
	float4 t = tex2D( DefSampler, transTexCoord );
	float shadowMapZ = t.r + ( t.g + t.b / 256.0f ) / 256.0f;

	// 基本色( 頂点 + テクスチャ )
	float4 Out = In.Color * tex2D( MeshSampler, In.MeshUV );
	float fog = max( 0.0f, min( 1.0f, fogCoord.x + In.Depth * fogCoord.y ) );
	Out = Out * ( 1.0f - fog ) + fogColor * fog;

	float shade = ( zValue > shadowMapZ ) ? 1 : 0;
	float dark = shadowMapZ * 0.1f * shade;

	Out.rgb -= dark;
	Out.a = In.Color.a; // アルファ値はディフューズ

	return Out;
}

//-----------------------------------------------------------------------------
// テクニック 通常のメッシュ
//-----------------------------------------------------------------------------
technique DepthShadowFogTec{
	pass P0{
		VertexShader = compile vs_2_0 DepthShadowFogVS();
		PixelShader = compile ps_2_0 DepthShadowFogPS();
	}
}
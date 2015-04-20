float4x4 matVP;
float4x4 matWorld;
float4 diffuse;

texture texMesh;

sampler MeshSampler = sampler_state{
	texture = ( texMesh );
	MIPFILTER = LINEAR;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
};

struct VSOutPut{
	float4 Pos : POSITION; // 射影変換後の座標
	float2 MeshUV : TEXCOORD0; // UV
};

struct VSOutPutUseColor{
	float4 Pos : POSITION; // 射影変換後の座標
	float4 Color : COLOR0;
	float2 MeshUV : TEXCOORD0; // UV
};

//-----------------------------------------------------------------------------
// 頂点シェーダ
//-----------------------------------------------------------------------------
VSOutPut BoardVS( float4 Pos : POSITION, float2 MeshUV : TEXCOORD0 ){
	VSOutPut Out = ( VSOutPut )0;

	// 通常の射影変換
	Out.Pos = mul( Pos, matWorld );
	Out.Pos = mul( Out.Pos, matVP );

	// UVはそのまま
	Out.MeshUV = MeshUV;

	return Out;
}


//-----------------------------------------------------------------------------
// ピクセルシェーダ
//-----------------------------------------------------------------------------
float4 BoardPS( float2 MeshUV : TEXCOORD0 ) : COLOR{
	// ディフューズ色 * テクスチャ
	float4 Out = diffuse * tex2D( MeshSampler, MeshUV );

	return Out;
}

//-----------------------------------------------------------------------------
// テクニック
//-----------------------------------------------------------------------------
technique BoardTec{
	pass P0{
		VertexShader = compile vs_2_0 BoardVS();
		PixelShader = compile ps_2_0 BoardPS();
	}
}



//-----------------------------------------------------------------------------
// 頂点シェーダ ディフューズ色を使う
//-----------------------------------------------------------------------------
VSOutPutUseColor BoardUseColorVS( float4 Pos : POSITION, float4 Color : COLOR0, float2 MeshUV : TEXCOORD0 ){
	VSOutPutUseColor Out = ( VSOutPutUseColor )0;

	// 通常の射影変換
	Out.Pos = mul( Pos, matWorld );
	Out.Pos = mul( Out.Pos, matVP );

	Out.Color = Color;

	// UVはそのまま
	Out.MeshUV = MeshUV;

	return Out;
}

//-----------------------------------------------------------------------------
// ピクセルシェーダ ディフューズ色を使う
//-----------------------------------------------------------------------------
float4 BoardUseColorPS( float4 Color : COLOR0, float2 MeshUV : TEXCOORD0 ) : COLOR{
	// 頂点色 * テクスチャ
	float4 Out = Color * tex2D( MeshSampler, MeshUV );

	return Out;
}

//-----------------------------------------------------------------------------
// テクニック ディフューズ色を使う
//-----------------------------------------------------------------------------
technique BoardUseColorTec{
	pass P0{
		VertexShader = compile vs_2_0 BoardUseColorVS();
		PixelShader = compile ps_2_0 BoardUseColorPS();
	}
}
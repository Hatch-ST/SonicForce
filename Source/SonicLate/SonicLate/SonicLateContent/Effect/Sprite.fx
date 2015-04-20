float4x4 matWVP;
texture texMesh;
float4 diffuse;
float bright;

float texU[ 5 ]; // X方向の隣のテクセル位置
float texV[ 5 ]; // Y方向の隣のテクセル位置

sampler MeshSampler = sampler_state{
	texture = ( texMesh );
	MIPFILTER = LINEAR;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
};

struct VSOutPut{
	float4 Pos : POSITION; // 射影変換後の座標
	float2 UV : TEXCOORD0; // UV
};

//-----------------------------------------------------------------------------
// 頂点シェーダ
//-----------------------------------------------------------------------------
VSOutPut SpriteVS( float4 Pos : POSITION, float2 UV : TEXCOORD0 ){
	VSOutPut Out;
	Out.Pos = mul( Pos, matWVP );
	Out.UV = UV;

	return Out;
}

//-----------------------------------------------------------------------------
// ピクセルシェーダ
//-----------------------------------------------------------------------------
float4 SpritePS( float2 UV : TEXCOORD0 ) : COLOR{
	float4 Color;
	Color = tex2D( MeshSampler, UV ) * diffuse;
	Color.rgb -= 1.0f - bright;

	return Color;
}

//-----------------------------------------------------------------------------
// ブラー用 中心が透明
//-----------------------------------------------------------------------------
float4 SpriteBlurPS( float2 UV : TEXCOORD0 ) : COLOR{
	float4 Color = ( float4 )0;
	Color = tex2D( MeshSampler, UV ) * diffuse;

	float2 v = float2( abs( UV.x - 0.5f ), abs( UV.y - 0.5f ) );
	float t = ( v.x * v.x + v.y * v.y ) / 0.5f; // ( 0.0 ≦ |v|^2 ≦ 0.5 )
	t = 2.4f * pow( t, 3 ) - 5.2f * pow( t, 2 ) + 3.8 * t + 0.06; // 透過範囲を狭める
	Color.rgb -= 1.0f - bright;
	Color.a = t * diffuse.a;

	return Color;
}

technique SpriteTec{
	pass P0{
		VertexShader = compile vs_2_0 SpriteVS();
		PixelShader = compile ps_2_0 SpritePS();
	}
}

technique SpriteBlurTec{
	pass P0{
		VertexShader = compile vs_2_0 SpriteVS();
		PixelShader = compile ps_2_0 SpriteBlurPS();
	}
}


//-----------------------------------------------------------------------------
// ブラーフィルタ
//-----------------------------------------------------------------------------
// X方向にぼかす
float4 SpriteBlurFilterPS0( VSOutPut In ) : COLOR0{
	// テクセルを取得
	float2 texel0 = In.UV + float2( -texU[ 0 ], 0.0f );
	float2 texel1 = In.UV + float2( -texU[ 1 ], 0.0f );
	float2 texel2 = In.UV + float2( -texU[ 2 ], 0.0f );
	float2 texel3 = In.UV + float2( -texU[ 3 ], 0.0f );
	float2 texel4 = In.UV + float2( -texU[ 4 ], 0.0f );

	float2 texel5 = In.UV + float2( texU[ 0 ], 0.0f );
	float2 texel6 = In.UV + float2( texU[ 1 ], 0.0f );
	float2 texel7 = In.UV + float2( texU[ 2 ], 0.0f );
	float2 texel8 = In.UV + float2( texU[ 3 ], 0.0f );
	float2 texel9 = In.UV + float2( texU[ 4 ], 0.0f );

	// カラー情報を取得 重みの合計は1.0f
	float4 p0 = tex2D( MeshSampler, In.UV ) * 0.20f;

	float4 p1 = tex2D( MeshSampler, texel0 ) * 0.12f;
	float4 p2 = tex2D( MeshSampler, texel1 ) * 0.10f;
	float4 p3 = tex2D( MeshSampler, texel2 ) * 0.08f;
	float4 p4 = tex2D( MeshSampler, texel3 ) * 0.06f;
	float4 p5 = tex2D( MeshSampler, texel4 ) * 0.04f;

	float4 p6 = tex2D( MeshSampler, texel5 ) * 0.12f;
	float4 p7 = tex2D( MeshSampler, texel6 ) * 0.10f;
	float4 p8 = tex2D( MeshSampler, texel7 ) * 0.08f;
	float4 p9 = tex2D( MeshSampler, texel8 ) * 0.06f;
	float4 p10 = tex2D( MeshSampler, texel9 ) * 0.04f;

	// カラーを合成する
	return p0 + p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9 + p10;
}

// Y方向にぼかす
float4 SpriteBlurFilterPS1( VSOutPut In ) : COLOR0{
	// テクセルを取得
	float2 texel0 = In.UV + float2( 0.0f, -texV[ 0 ] );
	float2 texel1 = In.UV + float2( 0.0f, -texV[ 1 ] );
	float2 texel2 = In.UV + float2( 0.0f, -texV[ 2 ] );
	float2 texel3 = In.UV + float2( 0.0f, -texV[ 3 ] );
	float2 texel4 = In.UV + float2( 0.0f, -texV[ 4 ] );

	float2 texel5 = In.UV + float2( 0.0f, texV[ 0 ] );
	float2 texel6 = In.UV + float2( 0.0f, texV[ 1 ] );
	float2 texel7 = In.UV + float2( 0.0f, texV[ 2 ] );
	float2 texel8 = In.UV + float2( 0.0f, texV[ 3 ] );
	float2 texel9 = In.UV + float2( 0.0f, texV[ 4 ] );

	// カラー情報を取得 重みの合計は1.0f
	float4 p0 = tex2D( MeshSampler, In.UV ) * 0.20f;

	float4 p1 = tex2D( MeshSampler, texel0 ) * 0.12f;
	float4 p2 = tex2D( MeshSampler, texel1 ) * 0.10f;
	float4 p3 = tex2D( MeshSampler, texel2 ) * 0.08f;
	float4 p4 = tex2D( MeshSampler, texel3 ) * 0.06f;
	float4 p5 = tex2D( MeshSampler, texel4 ) * 0.04f;

	float4 p6 = tex2D( MeshSampler, texel5 ) * 0.12f;
	float4 p7 = tex2D( MeshSampler, texel6 ) * 0.10f;
	float4 p8 = tex2D( MeshSampler, texel7 ) * 0.08f;
	float4 p9 = tex2D( MeshSampler, texel8 ) * 0.06f;
	float4 p10 = tex2D( MeshSampler, texel9 ) * 0.04f;

	// カラーを合成する
	return p0 + p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9 + p10;
}

technique SpriteBlurFilterTec{
	pass P0{
		VertexShader = compile vs_1_1 SpriteVS();
		PixelShader = compile ps_2_0 SpriteBlurFilterPS0();
	}
	pass P1{
		VertexShader = compile vs_1_1 SpriteVS();
		PixelShader = compile ps_2_0 SpriteBlurFilterPS1();
	}
}
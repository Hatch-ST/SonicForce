float texU[ 5 ]; // X方向の隣のテクセル位置
float texV[ 5 ]; // Y方向の隣のテクセル位置

sampler s0 : register( s0 ); // ブラーをかける元となるテクスチャ

struct VS_OUTPUT{
	float4 Pos : POSITION;
	float2 UV : TEXCOORD0;
};

VS_OUTPUT BlurFilterVS( float4 Pos : POSITION, float2 UV : TEXCOORD0 ){
	VS_OUTPUT Out;

	Out.Pos = Pos;
	Out.UV = UV;

	return Out;
}

// X方向にぼかす
float4 BlurFilterPS0( VS_OUTPUT In ) : COLOR0{
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
	float4 p0 = tex2D( s0, In.UV ) * 0.20f;

	float4 p1 = tex2D( s0, texel0 ) * 0.12f;
	float4 p2 = tex2D( s0, texel1 ) * 0.10f;
	float4 p3 = tex2D( s0, texel2 ) * 0.08f;
	float4 p4 = tex2D( s0, texel3 ) * 0.06f;
	float4 p5 = tex2D( s0, texel4 ) * 0.04f;

	float4 p6 = tex2D( s0, texel5 ) * 0.12f;
	float4 p7 = tex2D( s0, texel6 ) * 0.10f;
	float4 p8 = tex2D( s0, texel7 ) * 0.08f;
	float4 p9 = tex2D( s0, texel8 ) * 0.06f;
	float4 p10 = tex2D( s0, texel9 ) * 0.04f;

	// カラーを合成する
	return p0 + p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9 + p10;
}

// Y方向にぼかす
float4 BlurFilterPS1( VS_OUTPUT In ) : COLOR0{
	// テクセルを取得
	float2 texel0 = In.UV + float2( -texV[ 0 ], 0.0f );
	float2 texel1 = In.UV + float2( -texV[ 1 ], 0.0f );
	float2 texel2 = In.UV + float2( -texV[ 2 ], 0.0f );
	float2 texel3 = In.UV + float2( -texV[ 3 ], 0.0f );
	float2 texel4 = In.UV + float2( -texV[ 4 ], 0.0f );

	float2 texel5 = In.UV + float2( texV[ 0 ], 0.0f );
	float2 texel6 = In.UV + float2( texV[ 1 ], 0.0f );
	float2 texel7 = In.UV + float2( texV[ 2 ], 0.0f );
	float2 texel8 = In.UV + float2( texV[ 3 ], 0.0f );
	float2 texel9 = In.UV + float2( texV[ 4 ], 0.0f );

	// カラー情報を取得 重みの合計は1.0f
	float4 p0 = tex2D( s0, In.UV ) * 0.20f;

	float4 p1 = tex2D( s0, texel0 ) * 0.12f;
	float4 p2 = tex2D( s0, texel1 ) * 0.10f;
	float4 p3 = tex2D( s0, texel2 ) * 0.08f;
	float4 p4 = tex2D( s0, texel3 ) * 0.06f;
	float4 p5 = tex2D( s0, texel4 ) * 0.04f;

	float4 p6 = tex2D( s0, texel5 ) * 0.12f;
	float4 p7 = tex2D( s0, texel6 ) * 0.10f;
	float4 p8 = tex2D( s0, texel7 ) * 0.08f;
	float4 p9 = tex2D( s0, texel8 ) * 0.06f;
	float4 p10 = tex2D( s0, texel9 ) * 0.04f;

	// カラーを合成する
	return p0 + p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9 + p10;
}

technique BlurFilterTec{
	pass P0{
		VertexShader = compile vs_1_1 BlurFilterVS();
		PixelShader = compile ps_2_0 BlurFilterPS0();
	}
	pass P1{
		VertexShader = compile vs_1_1 BlurFilterVS();
		PixelShader = compile ps_2_0 BlurFilterPS1();
	}
}
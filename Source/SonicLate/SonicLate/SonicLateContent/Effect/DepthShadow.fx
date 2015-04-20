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
	float4 Pos : POSITION; // �ˉe�ϊ���̍��W
	float4 Color : COLOR0; // �f�B�t���[�Y�F
	float2 MeshUV : TEXCOORD0; // UV
	float4 ZCalcTex : TEXCOORD1; // Z�o�b�t�@�e�N�X�`�����W
	float2 Depth : TEXCOORD2;// �[�x
};

//-----------------------------------------------------------------------------
// ���_�V�F�[�_ �X�L�j���O���Ȃ�
//-----------------------------------------------------------------------------
VSOutPut DepthShadowVS( float4 Pos : POSITION, float3 Normal : NORMAL, float2 MeshUV : TEXCOORD0 ){
	VSOutPut Out = ( VSOutPut )0;

	// �ʏ�̎ˉe�ϊ�
	Out.Pos = mul( Pos, matWorld );
	Out.Pos = mul( Out.Pos, matVP );

	// Z�l��r�̂��߂Ƀ��C�g�ڐ��Ŏˉe�ϊ�
	Out.ZCalcTex = mul( Pos, matWorld );
	Out.ZCalcTex = mul( Out.ZCalcTex, matLightView );
	Out.ZCalcTex = mul( Out.ZCalcTex, matLightProj );

	// ���_�F���Z�o
	float3 n = normalize( mul( Normal, matWorld ) );
	Out.Color = ( max( 0.0, dot( n, -lightDir ) ) + ambient ) * diffuse;
	Out.Color.a = diffuse.a; // �A���t�@�l�̓f�B�t���[�Y�����̂܂܎g��
	
	// ���C�g�ڐ��ŗ��Ȃ�Z�l��0�ɂ���
	float3 l = float3( matLightView._13, matLightView._23, matLightView._33 );
	Out.ZCalcTex.z *= ceil( max( 0.0f, dot( n, l ) ) );

	// UV�͂��̂܂�
	Out.MeshUV = MeshUV;
	
	// �[�x
	Out.Depth.x = Out.Pos.z;
	Out.Depth.y = 0;

	return Out;
}

//-----------------------------------------------------------------------------
// ���_�V�F�[�_ �X�L�j���O����
//-----------------------------------------------------------------------------
VSOutPut DepthShadowSkinedVS( VSInput Input ){
	VSOutPut Out = ( VSOutPut )0;
	
	// ���[���h�ϊ��s����u�����h
	float4x4 matCombWorld = 0.0f;
	for ( int i = 0; i < 4; i++ ){
		matCombWorld += mul( bones[ Input.Indices[ i ] ], Input.Weights[ i ] );
	}

	// �ʏ�̎ˉe�ϊ�
	Out.Pos = mul( Input.Pos, matCombWorld );
	Out.Pos = mul( Out.Pos, matVP );
	Input.Normal = mul( Input.Normal, ( float3x3 )matCombWorld );

	// Z�l��r�̂��߂Ƀ��C�g�ڐ��Ŏˉe�ϊ�
	Out.ZCalcTex = mul( Input.Pos, matCombWorld );
	Out.ZCalcTex = mul( Out.ZCalcTex, matLightView );
	Out.ZCalcTex = mul( Out.ZCalcTex, matLightProj );

	// ���_�F���Z�o
	float3 n = mul( Input.Normal, matCombWorld );
	Out.Color = ( max( 0.0, dot( n, -lightDir ) ) + ambient ) * diffuse;
	Out.Color.a = diffuse.a; // �A���t�@�l�̓f�B�t���[�Y�����̂܂܎g��
	
	// ���C�g�ڐ��ŗ��Ȃ�Z�l��0�ɂ���
	float3 l = float3( matLightView._13, matLightView._23, matLightView._33 );
	Out.ZCalcTex.z *= ceil( max( 0.0f, dot( n, l ) ) );

	// UV�͂��̂܂�
	Out.MeshUV = Input.MeshUV;

	// �[�x
	Out.Depth.x = Out.Pos.z;
	Out.Depth.y = 0;

	return Out;
}

//-----------------------------------------------------------------------------
// �s�N�Z���V�F�[�_
//-----------------------------------------------------------------------------
float4 DepthShadowPS( VSOutPut In ) : COLOR{
	// ����̐[�x
	float zValue = ( In.ZCalcTex.z / In.ZCalcTex.w );
	zValue = min( zValue, 1.0f );

	// �Ή�����e�N�Z�����W
	float2 transTexCoord;
	transTexCoord.x = ( 1.0f + In.ZCalcTex.x / In.ZCalcTex.w ) * 0.5f;
	transTexCoord.y = ( 1.0f - In.ZCalcTex.y / In.ZCalcTex.w ) * 0.5f;

	 // Z�e�N�X�`���̐[�x���Z�o
	float4 t = tex2D( DefSampler, transTexCoord );
	float shadowMapZ = t.r + ( t.g + t.b / 256.0f ) / 256.0f;

	// ��{�F( ���_ + �e�N�X�`�� )
	float4 Out = In.Color * tex2D( MeshSampler, In.MeshUV );
	
	float shade = ( zValue > shadowMapZ ) ? 1 : 0;
	float dark = shadowMapZ * 0.1f * shade;

	Out.rgb -= dark;
	Out.a = In.Color.a; // �A���t�@�l�̓f�B�t���[�Y

	return Out;
}

//-----------------------------------------------------------------------------
// �e�N�j�b�N �ʏ�̃��b�V��
//-----------------------------------------------------------------------------
technique DepthShadowTec{
	pass P0{
		VertexShader = compile vs_2_0 DepthShadowVS();
		PixelShader = compile ps_2_0 DepthShadowPS();
	}
}

//-----------------------------------------------------------------------------
// �e�N�j�b�N �X�L�����b�V��
//-----------------------------------------------------------------------------
technique DepthShadowSkinedTec{
	pass P0{
		VertexShader = compile vs_2_0 DepthShadowSkinedVS();
		PixelShader = compile ps_2_0 DepthShadowPS();
	}
}

//-----------------------------------------------------------------------------
// �t�H�O
//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
// ���_�V�F�[�_ �ʏ�̃��b�V��
//-----------------------------------------------------------------------------
VSOutPut DepthShadowFogVS( float4 Pos : POSITION0, float3 Normal : NORMAL, float2 MeshUV : TEXCOORD0 ){
	VSOutPut Out = ( VSOutPut )0;

	// �ʏ�̎ˉe�ϊ�
	Out.Pos = mul( Pos, matWorld );
	Out.Pos = mul( Out.Pos, matVP );

	// Z�l��r�̂��߂Ƀ��C�g�ڐ��Ŏˉe�ϊ�
	Out.ZCalcTex = mul( Pos, matWorld );
	Out.ZCalcTex = mul( Out.ZCalcTex, matLightView );
	Out.ZCalcTex = mul( Out.ZCalcTex, matLightProj );

	// ���_�F���Z�o
	float3 n = normalize( mul( Normal, matWorld ) );
	Out.Color = ( max( 0.0f, dot( n, -lightDir ) ) + ambient ) * diffuse;
	Out.Color.a = diffuse.a; // �A���t�@�l�̓f�B�t���[�Y�����̂܂܎g��
	
	// ���C�g�ڐ��ŗ��Ȃ�Z�l��0�ɂ���
	float3 l = float3( matLightView._13, matLightView._23, matLightView._33 );
	//Out.ZCalcTex.z *= ceil( max( 0.0f, dot( n, l ) ) );
	
	// �[�x
	Out.Depth.x = Out.Pos.z;
	Out.Depth.y = 0;


	// UV�͂��̂܂�
	Out.MeshUV = MeshUV;

	return Out;
}

//-----------------------------------------------------------------------------
// �s�N�Z���V�F�[�_
//-----------------------------------------------------------------------------
float4 DepthShadowFogPS( VSOutPut In ) : COLOR0{
	// ����̐[�x
	float zValue = ( In.ZCalcTex.z / In.ZCalcTex.w );
	zValue = min( zValue, 1.0f );

	// �Ή�����e�N�Z�����W
	float2 transTexCoord;
	transTexCoord.x = ( 1.0f + In.ZCalcTex.x / In.ZCalcTex.w ) * 0.5f;
	transTexCoord.y = ( 1.0f - In.ZCalcTex.y / In.ZCalcTex.w ) * 0.5f;

	 // Z�e�N�X�`���̐[�x���Z�o
	float4 t = tex2D( DefSampler, transTexCoord );
	float shadowMapZ = t.r + ( t.g + t.b / 256.0f ) / 256.0f;

	// ��{�F( ���_ + �e�N�X�`�� )
	float4 Out = In.Color * tex2D( MeshSampler, In.MeshUV );
	float fog = max( 0.0f, min( 1.0f, fogCoord.x + In.Depth * fogCoord.y ) );
	Out = Out * ( 1.0f - fog ) + fogColor * fog;

	float shade = ( zValue > shadowMapZ ) ? 1 : 0;
	float dark = shadowMapZ * 0.1f * shade;

	Out.rgb -= dark;
	Out.a = In.Color.a; // �A���t�@�l�̓f�B�t���[�Y

	return Out;
}

//-----------------------------------------------------------------------------
// �e�N�j�b�N �ʏ�̃��b�V��
//-----------------------------------------------------------------------------
technique DepthShadowFogTec{
	pass P0{
		VertexShader = compile vs_2_0 DepthShadowFogVS();
		PixelShader = compile ps_2_0 DepthShadowFogPS();
	}
}
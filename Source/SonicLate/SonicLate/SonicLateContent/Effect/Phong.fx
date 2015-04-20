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
	float4 Pos : POSITION; // �ˉe�ϊ���̍��W
	float4 Color : COLOR0; // �f�B�t���[�Y�F
	float2 MeshUV : TEXCOORD0; // UV
	float3 RefValue : TEXCOORD1; // ���ʔ��ˌ�
};

//-----------------------------------------------------------------------------
// ���_�V�F�[�_ �ʏ�̃��b�V��
//-----------------------------------------------------------------------------
VSOutPut PhongVS( float4 Pos : POSITION, float3 Normal : NORMAL, float2 MeshUV : TEXCOORD0 ){
	VSOutPut Out = ( VSOutPut )0;

	// �ʏ�̎ˉe�ϊ�
	Out.Pos = mul( Pos, matWorld );
	Out.Pos = mul( Out.Pos, matVP );

	// ���_�F���Z�o
	float3 eye = normalize( eyePos - Pos.xyz ); // ���_���王�_�ւ̃x�N�g��
	float3 light = -lightDir;
	float3 n = normalize( mul( Normal.xyz, matWorld ) );
	float3 r = -eye + 2.0f * dot( n, eye ) * n; // ���˃x�N�g��

	Out.Color = diffuse * max( ambient, dot( n, light ) );
	Out.Color.a = diffuse.a; // �A���t�@�l�̓f�B�t���[�Y�����̂܂܎g��
	Out.RefValue = pow( max( 0, dot( light, r ) ), 10 );

	// UV�͂��̂܂�
	Out.MeshUV = MeshUV;

	return Out;
}

//-----------------------------------------------------------------------------
// ���_�V�F�[�_ �X�L�����b�V��
//-----------------------------------------------------------------------------
VSOutPut PhongSkinedVS( VSInput Input ){
	VSOutPut Out = ( VSOutPut )0;
	
	// ���[���h�ϊ��s����u�����h
	float4x4 matCombWorld = 0.0f;
	for ( int i = 0; i < 4; i++ ){
		matCombWorld += mul( bones[ Input.Indices[ i ] ], Input.Weights[ i ] );
	}

	// �ʏ�̎ˉe�ϊ�
	Out.Pos = mul( Input.Pos, matCombWorld );
	Out.Pos = mul( Out.Pos, matVP );

	// ���_�F���Z�o
	float3 eye = normalize( eyePos - Input.Pos.xyz ); // ���_���王�_�ւ̃x�N�g��
	float3 light = -lightDir;
	float3 n = normalize( mul( Input.Normal, ( float3x3 )matCombWorld ) );
	float3 r = -eye + 2.0f * dot( n, eye ) * n; // ���˃x�N�g��
	
	Out.Color = diffuse * max( ambient, dot( n, light ) );
	Out.Color.a = diffuse.a; // �A���t�@�l�̓f�B�t���[�Y�����̂܂܎g��
	Out.RefValue = pow( max( 0, dot( light, r ) ), 10 );

	// UV�͂��̂܂�
	Out.MeshUV = Input.MeshUV;

	return Out;
}

//-----------------------------------------------------------------------------
// �s�N�Z���V�F�[�_
//-----------------------------------------------------------------------------
float4 PhongPS( VSOutPut In ) : COLOR{
	// ���_�F + �e�N�X�`��
	float4 Out = In.Color * tex2D( MeshSampler, In.MeshUV );
	Out.xyz += In.RefValue;
	
	// �A���t�@�l�̓f�B�t���[�Y
	Out.a = In.Color.a;

	return Out;
}

//-----------------------------------------------------------------------------
// �e�N�j�b�N �ʏ�̃��b�V��
//-----------------------------------------------------------------------------
technique PhongTec{
	pass P0{
		VertexShader = compile vs_2_0 PhongVS();
		PixelShader = compile ps_2_0 PhongPS();
	}
}

//-----------------------------------------------------------------------------
// �e�N�j�b�N �X�L�����b�V��
//-----------------------------------------------------------------------------
technique PhongSkinedTec{
	pass P0{
		VertexShader = compile vs_2_0 PhongSkinedVS();
		PixelShader = compile ps_2_0 PhongPS();
	}
}
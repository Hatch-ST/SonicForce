float4x4 matWorld;
float4x4 matVP;
float4x4 bones[ 60 ];
float3 lightDir;
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
	float4 Pos : POSITION; // �ˉe�ϊ���̍��W
	float4 Color : COLOR0; // �f�B�t���[�Y�F
	float2 MeshUV : TEXCOORD0; // UV
};


//-----------------------------------------------------------------------------
// ���_�V�F�[�_ �ʏ�̃��b�V��
//-----------------------------------------------------------------------------
VSOutPut BasicVS( float4 Pos : POSITION0, float3 Normal : NORMAL, float2 MeshUV : TEXCOORD0 ){
	VSOutPut Out = ( VSOutPut )0;

	// �ʏ�̎ˉe�ϊ�
	Out.Pos = mul( Pos, matWorld );
	Out.Pos = mul( Out.Pos, matVP );

	// ���_�F���Z�o
	float3 n = normalize( mul( Normal, matWorld ) );
	Out.Color = ( max( 0.0, dot( n, -lightDir ) ) + ambient ) * diffuse;
	Out.Color.a = diffuse.a; // �A���t�@�l�̓f�B�t���[�Y�����̂܂܎g��

	// UV�͂��̂܂�
	Out.MeshUV = MeshUV;

	return Out;
}

//-----------------------------------------------------------------------------
// ���_�V�F�[�_ �X�L�����b�V��
//-----------------------------------------------------------------------------
VSOutPut BasicSkinVS( VSInput Input ){
	VSOutPut Out = ( VSOutPut )0;

	// ���[���h�ϊ��s����u�����h
	float4x4 matCombWorld = 0.0f;
	for ( int i = 0; i < 4; i++ ){
		matCombWorld += mul( bones[ Input.Indices[ i ] ], Input.Weights[ i ] );
	}

	Out.Pos = mul( Input.Pos, matCombWorld );
	Out.Pos = mul( Out.Pos, matVP );
	
	// ���_�F���Z�o
	float3 n = mul( Input.Normal, matCombWorld );
	Out.Color = ( max( 0.0, dot( n, -lightDir ) ) + ambient ) * diffuse;
	Out.Color.a = diffuse.a; // �A���t�@�l�̓f�B�t���[�Y�����̂܂܎g��

	// UV�͂��̂܂�
	Out.MeshUV = Input.MeshUV;

	return Out;
}

//-----------------------------------------------------------------------------
// �s�N�Z���V�F�[�_
//-----------------------------------------------------------------------------
float4 BasicPS( VSOutPut In ) : COLOR0{
	// ���_�F * �e�N�X�`��
	float4 Out = In.Color * tex2D( MeshSampler, In.MeshUV );

	return Out;
}

//-----------------------------------------------------------------------------
// �e�N�j�b�N �ʏ�̃��b�V��
//-----------------------------------------------------------------------------
technique BasicTec{
	pass P0{
		VertexShader = compile vs_2_0 BasicVS();
		PixelShader = compile ps_2_0 BasicPS();
	}
}

//-----------------------------------------------------------------------------
// �e�N�j�b�N �X�L�����b�V��
//-----------------------------------------------------------------------------
technique SkinnedBasicTec{
	pass P0{
		VertexShader = compile vs_2_0 BasicSkinVS();
		PixelShader = compile ps_2_0 BasicPS();
	}
}

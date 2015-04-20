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
	float4 Pos : POSITION; // �ˉe�ϊ���̍��W
	float4 Color : COLOR0; // �f�B�t���[�Y�F
	float2 MeshUV : TEXCOORD0; // UV
};


//-----------------------------------------------------------------------------
// ���_�V�F�[�_ �ʏ�̃��b�V��
//-----------------------------------------------------------------------------
VSOutPut DiffuseVS( float4 Pos : POSITION0, float3 Normal : NORMAL, float2 MeshUV : TEXCOORD0 ){
	VSOutPut Out = ( VSOutPut )0;

	// �ʏ�̎ˉe�ϊ�
	Out.Pos = mul( Pos, matWorld );
	Out.Pos = mul( Out.Pos, matVP );

	Out.Color = diffuse;
	Out.MeshUV = MeshUV;

	return Out;
}

//-----------------------------------------------------------------------------
// ���_�V�F�[�_ �X�L�����b�V��
//-----------------------------------------------------------------------------
VSOutPut DiffuseSkinVS( VSInput Input ){
	VSOutPut Out = ( VSOutPut )0;

	// ���[���h�ϊ��s����u�����h
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
// �s�N�Z���V�F�[�_
//-----------------------------------------------------------------------------
float4 DiffusePS( VSOutPut In ) : COLOR0{
	// ���_�F * �e�N�X�`��
	float4 Out = In.Color * tex2D( MeshSampler, In.MeshUV );

	return Out;
}

//-----------------------------------------------------------------------------
// �e�N�j�b�N �ʏ�̃��b�V��
//-----------------------------------------------------------------------------
technique DiffuseTec{
	pass P0{
		VertexShader = compile vs_2_0 DiffuseVS();
		PixelShader = compile ps_2_0 DiffusePS();
	}
}

//-----------------------------------------------------------------------------
// �e�N�j�b�N �X�L�����b�V��
//-----------------------------------------------------------------------------
technique SkinnedDiffuseTec{
	pass P0{
		VertexShader = compile vs_2_0 DiffuseSkinVS();
		PixelShader = compile ps_2_0 DiffusePS();
	}
}

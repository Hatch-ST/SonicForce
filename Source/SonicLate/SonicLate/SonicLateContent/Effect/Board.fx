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
	float4 Pos : POSITION; // �ˉe�ϊ���̍��W
	float2 MeshUV : TEXCOORD0; // UV
};

struct VSOutPutUseColor{
	float4 Pos : POSITION; // �ˉe�ϊ���̍��W
	float4 Color : COLOR0;
	float2 MeshUV : TEXCOORD0; // UV
};

//-----------------------------------------------------------------------------
// ���_�V�F�[�_
//-----------------------------------------------------------------------------
VSOutPut BoardVS( float4 Pos : POSITION, float2 MeshUV : TEXCOORD0 ){
	VSOutPut Out = ( VSOutPut )0;

	// �ʏ�̎ˉe�ϊ�
	Out.Pos = mul( Pos, matWorld );
	Out.Pos = mul( Out.Pos, matVP );

	// UV�͂��̂܂�
	Out.MeshUV = MeshUV;

	return Out;
}


//-----------------------------------------------------------------------------
// �s�N�Z���V�F�[�_
//-----------------------------------------------------------------------------
float4 BoardPS( float2 MeshUV : TEXCOORD0 ) : COLOR{
	// �f�B�t���[�Y�F * �e�N�X�`��
	float4 Out = diffuse * tex2D( MeshSampler, MeshUV );

	return Out;
}

//-----------------------------------------------------------------------------
// �e�N�j�b�N
//-----------------------------------------------------------------------------
technique BoardTec{
	pass P0{
		VertexShader = compile vs_2_0 BoardVS();
		PixelShader = compile ps_2_0 BoardPS();
	}
}



//-----------------------------------------------------------------------------
// ���_�V�F�[�_ �f�B�t���[�Y�F���g��
//-----------------------------------------------------------------------------
VSOutPutUseColor BoardUseColorVS( float4 Pos : POSITION, float4 Color : COLOR0, float2 MeshUV : TEXCOORD0 ){
	VSOutPutUseColor Out = ( VSOutPutUseColor )0;

	// �ʏ�̎ˉe�ϊ�
	Out.Pos = mul( Pos, matWorld );
	Out.Pos = mul( Out.Pos, matVP );

	Out.Color = Color;

	// UV�͂��̂܂�
	Out.MeshUV = MeshUV;

	return Out;
}

//-----------------------------------------------------------------------------
// �s�N�Z���V�F�[�_ �f�B�t���[�Y�F���g��
//-----------------------------------------------------------------------------
float4 BoardUseColorPS( float4 Color : COLOR0, float2 MeshUV : TEXCOORD0 ) : COLOR{
	// ���_�F * �e�N�X�`��
	float4 Out = Color * tex2D( MeshSampler, MeshUV );

	return Out;
}

//-----------------------------------------------------------------------------
// �e�N�j�b�N �f�B�t���[�Y�F���g��
//-----------------------------------------------------------------------------
technique BoardUseColorTec{
	pass P0{
		VertexShader = compile vs_2_0 BoardUseColorVS();
		PixelShader = compile ps_2_0 BoardUseColorPS();
	}
}
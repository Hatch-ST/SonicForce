float4x4 matWorld;
float4x4 matVP;
float4x4 bones[ 60 ];

struct VSInput{
	float4 Pos : POSITION0;
	int4 Indices : BLENDINDICES0;
	float4 Weights : BLENDWEIGHT0;
};

struct VSOutPut{
	float4 Pos : POSITION; // �ˉe�ϊ����W
	float4 ShadowMapTex : TEXCOORD0; // Z�o�b�t�@�e�N�X�`��
};

//-----------------------------------------------------------------------------
// ���_�V�F�[�_ �X�L�j���O���Ȃ�
//-----------------------------------------------------------------------------
VSOutPut DepthMapVS( float4 Pos : POSITION ){
	VSOutPut Out = ( VSOutPut )0;

	// �ʏ�̎ˉe�ϊ�
	Out.Pos = mul( Pos, matWorld );
	Out.Pos = mul( Out.Pos, matVP );

	// �e�N�X�`�����W�𒸓_�ɍ��킹��
	Out.ShadowMapTex = Out.Pos;

	return Out;
}

//-----------------------------------------------------------------------------
// ���_�V�F�[�_ �X�L�j���O����
//-----------------------------------------------------------------------------
VSOutPut DepthMapSkinedVS( VSInput Input ){
	VSOutPut Out = ( VSOutPut )0;

	// ���[���h�ϊ��s����u�����h
	float4x4 matCombWorld = 0.0f;
	for ( int i = 0; i < 4; i++ ){
		matCombWorld += mul( bones[ Input.Indices[ i ] ], Input.Weights[ i ] );
	}
	
	// �ʏ�̎ˉe�ϊ�
	Out.Pos = mul( Input.Pos, matCombWorld );
	Out.Pos = mul( Out.Pos, matVP );

	// �e�N�X�`�����W�𒸓_�ɍ��킹��
	Out.ShadowMapTex = Out.Pos;

	return Out;
}

//-----------------------------------------------------------------------------
// �s�N�Z���V�F�[�_
//-----------------------------------------------------------------------------
float4 DepthMapPS( float4 ShadowMapTex : TEXCOORD0 ) : COLOR{
	float z = ShadowMapTex.z / ShadowMapTex.w;
	float4 Depth = float4( 0, 0, 256.0f, 256.0f );
	Depth.g = modf( z * 256.0f, Depth.r ); // ��������r�Ɋi�[
	Depth.b *= modf( Depth.g * 256.0f, Depth.g ); // ��������g�Ɋi�[
	Depth /= 256.0f; // ���K��

	return Depth;
}

//-----------------------------------------------------------------------------
// �e�N�j�b�N �ʏ�̃��b�V��
//-----------------------------------------------------------------------------
technique DepthMapTec{
	pass P0{
		VertexShader = compile vs_2_0 DepthMapVS();
		PixelShader = compile ps_2_0 DepthMapPS();
	}
}

//-----------------------------------------------------------------------------
// �e�N�j�b�N �X�L�����b�V��
//-----------------------------------------------------------------------------
technique DepthMapSkinedTec{
	pass P0{
		VertexShader = compile vs_2_0 DepthMapSkinedVS();
		PixelShader = compile ps_2_0 DepthMapPS();
	}
}
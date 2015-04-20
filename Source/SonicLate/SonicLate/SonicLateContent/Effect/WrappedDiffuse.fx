float4x4 matWorld;
float4x4 matVP;
float4x4 bones[ 60 ];
float3 lightDir;
float4 ambient;
float4 diffuse;
float3 viewVec;
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
	float4 Back : COLOR1;
	float2 MeshUV : TEXCOORD0; // UV
};

//-----------------------------------------------------------------------------
// ���_�V�F�[�_ �ʏ�̃��b�V��
//-----------------------------------------------------------------------------
VSOutPut WrappedVS( float4 Pos : POSITION, float3 Normal : NORMAL, float2 MeshUV : TEXCOORD0 ){
	VSOutPut Out = ( VSOutPut )0;

	// �ʏ�̎ˉe�ϊ�
	Out.Pos = mul( Pos, matWorld );
	Out.Pos = mul( Out.Pos, matVP );
	
	float3 n = normalize( mul( Normal.xyz, matWorld ) );
	float cos = dot( n, lightDir );

	// ��{�F
	float4 color = max( ambient, -cos );

	// ��荞��
	float4 wrap = max( 0.0f, cos ) * 0.2f;

	// �^���U����
	float t = min( 0.8f, max( 0.0f, 1.0f - dot( n, -viewVec ) ) );
	Out.Back =  pow( t, 3 );

	Out.Color = diffuse * min( color + wrap, 1.0f );
	Out.Color.a = diffuse.a; // �A���t�@�l�̓f�B�t���[�Y�����̂܂܎g��

	// UV�͂��̂܂�
	Out.MeshUV = MeshUV;

	return Out;
}

//-----------------------------------------------------------------------------
// ���_�V�F�[�_ �X�L�����b�V��
//-----------------------------------------------------------------------------
VSOutPut WrappedSkinedVS( VSInput Input ){
	VSOutPut Out = ( VSOutPut )0;
		
	// ���[���h�ϊ��s����u�����h
	float4x4 matCombWorld = 0.0f;
	for ( int i = 0; i < 4; i++ ){
		matCombWorld += mul( bones[ Input.Indices[ i ] ], Input.Weights[ i ] );
	}

	// �ʏ�̎ˉe�ϊ�
	Out.Pos = mul( Input.Pos, matCombWorld );
	Out.Pos = mul( Out.Pos, matVP );

	float3 n = normalize( mul( Input.Normal.xyz, matCombWorld ) );
	float cos = dot( n, lightDir );

	// ��{�F
	float4 color = max( ambient, -cos );

	// ��荞��
	float4 wrap = max( 0.0f, cos ) * 0.2f;

	// �^���U����
	float t = min( 0.8f, max( 0.0f, 1.0f - dot( n, -viewVec ) ) );
	Out.Back =  pow( t, 3 );

	Out.Color = diffuse * min( color + wrap, 1.0f );
	Out.Color.a = diffuse.a; // �A���t�@�l�̓f�B�t���[�Y�����̂܂܎g��

	// UV�͂��̂܂�
	Out.MeshUV = Input.MeshUV;

	return Out;
}

//-----------------------------------------------------------------------------
// �s�N�Z���V�F�[�_
//-----------------------------------------------------------------------------
float4 WrappedPS( VSOutPut In ) : COLOR{
	// ���_�F + �e�N�X�`��
	float4 Out = In.Color * tex2D( MeshSampler, In.MeshUV ) + In.Back;
	
	// �A���t�@�l�̓f�B�t���[�Y
	//Out.a = In.Color.a;

	return Out;
}

//-----------------------------------------------------------------------------
// �e�N�j�b�N �ʏ�̃��b�V��
//-----------------------------------------------------------------------------
technique WrappedTec{
	pass P0{
		VertexShader = compile vs_2_0 WrappedVS();
		PixelShader = compile ps_2_0 WrappedPS();
	}
}

//-----------------------------------------------------------------------------
// �e�N�j�b�N �X�L�����b�V��
//-----------------------------------------------------------------------------
technique WrappedSkinedTec{
	pass P0{
		VertexShader = compile vs_2_0 WrappedSkinedVS();
		PixelShader = compile ps_2_0 WrappedPS();
	}
}
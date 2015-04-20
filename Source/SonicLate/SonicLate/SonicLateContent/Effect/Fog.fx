float4x4 matWorld;
float4x4 matVP;
float4x4 bones[ 60 ];
float3 lightDir;
float4 ambient;
float4 diffuse;
float4 fogColor;
float2 fogCoord;
float3 cameraPosition;
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
	float2 Depth : TEXCOORD1;// �[�x
};


//-----------------------------------------------------------------------------
// ���_�V�F�[�_ �ʏ�̃��b�V��
//-----------------------------------------------------------------------------
VSOutPut FogVS( float4 Pos : POSITION0, float3 Normal : NORMAL, float2 MeshUV : TEXCOORD0 ){
	VSOutPut Out = ( VSOutPut )0;

	// �ʏ�̎ˉe�ϊ�
	Out.Pos = mul( Pos, matWorld );
	Out.Pos = mul( Out.Pos, matVP );

	// ���_�F���Z�o
	float3 n = normalize( mul( Normal, matWorld ) );
	Out.Color = ( max( 0.0, dot( n, -lightDir ) ) + ambient ) * diffuse;
	Out.Color.a = diffuse.a; // �A���t�@�l�̓f�B�t���[�Y�����̂܂܎g��
	
	// �t�H�O
	Out.Depth.x = Out.Pos.z; // �[�x
	Out.Depth.y = length( Pos - cameraPosition ); // ����

	// UV�͂��̂܂�
	Out.MeshUV = MeshUV;

	return Out;
}

//-----------------------------------------------------------------------------
// ���_�V�F�[�_ �X�L�����b�V��
//-----------------------------------------------------------------------------
VSOutPut FogSkinVS( VSInput Input ){
	VSOutPut Out = ( VSOutPut )0;

	// ���[���h�ϊ��s����u�����h
	float4x4 matCombWorld = 0.0f;
	for ( int i = 0; i < 4; i++ ){
		matCombWorld += mul( bones[ Input.Indices[ i ] ], Input.Weights[ i ] );
	}

	Out.Pos = mul( Input.Pos, matCombWorld );
	Out.Pos = mul( Out.Pos, matVP );
	Input.Normal = mul( Input.Normal, ( float3x3 )matCombWorld );
	
	// ���_�F���Z�o
	float3 n = mul( Input.Normal, matCombWorld );
	Out.Color = ( max( 0.0, dot( n, -lightDir ) ) + ambient ) * diffuse;
	Out.Color.a = diffuse.a; // �A���t�@�l�̓f�B�t���[�Y�����̂܂܎g��
	
	// �t�H�O
	Out.Depth.x = Out.Pos.z; // �[�x
	Out.Depth.y = length( Input.Pos - cameraPosition ); // ����

	// UV�͂��̂܂�
	Out.MeshUV = Input.MeshUV;

	return Out;
}

//-----------------------------------------------------------------------------
// �s�N�Z���V�F�[�_
//-----------------------------------------------------------------------------
float4 FogPS( VSOutPut In ) : COLOR0{
	float4 color = In.Color * tex2D( MeshSampler, In.MeshUV );
	float fog = max( 0.0f, min( 1.0f, fogCoord.y + In.Depth * fogCoord.y ) );
	float4 Out = color * ( 1.0f - fog ) + fogColor * fog;
	Out.a = color.a;

	return Out;
}

//-----------------------------------------------------------------------------
// �e�N�j�b�N �ʏ�̃��b�V��
//-----------------------------------------------------------------------------
technique FogTec{
	pass P0{
		VertexShader = compile vs_2_0 FogVS();
		PixelShader = compile ps_2_0 FogPS();
	}
}

//-----------------------------------------------------------------------------
// �e�N�j�b�N �X�L�����b�V��
//-----------------------------------------------------------------------------
technique SkinnedFogTec{
	pass P0{
		VertexShader = compile vs_2_0 FogSkinVS();
		PixelShader = compile ps_2_0 FogPS();
	}
}

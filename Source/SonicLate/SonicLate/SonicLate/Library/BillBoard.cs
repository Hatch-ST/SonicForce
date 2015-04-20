using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SonicLate{
	class BillBoard{
		private static Effect mEffect = null;
		private static EffectParameter mTextureParam = null;
		private static EffectParameter mDiffuseParam = null;
		private static EffectParameter mMatVPParam = null;
		private static EffectParameter mMatWorldParam = null;

		private VertexBuffer mVertexBuffer = null;
		private List<Texture2D> mTexture = new List<Texture2D>();

		private static GraphicsDevice mDevice = null;
		
		/// <summary>
		/// アルファブレンド設定
		/// </summary>
		private static BlendState mBlendState = new BlendState(){
			ColorSourceBlend = Blend.SourceAlpha,
			ColorDestinationBlend = Blend.InverseSourceAlpha,
			AlphaSourceBlend = Blend.SourceAlpha,
			AlphaDestinationBlend = Blend.InverseSourceAlpha,
		};

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public BillBoard(){
			// 頂点バッファを初期化
			InitializeVertex();
		}
		
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="content">コンテントマネージャ</param>
		/// <param name="fileName">テクスチャのファイル名</param>
		public BillBoard( ContentManager content, string fileName ){
			// 頂点バッファを初期化
			InitializeVertex();

			// テクスチャを読み込む
			mTexture.Add( content.Load<Texture2D>( fileName ) );
		}

		/// <summary>
		/// コンストラクタ 複数枚のテクスチャを読み込む
		/// </summary>
		/// <param name="content">コンテントマネージャ</param>
		/// <param name="fileName">テクスチャのファイル名 ( "_番号"を除く )</param>
		/// <param name="numTexture">テクスチャの枚数</param>
		public BillBoard( ContentManager content, string fileName, int numTexture ){
			// 頂点バッファを初期化
			InitializeVertex();

			// テクスチャを読み込む
			for ( int i = 0; i < numTexture; i++ ){
				mTexture.Add( content.Load<Texture2D>( fileName + "_" + Convert.ToString( i + 1 ) ) );
			}
		}

		/// <summary>
		/// 頂点バッファを初期化する
		/// </summary>
		private void InitializeVertex(){
			// バッファを生成
			mVertexBuffer = new VertexBuffer( mDevice, typeof( VertexPositionTexture ), 4, BufferUsage.None );

			// バッファに頂点を登録
			VertexPositionTexture[] vertices = new VertexPositionTexture[ 4 ];
			vertices[ 0 ] = new VertexPositionTexture( new Vector3( -0.5f, 0.5f, 0.0f ), new Vector2( 0.0f, 0.0f ) );
			vertices[ 1 ] = new VertexPositionTexture( new Vector3( 0.5f, 0.5f, 0.0f ), new Vector2( 1.0f, 0.0f ) );
			vertices[ 2 ] = new VertexPositionTexture( new Vector3( -0.5f, -0.5f, 0.0f ), new Vector2( 0.0f, 1.0f ) );
			vertices[ 3 ] = new VertexPositionTexture( new Vector3( 0.5f, -0.5f, 0.0f ), new Vector2( 1.0f, 1.0f ) );

			mVertexBuffer.SetData( vertices );
		}

		/// <summary>
		/// レンダリングを行う
		/// </summary>
		/// <param name="camera">カメラ情報</param>
		/// <param name="texture">表示するテクスチャ</param>
		/// <param name="states">モデルの状態</param>
		/// <param name="alpha">アルファ値</param>
		public void Render( Camera camera, Texture2D texture, ModelStates states, float alpha ){
			Render( camera, texture, states, alpha, mBlendState );
		}
		
		/// <summary>
		/// レンダリングを行う
		/// </summary>
		/// <param name="camera">カメラ情報</param>
		/// <param name="texture">表示するテクスチャ</param>
		/// <param name="states">モデルの状態</param>
		/// <param name="alpha">アルファ値</param>
		/// <param name="blendState">ブレンド条件</param>
		public void Render( Camera camera, Texture2D texture, ModelStates states, float alpha, BlendState blendState ){
			// テクスチャを登録
			mTextureParam.SetValue( texture );

			// ディフューズ色を登録
			mDiffuseParam.SetValue( new Vector4( 1.0f, 1.0f, 1.0f, alpha ) );

			// ビュー変換行列×射影変換行列を登録
			Matrix matVP = camera.View * camera.Projection;
			mMatVPParam.SetValue( matVP );

			// モデルの状態をコピー
			ModelStates t = states.CopyPositionAngleScale();

			// ビュー変換行列の逆行列から回転行列を作成
			Matrix matRota;
			if ( states.Angle.LengthSquared() == 0.0f ){
				matRota = Matrix.Invert( camera.View );
				matRota.M14 = matRota.M24 = matRota.M34 = 0.0f;
				matRota.M41 = matRota.M42 = matRota.M43 = 0.0f;
				matRota.M44 = 1.0f;
			}else{
				matRota = Matrix.CreateRotationZ( t.AngleZ ) * Matrix.CreateRotationZ( t.AngleY ) * Matrix.CreateRotationZ( t.AngleX );
			}

			// 行列を合成
			Matrix matTrans = Matrix.CreateTranslation( t.Position );
			Matrix matScale = Matrix.CreateScale( t.Scale );
			Matrix worldMat = matScale * matRota * matTrans;

			// ワールド変換行列を登録
			mMatWorldParam.SetValue( worldMat );

			// 頂点バッファを登録
			mDevice.SetVertexBuffer( mVertexBuffer );

			// ブレンド条件を登録
			mDevice.BlendState = blendState;

			// 描画
			foreach ( EffectPass pass in mEffect.CurrentTechnique.Passes ){
				pass.Apply();
				mDevice.DrawPrimitives( PrimitiveType.TriangleStrip, 0, 2 );
			}

			// ブレンド条件の復帰
			mDevice.BlendState = BlendState.Opaque;
		}

		/// <summary>
		/// レンダリングを行う
		/// </summary>
		/// <param name="camera">カメラ情報</param>
		/// <param name="states">モデルの状態</param>
		/// <param name="alpha">アルファ値</param>
		public void Render( Camera camera, ModelStates states, float alpha ){
			Render( camera, mTexture[ 0 ], states, alpha, mBlendState );
		}
		
		/// <summary>
		/// レンダリングを行う
		/// </summary>
		/// <param name="camera">カメラ情報</param>
		/// <param name="states">モデルの状態</param>
		/// <param name="alpha">アルファ値</param>
		/// <param name="blendState">ブレンド条件</param>
		public void Render( Camera camera, ModelStates states, float alpha, BlendState blendState ){
			Render( camera, mTexture[ 0 ], states, alpha, blendState );
		}
	
		/// <summary>
		/// レンダリングを行う
		/// </summary>
		/// <param name="camera">カメラ情報</param>
		/// <param name="states">モデルの状態</param>
		/// <param name="alpha">アルファ値</param>
		/// <param name="textureIndex">テクスチャの番号</param>
		public void Render( Camera camera, ModelStates states, float alpha, int textureIndex ){
			if ( textureIndex >= mTexture.Count ) return;
			Render( camera, mTexture[ textureIndex ], states, alpha, mBlendState );
		}
		
		/// <summary>
		/// レンダリングを行う
		/// </summary>
		/// <param name="camera">カメラ情報</param>
		/// <param name="states">モデルの状態</param>
		/// <param name="alpha">アルファ値</param>
		/// <param name="textureIndex">テクスチャの番号</param>
		/// <param name="blendState">ブレンド条件</param>
		public void Render( Camera camera, ModelStates states, float alpha, int textureIndex, BlendState blendState ){
			if ( textureIndex >= mTexture.Count ) return;
			Render( camera, mTexture[ textureIndex ], states, alpha, blendState );
		}

		
		/// <summary>
		/// ビュー行列の逆行列
		/// </summary>
		private Matrix mViewInverseMatrix = Matrix.Identity;

		/// <summary>
		/// レンダリングを行う BillBoard.Begin後専用
		/// </summary>
		public void Render( ModelStates states, float alpha, int textureIndex ){
			// ディフューズ色を登録
			mDiffuseParam.SetValue( new Vector4( 1.0f, 1.0f, 1.0f, alpha ) );

			// 行列を合成
			Matrix matTrans = Matrix.CreateTranslation( states.Position );
			Matrix matScale = Matrix.CreateScale( states.Scale );
			Matrix worldMat = matScale * mViewInverseMatrix * matTrans;

			// ワールド変換行列を登録
			mMatWorldParam.SetValue( worldMat );

			// テクスチャを登録
			mTextureParam.SetValue( mTexture[ textureIndex ] );

			// 描画
			foreach ( EffectPass pass in mEffect.CurrentTechnique.Passes ){
			    pass.Apply();
			    mDevice.DrawPrimitives( PrimitiveType.TriangleStrip, 0, 2 );
			}
		}

		/// <summary>
		/// 描画開始処理を行う 
		/// </summary>
		public void Begin( Camera camera, BlendState blendState ){
			// ビュー変換行列×射影変換行列を登録
			Matrix matVP = camera.View * camera.Projection;
			mMatVPParam.SetValue( matVP );

			// ビュー変換行列の逆行列から回転行列を作成
			mViewInverseMatrix = Matrix.Invert( camera.View );
			mViewInverseMatrix.M14 = mViewInverseMatrix.M24 = mViewInverseMatrix.M34 = 0.0f;
			mViewInverseMatrix.M41 = mViewInverseMatrix.M42 = mViewInverseMatrix.M43 = 0.0f;
			mViewInverseMatrix.M44 = 1.0f;

			// 頂点バッファを登録
			mDevice.SetVertexBuffer( mVertexBuffer );

			// ブレンド条件を登録
			mDevice.BlendState = ( blendState != null ) ? blendState : mBlendState;
		}

		/// <summary>
		/// 描画終了処理を行う
		/// </summary>
		public void End(){
			// ブレンド条件の復帰
			mDevice.BlendState = BlendState.Opaque;
		}

		/// <summary>
		/// テクスチャの枚数
		/// </summary>
		public int NumTexture{
			get { return mTexture.Count; }
		}

		/// <summary>
		/// テクスチャ
		/// </summary>
		public Texture2D Texture{
			get { return mTexture[ 0 ]; }
		}

		/// <summary>
		/// 初期化処理を行う
		/// </summary>
		/// <param name="device">グラフィックスデバイス</param>
		/// <param name="content">コンテントマネージャ</param>
		public static void Initialize( GraphicsDevice device, ContentManager content ){
			// デバイスへの参照を保存
			mDevice = device;

			// エフェクトを読み込む
			mEffect = content.Load<Effect>( "Effect/Board" );

			// テクニックを登録
			mEffect.CurrentTechnique = mEffect.Techniques[ "BoardTec" ];

			// パラメータを取得
			mTextureParam = mEffect.Parameters[ "texMesh" ];
			mDiffuseParam = mEffect.Parameters[ "diffuse" ];
			mMatVPParam = mEffect.Parameters[ "matVP" ];
			mMatWorldParam = mEffect.Parameters[ "matWorld" ];
		}

		/// <summary>
		/// レンダリングを行う
		/// </summary>
		public static void Render( VertexBuffer vertexBuffer, BlendState blendState, Texture2D texture, Vector4 diffuse, Matrix viewProj, Matrix world ){
			// テクスチャを登録
			mTextureParam.SetValue( texture );

			// ディフューズ色を登録
			mDiffuseParam.SetValue( diffuse );

			// ビュー変換行列×射影変換行列を登録
			mMatVPParam.SetValue( viewProj );

			// ワールド変換行列を登録
			mMatWorldParam.SetValue( world );

			// 頂点バッファを登録
			mDevice.SetVertexBuffer( vertexBuffer );

			// ブレンド条件を登録
			mDevice.BlendState = blendState;

			foreach ( EffectPass pass in mEffect.CurrentTechnique.Passes ){
				pass.Apply();
				mDevice.DrawPrimitives( PrimitiveType.TriangleStrip, 0, vertexBuffer.VertexCount );
			}
		}

		/// <summary>
		/// 頂点色を有効にするかどうかを登録する
		/// </summary>
		public static bool VertexColorEnable{
			set {
				if ( value ){
					mEffect.CurrentTechnique = mEffect.Techniques[ "BoardUseColorTec" ];
				}else{
					mEffect.CurrentTechnique = mEffect.Techniques[ "BoardTec" ];
				}
			}
		}
	}
}

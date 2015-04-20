using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SonicLate{
	/// <summary>
	/// スプライト描画クラス
	/// </summary>
	class SpriteBoard {
		#region フィールド

		/// <summary>エフェクト</summary>
		private static Effect mEffect = null;

		/// <summary>テクスチャのパラメータ</summary>
		private static EffectParameter mTextureParam = null;

		/// <summary>ワールド変換×ビュー変換×射影変換行列のパラメータ</summary>
		private static EffectParameter mMatWVPParam = null;

		/// <summary>ディフューズ色のパラメータ</summary>
		private static EffectParameter mDiffuseParam = null;

		/// <summary>明るさのパラメータ</summary>
		private static EffectParameter mBrightParam = null;

		/// <summary>テクセル位置のパラメータ</summary>
		private static EffectParameter mTexU = null;
		private static EffectParameter mTexV = null;

		/// <summary>
		/// 頂点バッファ
		/// </summary>
		private static VertexBuffer mVertexBuffer = null;
		private static VertexBuffer mReverseVertexBuffer = null;

		/// <summary>
		/// グラフィックスデバイス
		/// </summary>
		private static GraphicsDevice mDevice = null;

		/// <summary>
		/// スクリーンの大きさ
		/// </summary>
		private static Vector2 mScleenSize = Vector2.Zero;

		/// <summary>
		/// アルファブレンド設定
		/// </summary>
		private static BlendState mBlendState = new BlendState(){
			ColorSourceBlend = Blend.SourceAlpha,
			ColorDestinationBlend = Blend.InverseSourceAlpha,
			AlphaSourceBlend = Blend.SourceAlpha,
			AlphaDestinationBlend = Blend.DestinationAlpha,
		};

		private static BlendState mBlendStateAdd = new BlendState(){
			ColorSourceBlend = Blend.SourceAlpha,
			ColorDestinationBlend = Blend.One,
			AlphaSourceBlend = Blend.SourceAlpha,
			AlphaDestinationBlend = Blend.InverseSourceAlpha,
		};

		private static BlendState mCurrentBlendState = null;

		#endregion

		/// <summary>
		/// レンダリングタイプ
		/// </summary>
		public enum Mode{
			Default,
			CenterTransparent,
			BlurFilter,
		}

		public enum BlendType{
			Default,
			Add,
		}

		/// <summary>
		/// 初期化処理を行う
		/// </summary>
		/// <param name="device">グラフィックスデバイス</param>
		/// <param name="content">コンテントマネージャ</param>
		public static void Initialize( GraphicsDevice device, ContentManager content, float width, float height ){
			// デバイスへの参照を保存
			mDevice = device;

			// エフェクトを読み込む
			mEffect = content.Load<Effect>( "Effect/Sprite" );

			// テクニックを登録
			mEffect.CurrentTechnique = mEffect.Techniques[ "SpriteTec" ];

			// パラメータを取得
			mTextureParam = mEffect.Parameters[ "texMesh" ];
			mMatWVPParam = mEffect.Parameters[ "matWVP" ];
			mDiffuseParam = mEffect.Parameters[ "diffuse" ];
			mBrightParam = mEffect.Parameters[ "bright" ];
			mTexU = mEffect.Parameters[ "texU" ];
			mTexV = mEffect.Parameters[ "texV" ];

			// バッファを生成
			mVertexBuffer = new VertexBuffer( mDevice, typeof( VertexPositionTexture ), 4, BufferUsage.None );

			// バッファに頂点を登録
			VertexPositionTexture[] vertices = new VertexPositionTexture[ 4 ];
			vertices[ 0 ] = new VertexPositionTexture( new Vector3( -0.5f, 0.5f, 0.0f ), new Microsoft.Xna.Framework.Vector2( 0.0f, 0.0f ) );
			vertices[ 1 ] = new VertexPositionTexture( new Vector3( 0.5f, 0.5f, 0.0f ), new Microsoft.Xna.Framework.Vector2( 1.0f, 0.0f ) );
			vertices[ 2 ] = new VertexPositionTexture( new Vector3( -0.5f, -0.5f, 0.0f ), new Microsoft.Xna.Framework.Vector2( 0.0f, 1.0f ) );
			vertices[ 3 ] = new VertexPositionTexture( new Vector3( 0.5f, -0.5f, 0.0f ), new Microsoft.Xna.Framework.Vector2( 1.0f, 1.0f ) );
			mVertexBuffer.SetData( vertices );
			
			
			mReverseVertexBuffer = new VertexBuffer( mDevice, typeof( VertexPositionTexture ), 4, BufferUsage.None );
			VertexPositionTexture[] revVertices = new VertexPositionTexture[ 4 ];
			revVertices[ 0 ] = new VertexPositionTexture( new Vector3( -0.5f, 0.5f, 0.0f ), new Microsoft.Xna.Framework.Vector2( 1.0f, 0.0f ) );
			revVertices[ 1 ] = new VertexPositionTexture( new Vector3( 0.5f, 0.5f, 0.0f ), new Microsoft.Xna.Framework.Vector2( 0.0f, 0.0f ) );
			revVertices[ 2 ] = new VertexPositionTexture( new Vector3( -0.5f, -0.5f, 0.0f ), new Microsoft.Xna.Framework.Vector2( 1.0f, 1.0f ) );
			revVertices[ 3 ] = new VertexPositionTexture( new Vector3( 0.5f, -0.5f, 0.0f ), new Microsoft.Xna.Framework.Vector2( 0.0f, 1.0f ) );
			mReverseVertexBuffer.SetData( revVertices );

			// スクリーンサイズを登録
			mScleenSize = new Vector2( width, height );

			// テクセル位置を登録
			SetTexelUV( GameMain.ScreenWidth, GameMain.ScreenHeight );

			mCurrentBlendState = mBlendState;
		}
		
		/// <summary>
		/// レンダリングを行う
		/// </summary>
		public static void Render( Texture2D texture, float x, float y, float width, float height, float screenW, float screenH, Vector4 diffuse, float bright ){
			// 反転させるかチェック
			bool reverse = ( width < 0.0f ) ? true : false;
			width = Math.Abs( width );
			
			// テクスチャを登録
			mTextureParam.SetValue( texture );

			// アルファ値を登録
			mDiffuseParam.SetValue( diffuse );

			// 明るさを登録
			mBrightParam.SetValue( bright );

			// ビュー変換行列
			Matrix view = Matrix.CreateLookAt( Vector3.Zero, new Vector3( 0, 0, -1 ), Vector3.Up );

			// 射影変換行列
			Matrix proj = Matrix.CreateOrthographic( width / height, 1.0f, 0.0f, 1000.0f );
			
			// 拡大縮小行列を作成
			float w = ( width + 1.0f ) / screenW;
			float h = ( height + 1.0f ) / screenH;
			Matrix scale = Matrix.CreateScale( w * ( width / height ), h, 0.0f );

			// スクリーン座標からワールド座標に変換
			x = ( ( x - 1.0f ) / screenW - 0.5f + w / 2 ) * ( width / height );
			y = ( y - 1.0f ) / -screenH + 0.5f - h / 2;

			// 移動行列を作成
			Matrix trans = Matrix.CreateTranslation( x, y, 0.0f );

			// 行列を連結して登録
			Matrix matWVP = scale * trans * view * proj;
			mMatWVPParam.SetValue( matWVP );

			// 頂点バッファを登録
			if ( !reverse ){
				mDevice.SetVertexBuffer( mVertexBuffer );
			}else{
				mDevice.SetVertexBuffer( mReverseVertexBuffer );
			}

			// ブレンド条件を登録
			mDevice.BlendState = mCurrentBlendState;

			// 深度バッファに書き込まない
			mDevice.DepthStencilState = DepthStencilState.DepthRead;

			// 描画
			foreach ( EffectPass pass in mEffect.CurrentTechnique.Passes ){
				pass.Apply();
				mDevice.DrawPrimitives( PrimitiveType.TriangleStrip, 0, 2 );
			}

			// ブレンド条件の復帰
			mDevice.BlendState = BlendState.Opaque;
			mDevice.DepthStencilState = DepthStencilState.Default;
		}

		public static void Render( Texture2D texture, float x, float y, float width, float height, float screenW, float screenH, float alpha, float bright ){
			Render( texture, x, y, width, height, screenW, screenH, new Vector4( 1.0f, 1.0f, 1.0f, alpha ), bright );
		}
		public static void Render( Texture2D texture, float x, float y, float width, float height, Vector4 diffuse, float bright ){
			Render( texture, x, y, width, height, mScleenSize.X, mScleenSize.Y, diffuse, bright );
		}
		public static void Render( Texture2D texture, float x, float y, float width, float height, float alpha, float bright ){
			Render( texture, x, y, width, height, mScleenSize.X, mScleenSize.Y, new Vector4( 1.0f, 1.0f, 1.0f, alpha ), bright );
		}
		public static void Render( Texture2D texture, float x, float y, float width, float height, float alpha ){
			Render( texture, x, y, width, height, mScleenSize.X, mScleenSize.Y, new Vector4( 1.0f, 1.0f, 1.0f, alpha ), 1.0f );
		}
		public static void Render( Texture2D texture, float x, float y, float alpha ){
			Render( texture, x, y, texture.Width, texture.Height, mScleenSize.X, mScleenSize.Y,  new Vector4( 1.0f, 1.0f, 1.0f, alpha ), 1.0f );
		}
		public static void Render( Texture2D texture, float x, float y, Vector4 diffuse, float bright ){
			Render( texture, x, y, texture.Width, texture.Height, mScleenSize.X, mScleenSize.Y, diffuse, bright );
		}

		/// <summary>
		/// 画像の中心座標を指定してレンダリングを行う
		/// </summary>
		public static void RenderUseCenterPosition( Texture2D texture, float centerX, float centerY, float width, float height, float alpha, float bright ){
			Render( texture, centerX - Math.Abs( width / 2.0f ), centerY - height / 2.0f, width, height, mScleenSize.X, mScleenSize.Y, new Vector4( 1.0f, 1.0f, 1.0f, alpha ), bright );
		}

		public static void RenderUseCenterPosition( Texture2D texture, float centerX, float centerY ){
			float width = texture.Width;
			float height = texture.Height;
			Render( texture, centerX - Math.Abs( width / 2.0f ), centerY - height / 2.0f, width, height, mScleenSize.X, mScleenSize.Y, new Vector4( 1.0f, 1.0f, 1.0f, 1.0f ), 1.0f );
		}

		/// <summary>
		/// 中心を透明にするかどうかを登録する
		/// </summary>
		public static void CenterTransparentEnable( bool enable ){
			if ( enable ){
				mEffect.CurrentTechnique = mEffect.Techniques[ "SpriteBlurTec" ];
			}else{
				mEffect.CurrentTechnique = mEffect.Techniques[ "SpriteTec" ];
			}
		}

		/// <summary>
		/// レンダリングタイプを登録する
		/// </summary>
		public static void SetRenderMode( Mode mode ){
			switch ( mode ){
				case Mode.CenterTransparent :
					mEffect.CurrentTechnique = mEffect.Techniques[ "SpriteBlurTec" ];
					break;
				case Mode.BlurFilter :
					mEffect.CurrentTechnique = mEffect.Techniques[ "SpriteBlurFilterTec" ];
					break;
				default :
					mEffect.CurrentTechnique = mEffect.Techniques[ "SpriteTec" ];
					break;
			}
		}

		public static void SetBlendType( BlendType type ){
			switch ( type ){
				case BlendType.Add :
					mCurrentBlendState = mBlendStateAdd;
					break;
				default :
					mCurrentBlendState = mBlendState;
					break;
			}
		}

		/// <summary>
		/// テクセル位置を登録する
		/// </summary>
		/// <param name="width">テクスチャの幅</param>
		/// <param name="height">テクスチャの高さ</param>
		public static void SetTexelUV( float width, float height ){
			// 1テクセルの大きさをセット
			float tU = 1.0f / width;
			float tV = 1.0f / height;

			int length = 5;
			float[] u = new float[ length ];
			float[] v = new float[ length ];

			for ( int i = 0; i < length; i++ ){
				u[ i ] = tU * ( i + 1 );
				v[ i ] = tV * ( i + 1 );
			}

			// シェーダに登録
			mTexU.SetValue( u );
			mTexV.SetValue( v );
		}
	}
}

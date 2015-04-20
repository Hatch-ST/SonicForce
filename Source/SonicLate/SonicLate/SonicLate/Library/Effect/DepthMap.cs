using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SonicLate{
	/// <summary>
	/// 深度マップのエフェクト
	/// </summary>
	class DepthMap : EffectParent{
		private GraphicsDevice mDevice = null;

		private RenderTarget2D[] mRenderTarget = null;

		private Matrix mMatView;
		private Matrix mMatProj;
		private Matrix mMatViewProj;

		private int mTextureSize = 512;
		private int mCompTextureIndex = 0;

		private EffectParameter mThisMatVPParam = null;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="device">グラフィックスデバイス</param>
		/// <param name="content">コンテントマネージャ</param>
		public DepthMap( GraphicsDevice device, ContentManager content, int textureSize ){
			// エフェクトを読み込む
			mEffect = content.Load<Effect>( "Effect/DepthMap" );

			// テクニックを登録
			mEffect.CurrentTechnique = mEffect.Techniques[ "DepthMapTec" ];

			// パラメータを取得
			mMatWorldParam = mEffect.Parameters[ "matWorld" ];
			mMatBoneParam = mEffect.Parameters[ "bones" ];

			mThisMatVPParam = mEffect.Parameters[ "matVP" ];

			// テクスチャサイズを登録
			mTextureSize = textureSize;

			// レンダリングターゲットを作成
			mRenderTarget = new RenderTarget2D[ 2 ];
			for ( int i = 0; i < 2; i++ ){
				mRenderTarget[ i ] = new RenderTarget2D( device, textureSize, textureSize, false, SurfaceFormat.Color, DepthFormat.Depth24 );
			}

			// デバイスを登録
			mDevice = device;

			// 射影変換行列の設定
			float nearClip = 1.0f;
			float farClip = 3000.0f;
			mMatProj = Matrix.CreatePerspectiveFieldOfView( MathHelper.ToRadians( 75.0f ), 1.0f, nearClip, farClip );
		}

		/// <summary>
		/// 通常のテクニックに設定する
		/// </summary>
		public override void SetToDefaultTechniques(){
			mEffect.CurrentTechnique = mEffect.Techniques[ "DepthMapTec" ];
		}

		/// <summary>
		/// スキニング用のテクニックに設定する
		/// </summary>
		public override void SetToSkinningTechniques(){
			mEffect.CurrentTechnique = mEffect.Techniques[ "DepthMapSkinedTec" ];
		}

		/// <summary>
		/// シャドウマップへのレンダリングを開始する
		/// </summary>
		/// <param name="lightPosition"></param>
		/// <param name="lightDirection"></param>
		public void Begin( Vector3 lightPosition, Vector3 lightTarget ){
			// ビュー変換行列を作成
			mMatView = Matrix.CreateLookAt( lightPosition, lightTarget, Vector3.Up );

			// ビュー変換×射影変換行列を登録
			mMatViewProj = mMatView * mMatProj;
			mThisMatVPParam.SetValue( mMatViewProj );
			
			// レンダリングターゲットを登録
			mDevice.SetRenderTarget( mRenderTarget[ 0 ] );
			mDevice.Clear( Color.White );
		}

		/// <summary>
		/// シャドウマップへのレンダリングを終了する
		/// </summary>
		public void End(){
			// レンダリングターゲットを元に戻す
			mDevice.SetRenderTarget( null );

			// 拡大してテクスチャに貼り付け
			mDevice.SetRenderTarget( mRenderTarget[ 1 ] );
			mDevice.Clear( Color.White );

			SpriteBoard.SetRenderMode( SpriteBoard.Mode.BlurFilter );

			// 端を白くするため小さ目に描画
			SpriteBoard.Render( mRenderTarget[ 0 ], 10, 10, mTextureSize - 20, mTextureSize - 20, mTextureSize, mTextureSize, 1.0f, 1.0f );
			
			SpriteBoard.SetRenderMode( SpriteBoard.Mode.Default );

			// レンダリングターゲットを元に戻す
			mDevice.SetRenderTarget( null );
			
			// スクリーンをクリア
			mDevice.Clear( GameMain.BackGroundColor );

			mCompTextureIndex = 1; // 完成したテクスチャのインデックス

			// 深度影エフェクトに渡す
			EffectManager.DepthSadow.SetDepthMap( this );
		}
	
		/// <summary>
		/// ライトのビューマトリックスと射影変換行列を取得する
		/// </summary>
		/// <param name="view"></param>
		/// <param name="proj"></param>
		public void GetLightState( out Matrix view, out Matrix proj ){
			view = mMatView;
			proj = mMatProj;
		}

		/// <summary>
		/// 深度マップを取得する
		/// </summary>
		public Texture2D RenderTarget{
			get { return mRenderTarget[ mCompTextureIndex ]; }
		}
	}
}

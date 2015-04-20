using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SonicLate{
	/// <summary>
	/// ブラーエフェクト
	/// </summary>
	class Blur{
		private RenderTarget2D[] mRenderTargets = null;
		private int mTarget = 0;
		private float mAlpha = 0.9f;
		private float mWidth, mHeight;
		private bool mTransparentEnable = true;

		private readonly GraphicsDevice mDevice;

		private RenderTarget2D mTempRenderTarget = null;
		private Color mBackGroundColor = Color.White;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="device">グラフィックスデバイス</param>
		/// <param name="width">テクスチャの幅</param>
		/// <param name="height">テクスチャの高さ</param>
		public Blur( GraphicsDevice device, int width, int height ){
			mRenderTargets = new RenderTarget2D[ 2 ];
			for ( int i = 0; i < 2; i++ ){
				mRenderTargets[ i ] = new RenderTarget2D( device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8 );
			}
			mDevice = device;
			mWidth = width;
			mHeight = height;
		}

		/// <summary>
		/// 描画を開始する
		/// ここから Blur.End() までにレンダリングされたものが Blur.Texture に書き込まれる
		/// renderTargetにnullを入れるとテクスチャに直接書き込む
		/// </summary>
		/// <param name="renderTarget">レンダリングターゲット</param>
		/// <param name="depthClearEnable">深度バッファをクリアするかどうか</param>
		/// <param name="backGroundColor">背景色</param>
		public void Begin( RenderTarget2D renderTarget, bool depthClearEnable, Vector4 backGroundColor ){
			// ターゲットを交換
			mTarget = 1 - mTarget;
			
			// レンダリングターゲットを登録
			mTempRenderTarget = renderTarget;
			if ( mTempRenderTarget != null ){
				mDevice.SetRenderTarget( mTempRenderTarget );
			}else{
				mDevice.SetRenderTarget( mRenderTargets[ mTarget ] );
			}

			mBackGroundColor = new Color( backGroundColor );

			// 深度バッファをクリアする
			if ( depthClearEnable ){
				mDevice.Clear( mBackGroundColor );
			}
			// 深度バッファをクリアしない
			else{
				mDevice.Clear( ClearOptions.Target | ClearOptions.Stencil, mBackGroundColor, 0.0f, 0 );
			}
		}
		
		/// <summary>
		/// 描画を終了する
		/// </summary>
		public void End( bool transparent, Nullable<float> alpha ){

			// 別のレンダリングターゲットの内容をテクスチャに書き込む
			if ( mTempRenderTarget != null ){
				mDevice.SetRenderTarget( mRenderTargets[ mTarget ] );
				mDevice.Clear( Color.White );
				SpriteBoard.Render( mTempRenderTarget, 0, 0, mWidth, mHeight, 1.0f );
			}

			// 中心を透過させる
			SpriteBoard.CenterTransparentEnable( transparent );

			// 前回の画面を合成
			SpriteBoard.Render( mRenderTargets[ 1 - mTarget ], 0, 0, mWidth, mHeight, ( ( alpha == null ) ? mAlpha : alpha.Value ) );
			
			// 中心を透過させない
			SpriteBoard.CenterTransparentEnable( false );

			// レンダリングターゲットを元に戻す
			mDevice.SetRenderTarget( null );
		}

		/// <summary>
		/// 描画を終了する
		/// </summary>
		public void End(){
			End( mTransparentEnable, mAlpha );
		}

		/// <summary>
		/// テクスチャを取得する
		/// </summary>
		public Texture2D Texture{
			get { return mRenderTargets[ mTarget ]; }
		}

		/// <summary>
		/// ブラーの強さを指定する
		/// </summary>
		public float Strength{
			get { return mAlpha; }
			set { mAlpha = Math.Max( 0.0f, Math.Min( 1.0f, value ) ); }
		}

		/// <summary>
		/// 中心を透過させるかどうかを取得または登録する
		/// </summary>
		public bool CenterTransparentEnable{
			get { return mTransparentEnable; }
			set { mTransparentEnable = value; }
		}
	}
}

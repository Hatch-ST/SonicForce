using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SonicLate{
	class LightBloom{
		private RenderTarget2D[] mRenderTargets = null;
		private RenderTarget2D mTempRenderTarget = null;
		private int mTarget = 0;

		private readonly GraphicsDevice mDevice;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public LightBloom( GraphicsDevice device, int width, int height ){
			mRenderTargets = new RenderTarget2D[ 2 ];
			for ( int i = 0; i < 2; i++ ){
				mRenderTargets[ i ] = new RenderTarget2D( device, width, height );
			}

			mDevice = device;
		}

		/// <summary>
		/// 描画を開始する
		/// </summary>
		/// <param name="renderTarget">レンダリングターゲット</param>
		/// <param name="depthClearEnable">深度バッファをクリアするかどうか</param>
		public void Begin( RenderTarget2D renderTarget, bool depthClearEnable ){
			// ターゲットを交換
			mTarget = 1 - mTarget;
			
			// レンダリングターゲットを登録
			mTempRenderTarget = renderTarget;
			if ( mTempRenderTarget != null ){
				mDevice.SetRenderTargets( mTempRenderTarget );
			}else{
				mDevice.SetRenderTarget( mRenderTargets[ mTarget ] );
			}
			
			// 深度バッファをクリアする
			if ( depthClearEnable ){
				mDevice.Clear( new Color( 1.0f, 1.0f, 1.0f, 0.0f ) );
			}
			// 深度バッファをクリアしない
			else{
				mDevice.Clear( ClearOptions.Target | ClearOptions.Stencil, new Color( 1.0f, 1.0f, 1.0f, 0.0f ), 0.0f, 0 );
			}
		}

		/// <summary>
		/// 描画を終了する
		/// </summary>
		/// <param name="level">ぼかしの強さ</param>
		public void End( int level ){
			// 別のレンダリングターゲットの内容をテクスチャに書き込む
			if ( mTempRenderTarget != null ){
				mDevice.SetRenderTarget( mRenderTargets[ mTarget ] );
				mDevice.Clear( new Color( 0.0f, 0.0f, 0.0f, 0.0f ) );
				SpriteBoard.Render( mTempRenderTarget, 0, 0, mRenderTargets[ mTarget ].Width, mRenderTargets[ mTarget ].Height, 1.0f );
			}

			// ブラーフィルターに設定
			SpriteBoard.SetRenderMode( SpriteBoard.Mode.BlurFilter );

			// 何度か繰り返してぼかす
			level = Math.Max( 0, level );
			for ( int i = 0; i < level; i++ ){
				// ターゲットを交換
				mTarget = 1 - mTarget;
				
				// レンダリングターゲットを登録
				mDevice.SetRenderTarget( mRenderTargets[ mTarget ] );
				mDevice.Clear( new Color( 0.0f, 0.0f, 0.0f, 1.0f ) );

				SpriteBoard.Render( mRenderTargets[ 1 - mTarget ], 0, 0, 1.0f );
			}

			SpriteBoard.SetRenderMode( SpriteBoard.Mode.Default );
			
			// レンダリングターゲットを元に戻す
			mDevice.SetRenderTarget( null );
		}

		/// <summary>
		/// レンダリングを行う
		/// </summary>
		public void Render( int x, int y, Vector4 color ){
			SpriteBoard.SetBlendType( SpriteBoard.BlendType.Add );
			//SpriteBoard.SetRenderMode( SpriteBoard.Mode.BlurFilter );
			
			SpriteBoard.Render( mRenderTargets[ mTarget ], x, y, color, 1.0f );

			SpriteBoard.SetBlendType( SpriteBoard.BlendType.Default );
			//SpriteBoard.SetRenderMode( SpriteBoard.Mode.Default );
		}
	}
}

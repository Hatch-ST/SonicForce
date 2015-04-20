using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SonicLate{
	class BlackWhole{
		private Texture2D mTexture = null;
		private Texture2D mTextureBlack = null;

		private bool mActive = false;
		private const float mMaxScale = 5.0f;
		private const float mSpeed = 0.06f;

		private float mScale = mMaxScale;
		private Mode mMode = Mode.Open;

		public enum Mode{
			Open,
			Close,
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public BlackWhole( ContentManager content ){
			mTexture = content.Load<Texture2D>( "Image/blackwhole" );
			mTextureBlack = content.Load<Texture2D>( "Image/black" );
		}

		/// <summary>
		/// 描画
		/// </summary>
		public void Draw( Camera camera, Player player ){
			if ( mActive ){
				switch ( mMode ){
					case Mode.Close :
						// 小さくする
						mScale = Math.Max( 0.0f, mScale - mSpeed );
						break;
					case Mode.Open :
						// 大きくする
						mScale = Math.Min( mMaxScale, mScale + mSpeed );

						if ( mScale == mMaxScale ){
							mActive = false;
						}
						break;
				}

				// スクリーン座標でのプレイヤーの位置を算出
				Vector3 p = Vector3.Transform( player.Position, camera.View * camera.Projection );
				p /= p.Z;
				p = ( p + Vector3.One ) / 2;

				// 画像の大きさ
				float w = mTexture.Width * mScale;
				float h = mTexture.Height * mScale;

				// 画像の出力先を算出
				float x = p.X * GameMain.ScreenWidth - w / 2;
				float y = ( 1.0f - p.Y ) * GameMain.ScreenHeight - h / 2;


				// 描画
				SpriteBoard.Render( mTexture, x, y, w, h, 1.0f );


				// 円の外側に黒い矩形を表示する
				SpriteBoard.Render( mTextureBlack, 0.0f, 0.0f, GameMain.ScreenWidth, y, 1.0f );
				SpriteBoard.Render( mTextureBlack, 0.0f, y + h, GameMain.ScreenWidth, GameMain.ScreenHeight, 1.0f );

				SpriteBoard.Render( mTextureBlack, 0.0f, y, Math.Max( 0.0f, x ), h, 1.0f );
				SpriteBoard.Render( mTextureBlack, x + w, y, Math.Max( 0.0f, GameMain.ScreenWidth - ( x + w ) ), h, 1.0f );

				if ( mScale == 0.0f ){
					SpriteBoard.Render( mTextureBlack, 0.0f, 0.0f, GameMain.ScreenWidth, GameMain.ScreenHeight, 1.0f );
				}
			}
		}

		/// <summary>
		/// 開く
		/// </summary>
		public void Open(){
			mMode = Mode.Open;
		}

		/// <summary>
		/// 閉じる
		/// </summary>
		public void Close(){
			mMode = Mode.Close;
			mActive = true;
		}

		/// <summary>
		/// 閉じたかどうかを取得する
		/// </summary>
		public bool Closed{
			get { return ( mScale == 0.0f ); }
		}

		/// <summary>
		/// 開いたかどうかを取得する
		/// </summary>
		public bool Opened{
			get { return mActive; }
		}
	}
}

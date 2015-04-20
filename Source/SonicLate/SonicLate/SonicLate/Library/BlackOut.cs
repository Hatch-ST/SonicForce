using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SonicLate{
	class BlackOut{
		private Texture2D mTexture = null;

		private const int mEndTime = 60;
		private float mSpeed = 1.0f / mEndTime;
		private float mAlpha = 1.0f;

		private Mode mMode = Mode.Open;

		private bool mToWhite = false;

		private bool mActive = false;

		public enum Mode{
			Open,
			Close,
		}
		
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="content">コンテンツマネージャ</param>
		public BlackOut( ContentManager content ){
			mTexture = content.Load<Texture2D>( "Image/black" );
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="content">コンテンツマネージャ</param>
		/// <param name="speed">ブラックイン/アウトが終了する時間</param>
		/// <param name="defaultMode">最初の状態</param>
		/// <param name="toWhite">白くするかどうか</param>
		public BlackOut( ContentManager content, int endTime, Mode defaultMode, bool toWhite ){
			mTexture = content.Load<Texture2D>( "Image/black" );
			mToWhite = toWhite;
			mSpeed = 1.0f / endTime;
			switch( defaultMode ){
				case Mode.Open :
					mAlpha = 0.0f;
					break;
				case Mode.Close :
					mAlpha = 1.0f;
					break;
			}
		}

		/// <summary>
		/// 描画を行う
		/// </summary>
		public void Draw(){
			if ( mActive ){
				switch ( mMode ){
					case Mode.Open :
						mAlpha = Math.Max( 0.0f, mAlpha - mSpeed );
						if ( Opened ) mActive = false;
						break;
					case Mode.Close :
						mAlpha = Math.Min( 1.0f, mAlpha + mSpeed );
						if ( Closed ) mActive = false;
						break;
				}
			}

			if ( mAlpha > 0.0f ){
				float bright = ( mToWhite ) ? 2.0f : 1.0f;
				SpriteBoard.Render( mTexture, 0.0f, 0.0f, GameMain.ScreenWidth, GameMain.ScreenHeight, mAlpha, bright );
			}
		}

		/// <summary>
		/// アルファ値を取得または登録する
		/// </summary>
		public float Alpha{
			set { mAlpha = Math.Max( 0.0f, Math.Min( 1.0f, value ) ); }
			get { return mAlpha; }
		}

		/// <summary>
		/// 開く
		/// </summary>
		public void Open(){
			mMode = Mode.Open;
			mActive = true;
		}

		/// <summary>
		/// 閉じる
		/// </summary>
		public void Close(){
			mMode = Mode.Close;
			mActive = true;
		}

		/// <summary>
		/// 開いているかどうか
		/// </summary>
		public bool Closed{
			get { return ( mAlpha == 1.0f ); }
		}

		/// <summary>
		/// 閉じているかどうか
		/// </summary>
		public bool Opened{
			get { return ( mAlpha == 0.0f ); }
		}
	}
}

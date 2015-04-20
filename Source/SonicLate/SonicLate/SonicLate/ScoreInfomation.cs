using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SonicLate{
	/// <summary>
	/// スコアのポップアップ
	/// </summary>
	class ScoreInfomation{
		private MultiTexture mTextures = null;
		private Interpolate mApperInter = null;
		private int mCount = 0;

		private Mode mMode = Mode.Apper;

		private int mValue = 0;
		private int mNumFigure = -1;
		private int[] mFigures = null;
		private Vector4 mDiffuse = Vector4.One;
		private float mAlpha = 1.0f;
		private Vector2 mDest = Vector2.Zero;
		
		private const int mApperEndTime = 10;
		private const int mDisplayEndTime = 60;
		private const int mHideEndTime = 10;

		private float mScaleRate = 1.0f;

		private const float mDistanceToPlayer = 50.0f;

		private bool mActive = false;
		
		private float mWidthRate = 0.0f;
		private float mHeightRate = 0.0f;

		private enum Mode{
			Apper,
			Display,
			Hide,
			End
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="content">コンテンツマネージャ</param>
		public ScoreInfomation( ContentManager content ){
			mApperInter = new Interpolate();
			mActive = false;
			mCount = 0;

			mTextures = new MultiTexture( content, "Image/point", 10 );

			mFigures = new int[ 10 ];
			for ( int i = 0; i < 10; i++ ){
				mFigures[ i ] = 0;
			}
		}

		/// <summary>
		/// 登録
		/// </summary>
		/// <param name="value">スコア</param>
		public void Set( int value, Camera camera, Vector3 position ){
			mValue = value;

			mFigures = new int[ 10 ];
			mNumFigure = -1;
			GetFigure( mFigures, mValue );

			mApperInter.Time = 0;
			mCount = 0;
			mMode = Mode.Apper;
			mAlpha = 1.0f;
			mActive = true;

			// スクリーン座標でのプレイヤーの位置を算出
			Vector3 p = Vector3.Transform( position, camera.View * camera.Projection );
			p /= p.Z;
			p = ( p + Vector3.One ) / 2;

			// 画像の出力先を算出
			mDest.X = p.X * GameMain.ScreenWidth;
			mDest.Y = ( 1.0f - p.Y ) * GameMain.ScreenHeight - mDistanceToPlayer;

			// 大きさを登録
			mScaleRate = Math.Max( 0.6f, Math.Min( 1.0f, value / 9999.0f ) ) * 1.2f;

			// 色を登録
			float rate = Math.Max( 0.0f, Math.Min( 1.0f, value / 9999.0f ) );
			Vector4 color = Vector4.One;
			switch ( ( int )( rate * 3.0f ) ){
				case 0 :
					color = new Vector4( 1.0f, 1.0f, 1.0f, 1.0f );
					break;
				case 1 :
					color = new Vector4( 0.7f, 1.0f, 1.0f, 1.0f );
					break;
				case 2 :
					color = new Vector4( 1.0f, 1.0f, 0.7f, 1.0f );
					break;
				case 3 :
					color = new Vector4( 1.0f, 1.0f, 0.7f, 1.0f );
					break;
			}
			mDiffuse = color;
		}

		/// <summary>
		/// 更新を行う
		/// </summary>
		public void Update(){
			if ( mActive ){
				switch ( mMode ){
					case Mode.Apper :
						mApperInter.GetSin( 0.0f, 1.0f, mApperEndTime );
						mWidthRate = 1.0f + ( 1.0f - mApperInter.Value ) * 3.0f;
						mHeightRate = mApperInter.Value;
						mAlpha = mApperInter.Value;
						if ( mCount > mApperEndTime ){
							mMode = Mode.Display;
							mCount = -1;
						}
						break;
					case Mode.Display :
						if ( mCount > mDisplayEndTime ){
							mMode = Mode.Hide;
							mCount = -1;
						}
						break;
					case Mode.Hide :
						mApperInter.GetSin( 1.0f, 0.0f, mHideEndTime );
						mWidthRate = ( 2.0f - mApperInter.Value );
						mHeightRate = mApperInter.Value;
						mAlpha = mApperInter.Value;
						if ( mCount > mHideEndTime ){
							mMode = Mode.End;
							mCount = -1;
						}
						break;
					case Mode.End :
						mActive = false;
						break;
				}

				mDiffuse.W = mAlpha;

				++mCount;
			}
		}

		/// <summary>
		/// 描画を行う
		/// </summary>
		public void Draw(){
			if ( mActive ){
				for ( int i = 0; i < mNumFigure; i++ ){
					float w = mTextures.Get[ 0 ].Width * mWidthRate * mScaleRate;
					float h = mTextures.Get[ 0 ].Height * mHeightRate * mScaleRate;
					float destX = mDest.X - ( w * mNumFigure * 0.5f ) + i * w;
					float destY = mDest.Y - ( h * 0.5f );
					float bright = 1.0f;
					if ( mValue >= 9999 && mCount % 12 < 6 ){
						bright = 1.4f;
					}

					SpriteBoard.Render( mTextures.Get[ mFigures[ mNumFigure - i - 1 ] ], destX, destY, w, h, mDiffuse, bright );
				}
			}
		}

		/// <summary>
		/// 生きてるかどうか
		/// </summary>
		public bool Active{
			get { return mActive; }
			set { mActive = value; }
		}

		/// <summary>
		///  桁に分割する
		/// </summary>
		private void GetFigure( int[] figures, int value ){
			for ( int i = 0; i < figures.Length; i++ ){
				figures[ i ] = ( int )( value % 10 );
				value /= 10;
				if ( value == 0 && mNumFigure == -1 ){
					mNumFigure = i + 1;
				}
			}
		}
	}
}

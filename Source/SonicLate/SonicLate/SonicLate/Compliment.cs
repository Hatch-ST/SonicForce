using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

namespace SonicLate{
	/// <summary>
	/// 褒め言葉
	/// </summary>
	class Compliment{
		private MultiTexture mTextures = null;

		private Interpolate mApperInter = null;
		private int mCount = 0;
		private float mBright = 1.0f;

		private const int mApperEndTime = 10;
		private const int mDisplayEndTime = 40;
		private const int mHideEndTime = 10;
		private const int mFlashTime = 8; // 点滅する間隔
		private const int mNumFlash = 2; // 点滅する回数

		private const float mDistanceToPlayer = 50.0f;
		private const float mMoveAmountOnApper = 50.0f;

		private int mLevel = 0;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="content">コンテンツマネージャ</param>
		public Compliment( ContentManager content ){
			mApperInter = new Interpolate();
			mApperInter.Time = 0xffff;
			mApperInter.Get( 0.0f, 0.0f, 0 );
			mCount = 0xffff;

			mTextures = new MultiTexture( content, "Image/Compliment", 7 );

		}

		/// <summary>
		/// 初期化
		/// </summary>
		public void Initialize(){
			mApperInter = new Interpolate();
			mApperInter.Time = 0xffff;
			mApperInter.Get( 0.0f, 0.0f, 0 );
			mCount = 0xffff;
		}

		/// <summary>
		/// 更新を行う
		/// </summary>
		/// <param name="player">プレイヤーへの参照</param>
		public void Update( Player player ){
			// プレイヤーが避けた瞬間なら補間をリセット
			if ( player.JustAvoid ){
				mApperInter.Reset();
				mCount = 0;

				mLevel = Math.Max( 0, Math.Min( 6, player.ComboCount - 1 ) );

				float volume = 1.0f;

				// Seを鳴らす
				switch ( mLevel ){
					case 0 :
						SoundManager.Play( SoundManager.SE.Good, volume );
						break;
					case 1 :
						SoundManager.Play( SoundManager.SE.Great, volume );
						break;
					case 2 :
						SoundManager.Play( SoundManager.SE.Excellent, volume );
						break;
					case 3 :
						SoundManager.Play( SoundManager.SE.Wonderful, volume );
						break;
					case 4 :
						SoundManager.Play( SoundManager.SE.Marvelous, volume );
						break;
					case 5 :
						SoundManager.Play( SoundManager.SE.Fantastic, volume );
						break;
					default :
						SoundManager.Play( SoundManager.SE.Unbelievable, volume );
						break;
				}
			}

			// 時間を進める
			if ( mCount > mApperEndTime + mDisplayEndTime && mCount <= mApperEndTime + mDisplayEndTime + mHideEndTime ){
				mApperInter.GetSin( 1.0f, 0.0f, mHideEndTime );
			}else if ( mCount <= mApperEndTime + mDisplayEndTime ){
				mApperInter.GetSin( 0.0f, 1.0f, mApperEndTime );
			}
			
			mBright = 1.0f;
			if ( mCount < mApperEndTime ){
				mBright = 0.5f;
			}else if ( mCount < mDisplayEndTime ){
				if ( mCount % mFlashTime == 0 && mCount < mApperEndTime + mFlashTime * mNumFlash )
				mBright = 1.5f;
			}

			++mCount;
		}

		/// <summary>
		/// 描画を行う
		/// </summary>
		/// <param name="camera">カメラ</param>
		/// <param name="player">プレイヤー</param>
		public void Draw( Camera camera, Player player ){
			if ( mApperInter.Value > 0.1f ){
				// スクリーン座標でのプレイヤーの位置を算出
				Vector3 p = Vector3.Transform( player.Position, camera.View * camera.Projection );
				p /= p.Z;
				p = ( p + Vector3.One ) / 2;

				// 画像の出力先を算出
				float cX = p.X * GameMain.ScreenWidth;
				float cY = ( 1.0f - p.Y ) * GameMain.ScreenHeight;

				// 少しずらす
				float t = mMoveAmountOnApper * ( ( float )Math.Min( mCount, mApperEndTime ) / mApperEndTime );
				cY -= mDistanceToPlayer + t;

				// 画像の大きさ
				float w = mTextures.Get[ mLevel ].Width;
				float h = mTextures.Get[ mLevel ].Height;

				// 描画
				SpriteBoard.Render( mTextures.Get[ mLevel ], cX - w / 2, cY - h / 2, w, h, mApperInter.Value, mBright );
			}
		}
	}
}

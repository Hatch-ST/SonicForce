using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SonicLate {
	class CheckPointImage{

		class CharacterImage{
			private Texture2D mTexture;
			private float mWidthRate = 0.0f;
			private float mHeightRate = 0.0f;

			private float mRadWidth = 0.0f;
			private float mRadHeight = 0.0f;

			private bool mWidthEnd = false;
			private bool mHeightEnd = false;

			private const int mEndTime = 60;

			public int EndTime{
				get { return mEndTime; }
			}

			public CharacterImage( Texture2D texture ){
				mTexture = texture;
			}

			public void Reset(){
				mWidthRate = 0.0f;
				mHeightRate = 0.0f;
				mRadWidth = 0.0f;
				mRadHeight = 0.0f;
				mWidthEnd = false;
				mHeightEnd = false;
			}

			public void Update(){
				if ( !mWidthEnd ){
					mRadWidth += ( float )( Math.PI * 2.5 ) / mEndTime;
					mWidthRate = ( float )Math.Sin( mRadWidth ) * 2.0f;
				}

				if ( !mHeightEnd ){
					mRadHeight += ( float )( Math.PI ) / mEndTime;
					mHeightRate = ( float )Math.Sin( mRadHeight ) * 2.0f;
				}
				

				if ( mRadWidth >= Math.PI * 2.0f && mWidthRate >= 1.0f ){
					mWidthRate = 1.0f;
					mWidthEnd = true;
				}

				if ( mRadHeight >= Math.PI * 0.5f && mHeightRate <= 1.0f ){
					mHeightRate = 1.0f;
					mHeightEnd = true;
				}
			}

			public void Draw( float x, float y, float alpha, float bright ){
				SpriteBoard.RenderUseCenterPosition( mTexture, x, y, mTexture.Width * mWidthRate, mTexture.Height * mHeightRate, alpha, bright );
			}
		}
		
		private MultiTexture mTextures = null;
		private CharacterImage[] mCharacterImages = null;
		private int mCount = 0;
		private const int mInterval = 8;
		private const int mEndTime = 180;

		private float mBright = 1.0f;
		private float mAlpha = 0.0f;

		private float mTotalWidth = 0.0f;

		public CheckPointImage( ContentManager content ){
			mTextures = new MultiTexture( content, "Image/CheckPoint/CheckPoint", 11 );
			mCharacterImages = new CharacterImage[ mTextures.Get.Length ];
			for ( int i = 0; i < mCharacterImages.Length; i++ ){
				mCharacterImages[ i ] = new CharacterImage( mTextures.Get[ i ] );

				mTotalWidth += mTextures.Get[ i ].Width;
			}
		}

		public void Initialize(){
			mCount = 0;
			mBright = 1.0f;
			mAlpha = 0.0f;
			for ( int i = 0; i < mCharacterImages.Length; i++ ){
				mCharacterImages[ i ].Reset();
			}
		}

		public void Reset(){
			mCount = 0;
			mBright = 1.0f;
			mAlpha = 1.0f;
			for ( int i = 0; i < mCharacterImages.Length; i++ ){
				mCharacterImages[ i ].Reset();
			}
		}

		public void Update(){
			if ( mAlpha > 0.0f ){
				for ( int i = 0; i < mCharacterImages.Length; i++ ){
					if ( mCount > mInterval * i ){
						mCharacterImages[ i ].Update();
					}
				}

				if ( mCount > mEndTime ){
					mBright += 0.1f;
					mAlpha -= 0.02f;
				}

				++mCount;
			}
		}

		public void Draw( Camera camera, Vector3 position ){
			if ( mAlpha > 0.0f ){
				// スクリーン座標でのプレイヤーの位置を算出
				Vector3 p = Vector3.Transform( position, camera.View * camera.Projection );
				p /= p.Z;
				p = ( p + Vector3.One ) / 2;

				// 画像の出力先を算出
				float x = p.X * GameMain.ScreenWidth - mTotalWidth / 2.0f;
				float y = ( 1.0f - p.Y ) * GameMain.ScreenHeight;

				float destX = x;
				for ( int i = 0; i < mCharacterImages.Length; i++ ){
					destX += mTextures.Get[ i ].Width / 2.0f;
					mCharacterImages[ i ].Draw( destX, ( float )y, mAlpha, mBright );
					destX += mTextures.Get[ i ].Width / 2.0f;
				}
			}
		}
	}
}

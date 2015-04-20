using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SonicLate{
	/// <summary>
	/// フェードアウトできるテクスチャ
	/// </summary>
	class FadeTexture{
		private Texture2D mTexture = null;
		private float mAlpha = 0.0f;
		private float mSpeed = 0.05f;

		private bool mOnOut = false;
		
		public FadeTexture( ContentManager content, string fileName ){
			mTexture = content.Load<Texture2D>( fileName );
		}
		
		public FadeTexture( ContentManager content, string fileName, float speed, bool outed ){
			mTexture = content.Load<Texture2D>( fileName );
			mSpeed = speed;

			mOnOut = outed;
			mAlpha = ( mOnOut ) ? 0.0f : 1.0f;
		}

		public FadeTexture( ContentManager content, string fileName, int endTime, bool outed ){
			mTexture = content.Load<Texture2D>( fileName );
			mSpeed = 1.0f / endTime;

			mOnOut = outed;
			mAlpha = ( mOnOut ) ? 0.0f : 1.0f;
		}

		public Texture2D Get{
			get { return mTexture; }
		}

		public void Render( float x, float y ){
			if ( mOnOut ){
				mAlpha = Math.Max( 0.0f, mAlpha - mSpeed );
			}else{
				mAlpha = Math.Min( 1.0f, mAlpha + mSpeed );
			}
			SpriteBoard.Render( mTexture, x, y, mAlpha );
		}

		public float Alpha{
			set { mAlpha = value; }
			get { return mAlpha; }
		}

		public void In(){
			mOnOut = false;
		}

		public void Out(){
			mOnOut = true;
		}
	}
}

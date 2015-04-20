using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SonicLate{
	/// <summary>
	/// 複数枚のテクスチャクラス
	/// </summary>
	class MultiTexture{
		private Texture2D[] mTextures = null;
		private int mCount = 0;
		private bool mIsAnimationEnd = false;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="content">コンテンツマネージャ</param>
		/// <param name="fileName">ファイル名 ( ****_1.png → **** )</param>
		/// <param name="numTexture">画像の枚数</param>
		public MultiTexture( ContentManager content, string fileName, int numTexture ){
			mTextures = new Texture2D[ numTexture ];
			for ( int i = 0; i < numTexture; i++ ){
				mTextures[ i ] = content.Load<Texture2D>( fileName + "_" + ( i + 1 ) );
			}
		}

		/// <summary>
		/// テクスチャを取得する
		/// </summary>
		public Texture2D[] Get{
			get { return mTextures; }
		}

		public Texture2D GetWithAdvanceAnimation( int oneFrame, bool loop ){
			int index = mCount % ( oneFrame * mTextures.Length ) / oneFrame;
			if ( !loop && mCount >= oneFrame * mTextures.Length ){
				index = mTextures.Length - 1;
				mIsAnimationEnd = true;
			}
			++mCount;

			return mTextures[ index ];
		}

		public void resetAnimation(){
			mCount = 0;
			mIsAnimationEnd = false;
		}

		public bool isAnimationEnd{
			get{ return mIsAnimationEnd; }
		}
	}
}

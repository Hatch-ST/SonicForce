using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace SonicLate{
	/// <summary>
	/// プレイ中のスコア表示クラス
	/// </summary>
	class Score{
		private MultiTexture mTexNumbers = null;

		/// <summary>合計スコア</summary>
		private UInt64 mTotalValue = 0;

		/// <summary>一時的に格納する変数</summary>
		private UInt64 mBuffer = 0;

		/// <summary>1フレームで加算する量</summary>
		private int mAddAmount = 100;

		/// <summary>各桁の値</summary>
		private int[] mFigures = null;

		/// <summary>桁数</summary>
		private readonly int mFigureLength = 10;

		/// <summary>最大値</summary>
		private readonly UInt64 mMax;
		
		/// <summary>
		/// コンストラクタ
		/// </summary>
		public Score( ContentManager content ){
			mTexNumbers = new MultiTexture( content, "Image/score/num", 10 );
			mFigures = new int[ mFigureLength ];

			mMax = ( UInt64 )Math.Pow( 10, mFigureLength ) - 1;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public Score( ContentManager content, string fileName, int figureLength ){
			mTexNumbers = new MultiTexture( content, fileName, 10 );

			mFigureLength = figureLength;
			mFigures = new int[ mFigureLength ];
			mMax = ( UInt64 )Math.Pow( 10, mFigureLength ) - 1;
		}
		
		/// <summary>
		/// スコアに加算
		/// </summary>
		/// <param name="value">加算する値</param>
		public void Add( int value ){
			value = Math.Max( 0, value );
			mBuffer += ( UInt64 )value;

			if ( mBuffer > mMax ){
				mBuffer = mMax;
			}
		}

		/// <summary>
		/// スコアに加算
		/// </summary>
		/// <param name="value">加算する値</param>
		public void Add( UInt64 value ){
			mBuffer += value;

			if ( mBuffer > mMax ){
				mBuffer = mMax;
			}
		}
		
		/// <summary>
		/// スコアを登録
		/// </summary>
		/// <param name="value">登録する値</param>
		public void Set( UInt64 value ){
			mBuffer = value;
		}

		/// <summary>
		/// 描画する
		/// </summary>
		/// <param name="x">X座標</param>
		/// <param name="y">Y座標</param>
		public void Draw( int x, int y ){
			GetFigure( mFigures, mTotalValue );

			float w = mTexNumbers.Get[ 0 ].Width;
			float h = mTexNumbers.Get[ 0 ].Height;

			for ( int i = 0; i < mFigures.Length; i++ ){
				float destX = x + i * w;
				float destY = y;
				SpriteBoard.Render( mTexNumbers.Get[ mFigures[ mFigures.Length - i - 1 ] ], destX, destY, w, h, 1.0f );
			}

			// 合計スコアを進める
			mTotalValue = Math.Min( mBuffer, mTotalValue + ( UInt64 )mAddAmount ); 
		}

		/// <summary>
		/// 描画が完了しているかどうか
		/// </summary>
		public bool DrawEnd(){
			return ( mTotalValue == mBuffer );
		}

		/// <summary>
		/// 現在のスコア
		/// </summary>
		public UInt64 Value(){
			return mBuffer;
		}

		/// <summary>
		///  桁に分割する
		/// </summary>
		private void GetFigure( int[] figures, UInt64 value ){
			for ( int i = 0; i < figures.Length; i++ ){
				figures[ i ] = ( int )( value % 10 );
				value /= 10;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SonicLate{
	/// <summary>
	/// 直線の補間 ベクター版
	/// </summary>
	class Interpolate3{
		/// <summary>
		/// カウント
		/// </summary>
		private int mCount = 0;

		private bool mReverseFirst = true;

		private Vector3 mStart = Vector3.Zero;
		private Vector3 mEnd = Vector3.Zero;

		private Vector3 mValue = Vector3.Zero;

		/// <summary>
		/// ２点間を直線で補間する
		/// </summary>
		/// <param name="start">最初の値</param>
		/// <param name="end">最後の値</param>
		/// <param name="endTime">補間する時間</param>
		/// <returns>現在の値</returns>
		public Vector3 Get( Vector3 start, Vector3 end, int endTime ){
			// 位置が変わったらカウントを初期化
			if ( mStart != start || mEnd != end ){
				mCount = 0;
			}

			mReverseFirst = true;

			// 開始位置と終了位置を保存
			mStart = start;
			mEnd = end;

			if ( endTime <= 0 ) return mStart;
			float t = ( float )mCount / endTime;
			mCount = Math.Min( mCount + 1, endTime );

			mValue = start + ( end - start ) * t;

			return mValue;
		}
		
		/// <summary>
		/// 登録された２点間を直線で補間する
		/// </summary>
		/// <param name="endTime">補間する時間</param>
		/// <returns>現在の値</returns>
		public Vector3 Get( int endTime ){
			return Get( mStart, mEnd, endTime );
		}

		/// <summary>
		/// ２点間をサインカーブで補間する
		/// </summary>
		/// <param name="start">最初の値</param>
		/// <param name="end">最後の値</param>
		/// <param name="endTime">補間する時間</param>
		/// <returns>現在の値</returns>
		public Vector3 GetSin( Vector3 start, Vector3 end, int endTime ){
			// 位置が変わったらカウントを初期化
			if ( mStart != start || mEnd != end ){
				mCount = 0;
			}

			mReverseFirst = true;

			// 開始位置と終了位置を保存
			mStart = start;
			mEnd = end;

			if ( endTime <= 0 ) return mStart;
			float t = ( float )Math.Sin( Math.PI * 0.5 * ( ( double )mCount / endTime ) );
			mCount = Math.Min( mCount + 1, endTime );
			
			mValue = start + ( end - start ) * t;

			return mValue;
		}
		
		/// <summary>
		/// 登録された２点間をサインカーブで補間する
		/// </summary>
		/// <param name="endTime">補間する時間</param>
		/// <returns>現在の値</returns>
		public Vector3 GetSin( int endTime ){
			return GetSin( mStart, mEnd, endTime );
		}

		/// <summary>
		/// カウントをリセットする
		/// </summary>
		public void Reset(){
			mCount = 0;
		}

		/// <summary>
		/// 開始位置と終了位置を反転させる
		/// </summary>
		public Vector3 GetSinReverse( int endTime ){
			if ( mReverseFirst ){
				mCount = 0;
				mReverseFirst = false;
			}

			if ( endTime <= 0 ) return mEnd;
			float t = ( float )Math.Sin( Math.PI * 0.5 * ( ( double )mCount / endTime ) );
			mCount = Math.Min( mCount + 1, endTime );
			
			mValue = mEnd + ( mStart - mEnd ) * t;

			return mValue;
		}

		/// <summary>
		/// 開始位置と終了位置を登録する
		/// </summary>
		/// <param name="start">最初の値</param>
		/// <param name="end">最後の値</param>
		public void Set( Vector3 start, Vector3 end ){
			mStart = start;
			mEnd = end;
		}

		/// <summary>
		///  現在の時間を取得または登録する
		/// </summary>
		public int Time{
			get { return mCount; }
			set { mCount = value; }
		}

		/// <summary>
		/// 最後に取得した値を取得する
		/// </summary>
		public Vector3 Value{
			get { return mValue; }
		}
	}
}

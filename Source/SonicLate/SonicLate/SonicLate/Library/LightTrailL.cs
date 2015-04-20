using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SonicLate{
	/// <summary>
	/// 光跡 軽量版
	/// </summary>
	class LightTrailL{

		#region パーツクラス
		/// <summary>
		/// パーツクラス
		/// </summary>
		private class Part{
			/// <summary>位置</summary>
			public Vector3 Position = Vector3.Zero;
			
			/// <summary>大きさ</summary>
			public float Scale = 1.0f;
		
			/// <summary>アルファ値</summary>
			public float Alpha = 0.0f;
			
			/// <summary>アルファ値の最大値</summary>
			public float MaxAlpha = 1.0f;

			/// <summary>
			/// 初期化
			/// </summary>
			public void Initialize(){
				Position = Vector3.Zero;
				Scale = 1.0f;
				Alpha = 0.0f;
				MaxAlpha = 1.0f;
			}
		}
		#endregion

		#region フィールド

		/// <summary>パーティクルを構成するパーツ配列</summary>
		private Part[] mParts = null;

		/// <summary>パーティクル全体の位置</summary>
		private Vector3 mPosition = Vector3.Zero;

		/// <summary>パーティクルの総数</summary>
		private int mNumPart;

		/// <summary>有効かどうかのフラグ</summary>
		private bool mEnable = true;
		
		/// <summary>パーティクルの大きさ</summary>
		private float mScale;

		/// <summary>前回の位置</summary>
		private Vector3 mLastPosition = Vector3.Zero;

		/// <summary>2枚のパーティクルの最大距離</summary>
		private float mMaxDistance = 2.5f;

		/// <summary>連結されたビルボード</summary>
		private LedBillBoard mLedBillBoard = null;

		private float mAppearSpeed = 0.0001f;
		private float mHideSpeed = 0.0005f;

		#endregion

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public LightTrailL( GraphicsDevice device, ContentManager content, string fileName, int numPart, float scale, float maxDistance ){
			// パーティクルの総数を登録
			mNumPart = numPart;
			
			// パーティクル間の最大距離を登録
			mMaxDistance = maxDistance;

			// 大きさを登録
			mScale = scale * 0.5f;

			// 連結ビルボードを生成
			mLedBillBoard = new LedBillBoard( device, content, fileName, numPart );

			// 各パーティクルデータを生成
			mParts = new Part[ mNumPart ];
			for ( int i = 0; i < mNumPart; i++ ){
				mParts[ i ] = new Part();

				// アルファ値を登録
				mParts[ i ].MaxAlpha = ( float )( Math.Cos( ( Math.PI * 0.5f * ( ( double )( i + 1 ) / mNumPart ) ) ) );

				//mParts[ i ].Alpha = mParts[ i ].MaxAlpha;
				mParts[ i ].Alpha = 0.0f;

				// 大きさを登録
				mParts[ i ].Scale = mScale * ( float )( 0.25f + Math.Sin( ( Math.PI * ( ( double )( i + 1 ) / mNumPart ) ) ) * 0.75f );

				// 頂点色を登録
				mLedBillBoard.SetBoardColor( i, mParts[ i ].Alpha, 1.0f, 1.0f, 1.0f );
			}

		}
		
		/// <summary>
		/// 初期化する
		/// </summary>
		public void Initialize(){
			for ( int i = 0; i < mNumPart; i++ ){
				// 位置を登録
				mParts[ i ].Position = Vector3.Zero;

				// アルファ値を登録
				mParts[ i ].MaxAlpha = ( float )( Math.Cos( ( Math.PI * 0.5f * ( ( double )( i + 1 ) / mNumPart ) ) ) );
				mParts[ i ].Alpha = 0.0f;

				// 大きさを登録
				mParts[ i ].Scale = mScale * ( float )( 0.25f + Math.Sin( ( Math.PI * ( ( double )( i + 1 ) / mNumPart ) ) ) * 0.75f );

				// 頂点色を登録
				mLedBillBoard.SetBoardColor( i, mParts[ i ].Alpha, 1.0f, 1.0f, 1.0f );
			}
		}
		
		/// <summary>
		/// 更新を行う
		/// </summary>
		public void Update(){
			float sqMaxDis = mMaxDistance * mMaxDistance;

			// 1枚目のパーティクル位置を登録
			mParts[ 0 ].Position = mPosition;

			// 頂点色を登録
			mLedBillBoard.SetBoardColor( 0, mParts[ 0 ].Alpha, 1.0f, 1.0f, 1.0f );

			// 2枚目以降
			for ( int i = 1; i < mNumPart; i++ ){
				Vector3 dif = mParts[ i ].Position - mParts[ i - 1 ].Position;

				// パーティクル間の距離を制限
				if ( dif.LengthSquared() > sqMaxDis ){
					float t = mMaxDistance * ( 1.0f - ( mParts[ i ].Scale / mScale ) );
					dif = Vector3.Normalize( dif ) * ( mMaxDistance - t );
				}

				// パーティクル位置を登録
				mParts[ i ].Position = mParts[ i - 1 ].Position + dif;
				
				// アルファ値を算出
				if ( mEnable ){
					mParts[ i ].Alpha = Math.Min( mParts[ i ].MaxAlpha, mParts[ i ].Alpha + i * mAppearSpeed );
				}else{
					mParts[ i ].Alpha = Math.Max( 0.0f, mParts[ i ].Alpha - ( mNumPart - i ) * mHideSpeed );
				}

				// 頂点色を登録
				mLedBillBoard.SetBoardColor( i, mParts[ i ].Alpha, 1.0f, 1.0f, 1.0f );
			}
		}

		/// <summary>
		/// レンダリングを行う
		/// </summary>
		/// <param name="device">グラフィックスデバイス</param>
		/// <param name="camera">カメラ</param>
		public void Render( GraphicsDevice device, Camera camera ){
			// 全ての頂点の位置を登録
			for ( int i = 0; i < mLedBillBoard.Length; i++ ){
				mLedBillBoard.SetBoardPosition( i, camera, mParts[ i ].Position, mParts[ i ].Scale );
			}

			device.DepthStencilState = DepthStencilState.None;

			mLedBillBoard.Render( camera );

			device.DepthStencilState = DepthStencilState.Default;
		}

		#region プロパティ

		/// <summary>
		/// 全体の座標
		/// </summary>
		public Vector3 Position{
			get{ return mPosition; }
			set{
				mLastPosition = mPosition;
				mPosition = value;
			}
		}

		/// <summary>
		/// パーティクルを表示するかどうか
		/// </summary>
		public bool Enable{
			get{ return mEnable; }
			set{ mEnable = value; }
		}

		#endregion
	}
}

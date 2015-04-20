using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SonicLate{
	/// <summary>
	/// 光跡
	/// </summary>
	class LightTrail{

		#region パーツクラス
		/// <summary>
		/// パーツクラス
		/// </summary>
		private class Part : IComparable{
			/// <summary>位置</summary>
			public Vector3 Position = Vector3.Zero;
			
			/// <summary>大きさ</summary>
			public Vector3 Scale = Vector3.One;
		
			/// <summary>アルファ値</summary>
			public float Alpha = 0.0f;

			/// <summary>深度</summary>
			public float Depth = 0.0f;

			/// <summary>テクスチャ番号</summary>
			public int TextureIndex = 0;

			/// <summary>
			/// 初期化
			/// </summary>
			public void Initialize(){
				Position = Vector3.Zero;
				Scale = Vector3.One;
				Alpha = 0.0f;
				Depth = 0.0f;
				TextureIndex = 0;
			}
			
			/// <summary>
			/// 深度でソート
			/// </summary>
			public int CompareTo( object obj ){
				Part t = ( Part )obj;
				float diff = t.Depth - Depth;

				if ( diff < 0.0f ) return -1;
				else if ( diff > 0.0f ) return 1;
				else return 0;
			}
		}
		#endregion

		#region フィールド

		/// <summary>パーティクルを構成するパーツ配列</summary>
		private Part[] mParts = null;

		/// <summary>ビルボード</summary>
		private BillBoard mBoard = null;

		/// <summary>パーティクル全体の位置</summary>
		private Vector3 mPosition = Vector3.Zero;

		/// <summary>パーティクルの総数</summary>
		private int mNumPart;

		/// <summary>有効かどうかのフラグ</summary>
		private bool mEnable = true;
		
		/// <summary>パーティクルの大きさ</summary>
		private Vector3 mScale;

		/// <summary>前回の位置</summary>
		private Vector3 mLastPosition = Vector3.Zero;

		/// <summary>2枚のパーティクルの最大距離</summary>
		private float mMaxDistance = 2.5f;

		/// <summary>
		/// アルファブレンド設定
		/// </summary>
		private static BlendState mBlendState = new BlendState(){
			ColorSourceBlend = Blend.SourceAlpha,
			ColorDestinationBlend = Blend.One,
			AlphaSourceBlend = Blend.SourceAlpha,
			AlphaDestinationBlend = Blend.InverseSourceAlpha,
		};

		#endregion

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public LightTrail( GraphicsDevice device, ContentManager content, string fileName, int numPart, float scale, float maxDistance ){
			// パーティクルの総数を登録
			mNumPart = numPart;
			
			// パーティクル間の最大距離を登録
			mMaxDistance = maxDistance;

			// 大きさを登録
			mScale = Vector3.One * scale;
			mScale.Z = 1.0f;

			// 板ポリゴンの生成
			mBoard = new BillBoard( content, fileName );

			// 各パーティクルデータを生成
			mParts = new Part[ mNumPart ];
			for ( int i = 0; i < mNumPart; i++ ){
				mParts[ i ] = new Part();

				// アルファ値を登録
				mParts[ i ].Alpha = ( float )( Math.Cos( ( Math.PI * 0.5f * ( ( double )( i + 1 ) / mNumPart ) ) ) );

				// 大きさを登録
				mParts[ i ].Scale = mScale * ( float )( 0.25f + Math.Sin( ( Math.PI * ( ( double )( i + 1 ) / mNumPart ) ) ) * 0.75f );
			}
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public LightTrail( BillBoard billBoard, int numPart, float scale, float maxDistance ){
			// パーティクルの総数を登録
			mNumPart = numPart;

			// パーティクル間の最大距離を登録
			mMaxDistance = maxDistance;

			// 大きさを登録
			mScale = Vector3.One * scale;
			mScale.Z = 1.0f;

			// 板ポリゴンを登録
			mBoard = billBoard;

			// 各パーティクルデータを生成
			mParts = new Part[ mNumPart ];
			for ( int i = 0; i < mNumPart; i++ ){
				mParts[ i ] = new Part();

				// アルファ値を登録
				mParts[ i ].Alpha = ( float )( Math.Cos( ( Math.PI * 0.5f * ( ( double )i / ( mNumPart -1 ) ) ) ) );

				// 大きさを登録
				mParts[ i ].Scale = mScale * ( float )( 0.25f + Math.Sin( ( Math.PI * ( ( double )i / ( mNumPart -1 ) ) ) * 0.75f ) );
			}
		}

		/// <summary>
		/// 更新を行う
		/// </summary>
		public void Update(){
			float sqMaxDis = mMaxDistance * mMaxDistance;

			// 1枚目のパーティクル位置を登録
			mParts[ 0 ].Position = mPosition;

			// 2枚目以降
			for ( int i = 1; i < mNumPart; i++ ){
				Vector3 dif = mParts[ i ].Position - mParts[ i - 1 ].Position;

				// パーティクル間の距離を制限
				if ( dif.LengthSquared() > sqMaxDis ){
					float t = mMaxDistance * 1.0f * ( 1.0f - ( mParts[ i ].Scale.X / mScale.X ) );
					dif = Vector3.Normalize( dif ) * ( mMaxDistance - t );
				}

				// パーティクル位置を登録
				mParts[ i ].Position = mParts[ i - 1 ].Position + dif;
			}
		}

		/// <summary>
		/// レンダリングを行う
		/// </summary>
		/// <param name="device">グラフィックスデバイス</param>
		/// <param name="camera">カメラ</param>
		public void Render( GraphicsDevice device, Camera camera ){
			if ( mBoard == null || mParts == null ) return;

			mBoard.Begin( camera, mBlendState );

			for ( int i = 0; i < mParts.Length; i++ ){
				device.DepthStencilState = DepthStencilState.None;
				ModelStates states = new ModelStates( mParts[ i ].Position, Vector3.Zero, mParts[ i ].Scale, null );

				mBoard.Render( states, mParts[ i ].Alpha, mParts[ i ].TextureIndex );
			}

			mBoard.End();
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

		/// <summary>
		/// ビルボードを取得する
		/// </summary>
		public BillBoard Board{
			get{ return mBoard; }
		}

		#endregion
	}
}

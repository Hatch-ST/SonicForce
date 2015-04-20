using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SonicLate{
	/// <summary>
	/// パーティクルクラス
	/// </summary>
	class Particle{
		#region パーティクルタイプ
		/// <summary>
		/// パーティクルタイプ列挙体
		/// </summary>
		public enum Type{
			Fire, // 炎
			Smoke, // 煙
			Snow, // 雪
			None,
		}
		#endregion

		#region パーツクラス
		/// <summary>
		/// パーツクラス
		/// </summary>
		private class Part : IComparable{
			/// <summary>状態</summary>
			public ModelStates state = new ModelStates( null );
			
			/// <summary>移動量</summary>
			public Vector3 move = Vector3.Zero;

			/// <summary>アルファ値</summary>
			public float alpha;

			/// <summary>カウンタ</summary>
			public int count;

			/// <summary>移動終了時間</summary>
			public int endTime;

			/// <summary>深度</summary>
			public float depth;

			/// <summary>テクスチャ番号</summary>
			public int textureIndex;
			
			/// <summary>
			/// 深度でソート
			/// </summary>
			public int CompareTo( object obj ){
				Part t = ( Part )obj;
				float diff = t.depth - depth;

				if ( diff < 0.0f ) return -1;
				else if ( diff > 0.0f ) return 1;
				else return 0;
			}
		}
		#endregion

		#region フィールド

		/// <summary>乱数生成クラス</summary>
		private Random mRandom = new Random();

		/// <summary>パーティクルを構成するパーツ配列</summary>
		private Part[] mParts = null;

		/// <summary>ビルボード</summary>
		private BillBoard mBoard = null;

		/// <summary>パーティクル全体の位置</summary>
		private Vector3 mPos;

		/// <summary>重力</summary>
		private Vector3 mGravity = new Vector3( 0.0f, -5.0f, 0.0f );

		/// <summary>パーティクルタイプ</summary>
		private Type mType;

		/// <summary>パーティクルの総数</summary>
		private int mNumPart;

		/// <summary>有効かどうかのフラグ</summary>
		private bool mEnable = true;

		/// <summary>Zソートするかどうかのフラグ</summary>
		private bool mEnableZSort = false;
		
		/// <summary>Zテストするかどうかのフラグ</summary>
		private bool mEnableZTest = true;

		/// <summary>加算ブレンドするかどうかのフラグ</summary>
		private bool mEnableBlendAdd = false;

		/// <summary>座標の最大値</summary>
		private float mMaxPosX, mMaxPosY;

		/// <summary>大きさの最大最小値</summary>
		private float mMaxScaleXY, mMinScaleXY;

		/// <summary>消えるまでの時間の最大最小値</summary>
		private int mMinTime, mMaxTime;

		/// <summary>移動スピードの最大最小値</summary>
		private float mMinSpeed, mMaxSpeed;
		
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

		#region コンストラクタ
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="device">グラフィックスデバイス</param>
		/// <param name="content">コンテンツマネージャ</param>
		/// <param name="fileName">ファイル名</param>
		/// <param name="numPart">パーティクルの総数</param>
		/// <param name="type">パーティクルの種類</param>
		/// <param name="maxPosX">X座標の最大値</param>
		/// <param name="maxPosY">Y座標の最大値</param>
		/// <param name="maxScaleXY">大きさの最大値</param>
		/// <param name="minScaleXY">大きさの最小値</param>
		/// <param name="maxSpeed">速さの最大値</param>
		/// <param name="minSpeed">速さの最小値</param>
		/// <param name="maxTime">消えるまでの最大時間</param>
		/// <param name="minTime">消えるまでの最小時間</param>
		public Particle(
		GraphicsDevice device,
		ContentManager content,
		string fileName,
		int numPart,
		Type type,
		float maxPosX, float maxPosY,
		float maxScaleXY, float minScaleXY,
		float maxSpeed, float minSpeed,
		int maxTime, int minTime ){
			// パーティクルの総数を登録
			mNumPart = numPart;

			// 板ポリゴンの生成
			mBoard = new BillBoard( content, fileName );

			// 各パーティクルデータを生成
			mParts = new Part[ mNumPart ];
			for ( int i = 0; i < mNumPart; i++ ){
				mParts[ i ] = new Part();
			}

			SetStates( type, maxPosX, maxPosY, maxScaleXY, minScaleXY, maxSpeed, minSpeed, maxTime, minTime );
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="device">グラフィックスデバイス</param>
		/// <param name="content">コンテンツマネージャ</param>
		/// <param name="fileName">ファイル名</param>
		/// <param name="numPart">パーティクルの総数</param>
		/// <param name="type">パーティクルの種類</param>
		public Particle( GraphicsDevice device, ContentManager content, string fileName, int numPart, Type type ){
			// パーティクルの総数を登録
			mNumPart = numPart;

			// 板ポリゴンの生成
			mBoard = new BillBoard( content, fileName );

			// 各パーティクルデータを生成
			mParts = new Part[ mNumPart ];
			for ( int i = 0; i < mNumPart; i++ ){
				mParts[ i ] = new Part();
			}

			switch ( type ){
				case Type.Snow:
					SetStates( type, 1000.0f, 100.0f, 30.0f, 8.0f, 4.0f, 1.0f, 100, 60 );
					break;
				default:
					SetStates( type, 100.0f, 100.0f, 50.0f, 20.0f, 10.0f, 2.0f, 60, 20 );
					break;
			}
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="billBoard">ポリゴンへの参照</param>
		/// <param name="numPart">パーティクルの総数</param>
		/// <param name="type">パーティクルの種類</param>
		public Particle( BillBoard billBoard, int numPart, Type type ){
			// パーティクルの総数を登録
			mNumPart = numPart;

			// 板ポリゴンの生成
			mBoard = billBoard;

			// 各パーティクルデータを生成
			mParts = new Part[ mNumPart ];
			for ( int i = 0; i < mNumPart; i++ ){
				mParts[ i ] = new Part();
			}

			switch ( type ){
				case Type.Snow:
					SetStates( type, 1000.0f, 100.0f, 30.0f, 8.0f, 4.0f, 1.0f, 100, 60 );
					break;
				default:
					SetStates( type, 60.0f, 10.0f, 50.0f, 20.0f, 10.0f, 2.0f, 60, 20 );
					break;
			}
		}


		#endregion

		#region 初期化
		/// <summary>
		/// 初期化処理を行う
		/// </summary>
		public void Initialize(){
			for ( int i = 0; i < mNumPart; i++ ){
				Reset( i );
			}
		}
		#endregion

		#region 更新
		/// <summary>
		/// 更新を行う
		/// </summary>
		public void Update(){
			for ( int i = 0; i < mNumPart; i++ ){
				if ( mParts[ i ].count <= mParts[ i ].endTime ){
					// 移動
					mParts[ i ].state.Position += mParts[ i ].move;

					// アルファ
					float alpha = ( float )( ( float )mParts[ i ].count / mParts[ i ].endTime ) * 2.0f; // 0.0f ～ 2.0f
					if ( alpha >= 1.0f ) alpha = 1.0f - ( alpha - 1.0f );

					// アルファ値を登録
					mParts[ i ].alpha = alpha;

				    ++mParts[ i ].count;
				}
				// リセット
				else if ( mEnable ){
				    Reset( i );
				}
			}
		}
		#endregion

		#region レンダリング
		/// <summary>
		/// レンダリングを行う
		/// </summary>
		/// <param name="device">グラフィックスデバイス</param>
		/// <param name="camera">カメラ</param>
		public void Render( GraphicsDevice device, Camera camera ){
			if ( mBoard == null || mParts == null ) return;

			if ( mEnableBlendAdd ){
			}

			// 描画開始処理
			mBoard.Begin( camera, mBlendState );

			// Zソート有り
			if ( mEnableZSort ){
				// Z値を算出
				Vector3 pos = camera.Position;
				Vector3 tar = camera.Target;
				Vector3 pt = tar - pos;
				pt.Normalize();
				for ( int i = 0; i < mNumPart; i++ ){
					//mParts[ i ].depth = Vector3.Dot( ( mParts[ i ].state.Position + mPos ), pt );
					mParts[ i ].depth = Vector3.Dot( ( mParts[ i ].state.Position ), pt );
				}

				// ソート
				Array.Sort( mParts );

				// レンダリング
				for ( int i = 0; i < mNumPart; i++ ){
					if ( mParts[ i ].alpha > 0.0f ){
						ModelStates states = mParts[ i ].state.CopyPositionAngleScale();
						//states.Position += mPos;

						// Z値が同じなら重ならないようにずらす
						if ( i > 0 && i < mNumPart-1 && mParts[ i ].depth == mParts[ i-1 ].depth ){
							states.Position -= pt * ( mParts[ i ].depth - mParts[ i+1 ].depth );
						}
						mBoard.Render( mParts[ i ].state, mParts[ i ].alpha, mParts[ i ].textureIndex );
					}
				}
			}
			// Zソートなし
			else{
				for ( int i = 0; i < mNumPart; i++ ){
					if ( mParts[ i ].alpha > 0.0f ){
						if ( !mEnableZTest ) device.DepthStencilState = DepthStencilState.None;

						ModelStates states = mParts[ i ].state.CopyPositionAngleScale();
						//states.Position += mPos;
						mBoard.Render( mParts[ i ].state, mParts[ i ].alpha, mParts[ i ].textureIndex );
					}
				}
				device.DepthStencilState = DepthStencilState.Default;
			}

			mBoard.End();
		}
		#endregion

		#region パーティクル情報の登録
		/// <summary>
		/// パーティクル情報を登録する
		/// </summary>
		/// <param name="type">パーティクルの種類</param>
		/// <param name="maxPosX">X座標の最大値</param>
		/// <param name="maxPosY">Y座標の最大値</param>
		/// <param name="maxScaleXY">大きさの最大値</param>
		/// <param name="minScaleXY">大きさの最小値</param>
		/// <param name="maxSpeed">速さの最大値</param>
		/// <param name="minSpeed">速さの最小値</param>
		/// <param name="maxTime">消えるまでの最大時間</param>
		/// <param name="minTime">消えるまでの最小時間</param>
		public void SetStates(
		Type type,
		float maxPosX, float maxPosY,
		float maxScaleXY, float minScaleXY,
		float maxSpeed, float minSpeed,
		int maxTime, int minTime ){
			mType = type;
			mMaxPosX = maxPosX;
			mMaxPosY = maxPosY;
			mMaxScaleXY = maxScaleXY;
			mMinScaleXY = minScaleXY;
			mMinSpeed = minSpeed;
			mMaxSpeed = maxSpeed;
			mMinTime = minTime;
			mMaxTime = maxTime;
	
			mEnableZSort = mEnableBlendAdd = false;
			switch ( mType ){
				case Type.Snow:
					// Zソート有り
					mEnableZSort = true;
					mEnableBlendAdd = false;
					break;
				default:
					// 加算ブレンド
					mEnableBlendAdd = true;
					break;
			}

			// 初期化
			Initialize();
		}
		
		/// <summary>
		/// パーティクルを置く範囲を設定する
		/// </summary>
		/// <param name="x">X座標の最大値</param>
		/// <param name="y">Y座標の最大値</param>
		public void SetMaxPosition( float x, float y ){
			mMaxPosX = x;
			mMaxPosY = y;
		}

		/// <summary>
		/// パーティクルの大きさの範囲を指定する
		/// </summary>
		/// <param name="max">最大値</param>
		/// <param name="min">最小値</param>
		public void SetScaleXYRange( float max, float min ){
			mMaxScaleXY = max;
			mMinScaleXY = min;
		}

		/// <summary>
		/// パーティクルが消えるまでの時間の範囲を指定する
		/// </summary>
		/// <param name="max">最大値</param>
		/// <param name="min">最小値</param>
		public void SetTimeRange( int max, int min ){
			mMaxTime = max;
			mMinTime = min;
		}

		/// <summary>
		/// パーティクルの移動する速さの範囲を指定する
		/// </summary>
		/// <param name="max">最大値</param>
		/// <param name="min">最小値</param>
		public void SetSpeedRange( float max, float min ){
			mMaxSpeed = max;
			mMinSpeed = min;
		}

		#endregion

		#region パーツのリセット
		/// <summary>
		/// 指定したパーツの情報をリセットする
		/// </summary>
		/// <param name="index">パーツのインデックス</param>
		private void Reset( int index ){
			// 一時的にコピー
			Part part = mParts[ index ];

			switch ( mType ){
				case Type.Snow:
					// 座標を登録
					float x = ( float )( mRandom.NextDouble() * mMaxPosX * 2.0f - mMaxPosX );
					float y = ( float )( mRandom.NextDouble() * mMaxPosY );
					float z = ( float )( mRandom.NextDouble() * mMaxPosX * 2.0f - mMaxPosX );
					part.state.Position = new Vector3( ( float )( mRandom.NextDouble() * mMaxPosX * 2.0f - mMaxPosX ),
													   ( float )( mRandom.NextDouble() * mMaxPosY ),
													   ( float )( mRandom.NextDouble() * mMaxPosX * 2.0f - mMaxPosX ) );

					part.state.Position += mPos;

					// 移動ベクトルを登録
					part.move = Vector3.Zero;
					part.move.Y = Math.Max( ( float )mRandom.NextDouble() * mMaxSpeed, mMinSpeed );
					break;
				default:
					// 座標を登録
					part.state.Position = new Vector3( ( float )( mRandom.NextDouble() * mMaxPosX ), ( float )( mRandom.NextDouble() * mMaxPosY ), 0.0f );
					

					// Y軸で回転
					Matrix rotaY = Matrix.CreateRotationY( ( float )( mRandom.NextDouble() * Math.PI * 4.0f ) );
					part.state.Position = Vector3.Transform( part.state.Position, rotaY );

					part.state.Position += mPos;

					// 移動ベクトルを登録
					part.move = Vector3.Zero;
					part.move.Y = Math.Max( ( float )mRandom.NextDouble() * mMaxSpeed, mMinSpeed );
					break;
			}

			// サイズを登録
			part.state.Scale = Vector3.One;
			part.state.Scale *= Math.Max( ( float )mRandom.NextDouble() * mMaxScaleXY, mMinScaleXY );
			part.state.ScaleZ = 1.0f;

			// 角度を登録
			part.state.Angle = Vector3.Zero;

			part.alpha = 0.0f;
			part.count = 0;
			part.endTime = mRandom.Next( mMinTime, mMaxTime );

			part.depth = 0.0f;

			part.textureIndex = mRandom.Next( mBoard.NumTexture );

		}
		#endregion

		#region プロパティ

		/// <summary>
		/// 全体の座標
		/// </summary>
		public Vector3 Position{
			get{ return mPos; }
			set{ mPos = value; }
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

		public bool ZTestEnable{
			set {
				mEnableZTest = value;
				if ( !value ){
					mEnableZSort = false;
				}
			}
		}

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SonicLate{
	/// <summary>
	/// 敵 ハリセンボン
	/// </summary>
	class EBurrfish : Enemy{
		/// <summary>
		/// 棘状態モデル
		/// </summary>
		private HLModel[] mModels = new HLModel[ 2 ];

		private int mModelIndex = 0;

		private int mCount = 0;

		private Interpolate3 mInterpolate = null;

		private Vector3 mMove;

		private Vector3 mAngleDefault = Vector3.Zero;
		private Vector3 mAngleAsisst = Vector3.Zero;

		/// <summary>次の移動</summary>
		private Vector3 mNextMove;

		/// <summary>向きを変えるまでの移動距離</summary>
		private float mDistanceToTurn;

		private const float mSpeed = 8.0f;
		private const float mRangeToChangeModel = 1000.0f;
		private const float mMinScale = 2.0f;
		private const float mMaxScale = 3.0f;
		private const int mScalingEndTime = 20;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="model">モデルへの参照</param>
		/// <param name="position">原点となる座標</param>
		/// <param name="angle">角度</param>
		public EBurrfish( HLModel model, HLModel model2, Vector3 position, Vector3 angle ){
			mName = Names.Burrfish;
			
			// モデルを登録
			mModel = model2;
			mModels[ 0 ] = model;
			mModels[ 1 ] = model2;

			// モデル情報を初期化
			mStates = new ModelStates( mModel.SkinData );
			mStates.SetAnimation( mModel, "Take 001", true, 0.0f );
			mStates.Scale *= mMinScale;
			mStates.Position = position;

			mAngleDefault = angle;

			// 原点を登録
			mPosition = position;

			// 境界球の情報を初期化
			mColPosition = new Vector3[ 1 ];
			mColPosition[ 0 ] = position;
			mColRange = new float[ 1 ];
			mColRange[ 0 ] = 50.0f;

			// 移動を初期化
			mNextMove = new Vector3( ( float )mRandom.NextDouble() - 0.5f, ( float )mRandom.NextDouble() - 0.5f, -( float )mRandom.NextDouble() );
			mNextMove.Normalize();
			mNextMove *= mSpeed * ( float )( 0.5 + mRandom.NextDouble() * 0.5 );

			// カウントをランダムに初期化
			mCount = mRandom.Next( 30 );

			// 行動開始判定の範囲
			mRangeToActive = 5000.0f;

			// 初めは待機
			mActive = false;

			// 補間クラスを初期化
			mInterpolate = new Interpolate3();
			mInterpolate.Set( mStates.Scale, mStates.Scale );

			mDistanceToTurn = 0.0f;
		}
		
		/// <summary>
		/// 更新を行う
		/// </summary>
		public override void Update( Camera camera, Player player, Stage stage, bool enableDelete ){
			base.Update( camera, player, stage, enableDelete );

			// プレイヤーが近くにいれば行動開始
			if ( !mActive && Collision.TestIntersectShere( player.Position, 1.0f, mStates.Position, mRangeToActive ) ){
				mActive = true;
			}

			float speed = ( player.OnStop() ? 0.0f : ( player.OnSlow() ? mSlowSpeedRate : 1.0f ) );

			if ( mActive ){
				// カメラに映っていなければ無効化
				float dot = Vector3.Dot( mStates.Position - camera.Position, Vector3.Normalize( camera.Target - camera.Position ) );
				if ( dot < 0.0f && enableDelete ){
					mDisable = true;
				}

				// モデルを切り替える
				if ( Collision.TestIntersectShere( player.Position, 1.0f, mStates.Position, mRangeToChangeModel ) ){
					if ( mModelIndex != 1 ){
						mInterpolate.Reset();
						mInterpolate.Set( mStates.Scale, Vector3.One * mMaxScale );
						mDistanceToTurn = 0.0f;
					}
					mModelIndex = 1;
					mStates.Scale = mInterpolate.Get( mScalingEndTime );
				}else{
					if ( mModelIndex != 0 ){
						mInterpolate.Reset();
						mInterpolate.Set( mStates.Scale, Vector3.One * mMinScale );
					}
					mModelIndex = 0;
					mStates.Scale = mInterpolate.Get( mScalingEndTime );
				}

				// プレイヤー状態を加味した移動量
				Vector3 move = mMove * speed;

				// 移動した距離を減算
				mDistanceToTurn -= move.Length();

				if ( mDistanceToTurn <= 0.0f ){
					// 次の移動を登録
					mMove = mNextMove;

					// 壁との交点からさらに次の移動を算出
					Vector3 p, normal;
					Vector3[] vertices;
					if ( stage.CollisionModel.IsIntersect( mStates.Position, mStates.Position + Vector3.Normalize( mMove ) * 5000.0f, out p, out vertices, out normal ) ){
						float angle;
						Vector3 axis;
						Matrix matrix;

						// １度目の回転
						angle = ( float )( Math.PI * 0.5 * mRandom.NextDouble() );
						axis = vertices[ 0 ] - p;
						axis.Normalize();
						matrix = Matrix.CreateFromAxisAngle( axis, angle );
						mNextMove = Vector3.Transform( normal, matrix );

						// ２度目の回転
						angle = ( float )( Math.PI * 2.0 * mRandom.NextDouble() );
						axis = normal;
						axis.Normalize();
						matrix = Matrix.CreateFromAxisAngle( axis, angle );
						mNextMove = Vector3.Transform( mNextMove, matrix );

						//mNextMove.Z = 0.0f;
						mNextMove.Normalize();

						float speedRate = ( float )( 0.5 + mRandom.NextDouble() * 0.5 );

						// 速さを乗算
						mNextMove *= mSpeed * speedRate;

						// 距離を登録
						mDistanceToTurn = ( p - mStates.Position ).Length() * 0.8f - mMove.Length();
					}
					// 当たらなければ適当な値を入れておく
					else{
						// 止まれ
						mNextMove = Vector3.Zero;
						mDistanceToTurn = float.MaxValue;
					}

					move = mMove * speed;
				}

				// 移動
				mStates.Position += move;
			}

			// 境界球の位置を登録
			mColPosition[ 0 ] = mStates.AnimPlayer.GetBonePosition( 0 );
			
			// 傾けてみたり
			mAngleAsisst.Z = ( float )( Math.Sin( mCount / 20.0 ) * Math.PI * 0.10 );
			mAngleAsisst.X = ( float )( Math.Sin( mCount / 40.0 ) * Math.PI * 0.10 );

			mStates.Angle = mAngleDefault + mAngleAsisst;

			// アニメーションスピードを変更
			mAnimationSpeed = speed;

			if ( speed > 0.0f ){
				++mCount;
			}
		}

		/// <summary>
		/// 描画を行う
		/// </summary>
		/// <param name="time">時間</param>
		/// <param name="camera">カメラ</param>
		public override void Draw( GameTime time, Camera camera, EffectManager.Type type ){
			if ( mActive && !mDisable ){
				mModels[ mModelIndex ].Render( camera, mStates, time, mAnimationSpeed, null, Vector4.One * mBright, true, type );
				
				// 境界球の位置を表示
				//Circle.Draw( camera, mColPosition[ 0 ], new Vector2( mColRange[ 0 ] * 2.0f, mColRange[ 0 ] * 2.0f ) );
			}
		}
	}
}

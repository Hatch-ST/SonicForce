using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SonicLate{
	class EBigHeteroconger : Enemy{

		private const int mNumBodies = 7;
		private const float mShiftDistance = 100.0f;
		private const float mCaveRadius = 5000.0f;
		private const float mMoveSpeed = 40.0f;
		private const float mBodyDistance = 4500.0f;

		private readonly float mRangeToActiveForLast = 0.0f;

		private Body[] mBodies = null;

		private Texture2D mTexture = null;


		/// <summary>
		/// 体クラス
		/// </summary>
		private class Body{
			public ModelStates mStates = null;
			public OBB mOBB = null;
			private Vector3 mMove = Vector3.Zero;
			private float mTotalMovedDistance = 0.0f;
			private const float mMoveEndTime = 200.0f;

			private float mTime = 0;

			public Vector3 mPosition;
			private bool mActive = false;
			private bool mExActive = false;

			public Body( HLModel model, Vector3 position, Vector3 direction, Vector3 origin ){
				mStates = new ModelStates( model.SkinData );
				mStates.Position = position;
				mStates.SetAngleFromDirection( direction );
				mStates.Scale *= 30.0f;
				
				mStates.SetAnimation( model, "teishi", true, 0.0f );
				mStates.AdvanceAnimation( 0 );

				mMove = direction * mMoveSpeed;

				Vector3 scale = new Vector3( 6.0f, 8.0f, 130.0f ) * mStates.Scale;

				mOBB = new OBB( Vector3.One );
				mOBB.Scale = scale * 0.5f;
				mOBB.SetRotation( mStates.Angle );

				mPosition = origin;
			}

			public void Update( Player player, float rangeToActive ){
				mExActive = mActive;

				// プレイヤーが近くにいれば行動開始
				if ( !mActive && Collision.TestIntersectShere( player.Position, 1.0f, mPosition, rangeToActive ) ){
					mActive = true;
				}

				if ( mActive ){
					float speedRate = player.OnSlow() ? player.ZoneScrollSpeedRatio : 1.0f;
					if ( player.OnStop() || player.IsDied ) speedRate = 0.0f;

					mTotalMovedDistance += mMoveSpeed * speedRate;

					mStates.Position += mMove * speedRate;
					mOBB.Position = mStates.Position;

					mTime += 1.0f * speedRate;
				}
			}

			public bool IsEnd(){
				return ( mTime >= mMoveEndTime );
			}

			public bool IsMoved(){
				return ( mTotalMovedDistance > 0.0f );
			}
			
			public bool JustActive(){
				return ( !mExActive && mActive );
			}
		}

		public EBigHeteroconger( HLModel model, Texture2D texture, int id, Vector3 startPosition, Vector3 angle ){
			mName = Names.BigHeteroconger;
			
			mModel = model;
			mTexture = texture;

			mStates = new ModelStates( null );
						
			// 原点を登録
			mPosition = startPosition;

			Matrix conv = Matrix.CreateRotationZ( angle.Z ) * Matrix.CreateRotationX( angle.X ) * Matrix.CreateRotationY( angle.Y );
			Vector3 endPosition = startPosition + Vector3.Transform( new Vector3( 0, 0, -1 ), conv ) * 4000.0f * mNumBodies;
			
			Vector3 moveDir = Vector3.Normalize( endPosition - startPosition );

			// 左右向きのベクトル
			Vector3 rightLeft = Vector3.Normalize( Vector3.Cross( moveDir, Vector3.Up ) );

			// 上下向きのベクトル
			Vector3 upDown = Vector3.Normalize( Vector3.Cross( moveDir, rightLeft ) );

			// 初期位置
			Vector3 first;
			first = rightLeft * mCaveRadius;
			first = Vector3.Transform( first, Matrix.CreateFromAxisAngle( moveDir, ( float )( mRandom.NextDouble() * Math.PI * 2.0f ) ) );
			first += startPosition;

			// 体を生成
			mBodies = new Body[ mNumBodies ];
			for ( int i = 0; i < mNumBodies; i++ ){
				// ターゲット
				Vector3 target = startPosition + moveDir * ( mBodyDistance * i );

				// ターゲットをランダムにずらす
				target += rightLeft * ( mShiftDistance * ( mRandom.Next( 3 ) - 1 ) );
				target += upDown * ( mShiftDistance * ( mRandom.Next( 3 ) - 1 ) );

				// 移動する向き
				Vector3 direction = Vector3.Normalize( ( target - first ) + ( moveDir * ( mBodyDistance * 0.2f ) ) );

				// 体を生成
				mBodies[ i ] = new Body( mModel, first, direction, startPosition + moveDir * ( mBodyDistance * i ) );
				
				// 次の体の初期位置を登録
				Vector3 targetToNext = Vector3.Normalize( target - first ) * mCaveRadius;
				float tempAngle = ( float )( ( mRandom.NextDouble() * ( ( Math.PI * 2.0 ) / 3.0 ) ) - ( Math.PI / 3.0 ) );
				targetToNext = Vector3.Transform( targetToNext, Matrix.CreateFromAxisAngle( moveDir, tempAngle ) );
				first += targetToNext + Vector3.Normalize( target - first ) * mCaveRadius;
				first += moveDir * mBodyDistance;
			}
			
			// 行動開始判定の範囲
			mRangeToActive = 2000.0f;

			mRangeToActiveForLast = mRangeToActive + id * 200.0f;

			// 初めは待機
			mActive = false;
		}

		public override void Update( Camera camera, Player player, Stage stage, bool enableDelete ){
			base.Update( camera, player, stage, false );

			// プレイヤーが近くにいれば行動開始
			if ( !mActive && Collision.TestIntersectShere( player.Position, 1.0f, mPosition, mRangeToActive ) ){
				mActive = true;
			}

			if ( mActive ){
				for ( int i = 0; i < mNumBodies; i++ ){
					float range = ( i != ( mNumBodies - 1 ) ) ? mRangeToActive : mRangeToActiveForLast;
					mBodies[ i ].Update( player, range );

					// ギリ避け復活
					if ( mBodies[ i ].JustActive() ){
						mAvoided = false;
					}
				}

				if ( mBodies[ mBodies.Length - 1 ].IsEnd() ){
					mActive = false;
					mDisable = true;
				}
			}
		}

		public override bool IsIntersect( Vector3 position, float collisionRange, bool toAvoid ){
			for ( int i = 0; i < mBodies.Length; i++ ){
				if ( mBodies[ i ].mOBB.IsIntersect( position, collisionRange ) ){
					return true;
				}
			}
			return false;
		}

		public override void Draw( GameTime time, Camera camera, EffectManager.Type type ){
			if ( mActive && !mDisable ){
				for ( int i = 0; i < mNumBodies; i++ ){
					if ( mBodies[ i ].IsMoved() ){			
						mModel.SetTexture( mTexture );
						mModel.Render( camera, mBodies[ i ].mStates, time, mAnimationSpeed, null, Vector4.One * mBright, true, type );
						mModel.SetTexture( null );
					}
				}
			}
		}
	}
}

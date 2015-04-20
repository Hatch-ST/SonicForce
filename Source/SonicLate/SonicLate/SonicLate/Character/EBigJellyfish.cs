using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SonicLate{
	class EBigJellyfish : Enemy{

		private Curve mMoveCurve = null;

		private HLModel mModelCore = null;
		private ModelStates mCoreStates = null;

		private int mCount = 0;
		private float mRotaitionSpeed = 0.1f;

		private float mAngle = 0.0f;

		private Bullet[] mBullets = null;
		private float mBulletDistance = 300.0f;

		private int mSetCount = 0;
		private int mShootCount = 0;
		private int[] mShootPattern = { 0, 1, 2, 1, 0, 2, 0, 1, 2 };

		private Vector3 mAllTimeMove = new Vector3( 0.0f, 0.0f, -20.0f );

		private Texture2D mTexture;
		private MultiTexture mTexEnergy = null;

		private const float mDrawRange = 9000.0f;
		private bool mDrawEnable = false;
		
		/// <summary>
		/// 弾クラス
		/// </summary>
		class Bullet{
			private HLModel mModel = null;
			private ModelStates mStates = null;
			private MultiTexture mTexEnergy = null;
			private Vector3 mMove = Vector3.Zero;

			private BillBoard mBoard = null;
			
			private bool mActive = false;
			private bool mLastActive = false;
			private bool mShoot = true;
			private bool mBig = false;

			private Vector3 mRotationVec = Vector3.Zero;
			private float mRotationAngle = 0.0f;
			private float mRotationVecDistance = 0.0f;

			private const float mNormalScale  = 200.0f;
			private const float mBigScale = 500.0f;
			private float mMaxScale = 1.0f;

			private int mCount = 0;
			private int mScalingEndTime = 20;

			private float mAlwaysScalingRate = 1.0f;
			private int mAlwaysScalingCount = 0;

			private const int mBigScalingEndTime = 40;
			private const int mNormalScalingEndTime = 20;

			private bool mStreat = false;

			private bool mOut = false;

			private const float mMaxHomingDistance = 2.0f;
			private const float mMoveSpeed = 20.0f;
			private const float mBigMoveSpeed = 12.0f;
			private float mCollisionRange = 0.0f;
			private readonly float mNormalCollisionRange = 60.0f;
			private readonly float mBigCollisionRange = 140.0f;

			private float mMovedDistance = 0.0f;
			private float mMaxHomingEnableDistance = 2000.0f;

			public Bullet( HLModel model, MultiTexture texEnergy ){
				mModel = model;
				mStates = new ModelStates( null );

				mBoard = new BillBoard();

				mTexEnergy = texEnergy;

			}

			public bool Set( bool big ){
				if ( !mActive ){
					mActive = true;
					mLastActive = mActive;
					mStates.Scale = Vector3.Zero;
					mCount = -1;
					mShoot = false;
					mOut = false;
					mMovedDistance = 0.0f;
					
					mRotationVec = Vector3.Zero;
					mRotationAngle = 0.5f;
					mRotationVecDistance = 0.0f;

					mAlwaysScalingCount = 0;

					mBig = big;
					if ( mBig ){
						mMaxScale = mBigScale;
						mCollisionRange = mBigCollisionRange;
						mScalingEndTime = mBigScalingEndTime;
					}else{
						mMaxScale = mNormalScale;
						mCollisionRange = mNormalCollisionRange;
						mScalingEndTime = mNormalScalingEndTime;
					}

					return true;
				}

				return false;
			}

			public void Update( Camera camera, Vector3 waitingPosition, Vector3 targetPosition, float speedRate ){
				mLastActive = mActive;

				if ( mActive ){
					// だんだん大きく
					if ( mCount < mScalingEndTime ){
						mStates.Scale += ( Vector3.One * mMaxScale ) / mScalingEndTime;
						mStates.Position = waitingPosition;
					}
					// ショット
					else if ( mShoot ){
						// でかいのは回転
						if ( mBig ){
							if ( mStreat ){
								mStates.Position += mMove * speedRate;
							}else{
								Vector3 exRotationVec = mRotationVec;
								mRotationAngle += 0.2f * speedRate;
								mRotationVecDistance += 7.5f * speedRate;
								mRotationVec = Vector3.Normalize( Vector3.Cross( mMove, Vector3.Up ) ) * mRotationVecDistance;
								mRotationVec = Vector3.Transform( mRotationVec, Matrix.CreateFromAxisAngle( Vector3.Normalize( mMove ), mRotationAngle ) );

								mStates.Position += mMove * speedRate + ( mRotationVec - exRotationVec );
							}
						}
						// 小さいのはホーミング
						else{
							if ( !mOut ){
								Vector3 toTarget = Vector3.Normalize( targetPosition - mStates.Position ) * mMoveSpeed;
								Vector3 homing = toTarget - mMove;
								if ( homing.Length() > mMaxHomingDistance ){
									homing = Vector3.Normalize( homing ) * mMaxHomingDistance;
								}
								mMove = Vector3.Normalize( mMove + homing ) * mMoveSpeed;

								// 外れる
								if ( ( targetPosition - mStates.Position ).Length() < 300.0f || ( mMovedDistance > mMaxHomingEnableDistance ) ){
									mOut = true;
								}
							}
							mStates.Position += mMove * speedRate;
							mMovedDistance += mMove.Length() * speedRate;
						}

					}
					// 待機
					else{
						mStates.Position = waitingPosition;
					}

					// カメラに映っていなければ無効化
					if ( mShoot ){
						float dot = Vector3.Dot( mStates.Position - camera.Position, Vector3.Normalize( camera.Target - camera.Position ) );
						if ( dot < 0.0f ){
							mActive = false;
							mShoot = false;
						}
					}

					// 常時拡縮
					mAlwaysScalingRate = mAlwaysScalingCount % 20 / 20.0f;
					if ( mAlwaysScalingCount >= 20 ) mAlwaysScalingRate = 1.0f - mAlwaysScalingRate;
					if ( mAlwaysScalingCount >= 40 ) mAlwaysScalingCount = 0;
					++mAlwaysScalingCount;

					++mCount;
				}
			}

			public bool IsIntersect( Vector3 position, float collisionRange, bool toAvoid ){
				if ( !mActive ) return false;
				return Collision.TestIntersectShere( mStates.Position, mCollisionRange, position, collisionRange );
			}

			public void Draw( GameTime time, Camera camera ){
				if ( mActive && mCount >= 0 ){
					ModelStates state = new ModelStates( null );
					state = mStates.CopyPositionAngleScale();
					state.Scale += Vector3.One * ( mAlwaysScalingRate * 40.0f );
					TransparentModelManager.Add( mTexEnergy.Get[ mCount % 15 / 5 ], state, 1.0f );
				}
			}

			public bool Shoot( Vector3 direction ){
				if ( !mActive || mShoot ) return false;

				mShoot = true;
				if ( mBig ){
					mMove = Vector3.Normalize( direction ) * mBigMoveSpeed;
					mStreat = ( mRandom.NextDouble() < 0.5 );
				}else{
					mMove = Vector3.Normalize( direction ) * mMoveSpeed;
				}

				mRotationVec = Vector3.Normalize( Vector3.Cross( mMove, Vector3.Up ) );

				return true;
			}

			public bool Active{
				get { return mActive; }
			}

			public Vector3 Position{
				get { return mStates.Position; }
			}

			public bool JustInactive{
				get { return ( mLastActive && !mActive ); }
			}
		}

		public EBigJellyfish( ContentManager content, HLModel model, HLModel modelCore, HLModel modelBullet, Vector3 position, Vector3 angle ){
			mName = Names.BigJellyfish;

			mTexEnergy = new MultiTexture( content, "Image/spark", 3 );
			mTexture = content.Load<Texture2D>( "Image/kurage_boss" );

			mModel = model;
			mModelCore = modelCore;

			mPosition = position;

			mStates = new ModelStates( mModel.SkinData );
			mStates.Position = mPosition;
			mStates.Angle = angle;
			mStates.Scale *= 7.0f;
			mStates.SetAnimation( mModel, "Rotation", false, 0.0f );
			mStates.AdvanceAnimation( 2000 );
			mStates.RotationZYX = true;
			
			mCoreStates = new ModelStates( null );
			mCoreStates.Position = mPosition;
			mCoreStates.Angle = angle;
			mCoreStates.Scale *= 7.0f;
			mCoreStates.Position = mStates.Position;
			mCoreStates.RotationZYX = true;

			// 弾を生成
			mBullets = new Bullet[ 3 ];
			for ( int i = 0; i < mBullets.Length; i++ ){
				mBullets[ i ] = new Bullet( modelBullet, mTexEnergy );
			}
			mBullets[ 0 ].Set( false );
			mBullets[ 1 ].Set( false );
			mSetCount = 2;
			
			// 回転変換行列
			Matrix conv = Matrix.CreateRotationZ( angle.Z ) * Matrix.CreateRotationX( angle.X ) * Matrix.CreateRotationY( angle.Y );

			mAllTimeMove = Vector3.Transform( mAllTimeMove, conv );

			// 補間時の制御点を登録
			mMoveCurve = new Curve();
			mMoveCurve.Add( 0.0f, Vector3.Zero, angle );
			mMoveCurve.Add( 1.0f, Vector3.Zero, angle + new Vector3( ( float )-Math.PI / 2, 0.0f, 0.0f ) );
			mMoveCurve.Add( 14.0f, Vector3.Zero, angle + new Vector3( ( float )-Math.PI / 2, 0.0f, 0.0f ) );
			mMoveCurve.Add( 1.0f, Vector3.Transform( new Vector3( 0.0f, 0.0f, -200.0f ), conv ), angle + new Vector3( ( float )Math.PI / 2, 0.0f, 0.0f ) );
			mMoveCurve.Add( 5.0f, Vector3.Transform( new Vector3( 0.0f, 0.0f, 25000.0f ), conv ), null );
			
			// 補間を計算
			mMoveCurve.InterpolateAll();
			
			// 初期座標
			mMoveCurve.Get( mTimeSpeed, ref mStates, false );
			mStates.Position += mPosition;

			// 行動開始判定の範囲
			mRangeToActive = 3000.0f;

			// 境界球の情報を初期化
			mColPosition = new Vector3[ 17 ];
			mColRange = new float[ mColPosition.Length ];
			mColRange[ 0 ] = 200.0f;
			for ( int i = 1; i < mColPosition.Length; i++ ){
				mColRange[ i ] = 30.0f;
			}

			// 初めは待機
			mActive = false;
		}

		/// <summary>
		/// 更新を行う
		/// </summary>
		public override void Update( Camera camera, Player player, Stage stage, bool enableDelete ){
			base.Update( camera, player, stage, false );

			// プレイヤーが近くにいれば行動開始
			if ( !mActive && Collision.TestIntersectShere( player.Position, 1.0f, mStates.Position, mRangeToActive ) ){
				mActive = true;
			}
			
			// プレイヤーが近くにいれば描画
			if ( !mDrawEnable && Collision.TestIntersectShere( player.Position, 1.0f, mStates.Position, mDrawRange ) ){
				mDrawEnable = true;
			}

			float speed = ( player.OnStop() ? 0.0f : ( player.OnSlow() ? player.ZoneScrollSpeedRatio : 1.0f ) );

			if ( mActive ){
				mMoveCurve.Get( mTimeSpeed * speed, ref mStates, false );

				// 弾を更新
				for ( int i = 0; i < mBullets.Length; i++ ){
					Vector3 bulletPosition = Vector3.Zero;
					Vector3 tempVec = Vector3.Zero;
					switch ( i ){
						case 0 :
							bulletPosition = mStates.AnimPlayer.GetBonePosition( 4 );
							tempVec = Vector3.Normalize( bulletPosition - mStates.AnimPlayer.GetBonePosition( 15 ) );
							bulletPosition += tempVec * mBulletDistance;
							break;
						case 1 :
							bulletPosition = mStates.AnimPlayer.GetBonePosition( 15 );
							tempVec = Vector3.Normalize( bulletPosition - mStates.AnimPlayer.GetBonePosition( 4 ) );
							bulletPosition += tempVec * mBulletDistance;
							break;
						case 2 :
							bulletPosition = mStates.AnimPlayer.GetBonePosition( 12 );
							tempVec = ( mStates.AnimPlayer.GetBonePosition( 37 ) - bulletPosition ) / 2.0f;
							bulletPosition += tempVec;
							break;
					}
					Vector3 target;
					if ( i != 2 ){
						target = player.Position + Vector3.Normalize( mAllTimeMove ) * 80.0f;
					}else{
						target = mStates.Position - mAllTimeMove * 10.0f;
					}
					mBullets[ i ].Update( camera, bulletPosition, target, speed );

					if ( mBullets[ i ].JustInactive ){
						// ギリ避け復活
						mAvoided = false;
					}
				}

				if ( mMoveCurve.GetNowIndex() == 1 ){
					// トップ加算なし
					mAddTopLevelEnable = false;
					
					// 改名
					mName = Names.Energy;

					// 弾を装填
					if ( mSetCount < mShootPattern.Length ){
						if ( mBullets[ 0 ].Set( false ) ){
							++mSetCount;
						}
						if ( mBullets[ 1 ].Set( false ) ){
							++mSetCount;
						}

						if ( mMoveCurve.Time > 2.0f ){
							if ( mBullets[ 2 ].Set( true ) ){
								++mSetCount;
							}
						}
					}

					// ショット
					if ( mCount % 90 == 0 ){
						if ( mShootCount < mShootPattern.Length ){
							int index = mShootPattern[ mShootCount ];
							Vector3 shootDir = Vector3.Zero;
							if ( index == 2 ){
								shootDir = mStates.AnimPlayer.GetBonePosition( 0 ) - mStates.AnimPlayer.GetBonePosition( 1 );
							}else{
								shootDir = player.Position - mBullets[ index ].Position;
							}
							
							if ( mBullets[ index ].Shoot( shootDir ) ){
								++mShootCount;
							}
						}
					}
				}
				

				if ( mMoveCurve.GetNowIndex() == 3 ){
					// 改名
					mName = Names.BigJellyfish;
				}

				if ( mMoveCurve.Time >= mMoveCurve.TotalTime ){
					mActive = false;
					mDisable = true;
				}
				
				mStates.Position += mPosition;
				mPosition += mAllTimeMove * speed;
				
				// 境界球の位置を登録
				// でかいの
				Vector3 t = mStates.AnimPlayer.GetBonePosition( 2 ) - mStates.AnimPlayer.GetBonePosition( 14 );
				mColPosition[ 0 ] = mStates.AnimPlayer.GetBonePosition( 14 ) + t * 0.5f;
				// 触手
				mColPosition[ 1 ] = mStates.AnimPlayer.GetBonePosition( 15 );
				mColPosition[ 2 ] = mStates.AnimPlayer.GetBonePosition( 16 );
				mColPosition[ 3 ] = mStates.AnimPlayer.GetBonePosition( 4 );
				mColPosition[ 4 ] = mStates.AnimPlayer.GetBonePosition( 5 );
				for ( int i = 0; i < 6; i++ ){
					mColPosition[ i + 5 ] = mColPosition[ 1 ] + ( mColPosition[ 2 ] - mColPosition[ 1 ] ) * ( ( i + 1 ) / 6.0f );
				}
				for ( int i = 0; i < 6; i++ ){
					mColPosition[ i + 11 ] = mColPosition[ 3 ] + ( mColPosition[ 4 ] - mColPosition[ 3 ] ) * ( ( i + 1 ) / 6.0f );
				}
			}

			mAngle += mRotaitionSpeed * speed;
			mStates.AngleY = mAngle;

			// 核の位置を登録
			mCoreStates.Position = mStates.Position;
			mCoreStates.Angle = mStates.Angle;
			
			// アニメーションスピードを変更
			mAnimationSpeed = speed;

			++mCount;
		}

		public override bool IsIntersect( Vector3 position, float collisionRange, bool toAvoid ){
			if ( !mActive || mDisable ) return false;

			// 全ての弾と衝突判定
			for ( int i = 0; i < mBullets.Length; i++ ){
				if ( mBullets[ i ].IsIntersect( position, collisionRange, toAvoid ) ){
					return true;
				}
			}

			// 全ての境界球と衝突判定
			// でかいのはギリ避けしない
			if ( !toAvoid && Collision.TestIntersectShere( mColPosition[ 0 ], mColRange[ 0 ], position, collisionRange ) ){
				return true;
			}
			// 触手
			for ( int i = 1; i < mColPosition.Length; i++ ){
				if ( Collision.TestIntersectShere( mColPosition[ i ], mColRange[ i ], position, collisionRange ) ){
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// 描画を行う
		/// </summary>
		/// <param name="time">時間</param>
		/// <param name="camera">カメラ</param>
		public override void Draw( GameTime time, Camera camera, EffectManager.Type type ){
			if ( ( mActive || mDrawEnable ) && !mDisable ){
				if ( type != EffectManager.Type.DepthMap ){
					mModelCore.Render( camera, mCoreStates, time, 0, null, Vector4.One * mBright, true, EffectManager.Type.Wrapped );
					TransparentModelManager.Add( mModel, mStates, mTexture, new Vector4( mBright, mBright, mBright, 0.7f ), 0.00f, type );
					for ( int i = 0; i < mBullets.Length; i++ ){
						if ( mBullets[ i ].Active ){
							mBullets[ i ].Draw( time, camera );
						}
					}

					//// 境界球の位置を表示
					//for ( int i = 0; i < mColPosition.Length; i++ ){
					//    Circle.Draw( camera, mColPosition[ i ], new Vector2( mColRange[ i ] * 2.0f, mColRange[ i ] * 2.0f ) );
					//}
				}else{
					mModel.Render( camera, mStates, time, mAnimationSpeed, EffectManager.Type.DepthMap );
				}
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SonicLate{
	/// <summary>
	/// 敵 サメ
	/// </summary>
	class EShark : Enemy{

		enum Mode{
			Streat,
			Trun,
			MoveCamera,
			Follow,
			BeforeAttack,
			Attack,
			Shoot,
			Wait,
			Out,
			End
		}

		private Mode mMode = Mode.Streat;

		/// <summary>移動ベクトル</summary>
		private Vector3 mMove = Vector3.Zero;

		private OBB mOBBTop = null;
		private OBB mOBBBottom = null;

		private bool mEnableCollision = true;

		private int mCount = 0;

		private Vector3 mAttackTarget = Vector3.Zero;
		private Interpolate mInter = new Interpolate();
		private Interpolate3 mInter3 = new Interpolate3();
		private float mMoveStreatSpeed = 80.0f; // 直進するときの速さ
		private float mFollowSpeed = 20.0f; // プレイヤーを追うときの速さ
		private int mAttackCount = 0;
		private bool mSoundEffectPlayed = false;
		private readonly int mMaxNumAttack = 7;

		private bool mUseBullet = false;
		private Bullet[] mBullets = null;
		private int mShootCount = 0;

		private int[][] mAttackData = {
				new int[]{ 0, 1, 1, 0, 0, 1, 0 },
				new int[]{ 1, 1, 0, 1, 0, 0, 1 },
				new int[]{ 1, 0, 1, 0, 1, 1, 0 }
		};

		private int[] mCurrentAttackData = null;
		
		/// <summary>
		/// 弾クラス
		/// </summary>
		class Bullet{
			private HLModel mModel = null;
			private ModelStates mStates = null;
			private bool mActive = false;
			private int mCount = 0;
			private Vector3 mMove = Vector3.Zero;
			private Particle mParticle = null;
			private readonly float mMoveSpeed = 18.0f;
			private readonly float mAttackSpeed = 90.0f;
			private readonly float mCollisionRange = 40.0f;
			public Bullet( HLModel model, BillBoard board ){
				mModel = model;
				mStates = new ModelStates( mModel.SkinData );
				mStates.SetAnimation( mModel, "swim", true, 0.0f );
				mStates.Scale *= 2.0f;

				// パーティクルを生成
				mParticle = new Particle( board, 8, Particle.Type.Snow );
				mParticle.SetMaxPosition( 6.0f, 10.0f );
				mParticle.SetScaleXYRange( 120.0f, 80.0f );
				mParticle.SetSpeedRange( 10.0f, 7.0f );
				mParticle.SetTimeRange( 30, 20 );
				mParticle.Initialize();
				mParticle.Enable = true;
				mParticle.ZTestEnable = false;
			}
			public void Set( Vector3 position, Vector3 direction ){
				mActive = true;
				mStates.Position = position;
				mMove = direction * mMoveSpeed;
				mCount = 0;
			}
			public void Update( Camera camera, Vector3 parentMove, Vector3 targetPosition, float speedRate ){
				// カメラに映っていなければ無効化
				float dot = Vector3.Dot( mStates.Position - camera.Position, Vector3.Normalize( camera.Target - camera.Position ) );
				if ( dot < 0.0f ){
					mActive = false;
				}

				if ( mActive ){
					// 外に向かって移動
					if ( mCount < 30 ){
						Vector3 move = parentMove * 2.0f + mMove;
						mStates.Position += move;
						mStates.SetAngleFromDirection( move );
					}
					// 待機
					else if ( mCount < 80 ){
						mStates.Position += parentMove;
						mStates.SetAngleFromDirection( targetPosition - mStates.Position, 0.2f );
					}
					// プレイヤーに向かって移動
					else{
						if ( mCount < 100 ){
							mMove = Vector3.Normalize( targetPosition - mStates.Position ) * mAttackSpeed;
							//mStates.SetAngleFromDirection( mMove );
						}

						mStates.Position += mMove * speedRate;
						mStates.SetAngleFromDirection( mMove );
					}

					mParticle.Position = mStates.Position;
					mParticle.Update();

					++mCount;
				}
			}
			public bool IsIntersect( Vector3 position, float collisionRange ){
				if ( !mActive ) return false;
				return Collision.TestIntersectShere( mStates.Position, mCollisionRange, position, collisionRange );
			}
			public void Draw( GameTime time, Camera camera, float bright, float animationSpeed, EffectManager.Type type ){
				if ( mActive ){
					mModel.Render( camera, mStates, time, animationSpeed, null, Vector4.One * bright, true, type );
				}
			}
			public void DrawParticle( GraphicsDevice device, Camera camera ){
				if ( mActive ){
					mParticle.Render( device, camera );
				}
			}

			public bool Active{
				get { return mActive; }
			}
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="model">モデルへの参照</param>
		/// <param name="bulletModel">弾丸モデル</param>
		/// <param name="board">ビルボード</param>
		/// <param name="position">原点となる座標</param>
		/// <param name="angle">角度</param>
		public EShark( HLModel model, HLModel bulletModel, BillBoard board, Vector3 position, Vector3 angle ){
			mName = Names.Shark;
			
			// モデルを登録
			mModel = model;

			// モデル情報を初期化
			mStates = new ModelStates( mModel.SkinData );
			mStates.SetAnimation( mModel, "OpenSwim", true, 0.0f );
			mStates.Scale *= 1.25f;

			// 原点を登録
			mPosition = position;

			// 初期座標
			mStates.Position = mPosition;

			// 境界球の情報を初期化
			mColPosition = new Vector3[ 10 ];
			mColRange = new float[ 10 ];

			// 行動開始判定の範囲
			mRangeToActive = 20000.0f;

			// 初めは待機
			mActive = false;

			// アニメーションを進めておく
			mStates.AdvanceAnimation( 0 );

			// 弾を使う
			mUseBullet = ( bulletModel != null );

			if ( mUseBullet ){
				// 弾を初期化
				mBullets = new Bullet[ 8 ];
				for ( int i = 0; i < 8; i++ ){
					mBullets[ i ] = new Bullet( bulletModel, board );
				}

				int num = 0;
				double rand = mRandom.NextDouble();
				if ( rand < 0.4 ) num = 0;
				else if ( rand < 0.8 ) num = 1;
				else num = 2;
				mCurrentAttackData = mAttackData[ num ];
			}

			mOBBTop = new OBB( new Vector3( 120.0f, 60.0f, 320.0f ) );
			mOBBTop.Position = mPosition;
			mOBBBottom = new OBB( new Vector3( 80.0f, 20.0f, 160.0f ) );
			mOBBBottom.SetRotation( new Vector3(  0.5f, 0.0f,0.0f ) );
			mOBBBottom.Position = mPosition;
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

			if ( mActive ){
				// アニメーションスピードを変更
				mAnimationSpeed = player.OnSlow() ? mSlowSpeedRate : 1.2f;
				if ( player.OnStop() || player.IsDied ) mAnimationSpeed = 0.0f;

				mEnableCollision = true;

				switch ( mMode ){
					// 直進
					case Mode.Streat :
						// プレイヤーの近くなら方向転換へ以降
						if ( Collision.TestIntersectShere( player.Position, 1.0f, mStates.Position, 1000.0f ) ){
							mMode = Mode.Trun;
							mCount = -1;
							//break;
						}else if ( mCount < 120 ){
							// プレイヤーに向かって移動
							mMove = Vector3.Normalize( player.Position - mStates.Position ) * mMoveStreatSpeed;
							mStates.SetAngleFromDirection( mMove );
						}
						mStates.Position += mMove * ( player.OnStop() ? 0.0f : ( player.OnSlow() ? mSlowSpeedRate : 1.0f ) );;
						break;

					// 振り返る
					case Mode.Trun :
						if ( mCount == 60 ){
							mMove = Vector3.Zero;
							mStates.SetAnimation( mModel, "CloseSwim", true, 0.0f );
						}
						if ( mCount > 180 ){
							Vector3 target = player.Position + Vector3.Normalize( camera.Position - camera.Target ) * 1000.0f;
							target.Y = player.Position.Y - 50.0f;

							// プレイヤーの近くならカメラ回転へ以降
							if ( Collision.TestIntersectShere( target, 1.0f, mStates.Position, 1000.0f ) ){
								mMode = Mode.MoveCamera;
								mCount = -1;
								break;
							}

							// プレイヤーに向かって移動
							mMove = Vector3.Normalize( target - mStates.Position ) * 80.0f;
							
						}
						mStates.Position += mMove;
						mStates.SetAngleFromDirection( mMove );

						break;

					// カメラ回転
					case Mode.MoveCamera :
						// カメラを反転
						stage.CameraReverseEnable = true;
						
						if ( mCount > 30 ){
							mMode = Mode.Follow;
							mCount = -1;
							break;
						}
						break;

					// プレイヤーを追う
					case Mode.Follow :
						if ( mCount == 0 ){
							mStates.SetAnimation( mModel, "CloseSwim", true, 0.0f );
						}

						// 衝突を無効化
						mEnableCollision = false;

						// ギリ避け復活
						mAvoided = false;
							
						// プレイヤーに向かって移動
						mMove = Vector3.Normalize( player.Position - mStates.Position ) * mFollowSpeed;
						mMove *= ( player.OnSlow() ? mSlowSpeedRate : 1.0f );
						mStates.SetAngleFromDirection( mMove, 0.2f );
						mStates.Position += mMove;

						if ( mCount > 120 ){
							mMode = Mode.BeforeAttack;
							mCount = -1;

							if ( mUseBullet ){
								if ( mCurrentAttackData[ mAttackCount ] == 0 ){
									mMode = Mode.BeforeAttack;
								}else{
									mMode = Mode.Shoot;
								}
							}
						}

						break;
					// 攻撃予備
					case Mode.BeforeAttack :
						if ( mCount > 20 ){
							mMode = Mode.Attack;
							mCount = -1;
						}

						break;
					// 攻撃
					case Mode.Attack :
						if ( mCount == 0 ){
							mStates.SetAnimation( mModel, "Bite", false, 0.0f );
							mEnableCollision = true;
							++mAttackCount;
							
							// 改名
							mName = Names.Shark;

							// トップ加算あり
							mAddTopLevelEnable = true;
						}

						if ( mCount < 10 ){
						    mAttackTarget = player.Position;
							mStates.SetAngleFromDirection( mMove );
						}

						if ( mStates.AnimPlayer.CurrentTime.TotalMilliseconds > 520.0 && !mSoundEffectPlayed ){
						    SoundManager.Play( SoundManager.SE.Shark );
						    mSoundEffectPlayed = true;
						}

						if ( mStates.AnimPlayer.CurrentTime >= mStates.AnimPlayer.CurrentClip.Duration ){
							mStates.SetAnimation( mModel, "CloseSwim", true, 0.0f );
						}

						// プレイヤーがいた位置に向かって移動
						mMove = ( mAttackTarget - mStates.Position ) * 0.045f;
						mMove *= ( player.OnSlow() ? 0.4f : 1.0f );
						if ( player.OnStop() ) mMove *= 0.0f;
						mStates.Position += mMove;

						if ( mCount > 60 ){
							mMode = Mode.Wait;
							mCount = -1;
							mSoundEffectPlayed = false;
							break;
						}

						break;
					// ショット
					case Mode.Shoot :
						if ( mCount == 0 ){
							++mAttackCount;

							//// ギリ避けなし
							//mAvoided = true;

							// トップ加算なし
							mAddTopLevelEnable = false;

							// 改名
							mName = Names.BoneFish;
						}

						// 衝突を無効化
						//mEnableCollision = false;

						// プレイヤーに向かって移動
						mMove = Vector3.Normalize( player.Position - mStates.Position ) * mFollowSpeed;
						mMove *= ( player.OnSlow() ? 0.5f : 1.0f );
						mStates.SetAngleFromDirection( mMove );
						mStates.Position += mMove;

						// 弾を追加
						if ( mCount % 20 == 0 && mShootCount < mBullets.Length ){
							for ( int i = 0; i < 8; i++ ){
								if ( !mBullets[ i ].Active ){
									Vector3 dir = Vector3.Normalize( new Vector3( ( float )mRandom.NextDouble() * 2.0f - 1.0f, ( float )mRandom.NextDouble() * 2.0f - 1.0f, 0.0f ) );
									mBullets[ i ].Set( mStates.Position, dir );
									++mShootCount;
									break;
								}
							}
						}
						// 弾を更新
						bool end = true;
						for ( int i = 0; i < mBullets.Length; i++ ){
							mBullets[ i ].Update( camera, mMove, player.Position, ( player.OnSlow() ? mSlowSpeedRate : 1.0f ) );
							if ( mBullets[ i ].Active ){
								end = false;
							}
						}

						// 弾の処理が終わっていたら次へ
						if ( mShootCount >= mBullets.Length && end ){
							if ( mAttackCount >= mMaxNumAttack ){
								mMode = Mode.Out;
							}else{
								mMode = Mode.Follow;
							}
							mShootCount = 0;
							mCount = -1;
						}
						break;
					// 待機
					case Mode.Wait :
						if ( mStates.AnimPlayer.CurrentTime >= mStates.AnimPlayer.CurrentClip.Duration ){
							mStates.SetAnimation( mModel, "CloseSwim", true, 0.0f );
						}
						
						// プレイヤーからある程度の距離離れるまで待機
						if ( !Collision.TestIntersectShere( player.Position, 1.0f, mStates.Position, 3000.0f ) ){
							if ( mAttackCount >= mMaxNumAttack ){
								mMode = Mode.Out;
							}else{
								mMode = Mode.Follow;
							}
							mCount = -1;
						}

						break;

					// いなくなる
					case Mode.Out :
						mMove = ( player.Position - mStates.Position ) * ( 1.0f / 120.0f );
						mStates.SetAngleFromDirection( mMove );

						if ( mCount > 120 ){
							mMode = Mode.End;
							mCount = -1;
						}
						break;

					// 終了
					case Mode.End :
						stage.CameraReverseEnable = false;

						if ( mCount > 120 ){
							mActive = false;
							mDisable = true;
						}
						break;
				}
				
				mOBBTop.Position = mStates.AnimPlayer.GetBonePosition( 16 );
				mOBBBottom.Position = mStates.AnimPlayer.GetBonePosition( 18 );

				++mCount;
			}


		}

		public override bool IsIntersect( Vector3 position, float collisionRange, bool toAvoid ){
			if ( mActive && !mDisable ){
				if ( mEnableCollision && ( mOBBTop.IsIntersect( position, collisionRange ) || mOBBBottom.IsIntersect( position, collisionRange ) ) ){
					return true;
				}
				if ( mUseBullet ){
					for ( int i = 0; i < mBullets.Length; i++ ){
						if ( mBullets[ i ].IsIntersect( position, collisionRange ) ){
							return true;
						}
					}
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
			if ( mActive && !mDisable ){
				mModel.Render( camera, mStates, time, mAnimationSpeed, null, Vector4.One * mBright, true, type );
					
				if ( mUseBullet ){
					for ( int i = 0; i < mBullets.Length; i++ ){
						mBullets[ i ].Draw( time, camera, mBright, mAnimationSpeed, type );
					}
				}
				//mOBBTop.Render( camera );
				//mOBBBottom.Render( camera );
			}
		}

		public override void DrawParticle( GraphicsDevice device, Camera camera ){
			if ( mActive && !mDisable && mUseBullet ){
				for ( int i = 0; i < mBullets.Length; i++ ){
					mBullets[ i ].DrawParticle( device, camera );
				}
			}
		}
	}
}

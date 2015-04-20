using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SonicLate{
	/// <summary>
	/// 敵 ヒトデ
	/// </summary>
	class EStarfish : Enemy{

		/// <summary>
		/// カウンタ
		/// </summary>
		private int mCount = 0;

		/// <summary>
		/// パーティクル
		/// </summary>
		private Particle[] mParticle = null;
		
		/// <summary>
		/// 移動・向きの補間クラス配列
		/// </summary>
		private Curve mMove = null;

		/// <summary>
		/// 判定用OBB
		/// </summary>
		private OBB mOBB = null;

		private bool mEnableMove = false;
		private readonly float mRangeToMove = 5000.0f;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="model">モデルへの参照</param>
		/// <param name="board">ビルボードへの参照</param>
		/// <param name="position">原点となる座標</param>
		/// <param name="angle">角度</param>
		public EStarfish( HLModel model, BillBoard board, Vector3 position, Vector3 angle ){
			mName = Names.Starfish;
			
			// モデルを登録
			mModel = model;

			// モデル情報を初期化
			mStates = new ModelStates( mModel.SkinData );
			mStates.SetAnimation( mModel, "Take 001", true, 0.0f );
			mStates.Scale *= 4.0f;

			// パーティクルを生成
			mParticle = new Particle[ 2 ];
			for ( int i = 0; i < mParticle.Length; i++ ){
				mParticle[ i ] = new Particle( board, 2, Particle.Type.Snow );
				mParticle[ i ].SetMaxPosition( 6.0f, 10.0f );
				mParticle[ i ].SetScaleXYRange( 60.0f, 40.0f );
				mParticle[ i ].SetSpeedRange( 10.0f, 7.0f );
				mParticle[ i ].SetTimeRange( 30, 20 );
				mParticle[ i ].Initialize();
				mParticle[ i ].Enable = true;
				mParticle[ i ].ZTestEnable = false;
			}

			// 原点を登録
			mPosition = position;

			Matrix conv = Matrix.CreateRotationZ( angle.Z ) * Matrix.CreateRotationX( angle.X ) * Matrix.CreateRotationY( angle.Y );

			// 補間時の制御点を登録
			float scale = 1.5f;
			float speed = 0.08f;
			int sign = mRandom.Next( 2 ) == 0 ? -1 : 1;
			mMove = new Curve();
			mMove.Add(  0.0f * speed, Vector3.Zero, angle );
			mMove.Add(  3.0f * speed, Vector3.Zero, angle + new Vector3( ( float )Math.PI * 0.1f, ( float )Math.PI * 0.5f * sign, 0.0f ) );
			mMove.Add(  1.6f * speed, Vector3.Transform( new Vector3( -20.0f,  -15.0f * sign,   10.0f ) * scale, conv ), null );
			mMove.Add(  1.5f * speed, Vector3.Transform( new Vector3( -30.0f,  -25.0f * sign,  -15.0f ) * scale, conv ), null );
			mMove.Add(  1.4f * speed, Vector3.Transform( new Vector3(   0.0f,    0.0f * sign,  -30.0f ) * scale, conv ), null );
			mMove.Add(  1.3f * speed, Vector3.Transform( new Vector3(  40.0f,   40.0f * sign,   10.0f ) * scale, conv ), null );
			mMove.Add(  1.2f * speed, Vector3.Transform( new Vector3(   0.0f,    0.0f * sign,   80.0f ) * scale, conv ), null );
			mMove.Add(  1.1f * speed, Vector3.Transform( new Vector3( -60.0f,  -70.0f * sign,    0.0f ) * scale, conv ), null );
			mMove.Add(  1.0f * speed, Vector3.Transform( new Vector3(   0.0f,    0.0f * sign, -100.0f ) * scale, conv ), null );
			mMove.Add(  1.0f * speed, Vector3.Transform( new Vector3(  80.0f,  140.0f * sign,  140.0f ) * scale, conv ), null );
			mMove.Add( 12.0f * speed, Vector3.Transform( new Vector3(   0.0f,    0.0f * sign, 1200.0f ) * scale, conv ), null );
			mMove.Add( 16.0f * speed, Vector3.Transform( new Vector3(   0.0f, -140.0f * sign,  140.0f ) * scale, conv ), null );

			// 補間を計算
			mMove.InterpolateAll();

			// 初期座標
			mMove.Get( mTimeSpeed, ref mStates, true );
			mStates.Position += mPosition;

			// OBBの情報を初期化
			Vector3 obbScale = mModel.Scale * mStates.Scale * 0.8f;
			obbScale.Z *= 0.1f;
			mOBB = new OBB( obbScale * 0.5f );
			mOBB.Position = mStates.Position;
			
			// 行動開始判定の範囲
			mRangeToActive = 7000.0f;
			
			// 移動開始判定の範囲
			mRangeToMove = 4500.0f;

			// 初めは待機
			mActive = false;
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
				if ( mEnableMove && dot < 0.0f && enableDelete ){
				    mDisable = true;
				}

				if ( !mEnableMove && Collision.TestIntersectShere( player.Position, 1.0f, mStates.Position, mRangeToMove ) ){
					mEnableMove = true;
					Vector3 pos = ( player.Position - mStates.Position );
					mMove.Set( 10, pos * 0.5f, null );
					mMove.Set( 11, pos * 2.0f, null );
					mMove.InterpolateAll();
				}
				if ( mEnableMove ){
					// 移動補間が終了したら無効化
					if ( mMove.Time >= mMove.TotalTime ){
						mDisable = true;
					}

					mMove.Get( mTimeSpeed * speed, ref mStates, false );
					mStates.Position += mPosition;

					mParticle[ 0 ].Position = mStates.Position + new Vector3( -4.0f, 0.0f, mModel.Scale.X * mStates.ScaleX * 0.5f );
					mParticle[ 0 ].Update();

					mParticle[ 1 ].Position = mStates.Position + new Vector3( 4.0f, -10.0f, -mModel.Scale.X * mStates.ScaleX * 0.5f );
					mParticle[ 1 ].Update();

					// OBBの情報を登録
					mOBB.SetRotation( mStates.Angle );
					mOBB.Position = mStates.Position;
				}
			}

			// アニメーションスピードを変更
			mAnimationSpeed = speed;

			++mCount;
		}

		/// <summary>
		/// 描画を行う
		/// </summary>
		/// <param name="time">時間</param>
		/// <param name="camera">カメラ</param>
		public override void Draw( GameTime time, Camera camera, EffectManager.Type type ){
			if ( mActive && !mDisable ){
			    mModel.Render( camera, mStates, time, mAnimationSpeed, null, Vector4.One * mBright, true, type );

				//mOBB.Render( camera );
			}
		}

		/// <summary>
		/// パーティクルを描画する
		/// </summary>
		/// <param name="device">グラフィックスデバイス</param>
		/// <param name="camera">カメラ</param>
		public override void DrawParticle( GraphicsDevice device, Camera camera ){
            if ( mActive && !mDisable ){
			    mParticle[ 0 ].Render( device, camera );
			    mParticle[ 1 ].Render( device, camera );
            }
		}

		/// <summary>
		/// 衝突を調べる
		/// </summary>
		/// <param name="position">任意の球の中心座標</param>
		/// <param name="collisionRange">任意の球の半径</param>
		public override bool IsIntersect( Vector3 position, float collisionRange, bool toAvoid ){
		    if ( !mActive || mDisable ) return false;
		    return mOBB.IsIntersect( position, collisionRange );
		}
	}
}

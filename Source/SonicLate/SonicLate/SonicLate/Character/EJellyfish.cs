using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SonicLate{
	/// <summary>
	/// 敵 クラゲ
	/// </summary>
	class EJellyfish : Enemy{
		
		/// <summary>
		/// 移動・向きの補間クラス配列
		/// </summary>
		private Curve mMoveCurve = null;

		/// <summary>
		/// 別モデル
		/// </summary>
		private HLModel mModelCore = null;

		private ModelStates mCoreStates = null;

		private Vector3 mUp;
		private int mCount = 0;

		private bool mSpecial = false;
		private bool mSpecialEnd = false;
		private float mSpecialMoveSpeed = 8.0f;
		private float mSpecialRange = 500.0f;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="model">モデルへの参照</param>
		/// <param name="position">原点となる座標</param>
		/// <param name="angle">角度</param>
		public EJellyfish( HLModel model, HLModel modelCore, Vector3 position, Vector3 angle ){
			mName = Names.Jellyfish;
			
			// モデルを登録
			mModel = model;
			mModelCore = modelCore;

			Matrix conv = Matrix.CreateRotationZ( angle.Z ) * Matrix.CreateRotationX( angle.X ) * Matrix.CreateRotationY( angle.Y );

			// モデル情報を初期化
			mStates = new ModelStates( mModel.SkinData );
			mStates.SetAnimation( mModel, "Rotation", true, 0.0f );

			mUp = Vector3.Transform( Vector3.Up, conv );

			mCoreStates = new ModelStates( null );
			mCoreStates.Position = mPosition;

			// 原点を登録
			mPosition = position;

			// 補間時の制御点を登録
			float scale = 1.5f;
			Vector3 center = new Vector3( 0.0f, 50.0f, 0.0f );
			mMoveCurve = new Curve();
			mMoveCurve.Add( 0.0f, Vector3.Transform( center, conv ), angle );
			mMoveCurve.Add( 1.0f, Vector3.Transform( center + new Vector3( 0.0f, 100.0f, 0.0f ) * scale, conv ), angle );
			mMoveCurve.Add( 1.0f, Vector3.Transform( center, conv ), angle );
			mMoveCurve.Add( 1.0f, Vector3.Transform( center + new Vector3( 0.0f, -100.0f, 0.0f ) * scale, conv ), angle );
			mMoveCurve.Add( 1.0f, Vector3.Transform( center, conv ), angle );

			// 補間を計算
			mMoveCurve.InterpolateAll();

			// 初期座標
			mMoveCurve.Get( mTimeSpeed, ref mStates, false );
			mStates.Position += mPosition;
			mStates.Scale *= 1.0f;

			// 境界球の情報を初期化
			mColPosition = new Vector3[ 1 ];
			mColPosition[ 0 ] = position;
			mColRange = new float[ 1 ];
			mColRange[ 0 ] = 40.0f;

			// 行動開始判定の範囲
			mRangeToActive = 5000.0f;

			// 初めは待機
			mActive = false;

			// ランダムで特殊
			if ( mRandom.Next( 5 ) == 0 ){
				mSpecial = true;
			}
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

				if ( mSpecial ){
					if ( mCount % 120 < 80 && !mSpecialEnd ){
						Vector3 move = Vector3.Normalize( player.Position - mStates.Position ) * mSpecialMoveSpeed;
						mStates.Position += move * speed;
					}else{
						if ( Collision.TestIntersectShere( player.Position, 1.0f, mStates.Position, mSpecialRange ) ){
							mSpecialEnd = true;
						}
					}
				}else{
					mMoveCurve.Get( mTimeSpeed * speed, ref mStates, true );

					mStates.Position += mPosition;
				}
			}

			// 核の位置を登録
			mCoreStates.Position = mStates.Position;
			mCoreStates.Angle = mStates.Angle;
			
			// 境界球の位置を登録
			mColPosition[ 0 ] = mStates.Position;

			// アニメーションスピードを変更
			mAnimationSpeed = speed;
			if ( player.OnStop() || player.IsDied ) mAnimationSpeed = 0.0f;

			++mCount;
		}

		/// <summary>
		/// 描画を行う
		/// </summary>
		/// <param name="time">時間</param>
		/// <param name="camera">カメラ</param>
		public override void Draw( GameTime time, Camera camera, EffectManager.Type type ){
			if ( mActive && !mDisable ){
				if ( type != EffectManager.Type.DepthMap ){
					mModelCore.Render( camera, mCoreStates, time, 0, null, Vector4.One * mBright, true, EffectManager.Type.Wrapped );
					TransparentModelManager.Add( mModel, mStates, new Vector4( mBright, mBright, mBright, 0.7f ), mAnimationSpeed, type );
				}else{
					mModel.Render( camera, mStates, time, mAnimationSpeed, EffectManager.Type.DepthMap );
				}
			}
		}
	}
}

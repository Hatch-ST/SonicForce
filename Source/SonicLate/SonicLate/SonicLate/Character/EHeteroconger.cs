using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SonicLate{
	/// <summary>
	/// 敵 チンアナゴ
	/// </summary>
	class EHeteroconger : Enemy{
		
		/// <summary>
		/// 移動・向きの補間クラス配列
		/// </summary>
		private Curve mMove = null;

		private float mDefaultAnimationSpeed = 1.0f;
		
		private const float mBigColRange = 2000.0f;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="model">モデルへの参照</param>
		/// <param name="position">原点となる座標</param>
		/// <param name="angle">角度</param>
		public EHeteroconger( HLModel model, Vector3 position, Vector3 angle ){
			mName = Names.Heteroconger;
			
			// モデルを登録
			mModel = model;

			// モデル情報を初期化
			mStates = new ModelStates( mModel.SkinData );
			mStates.SetAnimation( mModel, "teishi", true, 0.0f );
			mStates.Scale *= 4.0f;

			// 原点を登録
			mPosition = position;

			// 補間時の制御点を登録
			Vector3 direction = new Vector3( 0.0f, 0.0f, 1.0f );
			Matrix conv = Matrix.CreateRotationZ( angle.Z ) * Matrix.CreateRotationX( angle.X ) * Matrix.CreateRotationY( angle.Y );
			direction = Vector3.Normalize( Vector3.Transform( direction, conv ) );

			mMove = new Curve();
			mMove.AddFromDirection( 0.0f, direction * -1000.0f, direction );
			mMove.AddFromDirection( 0.2f, direction * -500.0f, direction );
			mMove.AddFromDirection( 0.2f, Vector3.Zero, direction );
			mMove.AddFromDirection( 5.0f, direction * 100.0f, direction );
			mMove.AddFromDirection( 0.5f, direction * -500.0f, direction );
			mMove.AddFromDirection( 0.5f, direction * -1000.0f, direction );

			// 補間を計算
			mMove.InterpolateAll();

			// 初期座標
			mMove.Get( mTimeSpeed, ref mStates, false );
			mStates.Position += mPosition;
			mStates.AngleY = ( float )Math.PI * 0.5f;
			
			// 境界球の情報を初期化
			mColPosition = new Vector3[ 9 ];
			mColRange = new float[ mColPosition.Length ];
			for ( int i = 0; i < mColPosition.Length; i++ ){
				mColRange[ i ] = 25.0f;
			}

			// 行動開始判定の範囲
			mRangeToActive = 3000.0f;

			// 初めは待機
			mActive = false;
			
			// アニメーションを進めておく
			mStates.AdvanceAnimation( 0 );
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

			if ( mActive ){
				// カメラに映っていなければ無効化
				float dot = Vector3.Dot( mStates.Position - camera.Position, Vector3.Normalize( camera.Target - camera.Position ) );
				if ( dot < 0.0f && enableDelete ){
					mDisable = true;
				}

				// 移動補間が終了したら無効化
				if ( mMove.Time >= mMove.TotalTime ){
					mDisable = true;
				}

				// アニメーションを変更
				if ( mMove.GetNowIndex() == 2 && mStates.PlayingAnimationName != "Take 001" ){
					mStates.SetAnimation( mModel, "Take 001", true, 20.0f );
				}

				float speed = mTimeSpeed * ( player.OnSlow() ? mSlowSpeedRate : 1.0f );
				mMove.Get( speed, ref mStates, false );
				mStates.Position += mPosition;

				// 境界球の位置を登録
				for ( int i = 0; i < mColPosition.Length / 2 + 1; i++ ){
					mColPosition[ i * 2 ] = mStates.AnimPlayer.GetBonePosition( i );
				}
				for ( int i = 1; i < mColPosition.Length - 1; i += 2 ){
					mColPosition[ i ] = mColPosition[ i - 1 ] + ( mColPosition[ i + 1 ] - mColPosition[ i - 1 ] ) * 0.5f;
				}
			}


			// アニメーションスピードを変更
			mAnimationSpeed = player.OnSlow() ? mDefaultAnimationSpeed * mSlowSpeedRate : mDefaultAnimationSpeed;
			if ( player.OnStop() || player.IsDied ) mAnimationSpeed = 0.0f;
		}

		/// <summary>
		/// 衝突を調べる
		/// </summary>
		/// <param name="position">任意の球の中心座標</param>
		/// <param name="collisionRange">任意の球の半径</param>
		public override bool IsIntersect( Vector3 position, float collisionRange, bool toAvoid ){
			if ( !mActive || mDisable ) return false;

			// 近くにいれば細かく判定
			if ( Collision.TestIntersectShere( position, collisionRange, mStates.Position, mBigColRange ) ){
				
				// 全ての境界球と衝突判定
				for ( int i = 0; i < mColPosition.Length; i++ ){
					if ( Collision.TestIntersectShere( mColPosition[ i ], mColRange[ i ], position, collisionRange ) ){
						return true;
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
			}
		}
	}
}

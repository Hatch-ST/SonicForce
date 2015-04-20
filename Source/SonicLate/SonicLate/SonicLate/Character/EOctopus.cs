using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SonicLate{
	/// <summary>
	/// 敵 タコ
	/// </summary>
	class EOctopus : Enemy{
		
		/// <summary>
		/// 移動・向きの補間クラス配列
		/// </summary>
		private Curve mMove = null;
		
		private const float mBigColRange = 2000.0f;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="model">モデルへの参照</param>
		/// <param name="position">原点となる座標</param>
		/// <param name="angle">角度</param>
		public EOctopus( HLModel model, Vector3 position, Vector3 angle, int animationIndex ){
			if ( animationIndex == 0 ){
				mName = Names.OctopusUpDown;
			}else{
				mName = Names.OctopusScrew;
			}
			
			// モデルを登録
			mModel = model;

			// モデル情報を初期化
			mStates = new ModelStates( mModel.SkinData );
			string animName = ( animationIndex == 0 ) ? "Attack" : "Screw";
			mStates.SetAnimation( mModel, animName, true, 0.0f );
			mStates.Scale *= 4.0f;

			// 原点を登録
			mPosition = position;

			// 補間時の制御点を登録
			mMove = new Curve();
			mMove.Add( 0.0f, Vector3.Zero, angle );
			mMove.Add( 0.5f, Vector3.Zero, angle );

			// 補間を計算
			mMove.InterpolateAll();

			// 初期座標
			mMove.Get( mTimeSpeed, ref mStates, false );
			mStates.Position += mPosition;
			
			// 境界球の情報を初期化
			mColPosition = new Vector3[ 11 ];
			mColRange = new float[ 11 ];
			for ( int i = 0; i < mColRange.Length; i++ ){
				mColRange[ i ] = 100.0f - 8.0f * i;
			}

			// 行動開始判定の範囲
			mRangeToActive = 8000.0f;

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

				float speed = mTimeSpeed * ( player.OnSlow() ? mSlowSpeedRate : 1.0f );
				mMove.Get( speed, ref mStates, false );
				mStates.Position += mPosition;
	
				// 境界球の位置を登録
				for ( int i = 0; i < mColPosition.Length; i++ ){
					mColPosition[ i ] = mStates.AnimPlayer.GetBonePosition( i );
				}
			}

			// アニメーションスピードを変更
			mAnimationSpeed = player.OnSlow() ? mSlowSpeedRate : 1.0f;
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
				
				//// 境界球の位置を表示
				//for ( int i = 0; i < mColPosition.Length; i++ ){
				//    Circle.Draw( camera, mColPosition[ i ], new Vector2( mColRange[ i ] * 2.0f, mColRange[ i ] * 2.0f ) );
				//}
			}
		}
	}
}

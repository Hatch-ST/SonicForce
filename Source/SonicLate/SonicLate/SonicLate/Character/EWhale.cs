using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SonicLate{
	/// <summary>
	/// 敵 クジラ
	/// </summary>
	class EWhale : Enemy{

		private float mRangeToAttack = 0.0f;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="model">モデルへの参照</param>
		/// <param name="position">原点となる座標</param>
		public EWhale( HLModel model, Vector3 position, Vector3 angle ){
			mName = Names.Whale;
			
			// モデルを登録
			mModel = model;

			// モデル情報を初期化
			mStates = new ModelStates( mModel.SkinData );
			mStates.SetAnimation( mModel, "Bite", false, 0.0f );
			mStates.Scale *= 18.0f;

			// 原点を登録
			mPosition = position;
			mStates.Position = mPosition;

			// 行動開始判定の範囲
			mRangeToActive = 10000.0f;
			mRangeToAttack = 1600.0f;

			// 初めは待機
			mActive = false;
		}
		
		/// <summary>
		/// 更新を行う
		/// </summary>
		public override void Update( Camera camera, Player player, Stage stage, bool enableDelete ){
			base.Update( camera, player, stage, false );
			
			// アニメーションスピードを変更
			mAnimationSpeed = player.OnSlow() ? mSlowSpeedRate : 1.0f;

			// プレイヤーが近くにいれば行動開始
			if ( !mActive && Collision.TestIntersectShere( player.Position, 1.0f, mStates.Position, mRangeToActive ) ){
				mActive = true;
			}

			if ( mActive ){
				if ( Collision.TestIntersectShere( player.Position, 1.0f, mStates.Position, mRangeToActive * 0.25f ) ){
					mAnimationSpeed = 1.0f;
				}else{
					mAnimationSpeed = 0.0f;
				}

				// 範囲内なら食う
				if ( stage.CameraMoveEnable && Collision.TestIntersectShere( player.Position, 1.0f, mStates.Position, mRangeToAttack ) ){
					SoundManager.Play( SoundManager.SE.Whale );
					stage.CameraMoveEnable = false;
				}
				
				mStates.Position += Vector3.Normalize( player.Position - mStates.Position ) * 2.0f;
			}

		}

		/// <summary>
		/// 描画を行う
		/// </summary>
		/// <param name="time">時間</param>
		/// <param name="camera">カメラ</param>
		public override void Draw( GameTime time, Camera camera, EffectManager.Type type ){
			if ( mActive && !mDisable ){
				if ( type != EffectManager.Type.DepthMap ){
					type = EffectManager.Type.Fog;
				}
				mModel.Render( camera, mStates, time, mAnimationSpeed, null, Vector4.One * mBright, true, type );
			}
		}
	}
}

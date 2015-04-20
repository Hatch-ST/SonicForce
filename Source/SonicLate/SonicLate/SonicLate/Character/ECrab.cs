using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SonicLate{
	/// <summary>
	/// 敵 かに
	/// </summary>
	class ECrab : Enemy{
		/// <summary>
		/// 移動・向きの補間クラス配列
		/// </summary>
		private Curve mMove = null;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="model">モデルへの参照</param>
		/// <param name="position">原点となる座標</param>
		/// <param name="angle">角度</param>
		public ECrab( HLModel model, Vector3 position, Vector3 angle ){
			mName = Names.Crab;

			// モデルを登録
			mModel = model;

			// モデル情報を初期化
			mStates = new ModelStates( mModel.SkinData );
			mStates.SetAnimation( mModel, "attack", true, 0.0f );
			mStates.Scale *= 1.5f;

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
			mColPosition = new Vector3[ 10 ];
			mColRange = new float[ 10 ];
			mColRange[ 0 ] = 0.0f; // 腕
			mColRange[ 1 ] = 50.0f; // ハサミ付け根
			mColRange[ 2 ] = 80.0f; // ハサミ上 付け根上
			mColRange[ 3 ] = 80.0f; // ハサミ間
			mColRange[ 4 ] = 60.0f; // ハサミ下

			mColRange[ 5 ] = 20.0f; // ハサミ上 先端
			mColRange[ 6 ] = 40.0f; // ハサミ上 先端手前
			mColRange[ 7 ] = 60.0f; // ハサミ上 付け根奥
			mColRange[ 8 ] = 20.0f; // ハサミ下 先端
			mColRange[ 9 ] = 40.0f; // ハサミ下 中央

			// 行動開始判定の範囲
			mRangeToActive = 10000.0f;

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

				mStates.Position = mPosition;
			}

			// 境界球の位置を登録
			for ( int i = 0; i < 5; i++ ){
				mColPosition[ i ] = mStates.AnimPlayer.GetBonePosition( i );
			}
			Vector3 vC32 = mColPosition[ 2 ] - mColPosition[ 3 ];
			Vector3 vC34 = mColPosition[ 4 ] - mColPosition[ 3 ];
			Vector3 axis = Vector3.Normalize( Vector3.Cross( vC32, vC34 ) );
			Matrix rota = Matrix.CreateFromAxisAngle( axis, ( float )Math.PI * 0.5f );
			Vector3 vC35 = Vector3.Transform( vC32, rota ) * 2.0f;
			mColPosition[ 5 ] = mColPosition[ 2 ] + vC35;
			mColPosition[ 6 ] = mColPosition[ 2 ] + vC35 * 0.8f;
			mColPosition[ 7 ] = mColPosition[ 2 ] + vC35 * 0.5f;
			mColPosition[ 8 ] = mColPosition[ 4 ] + vC34;
			mColPosition[ 9 ] = mColPosition[ 4 ] + vC34 * 0.6f;
			mColPosition[ 2 ] = mColPosition[ 3 ] + ( vC32 + ( mColPosition[ 7 ] - mColPosition[ 3 ] ) ) * 0.4f;

			// アニメーションスピードを変更
			mAnimationSpeed = player.OnSlow() ? mSlowSpeedRate : 1.0f;
			if ( player.OnStop() || player.IsDied ) mAnimationSpeed = 0.0f;
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
				//    Circle.Draw( camera, mColPosition[ i ] + camera.ReferenceTranslate * 0.5f, new Vector2( mColRange[ i ] * 2.0f, mColRange[ i ] * 2.0f ) );
				//}
			}
		}
	}
}

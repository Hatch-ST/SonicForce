using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SonicLate{
	abstract class Enemy{
		/// <summary>判定範囲の配列</summary>
		protected float[] mColRange = null;

		/// <summary>判定を置く位置の配列</summary>
		protected Vector3[] mColPosition = null;

		/// <summary>モデルインスタンス</summary>
		protected HLModel mModel = null;

		/// <summary>モデルの状態</summary>
		protected ModelStates mStates = null;

		/// <summary>1フレームに進む秒数</summary>
		protected float mTimeSpeed = 1.0f / 60.0f;

		/// <summary>アニメーションの速さ</summary>
		protected float mAnimationSpeed = 1.0f;

		/// <summary>移動の原点となる座標</summary>
		protected Vector3 mPosition = Vector3.Zero;

		/// <summary>行動しているか</summary>
		protected bool mActive = false;

		/// <summary>行動開始判定の範囲</summary>
		protected float mRangeToActive = 0.0f;

		/// <summary>乱数クラス</summary>
		protected static Random mRandom = new Random();

		/// <summary>低速時の速さの倍率</summary>
		protected static float mSlowSpeedRate = 0.2f;
		
		/// <summary>プレイヤーに避けられたかどうか</summary>
		protected bool mAvoided = false;

		/// <summary>無効化されているかどうか</summary>
		protected bool mDisable = false;

		/// <summary>無効化カウンタ</summary>
		protected int mDisableCount = 0;

		/// <summary>カメラより手前にあるときに消されるまでの時間</summary>
		protected static int mDisableTime = 1200;

		/// <summary>明るさ</summary>
		protected float mBright = 1.0f;
		
		/// <summary>ギリ避けコンボを加算できるかどうか</summary>
		protected bool mAddTopLevelEnable = true;
		
		/// <summary>お名前</summary>
		protected Names mName = Names.None;

		/// <summary>
		/// お名前列挙体
		/// </summary>
		public enum Names{
			Starfish,
			Heteroconger,
			Burrfish,
			OctopusUpDown,
			OctopusScrew,
			Jellyfish,
			Crab,
			Shark,
			BoneFish,
			Whale,
			BigJellyfish,
			Energy,
			BigHeteroconger,
			None
		}

		/// <summary>
		/// 更新を行う
		/// </summary>
		public virtual void Update( Camera camera, Player player, Stage stage, bool enableDelete ){
			// カメラに映っていなければ無効化カウンタを増やす
			float dot = Vector3.Dot( mStates.Position - camera.Position, Vector3.Normalize( camera.Target - camera.Position ) );
			if ( dot < 0.0f && enableDelete ){
				++mDisableCount;
			}else{
				mDisableCount = 0;
			}

			if ( mDisableCount > mDisableTime && enableDelete ){
				mDisable = true;
			}

			if ( player.OnSlow() ){
				mBright = 2.0f;
			}else{
				mBright = Math.Max( 1.0f, mBright - ( 1.0f / 60.0f ) );
			}
		}

		/// <summary>
		/// 全ての境界球との衝突を調べる
		/// </summary>
		/// <param name="position">任意の球の中心座標</param>
		/// <param name="collisionRange">任意の球の半径</param>
		public virtual bool IsIntersect( Vector3 position, float collisionRange, bool toAvoid ){
			if ( !mActive || mDisable ) return false;
			if ( mColRange != null ){
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
		/// <param name="type">エフェクトタイプ</param>
		public virtual void Draw( GameTime time, Camera camera, EffectManager.Type type ){
		}
		
		/// <summary>
		/// 深度マップ用の描画を行う
		/// </summary>
		/// <param name="time">時間</param>
		/// <param name="camera">カメラ</param>
		/// <param name="type">エフェクトタイプ</param>
		public virtual void DrawToShadowMap( GameTime time, Camera camera ){
			Draw( time, camera, EffectManager.Type.DepthMap );
		}
		
		/// <summary>
		/// パーティクルを描画する
		/// </summary>
		/// <param name="device">グラフィックスデバイス</param>
		/// <param name="camera">カメラ</param>
		public virtual void DrawParticle( GraphicsDevice device, Camera camera ){
		}

		/// <summary>
		/// プレイヤーにギリ避けされたかどうかを取得または登録する
		/// </summary>
		public bool Avoided{
			get { return mAvoided; }
			set { mAvoided = value; }
		}

		/// <summary>
		/// 無効化されているかどうかを取得または登録する
		/// </summary>
		public bool Disable{
			get { return mDisable; }
			set { mDisable = value; }
		}

		/// <summary>
		/// 名前を取得する
		/// </summary>
		public Names Name{
			get { return mName; }
		}

		/// <summary>
		/// 座標を取得する
		/// </summary>
		public Vector3 Position{
			get { return mStates.Position; }
		}
		
		/// <summary>
		/// ギリ避け時にコンボ数を加算させるかどうかを取得する
		/// </summary>
		public bool AddTopLevelEnable{
			get { return mAddTopLevelEnable; }
		}
	}
}

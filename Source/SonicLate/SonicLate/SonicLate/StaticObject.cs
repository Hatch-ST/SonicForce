using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SonicLate{
	/// <summary>
	/// 停止オブジェクト
	/// </summary>
	class StaticObject{
		private HLModel mModel = null;
		private ModelStates mStates = null;
		private Name mType;
		private bool mOuter = false;
		
		/// <summary>無効化されているかどうか</summary>
		protected bool mDisable = false;

		public enum Name{
			Obj_1,
			Obj_2,
			Obj_3,
			Obj_4,
			Obj_5,
			OldShip,
			Sango_S,
			Sango_M,
			Sango_L,
			Fish_1,
			Fish_2,
			Fish_3,
			Fish_4,
			Fish_5,
			Fish_6,
			WholeS,
			WholeL
		}


		public StaticObject( Name type, HLModel model, Vector3 position, Vector3 angle ){
			mModel = model;

			Vector3 scale = Vector3.One * 2.5f;

			switch ( type ){
				case Name.OldShip :
					scale = Vector3.One * 15.0f;
					break;
				case Name.Sango_S :
					scale = Vector3.One * 8.0f;
					mOuter = true;
					break;
				case Name.Sango_M :
					scale = Vector3.One * 9.5f;
					mOuter = true;
					break;
				case Name.Sango_L :
					scale = Vector3.One * 15.0f;
					mOuter = true;
					break;
				case Name.Fish_1 :
					scale = Vector3.One * 6.0f;
					mOuter = true;
					break;
				case Name.Fish_2 :
					scale = Vector3.One * 6.0f;
					mOuter = true;
					break;
				case Name.Fish_3 :
					scale = Vector3.One * 7.0f;
					mOuter = true;
					break;
				case Name.Fish_4 :
					scale = Vector3.One * 6.0f;
					mOuter = true;
					break;
				case Name.Fish_5 :
					scale = Vector3.One * 5.5f;
					mOuter = true;
					break;
				case Name.Fish_6 :
					scale = Vector3.One * 7.5f;
					mOuter = true;
					break;
				case Name.WholeS :
					scale = Vector3.One;
					scale.Y = 1.5f;
					mOuter = true;
					break;
				case Name.WholeL :
					scale = Vector3.One * 2.0f;
					scale.Y = 3.0f;
					mOuter = true;
					break;
			}

			mType = type;
			mStates = new ModelStates( position, angle, scale, null );
		}

		public void Update( Camera camera, bool enableDelete ){
			if ( !mDisable ){
				// カメラに映っていなければ無効化
				Vector3 view = ( camera.Position - camera.Target );
				float dot = Vector3.Dot( mStates.Position - ( camera.Position + view * 2.0f ), Vector3.Normalize( camera.Target - camera.Position ) );
				if ( dot < 0.0f && enableDelete ){
					mDisable = true;
				}
			}
		}

		public void Draw( Camera camera, GameTime time, Player player, EffectManager.Type type ){
			// 近くなら描画
			if ( !mDisable && Collision.TestIntersectShere( player.Position, 1.0f, mStates.Position, 12000.0f ) ){
				// 深度マップに外側は書かない
				if ( !( type == EffectManager.Type.DepthMap && mOuter ) ){
					Vector4 ambient;
					if ( mOuter ){
						ambient = new Vector4( 0.5f, 0.5f, 1.0f, 1.0f );
					}else{
						ambient = Vector4.One * 0.5f;
					}
					mModel.RenderLinearWrap( camera, mStates, time, 1.0f, ambient, null, true, type );
				}
			}
		}

		public Name Type{
			get { return mType; }
		}

		public ModelStates States{
			get { return mStates; }
		}

		/// <summary>
		/// 無効化されているかどうかを取得または登録する
		/// </summary>
		public bool Disable{
			get { return mDisable; }
			set { mDisable = value; }
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SonicLate{
	/// <summary>
	/// フォグエフェクト
	/// </summary>
	class Fog : EffectParent{

		private EffectParameter mFarCoordParam = null;
		private EffectParameter mFogColorParam = null;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="device">グラフィックスデバイス</param>
		/// <param name="content">コンテントマネージャ</param>
		public Fog( GraphicsDevice device, ContentManager content ){
			// エフェクトを読み込む
			mEffect = content.Load<Effect>( "Effect/Fog" );

			// テクニックを登録
			mEffect.CurrentTechnique = mEffect.Techniques[ "FogTec" ];

			// パラメータを取得
			mMatWorldParam = mEffect.Parameters[ "matWorld" ];
			mMatVPParam = mEffect.Parameters[ "matVP" ];
			mMatBoneParam = mEffect.Parameters[ "bones" ];
			mAmbientParam = mEffect.Parameters[ "ambient" ];
			mLightDirParam = mEffect.Parameters[ "lightDir" ];
			mDiffuseParam = mEffect.Parameters[ "diffuse" ];
			mTextureParam = mEffect.Parameters[ "texMesh" ];

			mFogColorParam = mEffect.Parameters[ "fogColor" ];
			mFarCoordParam = mEffect.Parameters[ "fogCoord" ];
			mEyePosParam = mEffect.Parameters[ "cameraPosition" ];

			SetNearAndFarDepth( 2000.0f, 9000.0f );
			mFogColorParam.SetValue( GameMain.BackGroundColor.ToVector4() );
		}

		/// <summary>
		/// 通常のテクニックに設定する
		/// </summary>
		public override void SetToDefaultTechniques(){
			mEffect.CurrentTechnique = mEffect.Techniques[ "FogTec" ];
		}

		/// <summary>
		/// スキニング用のテクニックに設定する
		/// </summary>
		public override void SetToSkinningTechniques(){
			mEffect.CurrentTechnique = mEffect.Techniques[ "SkinnedFogTec" ];
		}

		public void SetNearAndFarDepth( float near, float far ){
			Vector2 coord = new Vector2();
			coord.X = near / ( near - far );
			coord.Y = -1.0f / ( near - far );
			mFarCoordParam.SetValue( coord );
		}

		public Vector4 FogColor{
			get { return mFogColorParam.GetValueVector4(); }
			set { Vector4 t = value;
				t.X = Math.Min( 1.0f, value.X );
				t.Y = Math.Min( 1.0f, value.Y );
				t.Z = Math.Min( 1.0f, value.Z );
				t.W = Math.Min( 1.0f, value.W );
				mFogColorParam.SetValue( t );
			}
		}
	}
}

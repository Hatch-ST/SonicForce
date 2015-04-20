using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SonicLate{
	/// <summary>
	/// 深度マップを使用した影のエフェクト
	/// </summary>
	class DepthShadow : EffectParent{

		private EffectParameter mDepthTextureParam = null;
		private EffectParameter mMatLightViewParam = null;
		private EffectParameter mMatLightProjParam = null;
		private EffectParameter mFarCoordParam = null;
		private EffectParameter mFogColorParam = null;

		private bool mFogEnagle = true;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="device">グラフィックスデバイス</param>
		/// <param name="content">コンテントマネージャ</param>
		public DepthShadow( GraphicsDevice device, ContentManager content ){
			// エフェクトを読み込む
			mEffect = content.Load<Effect>( "Effect/DepthShadow" );

			// テクニックを登録
			mEffect.CurrentTechnique = mEffect.Techniques[ "DepthShadowTec" ];

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
			mMatLightViewParam = mEffect.Parameters[ "matLightView" ];
			mMatLightProjParam = mEffect.Parameters[ "matLightProj" ];
			mDepthTextureParam = mEffect.Parameters[ "texShadowMap" ];
			
			SetNearAndFarDepth( 2000.0f, 7000.0f );
			mFogColorParam.SetValue( GameMain.BackGroundColor.ToVector4() );
		}

		/// <summary>
		/// 通常のテクニックに設定する
		/// </summary>
		public override void SetToDefaultTechniques(){
			if ( mFogEnagle ){
				mEffect.CurrentTechnique = mEffect.Techniques[ "DepthShadowFogTec" ];
			}else{
				mEffect.CurrentTechnique = mEffect.Techniques[ "DepthShadowTec" ];
			}
		}

		/// <summary>
		/// スキニング用のテクニックに設定する
		/// </summary>
		public override void SetToSkinningTechniques(){
			mEffect.CurrentTechnique = mEffect.Techniques[ "DepthShadowSkinedTec" ];
		}

		/// <summary>
		/// 深度マップを登録する
		/// </summary>
		/// <param name="depthMap">深度マップ</param>
		public void SetDepthMap( DepthMap depthMap ){
			// ライトへの行列を取得
			Matrix lView, lProj;
			depthMap.GetLightState( out lView, out lProj );

			// 行列をシェーダに登録
			mMatLightViewParam.SetValue( lView );
			mMatLightProjParam.SetValue( lProj );

			// テクスチャをシェーダに登録
			mDepthTextureParam.SetValue( depthMap.RenderTarget );
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

		public bool FogEnable{
			set{ mFogEnagle = value; }
		}
	}
}

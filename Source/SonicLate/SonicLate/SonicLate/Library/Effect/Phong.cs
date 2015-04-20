using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SonicLate{
	/// <summary>
	/// フォンシェーディングエフェクト
	/// </summary>
	class Phong : EffectParent{
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="device">グラフィックスデバイス</param>
		/// <param name="content">コンテントマネージャ</param>
		public Phong( GraphicsDevice device, ContentManager content ){
			// エフェクトを読み込む
			mEffect = content.Load<Effect>( "Effect/Phong" );

			// テクニックを登録
			mEffect.CurrentTechnique = mEffect.Techniques[ "PhongTec" ];

			// パラメータを取得
			mMatWorldParam = mEffect.Parameters[ "matWorld" ];
			mMatVPParam = mEffect.Parameters[ "matVP" ];
			mMatBoneParam = mEffect.Parameters[ "bones" ];
			mLightDirParam = mEffect.Parameters[ "lightDir" ];
			mAmbientParam = mEffect.Parameters[ "ambient" ];
			mDiffuseParam = mEffect.Parameters[ "diffuse" ];
			mTextureParam = mEffect.Parameters[ "texMesh" ];
			mEyePosParam = mEffect.Parameters[ "eyePos" ];
		}

		/// <summary>
		/// 通常のテクニックに設定する
		/// </summary>
		public override void SetToDefaultTechniques(){
			mEffect.CurrentTechnique = mEffect.Techniques[ "PhongTec" ];
		}

		/// <summary>
		/// スキニング用のテクニックに設定する
		/// </summary>
		public override void SetToSkinningTechniques(){
			mEffect.CurrentTechnique = mEffect.Techniques[ "PhongSkinedTec" ];
		}
	}
}

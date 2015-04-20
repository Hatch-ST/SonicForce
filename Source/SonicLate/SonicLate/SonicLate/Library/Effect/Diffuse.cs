using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SonicLate{
	/// <summary>
	/// 影なしエフェクト
	/// </summary>
	class Diffuse : EffectParent{
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="device">グラフィックスデバイス</param>
		/// <param name="content">コンテントマネージャ</param>
		public Diffuse( GraphicsDevice device, ContentManager content ){
			// エフェクトを読み込む
			mEffect = content.Load<Effect>( "Effect/Diffuse" );

			// テクニックを登録
			mEffect.CurrentTechnique = mEffect.Techniques[ "DiffuseTec" ];

			// パラメータを取得
			mMatWorldParam = mEffect.Parameters[ "matWorld" ];
			mMatVPParam = mEffect.Parameters[ "matVP" ];
			mMatBoneParam = mEffect.Parameters[ "bones" ];
			mAmbientParam = mEffect.Parameters[ "ambient" ];
			mDiffuseParam = mEffect.Parameters[ "diffuse" ];
			mTextureParam = mEffect.Parameters[ "texMesh" ];
		}

		/// <summary>
		/// 通常のテクニックに設定する
		/// </summary>
		public override void SetToDefaultTechniques(){
			mEffect.CurrentTechnique = mEffect.Techniques[ "DiffuseTec" ];
		}

		/// <summary>
		/// スキニング用のテクニックに設定する
		/// </summary>
		public override void SetToSkinningTechniques(){
			mEffect.CurrentTechnique = mEffect.Techniques[ "SkinnedDiffuseTec" ];
		}
	}
}

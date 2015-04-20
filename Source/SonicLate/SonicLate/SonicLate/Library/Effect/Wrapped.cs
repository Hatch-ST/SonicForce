using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SonicLate{
	/// <summary>
	/// 後光レンダリングエフェクト
	/// </summary>
	class Wrapped : EffectParent{
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="device">グラフィックスデバイス</param>
		/// <param name="content">コンテントマネージャ</param>
		public Wrapped( GraphicsDevice device, ContentManager content ){
			// エフェクトを読み込む
			mEffect = content.Load<Effect>( "Effect/WrappedDiffuse" );

			// テクニックを登録
			mEffect.CurrentTechnique = mEffect.Techniques[ "WrappedTec" ];

			// パラメータを取得
			mMatWorldParam = mEffect.Parameters[ "matWorld" ];
			mMatVPParam = mEffect.Parameters[ "matVP" ];
			mMatBoneParam = mEffect.Parameters[ "bones" ];
			mLightDirParam = mEffect.Parameters[ "lightDir" ];
			mAmbientParam = mEffect.Parameters[ "ambient" ];
			mDiffuseParam = mEffect.Parameters[ "diffuse" ];
			mTextureParam = mEffect.Parameters[ "texMesh" ];
			mEyeDirParam = mEffect.Parameters[ "viewVec" ];
		}

		/// <summary>
		/// 通常のテクニックに設定する
		/// </summary>
		public override void SetToDefaultTechniques(){
			mEffect.CurrentTechnique = mEffect.Techniques[ "WrappedTec" ];
		}

		/// <summary>
		/// スキニング用のテクニックに設定する
		/// </summary>
		public override void SetToSkinningTechniques(){
			mEffect.CurrentTechnique = mEffect.Techniques[ "WrappedSkinedTec" ];
		}
	}
}

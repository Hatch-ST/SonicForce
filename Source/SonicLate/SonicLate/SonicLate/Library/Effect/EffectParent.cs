using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SonicLate{
	abstract class EffectParent{
		
		/// <summary>シェーダ</summary>
		protected Effect mEffect = null;

		/// <summary>テクスチャのパラメータ</summary>
		protected EffectParameter mTextureParam = null;

		/// <summary>ディフューズのパラメータ</summary>
		protected EffectParameter mDiffuseParam = null;
		
		/// <summary>ワールド変換行列のパラメータ</summary>
		protected EffectParameter mMatVPParam = null;
		
		/// <summary>ビュー変換行列×射影変換行列のパラメータ</summary>
		protected EffectParameter mMatWorldParam = null;
		
		/// <summary>ボーン配列のパラメータ</summary>
		protected EffectParameter mMatBoneParam = null;
		
		/// <summary>アンビエント色のパラメータ</summary>
		protected EffectParameter mAmbientParam = null;
		
		/// <summary>ライトの位置のパラメータ</summary>
		protected EffectParameter mLightPosParam = null;
	
		/// <summary>ライトの向きのパラメータ</summary>
		protected EffectParameter mLightDirParam = null;
		
		/// <summary>視点座標のパラメータ</summary>
		protected EffectParameter mEyePosParam = null;
		
		/// <summary>視線ベクトルのパラメータ</summary>
		protected EffectParameter mEyeDirParam = null;

		/// <summary>
		/// エフェクトを取得する
		/// </summary>
		public Effect Effect{
			get{ return mEffect; }
		}

		/// <summary>
		/// 通常のテクニックに設定する
		/// </summary>
		public abstract void SetToDefaultTechniques();

		/// <summary>
		/// スキニング用のテクニックに設定する
		/// </summary>
		public abstract void SetToSkinningTechniques();
		
		/// <summary>テクスチャ</summary>
		public Texture2D Texture{
			set{ if ( mTextureParam != null ) mTextureParam.SetValue( value ); }
		}

		/// <summary>ディフューズ</summary>
		public Vector4 Diffuse{
			set{ if ( mDiffuseParam != null ) mDiffuseParam.SetValue( value ); }
		}
		
		/// <summary>ワールド変換行列</summary>
		public Matrix WorldMatrix{
			set{ if ( mMatWorldParam != null ) mMatWorldParam.SetValue( value ); }
		}
		
		/// <summary>ビュー変換行列×射影変換行列</summary>
		public Matrix ViewProjMatrix{
			set{ if ( mMatVPParam != null ) mMatVPParam.SetValue( value ); }
		}
		
		/// <summary>ボーン配列</summary>
		public Matrix[] BoneMatrix{
			set{ if ( mMatBoneParam != null ) mMatBoneParam.SetValue( value ); }
		}
		
		/// <summary>アンビエント色</summary>
		public Vector4 Ambient{
			set{ if ( mAmbientParam != null ) mAmbientParam.SetValue( value ); }
		}
		
		/// <summary>ライトの位置</summary>
		public Vector3 LightPosition{
			set{ if ( mLightPosParam != null ) mLightPosParam.SetValue( value ); }
		}
		
		/// <summary>ライトの向き</summary>
		public Vector3 LightDirection{
			set{ if ( mLightDirParam != null ) mLightDirParam.SetValue( value ); }
		}
		
		/// <summary>視点座標</summary>
		public Vector3 EyePosition{
			set{ if ( mEyePosParam != null ) mEyePosParam.SetValue( value ); }
		}
		
		/// <summary>視線ベクトル</summary>
		public Vector3 EyeDirection{
			set{ if ( mEyeDirParam != null ) mEyeDirParam.SetValue( value ); }
		}
	}
}

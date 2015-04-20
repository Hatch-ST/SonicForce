using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SonicLate{
	/// <summary>
	/// エフェクト管理クラス
	/// </summary>
	class EffectManager{
		public enum Type{
			Basic, // 基本のシェーディング
			Phong, // フォンシェーディング
			Wrapped, // 回り込みを加味したシェーディング
			Fog, // フォグ
			DepthMap, // 深度マップ
			DepthShadow, // 深度影アリ
			Diffuse, // 影なし
			None,
		}

		private static Basic mBasic;
		private static Phong mPhong;
		private static Wrapped mWrapped;
		private static Fog mFog;
		private static DepthMap mDepthMap;
		private static DepthShadow mDepthShadow;
		private static Diffuse mDiffuse;

		public static void Initialize( GraphicsDevice device, ContentManager content ){
			mBasic = new Basic( device, content );
			mPhong = new Phong( device, content );
			mWrapped = new Wrapped( device, content );
			mFog = new Fog( device, content );
			mDepthMap = new DepthMap( device ,content, 512 );
			mDepthShadow = new DepthShadow( device, content );
			mDiffuse = new Diffuse( device, content );
		}

		public static EffectParent Get( Type type ){
			switch ( type ){
				case Type.Basic :
					return mBasic;
				case Type.Phong:
					return mPhong;
				case Type.Wrapped :
					return mWrapped;
				case Type.Fog :
					return mFog;
				case Type.DepthMap :
					return mDepthMap;
				case Type.DepthShadow :
					return mDepthShadow;
				case Type.Diffuse :
					return mDiffuse;
			}
			return null;
		}

		public static DepthMap DepthMap{
			get { return mDepthMap; }
		}

		public static DepthShadow DepthSadow{
			get { return mDepthShadow; }
		}

		public static Fog Fog{
			get { return mFog; }
		}
	}
}

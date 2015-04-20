using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SonicLate{
	/// <summary>
	/// デバッグ用円表示クラス
	/// </summary>
	class Circle{
		static BillBoard mBoard = null;
		static GraphicsDevice mDevice = null;

		/// <summary>
		/// 初期化する
		/// </summary>
		/// <param name="device">グラフィックスデバイス</param>
		/// <param name="content">コンテンツマネージャ</param>
		static public void Initialize( GraphicsDevice device, ContentManager content ){
			mDevice = device;
			mBoard = new BillBoard( content, "Image/light_3" );
		}

		/// <summary>
		/// 描画する
		/// </summary>
		/// <param name="camera">カメラ</param>
		/// <param name="position">位置</param>
		/// <param name="scale">幅と高さ</param>
		static public void Draw( Camera camera, Vector3 position, Vector2 scale ){
			mDevice.DepthStencilState = DepthStencilState.None;

			ModelStates states = new ModelStates( null );
			states.Position = position;
			states.Scale = new Vector3( scale.X, scale.Y, 0.0f );
			mBoard.Render( camera, states, 1.0f );
			
			mDevice.DepthStencilState = DepthStencilState.Default;
		}
	}
}

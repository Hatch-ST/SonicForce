using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace SonicLate{
	abstract class GameChild{
		/// <summary>
		/// グラフィックスデバイス
		/// </summary>
		static protected GraphicsDevice mDevice = null;

		/// <summary>
		/// コンテンツマネージャ
		/// </summary>
		static protected ContentManager mContent = null;

		/// <summary>
		/// スプライトフォント
		/// </summary>
		static protected SpriteFont mSpriteFont = null;

		/// <summary>
		/// スプライトバッチ
		/// </summary>
		static protected SpriteBatch mSpriteBatch;

		/// <summary>
		/// グラフィックスデバイスとコンテンツマネージャを登録する
		/// </summary>
		/// <param name="device">グラフィックスデバイス</param>
		/// <param name="content">コンテンツマネージャ</param>
		static public void setDeviceAndContent( GraphicsDevice device, ContentManager content ){
			mDevice = device;
			mContent = content;
			mSpriteBatch = new SpriteBatch( mDevice );
			mSpriteFont = mContent.Load<SpriteFont>( "SpriteFont" );
		}

		/// <summary>
		/// 更新を行う
		/// </summary>
		/// <returns>遷移先のGameChildインスタンス</returns>
		public virtual GameChild Update( GameTime time ){
			return null;
		}

		/// <summary>
		/// 描画を行う
		/// </summary>
		public virtual void Draw( GameTime time ){
		}

		/// <summary>
		/// コンテンツの削除
		/// </summary>
		public virtual void UnloadContent() {
		}

		/// <summary>
		/// デバッグ用の文字表示する
		/// </summary>
		/// <param name="message">表示テキスト</param>
		/// <param name="cood">表示座標</param>
		/// <param name="color">文字の色</param>
		protected void DrawText( string message, Vector2 cood, Color color ) {
			mSpriteBatch.DrawString( mSpriteFont, message, cood, color );
		}
	}
}

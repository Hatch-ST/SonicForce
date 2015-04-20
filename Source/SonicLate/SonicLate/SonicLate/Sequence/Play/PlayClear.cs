using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SonicLate{
	/// <summary>
	/// ゲーム中 クリア画面
	/// </summary>
	class PlayClear{

		private int mCount = 0;

		private Vector3 mPlayerMove = Vector3.Zero;

		private Mode mMode = Mode.Wait;

		private const int mWaitEndTime = 20;

		private Texture2D mTexGameClear = null;
		private Interpolate mTexGameClearAlpha = null;
		private Interpolate mTexGameClearBright = null;
		private BlackWhole mBlackWhole = null;

		public enum Mode{
			Wait,
			Infomation,
			Black,
			End
		}

		public PlayClear( ContentManager content ){
			mTexGameClear = content.Load<Texture2D>( "Image/play_gameclear" );
			mTexGameClearAlpha = new Interpolate();
			mTexGameClearBright = new Interpolate();
			mBlackWhole = new BlackWhole( content );
			
			mMode = Mode.Wait;
		}

		public void Initialize( Vector3 playerMove ){
			mTexGameClearAlpha = new Interpolate();
			mTexGameClearBright = new Interpolate();

			mPlayerMove = playerMove * 0.8f;

			mMode = Mode.Wait;
			mCount = 0;
		}

		public void Update( GameTime time, Camera camera, Player player ){

			switch ( mMode ){
				case Mode.Wait :
					if ( mCount <= mWaitEndTime ){
					}else{
						mCount = -1;
						mMode = Mode.Infomation;
						mBlackWhole.Close();
					}
					break;
				case Mode.Infomation :
					if ( mCount < 30 ){
						mTexGameClearAlpha.GetSin( 0.0f, 1.0f, 30 );
					}
					if ( mCount > 40 && mCount < 50 ){
						mTexGameClearBright.GetSin( 1.0f, 2.0f, 10 );
					}else if ( mCount >= 50 && mCount < 60 ){
						mTexGameClearBright.GetSin( 2.0f, 1.0f, 10 );
					}else{
						mTexGameClearBright.Get( 1.0f, 1.0f, 1 );
					}
					
					if ( mCount > 180 ){
						mCount = -1;
						mMode = Mode.Black;
					}
					break;
				case Mode.Black :
					if ( mCount < 60 ){
						mTexGameClearAlpha.GetSin( 1.0f, 0.0f, 60 );
					}else{
						mMode = Mode.End;
					}
					break;
				case Mode.End :
					break;
			}

			// プレイヤーを移動
			player.Position += mPlayerMove;

			// カメラを更新
			//camera.Target = camera.Target + ( player.Position - camera.Target ) / 100;

			camera.Update( time );

			++mCount;
		}

		public void Draw( Camera camera, Player player, GameTime time ){
			mBlackWhole.Draw( camera, player );

			if ( mMode == Mode.Infomation || mMode == Mode.Black ){
				float x = 650.0f;
				float y = 300.0f;
				float rate = 1.0f + ( 1.0f - mTexGameClearAlpha.Value ) * 2.0f;
				float width = mTexGameClear.Width * rate;
				float height = mTexGameClear.Height * mTexGameClearAlpha.Value * rate;
				SpriteBoard.RenderUseCenterPosition( mTexGameClear, x, y, width, height, mTexGameClearAlpha.Value, mTexGameClearBright.Value );
			}
		}

		public bool Ended{
			get { return ( mMode == Mode.End ); }
		}
	}
}

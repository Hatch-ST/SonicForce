using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SonicLate{
	/// <summary>
	/// ゲーム中 スタート画面
	/// </summary>
	class PlayStart{
		private readonly Vector3 mDefaultCameraRefTrs = Vector3.Zero;
		private int mCount = 0;

		private Mode mMode = Mode.Leave;

		private readonly Vector3 mCameraRefTrsOnFirst = new Vector3( 0.0f, 0.0f, -60.0f );
		private readonly Vector3 mCameraAngleOnFirst =  new Vector3( 0.1f, -0.2f, 0.0f );
		
		private const int mLeaveEndTime = 60;
		private readonly Vector3 mCameraRefTrsOnLeave = new Vector3( 0.0f, 0.0f, -150.0f );
		private readonly Vector3 mCameraAngleOnLeave = new Vector3( 0.1f, -0.2f, 0.0f );
		
		private const int mRotationEndTime = 60;
		private readonly Vector3 mCameraRefTrsOnRotation = new Vector3( 0.0f, 0.0f, -400.0f );
		private readonly Vector3 mCameraAngleOnRotation =  new Vector3( ( float )Math.PI * 0.5f, -( float )Math.PI * 0.5f, 0.0f );
		
		private const int mSetEndTime = 60;
		private readonly Vector3 mCameraRefTrsOnSet = new Vector3( 0.0f, 80.0f, -300.0f );
		private readonly Vector3 mCameraAngleOnSet =  new Vector3( 0.0f, -( float )Math.PI, 0.0f );
		
		private const int mWaitEndTime = 10;

		private const int mInfomationEndTime = 80;
		private Texture2D mTexGameStart = null;
		private Interpolate mTexGameStartAlpha = null;
		private Interpolate mTexGameStartBright = null;

		public enum Mode{
			Start,
			Leave,
			Rotation,
			Set,
			Wait,
			Infomation,
			End
		}

		public PlayStart( ContentManager content, Camera camera, Vector3 referenceTranslate ){
			mTexGameStart = content.Load<Texture2D>( "Image/play_gamestart" );
			mTexGameStartAlpha = new Interpolate();
			mTexGameStartBright = new Interpolate();

			mDefaultCameraRefTrs = referenceTranslate;
			mCameraRefTrsOnSet = new Vector3( referenceTranslate.X, referenceTranslate.Y, - referenceTranslate.Z );
			Initialize( camera );
		}

		public void Initialize( Camera camera ){
			mTexGameStartAlpha = new Interpolate();
			mTexGameStartBright = new Interpolate();

			mMode = Mode.Start;
			mCount = 0;
		}

		public void Update( GameTime time, Camera camera, Player player, bool toTutorial ){
			Vector3 rotaAngle = Vector3.Zero;
			Vector3 transMove = Vector3.Zero;

			if ( mMode == Mode.Start ){
				camera.ReferenceTranslate = mCameraRefTrsOnFirst;
				camera.SetRotation( mCameraAngleOnFirst );
				camera.Target = player.Position;
				mMode = Mode.Leave;

				// BGMを再生
				if ( !Config.IsSpecialMode ) {
					SoundManager.PlayMusic( SoundManager.Music.Game, true );
				}else{
					SoundManager.PlayMusic( SoundManager.Music.Game_B, true );
				}
			}

			switch ( mMode ){
				case Mode.Leave :
					if ( mCount <= mLeaveEndTime ){
						transMove = ( mCameraRefTrsOnLeave - mCameraRefTrsOnFirst ) / mLeaveEndTime;
					}else{
						mCount = 0;
						mMode = Mode.Rotation;
					}
					break;
				case Mode.Rotation :
					if ( mCount <= mRotationEndTime ){
						transMove = ( mCameraRefTrsOnRotation - mCameraRefTrsOnLeave ) / mRotationEndTime;
						rotaAngle = ( mCameraAngleOnRotation - mCameraAngleOnLeave ) / mRotationEndTime;
					}else{
						mCount = 0;
						mMode = Mode.Set;
					}
					break;
				case Mode.Set :
					if ( mCount <= mSetEndTime ){
						transMove = ( mCameraRefTrsOnSet - mCameraRefTrsOnRotation ) / mSetEndTime;
						rotaAngle = ( mCameraAngleOnSet - mCameraAngleOnRotation ) / mSetEndTime;
					}else{
						mCount = 0;
						mMode = Mode.Wait;
					}
					break;
				case Mode.Wait :
					if ( mCount <= mRotationEndTime ){
						camera.SetRotation( Vector3.Zero );
						camera.ReferenceTranslate = mDefaultCameraRefTrs;
					}else{
						mCount = 0;
						mMode = Mode.Infomation;
					}
					break;
				case Mode.Infomation :
					if ( mCount == 10 && !toTutorial ){
						//スタートボイス
						if ( !Config.IsSpecialMode ){
							SoundManager.Play(SoundManager.SE.GameStartA);
						}else{
							SoundManager.Play(SoundManager.SE.GameStartB);
						}
					}
					if ( mCount <= mInfomationEndTime ){
						if ( mCount < 10 ){
							mTexGameStartAlpha.GetSin( 0.0f, 1.0f, 10 );
						}
						if ( mCount > 40 && mCount < 50 ){
							mTexGameStartBright.GetSin( 1.0f, 2.0f, 10 );
						}else if ( mCount >= 50 && mCount < 60 ){
							mTexGameStartBright.GetSin( 2.0f, 1.0f, 10 );
						}else{
							mTexGameStartBright.Get( 1.0f, 1.0f, 1 );
						}

						if ( mCount > 110 ){
							mTexGameStartAlpha.GetSin( 1.0f, 0.0f, 10 );
						}
					}else{
						mCount = 0;
						mMode = Mode.End;
					}
					break;
				case Mode.End :
					break;
			}

			player.Position += new Vector3( 0.0f, 0.0f, -10.0f );

			// カメラを更新
			//camera.Target = player.Position;
			camera.Rotation( rotaAngle );
			camera.ReferenceTranslate += transMove;
			camera.Update( time );

			camera.Target = camera.Target + ( player.Position - camera.Target ) * 0.2f;

			++mCount;
		}

		public void UpdateToContinue( GameTime time ){
			mMode = Mode.Infomation;

			if ( mCount == 100 ){
				//スタートボイス
				if (!Config.IsSpecialMode) {
					SoundManager.Play(SoundManager.SE.GameStartA);
				} else {
					SoundManager.Play(SoundManager.SE.GameStartB);
				}
			}

			int wait = 60;
			if ( mCount <= mInfomationEndTime + wait ){
				if ( mCount > wait ){
					if ( mCount > 40 + wait && mCount < 50 + wait ){
						mTexGameStartBright.GetSin( 1.0f, 2.0f, 10 );
					}else if ( mCount >= 50 + wait && mCount < 60 + wait ){
						mTexGameStartBright.GetSin( 2.0f, 1.0f, 10 );
					}else{
						mTexGameStartBright.Get( 1.0f, 1.0f, 1 );
					}

					if ( mCount > 110 + wait ){
						mTexGameStartAlpha.GetSin( 1.0f, 0.0f, 10 );
					}
				}
			}else{
				mCount = 0;
				mMode = Mode.End;
			}
			
			++mCount;
		}

		public void Draw( GameTime time ){
			if ( mMode == Mode.Infomation ){
				float x = 300.0f;
				float y = 120.0f + ( mTexGameStart.Height * ( 1.0f - mTexGameStartAlpha.Value ) ) / 2.0f;
				float height = mTexGameStart.Height * mTexGameStartAlpha.Value;
				SpriteBoard.Render( mTexGameStart, x, y, mTexGameStart.Width, height, mTexGameStartAlpha.Value, mTexGameStartBright.Value );
			}
		}

		public bool Ended{
			get { return ( mMode == Mode.End ); }
		}
	}
}

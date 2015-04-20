using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace SonicLate{
	/// <summary>
	/// エンディング画面
	/// </summary>
	class Ending : GameChild{
		private Camera mCamera = null;
		private HLModel mModelPlayer = null;
		private ModelStates mPlayerStates = null;

		private HLModel mModelStage = null;
		private ModelStates mStageStates = null;
		private HLModel mModelLogo = null;
		private ModelStates mLogoStates = null;

		private Texture2D mTexture = null;

		private FadeTexture mTexCommand = null;

		private BlackOut mBlackOut = null;

		private Particle mParticle = null;

		private bool mEnd = false;

		private bool mFromOption = false;

		private static bool mDrawCommand = false;

		private int mCount = 0;

		public Ending( bool clear ){
			mModelStage = new HLModel( mContent, "Model/EndingObject" );
			mModelLogo = new HLModel( mContent, "Model/TitleObject" );
			
			mTexture = mContent.Load<Texture2D>( "Image/main_Player_top" );

			mTexCommand = new FadeTexture( mContent, "Image/credit_command", 60, true );

			mBlackOut = new BlackOut( mContent, 60, BlackOut.Mode.Close, false );
			mBlackOut.Open();

			mStageStates = new ModelStates( null );
			mLogoStates = new ModelStates( null );

			mModelPlayer = new HLModel( mContent, "Model/player" );
			mPlayerStates = new ModelStates( mModelPlayer.SkinData );
			mPlayerStates.SetAnimation( mModelPlayer, "Take 001", true, 0.0f );
			mPlayerStates.PositionZ = 10000.0f;
			mPlayerStates.SetAngleFromDirection( new Vector3( 0.0f, 0.0f, -1.0f ) );

			if ( Config.IsSpecialMode ){
				mModelPlayer.SetTexture( mTexture );
			}

			// カメラを初期化
			mCamera = new Camera();
			mCamera.FieldOfView = MathHelper.ToRadians( 60.0f );
			mCamera.AspectRatio = ( float )mDevice.Viewport.Width / ( float )mDevice.Viewport.Height;
			mCamera.NearPlaneDistance = 1.0f;
			mCamera.FarPlaneDistance = 9000.0f;
			mCamera.ReferenceTranslate = new Vector3( 0.0f, 40.0f, 100.0f );
			mCamera.Target = new Vector3( 0.0f, 0.0f, 0.0f );
			mCamera.Update();

			EffectManager.Fog.FogColor = Vector4.Zero;
			EffectManager.Fog.SetNearAndFarDepth( 0.0f, 2000.0f );
			
			mParticle = new Particle( new BillBoard( mContent, "Image/awa", 4 ), 100, Particle.Type.Snow );
			mParticle.SetMaxPosition( 2000.0f, 600.0f );
			mParticle.SetScaleXYRange( 150.0f, 100.0f );
			mParticle.SetSpeedRange( 10.0f, 5.0f );
			mParticle.SetTimeRange( 180, 120 );
			mParticle.Position = mPlayerStates.Position + new Vector3( 0.0f, -600.0f, 0.0f );
			mParticle.Initialize();
			mParticle.Enable = true;
			mParticle.ZTestEnable = false;

			mFromOption = !clear;

			if ( clear ){
				mDrawCommand = true;
			}
		}

		public override GameChild Update( GameTime time ){
			GameChild next = this;

			if ( mCount == 0 ){
				SoundManager.PlayMusic( SoundManager.Music.Ending, false );
			}

			Vector3 move = Vector3.Zero;
			float speed = 6.1f;

			Vector2 leftStick = InputManager.GetThumbSticksLeft( PlayerIndex.One );

			//上下左右反転か調べる
			if ( Config.IsReverseLeftRight ) {
				leftStick.X = -leftStick.X;
			}
			if ( Config.IsReverseUpDown ){
				leftStick.Y = -leftStick.Y;
			}
			
			move.X = leftStick.X;
			move.Y = leftStick.Y;
			
			if ( InputManager.IsKeyDown( Keys.Left ) ){
				move.X = -1.0f;
			}
			if ( InputManager.IsKeyDown( Keys.Right ) ){
				move.X = 1.0f;
			}
			if ( InputManager.IsKeyDown( Keys.Up ) ){
				move.Y = 1.0f;
			}
			if ( InputManager.IsKeyDown( Keys.Down ) ){
				move.Y = -1.0f;
			}

			if ( move.Length() > 0.0f ){
				move = Vector3.Normalize( move ) * speed;
			}
			move.Z = -speed;

			mPlayerStates.SetAngleFromDirection( move, 0.2f );

			mPlayerStates.Position += move;
			
			mPlayerStates.PositionX = Math.Max( -400.0f, Math.Min( 400.0f, mPlayerStates.PositionX ) );
			mPlayerStates.PositionY = Math.Max( -100.0f, Math.Min( 200.0f, mPlayerStates.PositionY ) );

			if ( mCount == 3030 ){
				SoundManager.Play( SoundManager.SE.Thank );
			}

			if ( mCount < 3030 ){
				mCamera.Target = mPlayerStates.Position;
			}else{
				mEnd = true;

				mTexCommand.In();

			}

			if ( mCount > 300 ){
				if ( InputManager.IsButtonDown( PlayerIndex.One, Buttons.A ) || InputManager.IsButtonDown( PlayerIndex.One, Buttons.Start ) || InputManager.IsJustKeyDown( Keys.Enter ) ){
					mBlackOut.Close();
				}
				if ( mBlackOut.Closed ){
					SoundManager.StopMusic();
					Release();
					if ( mFromOption ){
						next = new Option();
					}else{
						next = new Title();
					}
				}
			}

			mParticle.Position = mCamera.Target + new Vector3( 0.0f, -600.0f, 0.0f );
			mParticle.Update();

			mCamera.Update();

			++mCount;
				
			return next;
		}

		public override void Draw( GameTime time ){
			mDevice.Clear( Color.Black );

			if ( ( mCamera.Position - mPlayerStates.Position ).Length() < 2000.0f ){
				mModelPlayer.Render( mCamera, mPlayerStates, time, EffectManager.Type.Wrapped );
			}

			mModelStage.Render( mCamera, mStageStates, time, EffectManager.Type.Fog );
			mModelLogo.Render( mCamera, mLogoStates, time, EffectManager.Type.Fog );

			mParticle.Render( mDevice, mCamera );

			if ( mEnd && mDrawCommand ){
				mTexCommand.Render( 400, 620 );
			}

			mBlackOut.Draw();
		}

		public void Release(){
			mModelPlayer.Release();
			mModelLogo.Release();
			mModelStage.Release();
		}
	}
}

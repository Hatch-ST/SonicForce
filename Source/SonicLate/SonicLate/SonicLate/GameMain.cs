using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace SonicLate{
	public class GameMain : Microsoft.Xna.Framework.Game{
		GraphicsDeviceManager mGraphics;

		public static readonly Color BackGroundColor = new Color( 0.0f, 0.90f, 0.85f, 1.0f );
		public static readonly int ScreenWidth = 1280;
		public static readonly int ScreenHeight = 720;

		private GameChild mGame = null;

		public GameMain(){
			//TargetElapsedTime = TimeSpan.FromSeconds( 1.0 / 30.0 );

			mGraphics = new GraphicsDeviceManager( this );

			// ��ʉ𑜓x��ݒ�
			mGraphics.PreferredBackBufferWidth = ScreenWidth;
			mGraphics.PreferredBackBufferHeight = ScreenHeight;

			// �E�B���h�E�T�C�Y�̕ύX������
			Window.AllowUserResizing = false;

			// �}�E�X�J�[�\����\��
			IsMouseVisible = false;

			// �t���X�N���[��
			mGraphics.IsFullScreen = true;

			Content.RootDirectory = "Content";
		}

		// ������
		protected override void Initialize(){
			// �f�o�C�X�ƃR���e���c��o�^
			GameChild.setDeviceAndContent( GraphicsDevice, Content );
			
			BillBoard.Initialize( GraphicsDevice, Content );
			EffectManager.Initialize( GraphicsDevice, Content );
			SpriteBoard.Initialize( GraphicsDevice, Content, mGraphics.PreferredBackBufferWidth, mGraphics.PreferredBackBufferHeight );
			TransparentModelManager.Initialize( 32 );
			HLModel.Device = GraphicsDevice;
			SoundManager.Initialize( Content );
			Config.Initialize();
			Circle.Initialize( GraphicsDevice, Content );

			
			//mGame = new Play( Play.StageType.Normal );
			//mGame = new Title();
			mGame = new FullScreenWait();

			base.Initialize();
		}

		// ����̓ǂݍ���
		protected override void LoadContent(){
		}

		// �R���e���c�̍폜
		protected override void UnloadContent(){
			mGame.UnloadContent();
		}

		// �X�V
		protected override void Update( GameTime gameTime ){
			if ( InputManager.IsJustKeyDown( Keys.Escape ) ){
				if ( mGame.GetType().FullName != "SonicLate.Loading" ){
					this.Exit();
				}
			}

			InputManager.Update();
			
			GameChild next = mGame.Update( gameTime );
			if ( next != mGame ){
				mGame = next;
			}

			base.Update( gameTime );
		}

		// �`��
		protected override void Draw( GameTime gameTime ){
			GraphicsDevice.Clear( GameMain.BackGroundColor );
			

			mGame.Draw( gameTime );

			base.Draw( gameTime );
		}
	}
}

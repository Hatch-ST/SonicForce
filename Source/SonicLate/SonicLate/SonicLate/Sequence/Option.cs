using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace SonicLate {
	/// <summary>
	/// オプション画面
	/// </summary>
	class Option : GameChild {
		//テクスチャ
		Texture2D mOptionBack = null;
		Texture2D mOptionMenu = null;
		Texture2D mTriangle = null;
		Texture2D mMenuLine = null;
		Texture2D mVolumeBar = null;
		Texture2D mBackground = null;

		//表示座標
		Vector2 mOptionBackPos;
		Vector2 mOptionMenuPos;
		Vector2 mUpDownTrianglePos;
		Vector2 mLeftRightTrianglePos;
		Vector2 mVibrationTrianglePos;
		Vector2 mSEBarPos;
		Vector2 mBGMBarPos;
		Vector2 mMenuLinePos;

		//ボリューム
		int mSEVolumeNum;
		int mBGMVolumeNum;

		//メニュー番号
		int mMenuIndex;

		enum MenuName {
			SE = 0,
			BGM = 1,
			UpDown = 2,
			LeftRight = 3,
			Vibration = 4,
			Credit = 5,
			Back = 6
		}
		/// <summary>
		/// ボリューム最小
		/// </summary>
		const int mVolumeMin = 0;
		/// <summary>
		/// ボリューム最大
		/// </summary>
		const int mVolumeMax = 10;

		private BlackOut mBlackOut = null;
		private GameChild mNextSequence = null;

		public Option(){
			//テクスチャ読み込み
			mOptionBack = mContent.Load<Texture2D>("Image/option_back");
			mOptionMenu = mContent.Load<Texture2D>("Image/option_menu");
			mTriangle = mContent.Load<Texture2D>("Image/option_sankaku");
			mMenuLine = mContent.Load<Texture2D>("Image/option_sen");
			mVolumeBar = mContent.Load<Texture2D>("Image/option_volume");
			mBackground = mContent.Load<Texture2D>("Image/option_background");

			mSEVolumeNum = (int)(Config.SEVolume * 10);
			mBGMVolumeNum = (int)(Config.BGMVolume * 10);

			//座標設定
			mOptionBackPos = new Vector2(GameMain.ScreenWidth / 2 - mOptionBack.Width / 2, GameMain.ScreenHeight / 2 - mOptionBack.Height / 2);
			mOptionMenuPos = new Vector2(GameMain.ScreenWidth / 2 - mOptionMenu.Width / 2, GameMain.ScreenHeight / 2 - mOptionMenu.Height / 2);

			mBlackOut = new BlackOut( mContent, 20, BlackOut.Mode.Close, false );
			mBlackOut.Open();
		}
		/// <summary>
		/// 更新を行う
		/// </summary>
		/// <param name="time">時間</param>
		/// <returns>遷移先のGameChildインスタンス</returns>
		public override GameChild Update(GameTime time) {
			GameChild next = this;

			// 遷移先が決まっている
			if ( mNextSequence != null ){
				// ブラックアウトが終わったら遷移
				if ( mBlackOut.Closed ){
					return mNextSequence;
				}else{
					return next;
				}
			}

			//上下操作
			if (InputManager.IsJustKeyDown(Keys.Up) || InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.LeftThumbstickUp)
				|| InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.DPadUp)) {
				if (--mMenuIndex < (int)MenuName.SE) {
					mMenuIndex = (int)MenuName.Back;
				}
				SoundManager.Play(SoundManager.SE.Ok);
			} else if (InputManager.IsJustKeyDown(Keys.Down) || InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.LeftThumbstickDown)
				|| InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.DPadDown)) {
				if (++mMenuIndex > (int)MenuName.Back) {
					mMenuIndex = (int)MenuName.SE;
				}
				SoundManager.Play(SoundManager.SE.Ok);
			}
			switch (mMenuIndex) {
				case (int)MenuName.SE:
					if (InputManager.IsJustKeyDown(Keys.Left) ||
						InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.LeftThumbstickLeft) || InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.DPadLeft)) {
						if (--mSEVolumeNum < mVolumeMin) {
							mSEVolumeNum = mVolumeMin;
						}
						UpdateConfig();
						SoundManager.Play(SoundManager.SE.Marvelous);
					} else if (InputManager.IsJustKeyDown(Keys.Right) ||
						InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.LeftThumbstickRight) || InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.DPadRight)) {
						if (++mSEVolumeNum > mVolumeMax) {
							mSEVolumeNum = mVolumeMax;
						} else {
							UpdateConfig();
							SoundManager.Play(SoundManager.SE.Marvelous);
						}
					}
					break;
				case (int)MenuName.BGM:
					if (InputManager.IsJustKeyDown(Keys.Left) ||
						InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.LeftThumbstickLeft) || InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.DPadLeft)) {
						if (--mBGMVolumeNum < mVolumeMin) {
							mBGMVolumeNum = mVolumeMin;
						}
						UpdateConfig();
					} else if (InputManager.IsJustKeyDown(Keys.Right) ||
						InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.LeftThumbstickRight) || InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.DPadRight)) {
						if (++mBGMVolumeNum > mVolumeMax) {
							mBGMVolumeNum = mVolumeMax;
						}
						UpdateConfig();
					}
					break;
				case (int)MenuName.UpDown:
					if (InputManager.IsJustKeyDown(Keys.Left) || InputManager.IsJustKeyDown(Keys.Right) ||
						InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.LeftThumbstickLeft) || InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.LeftThumbstickRight) ||
						InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.DPadLeft)|| InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.DPadRight)) {
						Config.ChangeUpDown();
						SoundManager.Play(SoundManager.SE.Ok);
					}
					break;
				case (int)MenuName.LeftRight:
					if (InputManager.IsJustKeyDown(Keys.Left) || InputManager.IsJustKeyDown(Keys.Right) ||
						InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.LeftThumbstickLeft) || InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.LeftThumbstickRight) ||
						InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.DPadLeft) || InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.DPadRight)) {
						Config.ChangeLeftRight();
						SoundManager.Play(SoundManager.SE.Ok);
					}
					break;
				case (int)MenuName.Vibration:
					if (InputManager.IsJustKeyDown(Keys.Left) || InputManager.IsJustKeyDown(Keys.Right) ||
						InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.LeftThumbstickLeft) || InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.LeftThumbstickRight) ||
						InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.DPadLeft) || InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.DPadRight)) {
						Config.ChangeVibration();
						SoundManager.Play(SoundManager.SE.Ok);
					}
					break;
				case (int)MenuName.Credit:
					if (InputManager.IsJustKeyDown(Keys.Enter) || InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.A)) {
						UpdateConfig();
						next = new Ending( false );
						SoundManager.Play(SoundManager.SE.Ok);
					} 
					break;
				case (int)MenuName.Back:
					if (InputManager.IsJustKeyDown(Keys.Enter) || InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.A)) {
						UpdateConfig();
						SoundManager.Play(SoundManager.SE.Ok);
						mNextSequence = new Title();
						mBlackOut.Close();
					}
					break;
			}
			

			//座標設定
			//メニューライン
			if (mMenuIndex < (int)MenuName.Credit) {
				mMenuLinePos = new Vector2(mOptionBackPos.X + 115.0f, mOptionBackPos.Y + 175.0f + 51.0f * mMenuIndex);
			} else if (mMenuIndex == (int)MenuName.Credit) {
				mMenuLinePos = new Vector2(mOptionBackPos.X + 115.0f, mOptionBackPos.Y + 478.0f);
			} else {
				mMenuLinePos = new Vector2(mOptionBackPos.X + 115.0f, mOptionBackPos.Y + 548.0f);
			}

			//SEバー
			mSEBarPos = new Vector2(mOptionBackPos.X + 362.0f - mVolumeBar.Width / 2 + 29.3f*mSEVolumeNum, mOptionBackPos.Y + 162.0f);
			//BGMバー
			mBGMBarPos = new Vector2(mOptionBackPos.X + 362.0f - mVolumeBar.Width / 2 + 29.3f * mBGMVolumeNum, mOptionBackPos.Y + 162.0f+51.0f);
			

			//上下反転三角
			if (!Config.IsReverseUpDown) {
				mUpDownTrianglePos = new Vector2(mOptionBackPos.X + 360.0f, mOptionBackPos.Y + 285.0f);
			} else {
				mUpDownTrianglePos = new Vector2(mOptionBackPos.X + 360.0f + 185.0f, mOptionBackPos.Y + 285.0f);
			}
			//左右反転三角
			if (!Config.IsReverseLeftRight) {
				mLeftRightTrianglePos = new Vector2(mOptionBackPos.X + 360.0f, mOptionBackPos.Y + 285.0f + 50.0f);
			} else {
				mLeftRightTrianglePos = new Vector2(mOptionBackPos.X + 360.0f + 185.0f, mOptionBackPos.Y + 285.0f+ 50.0f);
			}
			//振動三角
			if (Config.IsVibrationOn) {
				mVibrationTrianglePos = new Vector2(mOptionBackPos.X + 360.0f, mOptionBackPos.Y + 285.0f + 50.0f * 2);
			} else {
				mVibrationTrianglePos = new Vector2(mOptionBackPos.X + 360.0f + 185.0f, mOptionBackPos.Y + 285.0f + 50.0f * 2);
			}

			return next;
		}

		
		// 描画
		public override void Draw(GameTime time) {
			//画面クリア
			mDevice.Clear(Color.Black);
			mSpriteBatch.Begin();
			mSpriteBatch.Draw(mBackground,Vector2.Zero, Color.White);
			mSpriteBatch.Draw(mOptionBack, mOptionBackPos, Color.White);
			mSpriteBatch.End();

			SpriteBoard.Render(mMenuLine, mMenuLinePos.X, mMenuLinePos.Y, 1.0f);
			SpriteBoard.Render(mOptionMenu,mOptionMenuPos.X,mOptionMenuPos.Y, 1.0f);
			SpriteBoard.Render(mVolumeBar, mSEBarPos.X, mSEBarPos.Y, 1.0f);
			SpriteBoard.Render(mVolumeBar, mBGMBarPos.X, mBGMBarPos.Y, 1.0f);

			SpriteBoard.RenderUseCenterPosition(mTriangle, mUpDownTrianglePos.X, mUpDownTrianglePos.Y,mTriangle.Width,mTriangle.Height, 1.0f,1.0f);
			SpriteBoard.RenderUseCenterPosition(mTriangle, mLeftRightTrianglePos.X, mLeftRightTrianglePos.Y, mTriangle.Width, mTriangle.Height, 1.0f, 1.0f);
			SpriteBoard.RenderUseCenterPosition(mTriangle, mVibrationTrianglePos.X, mVibrationTrianglePos.Y, mTriangle.Width, mTriangle.Height, 1.0f, 1.0f);

			mBlackOut.Draw();

		}

		/// <summary>
		/// コンフィグを更新する
		/// </summary>
		private void UpdateConfig() {
			Config.SetSEVolume((float)mSEVolumeNum / 10);
			Config.SetBGMVolume((float)mBGMVolumeNum / 10);
			SoundManager.Update();
		}

	}
}

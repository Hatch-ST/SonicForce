using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace SonicLate {
	/// <summary>
	/// タイトル画面
	/// </summary>
	class Title : GameChild {
		//テクスチャ
		Texture2D mTitleLogo = null;
		MultiTexture mPushStartMessage = null;
		MultiTexture mMainMenu = null;

		private const int mTitleLoopTime = 2 * 60 * 60;

		/// <summary>カメラ</summary>
		private Camera mCamera = null;

		/// <summary>背景ステージ</summary>
		private HLModel mBackgroundStage = null;

		private float mMenuAlpha = 0.0f;

		/// <summary>
		/// タイトルがアニメーションする番号
		/// </summary>
		int mLogoAnimationNum;
		/// <summary>
		/// アニメーション時のロゴの位置
		/// </summary>
		Vector3 mTitleLogoPos;
		/// <summary>
		/// アニメーション時のロゴの大きさ
		/// </summary>
		Vector3 mTitleLogoSize;

		BlackOut mBlackOut;

		Interpolate3 mInterPos;
		Interpolate3 mInterSize;

		/// <summary>
		/// スタートボタンが押されたか
		/// </summary>
		bool mPushedStart;
		/// <summary>
		/// メッセージを点滅させるか
		/// </summary>
		bool mIsBlinc;

		/// <summary>
		/// 選択中のメニュー番号
		/// </summary>
		MenuName mSelectedMenuIndex = MenuName.GameStart;

		/// <summary>
		/// 描画するテクスチャの番号
		/// </summary>
		int mDrawNum;

		/// <summary>
		/// 時間カウント
		/// </summary>
		int mCount;

		HLModel mModel = null;
		ModelStates mStates = null;

		/// <summary>
		/// 遷移先一時格納用
		/// </summary>
		GameChild mNextSequence = null;

		/// <summary>
		/// メニュー番号の名前
		/// </summary>
		private enum MenuName {
			GameStart = 1,
			HowToPlay = 2,
			KeyConfig = 3,
			Length = 4
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public Title() {
			mTitleLogo = mContent.Load<Texture2D>("Image/title_logo");
			mPushStartMessage = new MultiTexture(mContent,"Image/title_push" , 2);
			mMainMenu = new MultiTexture(mContent, "Image/title_menu", 4);

			// カメラを初期化
			mCamera = new Camera();
			mCamera.FieldOfView = MathHelper.ToRadians(60.0f);
			mCamera.AspectRatio = (float)mDevice.Viewport.Width / (float)mDevice.Viewport.Height;
			mCamera.NearPlaneDistance = 1.0f;
			mCamera.FarPlaneDistance = 10000.0f;
			mCamera.ReferenceTranslate = new Vector3(-400.0f, 0.0f, 100.0f);
			mCamera.Target = new Vector3(0.0f, 0.0f, 0.0f);
			mCamera.Update();

			// 背景ステージを初期化
			mBackgroundStage = new HLModel(mContent, "Model/stage_title");

			mBlackOut = new BlackOut(mContent,60,BlackOut.Mode.Close,false);
			mTitleLogoPos = new Vector3(GameMain.ScreenWidth + 20.0f, 40.0f,0.0f);
			mTitleLogoSize = new Vector3(mTitleLogo.Width, mTitleLogo.Height,0.0f);

			mInterPos = new Interpolate3();
			mInterSize = new Interpolate3();
			mModel = new HLModel( mContent, "Model/player" );
			mStates = new ModelStates( mModel.SkinData );
			mStates.SetAnimation( mModel, "Take 001", true, 0.0f );

			init();
		}

		//初期化
		private void init() {
			mCamera.Target = new Vector3(0.0f, 0.0f, 0.0f);
			mLogoAnimationNum = 0;
			mPushedStart = false;
			mIsBlinc = false;
			mDrawNum = 0;
			mCount = 0;

			mTitleLogoPos = new Vector3(GameMain.ScreenWidth + 20.0f, 40.0f, 0.0f);
			mTitleLogoSize = new Vector3(mTitleLogo.Width, mTitleLogo.Height, 0.0f);
			SoundManager.StopMusic();

			//隠しモード解除
			Config.SetSpecialMode(false);
		}

		/// <summary>
		/// 更新を行う
		/// </summary>
		/// <param name="time">時間</param>
		/// <returns>遷移先のGameChildインスタンス</returns>
		public override GameChild Update(GameTime time) {
			GameChild next = this;
			if (mCount == mTitleLoopTime) {
				mBlackOut.Close();
			} else if (mCount > mTitleLoopTime) {
				if (mBlackOut.Closed) {
					init();
				}
			}

			//ロゴアニメーション中
			if (mLogoAnimationNum > -1) {
				//アニメーションカット
				if (InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.Start) || InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.A) ||
					InputManager.IsJustKeyDown(Keys.Enter)) {
					mLogoAnimationNum = -1;
					mBlackOut.Open();
					if (!SoundManager.IsPlaying()) {
						SoundManager.PlayMusic(SoundManager.Music.Title, true);
					}
				}
				switch (mLogoAnimationNum) {
					case 0:
						mTitleLogoPos.X -= 30;
						if (mTitleLogoPos.X < 0) {
							mTitleLogoPos.X = 0;
							mLogoAnimationNum++;
						}
						break;
					case 1:
						mTitleLogoSize.X -= 25;
						mTitleLogoSize.Y += 3;
						if (mTitleLogoSize.X < 300) {
							mTitleLogoSize.X = 300;
							mLogoAnimationNum++;
						}
						break;
					case 2:
						mTitleLogoSize.X += 30;
						mTitleLogoSize.Y -= 3;
						if (mTitleLogoSize.X > mTitleLogo.Width + 150) {
							mLogoAnimationNum++;
							mInterPos.Get(mTitleLogoPos, new Vector3(GameMain.ScreenWidth / 2 - mTitleLogo.Width / 2 + 50.0f, 40.0f, 0.0f), 30);
							mInterPos.Time = 0;
							mInterSize.Get(mTitleLogoSize, new Vector3(mTitleLogo.Width, mTitleLogo.Height, 0.0f), 30);
							mInterSize.Time = 0;
						}
						break;
					case 3:
						if (mCount == 0) {
							SoundManager.Play(SoundManager.SE.Title);
						}
						mTitleLogoPos = mInterPos.Get(30);
						mTitleLogoSize = mInterSize.Get(30);
						
						if (mCount == 95) {
							mLogoAnimationNum++;
							mBlackOut.Open();
							if (!SoundManager.IsPlaying()) {
								SoundManager.PlayMusic(SoundManager.Music.Title, true);
							}
						}
						mCount++;
						break;
					case 4:
						if (mBlackOut.Opened) {
							mLogoAnimationNum = -1;
						}
						break;
				}
			} else {
				//点滅の更新
				if (mCount % 10 == 0) {
					mIsBlinc = !mIsBlinc;
				}
				
				int count = mCount % 60;
				if ( count < 30 ){
					mMenuAlpha = count / 30.0f;
				}else{
					mMenuAlpha = 1.0f - ( ( count - 30 ) / 30.0f );
				}

				//スタートを押す前
				if (!mPushedStart) {
					if (InputManager.IsJustKeyDown(Keys.Enter) || InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.Start)) {
						mPushedStart = true;
					}
				}
					//スタートを押した後
				else {
					// 遷移先が決まっている
					if ( mNextSequence != null ){
						// ブラックアウトが終わったら遷移
						if ( mBlackOut.Closed ){
							return mNextSequence;
						}else{
							return next;
						}
					}

					//決定を押したとき
					if (InputManager.IsJustKeyDown(Keys.Enter) || InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.A)) {
						SoundManager.Play(SoundManager.SE.Ok);
						if (mSelectedMenuIndex == MenuName.GameStart) {
							//隠しコマンド
							if (InputManager.IsButtonDown(PlayerIndex.One, Buttons.Back) && InputManager.IsButtonDown(PlayerIndex.One, Buttons.RightTrigger)) {
								Config.SetSpecialMode(true);
							}
							if (InputManager.IsKeyDown(Keys.Back) && InputManager.IsKeyDown(Keys.R) && InputManager.IsKeyDown(Keys.T)){
								Config.SetSpecialMode(true);
							}
							SoundManager.StopMusic();
							mNextSequence = new Loading(Loading.LoadClass.Play);
						} else if (mSelectedMenuIndex == MenuName.HowToPlay) {
							SoundManager.StopMusic();
							mNextSequence = new Loading(Loading.LoadClass.Tutorial);
						} else{
							mNextSequence = new Option();
						}

						// ブラックアウト
						mBlackOut.Close();
					}
					//メニューカーソルの移動
					if (InputManager.IsJustKeyDown(Keys.Down) ||
						InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.LeftThumbstickDown) || InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.DPadDown)) {
						if (++mSelectedMenuIndex == MenuName.Length) {
							mSelectedMenuIndex = MenuName.GameStart;
						}
					} else if (InputManager.IsJustKeyDown(Keys.Up) ||
						InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.LeftThumbstickUp) || InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.DPadUp)) {
						if (--mSelectedMenuIndex < MenuName.GameStart) {
							mSelectedMenuIndex = MenuName.Length - 1;
						}
					}

				}
				//点滅しているか判定
				if (mIsBlinc) {
					//mDrawNum = 0;
				} else {
					//スタートを押す前も1で対応している
					mDrawNum = (int)mSelectedMenuIndex;
				}
				
				mCount++;
			}
			mCamera.Move(new Vector3(0.0f, 0.0f, 1.0f));
			//カメラの更新
			mCamera.Update(time);
			return next;

		}

		//描画
		public override void Draw(GameTime time) {
			
			//画面クリア
			mDevice.Clear(GameMain.BackGroundColor);
			//ステージの描画
			mBackgroundStage.Render(mCamera, new ModelStates(null), time);
			//タイトルロゴアニメーション中
			if (mLogoAnimationNum >-1) {
				//ホワイトアウト
				mBlackOut.Draw();
				//アニメーションタイトルロゴ
				SpriteBoard.Render(mTitleLogo, mTitleLogoPos.X, mTitleLogoPos.Y, mTitleLogoSize.X, mTitleLogoSize.Y, 1.0f);
				
			} else {
				//mModel.Render(mCamera, mStates, time);

				//タイトルロゴ
				SpriteBoard.Render(mTitleLogo, GameMain.ScreenWidth / 2 - mTitleLogo.Width / 2 + 50.0f, 40.0f, mTitleLogo.Width, mTitleLogo.Height, 1.0f);
				//スタートを押す前
				if (!mPushedStart) {
					//プッシュスタートボタン
					SpriteBoard.Render(mPushStartMessage.Get[1], GameMain.ScreenWidth / 2 - mPushStartMessage.Get[1].Width / 2, GameMain.ScreenHeight - mPushStartMessage.Get[1].Height - 160.0f, mPushStartMessage.Get[1].Width, mPushStartMessage.Get[1].Height, mMenuAlpha);
					SpriteBoard.Render(mPushStartMessage.Get[0], GameMain.ScreenWidth / 2 - mPushStartMessage.Get[0].Width / 2, GameMain.ScreenHeight - mPushStartMessage.Get[0].Height - 160.0f, mPushStartMessage.Get[0].Width, mPushStartMessage.Get[0].Height, 1.0f);
				}
					//スタートを押した後
				else {
					//メインメニュー
					SpriteBoard.Render(mMainMenu.Get[mDrawNum], GameMain.ScreenWidth / 2 - mMainMenu.Get[mDrawNum].Width / 2, GameMain.ScreenHeight - mMainMenu.Get[mDrawNum].Height - 80.0f, mMainMenu.Get[mDrawNum].Width, mMainMenu.Get[mDrawNum].Height, mMenuAlpha);
					SpriteBoard.Render(mMainMenu.Get[0], GameMain.ScreenWidth / 2 - mMainMenu.Get[0].Width / 2, GameMain.ScreenHeight - mMainMenu.Get[0].Height - 80.0f, mMainMenu.Get[0].Width, mMainMenu.Get[0].Height, 1.0f);
				}
				//if (mCount > mTitleLoopTime) {
				//    mBlackOut.Draw();
				//}
				mBlackOut.Draw();
			}
			
		}
	}
}

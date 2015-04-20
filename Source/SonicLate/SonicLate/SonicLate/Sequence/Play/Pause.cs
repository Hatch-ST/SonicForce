using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace SonicLate {
	/// <summary>
	/// ゲーム中 ポーズ画面
	/// </summary>
	class Pause {
		//テクスチャ
		private Texture2D mPauseTex;		//ポーズ
		private MultiTexture mPauseMenu;	//ポーズメニュー
		private Texture2D mIconHitode;		//ヒトデのアイコン
		private Texture2D mPauseCheck;		//タイトルへの確認
		private MultiTexture mYesNo;		//YesNoの確認

		private MenuName mSelectedMenuIndex;//選択中のメニュー番号
		private bool mIsSelectedYes;		//Yesを選択中か
		private YesNo mYesNoIndex;			//YesNoの番号
		private int mCount;
		private bool mIsDrawCheck;			//確認を表示しているか

		private readonly Vector2 mMenuDrawPos;//メニューの表示座標
		private Vector2 mIconDrawPos;		//ヒトデアイコンの表示座標
		private readonly Vector2 mIconCenter;	//ヒトデアイコンの中心
		private float mHitedeRotete;		//ヒトデ回転角
		private bool mIsDrawHitode;			//ヒトデを描画するか
		private readonly Vector2 mCheckCenterPos;	//確認の座標
		private readonly Vector2 mYesNoCenterPos;	//YesNoの座標

		private float mCheckSize;			//確認の大きさ

		private Command mCommand = null; // 隠しコマンド

		private bool mBackToTitle = false;
		private BlackOut mBlackOut = null;
		
		/// <summary>
		/// メニュー番号の名前
		/// </summary>
		private enum MenuName {
			ReturnGame = 0,
			ReturnTitle = 1,
			Length = 2
		}
		/// <summary>
		/// YesNoの番号
		/// </summary>
		private enum YesNo{
			Yes = 0,
			No = 1
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="content">コンテンツマネージャ</param>
		public Pause(ContentManager content) {
			// テクスチャ読み込み
			mPauseTex = content.Load<Texture2D>("Image/pause");
			mPauseMenu = new MultiTexture(content, "Image/pause_menu", 2);
			mIconHitode = content.Load<Texture2D>("Image/icon_hitode");
			mPauseCheck = content.Load<Texture2D>("Image/pause_check");
			mYesNo = new MultiTexture(content, "Image/pause_yesno", 2);

			//メニューの表示座標決定
			mMenuDrawPos = new Vector2(GameMain.ScreenWidth - mPauseMenu.Get[0].Width - 50.0f, 450.0f);
			mIconCenter = new Vector2(mIconHitode.Width / 2, mIconHitode.Height / 2);
			mIconDrawPos = new Vector2(mMenuDrawPos.X-10.0f, mMenuDrawPos.Y+65.0f);
			mCheckCenterPos = new Vector2(GameMain.ScreenWidth / 2, GameMain.ScreenHeight / 2);
			mYesNoCenterPos = new Vector2(GameMain.ScreenWidth / 2, GameMain.ScreenHeight / 2 + 40.0f);

			mSelectedMenuIndex = MenuName.ReturnGame;
			mIsSelectedYes = false;
			mIsDrawCheck = false;
			mIsDrawHitode = true;
			mHitedeRotete = 0;
			mCount = 0;
			mCheckSize = 0.0f;

			mBlackOut = new BlackOut( content, 20, BlackOut.Mode.Open, false );

			mCommand = new Command();
		}

		/// <summary>
		/// 更新を行う
		/// </summary>
		/// <param name="player">プレイヤー</param>
		/// <returns>タイトルに戻る場合trueを返す</returns>
		public bool Update(Player player) {
			// タイトル画面に移行中
			if ( mBackToTitle ){
				// ブラックアウトが終わったらタイトルへ
				if ( mBlackOut.Closed ){
					return mBackToTitle;
				}else{
					return false;
				}
			}

			//ポーズ中かチェック
			if (!player.OnPause) {
				// 隠しコマンドをリセット
				mCommand.Reset();

				return false;
			}
			//確認表示中でない
			if (!mIsDrawCheck) {
				//メニューカーソルの移動
				if (InputManager.IsJustKeyDown(Keys.Down) ||
					InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.LeftThumbstickDown) || InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.DPadDown)) {
					if (++mSelectedMenuIndex == MenuName.Length) {
						mSelectedMenuIndex = MenuName.ReturnGame;
					}
				}
				if (InputManager.IsJustKeyDown(Keys.Up) ||
					InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.LeftThumbstickUp) || InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.DPadUp)) {
					if (--mSelectedMenuIndex < MenuName.ReturnGame) {
						mSelectedMenuIndex = MenuName.Length - 1;
					}
				}
				//決定を押したとき
				if ((InputManager.IsJustKeyDown(Keys.Enter) || InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.Start) || InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.A))
					&& mCount > 30) {
					SoundManager.Play(SoundManager.SE.Ok);
					if (mSelectedMenuIndex == MenuName.ReturnGame) {
						//ポーズ解除
						player.CancelPause();
						mCount = 0;
					} else if (mSelectedMenuIndex == MenuName.ReturnTitle) {
						mIsDrawCheck = true;
						mIsDrawHitode = false;
						mCount = 0;
					}
				}
				//ヒトデアイコンの座標更新(メニュー横に)
				if (mSelectedMenuIndex == MenuName.ReturnGame) {
					mIconDrawPos.X = mMenuDrawPos.X-10.0f;
					mIconDrawPos.Y = mMenuDrawPos.Y+65.0f;
				} else if (mSelectedMenuIndex == MenuName.ReturnTitle) {
					mIconDrawPos.X = mMenuDrawPos.X-10.0f + 30.0f;
					mIconDrawPos.Y = mMenuDrawPos.Y+65.0f + 70.0f;
				}
			}
 			//確認表示中
			else {
				//YesNoカーソルの移動
				if (InputManager.IsJustKeyDown(Keys.Left) || InputManager.IsJustKeyDown(Keys.Right) ||
				InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.LeftThumbstickLeft) || InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.LeftThumbstickRight) ||
					InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.DPadLeft) || InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.DPadRight)) {
					mIsSelectedYes = !mIsSelectedYes;
				}
				//決定を押したとき
				if ((InputManager.IsJustKeyDown(Keys.Enter) || InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.Start) || InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.A))
					&& mCount > 10) {
					SoundManager.Play(SoundManager.SE.Ok);
					if (mIsSelectedYes) {
						//タイトルへ
						mBackToTitle =  true;
						mBlackOut.Close();
					} else {
						mIsDrawCheck = false;
						mCheckSize = 0.0f;
					}
				}
				//Bでキャンセル
				else if(InputManager.IsJustButtonDown(PlayerIndex.One, Buttons.B)){
					mIsDrawCheck = false;
					mIsSelectedYes = false;
					mCheckSize = 0.0f;
				}
				//ヒトデアイコンの座標更新(YesNoの横に)
				if (mIsSelectedYes) {
					mIconDrawPos.X = mYesNoCenterPos.X-60.0f - mYesNo.Get[0].Width / 2;
					mIconDrawPos.Y = mYesNoCenterPos.Y - mYesNo.Get[0].Height / 2;
					//YesNo番号も更新
					mYesNoIndex = YesNo.Yes;
				} else {
					mIconDrawPos.X = mYesNoCenterPos.X + 180.0f - mYesNo.Get[0].Width / 2;
					mIconDrawPos.Y = mYesNoCenterPos.Y - mYesNo.Get[0].Height / 2;
					//YesNo番号も更新
					mYesNoIndex = YesNo.No;
				}
				//確認の大きさを変える
				if (mCount <= 10) {
					mCheckSize = mCount * 0.1f;
				} else {
					mIsDrawHitode = true;
				}
			}
			
			// 隠しコマンドを更新
			mCommand.Update();

			// コマンドに成功
			if ( mCommand.IsSucceed() ){
				mCommand.Reset();
				SoundManager.Play( SoundManager.SE.Fantastic );

				// デバッグモードに設定
				Config.SetDebugMode( true );
			}

			mIconDrawPos += mIconCenter;
			mHitedeRotete = (float)Math.PI * 2.0f * ((mCount % 100) / 100.0f);
			mCount++;
			return false;
		}

		/// <summary>
		/// 描画を行う
		/// </summary>
		/// <param name="time">時間</param>
		public void Draw(GameTime time, SpriteBatch spriteBatch,Player player) {
			//ポーズ中かチェック
			if (!player.OnPause) {
				return;
			}
			//ポーズ
			SpriteBoard.Render(mPauseTex, 20.0f, 50.0f, mPauseTex.Width, mPauseTex.Height, 1.0f);
			//ポーズメニュー
			SpriteBoard.Render(mPauseMenu.Get[(int)mSelectedMenuIndex], mMenuDrawPos.X,mMenuDrawPos.Y, mPauseMenu.Get[(int)mSelectedMenuIndex].Width, mPauseMenu.Get[(int)mSelectedMenuIndex].Height, 1.0f);

			

			//確認表示中 
			if(mIsDrawCheck) {
				//確認
				SpriteBoard.RenderUseCenterPosition(mPauseCheck, mCheckCenterPos.X, mCheckCenterPos.Y, mPauseCheck.Width * mCheckSize, mPauseCheck.Height * mCheckSize, 1.0f, 1.0f);
				//YesNo
				SpriteBoard.RenderUseCenterPosition(mYesNo.Get[(int)mYesNoIndex], mYesNoCenterPos.X, mYesNoCenterPos.Y, mYesNo.Get[(int)mYesNoIndex].Width * mCheckSize, mYesNo.Get[(int)mYesNoIndex].Height * mCheckSize, 1.0f, 1.0f);

			}
			//ヒトデのアイコン
			if (mIsDrawHitode) {
				spriteBatch.Begin();
				spriteBatch.Draw(mIconHitode, mIconDrawPos, null, Color.White, mHitedeRotete, mIconCenter, 1.0f, SpriteEffects.None, 0.0f);
				spriteBatch.End();
			}
			
			mBlackOut.Draw();
		}
	}
}

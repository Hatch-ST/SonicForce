using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Threading;

namespace SonicLate {
	/// <summary>
	/// ロード画面
	/// </summary>
	class Loading : GameChild{
		private MultiTexture mLoadingTexture;	//ローディング画像
		private int mTextureNum;					//テクスチャの数
		private int mTextureDrawIndex;			//テクスチャを描画する番号
		private Vector2 mTexturePos;			//テクスチャの表示座標
		private int mCount;

		private Texture2D mIconHitode;		//ヒトデのアイコン
		private Vector2 mIconPos;		//ヒトデアイコンの表示座標
		private readonly Vector2 mIconCenter;	//ヒトデアイコンの中心
		private float mHitedeRotete;		//ヒトデ回転角

		private GameChild mLoadingChild;		//ロードするGameChild
		private Thread mLoadingThread;			//ロードスレッド

		private LoadClass mLoadClass;		//ステージタイプ

		private Camera mCamera = null;
		private HLModel mModelPlayer = null;
		private ModelStates mPlayerStates = null;

		public enum LoadClass{
			Play = 0,
			Tutorial = 1
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public Loading(LoadClass loadClass) {
			mTextureNum = 4;
			//テクスチャ読み込み
			mLoadingTexture = new MultiTexture(mContent, "Image/now_loading", mTextureNum);
			mIconHitode = mContent.Load<Texture2D>("Image/icon_hitode");

			mTexturePos = new Vector2(GameMain.ScreenWidth - mLoadingTexture.Get[(int)mTextureDrawIndex].Width - 50.0f,
				GameMain.ScreenHeight - mLoadingTexture.Get[(int)mTextureDrawIndex].Height - 30.0f);
			mIconCenter = new Vector2(mIconHitode.Width / 2, mIconHitode.Height / 2);
			mIconPos = new Vector2(mTexturePos.X-40.0f,mTexturePos.Y+10.0f);
			mIconPos += mIconCenter;

			mCount = 0;

			mLoadClass = loadClass;

			mLoadingThread = new Thread(LoadingChild);
			mLoadingThread.Start();

			mModelPlayer = new HLModel(mContent, "Model/player_load");
			mPlayerStates = new ModelStates(mModelPlayer.SkinData);
			mPlayerStates.SetAnimation(mModelPlayer, "Take 001", true, 0.0f);
			mPlayerStates.AngleY = (float)Math.PI;

			// カメラを初期化
			mCamera = new Camera();
			mCamera.FieldOfView = MathHelper.ToRadians(60.0f);
			mCamera.AspectRatio = (float)mDevice.Viewport.Width / (float)mDevice.Viewport.Height;
			mCamera.NearPlaneDistance = 1.0f;
			mCamera.FarPlaneDistance = 9000.0f;
			mCamera.ReferenceTranslate = new Vector3(0.0f, 0.0f, 100.0f);
			mCamera.Target = new Vector3(0.0f, 0.0f, 0.0f);
			mCamera.Update();
		}

		/// <summary>
		/// 更新を行う
		/// </summary>
		/// <param name="time">時間</param>
		/// <returns>遷移先のGameChildインスタンス</returns>
		public override GameChild Update(GameTime time) {
			if (!mLoadingThread.IsAlive) {
				mModelPlayer.Release();
				return mLoadingChild;
			}

			Vector3 move = Vector3.Zero;
			float speed = 2.0f;

			Vector2 leftStick = InputManager.GetThumbSticksLeft(PlayerIndex.One);

			//上下左右反転か調べる
			if (Config.IsReverseLeftRight) {
				leftStick.X = -leftStick.X;
			}
			if (Config.IsReverseUpDown) {
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

			if (move.Length() > 0.0f) {
				move = Vector3.Normalize(move) * speed;
			}
			move.Z = -speed;

			mPlayerStates.SetAngleFromDirection(move, 0.2f);

			mPlayerStates.Position += move;

			mPlayerStates.PositionX = Math.Max(-80.0f, Math.Min(80.0f, mPlayerStates.PositionX));
			mPlayerStates.PositionY = Math.Max(-50.0f, Math.Min(50.0f, mPlayerStates.PositionY));

			mCamera.Target = new Vector3(0.0f,0.0f, mPlayerStates.Position.Z);
			mCamera.Update();

			//ロードのアニメーション
			mTextureDrawIndex = mCount % (mTextureNum * 15);
			mTextureDrawIndex /= 15;
			
			//ヒトデ回転
			mHitedeRotete = (float)Math.PI * 2.0f * ((mCount % 100) / 100.0f);
			mCount++;
			return this;
		}

		//描画
		public override void Draw(GameTime time) {
			//画面クリア
			mDevice.Clear(Color.Black);


			mModelPlayer.Render(mCamera, mPlayerStates, time, EffectManager.Type.Wrapped);


			mSpriteBatch.Begin();
			mSpriteBatch.Draw(mIconHitode, mIconPos, null, Color.White, mHitedeRotete, mIconCenter, 1.0f, SpriteEffects.None, 0.0f);
			mSpriteBatch.End();

			//ローディング
			SpriteBoard.Render(mLoadingTexture.Get[(int)mTextureDrawIndex],mTexturePos.X, mTexturePos.Y,
				mLoadingTexture.Get[(int)mTextureDrawIndex].Width, mLoadingTexture.Get[(int)mTextureDrawIndex].Height, 1.0f);
		}

		/// <summary>
		/// コンテンツの削除
		/// </summary>
		public override void UnloadContent() {
			if (mLoadingThread.IsAlive) {
				//mLoadingThread.Interrupt();
			}
		}

		/// <summary>
		/// スレッド用読み込みメソッド
		/// </summary>
		private void LoadingChild() {
			switch (mLoadClass) {
				case LoadClass.Play:
					mLoadingChild = new Play(Play.StageType.Normal);
					break;
				case LoadClass.Tutorial:
					mLoadingChild = new Tutorial();
					break;
			}

		}
	}
}

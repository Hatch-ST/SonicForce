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
	/// ゲーム画面
	/// </summary>
	class Play : GameChild{

		#region フィールド

		/// <summary>カメラ</summary>
		private Camera mCamera = null;
		
		/// <summary>プレイヤー</summary>
		private Player mPlayer = null;

		/// <summary>敵の配列</summary>
		private Enemy[] mEnemies = null;

		/// <summary>敵の最大数</summary>
		private readonly int mMaxNumEnemy = 50;

		/// <summary>静的オブジェクトの配列</summary>
		private StaticObject[] mStaticObjects = null;

		/// <summary>静的オブジェクトの最大数</summary>
		private readonly int mMaxNumStaticObject = 50;

		/// <summary>ステージ</summary>
		private Stage mStage = null;

		/// <summary>オブジェクト管理クラス</summary>
		private ObjectManager mObjectManager = null;
		
		/// <summary>UI</summary>
		private UIManager mUIManager = null;

		/// <summary>ブラーエフェクト</summary>
		private Blur mBlur = null;

		/// <summary>光跡エフェクト</summary>
		private LightTrailL[] mLightTrail = null;
		private LightTrailL[] mTopLightTrail = null;

		/// <summary>頭上の星</summary>
		private HitStar mHitStar = null;

		/// <summary>チェックポイント文字</summary>
		private CheckPointImage mCheckPointImage = null;

		/// <summary>プレイヤーが前回ゾーンモードだったかどうか</summary>
		private bool mLastPlayerSlowEnable = false;
		
		/// <summary>プレイヤーが前回トップモードだったかどうか</summary>
		private bool mLastPlayerFastEnable = false;

		/// <summary>カウンタ</summary>
		private int mCount = 0;

		/// <summary>スクリーンの明るさ</summary>
		private float mScreenBright = 1.0f;
		
		/// <summary>モードエフェクトのアルファ値</summary>
		private float mModeEffectAlpha = 0.0f;

		/// <summary>ゾーンしてる時間</summary>
		private int mPlayerSlowCount = 0;
		
		/// <summary>スコア</summary>
		private Score mScore = null;
		
		/// <summary>プレイヤーの移動</summary>
		private Vector3 mPlayerMove = Vector3.Zero;
		
		/// <summary>リザルト画面用のテクスチャ</summary>
		private RenderTarget2D mResultRenderTarget = null;

		/// <summary>ゲームモード</summary>
		private Mode mMode = Mode.Start;
		
		/// <summary>ステージタイプ</summary>
		private StageType mStageType = StageType.Normal;
		
		/// <summary>ライトブルーム</summary>
		private LightBloom[] mLightBlooms = null;

		/// <summary>レンダリングターゲット</summary>
		private RenderTarget2D mRenderTarget = null;

		// トップ・ゾーン時の明るさ等の補間
		private Interpolate mSlowEffectInter = null;
		private Interpolate mFastEffectInter = null;

		// テクスチャ関係
		private MultiTexture mSlowEffectTextures = null; // ゾーン時のエフェクト
		private MultiTexture mTopEffectTextures = null; // トップ時のエフェクト
		private MultiTexture mCurrentEffectTexture = null; // 現在使用しているエフェクトのテクスチャ

		// ブラックアウト
		private BlackOut mBlackOut = null;

		// 画面系
		private Compliment mCompliment = null;
		private Pause mPause = null;
		private PlayStart mPlayStart = null;
		private BlackWhole mMissTexture = null;
		private PlayClear mPlayClear = null;

		/// <summary>
		/// ゲームの状態
		/// </summary>
		public enum Mode{
			Start,
			Play,
			Continue,
			Clear,
		}

		/// <summary>
		/// ステージの種類
		/// </summary>
		public enum StageType{
			Normal,
			Tutorial,
		}

		#endregion

		#region 生成と解放

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public Play( StageType stageType ){

			// カメラを初期化
			mCamera = new Camera();
			mCamera.FieldOfView = MathHelper.ToRadians( 80.0f );
			mCamera.AspectRatio = ( float )mDevice.Viewport.Width / ( float )mDevice.Viewport.Height;
			mCamera.NearPlaneDistance = 1.0f;
			mCamera.FarPlaneDistance = 9000.0f;
			mCamera.ReferenceTranslate = new Vector3( 0.0f, 80.0f, 300.0f );
			mCamera.Target = new Vector3( 0.0f, 0.0f, 0.0f );
			mCamera.Update();

			// ステージタイプを登録
			mStageType = stageType;

			// ステージの初期化
			switch ( stageType ){
				case StageType.Normal :
					// ステージを初期化
					mStage = new Stage( mContent, StageType.Normal );

					// 敵とオブジェクトを初期化
					mObjectManager = new ObjectManager( mContent );
					mObjectManager.LoadEnemyData( mContent, "enemyData" );
					mObjectManager.LoadObjectData( mContent, "objectData", "collisionModelObjects" );
					break;

				case StageType.Tutorial :
					// ステージを初期化
					mStage = new Stage( mContent, StageType.Tutorial );

					// 敵とオブジェクトを初期化
					mObjectManager = new ObjectManager( mContent );
					mObjectManager.ClearEnemy();
					mObjectManager.ClearObject();
					break;
			}

			// オブジェクトマネージャから敵を取得して登録する
			mEnemies = new Enemy[ Math.Min( mObjectManager.EnemyCount, mMaxNumEnemy ) ];
			for ( int i = 0; i < mEnemies.Length; i++ ){
				mEnemies[ i ] = mObjectManager.GetNextEnemy();
			}

			// オブジェクトマネージャからオブジェクトを取得して登録する
			mStaticObjects = new StaticObject[ Math.Min( mObjectManager.ObjectCount, mMaxNumStaticObject ) ];
			for ( int i = 0; i < mStaticObjects.Length; i++ ){
				mStaticObjects[ i ] = mObjectManager.GetNextObject();
			}

			// プレイヤーを初期化
			mPlayer = new Player( mContent );
			mPlayer.Position = mStage.CameraPosition;

			// ステージのパーティクル位置を登録
			mStage.ParticlePosition = mPlayer.Position;
			
			// UIの初期化
			mUIManager = new UIManager( mContent );

			// ブラーの初期化
			mBlur = new Blur( mDevice, GameMain.ScreenWidth, GameMain.ScreenHeight );

			// 褒め言葉の初期化
			mCompliment = new Compliment( mContent );

			// スコアの初期化
			mScore = new Score( mContent, "Image/score_num/num", 10 );

			// 補間インスタンスの初期化
			mSlowEffectInter = new Interpolate();
			mFastEffectInter = new Interpolate();

			// テクスチャの読み込み
			mSlowEffectTextures = new MultiTexture( mContent, "Image/ZoneEffect", 20 ); // ゾーンエフェクト 
			mTopEffectTextures = new MultiTexture( mContent, "Image/TopEffect/TopEffect", 20 ); // トップエフェクト
			mCurrentEffectTexture = mSlowEffectTextures;

			// 光跡エフェクトを初期化
			string normalLight = "Image/light";
			string topLight = "Image/light_g";
			if ( Config.IsSpecialMode ){
				normalLight = "Image/light_g";
				topLight = "Image/light";
			}
			mLightTrail = new LightTrailL[ 2 ];
			mLightTrail[ 0 ] = new LightTrailL( mDevice, mContent, normalLight, 100, 12.0f, 4.0f );
			mLightTrail[ 1 ] = new LightTrailL( mDevice, mContent, normalLight, 100, 12.0f, 4.0f );

			mTopLightTrail = new LightTrailL[ 2 ];
			mTopLightTrail[ 0 ] = new LightTrailL( mDevice, mContent, topLight, 120, 14.0f, 5.0f );
			mTopLightTrail[ 1 ] = new LightTrailL( mDevice, mContent, topLight, 120, 14.0f, 5.0f );

			// ブラックアウト
			mBlackOut = new BlackOut( mContent, 90, BlackOut.Mode.Close, false );
			mBlackOut.Open();

			// リザルト画面用テクスチャの初期化
			mResultRenderTarget = new RenderTarget2D( mDevice, GameMain.ScreenWidth, GameMain.ScreenHeight );

			// 個々の画面処理インスタンスを生成
			mPause = new Pause( mContent );
			mPlayStart = new PlayStart( mContent, mCamera, new Vector3( 0.0f, 40.0f, 150.0f ) );
			mMissTexture = new BlackWhole( mContent );
			mPlayClear = new PlayClear( mContent );

			// フォグの設定
			Vector4 backColor = GameMain.BackGroundColor.ToVector4();
			if ( Config.IsSpecialMode ){
				backColor = new Vector4( 0, 0, 0, 1 );
			}
			EffectManager.DepthSadow.FogColor = backColor;
			EffectManager.Fog.FogColor = ( Config.IsSpecialMode ? new Vector4( 0, 0, 0, 1 ) : GameMain.BackGroundColor.ToVector4() );
			EffectManager.Fog.SetNearAndFarDepth( 2000.0f, 9000.0f );

			// ライトブルームの初期化
			mLightBlooms = new LightBloom[ 2 ];
			mLightBlooms[ 0 ] = new LightBloom( mDevice, GameMain.ScreenWidth, GameMain.ScreenHeight );
			mLightBlooms[ 1 ] = new LightBloom( mDevice, GameMain.ScreenWidth, GameMain.ScreenHeight );

			// レンダリングターゲットの生成
			mRenderTarget = new RenderTarget2D( mDevice, GameMain.ScreenWidth, GameMain.ScreenHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents );

			mHitStar = new HitStar( mContent );

			mCheckPointImage = new CheckPointImage( mContent );

			// デバッグ解除
			Config.SetDebugMode( false );
		}

		/// <summary>
		/// 解放処理
		/// </summary>
		public void Release(){
			SoundManager.StopMusic();
			InputManager.SetVibration( PlayerIndex.One, 0.0f, 0.0f );
			mPlayer.Release();
			mObjectManager.Release();
			mStage.Release();
		}

		#endregion

		#region 初期化

		/// <summary>
		/// 初期化する
		/// </summary>
		public void Initialize(){
			mPlayClear.Initialize( mPlayerMove );
			mEnemies = new Enemy[ Math.Min( mObjectManager.EnemyCount, mMaxNumEnemy ) ];
			for ( int i = 0; i < mEnemies.Length; i++ ){
				mEnemies[ i ] = mObjectManager.GetNextEnemy();
			}
			mStaticObjects = new StaticObject[ Math.Min( mObjectManager.ObjectCount, mMaxNumStaticObject ) ];
			for ( int i = 0; i < mStaticObjects.Length; i++ ){
				mStaticObjects[ i ] = mObjectManager.GetNextObject();
			}
			mObjectManager.BonusItems.Reset();

			mStage.Initialize();
			mPlayer.Initialize();
			mPlayer.Position =  mStage.CameraPosition;
			mBlackOut.Open();
			mBlackOut.Alpha = 0.0f;

			mBlur.CenterTransparentEnable = true;
			mBlur.Strength = 0.95f;
			mSlowEffectInter.Time = 0xffff;
			mModeEffectAlpha = 0.0f;
			mScreenBright = 1.0f;
			
			mCompliment.Initialize();
			mHitStar.Initialize();
			mCheckPointImage.Initialize();
			
			mLightTrail[ 0 ].Initialize();
			mLightTrail[ 1 ].Initialize();
			mTopLightTrail[ 0 ].Initialize();
			mTopLightTrail[ 1 ].Initialize();

			// フォグ色を初期化
			EffectManager.DepthSadow.FogColor = ( Config.IsSpecialMode ? new Vector4( 0, 0, 0, 1 ) : GameMain.BackGroundColor.ToVector4() );
			
			// デバッグ解除
			Config.SetDebugMode( false );

			mMissTexture.Open();
		}

		/// <summary>
		/// コンティニュー時の初期化処理を行う
		/// </summary>
		public void Continue( GameTime time ){
			mBlackOut.Open();
			mBlackOut.Alpha = 0.0f;

			mBlur.CenterTransparentEnable = true;
			mBlur.Strength = 0.95f;
			mSlowEffectInter.Time = 0xffff;
			mModeEffectAlpha = 0.0f;
			mScreenBright = 1.0f;

			mCompliment.Initialize();
			mHitStar.Initialize();
			mCheckPointImage.Initialize();

			mStage.Continue( mCamera, mPlayer, time );
			mPlayer.Initialize();
			mPlayer.Position = mStage.CameraPosition;

			if ( mStage.TransitedCheckPointCount == 1 ){
				mPlayer.GetPlayScore().Initialize();
			}

			mUIManager.Update( mPlayer );
			
			mObjectManager.ReloadEnemy( mContent );
			mObjectManager.ReloadObject( mContent );
			mObjectManager.MoveToCheckPoint( mStage.TransitedCheckPointCount - 1 );

			if ( mObjectManager.numLeftEnemy < mEnemies.Length ){
				mEnemies = new Enemy[ mObjectManager.numLeftEnemy ];
			}
			for ( int i = 0; i < mEnemies.Length; i++ ){
				mEnemies[ i ] = mObjectManager.GetNextEnemy();
			}
			if ( mObjectManager.numLeftObject < mStaticObjects.Length ){
				mStaticObjects = new StaticObject[ mObjectManager.numLeftObject ];
			}
			for ( int i = 0; i < mStaticObjects.Length; i++ ){
				mStaticObjects[ i ] = mObjectManager.GetNextObject();
			}
			mObjectManager.BonusItems.Reset();
			
			mLightTrail[ 0 ].Initialize();
			mLightTrail[ 1 ].Initialize();
			mTopLightTrail[ 0 ].Initialize();
			mTopLightTrail[ 1 ].Initialize();

			// フォグ色を初期化
			EffectManager.DepthSadow.FogColor = ( Config.IsSpecialMode ? new Vector4( 0, 0, 0, 1 ) : GameMain.BackGroundColor.ToVector4() );

			// デバッグ解除
			Config.SetDebugMode( false );
		}

		#endregion

		#region 更新

		/// <summary>
		/// 更新を行う
		/// </summary>
		/// <returns>遷移先のGameChildインスタンス</returns>
		public override GameChild Update( GameTime time ){
			GameChild next = this;

			switch ( mMode ){
				case Mode.Start :
					mPlayStart.Update( time, mCamera, mPlayer, ( mStageType == StageType.Tutorial ) );

					// 終了
					if ( mPlayStart.Ended ){
						mMode = Mode.Play;
						mStage.CameraPosition = mCamera.Target;
					}
					break;

				case Mode.Play :
					next = UpdateOnPlay( time );

					// クリア
					if ( mStage.Cleared ){
						SoundManager.StopMusic();
						mMode = Mode.Clear;
						mPlayClear.Initialize( mPlayerMove );
					}
					break;

				case Mode.Continue :
					mMissTexture.Open();
					
					mPlayStart.UpdateToContinue( time );

					mCamera.Update( time );

					if ( mPlayStart.Ended ){
						mMode = Mode.Play;
					}

					break;

				case Mode.Clear :
					mPlayClear.Update( time, mCamera, mPlayer );

					if ( mPlayClear.Ended ){
						Release();
						uint bounus = mPlayer.GetPlayScore().GetBonusSamScore();
						uint success = mPlayer.GetPlayScore().GetAvoidSamScore;
						uint combo = mPlayer.GetPlayScore().GetComboSamScore;
						uint ring = mPlayer.GetPlayScore().GetRingSamScore;
						uint scoreTime = mPlayer.GetPlayScore().GetTimeScore();
						uint noAvoid = mPlayer.GetPlayScore().GetNoAvoidScore();
						next = new Result( mResultRenderTarget, bounus, success, combo, ring, scoreTime, noAvoid );
					}
					break;
			}

			// 現在のスコアを登録
			mScore.Set( mPlayer.GetPlayScore().GetNowSamScore );

			// チェックポイントを通過した瞬間ならスコアを保存
			if ( mStage.OnTransitCheckPoint ){
				mPlayer.GetPlayScore().SaveCheckPointScore();
				mCheckPointImage.Reset();
			}

			return next;
		}
		
		/// <summary>
		/// Play時の更新処理
		/// </summary>
		private GameChild UpdateOnPlay( GameTime time ){
			GameChild next = this;

			// ポーズ中じゃない
			if ( !mPlayer.OnPause ){
				// 死んでる
				if ( mPlayer.IsDied ){
					// 頭上の星の更新
					mHitStar.Update();

					mMissTexture.Close();
					InputManager.SetVibration( PlayerIndex.One, 0.0f, 0.0f );
					
					if ( mMissTexture.Closed ){
						Continue( time );
						mMode = Mode.Continue;
					}

					return next;
				}

				// ステージを更新
				mStage.Update( mCamera, mPlayer );

				// カメラ移動がない クジラに食われたとき
				if ( !mStage.CameraMoveEnable ){
					mBlackOut.Close();

					// 黒くなった
					if ( mBlackOut.Closed ){
						// フォグ色を黒にする
						EffectManager.DepthSadow.FogColor = new Vector4( 0.0f, 0.0f, 0.0f, 1.0f );
						mStage.DrawBackModelEnable = false;

						// カメラを１つ進める
						mStage.AdvanceCameraIndex();

						// プレイヤーを初期化
						mPlayer.Initialize();
						mPlayer.Position = mStage.CurrentCameraPosition;

						// カメラ移動を有効化
						mStage.CameraMoveEnable = true;
					}
				}else{
					mBlackOut.Open();
				}

				Vector3 exPlayerPosition = mPlayer.Position;

				// プレイヤーを更新
				mPlayer.Update( mCamera, mEnemies, mStage, mObjectManager.ObjectCollisionModel );

				// ボーナスチェック
				mPlayer.CheckGotBonusItem( mObjectManager.BonusItems, mCamera );

				// プレイヤーの移動量を保存
				mPlayerMove = mPlayer.Position - exPlayerPosition;

				// パーティクルの位置を登録
				mStage.ParticlePosition = mPlayer.Position + new Vector3( 0.0f, -200.0f, -1500.0f );

				// 敵を更新
				for ( int i = 0; i < mEnemies.Length; i++ ){
					// 有効なら更新
					if ( !mEnemies[ i ].Disable ){
						mEnemies[ i ].Update( mCamera, mPlayer, mStage, !mStage.CameraReverseEnable );
					}
					// 無効化されていたら新しい敵を代入
					if ( mEnemies[ i ].Disable ){
					    Enemy nextEnemy = mObjectManager.GetNextEnemy();
					    if ( nextEnemy != null ){
					        mEnemies[ i ] = nextEnemy;
					    }
					}
				}

				// オブジェクトを更新
				for ( int i = 0; i < mStaticObjects.Length; i++ ){
					// 有効なら更新
					if ( !mStaticObjects[ i ].Disable ){
						mStaticObjects[ i ].Update( mCamera, !mStage.CameraReverseEnable );
					}

					// 無効化されていたら新しいオブジェクトを代入
					if ( mStaticObjects[ i ].Disable && !mStage.CameraReverseEnable ){
						StaticObject nextObject = mObjectManager.GetNextObject();
						if ( nextObject != null ){
							mStaticObjects[ i ] = nextObject;
						}
					}
				}

				// 光跡の更新
				UpdateLightTrail();

				// 褒め言葉の更新
				mCompliment.Update( mPlayer );

				// カメラの更新
				mCamera.Update( time );
			
				// スクリーン上のエフェクトの更新
				UpdateScreenEffect();
			
				// UIの更新
				mUIManager.Update( mPlayer );

				// プレイヤーのモード状態を保存
				mLastPlayerSlowEnable = mPlayer.OnSlow();
				mLastPlayerFastEnable = mPlayer.OnFast();
				
				// 適当に振動
				if ( Config.IsVibrationOn ){
					if ( mPlayer.OnSlow() ){
						if ( mPlayerSlowCount < 30 ){
							InputManager.SetVibration( PlayerIndex.One, 0.2f, 0.2f );
						}else{
							InputManager.SetVibration( PlayerIndex.One, 0.0f, 0.0f );
						}
						if ( mPlayer.OnStop() ){
							InputManager.SetVibration( PlayerIndex.One, 1.0f, 1.0f );
						}

						++mPlayerSlowCount;
					}else if ( mPlayer.OnFast() ){
						InputManager.SetVibration( PlayerIndex.One, 0.5f, 0.5f );
						mPlayerSlowCount = 0;
					}else{
						InputManager.SetVibration( PlayerIndex.One, 0.0f, 0.0f );
						mPlayerSlowCount = 0;
					}
				}

				mCheckPointImage.Update();

				++mCount;

			}
			// ポーズ中
			else{
				// ポーズ画面の更新
				if ( mPause.Update( mPlayer ) ){
					Release();
					next = new Title();
				}
			}

			return next;
		} 

		/// <summary>
		/// スクリーンのエフェクトの更新を行う
		/// </summary>
		private void UpdateScreenEffect(){
			// 低速になった瞬間なら補間をリセット
			if ( mLastPlayerSlowEnable != mPlayer.OnSlow() ){
				mSlowEffectInter.Reset();
			}
			// 高速になった瞬間なら補間をリセット
			if ( mLastPlayerFastEnable != mPlayer.OnFast() ){
				mFastEffectInter.Reset();
			}

			// プレイヤーの状況によってブラーの強さを変える
			// ゾーンモード
			if ( mPlayer.OnSlow() ){
				mBlur.CenterTransparentEnable = false;
				mBlur.Strength = 0.8f;

				// ゾーンエフェクトのアルファ値を増やす
				mModeEffectAlpha = mSlowEffectInter.GetSin( 0.0f, 1.0f, 60 );

				// 明るさは暗くする
				mScreenBright = 1.9f - mModeEffectAlpha;

				// テクスチャをゾーンモードにする
				mCurrentEffectTexture = mSlowEffectTextures;
			}
			// トップモード
			else if ( mPlayer.OnFast() ){
				mBlur.CenterTransparentEnable = false;
				mBlur.Strength = 0.9f;
				
				// ゾーンエフェクトのアルファ値を増やす
				mModeEffectAlpha = mSlowEffectInter.GetSin( 0.0f, 1.0f, 60 );

				// 明るくする
				mScreenBright = mFastEffectInter.GetSin( 1.8f, 1.0f, 60 );
				
				// テクスチャをトップモードにする
				mCurrentEffectTexture = mTopEffectTextures;
			}
			// 通常モード
			else{
				mBlur.CenterTransparentEnable = true;
				mBlur.Strength = 0.95f;

				// ゾーンエフェクトのアルファ値を減らす
				if ( mModeEffectAlpha != 0.0f ){
					mModeEffectAlpha = mSlowEffectInter.GetSinReverse( 60 );
				}

				mScreenBright = 1.0f;
			}
		}

		/// <summary>
		/// 光跡の更新を行う
		/// </summary>
		private void UpdateLightTrail(){
			bool inputed = false;

			if ( mPlayer.OnFast() ){
				inputed = true;
			}

			if ( InputManager.IsKeyDown( Keys.Up ) || InputManager.IsKeyDown( Keys.Down ) || InputManager.IsKeyDown( Keys.Left ) || InputManager.IsKeyDown( Keys.Right ) ||
				 InputManager.IsKeyDown( Keys.A ) || InputManager.IsKeyDown( Keys.S ) ){
				inputed = true;
			}

			if ( InputManager.GetThumbSticksLeft( PlayerIndex.One ).LengthSquared() != 0.0f ||
				 InputManager.IsButtonDown( PlayerIndex.One, Buttons.LeftTrigger ) || InputManager.IsButtonDown( PlayerIndex.One, Buttons.RightTrigger ) ){
				inputed = true;
			}

			mLightTrail[ 0 ].Enable = inputed;
			mLightTrail[ 1 ].Enable = inputed;

			Vector3 playerLeft = mPlayer.GetWingPos( Player.WingPosNum.left ) - mPlayer.Position;
			Vector3 playerRight = mPlayer.GetWingPos( Player.WingPosNum.right ) - mPlayer.Position;
			mLightTrail[ 0 ].Position = mPlayer.Position + playerLeft * 1.8f;
			mLightTrail[ 1 ].Position = mPlayer.Position + playerRight * 1.8f;
			mLightTrail[ 0 ].Update();
			mLightTrail[ 1 ].Update();

			mTopLightTrail[ 0 ].Position = mPlayer.Position + playerLeft * 1.4f;
			mTopLightTrail[ 1 ].Position = mPlayer.Position + playerRight * 1.4f;
			if ( mPlayer.OnFast() ){
				mTopLightTrail[ 0 ].Update();
				mTopLightTrail[ 1 ].Update();
			}
		}

		#endregion

		#region 描画

		/// <summary>
		/// 描画を行う
		/// </summary>
		public override void Draw( GameTime time ){
			// ポーズ中じゃない
			if ( !mPlayer.OnPause ){
				// 光源を算出
				Vector3 lightPos = mPlayer.Position + new Vector3( 0.0f, 500.0f, 0.0f ) + mCamera.ReferenceTranslate;
				Vector3 lightTar = mPlayer.Position - mCamera.ReferenceTranslate;


				// 深度マップへの書き込みを開始
				EffectManager.DepthMap.Begin( lightPos, lightTar );
		
				mPlayer.Draw( mCamera, time, EffectManager.Type.DepthMap );
				for ( int i = 0; i < mEnemies.Length; i++ ){
					mEnemies[ i ].DrawToShadowMap( time, mCamera );
				}
				for ( int i = 0; i < mStaticObjects.Length; i++ ){
				    mStaticObjects[ i ].Draw( mCamera, time, mPlayer, EffectManager.Type.DepthMap );
				}
			
				// 深度マップへの書き込みを終了
				EffectManager.DepthMap.End();
				

				// ブラーを開始
				mBlur.Begin( mRenderTarget, true, EffectManager.DepthSadow.FogColor );

				// モデルを描画
				mStage.Draw( mCamera, time, EffectManager.Type.DepthShadow );
				for ( int i = 0; i < mEnemies.Length; i++ ){
					mEnemies[ i ].Draw( time, mCamera, EffectManager.Type.Wrapped );
				}
				for ( int i = 0; i < mStaticObjects.Length; i++ ){
				    mStaticObjects[ i ].Draw( mCamera, time, mPlayer, EffectManager.Type.Fog );
				}
				mPlayer.Draw( mCamera, time, EffectManager.Type.Wrapped );
				mPlayer.DrawLing( mCamera, time, false, true, true );

				mHitStar.Render( mPlayer.Position + Vector3.Up * 40.0f );

				// 半透明モデルを描画
				TransparentModelManager.RenderAll( mCamera, time );
			
				// パーティクルを描画
				mStage.DrawParticle( mDevice, mCamera );
				for ( int i = 0; i < mEnemies.Length; i++ ){
					mEnemies[ i ].DrawParticle( mDevice, mCamera );
				}

				// 光跡を描画
				if ( mPlayer.OnFast() ){
					mTopLightTrail[ 0 ].Render( mDevice, mCamera );
					mTopLightTrail[ 1 ].Render( mDevice, mCamera );
				}else{
					mLightTrail[ 0 ].Render( mDevice, mCamera );
					mLightTrail[ 1 ].Render( mDevice, mCamera );
				}
				
				// ブラーを終了
				mBlur.End();
					

				// リザルト用に画面を保存しておく
				if ( mCount == 600 ){
					mDevice.SetRenderTarget( mResultRenderTarget );
					mDevice.Clear( Color.White );
					SpriteBoard.Render( mBlur.Texture, 0, 0, GameMain.ScreenWidth, GameMain.ScreenHeight, 1.0f, mScreenBright );
					mDevice.SetRenderTarget( null );
				}

				for ( int i = 0; i < 2; i++ ){
					// ライトブルーム開始
					mLightBlooms[ i ].Begin( mRenderTarget, false );

					// 判定リングを描画
					if ( i == 0 ){
						mPlayer.DrawLing( mCamera, time, true, true, false );
					}else{
						mPlayer.DrawLing( mCamera, time, true, false, true );
					}

					// ライトブルーム終了
					mLightBlooms[ i ].End( 3 );
				}


				// 画面クリア
				mDevice.Clear( Color.White );

				// ブラーのテクスチャを描画
				SpriteBoard.Render( mBlur.Texture, 0, 0, GameMain.ScreenWidth, GameMain.ScreenHeight, 1.0f, mScreenBright );
				
				// ライトブルームのテクスチャを描画
				mLightBlooms[ 0 ].Render( 0, 0, new Vector4( 2.0f, 1.8f, 0.6f, 2.0f ) );
				mLightBlooms[ 1 ].Render( 0, 0, new Vector4( 2.0f, 0.8f, 0.6f, 2.0f ) );

				// モードエフェクトのテクスチャを描画
				if ( mModeEffectAlpha > 0.0f ){
					float modeEffectBright = ( mCurrentEffectTexture == mSlowEffectTextures ) ? 1.0f : 1.4f;
					SpriteBoard.Render( mCurrentEffectTexture.Get[ mCount % 120 / 6 ], 0, 0, GameMain.ScreenWidth, GameMain.ScreenHeight, mModeEffectAlpha, modeEffectBright );
				}

				// プレイヤーでのスプライトを描画
				mPlayer.DrawSprite( mCamera );

				// UIの描画
				mUIManager.Draw( time, mSpriteBatch );

				// スコアの描画
				mScore.Draw( 1100, 666 );

				// 褒め言葉を描画
				mCompliment.Draw( mCamera, mPlayer );

				//スコアインフォメーションを描画
				mPlayer.DrawScoreInfomation();

				// チェックポイント文字
				mCheckPointImage.Draw( mCamera, mPlayer.Position + Vector3.Up * 60.0f );
				
				//mSpriteBatch.Begin();
				//float y = 20.0f;
				//DrawText( "FPS : " + 1.0f / time.ElapsedGameTime.TotalSeconds, new Vector2( 0.0f, y * 0 ), Color.White );
				//DrawText( "Time : " + mPlayer.GetPlayScore().GetPlayTime, new Vector2( 0.0f, y * 1 ), Color.White );
				//mSpriteBatch.End();

				// ブラックアウト
				mBlackOut.Draw();
			}
			// ポーズ中
			else{
				// 前回までのブラーのテクスチャを描画
				SpriteBoard.Render( mBlur.Texture, 0, 0, GameMain.ScreenWidth, GameMain.ScreenHeight, 1.0f, 0.5f );

				// モードエフェクトのテクスチャを描画
				if ( mModeEffectAlpha > 0.0f ){
					SpriteBoard.Render( mCurrentEffectTexture.Get[ mCount % 120 / 6 ], 0, 0, GameMain.ScreenWidth, GameMain.ScreenHeight, mModeEffectAlpha, 0.5f );
				}
				
				// UIの描画
				// mUIManager.Draw( time, mSpriteBatch );

				// ポーズ画面の描画
				mPause.Draw( time, mSpriteBatch, mPlayer ); 
			}
			
			if ( mStageType != StageType.Tutorial ){
				mPlayStart.Draw( time );
				mPlayClear.Draw( mCamera, mPlayer, time );
			}

			// 失敗テクスチャ
			mMissTexture.Draw( mCamera, mPlayer );
		}

		/// <summary>
		/// UIだけを描画する
		/// </summary>
		public void DrawUI( GameTime time, bool enableTopGauge, bool enableZoneGauge ){
			if ( !mPlayer.OnPause ){
				mUIManager.Draw( time, mSpriteBatch, enableTopGauge, enableZoneGauge );
			}else{
			}
		}

		#endregion
		
		#region プロパティ

		/// <summary>
		/// カメラを取得する
		/// </summary>
		public Camera Camera{
			get { return mCamera; }
		}

		/// <summary>
		/// プレイヤーを取得する
		/// </summary>
		public Player Player{
			get { return mPlayer; }
		}
		
		/// <summary>
		/// ステージを取得する
		/// </summary>
		public Stage Stage{
			get { return mStage; }
		}
		
		/// <summary>
		/// オブジェクトマネージャーを取得する
		/// </summary>
		public ObjectManager ObjectManager{
			get { return mObjectManager; }
		}

		#endregion

	}
}
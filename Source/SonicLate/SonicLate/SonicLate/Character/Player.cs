using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace SonicLate {
	/// <summary>
	/// プレイヤー
	/// </summary>
	class Player {
		#region フィールド

		#region enum関係
		/// <summary>
		/// モードの名称
		/// </summary>
		public enum ModeName {
			normal = 0,
			zone = 1,
			topGear = 2
		}

		/// <summary>
		/// 羽のボーン番号
		/// </summary>
		public enum WingPosNum {
			right = 17,
			left = 22
		}

		/// <summary>
		/// ギリ避け時の上昇レベル
		/// </summary>
		public enum AvoidLevel {
			Nothing = 0,
			Outside = 1,
			Inside = 2
		}
		#endregion

		#region メイン変数関係
		/// <summary>
		/// モデルインスタンス
		/// </summary>
		private HLModel mModel = null;

		/// <summary>
		/// モデルの状態
		/// </summary>
		private ModelStates mStates = null;

		private HLModel mModelTopEffect = null;
		private ModelStates mTopEffectStates = null;

		private MultiTexture mDeadTextures = null;
		private int mHitPositionID = -1;

		/// <summary>
		///  ギリ避け判定リング
		/// </summary>
		private HLModel mModSwayLing = null;

		/// <summary>
		/// モードの状態
		/// </summary>
		private int mMode = (int)ModeName.normal;

		/// <summary>
		/// 進行時間
		/// </summary>
		private ulong mTimeCount = 0;

		private ulong mTimeCountToLing = 0;

		/// <summary>
		/// 死亡フラグ
		/// </summary>
		private bool mDied = false;

		/// <summary>
		/// ポーズ中かどうか
		/// </summary>
		private bool mOnPause = false;

		/// <summary>
		/// ポーズ解除後の時間
		/// </summary>
		private int mAfterPauseCount = 0;

		/// <summary>
		/// ポーズ解除後にまたポーズできるようになるまでの時間
		/// </summary>
		private const float mAfterPauseTime = 0.5f * 60;


		/// <summary>
		/// ギリ避けスロー時の補間
		/// </summary>
		private Interpolate mAdvanceInter;


		/// <summary>
		/// トップ時のプレイヤーテクスチャ
		/// </summary>
		private Texture2D mTopTexture;

		/// <summary>
		/// 傷だらけのプレイヤーテクスチャ
		/// </summary>
		private Texture2D mInjureTexture;
		
		#endregion

		#region 移動処理関係
		/// <summary>
		/// スクロールスピード
		/// </summary>
		private const float mScrollSpeed = -20.0f;

		/// <summary>
		/// ゾーンモードのスクロールスピード倍率
		/// </summary>
		private const float mZoneScrollSpeedRatio = 0.5f;

		/// <summary>
		/// トップギアモードのスクロールスピード倍率
		/// </summary>
		private const float mTopGearScrollSpeedRatio = 2.0f;

		/// <summary>
		/// スロー時のスクロールスピード倍率
		/// </summary>
		private const float mStopScrollSpeedRatio = 0.05f;

		/// <summary>
		/// ギリ避け時のスクロールスピード倍率
		/// </summary>
		private const float mAdvanceScrollSpeedRatio = 1.25f;

		/// <summary>
		/// 左右の移動スピードの最大
		/// </summary>
		private const float mSpeedMax_X = 20.0f;

		/// <summary>
		/// 左右の移動スピードの加算値
		/// </summary>
		private const float mAcceleration_X = 2.0f;

		/// <summary>
		/// 上下の移動スピードの最大
		/// </summary>
		private const float mSpeedMax_Y = 14.0f;

		/// <summary>
		/// 操作前の移動ベクトル
		/// </summary>
		private Vector3 mMoveVecBefore;

		/// <summary>
		/// 慣性の補間クラス
		/// </summary>
		private Interpolate3 mMoveVecInter;

		/// <summary>
		/// 慣性が働く時間
		/// </summary>
		private const float mInertiaTime = 0.1f * 60;

		/// <summary>
		/// 慣性が働く残りフレーム
		/// </summary>
		private int mInertiaRestFrame = 0;
		#endregion

		#region ゲージ総合関係
		/// <summary>
		/// ゲージの最小(単位:%)
		/// </summary>
		private const float mGaugeMin = 0.0f;

		/// <summary>
		/// ゲージの最大(単位:%)
		/// </summary>
		private const float mGaugeMax = 100.0f;
		#endregion

		#region ゾーンゲージ関係
		/// <summary>
		/// ゾーンゲージの量(単位:%)
		/// </summary>
		private float mZoneGauge = mGaugeMax;

		/// <summary>
		/// 消費されたゾーンゲージの量(単位:%)
		/// </summary>
		private float mUsedZoneGauge = 0.0f;

		/// <summary>
		/// ゾーンゲージの1回の消費量する時間(単位:%)
		/// </summary>
		private const float mUsedZoneMin = 10.0f;

		/// <summary>
		/// ゾーンゲージが最大から全部消費するまでの時間(単位:秒)
		/// </summary>
		private const float mZoneSpendSecond = 8.0f;

		/// <summary>
		/// １フレーム毎のゾーンゲージ使用時の消費量(単位:%)
		/// </summary>
		private const float mZoneSpendParFrame = mGaugeMax / mZoneSpendSecond / 60;

		/// <summary>
		/// ゾーンゲージが0から全部回復するまでの時間(単位:秒)
		/// </summary>
		private const float mZoneRecoverSecond = 16.0f;

		/// <summary>
		/// １フレーム毎のゾーンゲージの回復量(単位:%)
		/// </summary>
		private const float mZoneRecoverParFrame = mGaugeMax / mZoneRecoverSecond / 60;
		#endregion

		#region コンボ関係
		/// <summary>
		/// コンボした回数
		/// </summary>
		private int mComboCount = 0;

		/// <summary>
		/// 最大コンボ回数
		/// </summary>
		private int mMaxCombo = 0;
		#endregion

		#region トップギアゲージ関係
		/// <summary>
		/// トップギアゲージの量
		/// </summary>
		private float mTopGearGauge = mGaugeMin;

		/// <summary>
		/// １フレーム毎のトップギアゲージ使用時の消費量
		/// </summary>
		private readonly float mTopGearSpendParFrame;

		/// <summary>
		/// トップギア発動までのコンボ回数
		/// </summary>
		private const int mTopGearMinCount = 3;

		/// <summary>
		/// トップギアゲージ最大までのコンボ回数
		/// </summary>
		private const int mTopGearMaxCount = 12;

		/// <summary>
		/// トップギアゲージの各コンボ数で回復する時間
		/// </summary>
		private readonly float[] mTopGearRecoverSecond = { 1.0f, 1.5f, 2.0f, 2.5f, 3.0f, 3.5f, 4.0f, 5.0f, 6.0f, 7.0f };

		/// <summary>
		/// トップギアゲージの各コンボ数で回復する割合(加算ではなく代入用)
		/// </summary>
		public readonly float[] mTopGearRecoverRatio = new float[mTopGearMaxCount - mTopGearMinCount + 1];

		/// <summary>
		/// トップギア後の無敵時間(秒 * 60フレーム)
		/// </summary>
		private const float mMatchlessSecondAfterTopGear = 2.0f * 60;

		/// <summary>
		/// トップギア後の残り無敵時間
		/// </summary>
		private int mRestMatchlessTime = 0;

		/// <summary>
		/// トップギアになった瞬間かどうか
		/// </summary>
		private bool mJustTopGearChanged;

		/// <summary>
		/// トップギアゲージが上昇するレベル
		/// </summary>
		private int mTopLevel = 0;

		#endregion

		#region 当たり判定関係
		/// <summary>
		/// 当たり判定の半径
		/// </summary>
		public readonly float mCollisionRange = 10.0f;

		/// <summary>
		/// ギリ避け判定の半径(外)
		/// </summary>
		public readonly float mAvoidRangeOutside;

		/// <summary>
		/// ギリ避け判定の半径(内)
		/// </summary>
		public readonly float mAvoidRangeInside;

		/// <summary>
		/// ギリ避けした瞬間かどうか
		/// </summary>
		private bool mJustAvoid = false;

		/// <summary>
		/// ギリ避けした瞬間のギリ避けレベル
		/// </summary>
		private AvoidLevel mJustAvoidLevel = AvoidLevel.Nothing;

		/// <summary>
		/// ギリ避け時のスロー時間(秒*60)
		/// </summary>
		private const float mStopSecond = 0.50f * 60;

		/// <summary>
		/// ギリ避け時の残りスロー時間
		/// </summary>
		private int mStopTime = 0;

		/// <summary>
		/// スロー後の加速時間(秒*60)
		/// </summary>
		private const float mAdvanceSecond = 0.5f * 60;

		/// <summary>
		/// スロー後の残り加速時間
		/// </summary>
		private int mAdvanceTime = 0;

		#endregion

		#region カメラ関係
		/// <summary>注視点からの距離の最大値 X</summary>
		float mMaxCameraDistanceX = 300.0f;

		/// <summary>注視点からの距離の最大値 Y</summary>
		float mMaxCameraDistanceY = 200.0f;

		/// <summary>ビュー座標系での注視点</summary>
		Vector3 mCameraTargetOnView = Vector3.Zero;

		/// <summary>前回のカメラ角度</summary>
		Vector3 mExCameraAngle = Vector3.Zero;
		#endregion

		#region ローリング関係
		/// <summary>
		/// ローリング猶予時間(単位:フレーム)
		/// </summary>
		private int mRollingDelayFrame = 20;

		/// <summary>
		/// 傾きボタンを押してからの時間(単位:フレーム)
		/// </summary>
		private int mPassedFrame = 0;

		/// <summary>
		/// 猶予時間内に左傾きボタンを押した回数
		/// </summary>
		private int mPushedRollButtonNum_R = 0;
		/// <summary>
		/// 猶予時間内に右傾きボタンを押した回数
		/// </summary>
		private int mPushedRollButtonNum_L = 0;

		/// <summary>
		/// ローリングしているか
		/// </summary>
		private bool isRolling = false;

		#endregion

		#region スコア関係
		/// <summary>
		/// プレイスコア
		/// </summary>
		private PlayScore mPlayScore;

		/// <summary>スコア表示クラス</summary>
		private ScoreInfomation[] mScoreInfomations = null;


		#endregion

		// 向き
		private Vector3 mAngle = Vector3.Zero;

		// カメラとの距離の最大数
		private float mMaxPlayerToCameraDistanceX = 120.0f;
		private float mMaxPlayerToCameraDistanceY = 100.0f;

		#endregion

		#region プロパティ,geter,seter
		/// <summary>
		/// 現在のモード
		/// </summary>
		public int Mode {
			get { return mMode; }
		}

		/// <summary>
		/// プレイヤーがやられているかどうか
		/// </summary>
		public bool IsDied {
			get { return mDied; }
		}

		/// <summary>
		/// ポーズ中かどうか
		/// </summary>
		public bool OnPause {
			get { return mOnPause; }
		}

		/// <summary>
		/// ポーズを解除する
		/// </summary>
		public void CancelPause() {
			mOnPause = false;
			mAfterPauseCount = (int)mAfterPauseTime;
		}
		
		/// <summary>
		/// 現在のコンボ回数
		/// </summary>
		public int ComboCount {
			get { return mComboCount; }
		}

		/// <summary>
		/// 現在のトップギアゲージの上昇レベル
		/// </summary>
		public int TopLevel {
			get { return mTopLevel; }
		}

		/// <summary>
		/// 現在のトップギアゲージ
		/// </summary>
		public float TopGearGauge {
			get { return mTopGearGauge; }
		}

		/// <summary>
		/// 現在のゾーンゲージ
		/// </summary>
		public float ZoneGauge {
			get { return mZoneGauge; }
		}

		/// <summary>
		/// ギリ避けした瞬間かどうか
		/// </summary>
		public bool JustAvoid {
			get { return mJustAvoid; }
		}

		/// <summary>
		/// ギリ避けした瞬間のギリ避けレベル
		/// </summary>
		public AvoidLevel JustAvoidLevel {
			get { return mJustAvoidLevel; }
		}

		/// <summary>
		/// トップギアになった瞬間かどうか
		/// </summary>
		public bool JustTopGearChanged {
			get { return mJustTopGearChanged; }
		}

		/// <summary>
		/// 位置
		/// </summary>
		public Vector3 Position {
			get { return mStates.Position; }
			set { mStates.Position = value; }
		}

		/// <summary>
		/// ゾーンモードであるか
		/// </summary>
		/// <returns>ゾーンモードならtrueを返す</returns>
		public bool OnSlow() {
			return mMode == (int)ModeName.zone;
		}

		/// <summary>
		/// トップギアであるか
		/// </summary>
		/// <returns>トップギアならtrueを返す</returns>
		public bool OnFast() {
			return mMode == (int)ModeName.topGear;
		}

		/// <summary>
		/// スローであるか
		/// </summary>
		/// <returns>スローならtrueを返す</returns>
		public bool OnStop() {
			return (mStopTime > 0);
		}

		/// <summary>
		/// 加速状態であるか
		/// </summary>
		/// <returns>加速状態ならtrueを返す</returns>
		public bool OnAdvance() {
			return (mAdvanceTime > 0);
		}

		/// <summary>
		/// 羽の位置を取得する
		/// </summary>
		/// <param name="num">22で左,17で右を取得</param>
		/// <returns>羽の位置</returns>
		public Vector3 GetWingPos(WingPosNum num) {
			Vector3 wingPos = mStates.AnimPlayer.GetBonePosition((int)num);
			//wingPos += (wingPos - mStates.Position) * 0.8f;
			return wingPos;
		}

		/// <summary>
		/// プレイ時間を取得する
		/// </summary>
		public ulong GetPlayTime {
			get { return mTimeCount; }
		}

		/// <summary>
		/// ゾーン中のスクロールスピード倍率を取得する
		/// </summary>
		public float ZoneScrollSpeedRatio{
			get { return mZoneScrollSpeedRatio; }
		}

		#endregion

		#region コンストラクタ
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="content">コンテンツマネージャ</param>
		public Player(ContentManager content) {
			mModel = new HLModel(content, "Model/player");
			mStates = new ModelStates(mModel.SkinData);
			mStates.SetAnimation(mModel, "Take 001", true, 0.0f);
			mStates.AngleY = (float)Math.PI;
			mStates.Scale *= 3.0f;

			mModelTopEffect = new HLModel( content, "Model/topeffect" );
			mTopEffectStates = new ModelStates( null );
			mTopEffectStates.Scale *= 3.0f;

			mDeadTextures = new MultiTexture( content, "Image/hit", 5 );

			// 判定リング
			mModSwayLing = new HLModel(content, "Model/ling_s");

			//テクスチャ
			mTopTexture = content.Load<Texture2D>("Image/main_Player_top");
			mInjureTexture = content.Load<Texture2D>("Image/main_Player_injure");

			mTopGearSpendParFrame = mGaugeMax / mTopGearRecoverSecond[mTopGearRecoverSecond.Length - 1] / 60;

			//定数の初期化
			for (int i = 0; i < mTopGearRecoverSecond.Length; i++) {
				mTopGearRecoverRatio[i] = mGaugeMax * mTopGearRecoverSecond[i] / mTopGearRecoverSecond[mTopGearRecoverSecond.Length - 1];
			}

			//ギリ避け範囲設定
			mAvoidRangeOutside = mModel.Scale.X * mStates.Scale.X * 0.8f;
			mAvoidRangeInside = mModel.Scale.X * mStates.Scale.X * 0.5f;

			mAdvanceInter = new Interpolate();
			mMoveVecInter = new Interpolate3();
			mPlayScore = new PlayScore();

			// スコアインフォメーションの初期化
			mScoreInfomations = new ScoreInfomation[10];
			for (int i = 0; i < mScoreInfomations.Length; i++) {
				mScoreInfomations[i] = new ScoreInfomation(content);
			}

			//裏モードのときはテクスチャを張り替えておく
			if (Config.IsSpecialMode) {
				mModel.SetTexture(mTopTexture);
			}

		}
		#endregion

		#region 初期化メソッド
		/// <summary>
		/// 初期化する
		/// </summary>
		public void Initialize() {
			if ( mDied ){
				mPlayScore.LoadCheckPointScore();
			}

			mCameraTargetOnView = Vector3.Zero;
			mStates.Position = Vector3.Zero;
			mStates.AngleZ = 0.0f;
			mDied = false;
			mZoneGauge = mGaugeMax;
			mTopGearGauge = mGaugeMin;
			mComboCount = 0;
			mMode = (int)ModeName.normal;
			mJustTopGearChanged = false;
			mTopLevel = 0;
			mMoveVecBefore = Vector3.Zero;
			mRestMatchlessTime = 0;
			mInertiaRestFrame = 0;
			mStopTime = 0;
			mAdvanceTime = 0;
			mTimeCountToLing = 0;

			if (Config.IsSpecialMode) {
				mModel.SetTexture(mTopTexture);
			}else{
				mModel.SetTexture( null );
			}
			
			mStates.SetAngleFromDirection( new Vector3( 0.0f, 0.0f, -1.0f ) );
			mAngle = Vector3.Zero;
			mStates.SetAnimation( mModel, "Take 001", true, 0.0f );
			InitScore();

			mHitPositionID = -1;
			mDeadTextures.resetAnimation();
		}

		/// <summary>
		/// 瞬間の状態変数と初期化する
		/// </summary>
		private void InitJustState() {
			mJustAvoid = false;
			mJustTopGearChanged = false;
		}
		#endregion

		#region 更新処理メソッド
		/// <summary>
		/// 更新を行う
		/// </summary>
		/// <param name="camera">カメラ</param>
		/// <param name="enemies">エネミー</param>
		/// <param name="stageModel">ステージモデル</param>
		/// <param name="stageStates">ステージの状態</param>
		public void Update( Camera camera, Enemy[] enemies, Stage stage, CollisionModel objectCollision) {
			//ポーズ中かチェック
			if (OnPause) {
				return;
			}

			//瞬間の変数を初期化
			InitJustState();

			Vector3 moveVec = Vector3.Zero;

			//スクロールによる移動
			moveVec.Z += mScrollSpeed;

			//スクロールの向きにプレイヤーの向きを合わせる
			mStates.SetAngleFromDirection(GetPlayerAngle( moveVec,stage));

			if ( !InputManager.IsConnected( PlayerIndex.One ) ){
				//キーボードによる移動
				moveVec += UpdateByKeyBoard(stage);
			}else{
				//コントローラーによる移動
				moveVec += UpdateByController(stage);
			}

			//傾きの操作
			ControllRoll();

			//モードによる移動ベクトルの倍率をかける
			moveVec *= GetModeSpeedRatio();

			//デバッグ用コンボ増加
			if (Config.IsDebugMode && (InputManager.IsJustKeyDown(Keys.C) && mMode == (int)ModeName.zone) ) {
				++mComboCount;
				++mTopLevel;
			}

			//モードによる処理
			UpdateByMode(enemies, moveVec + stage.CameraMove,camera);

			// スコアインフォメーションの更新
			for (int i = 0; i < mScoreInfomations.Length; i++) {
				mScoreInfomations[i].Update();
			}

			// ビュー座標系においてXY軸の移動があればビュー座標系での注視点の補正値に加算
			Vector3 nowCameraTargetOnView = Vector3.Zero;
			if (moveVec.X != 0.0f) {
				nowCameraTargetOnView.X = moveVec.X * 0.7f;
			}
			if (moveVec.Y != 0.0f) {
				nowCameraTargetOnView.Y = moveVec.Y * 0.7f;
			}

			// カメラの上下左右移動分を少しだけ加算
			float rate = ( mMode == ( int )ModeName.topGear ) ? 1.0f : 0.5f;
			Matrix revRota = Matrix.CreateRotationX(-stage.CameraAngle.X) * Matrix.CreateRotationY(-stage.CameraAngle.Y);
			Vector3 revCameraMove = Vector3.Transform( stage.CameraMove, revRota ) * new Vector3( rate, rate, 0.0f );
			moveVec += revCameraMove;


			// ワールド座標系への変換行列
			Matrix rotaY = Matrix.CreateRotationY(stage.CameraAngle.Y);
			Matrix rotaX = Matrix.CreateRotationX(stage.CameraAngle.X);
			Matrix rota = rotaX * rotaY;

			// 移動ベクトルをワールド座標系に変換
			moveVec = Vector3.Transform(moveVec, rota);

			//壁反発
			Vector3 moveVecAfterHit = CheckTopGearHitStage(stage, moveVec);
			//moveVecAfterHit = moveVec;
			bool moveCameraX = false, moveCameraY = false;

			// 壁に当たっていない
			if (moveVecAfterHit == moveVec){
				moveCameraX = moveCameraY = true;
			}
			
			moveVec = moveVecAfterHit;

			mStates.Position += moveVec;

			// 向きを登録
			if ( stage.CameraMove.Length() != 0.0f ){
				mStates.SetAngleFromDirection( stage.CameraMove );
				mStates.Angle += mAngle;
			}


			// カメラからの距離を制限する
			Vector3 cameraTarget = camera.Target + Vector3.Transform( mCameraTargetOnView, rota ); // 注視点
			Vector3 cameraTargetToPlayer = mStates.Position - cameraTarget; // 注視点からプレイヤー
			Vector3 view = cameraTarget - camera.Position; // 視線ベクトル
			Vector3 cameraAxisZ = Vector3.Normalize( view ); // 視線のZ軸
			Vector3 cameraAxisX = Vector3.Normalize( Vector3.Cross( cameraAxisZ, Vector3.Up ) ); // 視線のY軸
			Vector3 cameraAxisY = Vector3.Normalize( -Vector3.Cross( cameraAxisZ, cameraAxisX ) ); // 視線のX軸
			
			// 視野の中央 視線のZ軸上のプレイヤー位置
			Vector3 cameraCenter = cameraTarget + cameraAxisZ * Vector3.Dot( cameraAxisZ, cameraTargetToPlayer );

			Vector3 centerToPlayer = mStates.Position - cameraCenter; // 中央からプレイヤー
			float lengthX = Vector3.Dot( cameraAxisX, centerToPlayer ); // プレイヤーとの距離X
			float lengthY = Vector3.Dot( cameraAxisY, centerToPlayer ); // プレイヤーとの距離Y
			// 距離を制限
			if ( Vector3.Dot( cameraAxisX, centerToPlayer ) > mMaxPlayerToCameraDistanceX ){
			    lengthX = mMaxPlayerToCameraDistanceX;
			}else if ( Vector3.Dot( cameraAxisX, centerToPlayer ) < -mMaxPlayerToCameraDistanceX ){
				lengthX = -mMaxPlayerToCameraDistanceX;
			}
			if ( Vector3.Dot( cameraAxisY, centerToPlayer ) > mMaxPlayerToCameraDistanceY ){
			    lengthY = mMaxPlayerToCameraDistanceY;
			}else if ( Vector3.Dot( cameraAxisY, centerToPlayer ) < -mMaxPlayerToCameraDistanceY ){
			    lengthY = -mMaxPlayerToCameraDistanceY;
			}

			// プレイヤーの位置を新たに登録
			mStates.Position = cameraCenter;
			mStates.Position += ( cameraAxisX * lengthX );
			mStates.Position += ( cameraAxisY * lengthY );
			mStates.Position += ( cameraAxisZ * Vector3.Dot( cameraAxisZ, centerToPlayer ) );

			//通常、ゾーン時の衝突
			if (mMode != (int)ModeName.topGear){
				//無敵時間処理
				if (mRestMatchlessTime > 0) {
					mRestMatchlessTime--;
				}else {
					// デバッグボタンが押されていたら衝突チェックしない
					bool doTestIntersect = true;
					if ( Config.IsDebugMode && ( InputManager.IsKeyDown( Keys.Space ) || InputManager.IsKeyDown( Keys.X ) ||
						 InputManager.IsButtonDown( PlayerIndex.One, Buttons.Y ) || InputManager.IsButtonDown( PlayerIndex.One, Buttons.X ) ) ){
							doTestIntersect = false;
					}

					if ( doTestIntersect ){
						// 敵との衝突をチェック
						if (CheckHitEnemy(enemies, moveVec)){
						    // 死んだ瞬間ならSEを再生
						    if ( !mDied ){
						        SoundManager.Play( SoundManager.SE.Die );
								mPlayScore.IsMissed = true;
						    }
						    // あたり
						    mDied = true;
						    mStates.SetAnimation(mModel, "Dead", false, 10.0f);
						    mStates.AngleZ = 0.0f;
						}
					
						// オブジェクトとの衝突をチェック
						if (CheckHitObject(objectCollision, moveVec)){
							// 死んだ瞬間ならSEを再生
							if ( !mDied ){
								SoundManager.Play( SoundManager.SE.Die );
								mPlayScore.IsMissed = true;
							}
							mDied = true;
							mStates.SetAnimation(mModel, "Dead", false, 10.0f);
							mStates.AngleZ = 0.0f;
						}
					}
				}
			}

			// 注視点の補正値を更新
			if ( moveCameraX || moveCameraY ){
				// X移動が許可されている
				if ( moveCameraX ){
					mCameraTargetOnView.X += nowCameraTargetOnView.X;
					// 最大値を超えたら戻す
					if (Math.Abs(mCameraTargetOnView.X) > mMaxCameraDistanceX) {
						if (mCameraTargetOnView.X > 0.0f) {
							mCameraTargetOnView.X = mMaxCameraDistanceX;
						}
						if (mCameraTargetOnView.X < 0.0f) {
							mCameraTargetOnView.X = -mMaxCameraDistanceX;
						}
					}
				}
				// Y移動が許可されている
				if ( moveCameraY ){
					mCameraTargetOnView.Y += nowCameraTargetOnView.Y;
					// 最大値を超えたら戻す
					if (Math.Abs(mCameraTargetOnView.Y) > mMaxCameraDistanceY) {
						if (mCameraTargetOnView.Y > 0.0f) {
							mCameraTargetOnView.Y = mMaxCameraDistanceY;
						}
						if (mCameraTargetOnView.Y < 0.0f) {
							mCameraTargetOnView.Y = -mMaxCameraDistanceY;
						}
					}
				}
			}
			camera.Target += Vector3.Transform(mCameraTargetOnView, rota);

			mTopEffectStates.Position = mStates.Position;
			mTopEffectStates.Angle = mStates.Angle;

			//死亡時はカウントしない
			if (!IsDied) {
				mPlayScore.UpdateTime(++mTimeCount);

				if ( !OnStop() ){
					++mTimeCountToLing;
				}
			}
		}
		#endregion

		#region 描画処理メソッド
		/// <summary>
		/// 描画する
		/// </summary>
		/// <param name="camera">カメラ</param>
		/// <param name="time">時間</param>
		public void Draw(Camera camera, GameTime time, EffectManager.Type type ) {
			if ( type != EffectManager.Type.DepthMap ){
				float alpha = ( OnFast() ) ? 1.0f : mRestMatchlessTime / mMatchlessSecondAfterTopGear;
				Vector4 diffuse = new Vector4( 1.0f, 1.0f, 1.0f, alpha );
				mModelTopEffect.RenderWithOutDepth( camera, mTopEffectStates, time, 0, null, diffuse, false, EffectManager.Type.Diffuse );
				//TransparentModelManager.Add( mModelTopEffect, mTopEffectStates, 1.0f, 0, EffectManager.Type.Diffuse );
			}

			mModel.Render(camera, mStates, time, type);

			//Circle.Draw(camera, mStates.Position, new Vector2(mAvoidRangeOutside * 2.0f, mAvoidRangeOutside * 2.0f));
			//Circle.Draw(camera, mStates.Position, new Vector2(mAvoidRangeInside * 2.0f, mAvoidRangeInside * 2.0f));
			//Circle.Draw(camera, mStates.Position, new Vector2(mCollisionRange * 2.0f, mCollisionRange * 2.0f));
		}

		public void DrawLing( Camera camera, GameTime time, bool toLight, bool inside, bool outside ){
			if ( OnSlow() ){
				ModelStates states = new ModelStates( null );
				states.Position = Position;
				float angle = ( float )Math.PI * 2.0f * ( ( mTimeCountToLing % 100 ) / 100.0f );

				Vector4 inColor = new Vector4( 1.0f, 0.8f, 0.0f, 1.0f );
				Vector4 outColor = new Vector4( 1.0f, 0.5f, 0.5f, 1.0f );

				if ( toLight ){
					inColor = Vector4.One;
					outColor = Vector4.One;
				}

				//内側リング
				if ( inside ){
					states.Scale = Vector3.One * 90.0f;
					states.Angle = new Vector3( angle, angle, -angle );
					mModSwayLing.Render( camera, states, time, 1.0f, Vector4.One, inColor, false, EffectManager.Type.Diffuse );
					states.Angle = new Vector3( angle, -angle, angle );
					mModSwayLing.Render( camera, states, time, 1.0f, Vector4.One, inColor, false, EffectManager.Type.Diffuse );
				}

				//外側リング
				if ( outside ){
					states.Scale = Vector3.One * 140.0f;
					states.Angle = new Vector3( angle, -angle, -angle );
					mModSwayLing.Render( camera, states, time, 1.0f, Vector4.One, outColor, false, EffectManager.Type.Diffuse );
					states.Angle = new Vector3( -angle, angle, angle );
					mModSwayLing.Render( camera, states, time, 1.0f, Vector4.One, outColor, false, EffectManager.Type.Diffuse );
				}
			}
		}

		public void DrawSprite( Camera camera ){
			if ( mDied ){
				if ( !mDeadTextures.isAnimationEnd ){
					Vector3 position;
					switch ( mHitPositionID ){
						case 0 :
							position = mStates.Position;
							break;
						case 1 :
							position = GetWingPos( WingPosNum.left );
							break;
						case 2 :
							position = GetWingPos( WingPosNum.right );
							break;
						default :
							return;
					}

					// スクリーン座標でのプレイヤーの位置を算出
					Vector3 p = Vector3.Transform( position, camera.View * camera.Projection );
					p /= p.Z;
					p = ( p + Vector3.One ) / 2;

					// 画像の出力先を算出
					float destX = p.X * GameMain.ScreenWidth;
					float destY = ( 1.0f - p.Y ) * GameMain.ScreenHeight;

					SpriteBoard.RenderUseCenterPosition( mDeadTextures.GetWithAdvanceAnimation( 3, false ), destX, destY );
				}
			}
		}

		/// <summary>
		/// スコアインフォメーションを描画する
		/// </summary>
		public void DrawScoreInfomation() {
			// スコアインフォメーションの描画
			for (int i = 0; i < mScoreInfomations.Length; i++) {
				mScoreInfomations[i].Draw();
			}
			
		}
		#endregion

		#region 操作処理メソッド
		/// <summary>
		/// キーボードによる操作
		/// </summary>
		private Vector3 UpdateByKeyBoard(Stage stage) {
			Vector3 moveVec = Vector3.Zero;
			//ポーズする
			if(--mAfterPauseCount < 0 && InputManager.IsJustKeyDown(Keys.Enter)){
				mOnPause = true;
			}

			//ポーズ中では処理を飛ばす
			if (OnPause) {
				return moveVec;
			}
			
			if ( !OnStop()) {
				//モードをキーで切り替え
				if (!OnFast()) {
					if (!OnSlow()) {
						if (InputManager.IsJustKeyDown(Keys.Z) && mZoneGauge >= mUsedZoneMin) {
							mMode = (int)ModeName.zone;
						}
					} else {
						if (InputManager.IsKeyUp(Keys.Z) && mUsedZoneGauge >= mUsedZoneMin) {
							//トップギアになれるか
							if (!(mJustTopGearChanged = CheckTopGearChange())) {
								mComboCount = 0;
								mUsedZoneGauge = mGaugeMin;
								mMode = (int)ModeName.normal;
							}
						}
					}
				}

				//左右の移動
				if (InputManager.IsKeyDown(Keys.Left)) {
					moveVec += GetMoveX(-1.0f);
				} else if (InputManager.IsKeyDown(Keys.Right)) {
					moveVec += GetMoveX(1.0f);
				}
				//上下の移動
				if (InputManager.IsKeyDown(Keys.Up)) {
					moveVec += GetMoveY(1.0f);
				} else if (InputManager.IsKeyDown(Keys.Down)) {
					moveVec += GetMoveY(-1.0f);
				}

				//上下左右反転か調べる
				if (Config.IsReverseLeftRight) {
					moveVec.X = -moveVec.X;
				}
				if (Config.IsReverseUpDown) {
					moveVec.Y = -moveVec.Y;
				}
				
				// 入力した方向に応じてプレイヤーを回転
				if ( moveVec.X < 0.0f ){
					mAngle.Y = Math.Min( 0.6f, mAngle.Y + 0.04f );
				}else if ( moveVec.X > 0.0f ){
					mAngle.Y = Math.Max( -0.6f, mAngle.Y - 0.04f );
				}else{
					mAngle.Y *= 0.8f;
				}
				if ( moveVec.Y < 0.0f ){
					mAngle.X = Math.Min( 0.4f, mAngle.X + 0.04f );
				}else if ( moveVec.Y > 0.0f ){
					mAngle.X = Math.Max( -0.4f, mAngle.X - 0.04f );
				}else{
					mAngle.X *= 0.95f;
				}
			}
			return moveVec;
		}

		/// <summary>
		/// コントローラーによる操作
		/// </summary>
		/// <returns></returns>
		private Vector3 UpdateByController(Stage stage) {
			Vector3 moveVec = Vector3.Zero;
			//ポーズする
			if(--mAfterPauseCount < 0 && InputManager.IsJustButtonDown( PlayerIndex.One, Buttons.Start )){
				mOnPause = true;
			}

			if ( !OnStop()) {
				Vector2 leftStick = InputManager.GetThumbSticksLeft(PlayerIndex.One);
				//上下左右反転か調べる
				if (Config.IsReverseLeftRight) {
					leftStick.X = -leftStick.X;
				}
				if (Config.IsReverseUpDown) {
					leftStick.Y = -leftStick.Y;
				}

				moveVec += GetMoveX(leftStick.X);
				moveVec += GetMoveY(leftStick.Y);
				
				// 入力した方向に応じてプレイヤーを回転
				if ( moveVec.X < 0.0f ){
					mAngle.Y = Math.Min( 0.6f, mAngle.Y + 0.04f );
				}else if ( moveVec.X > 0.0f ){
					mAngle.Y = Math.Max( -0.6f, mAngle.Y - 0.04f );
				}else{
					mAngle.Y *= 0.8f;
				}
				if ( moveVec.Y < 0.0f ){
					mAngle.X = Math.Min( 0.4f, mAngle.X + 0.04f );
				}else if ( moveVec.Y > 0.0f ){
					mAngle.X = Math.Max( -0.4f, mAngle.X - 0.04f );
				}else{
					mAngle.X *= 0.95f;
				}


				//モードをキーで切り替え
				if (!OnFast()) {
					if (!OnSlow()) {
						if (InputManager.IsJustButtonDown(PlayerIndex.One, Config.ZoneButton) && mZoneGauge >= mUsedZoneMin) {
							mMode = (int)ModeName.zone;
						}
					} else {
						if (InputManager.IsButtonUp(PlayerIndex.One, Config.ZoneButton) && mUsedZoneGauge >= mUsedZoneMin) {
							//トップギアになれるか
							if (!(mJustTopGearChanged = CheckTopGearChange())) {
								mComboCount = 0;
								mUsedZoneGauge = mGaugeMin;
								mMode = (int)ModeName.normal;
							}
						}
					}
				}
			}
			//慣性
			if (mMoveVecBefore != Vector3.Zero && moveVec == Vector3.Zero) {
				mMoveVecInter.Get(mMoveVecBefore, mMoveVecBefore, (int)mInertiaTime);
				mMoveVecBefore = Vector3.Zero;
				mInertiaRestFrame = (int)mInertiaTime;
			} else {
				//移動分を保存
				mMoveVecBefore = moveVec;
			}
			if (mInertiaRestFrame > 0) {
				--mInertiaRestFrame;
				moveVec += mMoveVecInter.Get((int)mInertiaTime-mInertiaRestFrame);
			}
			return moveVec;
		}

		/// <summary>
		/// 傾きの操作(キーボード、コントローラ共通)
		/// </summary>
		private void ControllRoll() {

			//ローリングする
			if (isRolling) {
				float rad;		//回転角度
				//モードによって回転速度を変化させる
				if (OnFast()) {
					rad = 0.25f;
				} else if (OnStop()) {
					rad = 0.0075f;
				} else if (OnSlow()) {
					rad = 0.075f;
				} else {
					rad = 0.15f;
				}
				//左回転
				if (mPushedRollButtonNum_L == 2) {
					mStates.AngleZ -= rad;
					//押しっぱなしで回転し続ける
					if (!InputManager.IsKeyDown(Keys.A) && !InputManager.IsButtonDown(PlayerIndex.One, Config.LeftRotateButton)) {
						//１回転したら元に戻す
						if (mStates.AngleZ < -2 * Math.PI) {
							//回転し続けた場合の補正
							mStates.AngleZ %= (float)(2 * Math.PI);
							if (mStates.AngleZ < -Math.PI) {
								mStates.AngleZ += (float)(2 * Math.PI);
							}
							isRolling = false;
							mPushedRollButtonNum_L = 0;
						}
					}
				}
					//右回転
				else if (mPushedRollButtonNum_R == 2) {
					mStates.AngleZ += rad;
					//押しっぱなしで回転し続ける
					if (!InputManager.IsKeyDown(Keys.S) && !InputManager.IsButtonDown(PlayerIndex.One, Config.RightRotateButton)) {
						//１回転したら元に戻す
						if (mStates.AngleZ > 2 * Math.PI) {
							//回転し続けた場合の補正
							mStates.AngleZ %= (float)(2 * Math.PI);
							if (mStates.AngleZ > Math.PI) {
								mStates.AngleZ -= (float)(2 * Math.PI);
							}
							isRolling = false;
							mPushedRollButtonNum_R = 0;
						}
					}
				}
			} else {
				if (!OnStop()) {
					//２回連続で押した場合ローリングさせる
					//左回転
					if (InputManager.IsJustKeyDown(Keys.A) || InputManager.IsJustButtonDown(PlayerIndex.One, Config.LeftRotateButton)) {
						mPushedRollButtonNum_L++;
					}
					if (mPushedRollButtonNum_L == 1) {
						mPassedFrame++;
						//猶予時間を越えたらリセット
						if (mPassedFrame > mRollingDelayFrame) {
							mPushedRollButtonNum_L = 0;
							mPassedFrame = 0;
						}
					} else if (mPushedRollButtonNum_L == 2) {
						isRolling = true;
						mPassedFrame = 0;
					}
					//右回転
					if (InputManager.IsJustKeyDown(Keys.S) || InputManager.IsJustButtonDown(PlayerIndex.One, Config.RightRotateButton)) {
						mPushedRollButtonNum_R++;
					}
					if (mPushedRollButtonNum_R == 1) {
						mPassedFrame++;
						//猶予時間を越えたらリセット
						if (mPassedFrame > mRollingDelayFrame) {
							mPushedRollButtonNum_R = 0;
							mPassedFrame = 0;
						}
					} else if (mPushedRollButtonNum_R == 2) {
						isRolling = true;
						mPassedFrame = 0;
					}
					//傾ける
					//左90°
					if (InputManager.IsKeyDown(Keys.A) || InputManager.IsButtonDown(PlayerIndex.One, Config.LeftRotateButton)) {
						mStates.AngleZ -= 0.06f;
						if (mStates.AngleZ < -Math.PI / 2) {
							mStates.AngleZ = -(float)Math.PI / 2;
						}

					}
					//右90° 
					if (InputManager.IsKeyDown(Keys.S) || InputManager.IsButtonDown(PlayerIndex.One, Config.RightRotateButton)) {
						mStates.AngleZ += 0.06f;
						if (mStates.AngleZ > Math.PI / 2) {
							mStates.AngleZ = (float)Math.PI / 2;
						}
					}
 					//戻す
					if ((InputManager.IsKeyUp(Keys.A) && InputManager.IsButtonUp(PlayerIndex.One, Config.LeftRotateButton))
						&& (InputManager.IsKeyUp(Keys.S) && InputManager.IsButtonUp(PlayerIndex.One, Config.RightRotateButton))) {
						if (mStates.AngleZ < 0) {
							mStates.AngleZ += 0.06f;
							if (mStates.AngleZ > 0) {
								mStates.AngleZ = 0;
							}
						} else if (mStates.AngleZ > 0) {
							mStates.AngleZ -= 0.06f;
							if (mStates.AngleZ < 0) {
								mStates.AngleZ = 0;
							}
						}
					}
				}
			}
		}
		#endregion

		#region 移動処理メソッド
		/// <summary>
		/// 左右の移動ベクトルを取得する
		/// </summary>
		/// <param name="inputPad"></param>
		/// <returns>移動ベクトル</returns>
		private Vector3 GetMoveX(float inputPad) {
			Vector3 moveVec = Vector3.Zero;
			moveVec.X += inputPad * mSpeedMax_X;
			return moveVec;
		}

		/// <summary>
		/// 上下の移動ベクトルを取得する
		/// </summary>
		/// <param name="inputPad"></param>
		/// <returns>移動ベクトル</returns>
		private Vector3 GetMoveY(float inputPad) {
			Vector3 moveVec = Vector3.Zero;
			moveVec.Y += inputPad * mSpeedMax_Y;
			return moveVec;
		}

		/// <summary>
		/// モードによる移動ベクトルの倍率を取得する
		/// </summary>
		/// <returns></returns>
		private float GetModeSpeedRatio() {
			//デバッグ用高速化
			if(Config.IsDebugMode && ( InputManager.IsKeyDown(Keys.Space) || InputManager.IsButtonDown( PlayerIndex.One, Buttons.Y ) ) ){
				return 10.0f;
			}
			
			if (mStopTime > 0) {
				mStopTime--;
				if (mStopTime == 0) {
					mAdvanceTime = (int)mAdvanceSecond;
				}
				return mStopScrollSpeedRatio;
			} else if (mAdvanceTime > 0) {
				mAdvanceTime--;
				return mAdvanceInter.Get(mAdvanceScrollSpeedRatio, 1.0f, 60 - mStopTime);
			} else {
				//モードによるスピード調整
				switch (mMode) {
					case (int)ModeName.zone:
						return mZoneScrollSpeedRatio;
					case (int)ModeName.topGear:
						return mTopGearScrollSpeedRatio;
					default:
						return 1.0f;
				}
			}
		}

		/// <summary>
		/// プレイヤーの向きを取得する
		/// </summary>
		/// <param name="moveVec">移動ベクトル(スクロールのみで取得)</param>
		/// <param name="stage">ステージ</param>
		/// <returns>プレイヤーの向き</returns>
		private Vector3 GetPlayerAngle(Vector3 moveVec, Stage stage) {
			Vector3 angle = Vector3.Zero;
			// ワールド座標系への変換行列
			Matrix rotaY = Matrix.CreateRotationY(stage.CameraAngle.Y);
			Matrix rotaX = Matrix.CreateRotationX(stage.CameraAngle.X);
			Matrix rota = rotaX * rotaY;

			// 移動ベクトルをワールド座標系に変換
			angle = Vector3.Transform(moveVec, rota);
			return angle;
		}
		#endregion

		#region 衝突処理メソッド
		/// <summary>
		/// 敵と衝突したかを調べる
		/// </summary>
		/// <param name="enemies">敵</param>
		/// <param name="moveVec">移動ベクトル</param>
		/// <returns>衝突したらtrueを返す</returns>
		private bool CheckHitEnemy(Enemy[] enemies, Vector3 moveVec) {
			foreach (Enemy enemy in enemies) {
				//敵と衝突
				if (enemy.IsIntersect(Position + moveVec, mCollisionRange, false)) {
					mHitPositionID = 0;
					return true;
				}
				if (enemy.IsIntersect(GetWingPos(WingPosNum.left) + moveVec, mCollisionRange, false)) {
					mHitPositionID = 1;
					return true;
				}
				if (enemy.IsIntersect(GetWingPos(WingPosNum.right) + moveVec, mCollisionRange, false)) {
					mHitPositionID = 2;
					return true;
				}
			}
			return false;
		}

		private bool CheckHitObject( CollisionModel objectCollision, Vector3 moveVec ){
			if (objectCollision.IsIntersect(mStates.Position - moveVec, mStates.Position)) {
				mHitPositionID = 0;
				return true;
			}
			if (objectCollision.IsIntersect(GetWingPos(WingPosNum.left) - moveVec, GetWingPos(WingPosNum.left)) ) {
				mHitPositionID = 1;
				return true;
			}
			if (objectCollision.IsIntersect(GetWingPos(WingPosNum.right) - moveVec, GetWingPos(WingPosNum.right))) {
				mHitPositionID = 2;
				return true;
			}
			return false;
		}

		/// <summary>
		/// ステージとと衝突したか調べる
		/// </summary>
		/// <param name="stage">ステージ</param>
		/// <param name="moveVec">移動ベクトル</param>
		/// <returns>衝突したらtrueを返す</returns>
		private bool CheckHitStage(Stage stage, Vector3 moveVec) {
			//ステージと衝突
			if (stage.CollisionModel.IsIntersect(Position, mCollisionRange)) {
				return true;
			}
			return false;
		}

		/// <summary>
		/// ギリ避けしたかチェックする
		/// </summary>
		/// <param name="enemies">敵</param>
		/// <param name="moveVec">移動ベクトル</param>
		/// <returns>ギリ避けした瞬間にtrueを返す</returns>
		public bool CheckAvoid(Enemy[] enemies, Vector3 moveVec,Camera camera) {
			bool justAvoid = false;
			foreach (Enemy enemy in enemies) {
				if (!enemy.Avoided) {
					//内側の範囲から調べていく
					//敵のギリ避け範囲を抜けた瞬間
					if (enemy.IsIntersect(Position, mAvoidRangeInside, true) &&
						!enemy.IsIntersect(Position + moveVec, mAvoidRangeInside, true)) {
						justAvoid = enemy.Avoided = true;
						mJustAvoidLevel = AvoidLevel.Inside;
						UpdateByJustAvoid(enemy,camera,true);
					} else if (enemy.IsIntersect(Position, mAvoidRangeOutside, true) &&
						  !enemy.IsIntersect(Position + moveVec, mAvoidRangeOutside, true)) {
						justAvoid = enemy.Avoided = true;
						mJustAvoidLevel = AvoidLevel.Outside;
						UpdateByJustAvoid(enemy,camera,false);
					}
				}
			}
			return justAvoid;
		}
		#endregion

		/// <summary>
		/// ギリ避け成功時の処理
		/// </summary>
		private void UpdateByJustAvoid(Enemy enemy,Camera camera,bool isInner) {
			//最大コンボかチェック
			if (++mComboCount > mMaxCombo) {
				mMaxCombo = mComboCount;
			}
			if ( enemy.AddTopLevelEnable ){
				mTopLevel += (int)mJustAvoidLevel;
			}
			mStopTime = (int)mStopSecond;
			mAdvanceTime = 0;
			//スコア加算
			mPlayScore.AddScore((int)enemy.Name, mComboCount,isInner);
			mPlayScore.IsAvoid = true;
			//スコアインフォ表示
			for(int i=0;i<10;i++) {
				if (!mScoreInfomations[i].Active) {
					int score = Math.Min( (int)mPlayScore.GetNowAddScore,9999);
					Vector3 postion = (Position + enemy.Position) / 2;
					mScoreInfomations[i].Set(score, camera, postion);
					break;
				}
			}
		}

		/// <summary>
		/// トップギア時のステージとの衝突
		/// </summary>
		/// <param name="stage"></param>
		/// <param name="moveVec"></param>
		/// <returns></returns>
		public Vector3 CheckTopGearHitStage(Stage stage, Vector3 moveVec) {
			int count = 0;
			Vector3 cross;	//衝突点
			Vector3[] vertices;	//衝突面を形成する3頂点
			Vector3 normal;	//法線
			//ステージと衝突しているか
			while (stage.CollisionModel.IsIntersect(mStates.Position, mStates.Position + moveVec, out cross, out vertices, out normal)) {
				// 10回以上ループしてたら諦める
				if ( count > 10 ){
					moveVec = normal * 10.0f;
					break;
				}

				Vector3 vP0C = cross - mStates.Position;
				Vector3 vCP1 = ( mStates.Position + moveVec ) - cross;
				Vector3 axis = Vector3.Cross( normal, vCP1 );
				float r = ( float )( Math.PI * 0.5f - Math.Acos( ( float )Vector3.Dot( Vector3.Normalize( -vP0C ), normal ) ) );
				moveVec = vP0C + Vector3.Transform( vCP1, Matrix.CreateFromAxisAngle( axis, -r ) );

				moveVec = Vector3.Normalize( moveVec ) * 15.0f * GetModeSpeedRatio();

				//return (normal * 10);
				++count;
			}
			return moveVec;
		}
		#region モード処理メソッド
		/// <summary>
		/// モードによる処理
		/// </summary>
		/// <param name="enemies">敵</param>
		/// <param name="moveVec">移動ベクトル</param>
		private void UpdateByMode(Enemy[] enemies, Vector3 moveVec,Camera camera) {
			//モードごとの処理
			switch (mMode) {
				//ノーマルモード
				case (int)ModeName.normal:
					//ゲージの回復
					mZoneGauge += mZoneRecoverParFrame;
					if (mZoneGauge >= mGaugeMax) {
						mZoneGauge = mGaugeMax;
					}
					break;
				//ゾーンモード
				case (int)ModeName.zone:
					//ギリ避けを調べる
					mJustAvoid = CheckAvoid(enemies, moveVec,camera);

					// 避けた瞬間ならSEを再生
					if ( mJustAvoid ){
						SoundManager.Play( SoundManager.SE.Avoid );
					}

					//ゲージの消費
					float useGauge;
					if (mStopTime > 0) {
						useGauge = mZoneSpendParFrame * mStopScrollSpeedRatio;
					} else {
						useGauge = mZoneSpendParFrame;
					}
					//ローリング中は消費を減らす
					if (isRolling) {
						useGauge *= 0.8f;
					}
					mZoneGauge -= useGauge;
						
					//ゲージが0になった場合
					mUsedZoneGauge += mZoneSpendParFrame;
					if (mZoneGauge <= mGaugeMin) {
						if (!(mJustTopGearChanged = CheckTopGearChange())) {
							mZoneGauge = mGaugeMin;
							mComboCount = 0;
							mTopLevel = 0;
							mUsedZoneGauge = mGaugeMin;
							mMode = (int)ModeName.normal;
						}
						// トップギアになる瞬間
						else{
							SoundManager.Play( SoundManager.SE.TopGear );
							mStates.SetAnimation(mModel, "top", true, 10.0f);
							if (!Config.IsSpecialMode) {
								mModel.SetTexture(mTopTexture);
							} else {
								mModel.SetTexture(mInjureTexture);
							}
						}
					}
					break;
				//トップギアモード
				case (int)ModeName.topGear:
					//ここでトップギアになった瞬間である場合は処理を1回無視する
					if (!mJustTopGearChanged) {
						//ゲージの消費
						mTopGearGauge -= mTopGearSpendParFrame;
						//ゲージが0になった場合
						if (mTopGearGauge <= mGaugeMin) {
							mTopGearGauge = mGaugeMin;
							mRestMatchlessTime = (int)mMatchlessSecondAfterTopGear;
							mMode = (int)ModeName.normal;
							mComboCount = 0;
							mTopLevel = 0;
							mStates.SetAnimation(mModel, "Take 001", true, 30.0f);
							if (!Config.IsSpecialMode) {
								mModel.SetTexture(null);
							} else {
								mModel.SetTexture(mTopTexture);
							}
						}
					}
					// トップギアになった瞬間
					else{
						SoundManager.Play( SoundManager.SE.TopGear );
						mStates.SetAnimation(mModel, "top", true, 30.0f);
						if (!Config.IsSpecialMode) {
							mModel.SetTexture(mTopTexture);
						} else {
							mModel.SetTexture(mInjureTexture);
						}
					}
					break;
			}
		}

		/// <summary>
		/// トップギアに移行できるか
		/// </summary>
		/// <returns>トップギアに移行した場合trueを返す</returns>
		private bool CheckTopGearChange() {
			//コンボ条件達成でトップギアになる
			if (mTopLevel >= mTopGearMinCount) {
				mMode = (int)ModeName.topGear;
				mZoneGauge = mGaugeMax;
				mUsedZoneGauge = mGaugeMin;
				int num = Math.Min(mTopLevel - mTopGearMinCount, mTopGearRecoverRatio.Length - 1);
				mStopTime = 0;
				mAdvanceTime = 0;
				//トップギアゲージ回復
				mTopGearGauge = mTopGearRecoverRatio[num];
				return true;
			}
			return false;
		}
		#endregion

		/// <summary>
		/// モデルの解放処理
		/// </summary>
		public void Release(){
			mModel.Release();
			mModSwayLing.Release();
			mModelTopEffect.Release();
		}

		public void SetMoveXReverse( bool enable ){
			Config.SetVsShark(enable);
		}

		/// <summary>
		/// 殺す
		/// </summary>
		public void Kill(){
			mDied = true;
		}

		public PlayScore GetPlayScore(){
			return mPlayScore;
		}

		/// <summary>
		/// スコアポップアップの表示をリセットする
		/// </summary>
		public void InitScore(){
			for (int i = 0; i < mScoreInfomations.Length; i++) {
				mScoreInfomations[i].Active = false;
			}
		}

		/// <summary>
		/// ボーナスアイテムを取得したかどうかを調べ、スコアに加算する
		/// </summary>
		public void CheckGotBonusItem( BonusItems bonusItems, Camera camera ){
			CheckGotBonusItem(bonusItems, camera, Position, mCollisionRange);
			CheckGotBonusItem(bonusItems, camera, GetWingPos(WingPosNum.left), mCollisionRange);
			CheckGotBonusItem(bonusItems, camera, GetWingPos(WingPosNum.right), mCollisionRange);
		}

		private void CheckGotBonusItem( BonusItems bonusItems, Camera camera, Vector3 position, float range ){
			if (bonusItems.TestIntersect(position, range)){
				//スコア加算
				mPlayScore.AddOldShipBonus();
				//スコアインフォ表示
				for(int i=0;i<10;i++) {
					if (!mScoreInfomations[i].Active) {
						mScoreInfomations[i].Set((int)mPlayScore.GetOldShipBonus, camera, position);
						break;
					}
				}
				SoundManager.Play( SoundManager.SE.Unbelievable );
			}
		}
	}
}

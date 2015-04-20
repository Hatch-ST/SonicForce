using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SonicLate {
	/// <summary>
	/// プレイ中のスコア管理クラス
	/// </summary>
	class PlayScore {
		/// <summary>
		/// ギリ避けでの合計スコア
		/// </summary>
		private uint mAvoidSamScore;

		/// <summary>
		/// チェックポイントのギリ避けスコア
		/// </summary>
		private uint mCheckPointAvoidScore;

		/// <summary>
		/// コンボでの合計スコア
		/// </summary>
		private uint mComboSamScore;

		/// <summary>
		/// チェックポイントのコンボスコア
		/// </summary>
		private uint mCheckPointComboScore;

		/// <summary>
		/// リングでの合計スコア
		/// </summary>
		private uint mRingSamScore;

		/// <summary>
		/// チェックポイントのリングスコア
		/// </summary>
		private uint mCheckPointRingScore;

		/// <summary>
		/// 現在の合計スコア
		/// </summary>
		private uint mNowSamScore;

		/// <summary>
		/// チェックポイントのスコア
		/// </summary>
		private uint mCheckPointScore;

		/// <summary>
		/// 加算スコア
		/// </summary>
		private uint mAddScore;

		/// <summary>
		/// プレイ時間(単位:フレーム)
		/// </summary>
		private ulong mPlayTime;

		/// <summary>
		/// 敵の番号
		/// </summary>
		private enum EnemyName{
			starfish = 0,
			heteroconger = 1,
			burrfish = 2,
			octopus_upDown = 3,
			octopus_screw = 4,
			jellyfish = 5,
			crab = 6,
			shark = 7,
			boneFish,
			whale,
			bigJellyfish,
			energy,
			bigHeteroconger,
		}

		/// <summary>
		/// 敵ごとのスコア
		/// </summary>
		private int[] mEnemyAvoidScore = { 1000, 500, 300, 400, 700, 600, 800, 4000, 300, 0, 20000, 200, 5000 };

		/// <summary>
		/// コンボ回数ごとのスコア
		/// </summary>
		private int[] mComboScore = { 0, 1000, 1500, 2000, 3000, 4500, 6000, 8000, 10000 };

		/// <summary>
		/// 内リングでのボーナススコア
		/// </summary>
		private int mRingScore = 2500;

		/// <summary>
		/// タイムごとのスコア
		/// </summary>
		private uint[] mTimeScore = { 80000, 50000, 20000,10000, 0 };

		/// <summary>
		/// ボーナスがもらえるタイム
		/// </summary>
		private int[] mBonusTime = { 60*60*5, 60*30*15, 60*60*10, 60*60*15 };


		/// <summary>
		/// 沈没船ボーナス
		/// </summary>
		private uint mOldShipBonus = 50000;

		/// <summary>
		/// 失敗したか
		/// </summary>
		private bool mIsMissed;

		/// <summary>
		/// ノーミスボーナス
		/// </summary>
		private uint mNoMissBonus = 100000;

		/// <summary>
		/// ボーナススコア
		/// </summary>
		private uint mBonusSamScore = 0;

		/// <summary>
		/// ギリ避けしたか
		/// </summary>
		private bool mIsAvoid;

		/// <summary>
		/// ノーギリ避け減点
		/// </summary>
		private uint mNoAvoidScore = 999999999;

		/// <summary>
		/// Success(ギリ避け)の合計スコアを取得する
		/// </summary>
		public uint GetAvoidSamScore {
			get { return mAvoidSamScore; }
		}

		/// <summary>
		/// Comboの合計スコアを取得する
		/// </summary>
		public uint GetComboSamScore {
			get { return mComboSamScore; }
		}

		/// <summary>
		/// Ringの合計スコアを取得する
		/// </summary>
		public uint GetRingSamScore {
			get { return mRingSamScore; }
		}

		/// <summary>
		///  現在の合計スコアを取得する
		/// </summary>
		public uint GetNowSamScore {
			get { return mNowSamScore; }
		}

		/// <summary>
		///  加算スコアを取得する
		/// </summary>
		public uint GetNowAddScore {
			get { return mAddScore; }
		}
		
		/// <summary>
		///  プレイ時間を取得する
		/// </summary>
		public ulong GetPlayTime {
			get { return mPlayTime; }
		}

		/// <summary>
		///  沈没船ボーナスのスコアを取得する
		/// </summary>
		public ulong GetOldShipBonus {
			get { return mOldShipBonus; }
		}

		/// <summary>
		///  ミスしたかどうか
		/// </summary>
		public bool IsMissed {
			get { return mIsMissed; }
			set { mIsMissed = value; }
		}

		/// <summary>
		///  ギリ避けしたかどうか
		/// </summary>
		public bool IsAvoid {
			get { return mIsAvoid; }
			set { mIsAvoid = value; }
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public PlayScore() {
			Initialize();
		}

		/// <summary>
		/// 初期化
		/// </summary>
		public void Initialize(){
			mAvoidSamScore=0;
			mComboSamScore=0;
			mNowSamScore=0;
			mRingSamScore = 0;
			mAddScore=0;
			mPlayTime = 0;
			mIsMissed = false;
			mIsAvoid = false;
			mBonusSamScore = 0;
		}

		/// <summary>
		/// プレイ時間を更新する
		/// </summary>
		/// <param name="playTime">プレイ時間</param>
		public void UpdateTime(ulong playTime) {
			mPlayTime = playTime;
		}

		/// <summary>
		/// スコアを加算する
		/// </summary>
		/// <param name="enemyType"></param>
		/// <param name="comboCount">コンボ数</param>
		/// <param name="isRotate">傾けているか</param>
		/// <param name="isInner">内リングで避けたか</param>
		public void AddScore(int enemyType, int comboCount,bool isInner) {
			//加算スコアをリセット
			mAddScore = 0;
			//ギリ避けスコア加算
			mAvoidSamScore += (uint)mEnemyAvoidScore[enemyType];
			mAddScore += (uint)mEnemyAvoidScore[enemyType];
			//コンボスコア加算
			if (comboCount < mComboScore.Length) {
				mComboSamScore += (uint)mComboScore[comboCount];
				mAddScore += (uint)mComboScore[comboCount];
			} else {
				mComboSamScore += (uint)mComboScore[mComboScore.Length - 1];
				mAddScore += (uint)mComboScore[mComboScore.Length - 1];
			}
			//リングスコア加算
			if (isInner) {
				mRingSamScore += (uint)mRingScore;
				mAddScore += (uint)mRingScore;
			}
			//合計スコア加算
			mNowSamScore += mAddScore;
		}

		/// <summary>
		/// スコアをチェックポイントまで戻す
		/// </summary>
		public void LoadCheckPointScore() {
			mNowSamScore = mCheckPointScore;
			mAvoidSamScore = mCheckPointAvoidScore;
			mComboSamScore = mCheckPointComboScore;
			mRingSamScore = mCheckPointRingScore;
			mBonusSamScore = 0;
		}

		/// <summary>
		/// チェックポイントでのスコアを保存する
		/// </summary>
		public void SaveCheckPointScore() {
			mCheckPointScore = mNowSamScore;
			mCheckPointAvoidScore = mAvoidSamScore;
			mCheckPointComboScore = mComboSamScore;
			mCheckPointRingScore = mRingSamScore;
		}

		/// <summary>
		/// タイムボーナススコアを取得する
		/// </summary>
		/// <returns>タイムボーナス</returns>
		public uint GetTimeScore() {
			if (mPlayTime < (uint)mBonusTime[0]) {
				return mTimeScore[0];
			} else if (mPlayTime < (uint)mBonusTime[1]) {
				return mTimeScore[1];
			} else if (mPlayTime < (uint)mBonusTime[2]) {
				return mTimeScore[2];
			} else if (mPlayTime < (uint)mBonusTime[3]) {
				return mTimeScore[3];
			} else {
				return mTimeScore[4];
			}

		}

		/// <summary>
		///	合計ボーナススコアを取得する
		/// </summary>
		/// <returns></returns>
		public uint GetBonusSamScore() {
			if (!mIsMissed) {
				mBonusSamScore += mNoMissBonus;
			}
			return mBonusSamScore;
		}

		/// <summary>
		/// ノーギリ避け減点を取得する
		/// </summary>
		/// <returns></returns>
		public uint GetNoAvoidScore() {
			if (mIsAvoid) {
				return 0;
			} else {
				return mNoAvoidScore;
			}
		}

		/// <summary>
		/// 沈没船ボーナスを加算する
		/// </summary>
		public void AddOldShipBonus() {
			mBonusSamScore += mOldShipBonus;
			mNowSamScore += mOldShipBonus;
		}

	}
}

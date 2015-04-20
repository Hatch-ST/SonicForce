using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace SonicLate {
	/// <summary>
	/// コンフィグ
	/// </summary>
	class Config {
		/// <summary>
		/// ゾーンボタン
		/// </summary>
		private static Buttons mZoneButton = Buttons.B;

		/// <summary>
		/// 左傾けボタン
		/// </summary>
		private static Buttons mLeftRotateButton = Buttons.LeftTrigger;

		/// <summary>
		/// 右傾けボタン
		/// </summary>
		private static Buttons mRighttRotateButton = Buttons.RightTrigger;

		/// <summary>
		/// 上下移動反対かどうか
		/// </summary>
		private static bool mIsReverseUpDown = false;

		/// <summary>
		/// 左右移動反対かどうか
		/// </summary>
		private static bool mIsReverseLeftRight = false;

		/// <summary>
		/// サメと対決中であるか
		/// </summary>
		private static bool mIsVsShark = false;

		/// <summary>
		/// SEのボリューム
		/// </summary>
		private static float mSEVolume = 0.8f;

		/// <summary>
		/// BGMのボリューム
		/// </summary>
		private static float mBGMVolume = 0.8f;

		/// <summary>
		/// 振動するか
		/// </summary>
		private static bool mIsVibrationOn = true;

		/// <summary>
		/// 裏モードか
		/// </summary>
		private static bool mIsSpecialMode = false;

		/// <summary>
		/// デバッグモードか
		/// </summary>
		private static bool mIsDebugMode = false;

		/// <summary>
		/// ゾーンボタン
		/// </summary>
		public static Buttons ZoneButton {
			get { return mZoneButton; }
		}

		/// <summary>
		/// 左傾けボタン
		/// </summary>
		public static Buttons LeftRotateButton {
			get { return mLeftRotateButton; }
		}

		/// <summary>
		/// 右傾けボタン
		/// </summary>
		public static Buttons RightRotateButton {
			get { return mRighttRotateButton; }
		}

		/// <summary>
		/// 上下移動反対かどうか
		/// </summary>
		public static bool IsReverseUpDown {
			get { return mIsReverseUpDown; }
		}

		/// <summary>
		/// 左右移動反対かどうか
		/// </summary>
		public static bool IsReverseLeftRight {
			get {
				if(mIsVsShark){
  					return !mIsReverseLeftRight;
				}else{
					return mIsReverseLeftRight;
				}
			}
		}

		/// <summary>
		/// SEボリューム
		/// </summary>
		public static float SEVolume {
			get { return mSEVolume; }
		}

		/// <summary>
		/// BGMボリューム
		/// </summary>
		public static float BGMVolume {
			get { return mBGMVolume; }
		}

		/// <summary>
		/// 振動がオンかどうか
		/// </summary>
		public static bool IsVibrationOn {
			get { return mIsVibrationOn; }
		}
		
		/// <summary>
		/// 裏モードかどうか
		/// </summary>
		public static bool IsSpecialMode {
			get { return mIsSpecialMode; }
		}

		/// <summary>
		/// デバッグモードかどうか
		/// </summary>
		public static bool IsDebugMode {
			get { return mIsDebugMode; }
		}

		//デフォルト値を入れる
		public static void Initialize() {
			mZoneButton = Buttons.B;
			mLeftRotateButton = Buttons.LeftTrigger;
			mRighttRotateButton = Buttons.RightTrigger;
			mIsReverseUpDown = false;
			mIsReverseLeftRight = false;
			mIsVsShark = false;
			mSEVolume = 0.8f;
			mBGMVolume = 0.8f;
			mIsVibrationOn = true;
		}

		/// <summary>
		/// 上下移動反対キーを変更する
		/// </summary>
		public static void ChangeUpDown(){
			mIsReverseUpDown = !mIsReverseUpDown;
		}

		/// <summary>
		/// 左右移動反対キーを変更する
		/// </summary>
		public static void ChangeLeftRight() {
			mIsReverseLeftRight = !mIsReverseLeftRight;
		}

		/// <summary>
		/// 振動を変更する
		/// </summary>
		public static void ChangeVibration() {
			mIsVibrationOn = !mIsVibrationOn;
		}

		/// <summary>
		/// SEボリュームを変更する
		/// </summary>
		public static void SetSEVolume(float volume) {
			mSEVolume = volume;
		}

		/// <summary>
		/// BGMボリュームを変更する
		/// </summary>
		public static void SetBGMVolume(float volume) {
			mBGMVolume = volume;
		}

		/// <summary>
		/// サメモードを変更する
		/// </summary>
		/// <param name="enable">サメ遭遇時であるか</param>
		public static void SetVsShark( bool enable ){
			mIsVsShark = enable;
		}

		/// <summary>
		/// モードを変更する
		/// </summary>
		/// <param name="enable">裏モードならtrue</param>
		public static void SetSpecialMode(bool enable) {
			mIsSpecialMode = enable;
		}
		
		/// <summary>
		/// デバッグモードかどうかを変更する
		/// </summary>
		/// <param name="enable">デバッグモードならtrue</param>
		public static void SetDebugMode(bool enable){
			mIsDebugMode = enable;
		}
	}
}

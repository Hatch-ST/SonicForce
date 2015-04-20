#region File Description
//-----------------------------------------------------------------------------
// AnimationClip.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
#endregion

namespace SkinnedModel{
	/// <summary>
	/// アニメーションに必要な情報を保持するクラス
	/// １つのアニメーションに必要な情報です。
	/// </summary>
	public class AnimationClip{
		TimeSpan mDuration;
		IList<Keyframe> mKeyframes;
		String mNameValue;
		List<RoundKeyframe> mRoundKeyframes;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public AnimationClip( String name, TimeSpan duration, IList<Keyframe> keyframes ){
			mNameValue = name;
			mDuration = duration;
			mKeyframes = keyframes;

			// 時間毎にキーフレームをまとめる
			mRoundKeyframes = new List<RoundKeyframe>();
			List<Keyframe> t = new List<Keyframe>();
			for ( int i = 0; i < mKeyframes.Count; i++ ){
				t.Add( mKeyframes[ i ] );

				// 次のキーフレームの時間が今回と異なれば、そこまでを一括りとして追加
				if ( i + 1 < mKeyframes.Count && mKeyframes[ i ].Time != mKeyframes[ i + 1 ].Time ){
					mRoundKeyframes.Add( new RoundKeyframe( t.ToArray(), mKeyframes[ i ].Time ) );
					t.Clear();
				}
			}
			// 最初のフレームを最後尾に追加
			//mRoundKeyframes.Add( new RoundKeyframe( mRoundKeyframes[ 0 ].Keyframes, mRoundKeyframes[ 0 ].Time ) );

			// 個々のフレームにも追加
			foreach ( Keyframe keyframe in mRoundKeyframes[ 0 ].Keyframes ){
				mKeyframes.Add( keyframe );
			}
		}

		/// <summary>
		/// 合計時間の取得
		/// </summary>
		public TimeSpan Duration{
			get { return mDuration; }
		}

		/// <summary>
		/// キーフレーム配列の取得
		/// 時間によってソートされている必要があります。
		/// </summary>
		public IList<Keyframe> Keyframes{
			get { return mKeyframes; }
		}

		/// <summary>
		/// クリップ名の取得
		/// </summary>
		public String Name{
			get { return mNameValue; }
		}

		/// <summary>
		/// 時間毎のキーフレームを取得
		/// </summary>
		public List<RoundKeyframe> RoundKeyframes{
			get { return mRoundKeyframes; }
		}
	}
}

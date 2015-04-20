using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace SonicLate{
	/// <summary>
	/// サウンド管理クラス
	/// </summary>
	class SoundManager{
		private static List<SoundEffect> mSoundEffects = null;
		private static List<Song> mSongs = null;

		public enum Music{
			Title,
			Game,
			Clear,
			Ending,
			Game_B,
		}

		public enum SE{
			Die,			// 死亡
			Avoid,			// ギリ避け
			TopGear,		// トップギア
			Shark,			// サメ噛み
			Ok,				// 決定
			Whale,			// クジラ
			Title,			// ソニックフォース
			Good,			// Good!
			Great,			// Great!
			Excellent,		// Excellent!
			Wonderful,		// Wonderful!
			Marvelous,		// Marvelous!
			Fantastic,		// Fantastic!
			Unbelievable,	// Unbelievable!
			ScoreSSS,		// 評価SSS
			ScoreSS,		// 評価SS
			ScoreS,			// 評価S
			ScoreA,			// 評価A
			ScoreB,			// 評価B
			ScoreC,			// 評価C
			ScoreZ,			// 評価Z
			Drums,			// ドラムロール
			DrumEnd,		// ドラム終了
			GameStartA,		// ゲームスタート
			GameStartB,		// ゲームスタート その２
			Thank,			// 遊んでくれて
			Zafter,			// 笑
		}

		public static void Initialize( ContentManager content ){
			mSoundEffects = new List<SoundEffect>();
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/die" ) );
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/attack02" ) );
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/topgear" ) );
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/shark" ) );
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/decision" ) );
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/whale" ) );
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/title" ) );
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/good" ) );
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/great" ) );
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/excellent" ) );
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/wonderful" ) );
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/marvelous" ) );
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/fantastic" ) );
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/unbelievable" ) );
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/ScoreSSS" ) );
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/ScoreSS" ) );
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/ScoreS" ) );
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/ScoreA" ) );
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/ScoreB" ) );
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/ScoreC" ) );
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/ScoreZ" ) );
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/score2" ) );
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/score1" ) );
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/start1" ) );
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/start2" ) );
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/thanks" ) );
			mSoundEffects.Add( content.Load<SoundEffect>( "Audio/Se/zafter" ) );

			mSongs = new List<Song>();
			mSongs.Add( content.Load<Song>( "Audio/bgm_Title_w" ) );
			mSongs.Add( content.Load<Song>( "Audio/bgm_stage_w" ) );
			mSongs.Add( content.Load<Song>( "Audio/bgm_Clear" ) );
			mSongs.Add( content.Load<Song>( "Audio/bgm_Ending" ) );
			mSongs.Add( content.Load<Song>( "Audio/bgm_stage_B_w" ) );
		}
		
		public static void Play( SE type ){
			mSoundEffects[ ( int )type ].Play( 1.0f * Config.SEVolume, 0.0f, 0.0f );
		}

		public static void Play( SE type, float volume ){
			mSoundEffects[ ( int )type ].Play( volume * Config.SEVolume, 0.0f, 0.0f );
		}

		public static void PlayMusic( Music type, bool loop ){
			MediaPlayer.Stop();
			MediaPlayer.IsRepeating = loop;
			MediaPlayer.Volume = Config.BGMVolume;
			MediaPlayer.Play( mSongs[ ( int )type ] );
		}

		public static void StopMusic(){
			MediaPlayer.Stop();
		}

		public static void Update(){
			MediaPlayer.Volume = Config.BGMVolume;
		}

		public static bool IsPlaying() {
			if (MediaPlayer.PlayPosition != TimeSpan.Zero) {
				return true;
			}
			return false;
		}

		private SoundManager(){}
		private SoundManager( SoundManager value ){}
	}
}

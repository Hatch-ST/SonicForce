using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SonicLate{
	/// <summary>
	/// リザルト画面
	/// </summary>
	class Result : GameChild{
		private MultiTexture mTexValuation = null;
		private MultiTexture mTexNumber = null;
		private MultiTexture mTexNumberRed = null;
		private Interpolate mValuationAlpha = null;
		private Interpolate mValuationScale = null;

		private FadeTexture mTexMain = null;
		private FadeTexture mTexWave = null;
		private FadeTexture mTexPiffling = null;

		private RandomScore[] mScores = null;
		private Vector2[] mScorePositions = null;
		private bool[] mScoreEnables = null;

		private Texture2D mBackGround;

		private float mMainPosX;
		private float mWavePosX;

		private int mCount = 0;
		
		private const int mMainMoveEndTime = 15;
		private const int mMainWaveEndTime = 15;

		private Mode mMode = Mode.Wait;

		private BlackOut mBlackOut = null;
		
		private UInt64[] mScoreValues = null;

		private int mCurrentScore = 0;

		private int mValuation = 0;
		private bool mEnableDrawValuation = false;

		private UInt64[] mValuations = { 600000, 600000, 400000, 300000, 250000, 1, 0 };

		private enum Mode{
			Wait,
			BackIn,
			ScoreIn,
			ScoreTotalIn,
			WaveIn,
			End
		}

		public Result( Texture2D backGround, UInt64 bonus, UInt64 success, UInt64 combo, UInt64 ring, UInt64 time, UInt64 piffling ){
			mBackGround = backGround;
			
			mBlackOut = new BlackOut( mContent, 60, BlackOut.Mode.Close, false );

			mTexMain = new FadeTexture( mContent, "Image/score/score_main", mMainMoveEndTime, true );
			mTexWave = new FadeTexture( mContent, "Image/score/score_nami", mMainWaveEndTime, true );
			mTexPiffling = new FadeTexture( mContent, "Image/score/score_main_Z", 5, true );

			mTexValuation = new MultiTexture( mContent, "Image/score/valuation", 7 );
			mTexNumber = new MultiTexture( mContent, "Image/score/num", 10 );
			mTexNumberRed = new MultiTexture( mContent, "Image/score/num_red", 10 );

			mValuationAlpha = new Interpolate();
			mValuationAlpha.Set( 0.0f, 1.0f );
			mValuationScale = new Interpolate();

			UInt64 total = bonus + success + combo + ring + time;

			if ( piffling > 0 ){
				total = 0;
			}

			mScoreValues = new UInt64[ 7 ];
			mScoreValues[ 0 ] = bonus;
			mScoreValues[ 1 ] = success;
			mScoreValues[ 2 ] = combo;
			mScoreValues[ 3 ] = ring;
			mScoreValues[ 4 ] = time;
			mScoreValues[ 5 ] = piffling;
			mScoreValues[ 6 ] = total;

			for ( int i = 0; i < mValuations.Length; i++ ){
				if ( total >= mValuations[ i ] ){
					mValuation = i;
					break;
				}
			}

			mScores = new RandomScore[ 7 ];
			mScorePositions = new Vector2[ 7 ];
			mScoreEnables = new bool[ 7 ];
			for ( int i = 0; i < mScores.Length; i++ ){
				MultiTexture numTex = ( i != 5 ) ? mTexNumber : mTexNumberRed;
				mScores[ i ] = new RandomScore( numTex, 9, mScoreValues[ i ], 8 );
				mScorePositions[ i ] = new Vector2( 600.0f, 168 + i * 53.5f );
				mScoreEnables[ i ] = false;
			}
			
			mScorePositions[ 6 ].X = 550.0f;
			mScorePositions[ 6 ].Y += 10.0f;
			mScores[ 6 ].SetScale( 1.2f );

			mMainPosX = 500.0f;
			mWavePosX = 500.0f;

			mCurrentScore = 0;
		}

		public override GameChild Update( GameTime time ){
			GameChild next = this;


			switch ( mMode ){
				case Mode.Wait :
					mBlackOut.Open();
					if ( mCount == 0 ){
						SoundManager.StopMusic();
						SoundManager.PlayMusic( SoundManager.Music.Clear, false );
					}

					if ( mCount > 60 ){
						mMode = Mode.BackIn;
						mCount = -1;
					}
					break;
				case Mode.BackIn :
					if ( mCount < mMainMoveEndTime ){
						// スライドイン
						mTexMain.In();
						mMainPosX -= 500.0f / mMainMoveEndTime;
					}else{
						mMode = Mode.ScoreIn;
						mCount = -1;
					}
					break;
				case Mode.ScoreIn :
					// 描画有効化
					mScoreEnables[ mCurrentScore ] = true;

					if ( mCount % 8 == 0 ){
						SoundManager.Play( SoundManager.SE.Drums );
					}

					// 桁回しが終わった
					if ( mScores[ mCurrentScore ].DrawEnd() ){
						SoundManager.Play( SoundManager.SE.DrumEnd );

						// 次のスコアへ
						++mCurrentScore;

						// 最後まで表示したら次へ
						if ( mCurrentScore >= 6 || ( mCurrentScore == 5 && mScoreValues[ 5 ] == 0 ) ){
							mMode = Mode.ScoreTotalIn;
							mCount = -1;
							break;
						}
					}

					break;
				case Mode.ScoreTotalIn :
					if ( mCount > 20 ){
						mScoreEnables[ 6 ] = true;
						
						if ( mCount % 8 == 0 ){
							SoundManager.Play( SoundManager.SE.Drums );
						}

						// 最後まで表示したら次へ
						if ( mScores[ 6 ].DrawEnd() ){
							SoundManager.Play( SoundManager.SE.DrumEnd );

							mMode = Mode.WaveIn;
							mCount = -1;
							break;
						}
					}
					break;
				case Mode.WaveIn :
					if ( mCount < mMainWaveEndTime ){
						// スライドイン
						mTexWave.In();
						mWavePosX -= 500.0f / mMainWaveEndTime;

						mEnableDrawValuation = true;
					}else if ( InputManager.IsJustButtonDown( PlayerIndex.One, Buttons.A ) || InputManager.IsJustKeyDown( Keys.Enter ) ){
						mMode = Mode.End;
						mCount = -1;
					}

					if ( mCount == 10 ){
						switch ( mValuation ){
							case 0 :
								SoundManager.Play( SoundManager.SE.ScoreSSS );
								break;
							case 1 :
								SoundManager.Play( SoundManager.SE.ScoreSS );
								break;
							case 2 :
								SoundManager.Play( SoundManager.SE.ScoreS );
								break;
							case 3 :
								SoundManager.Play( SoundManager.SE.ScoreA );
								break;
							case 4 :
								SoundManager.Play( SoundManager.SE.ScoreB );
								break;
							case 5 :
								SoundManager.Play( SoundManager.SE.ScoreC );
								break;
							case 6 :
								SoundManager.Play( SoundManager.SE.ScoreZ );
								break;
						}
					}
					if ( mValuation == 6 && mCount == 130 ){
						SoundManager.Play( SoundManager.SE.Zafter );
					}

					break;
				case Mode.End :
					mBlackOut.Close();

					// 終了
					if ( mCount > 60 ){
						next = new Ending( true );
					}
					break;
			}

			++mCount;

			return next;
		}

		public override void Draw( GameTime time ){
			if ( mBackGround != null ){
				SpriteBoard.Render( mBackGround, 0, 0, 1.0f );
			}

			mTexMain.Render( mMainPosX, 0.0f );
			mTexWave.Render( mWavePosX, 0.0f );

			for ( int i = 0; i < mScores.Length; i++ ){
				if ( mScoreEnables[ i ] ){
					mScores[ i ].Draw( ( int )mScorePositions[ i ].X, ( int )mScorePositions[ i ].Y );
				}
			}

			if ( mScoreEnables[ 5 ] ){
				mTexPiffling.In();
				mTexPiffling.Render( 0.0f,0.0f );
			}

			if ( mEnableDrawValuation ){
				float rate = mValuationScale.Get( 2.0f, 1.0f, 20 );
				float width = mTexValuation.Get[ mValuation ].Width * rate;
				float height = mTexValuation.Get[ mValuation ].Height * rate;
				SpriteBoard.RenderUseCenterPosition( mTexValuation.Get[ mValuation ], 1072.0f, 547.0f, width, height, mValuationAlpha.Get( 60 ), 1.0f );
			}

			mBlackOut.Draw();
		}

		private class RandomScore{
			private MultiTexture mTexNumbers = null;
			private UInt64 mTotalValue = 0;
			private int[] mFigures = null;
			private int mNumFigure = -1;
			private readonly int mFigureLength = 10;
			private readonly UInt64 mMax;
			private int mWaitTime = 10;
			private int mCount = 0;
			private Random mRandom = null;
			private bool mDrawEnd = false;
			private float mScaleRate = 1.0f;
			public RandomScore( MultiTexture numberTexture, int figureLength, UInt64 score, int waitTime ){
				mTexNumbers = numberTexture;
				mFigureLength = figureLength;
				mFigures = new int[ mFigureLength ];
				mMax = ( UInt64 )Math.Pow( 10, mFigureLength ) - 1;
				mWaitTime = waitTime;
				mRandom = new Random();

				mTotalValue = score;

				UInt64 t = mTotalValue;
				for ( int i = 0; i < mFigures.Length; i++ ){
					mFigures[ i ] = ( int )( t % 10 );
					t /= 10;
					if ( t == 0 && mNumFigure == -1 ){
						mNumFigure = i + 1;
					}
				}
			}
			public void Draw( int x, int y ){

				float w = mTexNumbers.Get[ 0 ].Width * mScaleRate;
				float h = mTexNumbers.Get[ 0 ].Height * mScaleRate;

				mDrawEnd = true;
				
				mSpriteBatch.Begin();

				for ( int i = 0; i < mFigures.Length; i++ ){
					float destX = x + i * w;
					float destY = y;
					int num = 0;
					int figureIndex = mFigures.Length - i - 1;
					if ( mCount >= mWaitTime * figureIndex ){
						if ( mCount < mWaitTime * ( figureIndex + 1 ) && figureIndex < mNumFigure ){
							num = mRandom.Next( 10 );
							mDrawEnd = false;
						}else{
							num = mFigures[ figureIndex ];
						}

						if ( figureIndex < mNumFigure ){
							mSpriteBatch.Draw( mTexNumbers.Get[ num ], new Vector2( destX, destY ), null, Color.White, 0.0f, Vector2.Zero, mScaleRate, SpriteEffects.None, 0.0f );
							//SpriteBoard.Render( mTexNumbers.Get[ num ], destX, destY, w, h, 1.0f );
						}
					}

				}

				mSpriteBatch.End();

				++mCount;
			}
			public bool DrawEnd(){
				return mDrawEnd;
			}

			public void SetScale( float scale ){
				mScaleRate = scale;
			}
		}
	}
}

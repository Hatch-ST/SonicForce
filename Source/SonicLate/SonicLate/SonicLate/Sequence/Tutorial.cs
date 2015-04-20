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
	/// チュートリアル画面
	/// </summary>
	class Tutorial : GameChild{
		private Play mPlay = null;

		private Camera mCamera;
		private Player mPlayer;
		private Stage mStage;
		private ObjectManager mObjectManager;

		private BlackWhole mBlackWhole;
		private BlackOut mBlackOut;
		private Mode mMode = Mode.Start;

		private Texture2D mTexTopGauge = null;
		private Texture2D mTexZoneGauge = null;

		private List<FadeTexture> mFadeTextures = null;

		private FadeTexture[] mTexOther = null;
		private int mTexOtherIndex = 0;

		private FadeTexture mTexLeaveBButton = null;

		private int mCount = 0;
		private bool mModeChanged = true;

		private int mTextureIndex = 0;

		private float mTotalInputAmount = 0.0f;

		private Buttons mOKButton = Buttons.A;

		private bool mOnEnd = false;

		enum Mode{
			Start,
			Controll, ControllEnd,
			Tilt, TiltEnd,
			Zone, ZoneEnd,
			Avoid, AvoidEnd,
			Combo, ComboEnd,
			HighCombo, HightComboEnd,
			End,
		}

		public Tutorial(){
			mPlay = new Play( Play.StageType.Tutorial );

			mCamera = mPlay.Camera;
			mPlayer = mPlay.Player;
			mStage = mPlay.Stage;

			mObjectManager = mPlay.ObjectManager;

			mBlackWhole = new BlackWhole( mContent );
			mBlackOut = new BlackOut( mContent );

			mBlackOut.Alpha = 1.0f;

			mFadeTextures = new List<FadeTexture>();
			mFadeTextures.Add( new FadeTexture( mContent, "Image/tutorial/tutorial_start", 30, true ) );
			mFadeTextures.Add( new FadeTexture( mContent, "Image/tutorial/tutorial_move", 30, true ) );
			mFadeTextures.Add( new FadeTexture( mContent, "Image/tutorial/tutorial_move_end", 30, true ) );
			mFadeTextures.Add( new FadeTexture( mContent, "Image/tutorial/tutorial_tilt", 30, true ) );
			mFadeTextures.Add( new FadeTexture( mContent, "Image/tutorial/tutorial_tilt_end", 30, true ) );
			mFadeTextures.Add( new FadeTexture( mContent, "Image/tutorial/tutorial_zone", 30, true ) );
			mFadeTextures.Add( new FadeTexture( mContent, "Image/tutorial/tutorial_zone_end", 30, true ) );
			mFadeTextures.Add( new FadeTexture( mContent, "Image/tutorial/tutorial_avoid", 30, true ) );
			mFadeTextures.Add( new FadeTexture( mContent, "Image/tutorial/tutorial_avoid_end", 30, true ) );
			mFadeTextures.Add( new FadeTexture( mContent, "Image/tutorial/tutorial_combo", 30, true ) );
			mFadeTextures.Add( new FadeTexture( mContent, "Image/tutorial/tutorial_combo_end", 30, true ) );
			mFadeTextures.Add( new FadeTexture( mContent, "Image/tutorial/tutorial_highcombo", 30, true ) );
			mFadeTextures.Add( new FadeTexture( mContent, "Image/tutorial/tutorial_highcombo_end", 30, true ) );

			mTexOther = new FadeTexture[ 2 ];
			mTexOther[ 0 ] = new FadeTexture( mContent, "Image/tutorial/tutorial_ganbare", 30, true );
			mTexOther[ 1 ] = new FadeTexture( mContent, "Image/tutorial/tutorial_hetakuso", 30, true );

			mTexTopGauge = mContent.Load<Texture2D>( "Image/tutorial/tutorial_gage_top" );
			mTexZoneGauge = mContent.Load<Texture2D>( "Image/tutorial/tutorial_gage_zone" );
			
			mFadeTextures[ mTextureIndex ].Alpha = 0.0f;

			mTexLeaveBButton = new FadeTexture( mContent, "Image/tutorial/tutorial_hanase" );
			mTexLeaveBButton.Alpha = 0.0f;
			mTexLeaveBButton.Out();
		}

		public override GameChild Update( GameTime time ){
			GameChild next = this;

			switch ( mMode ){
				// 開始
				case Mode.Start :
					if ( mModeChanged ){
						mBlackOut.Open();
						mFadeTextures[ mTextureIndex ].In();
						mModeChanged = false;
						mOnEnd = false;
					}
					
					if ( mCount > 10 ){
						// 押したら進む
						if ( mCount > 300 ){
							mFadeTextures[ mTextureIndex ].Out();
						}

						// アルファが 0なら次へ
						if ( mFadeTextures[ mTextureIndex ].Alpha == 0.0f ){
							ChangeMode( Mode.Controll );
						}
					}

					break;
				// 移動させる
				case Mode.Controll :
					if ( mModeChanged ){
						++mTextureIndex;
						mFadeTextures[ mTextureIndex ].In();
						mModeChanged = false;
						mOnEnd = false;
					}
					
					// ちょっと待つ
					if ( mCount > 10 ){
						if ( InputManager.IsJustButtonDown( PlayerIndex.One, mOKButton ) || mCount > 300 ){
							mFadeTextures[ mTextureIndex ].Out();
						}

						// いっぱい移動したら終り
						if ( mTotalInputAmount > 180.0f && mFadeTextures[ mTextureIndex ].Alpha == 0.0f ){
							ChangeMode( Mode.ControllEnd );
						}
					}

					// スティックの移動量を保存
					mTotalInputAmount += InputManager.GetThumbSticksLeft( PlayerIndex.One ).Length();

					if ( InputManager.IsKeyDown( Keys.Left ) || InputManager.IsKeyDown( Keys.Right ) || InputManager.IsKeyDown( Keys.Up ) || InputManager.IsKeyDown( Keys.Down ) ){
						++mTotalInputAmount;
					}

					break;
				// 移動した
				case Mode.ControllEnd :
					if ( mModeChanged ){
						++mTextureIndex;
						mFadeTextures[ mTextureIndex ].In();
						mModeChanged = false;
						mOnEnd = true;
					}

					if ( mCount > 10 ){
						// 押したら消す
						if ( mCount > 240 ){
							mFadeTextures[ mTextureIndex ].Out();
						}

						// ブラックアウト
						if ( mFadeTextures[ mTextureIndex ].Alpha == 0.0f ){
							mBlackOut.Close();
						}

						// ブラックアウトが終わったら次へ
						if ( mBlackOut.Closed ){
							ChangeMode( Mode.Tilt );

							mStage.Initialize();
							mObjectManager.LoadObjectData( mContent, "objectData_tutorial", "collisionModelObjects_tutorial" );
							mPlay.Initialize();
						}
					}

					break;
				// 傾けさせる
				case Mode.Tilt :
					if ( mModeChanged ){
						mBlackOut.Open();

						++mTextureIndex;
						mFadeTextures[ mTextureIndex ].In();
						mModeChanged = false;
						mOnEnd = false;
					}

					if ( mBlackOut.Opened && mCount > 10 ){
						// 押したら消す
						if ( InputManager.IsJustButtonDown( PlayerIndex.One, mOKButton ) || mCount > 600 ){
							mFadeTextures[ mTextureIndex ].Out();
						}

						// 特定のカメラまで進んだら次へ
						if ( mStage.CurrentCameraIndex > 5 ){
						    mFadeTextures[ mTextureIndex ].Out();

						    if ( mFadeTextures[ mTextureIndex ].Alpha == 0.0f ){
						        ChangeMode( Mode.TiltEnd );
						    }
						}

					}

					break;
				// 傾けた
				case Mode.TiltEnd :
					if ( mModeChanged ){
						++mTextureIndex;
						mFadeTextures[ mTextureIndex ].In();
						mModeChanged = false;
						mOnEnd = true;
					}

					// ちょっとしたら次へ
					if ( mCount > 10 ){
						// 押したら進む
						if ( mCount > 240 ){
							mFadeTextures[ mTextureIndex ].Out();
						}

						// アルファが 0ならブラックアウト
						if ( mFadeTextures[ mTextureIndex ].Alpha == 0.0f ){
							mBlackOut.Close();
						}

						// ブラックアウトが終わったら次へ
						if ( mBlackOut.Closed ){
							ChangeMode( Mode.Zone );

							mObjectManager.ClearObject();
							mObjectManager.LoadEnemyData( mContent, "enemyData_tutorial_1" );
							mPlay.Initialize();
						}
					}

					break;
				// ゾーンさせる
				case Mode.Zone :
					if ( mModeChanged ){
						mBlackOut.Open();

						++mTextureIndex;
						mFadeTextures[ mTextureIndex ].In();
						mModeChanged = false;

						mTotalInputAmount = 0;
						mOnEnd = false;
					}

					if ( mBlackOut.Opened && mCount > 10 ){
						// 押したら進む
						if ( InputManager.IsJustButtonDown( PlayerIndex.One, mOKButton ) || mCount > 300 ){
							mFadeTextures[ mTextureIndex ].Out();
						}

						// ゾーン中ならカウント
						if ( mPlayer.OnSlow() ){
							mTotalInputAmount += 1.0f;
						}

						// いっぱいゾーンしたら次へ
						if ( mTotalInputAmount > 60.0f ){
							mFadeTextures[ mTextureIndex ].Out();

							if ( mFadeTextures[ mTextureIndex ].Alpha == 0.0f ){
								ChangeMode( Mode.ZoneEnd );
							}
						}
					}

					break;
				// ゾーンできた
				case Mode.ZoneEnd :
					if ( mModeChanged ){
						++mTextureIndex;
						mFadeTextures[ mTextureIndex ].In();
						mModeChanged = false;
						mOnEnd = true;
					}
					if ( mCount > 10 ){
						if ( mCount > 240 ){
							mFadeTextures[ mTextureIndex ].Out();
						}

						// アルファが 0ならブラックアウト
						if ( mFadeTextures[ mTextureIndex ].Alpha == 0.0f ){
							mBlackOut.Close();
						}

						// ブラックアウトが終わったら次へ
						if ( mBlackOut.Closed ){
							ChangeMode( Mode.Avoid );
							
							mObjectManager.LoadEnemyData( mContent, "enemyData_tutorial_1" );
							mPlay.Initialize();
						}
					}
					break;
				// ギリ避けさせる
				case Mode.Avoid :
					if ( mModeChanged ){
						mBlackOut.Open();

						++mTextureIndex;
						mFadeTextures[ mTextureIndex ].In();
						mTotalInputAmount = 0.0f;
						mModeChanged = false;
						mOnEnd = false;
					}
					if ( mBlackOut.Opened && mCount > 10 ){
						if ( InputManager.IsJustButtonDown( PlayerIndex.One, mOKButton ) || mCount > 300 ){
							mFadeTextures[ mTextureIndex ].Out();
						}

						// ギリ避けしたら次へ
						if ( mPlayer.OnAdvance() ){
							mFadeTextures[ mTextureIndex ].Out();

							mTotalInputAmount = 1.0f;
						}

						if ( mTotalInputAmount > 0.0f && mFadeTextures[ mTextureIndex ].Alpha == 0.0f ){
							ChangeMode( Mode.AvoidEnd );
						}
					}
					break;
				// ギリ避けできた
				case Mode.AvoidEnd :
					if ( mModeChanged ){
						++mTextureIndex;
						mFadeTextures[ mTextureIndex ].In();
						mModeChanged = false;
						mOnEnd = true;
					}

					if ( mCount > 10 ){
						if ( mCount > 240 ){
							mFadeTextures[ mTextureIndex ].Out();
						}

						// アルファが 0ならブラックアウト
						if ( mFadeTextures[ mTextureIndex ].Alpha == 0.0f ){
							mBlackOut.Close();
						}

						// ブラックアウトが終わったら次へ
						if ( mBlackOut.Closed ){
							ChangeMode( Mode.Combo );
							
							mObjectManager.LoadEnemyData( mContent, "enemyData_tutorial_2" );
							mPlay.Initialize();
						}
					}

					break;
				// コンボさせる
				case Mode.Combo :
					if ( mModeChanged ){
						mBlackOut.Open();

						++mTextureIndex;
						mFadeTextures[ mTextureIndex ].In();
						mTotalInputAmount = 0.0f;
						mModeChanged = false;
						mOnEnd = false;
					}

					if ( mBlackOut.Opened && mCount > 10 ){
						if ( InputManager.IsJustButtonDown( PlayerIndex.One, mOKButton ) || mCount > 300 ){
							mFadeTextures[ mTextureIndex ].Out();
						}

						// 3コンボ以上なら「Bボタンを離せ！」
						if ( mPlayer.ComboCount >= 3 ){
							mFadeTextures[ mTextureIndex ].Out();
							mTexLeaveBButton.In();
						}else{
							mTexLeaveBButton.Out();
						}

						// トップなら次へ
						if ( mPlayer.OnFast() ){
							mFadeTextures[ mTextureIndex ].Out();
							mTexLeaveBButton.Out();
							mTotalInputAmount = 1.0f;
						}

						if ( mTotalInputAmount > 0.0f && mFadeTextures[ mTextureIndex ].Alpha == 0.0f ){
							ChangeMode( Mode.ComboEnd );
						}
					}

					break;
				// コンボできた
				case Mode.ComboEnd :
					if ( mModeChanged ){
						++mTextureIndex;
						mFadeTextures[ mTextureIndex ].In();
						mModeChanged = false;
						mOnEnd = true;
					}

					if ( mCount > 10 ){
						if ( mCount > 300 ){
							mFadeTextures[ mTextureIndex ].Out();
						}

						// アルファが 0ならブラックアウト
						if ( mFadeTextures[ mTextureIndex ].Alpha == 0.0f ){
							mBlackOut.Close();
						}

						// ブラックアウトが終わったら次へ
						if ( mBlackOut.Closed ){
							ChangeMode( Mode.HighCombo );
							
							mObjectManager.LoadEnemyData( mContent, "enemyData_tutorial_3" );
							mPlay.Initialize();
						}
					}

					break;
				// 連続コンボさせる
				case Mode.HighCombo :
					if ( mModeChanged ){
						mBlackOut.Open();

						++mTextureIndex;
						mFadeTextures[ mTextureIndex ].In();
						mTotalInputAmount = 0.0f;
						mModeChanged = false;
						mOnEnd = false;
					}

					if ( mBlackOut.Opened && mCount > 10 ){
						if ( InputManager.IsJustButtonDown( PlayerIndex.One, mOKButton ) || mCount > 300 ){
							mFadeTextures[ mTextureIndex ].Out();
						}

						if ( mPlayer.ComboCount >= 5 ){
							mFadeTextures[ mTextureIndex ].Out();
							mTotalInputAmount = 1.0f;
						}

						if ( mTotalInputAmount > 0.0f && mFadeTextures[ mTextureIndex ].Alpha == 0.0f ){
							ChangeMode( Mode.HightComboEnd );
						}
					}

					break;
				// 連続コンボできた
				case Mode.HightComboEnd :
					if ( mModeChanged ){
						++mTextureIndex;
						mFadeTextures[ mTextureIndex ].In();
						mModeChanged = false;
						mOnEnd = true;
					}
					if ( mCount > 10 ){
						if ( mCount > 600 ){
							mFadeTextures[ mTextureIndex ].Out();
						}

						if ( mFadeTextures[ mTextureIndex ].Alpha == 0.0f ){
							mBlackOut.Close();
						}

						if ( mBlackOut.Closed ){
							ChangeMode( Mode.End );
						}
					}

					break;
				// 終わり
				case Mode.End :
					if ( mCount > 120 ){
						mPlay.Release();
						SoundManager.StopMusic();
						next = new Title();
					}
					break;
			}

			// 特定のカメラまで進んだ
			if ( mStage.CurrentCameraIndex > 7 && !mOnEnd ){
				// プレイヤーを殺す
				mPlayer.Kill();

				// テクスチャオン
				mTexOther[ mTexOtherIndex ].In();
			}else if ( mStage.CurrentCameraIndex >= 1 ){

				// テクスチャオフ
				mTexOther[ mTexOtherIndex ].Out();

				// テクスチャのフェードアウトが終わっていれば番号を弄る
				if ( mTexOther[ mTexOtherIndex ].Alpha == 0.0f ){
					mTexOtherIndex = mCount % 2;
				}
			}

			// Playを更新
			if ( mMode != Mode.End ){
				GameChild t = mPlay.Update( time );
				if ( t != mPlay ){
					next = t;
				}
			}

			++mCount;

			return next;
		}

		public override void Draw( GameTime time ){
			mPlay.Draw( time );

			// ゾーンゲージとトップゲージ
			if ( mMode == Mode.ZoneEnd ){
				SpriteBoard.Render( mTexZoneGauge, 33.0f, 470.0f, 1.0f );
				mPlay.DrawUI( time, false, true );
			}else if ( mMode == Mode.ComboEnd ){
				SpriteBoard.Render( mTexTopGauge, 14.0f, 577.0f, 1.0f );
				mPlay.DrawUI( time, true, false );
			}

			mFadeTextures[ mTextureIndex ].Render( 0, 0 );

			// 説明のアルファ値が0ならがんばれ
			if ( mFadeTextures[ mTextureIndex ].Alpha == 0.0f && !mOnEnd ){
				mTexOther[ 0 ].Render( 0, 0 );
				mTexOther[ 1 ].Render( 0, 0 );
			}

			mTexLeaveBButton.Render( 0, 0 );

			mBlackOut.Draw();
			mBlackWhole.Draw( mCamera, mPlayer );
		}

		private void ChangeMode( Mode nextMode ){
			mMode = nextMode;
			mModeChanged = true;
			mCount = 0;
			
			mTexOther[ 0 ].Out();
			mTexOther[ 0 ].Alpha = 0.0f;
			mTexOther[ 1 ].Out();
			mTexOther[ 1 ].Alpha = 0.0f;
		}
	}
}

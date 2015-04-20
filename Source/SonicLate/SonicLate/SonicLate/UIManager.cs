using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SonicLate {
	/// <summary>
	/// ゲーム中のユーザーインターフェイス管理クラス
	/// </summary>
	class UIManager {
		//テクスチャ
		private Texture2D[] mUIGauge; // ゲージ枠
		private Texture2D mUIGaugeZone;	// ゾーンゲージ
		private Texture2D mUIGaugeStone; // トップゲージ中央
		private MultiTexture mUIGaugeTop; // トップゲージ

		private Texture2D mUIScore; // スコア枠

		//ゾーンゲージの座標
		private const int mZoneLeftPos = 72;
		private const int mZoneRightPos = 380;

		private int mZoneGaugeDrawPos;		//ゾーンゲージの表示する座標
		private Color mZoneDrawColor;		//ゾーンゲージの表示色

		private Color mStoneDrawColor;		//トップゲージ中央の表示色

		private bool mOnFast;				//トップギアであるか
		private int mTopGaugeIndex;			//トップギアゲージの表示する画像番号

		private float mTopGaugeOneOfMax;	//トップギアゲージの1つ当たりの表示割合
		private int mTopGaugeMaxIndex;		//トップギア発動時の最大の表示個数

		private bool mTopBlink;			//トップギアゲージが点滅するか
		private const int mTopBlinkFrame = 8;	//トップギアゲージが点滅するフレーム間隔
		private bool mStoneBlink;				//中央が点滅するか
		private const int mStoneBlinkFrame = 5;	//中央が点滅するフレーム間隔
		
		private float mZoneGaugeScaleRate = 1.0f; // ゲージの拡大率
		private int mZoneScalingCount = 0;

		private float mTopGaugeScaleRate = 1.0f; // ゲージの拡大率
		private int mTopScalingCount = 0;

		private int mCount;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="content">コンテンツマネージャ</param>
		public UIManager(ContentManager content) {
			// テクスチャ読み込み
			mUIGauge = new Texture2D[ 2 ] ;
			mUIGauge[ 0 ] = content.Load<Texture2D>("Image/UI_gauge1");
			mUIGauge[ 1 ] = content.Load<Texture2D>("Image/UI_gauge2");
			mUIGaugeZone = content.Load<Texture2D>("Image/UI_gauge_zone");
			mUIGaugeStone = content.Load<Texture2D>("Image/UI_gauge_stone");
			mUIGaugeTop = new MultiTexture(content, "Image/UI_gauge_top", 11);
			mUIScore = content.Load<Texture2D>( "Image/playscore" );
			mTopGaugeIndex = 0;
			mZoneDrawColor = Color.White;
			mStoneDrawColor = Color.White;
			mOnFast = false;
			mCount = 0;
		}

		/// <summary>
		/// 更新を行う
		/// </summary>
		/// <param name="player">プレイヤー</param>
		public void Update(Player player) {
			//ゾーンゲージの処理
			//ゾーンゲージ右に減っていくバージョン
			mZoneGaugeDrawPos = (int)(mZoneRightPos - (mZoneRightPos - mZoneLeftPos) * player.ZoneGauge * 0.01);
			//ゾーン使用中のゲージ色変化
			if (player.OnSlow()) {
				mZoneDrawColor = new Color(255,150,150);

				// 拡大縮小
				int count = mZoneScalingCount % 60;
				if ( count < 30 ){
					mZoneGaugeScaleRate = 1.0f + ( count / 30.0f ) * 0.02f;
				}else{
					mZoneGaugeScaleRate = 1.0f + ( 1.0f - ( ( count - 30 ) / 30.0f ) ) * 0.02f;
				}
			} else {
				mZoneDrawColor = Color.White;
				
				// 拡大率を戻す
				mZoneGaugeScaleRate = mZoneGaugeScaleRate + ( 1.0f - mZoneGaugeScaleRate ) * 0.1f;
				mZoneScalingCount = 0;
			}

			//トップギアゲージ
			//トップギアになった瞬間
			if (player.JustTopGearChanged) {
				mTopBlink = false;
				mStoneBlink = false;
				//そのコンボでの最大発動時間を取得する
				mTopGaugeMaxIndex = Math.Max(0, Math.Min(player.TopLevel - 2, mUIGaugeTop.Get.Length - 1));
				mTopGaugeOneOfMax = player.mTopGearRecoverRatio[mTopGaugeMaxIndex-1]/mTopGaugeMaxIndex;
			}
			//トップギア時
			if (mOnFast = player.OnFast()) {
				mTopGaugeIndex = (int)Math.Max(0,Math.Min(player.TopGearGauge / mTopGaugeOneOfMax + 1, mUIGaugeTop.Get.Length - 1));
				//残りゲージ１のとき、中央を赤く点滅させる
				if (mTopGaugeIndex == 1) {
					if (mCount % mStoneBlinkFrame == 0) {
						mStoneBlink = !mStoneBlink;
					}
					if (mStoneBlink) {
						mStoneDrawColor = new Color(255, 100, 100);
					} else {
						mStoneDrawColor = new Color(255, 150, 150);
					}
				} else {
					mStoneDrawColor = Color.White;
				}

				//ゲージを点滅させる
				if (mCount % mTopBlinkFrame == 0) {
					mTopBlink = !mTopBlink;
				}
				if (mTopBlink) {
					mTopGaugeIndex--;
				}
				
				// 拡大縮小
				int count = mTopScalingCount % 60;
				if ( count < 30 ){
					mTopGaugeScaleRate = 1.0f + ( count / 30.0f ) * 0.04f;
				}else{
					mTopGaugeScaleRate = 1.0f + ( 1.0f - ( ( count - 30 ) / 30.0f ) ) * 0.04f;
				}

			} else {
				mTopGaugeIndex = Math.Max(0, Math.Min(player.TopLevel - 2, mUIGaugeTop.Get.Length - 1));
				
				// 拡大率を戻す
				mTopGaugeScaleRate = mTopGaugeScaleRate + ( 1.0f - mTopGaugeScaleRate ) * 0.1f;
				mTopScalingCount = 0;
			}

			// 死んだら拡大率を初期化
			if ( player.IsDied ){
				mZoneGaugeScaleRate = 1.0f;
				mZoneScalingCount = 0;
				mTopGaugeScaleRate = 1.0f;
				mTopScalingCount = 0;
			}
			
			mZoneScalingCount++;
			mTopScalingCount++;
			mCount++;
		}

		/// <summary>
		/// 描画を行う
		/// </summary>
		/// <param name="time">時間</param>
		public void Draw(GameTime time, SpriteBatch spriteBatch) {
			Draw( time, spriteBatch, true, true );
		}

		public void Draw(GameTime time, SpriteBatch spriteBatch, bool enableTopGauge, bool enableZoneGauge ) {
			// UIの描画
			spriteBatch.Begin();
			
			Vector2 scale, position, origin;
			scale = new Vector2( mZoneGaugeScaleRate, mZoneGaugeScaleRate );

			if ( enableZoneGauge ){
				//ゾーンゲージ
				position = new Vector2( 10.0f + mUIGauge[ 0 ].Width / 2.0f, -5.0f + GameMain.ScreenHeight - mUIGauge[ 0 ].Height + mUIGauge[ 0 ].Height / 2.0f );
				origin = new Vector2( mUIGauge[ 0 ].Width / 2.0f, mUIGauge[ 0 ].Height / 2.0f );
				spriteBatch.Draw( mUIGauge[ 0 ], position, null, Color.White, 0.0f, origin, scale, SpriteEffects.None, 0.0f );
			
				//右に減っていくバージョン
				position = new Vector2( 10.0f + mUIGaugeZone.Width / 2.0f, -5.0f + GameMain.ScreenHeight - mUIGaugeZone.Height + mUIGaugeZone.Height / 2.0f );
				Rectangle rect = new Rectangle( mZoneGaugeDrawPos, 0, mZoneRightPos, mUIGaugeZone.Height );
				origin = new Vector2( mUIGaugeZone.Width / 2.0f - mZoneGaugeDrawPos, mUIGaugeZone.Height / 2.0f );
				spriteBatch.Draw( mUIGaugeZone, position, rect, mZoneDrawColor, 0.0f, origin, scale, SpriteEffects.None, 0.0f );
			}
			if ( enableTopGauge ){
				//トップギアゲージ枠
				position = new Vector2( 10.0f, -5.0f + GameMain.ScreenHeight - mUIGauge[ 1 ].Height );
				spriteBatch.Draw( mUIGauge[ 1 ], position, Color.White );

				// トップゲージ
				scale = new Vector2( mTopGaugeScaleRate, mTopGaugeScaleRate );
				position = new Vector2( 10.0f + 74, -5.0f + GameMain.ScreenHeight - mUIGaugeTop.Get[0].Height + 162 );
				origin = new Vector2( 74, 162 );
				spriteBatch.Draw(mUIGaugeTop.Get[mTopGaugeIndex], position, null, Color.White, 0.0f, origin, scale, SpriteEffects.None, 0.0f );

				//トップギア中央
				if (mOnFast) {
					position = new Vector2( 10.0f + 74, -5.0f + GameMain.ScreenHeight - mUIGaugeStone.Height + 162 );
					origin = new Vector2( 74, 162 );
					spriteBatch.Draw(mUIGaugeStone, position, null, mStoneDrawColor, 0.0f, origin, scale, SpriteEffects.None, 0.0f );
				}
			}
			
			// スコア
			spriteBatch.Draw(mUIScore, new Vector2( GameMain.ScreenWidth - mUIScore.Width, 660.0f ), Color.White);
			// SpriteBoard.Render( mUIScore, GameMain.ScreenWidth - mUIScore.Width, 660.0f, 1.0f );

			spriteBatch.End();
		}
	}
}

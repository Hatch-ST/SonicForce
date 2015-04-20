using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Xml.Linq;

namespace SonicLate{
	/// <summary>
	/// ステージ
	/// </summary>
	class Stage{
		/// <summary>モデル</summary>
		private HLModel[] mModels = null;
		private Vector3[] mModelPositions = null;
		private ModelStates mModelStates = null;
		private const float mModelDarwRange = 50000.0f;

		private HLModel mModelBack = null;
		private ModelStates mStatesBack = null;
		private Vector3 mBackDefaultAngle = Vector3.Zero;
		private bool mDrawBackModelEnable = true;

		private HLModel mModelGround = null;
		private ModelStates mGroundStates = null;

		private HLModel mModelSea = null;
		private ModelStates mSeaStates = null;

		private HLModel[] mModelObjects = null;
		private Vector3[] mObjectPositions = null;
		private ModelStates mObjectStates = null;
		private const float mObjectDarwRange = 30000.0f;

		/// <summary>衝突用モデル</summary>
		private CollisionModel mCollisionModel = null;

		/// <summary>パーティクル</summary>
		private Particle mParticle = null;

		/// <summary>カメラ補間クラス</summary>
		private Curve[] mCameraCurve = null;

		/// <summary>使用中のカメラ補間クラスインスタンス</summary>
		private int mCurrentCameraCurveIndex = 0;

		/// <summary>使用中のカメラ番号</summary>
		private int mCameraIndex = 0;

		/// <summary>カメラ補間にかかる時間</summary>
		private float mCameraTransEndTime = 0.18f;

		/// <summary>分岐点</summary>
		private List<int[]> mBranch = null;
		
		/// <summary>復帰点</summary>
		private List<int[]> mCheckPoints = null;
		
		/// <summary>通過した分岐点の数</summary>
		private int mTransitedBranchCount = 0;

		/// <summary>通過したチェックポイントの数</summary>
		private int mTransitedCheckPointCount = 0;
		
		/// <summary>通常時のカメラ位置と注視点との距離</summary>
		private Vector3 mDefaultReferenceTranslate = new Vector3( 0.0f, 40.0f, 150.0f );

		/// <summary>カメラ反転時のカメラ位置と注視点との距離</summary>
		private Vector3 mReverseReferenceTranslate = new Vector3( 0.0f, 80.0f, -400.0f );
		
		/// <summary>カメラを反転する速さ</summary>
		private float mCameraReverseSpeed = 15.0f;
		
		/// <summary>カメラ反転が有効かどうか</summary>
		private bool mCameraReverseEnable = false;

		/// <summary>チェックポイントを通過した瞬間かどうか</summary>
		private bool mNowTransitCheckPoint = false;

		
		/// <summary>コンティニューした後瞬間かどうか</summary>
		private bool mContinued = false;
		
		/// <summary>カメラ反転が有効かどうかを取得または登録する</summary>
		public bool CameraReverseEnable{
			get { return mCameraReverseEnable; }
			set { mCameraReverseEnable = value; }
		}

		/// <summary>
		/// カメラ移動が有効かどうか
		/// </summary>
		private bool mCameraMoveEnable = true;

		/// <summary>
		/// カメラ移動が有効かどうかを取得または登録する
		/// </summary>
		public bool CameraMoveEnable{
			get { return mCameraMoveEnable; }
			set { mCameraMoveEnable = value; }
		}

		/// <summary>
		/// 進行方向ベクトル
		/// </summary>
		private Vector3 mAdvanceDirection = Vector3.Zero;

		private Vector3 mCameraPosition = Vector3.Zero;
		private Vector3 mCameraAngle = Vector3.Zero;
		private Vector3 mCameraMove = Vector3.Zero;

		private Interpolate mAvoidStopInter;
		private int mAvoidStopEndTime = 60;

		//private HLModel mCModel = null;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="content">コンテンツマネージャ</param>
		/// <param name="cameraDataFileNames">カメラ情報のXMLファイル名</param>
		public Stage( ContentManager content, Play.StageType stageType ){
			switch ( stageType ){
				case Play.StageType.Normal :
					// モデルの大きさを登録
					mModelStates = new ModelStates( null );
					mModelStates.Scale *= 28.0f;
					mModelStates.ScaleY *= 0.9f;

					// モデルの読み込み
					mModels = new HLModel[ 8 ];
					mModelPositions = new Vector3[ 8 ];
					for ( int i = 0; i < mModels.Length; i++ ){
						mModels[ i ] = new HLModel( content, "Model/Stage/stage" + Convert.ToString( i + 1 ) );
						Vector3 min = mModels[ i ].MinVertex * mModelStates.Scale;
						Vector3 max = mModels[ i ].MaxVertex * mModelStates.Scale;
						mModelPositions[ i ] = min + ( max - min ) * 0.5f;
					}

					
					// モデルの大きさを登録
					mObjectStates = new ModelStates( null );
					mObjectStates.Scale *= 28.0f;
					
					// モデルの読み込み
					mModelObjects = new HLModel[ 16 ];
					mObjectPositions = new Vector3[ 16 ];
					for ( int i = 0; i < mModelObjects.Length; i++ ){
						mModelObjects[ i ] = new HLModel( content, "Model/Stage/object/stageObject_" + Convert.ToString( i + 1 ) );
						Vector3 min = mModelObjects[ i ].MinVertex * mModelStates.Scale;
						Vector3 max = mModelObjects[ i ].MaxVertex * mModelStates.Scale;
						mObjectPositions[ i ] = min + ( max - min ) * 0.5f;
					}


					// 衝突モデルの読み込み
					mCollisionModel = new CollisionModel( content, "collisionModelStage" );
					//mCollisionModel = new CollisionModel();

					// カメラ情報を読み込む
					LoadCameraData( content, "cameraData", "cameraData_sub" );

					break;

				case Play.StageType.Tutorial :
					// モデルの大きさを登録
					mModelStates = new ModelStates( null );
					mModelStates.Scale *= 28.0f;
					mModelStates.ScaleY *= 0.9f;

					// モデルの読み込み
					mModels = new HLModel[ 1 ];
					mModelPositions = new Vector3[ 1 ];
					for ( int i = 0; i < mModels.Length; i++ ){
						mModels[ i ] = new HLModel( content, "Model/Stage/stage" + Convert.ToString( i + 1 ) );
						Vector3 min = mModels[ i ].MinVertex * mModelStates.Scale;
						Vector3 max = mModels[ i ].MaxVertex * mModelStates.Scale;
						mModelPositions[ i ] = min + ( max - min ) * 0.5f;
					}
					
					// モデルの大きさを登録
					mObjectStates = new ModelStates( null );
					mObjectStates.Scale *= 28.0f;
					
					// モデルの読み込み
					mModelObjects = new HLModel[ 4 ];
					mObjectPositions = new Vector3[ 4 ];
					for ( int i = 0; i < mModelObjects.Length; i++ ){
						mModelObjects[ i ] = new HLModel( content, "Model/Stage/object/stageObject_" + Convert.ToString( i + 1 ) );
						Vector3 min = mModelObjects[ i ].MinVertex * mModelStates.Scale;
						Vector3 max = mModelObjects[ i ].MaxVertex * mModelStates.Scale;
						mObjectPositions[ i ] = min + ( max - min ) * 0.5f;
					}

					// 衝突モデルの読み込み
					//mCollisionModel = new CollisionModel( content, "collisionModelStage" );
					mCollisionModel = new CollisionModel();

					// カメラ情報を読み込む
					LoadCameraData( content, "cameraData_tutorial" );

					break;
			}

			mModelGround = new HLModel( content, "Model/Gaia" );
			mGroundStates = new ModelStates( null );
			mGroundStates.Scale *= 28.0f;

			mModelBack = new HLModel( content, "Model/Stage/back" );
			mStatesBack = new ModelStates( null );
			mStatesBack.ScaleX = 5.0f;
			mStatesBack.ScaleY = 1.0f;
			mStatesBack.ScaleZ = 4.0f;
			mBackDefaultAngle.X = ( float )Math.PI * 0.5f;
			mBackDefaultAngle.Y = ( float )Math.PI;
			mStatesBack.Angle = mBackDefaultAngle;

			mModelSea = new HLModel( content, "Model/Stage/sea" );
			mSeaStates = new ModelStates( null );
			mSeaStates.Scale *= 28.0f;

			//// 衝突モデルの初期化
			//ModelStates colStates = new ModelStates( null );
			//colStates.Scale *= mModelStates[ 0 ].Scale;
			//mCollisionModel = new CollisionModel( content, "Model/Stage/atari", colStates );
			//mCollisionModel.Add( new HLModel( content, "Model/Stage/kujira_atari" ), colStates );
			//mCollisionModel.Partition();

			//// 衝突モデルデータを書き出す
			//mCollisionModel.SaveToXmlFile( "collisionModelStage.xml" );

			
			//mCollisionModel = new CollisionModel();
			//for ( int i = 0; i < mModels.Length - 1; i++ ){
			//    mCollisionModel.Add( mModels[ i ], mModelStates[ i ] );
			//}
			//mCollisionModel.Partition();
			//mCollisionModel.SaveToXmlFile( "collisionModelStage2.xml" );

			
			// パーティクルの初期化
			mParticle = new Particle( new BillBoard( content, "Image/awa", 4 ), 50, Particle.Type.Snow );
			mParticle.SetMaxPosition( 1500.0f, 600.0f );
			mParticle.SetScaleXYRange( 80.0f, 60.0f );
			mParticle.SetSpeedRange( 6.0f, 3.0f );
			mParticle.SetTimeRange( 120, 80 );
			mParticle.Position = new Vector3( 0.0f, -200.0f, -1000.0f );
			mParticle.Initialize();
			mParticle.Enable = true;
			mParticle.ZTestEnable = false;

			mCameraCurve[ 0 ].Get( 1.0f, out mCameraPosition, out mCameraAngle, false );

			mAvoidStopInter = new Interpolate();
			mAvoidStopInter.Time = mAvoidStopEndTime + 1;
			mAvoidStopInter.Value = 2.0f;
			mAvoidStopInter.Set( 0.0f, 2.0f );
			
			// カメラの位置から進行方向のベクトルを算出
			mAdvanceDirection = Vector3.Normalize( mCameraCurve[ 0 ].Points[ mCameraCurve[ 0 ].Points.Length -1 ].Position - mCameraCurve[ 0 ].Points[ 0 ].Position );
		}

		/// <summary>
		/// 初期化処理を行う
		/// </summary>
		public void Initialize(){
			mCameraIndex = 0;
			mCurrentCameraCurveIndex = 0;
			mTransitedBranchCount = 0;
			mTransitedCheckPointCount = 0;

			// カメラを初期化
			foreach ( Curve curve in mCameraCurve ){
				curve.Time = 0.0f;
				curve.Get( 1.0f, out mCameraPosition, out mCameraAngle, false );
			}

			mAvoidStopInter.Time = mAvoidStopEndTime + 1;
			mAvoidStopInter.Value = 2.0f;

			mDrawBackModelEnable = true;
			mContinued = false;
		}

		/// <summary>
		/// 更新を行う
		/// </summary>
		/// <param name="player">プレイヤー情報</param>
		public void Update( Camera camera, Player player ){
			mParticle.Update();
			
			// カメラ移動が有効
			if ( mCameraMoveEnable ){
				Vector3 exCameraPos = mCameraPosition;

				// プレイヤーの位置からカメラ情報を登録する
				SetCameraStatesFromPlayerPosition( camera, player );

				if ( !mContinued ){
					mCameraMove = mCameraPosition - exCameraPos;
				}else{
					mContinued = false;
				}
			}
			// カメラ移動が無効
			else{
				camera.Target = mCameraPosition;
				camera.SetRotation( mCameraAngle );
				mCameraMove = Vector3.Zero;
			}

			// カメラ反転
			if ( mCameraReverseEnable ){
				if ( camera.ReferenceTranslate.Z > mReverseReferenceTranslate.Z ){
					camera.ReferenceTranslate -= new Vector3( 0.0f, 0.0f, mCameraReverseSpeed );
				}else{
					camera.ReferenceTranslate = mReverseReferenceTranslate;
				}
			}else{
				if ( camera.ReferenceTranslate.Z < mDefaultReferenceTranslate.Z ){
					camera.ReferenceTranslate += new Vector3( 0.0f, 0.0f, mCameraReverseSpeed );
				}else{
					camera.ReferenceTranslate = mDefaultReferenceTranslate;
				}
			}
			
			// カメラが反転していれば操作を反転
			player.SetMoveXReverse( mCameraReverseEnable );

			// プレイヤーが回避したら補間をリセット
			if ( player.JustAvoid ){
			    mAvoidStopInter.Reset();
			}
			mAvoidStopInter.Get( 0.0f, 2.0f, mAvoidStopEndTime );

			// ゴールに近づくにつれて明るくする
			if ( mCameraIndex > mCameraCurve[ mCurrentCameraCurveIndex ].Count - 5 ){
				EffectManager.DepthSadow.FogColor += new Vector4( 0.01f, 0.01f, 0.01f, 0.0f );
			}
		}

		
		/// <summary>
		/// プレイヤーの位置からカメラ情報を登録する
		/// </summary>
		private void SetCameraStatesFromPlayerPosition( Camera camera, Player player ){
			//mCameraIndex = 0;  // 戻れるバージョン
			
			// 現在のカメラ
			Curve currentCurve = mCameraCurve[ mCurrentCameraCurveIndex ];

			// プレイヤーの位置に最も近い制御点までカメラ番号を進める
			Vector3 vC1P = Vector3.Zero; // カメラ1からプレイヤーへのベクトル
			Vector3 vC1C2 = Vector3.Zero; // カメラ1からカメラ2へのベクトル
			Vector3 nC1C2 = Vector3.Zero; // 正規化したカメラ1からカメラ2へのベクトル
			float length1 = 0.0f; // vC1PをvC1C2へ投影した長さ
			while ( mCameraIndex < currentCurve.Points.Length -1 ){
				vC1P = player.Position - currentCurve.Points[ mCameraIndex ].Position;
				vC1C2 = currentCurve.Points[ mCameraIndex + 1 ].Position - currentCurve.Points[ mCameraIndex ].Position;
				nC1C2 = Vector3.Normalize( vC1C2 );
				length1 = Vector3.Dot( vC1P, nC1C2 );
				// vC1PをvC1C2へ投影した長さがvC1C2の長さを超えていたら次のカメラへ
				if ( length1 > vC1C2.Length() ){
					++mCameraIndex;
				}else{
					break;
				}
			}

			// 現在のカメラを入れなおす
			currentCurve = mCameraCurve[ mCurrentCameraCurveIndex ];

			// 今分岐点にいる
			if ( mTransitedBranchCount < mBranch.Count && mCameraIndex == mBranch[ mTransitedBranchCount ][ mCurrentCameraCurveIndex ] ){
				// 全ての分岐先位置との距離を比較
				int nearCurveIndex = mCurrentCameraCurveIndex; // 最も近いカメラルート
				float minDistance = 0.0f; // 最も近いカメラとの距離
				Vector3 branchPos = currentCurve.Points[ mCameraIndex ].Position; // 現在のカメラ位置
				Vector3 vBP = player.Position - branchPos; // 分岐点からプレイヤーへのベクトル

				// 現在の分岐点での各ルートとの距離を比較する
				for ( int i = 0; i < mBranch[ mTransitedBranchCount ].Length; i++ ){
					Vector3 vBC = mCameraCurve[ i ].Points[ mBranch[ mTransitedBranchCount ][ i ] + 1 ].Position - branchPos; // 分岐点から次のカメラへのベクトル
					Vector3 nBC = Vector3.Normalize( vBC ); // 正規化した vBC
					Vector3 posBet = branchPos + nBC * Math.Abs( Vector3.Dot( nBC, vBP ) ); // 分岐点とカメラ位置の間にある vBPをnBCへ投影したベクトル
					float distance = ( player.Position - posBet ).LengthSquared(); // ルートとの距離
					// １回目なら即登録
					if ( i == 0 ){
						minDistance = distance;
						nearCurveIndex = i;
					}
					// ２回目以降なら比較して登録
					else if ( minDistance > distance ){
						minDistance = distance;
						nearCurveIndex = i;
					}
				}

				// ルートを登録
				mCurrentCameraCurveIndex = nearCurveIndex;

				// 通過した分岐点を登録
				++mTransitedBranchCount;

				// 現在のカメラを入れなおす
				currentCurve = mCameraCurve[ mCurrentCameraCurveIndex ];

				// 新たなカメラでカメラ情報を算出
				vC1P = player.Position - currentCurve.Points[ mCameraIndex ].Position;
				vC1C2 = currentCurve.Points[ mCameraIndex + 1 ].Position - currentCurve.Points[ mCameraIndex ].Position;
				nC1C2 = Vector3.Normalize( vC1C2 );
				length1 = Vector3.Dot( vC1P, nC1C2 );
			}

			// 長さが負ならvC1PとC1C2のなす角は90度以上なので0にする
			length1 = Math.Max( length1, 0.0f );

			// 時間を算出
			float t = currentCurve.Points[ mCameraIndex ].Time + length1;

			if ( mCameraIndex < currentCurve.Points.Length -2 ){
				 // カメラ2からプレイヤーへのベクトル
			    Vector3 vC2P = player.Position - currentCurve.Points[ mCameraIndex +1 ].Position;

				// 正規化したカメラ2からカメラ3へのベクトル
			    Vector3 nC2C3 = Vector3.Normalize( currentCurve.Points[ mCameraIndex +2 ].Position - currentCurve.Points[ mCameraIndex +1 ].Position );

				// vC2PをvC2C3へ投影した長さ
				float length2 = Vector3.Dot( vC2P, nC2C3 );
				
				// 長さが負ならvC2PとC2C3のなす角は90度以上なので0にする
				length2 = Math.Max( length2, 0.0f );

				// 時間に加算
				t += length2;
			}

			// カメラ情報を取得
			Vector3 nextPos, nextAngle;
			currentCurve.GetFromTime( t, out nextPos, out nextAngle, false );

			// カメラの位置と角度を算出
			mCameraPosition = mCameraPosition + ( nextPos - mCameraPosition ) * mCameraTransEndTime;
			mCameraAngle = mCameraAngle + ( nextAngle - mCameraAngle ) * mCameraTransEndTime;

			// 注視点を登録
			camera.Target = mCameraPosition;

			// カメラを回転
			camera.SetRotation( mCameraAngle );


			// 前回のチェックポイント通過情報
			int exTransitedCheckPointCount = mTransitedCheckPointCount;

			// 通過したチェックポイント情報を登録
			mTransitedCheckPointCount = 0;
			for ( int i = 0; i < mCheckPoints.Count(); i++ ){
				if ( mCameraIndex >= mCheckPoints[ i ][ mCurrentCameraCurveIndex ] ){
					++mTransitedCheckPointCount;
				}else{
					break;
				}
			}

			// チェックポイントを通過した瞬間
			if ( exTransitedCheckPointCount != mTransitedCheckPointCount && exTransitedCheckPointCount > 0 ){
				mNowTransitCheckPoint = true;
			}else{
				mNowTransitCheckPoint = false;
			}
		}

		/// <summary>
		/// 描画を行う
		/// </summary>
		/// <param name="camera">カメラ</param>
		/// <param name="time">ゲームタイム</param>
		public void Draw( Camera camera, GameTime time, EffectManager.Type type ){
			// ステージを描画
			for ( int i = 0; i < mModelPositions.Length; i++ ){
				if ( Collision.TestIntersectShere( camera.Position, 1.0f, mModelPositions[ i ], mModelDarwRange ) ){
					// 回避中は暗くする
					Vector4 ambient = Vector4.One * 0.5f;
					if ( mAvoidStopInter.Value < 1.0f ){
						ambient *= ( 1.0f - mAvoidStopInter.Value ) - 0.5f;
					}else{
						ambient *= mAvoidStopInter.Value - 1.0f;
					}
					ambient.W = 0.0f;

					// レンダリング
					mModels[ i ].Render( camera, mModelStates, time, 0.0f, ambient, null, true, type );
				}
			}

			// サンゴを描画
			for ( int i = 0; i < mObjectPositions.Length; i++ ){
				if ( Collision.TestIntersectShere( camera.Position, 1.0f, mObjectPositions[ i ], mObjectDarwRange ) ){
					Vector4 ambient = new Vector4( 0.5f, 0.5f, 1.0f, 1.0f );
					mModelObjects[ i ].RenderLinearWrap( camera, mObjectStates, time, 1.0f, ambient, null, true, EffectManager.Type.Fog );
				}
			}

			// 地面を描画
			if ( mDrawBackModelEnable ){
				mModelGround.RenderLinearWrap( camera, mGroundStates, time, EffectManager.Type.Fog );

				// 背景を描画
				if ( !Config.IsSpecialMode ){
				    mStatesBack.Position = mCameraPosition;
				    mStatesBack.Angle = mBackDefaultAngle + new Vector3( -mCameraAngle.X, mCameraAngle.Y, 0.0f );
				    mModelBack.Render( camera, mStatesBack, time, EffectManager.Type.Fog );
				}
			}

			// 海面を描画
			mModelSea.RenderLinearWrap( camera, mSeaStates, time, EffectManager.Type.Fog );
		}

		/// <summary>
		/// コンティニュー時の処理
		/// </summary>
		public void Continue( Camera camera, Player player, GameTime time ){
			mCameraIndex = mCheckPoints[ mTransitedCheckPointCount - 1 ][ mCurrentCameraCurveIndex ];
			// 通過した分岐点情報を登録
			mTransitedBranchCount = 0;
			for ( int i = 0; i < mBranch.Count(); i++ ){
				if ( mCameraIndex >= mBranch[ i ][ mCurrentCameraCurveIndex ] ){
					++mTransitedBranchCount;
				}else{
					break;
				}
			}
			mAvoidStopInter.Time = mAvoidStopEndTime + 1;
			mAvoidStopInter.Value = 2.0f;
			mCameraReverseEnable = false;
			camera.ReferenceTranslate = mDefaultReferenceTranslate;
			player.Position = mCameraCurve[ mCurrentCameraCurveIndex ].Points[ mCameraIndex ].Position;
			mCameraPosition = mCameraCurve[ mCurrentCameraCurveIndex ].Points[ mCameraIndex ].Position;
			mCameraAngle = mCameraCurve[ mCurrentCameraCurveIndex ].Points[ mCameraIndex ].Angle;
			mCameraMove = Vector3.Zero;
			SetCameraStatesFromPlayerPosition( camera, player );

			mDrawBackModelEnable = true;
			mContinued = true;
		}

		/// <summary>
		/// パーティクルの描画を行う
		/// </summary>
		/// <param name="device">グラフィックスデバイス</param>
		/// <param name="camera">カメラ</param>
		public void DrawParticle( GraphicsDevice device, Camera camera ){
			mParticle.Render( device, camera );
		}

		/// <summary>
		/// XMLファイルからカメラ情報を登録する
		/// </summary>
		/// <param name="content">コンテンツマネージャ</param>
		/// <param name="fileName">ファイル名</param>
		private void LoadCameraData( ContentManager content, params string[] fileNames ){

			// カメラ補間を初期化
			mCameraCurve = new Curve[ fileNames.Length ];
			
			// 分岐点を初期化
			mBranch = new List<int[]>();
			mCheckPoints = new List<int[]>();

			for ( int i = 0; i < mCameraCurve.Length; i++ ){
				mCameraCurve[ i ] = new Curve();
			
				XDocument doc = XDocument.Parse( content.Load<string>( fileNames[ i ] ) );
				//XDocument doc = XDocument.Load( fileNames[ i ] + ".xml" );

				Vector3 exPosition = Vector3.Zero;
				int itemCount = 0;
				int branchCount = 0;
				int checkPosintCount = 0;
				foreach ( XElement node in doc.Elements() ){
					if ( node.Name == "CameraData" ){
						foreach ( XElement item in node.Elements() ){
							if ( item.Name == "item" ){
								string sPosition = item.Attribute( "position" ).Value;
								string sAngle = item.Attribute( "angle" ).Value;
								string sTime = "";

								string[] p = sPosition.Split( ',' );
								Vector3 position = new Vector3( Convert.ToSingle( p[ 0 ] ), Convert.ToSingle( p[ 1 ] ), Convert.ToSingle( p[ 2 ] ) );

								float time;
								if ( sTime == "" ){
									if ( mCameraCurve[ i ].Count == 0 ){
										time = 0.0f;
									}else{
										// 前回の点からの距離で時間を登録
										float distance = ( position - exPosition ).Length();
										time = distance;// / 800;
									}
								}else{
									time = Convert.ToSingle( sTime );
								}
								exPosition = position;

								// 角度情報がある
								if ( sAngle != "" ){
									string[] a = sAngle.Split( ',' );
									Vector3 angle = new Vector3( Convert.ToSingle( a[ 0 ] ), Convert.ToSingle( a[ 1 ] ), Convert.ToSingle( a[ 2 ] ) );

									// 制御点を追加
									mCameraCurve[ i ].Add( time, position, angle );
								}
								// 角度情報がない
								else{
									// 角度なしで制御点を追加
									mCameraCurve[ i ].Add( time, position, null );
								}

								++itemCount;
							}else if ( item.Name == "branch" ){
								// １回目ならリストに新しく追加
								if ( i == 0 ){
									int[] points = new int[ mCameraCurve.Length ];
									points[ 0 ] = itemCount - 1;
									mBranch.Add( points );
								}else{
									mBranch[ branchCount ][ i ] = itemCount - 1;
								}
								++branchCount;
							}else if ( item.Name == "checkPoint" ){
								// １回目ならリストに新しく追加
								if ( i == 0 ){
									int[] points = new int[ mCameraCurve.Length ];
									points[ 0 ] = itemCount;
									mCheckPoints.Add( points );
								}else{
									mCheckPoints[ checkPosintCount ][ i ] = itemCount;
								}
								++checkPosintCount;
							}
						}
					}
				}

				mCameraCurve[ i ].InterpolateAll();
			}
		}

		/// <summary>
		/// カメラをひとつ進める
		/// </summary>
		public void AdvanceCameraIndex(){
			if ( mCameraIndex < mCameraCurve[ mCurrentCameraCurveIndex ].Count - 1 ){
				++mCameraIndex;
				mCameraPosition = mCameraCurve[ mCurrentCameraCurveIndex ].Points[ mCameraIndex + 1 ].Position;
				mCameraAngle = mCameraCurve[ mCurrentCameraCurveIndex ].Points[ mCameraIndex + 1 ].Angle;
			}
		}

		/// <summary>
		/// モデルの解放処理
		/// </summary>
		public void Release(){
			for ( int i = 0; i < mModels.Length; i++ ){
				mModels[ i ].Release();
			}
			for ( int i = 0; i < mModelObjects.Length; i++ ){
				mModelObjects[ i ].Release();
			}
			mModelGround.Release();
			mModelBack.Release();
			mModelSea.Release();
		}

		/// <summary>
		/// 現在参照しているカメラの位置を取得する
		/// </summary>
		public Vector3 CurrentCameraPosition{
			get { return mCameraCurve[ mCurrentCameraCurveIndex ].Points[ mCameraIndex ].Position; }
		}

		/// <summary>
		/// パーティクルの位置を登録する
		/// </summary>
		public Vector3 ParticlePosition{
			set { mParticle.Position = value; }
		}

		/// <summary>
		/// 衝突用モデルを取得する
		/// </summary>
		public CollisionModel CollisionModel{
			get { return mCollisionModel; }
		}

		/// <summary>
		/// カメラ位置を取得する
		/// </summary>
		public Vector3 CameraPosition{
			get { return mCameraPosition; }
			set { mCameraPosition = value; }
		}

		/// <summary>
		/// カメラの角度を取得する
		/// </summary>
		public Vector3 CameraAngle{
			get { return mCameraAngle; }
		}

		/// <summary>
		/// カメラの移動ベクトルを取得する
		/// </summary>
		public Vector3 CameraMove{
			get { return mCameraMove; }
		}

		/// <summary>
		/// 現在のカメラ番号を取得する
		/// </summary>
		public int CurrentCameraIndex{
			get { return mCameraIndex; }
		}

		/// <summary>
		/// クリアしたかどうかを取得する
		/// </summary>
		public bool Cleared{
			get { return mCameraIndex == mCameraCurve[ mCurrentCameraCurveIndex ].Points.Count() - 1; }
		}

		/// <summary>
		/// 通過したチェックポイントの数を取得する
		/// </summary>
		public int TransitedCheckPointCount{
			get { return mTransitedCheckPointCount; }
		}

		/// <summary>
		/// チェックポイントを通過した瞬間かどうかを取得する
		/// </summary>
		public bool OnTransitCheckPoint{
			get { return mNowTransitCheckPoint; }
		}

		/// <summary>
		/// 進行方向のベクトルを取得する
		/// </summary>
		public Vector3 AdvanceDirection{
			get { return mAdvanceDirection; }
		}

		/// <summary>
		/// 背景モデルを描画するかどうかを登録する
		/// </summary>
		public bool DrawBackModelEnable{
			set { mDrawBackModelEnable = value; }
		}
	}
}

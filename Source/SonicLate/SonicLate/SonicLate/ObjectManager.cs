using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Xml.Linq;

namespace SonicLate{
	/// <summary>
	/// オブジェクト管理クラス
	/// </summary>
	class ObjectManager{
		private HLModel mModStarfish = null;
		private HLModel mModBurrfish = null;
		private HLModel mModBurrfishNeedle = null;
		private HLModel mModHeteroconger = null;
		private HLModel mModOctopus = null;
		private HLModel mModJellyfish = null;
		private HLModel mModJellyfishCore = null;
		private HLModel mModCrab = null;
		private HLModel mModShark = null;
		private HLModel mModWhole = null;
		private HLModel mModWhale = null;
		private HLModel mModBoneFish = null;

		private Texture2D[] mTexBigHeteroconger;

		private HLModel[] mModObjects = null;
		private HLModel[] mModObjectsCollision = null;
		private HLModel mModOldShip = null;
		private HLModel mModOldShipCollision = null;
		private HLModel mModSango = null;
		private HLModel[] mModFishes = null;

		private CollisionModel mObjectCollisionModel = null;

		private BillBoard mBoard = null;

		private List<Enemy> mEnemies = null;
		private List<StaticObject> mObjects = null;
		private BonusItems mBonusItems = null;
		
		private List<int> mEnemyCheckPoints = null;
		private List<int> mObjectCheckPoints = null;

		private string mCurrentEnemyDataFileName = "";

		private string mCurrentObjectDataFileName = "";
		private string mCurrentObjectCollisionDataFileName = "";

		/// <summary>
		/// 最後に取得された敵の番号
		/// </summary>
		private int mLastEnemyIndex = 0;

		/// <summary>
		/// 最後に取得されたオブジェクトの取得
		/// </summary>
		private int mLastObjectIndex = 0;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="content">コンテンツマネージャ</param>
		public ObjectManager( ContentManager content ){
			mModStarfish = new HLModel( content, "Model/hitode" );
			mModBurrfish = new HLModel( content, "Model/harisenbon" );
			mModBurrfishNeedle = new HLModel( content, "Model/togesenbon" );
			mModHeteroconger = new HLModel( content, "Model/Chinanago" );
			mModOctopus = new HLModel( content, "Model/tako" );
			mModJellyfish = new HLModel( content, "Model/kurage" );
			mModJellyfishCore = new HLModel( content, "Model/kurage_core" );
			mModCrab = new HLModel( content, "Model/kani" );
			mModShark = new HLModel( content, "Model/same" );
			mModWhole = new HLModel( content, "Model/whole" );
			mModWhale = new HLModel( content, "Model/kujira" );
			mModBoneFish = new HLModel( content, "Model/BoneFish" );

			mModObjects = new HLModel[ 5 ];
			mModObjects[ 0 ] = new HLModel( content, "Model/object/obj_1" );
			mModObjects[ 1 ] = new HLModel( content, "Model/object/obj_2" );
			mModObjects[ 2 ] = new HLModel( content, "Model/object/obj_3" );
			mModObjects[ 3 ] = new HLModel( content, "Model/object/obj_4" );
			mModObjects[ 4 ] = new HLModel( content, "Model/object/obj_5" );

			mModObjectsCollision = new HLModel[ 5 ];
			mModObjectsCollision[ 0 ] = new HLModel( content, "Model/object/obj_atari_1" );
			mModObjectsCollision[ 1 ] = new HLModel( content, "Model/object/obj_atari_2" );
			mModObjectsCollision[ 2 ] = new HLModel( content, "Model/object/obj_atari_3" );
			mModObjectsCollision[ 3 ] = new HLModel( content, "Model/object/obj_atari_4" );
			mModObjectsCollision[ 4 ] = new HLModel( content, "Model/object/obj_atari_5" );

			mModOldShip = new HLModel( content, "Model/object/OldShip" );
			mModOldShipCollision = new HLModel( content, "Model/object/OldShip_atari" );

			mModSango = new HLModel( content, "Model/object/sango" );

			mModFishes = new HLModel[ 6 ];
			mModFishes[ 0 ] = new HLModel( content, "Model/object/fish1" );
			mModFishes[ 1 ] = new HLModel( content, "Model/object/fish2" );
			mModFishes[ 2 ] = new HLModel( content, "Model/object/fish3" );
			mModFishes[ 3 ] = new HLModel( content, "Model/object/fish4" );
			mModFishes[ 4 ] = new HLModel( content, "Model/object/fish5" );
			mModFishes[ 5 ] = new HLModel( content, "Model/object/fish6" );

			mBoard = new BillBoard( content, "Image/awa", 4 );

			mBonusItems = new BonusItems();

			mTexBigHeteroconger = new Texture2D[ 3 ];
			mTexBigHeteroconger[ 0 ] = content.Load<Texture2D>( "Image/main_chinanago_boss1" );
			mTexBigHeteroconger[ 1 ] = content.Load<Texture2D>( "Image/main_chinanago_boss2" );
			mTexBigHeteroconger[ 2 ] = content.Load<Texture2D>( "Image/main_chinanago_boss3" );
		}

		/// <summary>
		/// 敵データを読み込む
		/// </summary>
		/// <param name="content">コンテンツマネージャ</param>
		/// <param name="fileName">ファイル名</param>
		/// <param name="enemies">出力先敵リスト</param>
		public void LoadEnemyData( ContentManager content, string fileName ){
			mCurrentEnemyDataFileName = fileName;

			mEnemies = new List<Enemy>();
			mEnemyCheckPoints = new List<int>();

			XDocument doc = XDocument.Parse( content.Load<string>( fileName ) );
			//XDocument doc = XDocument.Load( "enemyData.xml" );

			int itemCount = 0;
			foreach ( XElement node in doc.Elements() ){
			    if ( node.Name == "EnemyData" ){
			        foreach ( XElement item in node.Elements() ){
						if ( item.Name == "item" ){
							string type = item.Attribute( "type" ).Value;
							string sPosition = item.Attribute( "position" ).Value;
							string sAngle = item.Attribute( "angle" ).Value;

							string[] p = sPosition.Split( ',' );
							Vector3 position = new Vector3( Convert.ToSingle( p[ 0 ] ), Convert.ToSingle( p[ 1 ] ), Convert.ToSingle( p[ 2 ] ) );

							string[] a = sAngle.Split( ',' );
							Vector3 angle = new Vector3( Convert.ToSingle( a[ 0 ] ), Convert.ToSingle( a[ 1 ] ), Convert.ToSingle( a[ 2 ] ) );

							switch ( type ){
								case "starfish" :
									mEnemies.Add( new EStarfish( mModStarfish, mBoard, position, angle ) );
									break;
								case "heteroconger" :
									mEnemies.Add( new EHeteroconger( mModHeteroconger, position, angle ) );
									break;
								case "burrfish" :
									mEnemies.Add( new EBurrfish( mModBurrfish, mModBurrfishNeedle, position, angle ) );
									break;
								case "octopus_1" :
									mEnemies.Add( new EOctopus( mModOctopus, position, angle, 0 ) );
									break;
								case "octopus_2" :
									mEnemies.Add( new EOctopus( mModOctopus, position, angle, 1 ) );
									break;
								case "jellyfish" :
									mEnemies.Add( new EJellyfish( mModJellyfish, mModJellyfishCore, position, angle ) );
									break;
								case "crab" :
									mEnemies.Add( new ECrab( mModCrab, position, angle ) );
									break;
								case "shark" :
									mEnemies.Add( new EShark( mModShark, null, mBoard, position, angle ) );
									break;
								case "shark_b" :
									mEnemies.Add( new EShark( mModShark, mModBoneFish, mBoard, position, angle ) );
									break;
								case "whale" :
									mEnemies.Add( new EWhale( mModWhale, position, angle ) );
									break;
								case "bigHeteroconger_a" :
									mEnemies.Add( new EBigHeteroconger( mModHeteroconger, mTexBigHeteroconger[ 0 ], 0, position, angle ) );
									break;
								case "bigHeteroconger_b" :
									mEnemies.Add( new EBigHeteroconger( mModHeteroconger, mTexBigHeteroconger[ 1 ], 1, position, angle ) );
									break;
								case "bigHeteroconger_c" :
									mEnemies.Add( new EBigHeteroconger( mModHeteroconger, mTexBigHeteroconger[ 2 ], 2, position, angle ) );
									break;
								case "bigJellyfish" :
									mEnemies.Add( new EBigJellyfish( content, mModJellyfish, mModJellyfishCore, mModBurrfishNeedle, position, angle ) );
									break;
							}

							++itemCount;
						}else if ( item.Name == "checkPoint" ){
							mEnemyCheckPoints.Add( itemCount );
						}
			        }
			    }
			}

			//mEnemies.Clear();
			//mEnemies.Add( new EBigHeteroconger( mModHeteroconger, new Vector3( 0.0f, -5000.0f, 115500.0f ), new Vector3( 0.0f, 0.0f, 0.0f ) ) );
			//mEnemies.Add( new EBigHeteroconger( mModHeteroconger, new Vector3( 0.0f, -5000.0f, 114000.0f ), new Vector3( 0.0f, 0.0f, 0.0f ) ) );
			//mEnemies.Add( new EBigHeteroconger( mModHeteroconger, new Vector3( 0.0f, -5000.0f, 112500.0f ), new Vector3( 0.0f, 0.0f, 0.0f ) ) );
			
			//mEnemies.Add( new EShark( mModShark, mModBoneFish, mBoard, new Vector3( 0.0f, -5000.0f, 110000.0f ), Vector3.Zero ) );
			//mEnemies.Add( new EBigJellyfish( content, mModJellyfish, mModJellyfishCore, mModBurrfishNeedle, new Vector3( 0.0f, -5000.0f, 110000.0f ), new Vector3( 0.0f, 0.0f, 0.0f ) ) );

			
			//mEnemies.Add( new EOctopus( mModOctopus, mModWhole, new Vector3( 0.0f, -5000.0f, 110000.0f ), new Vector3( 0.0f, ( float )( Math.PI / 2 ), 0.0f ), 1 ) );

			// 番号を先頭に戻す
			mLastEnemyIndex = 0;

			// 穴の情報を書き出す
			//WriteWholeData( content, fileName, "collisionModelStage2" );
		}
		
		/// <summary>
		/// オブジェクトデータを読み込む
		/// </summary>
		/// <param name="content">コンテンツマネージャ</param>
		/// <param name="fileName">ファイル名</param>
		/// <param name="collisionModelFileName">衝突モデルのファイル名</param>
		public void LoadObjectData( ContentManager content, string fileName, string collisionModelFileName ){
			mCurrentObjectDataFileName = fileName;
			mCurrentObjectCollisionDataFileName = collisionModelFileName;

			mObjects = new List<StaticObject>();
			mObjectCheckPoints = new List<int>();

			XDocument doc = XDocument.Parse( content.Load<string>( fileName ) );
			//XDocument doc = XDocument.Load( fileName );
			
			int itemCount = 0;
			foreach ( XElement node in doc.Elements() ){
			    if ( node.Name == "ObjectData" ){
			        foreach ( XElement item in node.Elements() ){
						if ( item.Name == "item" ){
							string type = item.Attribute( "type" ).Value;
							string sPosition = item.Attribute( "position" ).Value;
							string sAngle = item.Attribute( "angle" ).Value;

							string[] p = sPosition.Split( ',' );
							Vector3 position = new Vector3( Convert.ToSingle( p[ 0 ] ), Convert.ToSingle( p[ 1 ] ), Convert.ToSingle( p[ 2 ] ) );

							string[] a = sAngle.Split( ',' );
							Vector3 angle = new Vector3( Convert.ToSingle( a[ 0 ] ), Convert.ToSingle( a[ 1 ] ), Convert.ToSingle( a[ 2 ] ) );

							switch ( type ){
								case "object_1" :
									mObjects.Add( new StaticObject( StaticObject.Name.Obj_1, mModObjects[ 0 ], position, angle ) );
									break;
								case "object_2" :
									mObjects.Add( new StaticObject( StaticObject.Name.Obj_2, mModObjects[ 1 ], position, angle ) );
									break;
								case "object_3" :
									mObjects.Add( new StaticObject( StaticObject.Name.Obj_3, mModObjects[ 2 ], position, angle ) );
									break;
								case "object_4" :
									mObjects.Add( new StaticObject( StaticObject.Name.Obj_4, mModObjects[ 3 ], position, angle ) );
									break;
								case "object_5" :
									mObjects.Add( new StaticObject( StaticObject.Name.Obj_5, mModObjects[ 4 ], position, angle ) );
									break;
								case "oldship" :
									mObjects.Add( new StaticObject( StaticObject.Name.OldShip, mModOldShip, position, angle ) );
									break;
								case "sango_s" :
									mObjects.Add( new StaticObject( StaticObject.Name.Sango_S, mModSango, position, angle ) );
									break;
								case "sango_m" :
									mObjects.Add( new StaticObject( StaticObject.Name.Sango_M, mModSango, position, angle ) );
									break;
								case "sango_l" :
									mObjects.Add( new StaticObject( StaticObject.Name.Sango_L, mModSango, position, angle ) );
									break;
								case "fish_1" :
									mObjects.Add( new StaticObject( StaticObject.Name.Fish_1, mModFishes[ 0 ], position, angle ) );
									break;
								case "fish_2" :
									mObjects.Add( new StaticObject( StaticObject.Name.Fish_2, mModFishes[ 1 ], position, angle ) );
									break;
								case "fish_3" :
									mObjects.Add( new StaticObject( StaticObject.Name.Fish_3, mModFishes[ 2 ], position, angle ) );
									break;
								case "fish_4" :
									mObjects.Add( new StaticObject( StaticObject.Name.Fish_4, mModFishes[ 3 ], position, angle ) );
									break;
								case "fish_5" :
									mObjects.Add( new StaticObject( StaticObject.Name.Fish_5, mModFishes[ 4 ], position, angle ) );
									break;
								case "fish_6" :
									mObjects.Add( new StaticObject( StaticObject.Name.Fish_6, mModFishes[ 5 ], position, angle ) );
									break;
								case "wholeS" :
									mObjects.Add( new StaticObject( StaticObject.Name.WholeS, mModWhole, position, angle ) );
									break;
								case "wholeL" :
									mObjects.Add( new StaticObject( StaticObject.Name.WholeL, mModWhole, position, angle ) );
									break;
							}
							++itemCount;
						}
						// チェックポイント
						else if ( item.Name == "checkPoint" ){
							mObjectCheckPoints.Add( itemCount );
						}
						// ボーナス
						else if ( item.Name == "bonus" ){
							string[] p = item.Value.Split( ',' );
							Vector3 position = new Vector3( Convert.ToSingle( p[ 0 ] ), Convert.ToSingle( p[ 1 ] ), Convert.ToSingle( p[ 2 ] ) );
							mBonusItems.Add( position );
						}
			        }
			    }
			}


			if ( collisionModelFileName != null ){
				// XMLファイルから読み込む
				mObjectCollisionModel = new CollisionModel( content, collisionModelFileName );
			}else{
				// 衝突モデルに追加
				mObjectCollisionModel = new CollisionModel();
				foreach ( StaticObject obj in mObjects ){
				    switch ( obj.Type ){
				        case StaticObject.Name.Obj_1 :
				            mObjectCollisionModel.Add( mModObjectsCollision[ 0 ], obj.States );
				            break;
				        case StaticObject.Name.Obj_2 :
				            mObjectCollisionModel.Add( mModObjectsCollision[ 1 ], obj.States );
				            break;
				        case StaticObject.Name.Obj_3 :
				            mObjectCollisionModel.Add( mModObjectsCollision[ 2 ], obj.States );
				            break;
				        case StaticObject.Name.Obj_4 :
				            mObjectCollisionModel.Add( mModObjectsCollision[ 3 ], obj.States );
				            break;
				        case StaticObject.Name.Obj_5 :
				            mObjectCollisionModel.Add( mModObjectsCollision[ 4 ], obj.States );
				            break;
				        case StaticObject.Name.OldShip :
				            mObjectCollisionModel.Add( mModOldShipCollision, obj.States );
				            break;
				    }
				}
				mObjectCollisionModel.Partition();

				// XMLに書き出す
				mObjectCollisionModel.SaveToXmlFile( "collisionModelObjects.xml" );
			}

			// 番号を先頭に戻す
			mLastObjectIndex = 0;
		}

		/// <summary>
		/// 穴の情報を書き出す
		/// </summary>
		/// <param name="content">コンテンツマネージャ</param>
		/// <param name="enemyDataFileName">敵情報のファイル名</param>
		/// <param name="collisionModelFileName">衝突モデルのファイル名</param>
		public void WriteWholeData( ContentManager content, string enemyDataFileName, string collisionModelFileName ){
			
			// 読み込み元
			XDocument doc = XDocument.Parse( content.Load<string>( enemyDataFileName ) );
			//XDocument doc = XDocument.Load( "enemyData.xml" );

			// 出力先
			XDocument outDoc = new XDocument();
			
			var dataElement = new XElement( "ObjectData" );
			int num = 1;

			CollisionModel collisionModel = new CollisionModel( content, collisionModelFileName );
			List<Enemy> enemies = new List<Enemy>();

			foreach ( XElement node in doc.Elements() ){
			    if ( node.Name == "EnemyData" ){
			        foreach ( XElement item in node.Elements() ){
						if ( item.Name == "item" ){
							string type = item.Attribute( "type" ).Value;

							if ( type == "heteroconger" ){ // || type == "octopus_1" || type == "octpus_2" ){
								string sPosition = item.Attribute( "position" ).Value;
								string sAngle = item.Attribute( "angle" ).Value;

								string[] p = sPosition.Split( ',' );
								Vector3 position = new Vector3( Convert.ToSingle( p[ 0 ] ), Convert.ToSingle( p[ 1 ] ), Convert.ToSingle( p[ 2 ] ) );

								string[] a = sAngle.Split( ',' );
								Vector3 angle = new Vector3( Convert.ToSingle( a[ 0 ] ), Convert.ToSingle( a[ 1 ] ), Convert.ToSingle( a[ 2 ] ) );

								Vector3 direction = new Vector3( 0.0f, 0.0f, 1.0f );
								Matrix conv = Matrix.CreateRotationZ( angle.Z ) * Matrix.CreateRotationX( angle.X ) * Matrix.CreateRotationY( angle.Y );
								direction = Vector3.Normalize( Vector3.Transform( direction, conv ) );

								Vector3 cross, normal;
								Vector3[] vertices;
								ModelStates states = new ModelStates( null );
								collisionModel.IsIntersect( position - direction * 1000.0f, position + direction * 1000.0f, out cross, out vertices, out normal  );
								states.Position = cross + direction * 20.0f;
								states.SetAngleFromDirection( direction );
								states.AngleX += ( float )Math.PI * 0.5f;

								string typeName = "";
								switch ( type ){
									case "heteroconger" :
										typeName = "wholeS";
										break;
									case "octopus_1" :
									case "octopus_2" :
										typeName = "wholeL";
										break;
								}
														
								var itemElement = new XElement("item");

								itemElement.Add( new XAttribute( "id", Convert.ToString( num++ ) ) );
								itemElement.Add( new XAttribute( "type", typeName ) );

								string strPosition = states.Position.X + ", " + states.Position.Y + ", " + states.Position.Z;
								itemElement.Add( new XAttribute("position", strPosition ) );

								string strAngle = states.Angle.X + ", " + states.Angle.Y + ", " + states.Angle.Z;
								itemElement.Add( new XAttribute( "angle", strAngle ) );

								dataElement.Add( itemElement );
							}
						}
			        }
			    }
			}
			
			outDoc.Add( dataElement );

			// 書き出し
			outDoc.Save( @"wholeData.xml" );
		}

		/// <summary>
		/// 指定した番号の敵を取得する
		/// </summary>
		/// <param name="index">敵の番号</param>
		/// <returns>敵インスタンスへの参照</returns>
		public Enemy GetEnemy( int index ){
			if ( index >= 0 && index < mEnemies.Count - 1 ){
				Enemy next = mEnemies[ index ];

				return next;
			}else{
				return null;
			}
		}

		/// <summary>
		/// まだ取得されていない次の敵インスタンスを取得する
		/// </summary>
		/// <returns>敵インスタンスへの参照</returns>
		public Enemy GetNextEnemy(){
			if ( mLastEnemyIndex < mEnemies.Count ){
				Enemy next = mEnemies[ mLastEnemyIndex ];
				++mLastEnemyIndex;
		
				return next;
			}else{
				return null;
			}
		}
		
		/// <summary>
		/// まだ取得されていない次のオブジェクトインスタンスを取得する
		/// </summary>
		/// <returns>オブジェクインスタンスへの参照</returns>
		public StaticObject GetNextObject(){
			if ( mLastObjectIndex < mObjects.Count ){
				StaticObject next = mObjects[ mLastObjectIndex ];
				++mLastObjectIndex;

				return next;
			}else{
				return null;
			}
		}

		/// <summary>
		/// チェックポイントへ移動する
		/// </summary>
		public void MoveToCheckPoint( int checkPoint ){
			if ( mEnemyCheckPoints == null ){
				mLastEnemyIndex = 0;
			}else{
				mLastEnemyIndex = mEnemyCheckPoints[ checkPoint ];
			}

			if ( mObjectCheckPoints == null ){
				mLastObjectIndex = 0;
			}else{
				mLastObjectIndex = mObjectCheckPoints[ checkPoint ];
			}
		}

		public int numLeftEnemy{
			get{ return mEnemies.Count - mLastEnemyIndex; }
		}

		public int numLeftObject{
			get { return mObjects.Count - mLastObjectIndex; }
		}

		/// <summary>
		/// 次に取得する敵の番号を登録する
		/// </summary>
		public void SetNextEnemyIndex( int index ){
			mLastEnemyIndex = index;
		}
		
		/// <summary>
		/// 敵の数を取得する
		/// </summary>
		public int EnemyCount{
			get { return mEnemies.Count; }
		}

		/// <summary>
		/// オブジェクトの数を取得する
		/// </summary>
		public int ObjectCount{
			get { return mObjects.Count; }
		}

		/// <summary>
		/// オブジェトの衝突モデルを取得する
		/// </summary>
		public CollisionModel ObjectCollisionModel{
			get { return mObjectCollisionModel; }
		}

		/// <summary>
		/// モデルの解放処理
		/// </summary>
		public void Release(){
			mModStarfish.Release();
			mModBurrfish.Release();
			mModBurrfishNeedle.Release();
			mModHeteroconger.Release();
			mModOctopus.Release();
			mModJellyfish.Release();
			mModJellyfishCore.Release();
			mModCrab.Release();
			mModShark.Release();
			mModWhole.Release();
			mModWhale.Release();
			mModBoneFish.Release();
			foreach ( HLModel model in mModObjects ){
				model.Release();
			}
			foreach ( HLModel model in mModObjectsCollision ){
				model.Release();
			}
			mModOldShip.Release();
			mModOldShipCollision.Release();
			foreach ( HLModel model in mModFishes ){
				model.Release();
			}
			mModSango.Release();
		}

		/// <summary>
		/// オブジェクトを全て削除する
		/// </summary>
		public void ClearObject(){
			mObjects = new List<StaticObject>();
			mObjectCollisionModel = new CollisionModel();
			mLastObjectIndex = 0;
			mCurrentObjectDataFileName = "";
			mCurrentObjectCollisionDataFileName = "";
		}

		/// <summary>
		/// 敵を全て削除する
		/// </summary>
		public void ClearEnemy(){
			mEnemies = new List<Enemy>();
			mLastEnemyIndex = 0;
			mCurrentEnemyDataFileName = "";
		}
		
		/// <summary>
		/// 敵を読み込みし直す
		/// </summary>
		public void ReloadEnemy( ContentManager content ){
			if ( mCurrentEnemyDataFileName != "" ){
				LoadEnemyData( content, mCurrentEnemyDataFileName );
			}else{
				ClearEnemy();
			}
		}

		/// <summary>
		/// オブジェクトを読み込みし直す
		/// </summary>
		public void ReloadObject( ContentManager content ){
			if ( mCurrentObjectDataFileName != "" ){
				LoadObjectData( content, mCurrentObjectDataFileName, mCurrentObjectCollisionDataFileName );
			}else{
				ClearObject();
			}
		}

		/// <summary>
		/// ボーナスアイテムリストを取得する
		/// </summary>
		public BonusItems BonusItems{
			get { return mBonusItems; }
		}
	}
}

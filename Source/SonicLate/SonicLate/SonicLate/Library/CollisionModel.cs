using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Xml.Linq;

namespace SonicLate{
	/// <summary>
	/// 衝突モデル
	/// </summary>
	class CollisionModel{

		#region フィールド
		/// <summary>
		/// モデルの面リスト
		/// </summary>
		private List<Face> mFaces;

		/// <summary>
		/// 面をまとめたエリア情報
		/// </summary>
		private List<Face>[] mAreas;

		/// <summary>
		/// X軸での分割数
		/// </summary>
		private const int mNumPartingLineX = 5;
		
		/// <summary>
		/// Y軸での分割数
		/// </summary>
		private const int mNumPartingLineY = 5;
		
		/// <summary>
		/// Z軸での分割数
		/// </summary>
		private const int mNumPartingLineZ = 50;

		/// <summary>
		/// 最小の点
		/// </summary>
		private Vector3 mMinVertex;

		/// <summary>
		/// 最大の点
		/// </summary>
		private Vector3 mMaxVertex;

		/// <summary>
		/// 分割された１つのエリアの大きさ
		/// </summary>
		private Vector3 mAreaScale;

		/// <summary>
		/// 計算用配列
		/// </summary>
		private int[] mWorkArray = new int[ 64 ];

		#endregion

		#region Class Face

		/// <summary>
		/// ポリゴンクラス
		/// </summary>
		class Face : IComparable{
			private Vector3[] mVertices = new Vector3[ 3 ];
			private Vector3 mMin;
			private Vector3 mMax;
			private Vector3 mNormal;

			/// <summary>
			/// 属しているエリア番号
			/// </summary>
			private List<int> mAreaIndexes = new List<int>();
			
			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="vertices">面を形成する3頂点</param>
			public Face( Vector3[] vertices ){
				SetVertices( vertices );
			}

			/// <summary>
			/// コンストラクタ
			/// </summary>
			public Face(){
			}

			/// <summary>
			/// 頂点情報を取得または登録する
			/// </summary>
			public Vector3[] Vertices{
				get { return mVertices; }
				set { mVertices = value; }
			}

			/// <summary>
			/// 頂点情報を登録する
			/// </summary>
			public void SetVertices( Vector3[] vertices ){
				mVertices = vertices;

				// 法線を登録する
				Vector3 a = mVertices[ 2 ] - mVertices[ 0 ];
				Vector3 b = mVertices[ 1 ] - mVertices[ 0 ];
				mNormal = Vector3.Cross( a, b );
				mNormal.Normalize();

				//// 法泉を反転
				//mNormal = -Normal;
					
				// 最大点と最小点を算出する
				mMax = Vertices[ 0 ];
				mMin = Vertices[ 0 ];
				for ( int i = 1; i < 3; i++ ){
					mMax.X = Math.Max( mMax.X, mVertices[ i ].X );
					mMax.Y = Math.Max( mMax.Y, mVertices[ i ].Y );
					mMax.Z = Math.Max( mMax.Z, mVertices[ i ].Z );

					mMin.X = Math.Min( mMin.X, mVertices[ i ].X );
					mMin.Y = Math.Min( mMin.Y, mVertices[ i ].Y );
					mMin.Z = Math.Min( mMin.Z, mVertices[ i ].Z );
				}
			}

			/// <summary>
			/// エリア番号を比較
			/// </summary>
			public int CompareTo( object obj ){
				int a = mAreaIndexes[ 0 ];
				int b = ( ( Face )obj ).AreaIndexes[ 0 ];
				return a - b;
			}

			/// <summary>
			/// 最大点
			/// </summary>
			public Vector3 Max{
				get { return mMax; }
				set { mMax = value; }
			}
			/// <summary>
			/// 最小点
			/// </summary>
			public Vector3 Min{
				get { return mMin; }
				set { mMin = value; }
			}
			
			/// <summary>
			/// 法線
			/// </summary>
			public Vector3 Normal{
				get { return mNormal; }
				set { mNormal = value; }
			}

			/// <summary>
			/// エリア番号
			/// </summary>
			public List<int> AreaIndexes{
				get { return mAreaIndexes; }
				set { mAreaIndexes = value; }
			}
		}

		#endregion

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="content">コンテンツマネージャ</param>
		/// <param name="fileName">ファイル名</param>
		/// <param name="states">モデルの状態</param>
		public CollisionModel( ContentManager content, string fileName, ModelStates states ){
			mFaces = new List<Face>();
			Add( new HLModel( content, fileName ), states );
			Partition();
		}

		/// <summary>
		/// コンストラクタ XMLファイルから情報を読み込む
		/// </summary>
		/// <param name="content">コンテンツマネージャ</param>
		/// <param name="fileName">ファイル名</param>
		public CollisionModel( ContentManager content, string fileName ){
			SetFacesFromXmlFile( content, fileName );
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public CollisionModel(){
			mFaces = new List<Face>();
		}

		/// <summary>
		/// モデルの状態を追加する
		/// 全ての面情報が計算し直される
		/// </summary>
		/// <param name="model">モデル</param>
		/// <param name="states">モデルの状態</param>
		public void Add( HLModel model, ModelStates states ){
			// 全ての面情報を取得
			List<Vector3[]> faces;
			model.GetFaces( out faces, states );

			// 面情報を登録
			foreach ( Vector3[] face in faces ){
				mFaces.Add( new Face( face ) );
			}
		}

		/// <summary>
		/// モデルを分割する
		/// </summary>
		public void Partition(){
			if ( mFaces.Count == 0 ) return;

			// 最大点と最小点を調べる
			mMinVertex = mFaces[ 0 ].Vertices[ 0 ];
			mMaxVertex = mFaces[ 0 ].Vertices[ 0 ];
			foreach ( Face face in mFaces ){
				for ( int i = 0; i < 3; i++ ){
					mMaxVertex.X = Math.Max( mMaxVertex.X, face.Vertices[ i ].X );
					mMaxVertex.Y = Math.Max( mMaxVertex.Y, face.Vertices[ i ].Y );
					mMaxVertex.Z = Math.Max( mMaxVertex.Z, face.Vertices[ i ].Z );

					mMinVertex.X = Math.Min( mMinVertex.X, face.Vertices[ i ].X );
					mMinVertex.Y = Math.Min( mMinVertex.Y, face.Vertices[ i ].Y );
					mMinVertex.Z = Math.Min( mMinVertex.Z, face.Vertices[ i ].Z );
				}
			}

			// 各エリアの大きさを算出
			mAreaScale.X = ( mMaxVertex - mMinVertex ).X / mNumPartingLineX;
			mAreaScale.Y = ( mMaxVertex - mMinVertex ).Y / mNumPartingLineY;
			mAreaScale.Z = ( mMaxVertex - mMinVertex ).Z / mNumPartingLineZ;

			// エリアリストを初期化
			int numArea = mNumPartingLineX * mNumPartingLineY * mNumPartingLineZ;
			mAreas = new List<Face>[ numArea ];
			for ( int i = 0; i < numArea; i++ ){
				mAreas[ i ] = new List<Face>();
			}

			// 全ての面がどのエリアに属しているかを調べる
			// エリア番号は小さい順に並ぶ
			foreach ( Face face in mFaces ){
				for ( int z = 0; z < mNumPartingLineZ; z++ ){
					for ( int y = 0; y < mNumPartingLineY; y++ ){
						for ( int x = 0; x < mNumPartingLineX; x++ ){
							// エリアの最小点と最大点
							Vector3 min = mMinVertex + new Vector3( mAreaScale.X * x, mAreaScale.Y * y, mAreaScale.Z * z );
							Vector3 max = min + mAreaScale;

							OBB obb = new OBB( min, max );
							obb.Position = min + mAreaScale / 2;

							// 面との衝突があればそのエリアを登録する
							if ( obb.IsIntersect( face.Vertices[ 0 ], face.Vertices[ 1 ], face.Vertices[ 2 ] ) ){
								int index = y * mNumPartingLineX + z * ( mNumPartingLineX * mNumPartingLineY ) + x;
								// 面に追加
								face.AreaIndexes.Add( index );

								// エリアに面を追加
								mAreas[ index ].Add( face );
							}
						}
					}
				}
			}

			// エリア番号順にソート
			mFaces.Sort();
		}

		/// <summary>
		/// XML形式でモデル情報を保存する
		/// </summary>
		/// <param name="destinationPath">保存先のファイル名</param>
		public void SaveToXmlFile( string fileName ){
			if ( mFaces.Count == 0 ) return;

			XDocument doc = new XDocument( new XDeclaration( "1.0", "utf-8", "yes" ) );

			// <CollisionModel>
			var dataElement = new XElement( "CollisionModel" );

			// 全ての面を調べる
			for ( int i = 0; i < mFaces.Count; i++ ){
				Face face = mFaces[ i ];

				string str = "";
				for ( int j = 0; j < 3; j++ ){
					// 頂点情報を取得して文字列を作成
					str += ConvertToString( face.Vertices[ j ].X, 1 ) + ",";
					str += ConvertToString( face.Vertices[ j ].Y, 1 ) + ",";
					str += ConvertToString( face.Vertices[ j ].Z, 1 ) + ",";
				}

				// 法線情報を取得して文字列に加算
				str += ConvertToString( face.Normal.X, 4 ) + ",";
				str += ConvertToString( face.Normal.Y, 4 ) + ",";
				str += ConvertToString( face.Normal.Z, 4 ) + ",";

				// エリア番号を取得して文字列に加算
				for ( int j = 0; j < face.AreaIndexes.Count; j++ ){
					if ( j > 0 ) str += ",";
					str += Convert.ToString( face.AreaIndexes[ j ] );
				}

				// <item> CollisionModelDataエレメントにfaceエレメントを追加
				dataElement.Add( new XElement( "f", str ) );
			}

			// ドキュメントにCollisionModelDataエレメントを追加
			doc.Add( dataElement );

			// ドキュメントを保存
			doc.Save( @fileName );
		}

		/// <summary>
		/// 小数点以下の桁数を指定して文字列に変換する
		/// </summary>
		private string ConvertToString( float value, int numFigure ){
			string t = Convert.ToString( value );
			int n = t.LastIndexOf( "." ) + 1;
			if ( n > 0 && t.Length > n + numFigure ){
				t = t.Remove( n + numFigure );
			}
			return t;
		}

		/// <summary>
		/// XML形式のモデルデータを読み込む
		/// </summary>
		/// <param name="fileData"></param>
		public void SetFacesFromXmlFile( ContentManager content, string fileName ){
			// 面リストを初期化
			mFaces = new List<Face>();

			// エリアリストを初期化
			int numArea = mNumPartingLineX * mNumPartingLineY * mNumPartingLineZ;
			mAreas = new List<Face>[ numArea ];
			for ( int i = 0; i < numArea; i++ ){
				mAreas[ i ] = new List<Face>();
			}

			// ドキュメントを読み込む
			XDocument doc = XDocument.Parse( content.Load<string>( fileName ) );

			foreach ( XElement node in doc.Elements() ){
				if ( node.Name == "CollisionModel" ){
					Face face = null;
					foreach ( XElement item in node.Elements() ){
						if ( item.Name == "f" ){
							string[] value = item.Value.Split( ',' );

							Vector3[] vertices = new Vector3[ 3 ];
							for ( int i = 0; i < 3; i++ ){
								vertices[ i ] = new Vector3(
									Convert.ToSingle( value[ 0 + i * 3 ] ),
									Convert.ToSingle( value[ 1 + i * 3 ] ),
									Convert.ToSingle( value[ 2 + i * 3 ] ) );
							}

							Vector3 normal = new Vector3(
								Convert.ToSingle( value[ 0 + 9 ] ),
								Convert.ToSingle( value[ 1 + 9 ] ),
								Convert.ToSingle( value[ 2 + 9 ] ) );

							int[] indexes = new int[ value.Length - 12 ];
							for ( int i = 0; i < indexes.Length; i++ ){
								indexes[ i ] = Convert.ToInt32( value[ i + 12 ] );
							}

							face = new Face();
							face.SetVertices( vertices );
							face.Normal = normal;
							face.AreaIndexes = indexes.ToList();

						    // 面を追加
						    mFaces.Add( face );

						    // エリアリストに登録
						    for ( int i = 0; i < face.AreaIndexes.Count; i++ ){
						        mAreas[ face.AreaIndexes[ i ] ].Add( face );
						    }
						}
					}
				}
			}

			// 最大点と最小点を調べる
			mMinVertex = mFaces[ 0 ].Vertices[ 0 ];
			mMaxVertex = mFaces[ 0 ].Vertices[ 0 ];
			foreach ( Face face in mFaces ){
				for ( int i = 0; i < 3; i++ ){
					mMaxVertex.X = Math.Max( mMaxVertex.X, face.Vertices[ i ].X );
					mMaxVertex.Y = Math.Max( mMaxVertex.Y, face.Vertices[ i ].Y );
					mMaxVertex.Z = Math.Max( mMaxVertex.Z, face.Vertices[ i ].Z );

					mMinVertex.X = Math.Min( mMinVertex.X, face.Vertices[ i ].X );
					mMinVertex.Y = Math.Min( mMinVertex.Y, face.Vertices[ i ].Y );
					mMinVertex.Z = Math.Min( mMinVertex.Z, face.Vertices[ i ].Z );
				}
			}

			// 各エリアの大きさを算出
			mAreaScale.X = ( mMaxVertex - mMinVertex ).X / mNumPartingLineX;
			mAreaScale.Y = ( mMaxVertex - mMinVertex ).Y / mNumPartingLineY;
			mAreaScale.Z = ( mMaxVertex - mMinVertex ).Z / mNumPartingLineZ;
		}

		#region 衝突検出

		/// <summary>
		/// 境界球との衝突を検出する
		/// </summary>
		/// <param name="position">境界球の中心座標</param>
		/// <param name="range">境界球の半径</param>
		/// <returns>衝突していれば true</returns>
		public bool IsIntersect( Vector3 position, float range ){
			if ( mFaces.Count == 0 ) return false;

			// 相手の直方体
			Vector3 max, min;
			min = position + new Vector3( -range, -range, -range );
			max = position + new Vector3( range, range, range );
			
			// 境界球がどのエリアに属しているかを調べる
			// エリア番号は小さい順に並ぶ
			int[] indexes = mWorkArray; // 属しているエリア番号
			int numIndex = 0;
			Vector3 minOnArea = min - mMinVertex; // エリア上の最小点
			Vector3 maxOnArea = max - mMinVertex; // エリア上の最大点
			int minAreaX = Math.Max( 0, Math.Min( mNumPartingLineX -1, ( int )( minOnArea.X ) / ( int )mAreaScale.X ) );
			int minAreaY = Math.Max( 0, Math.Min( mNumPartingLineY -1, ( int )( minOnArea.Y ) / ( int )mAreaScale.Y ) );
			int minAreaZ = Math.Max( 0, Math.Min( mNumPartingLineZ -1, ( int )( minOnArea.Z ) / ( int )mAreaScale.Z ) );
			int maxAreaX = Math.Max( 0, Math.Min( mNumPartingLineX -1, ( int )( maxOnArea.X ) / ( int )mAreaScale.X ) );
			int maxAreaY = Math.Max( 0, Math.Min( mNumPartingLineY -1, ( int )( maxOnArea.Y ) / ( int )mAreaScale.Y ) );
			int maxAreaZ = Math.Max( 0, Math.Min( mNumPartingLineZ -1, ( int )( maxOnArea.Z ) / ( int )mAreaScale.Z ) );
			
			bool indexOver = false;
			for ( int x = minAreaX; x <= maxAreaX; x++ ){
			    for ( int y = minAreaY; y <= maxAreaY; y++ ){
			        for ( int z = minAreaZ; z <= maxAreaZ; z++ ){
						indexes[ numIndex ] = y * mNumPartingLineX + z * ( mNumPartingLineX * mNumPartingLineY ) + x;
						++numIndex;

						if ( numIndex >= indexes.Length ){
							indexOver = true;
						}
			        }
					if ( indexOver ) break;
			    }
				if ( indexOver ) break;
			}

			// 属しているエリアにある全ての面との衝突を調べる
			for ( int i = 0; i < numIndex; i++ ){
				foreach ( Face face in mAreas[ indexes[ i ] ] ){
					// とりあえず大雑把に検出
					if ( Collision.TestInersectCube( face.Min, face.Max, min, max ) ){
						// 面との衝突を調べる
						Vector3[] v = face.Vertices;
						if ( Collision.TestIntersectTriangleAndSphere( v[ 0 ], v[ 1 ], v[ 2 ], position, range ) ){
							return true;
						}
					}
				}
			}

			return false;
		}
		
		/// <summary>
		/// 線分との衝突を検出する
		/// </summary>
		/// <param name="positionA">線分の始点</param>
		/// <param name="positionB">線分の終点</param>
		/// <returns>衝突していれば true</returns>
		public bool IsIntersect( Vector3 positionA, Vector3 positionB ){
			Vector3 cross;
			Vector3[] vertices;
			Vector3 normal;

			return IsIntersect( positionA, positionB, out cross, out vertices, out normal );
		}

		/// <summary>
		/// 線分との衝突を検出する
		/// </summary>
		/// <param name="positionA">線分の始点</param>
		/// <param name="positionB">線分の終点</param>
		/// <param name="cross">衝突した点</param>
		/// <returns>衝突していれば true</returns>
		public bool IsIntersect( Vector3 positionA, Vector3 positionB, out Vector3 cross ){
			Vector3[] vertices;
			Vector3 normal;

			return IsIntersect( positionA, positionB, out cross, out vertices, out normal );
		}

		/// <summary>
		/// 線分との衝突を検出する
		/// </summary>
		/// <param name="positionA">線分の始点</param>
		/// <param name="positionB">線分の終点</param>
		/// <param name="cross">衝突した点</param>
		/// <param name="vertices">衝突した面を形成する3頂点</param>
		/// <returns>衝突していれば true</returns>
		public bool IsIntersect( Vector3 positionA, Vector3 positionB, out Vector3 cross, out Vector3[] vertices ){
			Vector3 normal;

			return IsIntersect( positionA, positionB, out cross, out vertices, out normal );
		}
		
		/// <summary>
		/// 線分との衝突を検出する
		/// </summary>
		/// <param name="positionA">線分の始点</param>
		/// <param name="positionB">線分の終点</param>
		/// <param name="cross">衝突した点</param>
		/// <param name="vertices">衝突した面を形成する3頂点</param>
		/// <param name="normal">衝突した面の法線</param>
		/// <returns>衝突していれば true</returns>
		public bool IsIntersect( Vector3 positionA, Vector3 positionB, out Vector3 cross, out Vector3[] vertices, out Vector3 normal ){
			//System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
			//Console.WriteLine( "a : " + sw.Elapsed );

			cross = Vector3.Zero;
			vertices = new Vector3[ 3 ];
			normal = Vector3.Zero;

			if ( mFaces.Count == 0 ) return false;

			// 相手の直方体
			Vector3 max = Vector3.Zero, min = Vector3.Zero;
			min.X = Math.Min( positionA.X, positionB.X ); 
			min.Y = Math.Min( positionA.Y, positionB.Y );
			min.Z = Math.Min( positionA.Z, positionB.Z );

			max.X = Math.Max( positionA.X, positionB.X );
			max.Y = Math.Max( positionA.Y, positionB.Y );
			max.Z = Math.Max( positionA.Z, positionB.Z );
			

			// 線分がどのエリアに属しているかを調べる
			// エリア番号は小さい順に並ぶ
			int[] indexes = mWorkArray; // 属しているエリア番号
			int numIndex = 0;
			Vector3 minOnArea = min - mMinVertex; // エリア上の最小点
			Vector3 maxOnArea = max - mMinVertex; // エリア上の最大点
			int minAreaX = Math.Max( 0, Math.Min( mNumPartingLineX -1, ( int )( minOnArea.X ) / ( int )mAreaScale.X ) );
			int minAreaY = Math.Max( 0, Math.Min( mNumPartingLineY -1, ( int )( minOnArea.Y ) / ( int )mAreaScale.Y ) );
			int minAreaZ = Math.Max( 0, Math.Min( mNumPartingLineZ -1, ( int )( minOnArea.Z ) / ( int )mAreaScale.Z ) );
			int maxAreaX = Math.Max( 0, Math.Min( mNumPartingLineX -1, ( int )( maxOnArea.X ) / ( int )mAreaScale.X ) );
			int maxAreaY = Math.Max( 0, Math.Min( mNumPartingLineY -1, ( int )( maxOnArea.Y ) / ( int )mAreaScale.Y ) );
			int maxAreaZ = Math.Max( 0, Math.Min( mNumPartingLineZ -1, ( int )( maxOnArea.Z ) / ( int )mAreaScale.Z ) );

			bool indexOver = false;
			for ( int x = minAreaX; x <= maxAreaX; x++ ){
			    for ( int y = minAreaY; y <= maxAreaY; y++ ){
			        for ( int z = minAreaZ; z <= maxAreaZ; z++ ){
						indexes[ numIndex ] = y * mNumPartingLineX + z * ( mNumPartingLineX * mNumPartingLineY ) + x;
						++numIndex;

						if ( numIndex >= indexes.Length ){
							indexOver = true;
							break;
						}
			        }
					if ( indexOver ) break;
			    }
				if ( indexOver ) break;
			}

			//Console.WriteLine( "b : " + sw.Elapsed );
			
			bool hit = false;
			Vector3[] v;
			Vector3 p;

			// 属しているエリアにある全ての面との衝突を調べる
			for ( int i = 0; i < numIndex; i++ ){
				foreach ( Face face in mAreas[ indexes[ i ] ] ){
	                // とりあえず大雑把に検出
	                if ( Collision.TestInersectCube( face.Min, face.Max, min, max ) ){
	                    // 面との衝突を調べる
	                    v = face.Vertices;
	                    if ( Collision.TestIntersectTriangleAndLine( v[ 0 ], v[ 1 ], v[ 2 ], positionA, positionB, out p ) ){
	                        // 初回のヒット or 始点と交点とが近ければ保存
	                        if ( !hit || ( p - positionA ).LengthSquared() < ( cross - positionA ).LengthSquared() ){
	                            hit = true;
	                            cross = p;
	                            vertices[ 0 ] = new Vector3( v[ 0 ].X, v[ 0 ].Y, v[ 0 ].Z );
	                            vertices[ 1 ] = new Vector3( v[ 1 ].X, v[ 1 ].Y, v[ 1 ].Z );
	                            vertices[ 2 ] = new Vector3( v[ 2 ].X, v[ 2 ].Y, v[ 2 ].Z );
	                            normal = face.Normal;
	                        }
	                    }
	                }
				}
			}
			
			//Console.WriteLine( "d : " + sw.Elapsed );
			//Console.WriteLine();

			return hit;
		}

		#endregion

		/// <summary>
		/// 最大の頂点
		/// </summary>
		public Vector3 MaxVertex{
			get { return mMaxVertex; }
		}

		/// <summary>
		/// 最小の頂点
		/// </summary>
		public Vector3 MinVertex{
			get { return mMinVertex; }
		}
	}
}

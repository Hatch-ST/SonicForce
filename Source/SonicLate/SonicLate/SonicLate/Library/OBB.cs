using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SonicLate{
	/// <summary>
	/// OBBクラス
	/// </summary>
	class OBB{

		#region フィールド

		/// <summary>
		/// 中心座標
		/// </summary>
		private Vector3 mPosition = new Vector3( 0, 0, 0 );

		/// <summary>
		/// X軸の向き
		/// </summary>
		private Vector3 mDirX = new Vector3( 1, 0, 0 );

		/// <summary>
		/// Y軸の向き
		/// </summary>
		private Vector3 mDirY = new Vector3( 0, 1, 0 );

		/// <summary>
		/// Z軸の向き
		/// </summary>
		private Vector3 mDirZ = new Vector3( 0, 0, 1 );

		/// <summary>
		/// 中心座標から端までのX軸方向の長さ
		/// </summary>
		private float mXLength;

		/// <summary>
		/// 中心座標から端までのY軸方向の長さ
		/// </summary>
		private float mYLength;

		/// <summary>
		/// 中心座標から端までのZ軸方向の長さ
		/// </summary>
		private float mZLength;

		#endregion

		#region コンストラクタ
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="scale">各軸方向の長さ</param>
		public OBB( Vector3 scale ){
			mXLength = scale.X;
			mYLength = scale.Y;
			mZLength = scale.Z;
		}
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="min">最大座標</param>
		/// <param name="max">最小座標</param>
		public OBB( Vector3 min, Vector3 max ){
			setScale( min, max );
		}
		#endregion

		#region 最大座標と最小座標から大きさを登録する
		/// <summary>
		/// 最大座標と最小座標から大きさを登録する
		/// </summary>
		/// <param name="min">最小座標</param>
		/// <param name="max">最大座標</param>
		public void setScale( Vector3 min, Vector3 max ){
			Vector3 t = ( max - min ) * 0.5f;
			mXLength = Math.Abs( t.X );
			mYLength = Math.Abs( t.Y );
			mZLength = Math.Abs( t.Z );
		}
		#endregion

		#region 回転
		public void SetRotation( Vector3 angle ){
			mDirX = new Vector3( 1.0f, 0.0f, 0.0f );
			mDirY = new Vector3( 0.0f, 1.0f, 0.0f );
			mDirZ = new Vector3( 0.0f, 0.0f, 1.0f );
			Matrix rotaX = Matrix.CreateRotationX( angle.X );
			Matrix rotaY = Matrix.CreateRotationY( angle.Y );
			Matrix rotaZ = Matrix.CreateRotationZ( angle.Z );
			Matrix rotation = rotaZ * rotaX * rotaY;
			mDirX = Vector3.Transform( mDirX, rotation );
			mDirY = Vector3.Transform( mDirY, rotation );
			mDirZ = Vector3.Transform( mDirZ, rotation );

			// 方向ベクトルを正規化しておく
			mDirX.Normalize();
			mDirY.Normalize();
			mDirZ.Normalize();
		}

		/// <summary>
		/// Y軸回転
		/// </summary>
		/// <param name="angle">角度</param>
		public void RotaY( float angle ){
			Matrix rotaYMat = Matrix.CreateRotationY( angle );
			mDirX = Vector3.Transform( mDirX, rotaYMat );
			mDirY = Vector3.Transform( mDirY, rotaYMat );
			mDirZ = Vector3.Transform( mDirZ, rotaYMat );

			// 方向ベクトルを正規化しておく
			mDirX.Normalize();
			mDirY.Normalize();
			mDirZ.Normalize();
		}
		#endregion
		
		#region OBBとの衝突検出
		/// <summary>
		/// OBBとの衝突検出 衝突していればtrueを返す
		/// </summary>
		/// <param name="t">衝突を調べるOBB</param>
		/// <returns>衝突しているかどうか</returns>
		public bool IsIntersect( OBB t ){
			Vector3 myVX, myVY, myVZ;
			// 自分の各方向のベクトル
			myVX = mDirX * mXLength;
			myVY = mDirY * mYLength;
			myVZ = mDirZ * mZLength;

			// 相手の各方向のベクトル
			Vector3 tarVX, tarVY, tarVZ;
			tarVX = t.DirX * t.Scale.X;
			tarVY = t.DirY * t.Scale.Y;
			tarVZ = t.DirZ * t.Scale.Z;

			// 両OBBの中心点間の距離
			Vector3 diff;
			diff = mPosition - t.Position;

			// 分離軸 自分のX,Y,Z軸
			if ( CompareDiffAndPjLine( mDirX, mXLength, diff, tarVX, tarVY, tarVZ ) ) return false;
			if ( CompareDiffAndPjLine( mDirY, mYLength, diff, tarVX, tarVY, tarVZ ) ) return false;
			if ( CompareDiffAndPjLine( mDirZ, mZLength, diff, tarVX, tarVY, tarVZ ) ) return false;

			// 分離軸 相手のX,Y,Z軸
			if ( CompareDiffAndPjLine( t.DirX, t.Scale.X, diff, myVX, myVY, myVZ ) ) return false;
			if ( CompareDiffAndPjLine( t.DirY, t.Scale.Y, diff, myVX, myVY, myVZ ) ) return false;
			if ( CompareDiffAndPjLine( t.DirZ, t.Scale.Z, diff, myVX, myVY, myVZ ) ) return false;

			// 双方の方向ベクトルに垂直な分離軸との検査
			if ( CompareDiffAndCrossPjLine( mDirX, myVX, myVY, myVZ, t, tarVX, tarVY, tarVZ, diff ) ) return false;
			if ( CompareDiffAndCrossPjLine( mDirY, myVX, myVY, myVZ, t, tarVX, tarVY, tarVZ, diff ) ) return false;
			if ( CompareDiffAndCrossPjLine( mDirZ, myVX, myVY, myVZ, t, tarVX, tarVY, tarVZ, diff ) ) return false;

			return true;
		}
		#endregion

		#region 境界球との衝突検出
		/// <summary>
		/// 境界球との衝突検出 衝突していればtrueを返す
		/// </summary>
		/// <param name="tarPos">境界球の中心座標</param>
		/// <param name="r">境界球の半径</param>
		/// <returns>衝突しているかどうか</returns>
		public bool IsIntersect( Vector3 tarPos, float r ){
			Vector3 diff;
			diff = mPosition - tarPos;

			float rA, rB, diffLen;
			rA = r;
			rB = mXLength;
			diffLen = Math.Abs( Vector3.Dot( diff, mDirX ) );
			if ( rA + rB < diffLen ) return false;

			rB = mYLength;
			diffLen = Math.Abs( Vector3.Dot( diff, mDirY ) );
			if ( rA + rB < diffLen ) return false;

			rB = mZLength;
			diffLen = Math.Abs( Vector3.Dot( diff, mDirZ ) );
			if ( rA + rB < diffLen ) return false;

			return true;
		}
		#endregion
		
		#region 線分との衝突検出
		/// <summary>
		/// 線分と衝突した位置を取得する
		/// </summary>
		/// <param name="line0">線分の始点</param>
		/// <param name="line1">線分の終点</param>
		/// <param name="output">交点</param>
		/// <returns>衝突していればtrueを返す</returns>
		public bool GetIntersectPoint( Vector3 line0, Vector3 line1, ref Vector3 output  ){
			Vector3 vX, rvX, vY, rvY, vZ, rvZ;
			vX = mDirX * mXLength;
			rvX = mDirX * -mXLength;
			vY = mDirY * mYLength;
			rvY = mDirY * -mYLength;
			vZ = mDirZ * mZLength;
			rvZ = mDirZ * -mZLength;

			Vector3 p0, p1, p2, p3, p4, p5, p6, p7;
			// 前面の頂点 正面から見たときに右上から反時計回り
			p0 = vX + vY + rvZ + mPosition;
			p1 = rvX + vY + rvZ + mPosition;
			p2 = rvX + rvY + rvZ + mPosition;
			p3 = vX + rvY + rvZ + mPosition;
	
			// 背面の頂点 正面から見たときに右上から反時計回り
			p4 = vX + vY + vZ + mPosition;
			p5 = rvX + vY + vZ + mPosition;
			p6 = rvX + rvY + vZ + mPosition;
			p7 = vX + rvY + vZ + mPosition;


			// 頂点情報をコピー
			Vector3[,] vertex = new Vector3[ 12, 3 ]{
				{ p0, p1, p2 },
				{ p2, p3, p0 },
				{ p4, p0, p3 },
				{ p3, p7, p4 },
				{ p4, p5, p6 },
				{ p6, p7, p4 },
				{ p1, p5, p6 },
				{ p6, p2, p1 },
				{ p4, p5, p1 },
				{ p1, p0, p4 },
				{ p7, p6, p2 },
				{ p2, p3, p7 }
			};

			Vector3 line;
			line = line1 - line0;
			double exSqLength = line.LengthSquared();

			Vector3 p = Vector3.Zero;
			bool hit = false;
			// それぞれの面と衝突があるか確かめる
			for ( int i = 0; i < 12; i++ ){
				if ( Collision.TestIntersectTriangleAndLine( vertex[ i, 0 ], vertex[ i, 1 ], vertex[ i, 2 ], line0, line1, out p ) ){
						// 交点までの距離を算出
						line = p - line0;
						double sqLength = line.LengthSquared();

						// 前回までより短ければ長さと交点を保存
						if ( sqLength < exSqLength ){
							hit = true;
							exSqLength = sqLength;
							output = p;
						}
				}
			}

			return hit;
		}
		/// <summary>
		/// 線分との衝突を検出する
		/// </summary>
		/// <param name="line0">線分の始点</param>
		/// <param name="line1">線分の終点</param>
		/// <returns>衝突していればtrueを返す</returns>
		public bool GetIntersectPoint( Vector3 line0, Vector3 line1 ){
			Vector3 t = new Vector3();
			return GetIntersectPoint( line0, line1, ref t );
		}
		#endregion

		#region 三角形との衝突検出

		/// <summary>
		///  三角形との衝突を検出する
		/// </summary>
		/// <param name="tr0">三角形の頂点1</param>
		/// <param name="tr1">三角形の頂点2</param>
		/// <param name="tr2">三角形の頂点3</param>
		/// <returns>衝突があれば true</returns>
		public bool IsIntersect( Vector3 tr0, Vector3 tr1, Vector3 tr2 ){
			// 相手の各辺のベクトル
			Vector3 triA, triB, triC;
			triA = tr1 - tr0;
			triB = tr2 - tr0;
			triC = tr1 - tr2;

			// OBBの中心点から三角形の各頂点までの距離
			Vector3 diffA, diffB, diffC;
			diffA = mPosition - tr0;
			diffB = mPosition - tr1;
			diffC = mPosition - tr2;

			// 分離軸 自分のX,Y,Z軸
			float rA, rB, diffLen;
			rA = mXLength;
			// 各辺を投影して最長のものを使用する
			rB = Math.Max( Math.Abs( Vector3.Dot( triA, mDirX ) ), Math.Max( Math.Abs( Vector3.Dot( triB, mDirX ) ), Math.Abs( Vector3.Dot( triC, mDirX ) ) ) );
			// OBBの中心点から三角形の各頂点までの距離を投影して最長のものを使用する
			diffLen = Math.Max( Math.Abs( Vector3.Dot( diffA, mDirX ) ), Math.Max( Math.Abs( Vector3.Dot( diffB, mDirX ) ), Math.Abs( Vector3.Dot( diffC, mDirX ) ) ) );
			if ( rA + rB < diffLen ) return false;
	
			rA = mYLength;
			rB = Math.Max( Math.Abs( Vector3.Dot( triA, mDirY ) ), Math.Max( Math.Abs( Vector3.Dot( triB, mDirY ) ), Math.Abs( Vector3.Dot( triC, mDirY ) ) ) );
			diffLen = Math.Max( Math.Abs( Vector3.Dot( diffA, mDirY ) ), Math.Max( Math.Abs( Vector3.Dot( diffB, mDirY ) ), Math.Abs( Vector3.Dot( diffC, mDirY ) ) ) );
			if ( rA + rB < diffLen ) return false;

			rA = mZLength;
			rB = Math.Max( Math.Abs( Vector3.Dot( triA, mDirZ ) ), Math.Max( Math.Abs( Vector3.Dot( triB, mDirZ ) ), Math.Abs( Vector3.Dot( triC, mDirZ ) ) ) );
			diffLen = Math.Max( Math.Abs( Vector3.Dot( diffA, mDirZ ) ), Math.Max( Math.Abs( Vector3.Dot( diffB, mDirZ ) ), Math.Abs( Vector3.Dot( diffC, mDirZ ) ) ) );
			if ( rA + rB < diffLen ) return false;

	
			// 自分の各方向のベクトル
			Vector3 myVX, myVY, myVZ;
			myVX = mDirX * mXLength;
			myVY = mDirY * mYLength;
			myVZ = mDirZ * mZLength;

			// 三角形の法線を算出
			Vector3 triN;
			triN = Vector3.Cross( triA, triB );

			// 分離軸 三角形の法線とOBBの各基底ベクトルとの法線ベクトル
			Vector3 n = Vector3.One;
			for ( int i = 0; i < 3; i++ ){
				switch ( i ){
					case 0:
						n = Vector3.Cross( mDirX, triN );
						break;
					case 1:
						n = Vector3.Cross( mDirY, triN );
						break;
					case 2:
						n = Vector3.Cross( mDirZ, triN );
						break;
				}
				n.Normalize();
				rA = GetProjectionLine( n, myVX, myVY, myVZ );
				rB = Math.Max( Math.Abs( Vector3.Dot( triA, n ) ), Math.Max( Math.Abs( Vector3.Dot( triB, n ) ), Math.Abs( Vector3.Dot( triC, n ) ) ) );
				diffLen = Math.Max( Math.Abs( Vector3.Dot( diffA, n ) ), Math.Max( Math.Abs( Vector3.Dot( diffB, n ) ), Math.Abs( Vector3.Dot( diffC, n ) ) ) );
				if ( rA + rB < diffLen ) return false;
			}

			return true;
		}

		#endregion

		#region 衝突を加味した移動ベクトルを取得する
		/// <summary>
		/// 衝突を加味した移動ベクトルを取得する
		/// </summary>
		/// <param name="output">衝突後の移動ベクトル</param>
		/// <param name="move">移動ベクトル</param>
		/// <param name="tOBB">対象のOBB</param>
		/// <returns>衝突していればtrueを返す</returns>
		public bool GetVectorAfterCollisionDetection( ref Vector3 output, Vector3 move, OBB tOBB ){
			bool hit = false;
			OBB movedOBB = tOBB;

			// 元々の移動ベクトルをコピー
			Vector3 result = move;

			// X軸方向の移動での衝突を検出
			movedOBB.Position = tOBB.Position;
			movedOBB.Position += new Vector3( move.X, 0.0f, 0.0f );
			if ( IsIntersect( movedOBB ) ){
				// 衝突していればX軸方向の移動を殺す
				result.X = 0.0f;
				hit = true;
			}

			// Y軸方向の移動での衝突を検出
			movedOBB.Position = tOBB.Position;
			movedOBB.Position += new Vector3( 0.0f, move.Y, 0.0f );
			if ( IsIntersect( movedOBB ) ){
				result.Y = 0.0f;
				hit = true;
			}

			// Z軸方向の移動での衝突を検出
			movedOBB.Position = tOBB.Position;
			movedOBB.Position += new Vector3( 0.0f, 0.0f, move.Z );
			if ( IsIntersect( movedOBB ) ){
				result.Z = 0.0f;
				hit = true;
			}

			// XYZ全て移動した際の衝突を検出
			movedOBB.Position = tOBB.Position + move;
			if ( !hit && IsIntersect( movedOBB ) ){
				result.X = 0.0f; //とりあえずX移動を殺す
			}

			output = result;

			return hit;
		}
		/// <summary>
		/// 衝突を加味した移動ベクトルを取得する
		/// </summary>
		/// <param name="output">衝突後の移動ベクトル</param>
		/// <param name="move">移動ベクトル</param>
		/// <param name="pos">対象の中心座標</param>
		/// <param name="range">対象の半径</param>
		/// <returns>衝突していればtrueを返す</returns>
		public bool GetVectorAfterCollisionDetection( ref Vector3 output, Vector3 move, Vector3 pos, float range ){
			bool hit = false;
			Vector3 moved;

			// 元々の移動ベクトルをコピー
			Vector3 result = move;

			// X軸方向の移動での衝突を検出
			moved = pos;
			moved += new Vector3( move.X, 0.0f, 0.0f );
			if ( IsIntersect( moved, range ) ){
				// 衝突していればX軸方向の移動を殺す
				result.X = 0.0f;
				hit = true;
			}

			// Y軸方向の移動での衝突を検出
			moved = pos;
			moved += new Vector3( 0.0f, move.Y, 0.0f );
			if ( IsIntersect( moved, range ) ){
				result.Y = 0.0f;
				hit = true;
			}

			// Z軸方向の移動での衝突を検出
			moved = pos;
			moved += new Vector3( 0.0f, 0.0f, move.Z );
			if ( IsIntersect( moved, range ) ){
				result.Z = 0.0f;
				hit = true;
			}

			// XYZ全て移動した際の衝突を検出
			moved = pos + move;
			if ( !hit && IsIntersect( moved, range ) ){
				result.X = 0.0f; //とりあえずX移動を殺す
			}

			output = result;

			return hit;
		}
		#endregion

		#region 衝突計算用メソッド
		/// <summary>
		/// 分離軸に投影された軸成分から投影線分の長さを算出する
		/// </summary>
		/// <param name="sep">正規化した分離軸</param>
		/// <param name="x">X軸方向のベクトル</param>
		/// <param name="y">Y軸方向のベクトル</param>
		/// <param name="z">Z軸方向のベクトル</param>
		/// <returns>投影線分の長さ</returns>
		private float GetProjectionLine( Vector3 sep, Vector3 x, Vector3 y, Vector3 z ){
			float r1 = Math.Abs( Vector3.Dot( sep, x ) );
			float r2 = Math.Abs( Vector3.Dot( sep, y ) );
			float r3 = Math.Abs( Vector3.Dot( sep, z ) );
			return r1 + r2 + r3;
		}
		
		/// <summary>
		/// 分離軸に投影された軸成分から投影線分の長さを算出する
		/// </summary>
		/// <param name="sep">正規化した分離軸</param>
		/// <param name="x">X軸方向のベクトル</param>
		/// <param name="y">Y軸方向のベクトル</param>
		/// <returns>投影線分の長さ</returns>
		private float GetProjectionLine( Vector3 sep, Vector3 x, Vector3 y ){
			float r1 = Math.Abs( Vector3.Dot( sep, x ) );
			float r2 = Math.Abs( Vector3.Dot( sep, y ) );
			return r1 + r2;
		}

		/// <summary>
		/// 投影線分の長さとOBBの中心点間の距離を比較する
		/// </summary>
		/// <param name="dir">分離軸</param>
		/// <param name="length">投影線分の長さ</param>
		/// <param name="diff">OBB同士の距離</param>
		/// <param name="tarX">比較するOBBのX軸方向のベクトル</param>
		/// <param name="tarY">比較するOBBのY軸方向のベクトル</param>
		/// <param name="tarZ">比較するOBBのZ軸方向のベクトル</param>
		/// <returns>中心点間の距離の方が長ければtrueを返す</returns>
		private bool CompareDiffAndPjLine( Vector3 dir, float length, Vector3 diff, Vector3 tarX, Vector3 tarY, Vector3 tarZ ){
			float rA = length;
			float rB = GetProjectionLine( dir, tarX, tarY, tarZ );
			float diffLen = Math.Abs( Vector3.Dot( diff, dir ) );
			if ( diffLen > rA + rB ) return true;

			return false;
		}
		
		/// <summary>
		/// 双方の方向ベクトルに垂直な分離軸との検査
		/// </summary>
		private bool CompareDiffAndCrossPjLine(
			Vector3 dir,
			Vector3 vX,
			Vector3 vY,
			Vector3 vZ,
			OBB tar,
			Vector3 tarX,
			Vector3 tarY,
			Vector3 tarZ,
			Vector3 diff
		){
			// 分離軸 相手X軸との法線
			Vector3 n;
			n = Vector3.Cross( dir, tar.mDirX );
			n.Normalize();
			float rA = GetProjectionLine( n, vX, vY, vZ );
			float rB = GetProjectionLine( n, tarY, tarZ );
			float diffLen = Math.Abs( Vector3.Dot( diff, n ) );
			if ( diffLen > rA + rB ) return true;

			// 分離軸 相手Y軸との法線
			n = Vector3.Cross( dir, tar.mDirY );
			n.Normalize();
			rA = GetProjectionLine( n, vX, vY, vZ );
			rB = GetProjectionLine( n, tarX, tarZ );
			diffLen = Math.Abs( Vector3.Dot( diff, n ) );
			if ( diffLen > rA + rB ) return true;

			// 分離軸 相手Z軸との法線
			n = Vector3.Cross( dir, tar.mDirZ );
			n.Normalize();
			rA = GetProjectionLine( n, vX, vY, vZ );
			rB = GetProjectionLine( n, tarX, tarY );
			diffLen = Math.Abs( Vector3.Dot( diff, n ) );
			if ( diffLen > rA + rB ) return true;

			return false;
		}
		#endregion

		/// <summary>
		/// 描画する
		/// </summary>
		public void Render( Camera camera ){
            VertexPositionColor[] vertices = new VertexPositionColor[ 2 ];
			
			Vector3 x = mDirX * mXLength;
			Vector3 y = mDirY * mYLength;
			Vector3 z = mDirZ * mZLength;
			
            Circle.Draw( camera, mPosition + x + y + z, Vector2.One * 10.0f );
            Circle.Draw( camera, mPosition - x + y + z, Vector2.One * 10.0f );
            Circle.Draw( camera, mPosition + x - y + z, Vector2.One * 10.0f );
            Circle.Draw( camera, mPosition - x - y + z, Vector2.One * 10.0f );

            Circle.Draw( camera, mPosition + x + y - z, Vector2.One * 10.0f );
            Circle.Draw( camera, mPosition - x + y - z, Vector2.One * 10.0f );
            Circle.Draw( camera, mPosition + x - y - z, Vector2.One * 10.0f );
            Circle.Draw( camera, mPosition - x - y - z, Vector2.One * 10.0f );
		}

		#region プロパティ
		/// <summary>
		/// 中心座標
		/// </summary>
		public Vector3 Position{
			get{ return mPosition; }
			set{ mPosition = value; }
		}

		/// <summary>
		/// X軸の向き
		/// </summary>
		public Vector3 DirX{
			get{ return mDirX; }
		}

		/// <summary>
		/// Y軸のむき
		/// </summary>
		public Vector3 DirY{
			get{ return mDirY; }
		}

		/// <summary>
		/// Z軸の向き
		/// </summary>
		public Vector3 DirZ{
			get{ return mDirZ; }
		}

		/// <summary>
		/// 各軸方向の大きさ
		/// </summary>
		public  Vector3 Scale{
			set{
				mXLength = value.X;
				mYLength = value.Y;
				mZLength = value.Z;
			}
			get{
				return new Vector3( mXLength, mYLength, mZLength );
			}
		}
		#endregion

	}
}

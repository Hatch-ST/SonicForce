using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SonicLate{
	/// <summary>
	/// 衝突検出
	/// </summary>
	class Collision{

		#region 三角形との衝突

		/// <summary>
		/// 三角形と三角形の衝突を検出する
		/// </summary>
		/// <param name="trA0">三角形Aの頂点</param>
		/// <param name="trB0">三角形Bの頂点</param>
		static public bool TestIntersectTriangleAndTriangle( Vector3 trA0, Vector3 trA1, Vector3 trA2, Vector3 trB0, Vector3 trB1, Vector3 trB2 ){
			Vector3 t = Vector3.Zero;
			// 各辺と三角形との衝突を得る
			if ( TestIntersectTriangleAndLine( trA0, trA1, trA2, trB0, trB1, out t ) ) return true;
			if ( TestIntersectTriangleAndLine( trA0, trA1, trA2, trB0, trB2, out t ) ) return true;
			if ( TestIntersectTriangleAndLine( trA0, trA1, trA2, trB1, trB2, out t ) ) return true;
			if ( TestIntersectTriangleAndLine( trB0, trB1, trB2, trA0, trA1, out t ) ) return true;
			if ( TestIntersectTriangleAndLine( trB0, trB1, trB2, trA0, trA2, out t ) ) return true;
			if ( TestIntersectTriangleAndLine( trB0, trB1, trB2, trA1, trA2, out t ) ) return true;

			return false;
		}

		/// <summary>
		/// 三角形と線分の衝突検出と衝突位置の取得を行う
		/// </summary>
		/// <param name="tr0">三角形頂点1</param>
		/// <param name="tr1">三角形頂点2</param>
		/// <param name="tr2">三角形頂点3</param>
		/// <param name="li0">線分の始点</param>
		/// <param name="li1">線分の終点</param>
		/// <param name="output">三角形と線分の交点</param>
		static public bool TestIntersectTriangleAndLine( Vector3 tr0, Vector3 tr1, Vector3 tr2, Vector3 li0, Vector3 li1, out Vector3 output ){
			output = Vector3.Zero;

			Vector3 a, b, c, d, e, f;

			a = li0;
			b = li1 - li0;
			c = tr0;

			// tri0から伸びる三角形の2本の辺
			d = tr1 - tr0;
			e = tr2 - tr0;

			f = c - a;

			// 三角形の法線ベクトル
			Vector3 n;
			n = Vector3.Cross( d, e );

			// 交点P = a + t( b ) のtを求める
			float nf = Vector3.Dot( n, f );
			float nb = Vector3.Dot( n, b );
			if ( nb == 0.0f ) return false;	// 分母が0ならはずれ
			float t = nf / nb;

			// tの範囲チェック
			if ( t < 0.0f || t > 1.0f ) return false;

			// 交点Pを求める
			Vector3 p;
			p = b * t;
			p += a;
	
			Vector3 g;
			g = p - c; // li0から交点へのベクトル

			// 三角形の範囲内か調べる
			float gd = Vector3.Dot( g, d );
			float ge = Vector3.Dot( g, e );
			float dd = Vector3.Dot( d, d );
			float ee = Vector3.Dot( e, e );
			float de = Vector3.Dot( d, e );

			float u = ( gd * de - ge * dd ) / ( de * de - ee * dd );
			if ( u < 0.0f || u > 1.0f ) return false;

			float v = ( ge * de - gd * ee ) / ( de * de - ee * dd );
			if ( v < 0.0f || ( u + v > 1.0f ) ) return false;

			output = p; // 交点を保存

			return true;
		}

		/// <summary>
		///  頂点から三角形への最近接点を求める
		/// </summary>
		/// <param name="p">任意の点</param>
		/// <param name="a">三角形の頂点1</param>
		/// <param name="b">三角形の頂点2</param>
		/// <param name="c">三角形の頂点3</param>
		/// <param name="output">最近接点</param>
		static public void GetClosetPosTriangle( Vector3 p, Vector3 tr0, Vector3 tr1, Vector3 tr2, out Vector3 output ){
			Vector3 a, b, c, ab, ac, ap;
			a = tr0;
			b = tr1;
			c = tr2;
			ab = b - a;
			ac = c - a;
			ap = p - a;
			float d1 = Vector3.Dot( ap, ab );
			float d2 = Vector3.Dot( ap, ac );
			if ( d1 <= 0.0f && d2 <= 0.0f ){
				output = a;
				return;
			}

			Vector3 bp;
			bp = p - b;
			float d3 = Vector3.Dot( bp, ab );
			float d4 = Vector3.Dot( bp, ac );
			if ( d3 >= 0.0f && d4 < d3 ){
				output = b;
				return;
			}

			float vc = d1 * d4 - d3 * d2;
			if ( vc <= 0.0f && d1 >= 0.0f && d3 < 0.0f ){
				float v = d1 / ( d1 - d3 );
				output = a + ab * v;
				return;
			}

			Vector3 cp;
			cp = p - c;
			float d5 = Vector3.Dot( cp, ab );
			float d6 = Vector3.Dot( cp, ac );
			if ( d6 >= 0.0f && d5 <= d6 ){
				output = c;
				return;
			}

			float vb = d5 * d2 - d1 * d6;
			if ( vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f ){
				float w = d2 / ( d2 - d6 );
				output = a + ac * w;
				return;
			}

			float va = d3 * d6 - d5 * d4;
			if ( va <= 0.0f && ( d4 - d3 ) >= 0.0f && ( d5 - d6 ) >= 0.0f ){
				float w = ( d4 - d3 ) / ( ( d4 - d3 ) + ( d5 - d6 ) );
				output = b + ( c - b ) * w;
				return;
			}

			float denom = 1.0f / ( va + vb + vc );
			float tV = vb * denom;
			float tW = vc * denom;
			output = a + ab * tV + ac * tW;
		}

		/// <summary>
		///  三角形と球の衝突を調べる
		/// </summary>
		/// <param name="tr0">三角形の頂点1</param>
		/// <param name="tr1">三角形の頂点2</param>
		/// <param name="tr2">三角形の頂点3</param>
		/// <param name="spPos">球の中心座標</param>
		/// <param name="spR">球の半径</param>
		/// <returns>衝突していれば true</returns>
		static public bool TestIntersectTriangleAndSphere( Vector3 tr0, Vector3 tr1, Vector3 tr2, Vector3 spPos, float spR ){
			Vector3 p;
			GetClosetPosTriangle( spPos, tr0, tr1, tr2, out p );

			Vector3 disVec;
			disVec = p - spPos;

			return ( disVec.LengthSquared() <= spR * spR );
		}

		#endregion

		#region 境界球の衝突を調べる

		/// <summary>
		/// 境界球と境界球の衝突を調べる
		/// </summary>
		/// <param name="posA">球Aの中心座標</param>
		/// <param name="rA">球Aの半径</param>
		/// <param name="posB">球Bの中心座標</param>
		/// <param name="rB">球Bの半径</param>
		/// <returns>衝突していれば true</returns>
		static public bool TestIntersectShere( Vector3 posA, float rA, Vector3 posB, float rB ){
			Vector3 dif = posA - posB;
			return ( dif.LengthSquared() < ( rA + rB ) * ( rA + rB ) );
		}

		#endregion
		
		/// <summary>
		/// 軸に沿った直方体同士の衝突を調べる
		/// </summary>
		/// <param name="minA">直方体Aの最小点</param>
		/// <param name="maxA">直方体Aの最大点</param>
		/// <param name="minB">直方体Bの最小点</param>
		/// <param name="maxB">直方体Bの最大点</param>
		/// <returns>衝突していれば true</returns>
		static public bool TestInersectCube( Vector3 minA, Vector3 maxA, Vector3 minB, Vector3 maxB ){
			if ( minA.X < maxB.X && maxA.X >minB.X ){
				if ( minA.Y < maxB.Y && maxA.Y >minB.Y ){
					if ( minA.Z < maxB.Z && maxA.Z >minB.Z ){
						return true;
					}
				}
			}
			return false;
		}
	}
}

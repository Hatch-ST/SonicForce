using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SonicLate{
	/// <summary>
	/// 座標同士を曲線で補間するクラス
	/// </summary>
	class Curve{

		#region 制御点データクラス
		public class Data{
			#region フィールド
			/// <summary>
			/// 時間
			/// </summary>
			private float mTime;

			/// <summary>
			/// 座標
			/// </summary>
			private Vector3 mPosition;

			/// <summary>
			/// 座標補間の係数
			/// </summary>
			private Vector3[] mPositionEquations = new Vector3[ 3 ];

			/// <summary>
			/// 座標補間の傾き
			/// </summary>
			private Vector3 mPositionSlopes;

			/// <summary>
			/// 角度
			/// </summary>
			private Vector3 mAngle;
			
			/// <summary>
			/// 角度補間の係数
			/// </summary>
			private Vector3[] mAngleEquations = new Vector3[ 3 ];

			/// <summary>
			/// 角度補間の傾き
			/// </summary>
			private Vector3 mAngleSlopes = Vector3.Zero;

			#endregion

			#region コンストラクタ
			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="time">時間</param>
			/// <param name="position">座標</param>
			/// <param name="angle">角度</param>
			public Data( float time, Vector3 position, Vector3 angle ){
				mPosition = position;
				mAngle = angle;
				mTime = time;
				mPositionSlopes = Vector3.Zero;
			}
			#endregion

			#region 次の点までを補間する
			/// <summary>
			/// 次の点までを補間する
			/// </summary>
			/// <param name="next">次の制御点</param>
			public void Interpolate( ref Data next ){
				// 時間の変位
				float difTime = next.Time - mTime;

				// 傾きの大きさを制限
				float length = ( next.Position - mPosition ).Length();
				if ( mPositionSlopes != Vector3.Zero ){
					mPositionSlopes = Vector3.Normalize( mPositionSlopes ) * length / difTime;
				}
				length = ( next.Angle - mAngle ).Length();
				if ( mAngleSlopes != Vector3.Zero ){
					mAngleSlopes = Vector3.Normalize( mAngleSlopes ) * length / difTime;
				}

				// 補間の方程式( p = a * t^2 + b * t + c )におけるa, b, cを算出 
				// 座標
				Vector3[] e = new Vector3[ 3 ]; // a, b, c
				e[ 0 ] = ( ( next.Position - mPosition ) - mPositionSlopes * difTime ) / ( difTime * difTime );
				e[ 1 ] = mPositionSlopes - 2.0f * e[ 0 ] * mTime;
				e[ 2 ] = mPosition - e[ 0 ] * mTime * mTime - e[ 1 ] * mTime;
				next.PositionSlopes = 2.0f * e[ 0 ] * next.Time + e[ 1 ]; // 次のデータの傾きを登録
				e.CopyTo( mPositionEquations, 0 ); // コピーして登録
				
				// 角度
				e[ 0 ] = ( ( next.Angle - mAngle ) - mAngleSlopes * difTime ) / ( difTime * difTime );
				e[ 1 ] = mAngleSlopes - 2.0f * e[ 0 ] * mTime;
				e[ 2 ] = mAngle - e[ 0 ] * mTime * mTime - e[ 1 ] * mTime;
				next.AngleSlopes = 2.0f * e[ 0 ] * next.Time + e[ 1 ]; // 次のデータの傾きを登録
				e.CopyTo( mAngleEquations, 0 ); // コピーして登録
			}
			#endregion

			#region プロパティ

			/// <summary>
			/// 時間
			/// </summary>
			public float Time{
				get{ return mTime; }
			}

			/// <summary>
			/// 座標
			/// </summary>
			public Vector3 Position{
				get{ return mPosition; }
			}
			
			/// <summary>
			/// 座標補間の係数
			/// </summary>
			public Vector3[] PositionEquations{
				get{ return mPositionEquations; }
			}

			/// <summary>
			/// 座標補間の傾き
			/// </summary>
			public Vector3 PositionSlopes{
				set{ mPositionSlopes = value; }
			}

			/// <summary>
			/// 角度
			/// </summary>
			public Vector3 Angle{
				get{ return mAngle; }
				set{ mAngle = value; }
			}

			/// <summary>
			/// 角度補間の係数
			/// </summary>
			public Vector3[] AngleEquations{
				get{ return mAngleEquations; }
			}

			/// <summary>
			/// 角度補間の傾き
			/// </summary>
			public Vector3 AngleSlopes{
				set{ mAngleSlopes = value; }
			}

			#endregion
		}
		#endregion

		/// <summary>
		/// 制御点データ
		/// </summary>
		private List<Data> mData = new List<Data>();

		/// <summary>
		/// 時間
		/// </summary>
		private float mTime = 0.0f;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public Curve(){
		}
		
		/// <summary>
		/// 制御点を追加する
		/// </summary>
		/// <param name="time">前回の点から今回の点までにかかる時間</param>
		/// <param name="position">座標</param>
		/// <param name="angle">角度</param>
		public void Add( float time, Nullable<Vector3> position, Nullable<Vector3> angle ){
			float totalTime = time;
			if ( mData.Count > 0 ){
				totalTime += mData[ mData.Count - 1 ].Time;
				if ( position == null ) position = mData[ mData.Count - 1 ].Position;
				if ( angle == null ) angle = mData[ mData.Count - 1 ].Angle;
			}else{
				if ( position == null ) position = Vector3.Zero;
				if ( angle == null ) angle = Vector3.Zero;
			}
			mData.Add( new Data( totalTime, position.Value, angle.Value ) );
		}
		
		/// <summary>
		/// 制御点を追加する
		/// </summary>
		/// <param name="time">前回の点から今回の点までにかかる時間</param>
		/// <param name="position">座標</param>
		/// <param name="angle">向きベクトル</param>
		public void AddFromDirection( float time, Vector3 position, Vector3 direction ){
			float totalTime = time;
			if ( mData.Count > 0 ){
				totalTime += mData[ mData.Count - 1 ].Time;
			}
			direction.Normalize();
			mData.Add( new Data( totalTime, position, GetAngleFromDirection( direction ) ) );
		}

		/// <summary>
		/// 制御点を追加する
		/// 移動の向きで角度が登録される
		/// </summary>
		/// <param name="time">前回の点から今回の点までにかかる時間</param>
		/// <param name="position">座標</param>
		public void Add( float time, Vector3 position ){
			float totalTime = time;
			Vector3 angle = Vector3.Zero;
			if ( mData.Count > 0 ){
				totalTime += mData[ mData.Count - 1 ].Time;

				// 向きを算出
				Vector3 dir = position - mData[ mData.Count - 1 ].Position;
				dir.Normalize();

				angle = GetAngleFromDirection( dir );

				mData[ mData.Count - 1 ].Angle = angle;
			}

			mData.Add( new Data( totalTime, position, angle ) );
		}

		/// <summary>
		/// 全ての制御点を補間する
		/// </summary>
		public void InterpolateAll(){
			// 最初のデータの傾きを算出
			if ( mData.Count > 2 ){
				Vector3 v01 = mData[ 1 ].Position - mData[ 0 ].Position;
				Vector3 v21 = mData[ 1 ].Position - mData[ 2 ].Position;
				mData[ 0 ].PositionSlopes = v01 + v21;

				v01 = mData[ 1 ].Angle - mData[ 0 ].Angle;
				v21 = mData[ 1 ].Angle - mData[ 2 ].Angle;
				mData[ 0 ].AngleSlopes = v01 + v21;
			}

			// 各区間を補間する
			for ( int i = 0; i < mData.Count - 1; i++ ){
				if ( i < mData.Count - 2 ){
					// 3点が直線なら傾きを直線で登録
					Vector3 difA = Vector3.Normalize( mData[ i + 1 ].Position - mData[ i ].Position );
					Vector3 difB = Vector3.Normalize( mData[ i + 2 ].Position - mData[ i ].Position );
					float dot = Vector3.Dot( difA, difB );
					float t = 0.00001f;
					if ( dot > 1.0f - t && dot < 1.0f + t ){
						mData[ i ].PositionSlopes = ( mData[ i + 1 ].Position - mData[ i ].Position );
					}

					difA = Vector3.Normalize( mData[ i + 1 ].Angle - mData[ i ].Angle );
					difB = Vector3.Normalize( mData[ i + 2 ].Angle - mData[ i ].Angle );
					dot = Vector3.Dot( difA, difB );
					if ( dot > 1.0f - t && dot < 1.0f + t ){
						mData[ i ].AngleSlopes = ( mData[ i + 1 ].Angle - mData[ i ].Angle );
					}
				}

				Data next = mData[ i + 1 ];
				mData[ i ].Interpolate( ref next );
			}
		}

		/// <summary>
		/// 現在時刻での情報を取得する
		/// </summary>
		/// <param name="time">進める時間( 秒 )</param>
		/// <param name="states">値を登録するモデル情報</param>
		/// <param name="enableLoop">ループさせるどうか</param>
		public void Get( float timeSpeed, ref ModelStates states, bool enableLoop ){
			GetFromTime( mTime + timeSpeed, ref states, enableLoop );
		}

		/// <summary>
		/// 指定した時間での情報を取得する
		/// </summary>
		/// <param name="totalTime">合計時間</param>
		/// <param name="states">値を登録するモデル情報</param>
		/// <param name="enableLoop">ループさせるどうか</param>
		public void GetFromTime( float totalTime, ref ModelStates states, bool enableLoop ){
			// 時間を進める
			mTime = totalTime;

			// ループ
			while ( mTime > mData[ mData.Count - 1 ].Time ){
				if ( enableLoop ) mTime -= mData[ mData.Count - 1 ].Time;
				else mTime = mData[ mData.Count - 1 ].Time;
			}

			// 区間の開始位置を調べる
			int begin = 0;
			for ( int i = 0; i < mData.Count; i++ ){
				if ( mData[ i ].Time > mTime ){
					break;
				}
				begin = i;
			}
			
			// 座標を算出
			Vector3[] e;
			if ( begin == mData.Count - 1 || mData[ begin ].Position == mData[ begin + 1 ].Position ){
				states.Position = mData[ begin ].Position;
			}else{
				e = mData[ begin ].PositionEquations;
				states.Position = ( ( e[ 0 ] * mTime ) + e[ 1 ] ) * mTime + e[ 2 ];
			}

			// 角度を算出
			if ( begin == mData.Count - 1 || mData[ begin ].Angle == mData[ begin + 1 ].Angle ){
				states.Angle = mData[ begin ].Angle;
			}else{
				//float t = ( mTime - mData[ begin ].Time ) / ( mData[ begin + 1 ].Time - mData[ begin ].Time );
				//states.Angle = mData[ begin + 1 ].Angle * t + mData[ begin ].Angle * ( 1.0f - t );
				e = mData[ begin ].AngleEquations;
				states.Angle = ( ( e[ 0 ] * mTime ) + e[ 1 ] ) * mTime + e[ 2 ];
			}
		}
		
		/// <summary>
		/// 現在時刻での情報を取得する
		/// </summary>
		/// <param name="time">進める時間( 秒 )</param>
		/// <param name="position">座標</param>
		/// <param name="angle">角度</param>
		/// <param name="enableLoop">ループさせるどうか</param>
		public void Get( float timeSpeed, out Vector3 position, out Vector3 angle, bool enableLoop ){
			GetFromTime( mTime + timeSpeed, out position, out angle, enableLoop );
		}
		

		/// <summary>
		/// 指定した時間での情報を取得する
		/// </summary>
		/// <param name="totalTime">合計時間</param>
		/// <param name="position">座標</param>
		/// <param name="angle">角度</param>
		/// <param name="enableLoop">ループさせるどうか</param>
		public void GetFromTime( float totalTime, out Vector3 position, out Vector3 angle, bool enableLoop ){
			// 時間を登録する
			mTime = totalTime;
			
			// ループ
			while ( mTime > mData[ mData.Count - 1 ].Time ){
				if ( enableLoop ) mTime -= mData[ mData.Count - 1 ].Time;
				else mTime = mData[ mData.Count - 1 ].Time;
			}

			// 区間の開始位置を調べる
			int begin = 0;
			for ( int i = 0; i < mData.Count; i++ ){
				if ( mData[ i ].Time > mTime ){
					break;
				}
				begin = i;
			}

			// 座標を算出
			Vector3[] e;
			if ( begin == mData.Count - 1 || mData[ begin ].Position == mData[ begin + 1 ].Position ){
				position = mData[ begin ].Position;
			}else{
				e = mData[ begin ].PositionEquations;
				position = ( ( e[ 0 ] * mTime ) + e[ 1 ] ) * mTime + e[ 2 ];
			}
			
			// 角度を算出
			if ( begin == mData.Count - 1 || mData[ begin ].Angle == mData[ begin + 1 ].Angle ){
				angle = mData[ begin ].Angle;
			}else{
				//float t = ( mTime - mData[ begin ].Time ) / ( mData[ begin + 1 ].Time - mData[ begin ].Time );
				//angle = mData[ begin + 1 ].Angle * t + mData[ begin ].Angle * ( 1.0f - t );
				e = mData[ begin ].AngleEquations;
				angle = ( ( e[ 0 ] * mTime ) + e[ 1 ] ) * mTime + e[ 2 ];
			}
		}
		
		/// <summary>
		/// 制御点を変更する
		/// </summary>
		/// <param name="index">変更する制御点の番号</param>
		/// <param name="position">座標</param>
		/// <param name="angle">角度</param>
		public void Set( int index, Nullable<Vector3> position, Nullable<Vector3> angle ){
			if ( index < 0 || index >= mData.Count ) return;
			if ( position == null ) position = mData[ index ].Position;
			if ( angle == null ) angle = mData[ index ].Angle;
			mData[ index ] = new Data( mData[ index ].Time, position.Value, angle.Value );
		}

		/// <summary>
		/// 現在時刻を取得または登録する
		/// </summary>
		public float Time{
			set{ mTime = value; }
			get{ return mTime; }
		}

		/// <summary>
		/// 合計時間を取得する
		/// </summary>
		public float TotalTime{
			get { return mData[ mData.Count - 1 ].Time; }
		}

		/// <summary>
		/// 向きベクトルから角度を算出する
		/// </summary>
		/// <param name="direction">向きベクトル</param>
		/// <returns>角度</returns>
		private Vector3 GetAngleFromDirection( Vector3 direction ){
			Vector3 angle = new Vector3();

			// 向きから角度を算出
			float xZ = ( float )( -Math.Atan2( direction.Y, direction.Z ) );
			if ( xZ > ( float )( Math.PI * 0.5 ) ){
				xZ = ( float )Math.PI - xZ;
			}else if ( xZ < ( float )( -Math.PI * 0.5 ) ){
				xZ = -( float )Math.PI - xZ;
			}
			float xX = ( float )( -Math.Atan2( direction.Y, direction.X ) );
			if ( xX > ( float )( Math.PI * 0.5 ) ){
				xX = ( float )Math.PI - xX;
			}else if ( xX < ( float )( -Math.PI * 0.5 ) ){
				xX = -( float )Math.PI - xX;
			}

			if ( xZ >= 0.0f ){
				angle.X = Math.Min( xZ, xX );
			}else{
				angle.X = Math.Max( xZ, xX );
			}

			angle.Y = ( float )( Math.Atan2( direction.X, direction.Z ) );

			return angle;
		}

		/// <summary>
		/// 制御点の数を取得する
		/// </summary>
		public int Count{
			get { return mData.Count; }
		}

		/// <summary>
		/// 今見ている制御点の番号を取得する
		/// </summary>
		public int GetNowIndex(){
			// 区間の開始位置を調べる
			int begin = 0;
			for ( int i = 0; i < mData.Count; i++ ){
				if ( mData[ i ].Time > mTime ){
					break;
				}
				begin = i;
			}

			return begin;
		}

		/// <summary>
		/// 制御点データを取得する
		/// </summary>
		public Data[] Points{
			get { return mData.ToArray(); }
		}
	}
}

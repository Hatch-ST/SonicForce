using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SkinnedModel;

namespace SonicLate{
	/// <summary>
	/// モデル状態管理クラス
	/// </summary>
	class ModelStates{

		#region フィールド
		/// <summary>
		/// 位置
		/// </summary>
		private Vector3 mPosition;

		/// <summary>
		/// 角度
		/// </summary>
		private Vector3 mAngle;

		/// <summary>
		/// 大きさ
		/// </summary>
		private Vector3 mScale;

		/// <summary>
		/// アニメーションプレイヤー
		/// </summary>
		private AnimationPlayer mAnimPlayer = null;

		/// <summary>
		/// 正面の向き
		/// </summary>
		private Vector3 mFrontDirection = new Vector3( 0.0f, 0.0f, 1.0f );

		/// <summary>
		/// 再生中のアニメーション
		/// </summary>
		private string mPlayingClip = "";

		private Nullable<Matrix> mRotationMatrix = null;

		private bool mRotationZYX = false;

		#endregion

		#region コンストラクタ
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="position">位置</param>
		/// <param name="angle">角度</param>
		/// <param name="scale">大きさ</param>
		/// <param name="skinData">スキニングデータ</param>
		public ModelStates( Vector3 position, Vector3 angle, Vector3 scale, SkinningData skinData ){
			mPosition = new Vector3( position.X, position.Y, position.Z );
			mAngle = new Vector3( angle.X, angle.Y, angle.Z );
			mScale = new Vector3( scale.X, scale.Y, scale.Z );
			
			// スキニングデータがあればアニメーションプレイヤーを生成
			if ( skinData != null ){
				mAnimPlayer = new AnimationPlayer( skinData );
			}
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="skinData">スキニングデータ</param>
		public ModelStates( SkinningData skinData ){
			// デフォルトで初期化
			mAngle = new Vector3( 0.0f, 0.0f, 0.0f );
			mPosition = new Vector3( 0.0f, 0.0f, 0.0f );
			mScale = new Vector3( 1.0f, 1.0f, 1.0f );
			
			// スキニングデータがあればアニメーションプレイヤーを生成
			if ( skinData != null ){
				mAnimPlayer = new AnimationPlayer( skinData );
			}
		}
		#endregion

		#region アニメーションを登録
		/// <summary>
		/// アニメーションを登録する
		/// </summary>
		/// <param name="model">アニメーションさせるモデル</param>
		/// <param name="clipName">アニメーションの名前</param>
		/// <param name="enableLoop">ループの可否</param>
		/// <param name="weight">ウェイト</param>
		public void SetAnimation( HLModel model, string clipName, bool enableLoop, float weight ){
			// 再生中でなければ再生
			if ( clipName != mPlayingClip ){
				mAnimPlayer.StartClip( model.SkinData.AnimationClips[ clipName ], enableLoop, weight );
				mPlayingClip = clipName;
			}
		}

		#endregion

		/// <summary>
		/// 座標, 角度, 大きさをコピーする
		/// </summary>
		/// <param name="states">コピー先</param>
		public ModelStates CopyPositionAngleScale(){
			ModelStates states = new ModelStates( null );
			states.Position = mPosition;
			states.Angle = mAngle;
			states.Scale = mScale;

			return states;
		}
		
		/// <summary>
		/// 向きベクトルから角度を登録する
		/// </summary>
		/// <param name="direction">モデルの向きを表すベクトル</param>
		public void SetAngleFromDirection( Vector3 direction, float weight ){
			Vector3 exAngle = mAngle;
			SetAngleFromDirection( direction );
			Vector3 dif = mAngle - exAngle;

			// 逆回転を防ぐ
			if ( dif.X > Math.PI ){
				dif.X = dif.X - ( float )Math.PI * 2.0f;
			}
			if ( dif.X < -Math.PI ){
				dif.X = dif.X + ( float )Math.PI * 2.0f;
			}
			if ( dif.Y > Math.PI ){
				dif.Y = dif.Y - ( float )Math.PI * 2.0f;
			}
			if ( dif.Y < -Math.PI ){
				dif.Y = dif.Y + ( float )Math.PI * 2.0f;
			}

			mAngle = exAngle + dif * Math.Min( 1.0f, weight );
		}

		/// <summary>
		/// 向きベクトルから角度を登録する
		/// </summary>
		/// <param name="direction">モデルの向きを表すベクトル</param>
		public void SetAngleFromDirection( Vector3 direction ){
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
			    mAngle.X = Math.Min( xZ, xX );
			}else{
			    mAngle.X = Math.Max( xZ, xX );
			}

			mAngle.Y = ( float )( Math.Atan2( direction.X, direction.Z ) );
		}
		
		/// <summary>
		/// 向きベクトルから角度を取得する
		/// </summary>
		/// <param name="direction">モデルの向きを表すベクトル</param>
		public static Vector3 GetAngleFromDirection( Vector3 direction ){
			Vector3 output = Vector3.Zero;

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
			    output.X = Math.Min( xZ, xX );
			}else{
			    output.X = Math.Max( xZ, xX );
			}

			output.Y = ( float )( Math.Atan2( direction.X, direction.Z ) );

			return output;
		}
		
		/// <summary>
		/// 向きベクトルから角度を登録する
		/// </summary>
		/// <param name="direction">モデルの向きを表すベクトル</param>
		/// <param name="camera">カメラ</param>
		public void SetAngleFromDirection( Vector3 direction, Vector3 up ){
			Vector3 z = Vector3.Normalize( direction );
			Vector3 x = Vector3.Normalize( Vector3.Cross( up, z ) );
			Vector3 y = Vector3.Normalize( Vector3.Cross( z, x ) );

			Matrix rota= new Matrix();
			rota.M11 = x.X; rota.M12 = x.Y; rota.M13 = x.Z; rota.M14 = 0.0f;
			rota.M21 = y.X; rota.M22 = y.Y; rota.M23 = y.Z; rota.M24 = 0.0f;
			rota.M31 = z.X; rota.M32 = z.Y; rota.M33 = z.Z; rota.M34 = 0.0f;
			rota.M41 = 0.0f; rota.M42 = 0.0f; rota.M43 = 0.0f; rota.M44 = 1.0f;

			mRotationMatrix = rota;
		}

		/// <summary>
		/// アニメーションを進める
		/// </summary>
		/// <param name="time">進める時間( ミリ秒 )</param>
		public void AdvanceAnimation( long time ){
			if ( mAnimPlayer == null ) return;

			TimeSpan timeSpan = new TimeSpan( time * 10000 );
			mAnimPlayer.Update( timeSpan, true, WorldMatrix );
		}

		#region プロパティ

		/// <summary>
		/// アニメーションプレイヤー
		/// </summary>
		public AnimationPlayer AnimPlayer{
			get{ return mAnimPlayer; }
		}

		/// <summary>
		/// ワールド変換行列
		/// </summary>
		public Matrix WorldMatrix{
			get{
				Matrix matTrans = Matrix.CreateTranslation( mPosition );
				Matrix matScale = Matrix.CreateScale( mScale );

				Matrix matRota;
				if ( mRotationMatrix != null ){
					matRota = mRotationMatrix.Value;
				}else{
					Matrix matRotaX = Matrix.CreateRotationX( mAngle.X );
					Matrix matRotaY = Matrix.CreateRotationY( mAngle.Y );
					Matrix matRotaZ = Matrix.CreateRotationZ( mAngle.Z );
					if ( !mRotationZYX ){
						matRota = matRotaZ * matRotaX * matRotaY;
					}else{
						matRota = matRotaZ * matRotaY * matRotaX;
					}
				}

				return matScale * matRota * matTrans;
			}
		}

		/// <summary>
		/// 再生中のアニメーションの名前を取得する
		/// </summary>
		public string PlayingAnimationName{
			get { return mAnimPlayer.CurrentClip.Name; }
		}

		/// <summary>
		/// Z * Y * X の順に回転行列を乗算する
		/// </summary>
		public bool RotationZYX{
			set { mRotationZYX = value; }
		}

		#region 座標
		/// <summary>
		/// 座標
		/// </summary>
		public Vector3 Position{
			get{ return mPosition; }
			set{ mPosition = value; }
		}
		
		/// <summary>
		/// X座標
		/// </summary>
		public float PositionX{
			get{ return mPosition.X; }
			set{ mPosition.X = value; }
		}
		
		/// <summary>
		/// Y座標
		/// </summary>
		public float PositionY{
			get{ return mPosition.Y; }
			set{ mPosition.Y = value; }
		}
		
		/// <summary>
		/// Z座標
		/// </summary>
		public float PositionZ{
			get{ return mPosition.Z; }
			set{ mPosition.Z = value; }
		}

		#endregion

		#region 角度
		/// <summary>
		/// 角度
		/// </summary>
		public Vector3 Angle{
			get{ return mAngle; }
			set{ mAngle = value; }
		}
		
		/// <summary>
		/// X軸角度
		/// </summary>
		public float AngleX{
			get{ return mAngle.X; }
			set{ mAngle.X = value; }
		}
		
		/// <summary>
		/// Y軸角度
		/// </summary>
		public float AngleY{
			get{ return mAngle.Y; }
			set{ mAngle.Y = value; }
		}
		
		/// <summary>
		/// Z軸角度
		/// </summary>
		public float AngleZ{
			get{ return mAngle.Z; }
			set{ mAngle.Z = value; }
		}
		#endregion

		#region 大きさ
		/// <summary>
		/// 大きさ
		/// </summary>
		public Vector3 Scale{
			get{ return mScale; }
			set{ mScale = value; }
		}
		
		/// <summary>
		/// X軸での大きさ
		/// </summary>
		public float ScaleX{
			get{ return mScale.X; }
			set{ mScale.X = value; }
		}
		
		/// <summary>
		/// Y軸での大きさ
		/// </summary>
		public float ScaleY{
			get{ return mScale.Y; }
			set{ mScale.Y = value; }
		}
		
		/// <summary>
		/// Z軸での大きさ
		/// </summary>
		public float ScaleZ{
			get{ return mScale.Z; }
			set{ mScale.Z = value; }
		}
		#endregion

		#endregion
	}
}

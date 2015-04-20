using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SonicLate{
	/// <summary>
	/// 半透明モデル管理クラス
	/// </summary>
	class TransparentModelManager{
		private static int mMaxNumBuffer = 32;
		private static int mAddedCount = 0;
		private static TransparentModel[] mData = null;

		private static BillBoard mBoard;

		/// <summary>
		/// 半透明モデルクラス
		/// </summary>
		private class TransparentModel : IComparable{
			private HLModel mModel = null;
			private ModelStates mStates = null;
			private BillBoard mBoard = null;
			private Texture2D mTexture = null;
			private float mZ = 0.0f;
			private float mSpeedRate = 1.0f;
			private EffectManager.Type mEffectType = EffectManager.Type.Basic;
			private Vector4 mDiffuse = Vector4.One;
			
			public TransparentModel( HLModel model, ModelStates states, Vector4 diffuse, float speedRate, EffectManager.Type effectType ){
				Initialize( model, null, null, states, diffuse, speedRate, effectType );
			}

			public TransparentModel( HLModel model, ModelStates states, Texture2D texture, Vector4 diffuse, float speedRate, EffectManager.Type effectType ){
				Initialize( model, null, texture, states, diffuse, speedRate, effectType );
			}

			public TransparentModel( HLModel model, ModelStates states, float alpha, float speedRate, EffectManager.Type effectType ){
				Vector4 diffuse = new Vector4( 1.0f, 1.0f, 1.0f, alpha );
				Initialize( model, null, null, states, diffuse, speedRate, effectType );
			}

			public TransparentModel( BillBoard board, Texture2D texture, ModelStates states, float alpha ){
				Vector4 diffuse = new Vector4( 1.0f, 1.0f, 1.0f, alpha );
				Initialize( null, board, texture, states, diffuse, 0, EffectManager.Type.None );
			}

			private void Initialize( HLModel model, BillBoard board, Texture2D texture, ModelStates states, Vector4 diffuse, float speedRate, EffectManager.Type effectType ){
				mModel = model;
				mBoard = board;
				mTexture = texture;
				mStates = states;
				mSpeedRate = speedRate;
				mEffectType = effectType;
				mDiffuse = diffuse;
			}

			public void Render( Camera camera, GameTime gameTime ){
				if ( mModel != null ){
					if ( mTexture != null ) mModel.SetTexture( mTexture );
					mModel.Render( camera, mStates, gameTime, mSpeedRate, null, mDiffuse, true, mEffectType );
					if ( mTexture != null ) mModel.SetTexture( null );
				}else if ( mTexture != null && mBoard != null ){
					mBoard.Render( camera, mTexture, mStates, mDiffuse.W );
				}
			}

			public Vector3 Position{
				get { return mStates.Position; }
			}

			public float Z{
				get { return mZ; }
				set { mZ = value; }
			}

			/// <summary>
			/// 深度でソート
			/// </summary>
			public int CompareTo( object obj ){
				if ( ( TransparentModel )obj == null ){
					return -1;
				}

				TransparentModel t = ( TransparentModel )obj;
				float diff = t.Z - mZ;

				if ( diff < 0.0f ) return -1;
				else if ( diff > 0.0f ) return 1;
				else return 0;
			}
		}

		private TransparentModelManager(){
		}

		/// <summary>
		/// 初期化処理を行う
		/// </summary>
		/// <param name="maxNumBuffer">バッファの数</param>
		static public void Initialize( int maxNumBuffer ){
			mMaxNumBuffer = maxNumBuffer;

			// バッファを初期化
			mData = new TransparentModel[ mMaxNumBuffer ];

			mBoard = new BillBoard();
		}

		/// <summary>
		/// 登録された全てのモデルを描画する
		/// </summary>
		/// <param name="camera">カメラ</param>
		/// <param name="gameTime">ゲーム時間</param>
		static public void RenderAll( Camera camera, GameTime gameTime ){
			for ( int i = 0; i < mAddedCount; i++ ){
				mData[ i ].Z = ( Vector3.Transform( mData[ i ].Position, camera.View * camera.Projection ) ).Z;
			}

			// Z値でソート
			Array.Sort( mData, 0, mAddedCount );

			// 全て描画
			for ( int i = 0; i < mAddedCount; i++ ){
				mData[ i ].Render( camera, gameTime );
			}

			mAddedCount = 0;
		}

		/// <summary>
		/// 半透明モデルを追加する
		/// </summary>
		/// <param name="model">モデル</param>
		/// <param name="states">モデルの状態</param>
		/// <param name="alpha">アルファ値</param>
		/// <param name="speedRate">アニメーションスピードの倍率</param>
		/// <param name="effectType">エフェクト</param>
		static public void Add( HLModel model, ModelStates states, float alpha, float speedRate, EffectManager.Type effectType ){
			if ( mAddedCount >= mData.Length ) return;

			mData[ mAddedCount ] = new TransparentModel( model, states, alpha, speedRate, effectType );
			++mAddedCount;
		}

		static public void Add( HLModel model, ModelStates states, Texture2D texture, Vector4 diffuse, float speedRate, EffectManager.Type effectType ){
			if ( mAddedCount >= mData.Length ) return;

			mData[ mAddedCount ] = new TransparentModel( model, states, texture, diffuse, speedRate, effectType );
			++mAddedCount;
		}
		
		static public void Add( HLModel model, ModelStates states, Vector4 diffuse, float speedRate, EffectManager.Type effectType ){
			if ( mAddedCount >= mData.Length ) return;

			mData[ mAddedCount ] = new TransparentModel( model, states, diffuse, speedRate, effectType );
			++mAddedCount;
		}
		
		static public void Add( Texture2D texture, ModelStates states, float alpha ){
			if ( mAddedCount >= mData.Length ) return;

			mData[ mAddedCount ] = new TransparentModel( mBoard, texture, states, alpha );
			++mAddedCount;
		}
		static public void Add( BillBoard board, Texture2D texture, ModelStates states, float alpha ){
			if ( mAddedCount >= mData.Length ) return;

			mData[ mAddedCount ] = new TransparentModel( board, texture, states, alpha );
			++mAddedCount;
		}
		static public void Add( BillBoard board, ModelStates states, float alpha ){
			if ( mAddedCount >= mData.Length ) return;

			mData[ mAddedCount ] = new TransparentModel( board, board.Texture, states, alpha );
			++mAddedCount;
		}

		static public int DataCount{
			get { return mAddedCount; }
		}
	}
}

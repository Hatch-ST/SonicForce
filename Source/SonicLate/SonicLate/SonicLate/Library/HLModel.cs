using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SkinnedModel;

namespace SonicLate{
	/// <summary>
	/// 3Dモデル
	/// </summary>
	class HLModel{
		#region フィールド
		/// <summary>
		/// 3Dモデル
		/// </summary>
		private Model mModel;

		/// <summary>
		/// 変換行列
		/// </summary>
		private Matrix[] mTransMat;

		/// <summary>
		/// スキニングデータ
		/// </summary>
		private SkinningData mSkinData;

		/// <summary>
		/// 最小座標
		/// </summary>
		private Vector3 mMin = Vector3.Zero;

		/// <summary>
		/// 最大座標
		/// </summary>
		private Vector3 mMax = Vector3.Zero;

		/// <summary>
		/// 大きさ
		/// </summary>
		private Vector3 mScale = Vector3.Zero;

		/// <summary>
		/// メッシュデータ
		/// </summary>
		private List<MeshData> mMeshData = new List<MeshData>();

		/// <summary>
		/// 変更用テクスチャ
		/// </summary>
		private Texture2D mTexture = null;

		/// <summary>
		/// 適用されているエフェクト
		/// </summary>
		private EffectManager.Type mCurrentEffectType = EffectManager.Type.None;
		
		/// <summary>
		/// デバイスへの参照
		/// </summary>
		private static GraphicsDevice mDevice = null;


		/// <summary>
		/// アルファブレンド設定
		/// </summary>
		private static BlendState mBlendState = new BlendState(){
			ColorSourceBlend = Blend.SourceAlpha,
			ColorDestinationBlend = Blend.InverseSourceAlpha,
			AlphaSourceBlend = Blend.SourceAlpha,
			AlphaDestinationBlend = Blend.InverseSourceAlpha,
		};

		/// <summary>
		///  ラスタライズ設定
		/// </summary>
		private static RasterizerState mRasterizerStateCullEnable = new RasterizerState(){ CullMode = CullMode.CullCounterClockwiseFace };
		private static RasterizerState mRasterizerStateCullDisable = new RasterizerState(){ CullMode = CullMode.None };
		
		#endregion

		#region メッシュデータクラス
		/// <summary>
		/// メッシュデータクラス
		/// <summary>
		class MeshData{
			public Texture2D Texture;
			public Effect Effect;
		}
		#endregion

		#region コンストラクタ

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="content">コンテンツマネージャ</param>
		/// <param name="fileName">ファイル名</param>
		public HLModel( ContentManager content, string fileName ){
			// モデルの読み込み
			mModel = content.Load<Model>( fileName );

			// スキニングデータを登録
			mSkinData = mModel.Tag as SkinningData;

			// スキンデータがなければボーンの数だけ位置をコピーする
			if ( mSkinData == null ){
				mTransMat = new Matrix[ mModel.Bones.Count ];
				mModel.CopyAbsoluteBoneTransformsTo( mTransMat );
			}

			foreach ( ModelMesh mesh in mModel.Meshes ){
				mMeshData.Add( new MeshData() );
				
				// エフェクトへの参照を取得
				foreach ( Effect effect in mesh.Effects ){
					mMeshData[ mMeshData.Count-1 ].Effect = effect;
					break;
				}

				// テクスチャへの参照を取得
				foreach ( ModelMeshPart parts in mesh.MeshParts ){
					mMeshData[ mMeshData.Count-1 ].Texture = parts.Effect.Parameters[ "Texture" ].GetValueTexture2D();
					break;
				}
			}

			// 最小と最大の頂点を探す
			bool inputed = false;
			foreach ( ModelMesh mesh in mModel.Meshes ){
				foreach ( ModelMeshPart parts in mesh.MeshParts ){
					VertexPositionNormalTexture[] vertex = new VertexPositionNormalTexture[ parts.NumVertices ];
					parts.VertexBuffer.GetData<VertexPositionNormalTexture>( vertex, 0, parts.NumVertices );

					if ( !inputed ){
						mMin = vertex[ 0 ].Position;
						mMax = vertex[ 0 ].Position;
						inputed = true;
					}

					for ( int i = 0; i < parts.NumVertices; i++ ){
						mMin.X = Math.Min( mMin.X, vertex[ i ].Position.X );
						mMin.Y = Math.Min( mMin.Y, vertex[ i ].Position.Y );
						mMin.Z = Math.Min( mMin.Z, vertex[ i ].Position.Z );
						mMax.X = Math.Max( mMax.X, vertex[ i ].Position.X );
						mMax.Y = Math.Max( mMax.Y, vertex[ i ].Position.Y );
						mMax.Z = Math.Max( mMax.Z, vertex[ i ].Position.Z );
					}
				}
			}

			// 大きさを登録
			mScale = mMax - mMin;
		}
		#endregion

		#region レンダリング

		/// <summary>
		/// レンダリングを行う
		/// </summary>
		/// <param name="camera">カメラ情報</param>
		/// <param name="state">モデルの状態</param>
		/// <param name="timeSpan">タイム</param>
		public void Render( Camera camera, ModelStates states, GameTime gameTime, float speedRate ){
			TimeSpan timeSpan = new TimeSpan( ( long )( gameTime.ElapsedGameTime.Ticks * speedRate ) );

			// エフェクトをデフォルトに設定
			mCurrentEffectType = EffectManager.Type.None;
			int meshIndex = 0;
			foreach ( ModelMesh mesh in mModel.Meshes ){
				foreach ( ModelMeshPart part in mesh.MeshParts ){
					part.Effect = mMeshData[ meshIndex ].Effect;
				}
				++meshIndex;
			}

			// ワールド変換行列を算出
			Matrix worldMat = states.WorldMatrix;
			
			// ブレンド条件の登録
			mDevice.BlendState = mBlendState;
			
			// アニメーションがある
			if ( states.AnimPlayer != null ){
				// アニメーションを更新
				states.AnimPlayer.Update( timeSpan, true, worldMat );

				// ボーンの変換行列を取得
				Matrix[] bones = states.AnimPlayer.GetSkinTransforms();

				// モデル内のメッシュをすべて描画する
				foreach ( ModelMesh mesh in mModel.Meshes ){
					foreach ( SkinnedEffect effect in mesh.Effects ){
						// デフォルトのライティング
						effect.EnableDefaultLighting();

						effect.Parameters[ "Bones" ].SetValue( bones );
						effect.View = camera.View;
						effect.Projection = camera.Projection;
					}
					// メッシュの描画
					mesh.Draw();
				}
			}
			// アニメーションプレイヤーがない
			else{
				// モデル内のメッシュをすべて描画する
				foreach ( ModelMesh mesh in mModel.Meshes ){
					foreach ( BasicEffect effect in mesh.Effects ){
						// デフォルトのライティング
						effect.EnableDefaultLighting();

						effect.World = worldMat;
						effect.View = camera.View;
						effect.Projection = camera.Projection;
					}
					// メッシュの描画
					mesh.Draw();
				}
			}

			// ブレンド条件の復帰
			mDevice.BlendState = BlendState.Opaque;
		}
		
		/// <summary>
		/// レンダリングを行う
		/// </summary>
		/// <param name="camera">カメラ情報</param>
		/// <param name="state">モデルの状態</param>
		/// <param name="gameTime">ゲームタイム</param>
		public void Render( Camera camera, ModelStates states, GameTime gameTime ){
			Render( camera, states, gameTime, 1.0f );
		}

		/// <summary>
		/// エフェクトを指定してレンダリング
		/// </summary>
		/// <param name="camera">カメラ情報</param>
		/// <param name="states">モデルの状態</param>
		/// <param name="gameTime">ゲームタイム</param>
		/// <param name="speedRate">アニメーションスピードの倍率</param>
		/// <param name="ambient">アンビエント色</param>
		/// <param name="diffuse">ディフューズ色</param>
		/// <param name="effectType">使用するエフェクトの種類</param>
		public void Render( Camera camera, ModelStates states, GameTime gameTime, float speedRate, Nullable<Vector4> ambient, Nullable<Vector4> diffuse, bool enableCulling, EffectManager.Type effectType ){
			TimeSpan timeSpan = new TimeSpan( ( long )( gameTime.ElapsedGameTime.Ticks * speedRate ) );
			
			// ワールド変換行列を算出
			Matrix worldMat = states.WorldMatrix;

			EffectParent effect = EffectManager.Get( effectType );
			
			effect.ViewProjMatrix = camera.View * camera.Projection;

			Vector3 light = new Vector3( 0.0f, -0.40f, -1.0f );
			light.Normalize();
			effect.LightDirection = light;
			effect.Ambient = ( ambient != null ) ? ambient.Value : new Vector4( 0.5f, 0.5f, 0.5f, 0.0f );
			effect.Diffuse = ( diffuse != null ) ? diffuse.Value : Vector4.One;
			effect.EyePosition = camera.Position;

			// 視線ベクトルを登録
			if ( effectType == EffectManager.Type.Wrapped ){
				Vector3 view = states.Position - camera.Position;
				view.Normalize();
				effect.EyeDirection = view;
			}

			// エフェクトが前回までと異なる場合、新しいエフェクトを登録
			if ( mCurrentEffectType != effectType ){
				mCurrentEffectType = effectType;
				foreach ( ModelMesh mesh in mModel.Meshes ){
					foreach ( ModelMeshPart part in mesh.MeshParts ){
						part.Effect = effect.Effect;
					}
				}
			}

			// ブレンド条件の登録
			mDevice.BlendState = mBlendState;
			mDevice.RasterizerState = ( enableCulling ) ? mRasterizerStateCullEnable : mRasterizerStateCullDisable;
			
			// アニメーションがある
			if ( states.AnimPlayer != null ){
				// テクニックを指定
				effect.SetToSkinningTechniques();

				// アニメーションを更新
				if ( effectType != EffectManager.Type.DepthMap ){
					states.AnimPlayer.Update( timeSpan, true, worldMat );
				}

				// ボーンの変換行列を取得
				Matrix[] bones = states.AnimPlayer.GetSkinTransforms();
				
				// モデル内のメッシュをすべて描画する
				int meshIndex = 0;
				foreach ( ModelMesh mesh in mModel.Meshes ){
					// テクスチャを登録
					effect.Texture = ( mTexture != null ) ? mTexture : mMeshData[ meshIndex ].Texture;

					effect.BoneMatrix = bones;

					// メッシュの描画
					mesh.Draw();

					++meshIndex;
				}
			}
			// アニメーションがない
			else{
				// テクニックを指定
				effect.SetToDefaultTechniques();

				// モデル内のメッシュをすべて描画する
				int meshIndex = 0;
				foreach ( ModelMesh mesh in mModel.Meshes ){
					// テクスチャを登録
					effect.Texture = ( mTexture != null ) ? mTexture : mMeshData[ meshIndex ].Texture;

					effect.WorldMatrix = worldMat;

					// メッシュの描画
					mesh.Draw();

					++meshIndex;
				}
			}

			
			// ブレンド条件の復帰
			mDevice.BlendState = BlendState.Opaque;
		}

		/// <summary>
		/// エフェクトを指定してレンダリング
		/// </summary>
		/// <param name="camera">カメラ情報</param>
		/// <param name="states">モデルの状態</param>
		/// <param name="gameTime">ゲームタイム</param>
		/// <param name="effectType">使用するエフェクトの種類</param>
		public void Render( Camera camera, ModelStates states, GameTime gameTime, EffectManager.Type effectType ){
			Render( camera, states, gameTime, 1.0f,  effectType );
		}

		/// <summary>
		/// エフェクトを指定してレンダリング
		/// </summary>
		/// <param name="camera">カメラ情報</param>
		/// <param name="states">モデルの状態</param>
		/// <param name="gameTime">ゲームタイム</param>
		/// <param name="speedRate">アニメーションスピードの倍率</param>
		/// <param name="effectType">使用するエフェクトの種類</param>
		public void Render( Camera camera, ModelStates states, GameTime gameTime, float speedRate, EffectManager.Type effectType ){
			Render( camera, states, gameTime, speedRate, null, null, true, effectType );
		}
		
		/// <summary>
		/// エフェクトを指定してレンダリング
		/// </summary>
		/// <param name="camera">カメラ情報</param>
		/// <param name="states">モデルの状態</param>
		/// <param name="gameTime">ゲームタイム</param>
		/// <param name="speedRate">アニメーションスピードの倍率</param>
		/// <param name="ambient">アンビエント色</param>
		/// <param name="effectType">使用するエフェクトの種類</param>
		public void Render( Camera camera, ModelStates states, GameTime gameTime, float speedRate, Vector3 ambient, EffectManager.Type effectType ){
			Render( camera, states, gameTime, speedRate, null, null, true, effectType );
		}
		
		/// <summary>
		/// リニアフィルタとテクスチャラッピングサンプラーでエフェクトを指定してレンダリング
		/// </summary>
		/// <param name="camera">カメラ情報</param>
		/// <param name="states">モデルの状態</param>
		/// <param name="gameTime">ゲームタイム</param>
		/// <param name="speedRate">アニメーションスピードの倍率</param>
		/// <param name="ambient">アンビエント色</param>
		/// <param name="diffuse">ディフューズ色</param>
		/// <param name="effectType">使用するエフェクトの種類</param>
		public void RenderLinearWrap( Camera camera, ModelStates states, GameTime gameTime, float speedRate, Nullable<Vector4> ambient, Nullable<Vector4> diffuse, bool enableCulling, EffectManager.Type effectType ){
			// サンプラー条件を登録
			for ( int i = 0; i < 16; ++i ){
				mDevice.SamplerStates[ i ] = SamplerState.LinearWrap;
			}

			Render( camera, states, gameTime, speedRate, ambient, diffuse, enableCulling, effectType );
		}
		
		/// <summary>
		/// リニアフィルタとテクスチャラッピングサンプラーでエフェクトを指定してレンダリング
		/// </summary>
		/// <param name="camera">カメラ情報</param>
		/// <param name="states">モデルの状態</param>
		/// <param name="gameTime">ゲームタイム</param>
		/// <param name="effectType">使用するエフェクトの種類</param>
		public void RenderLinearWrap( Camera camera, ModelStates states, GameTime gameTime, EffectManager.Type effectType ){
			// サンプラー条件を登録
			for ( int i = 0; i < 16; ++i ){
				mDevice.SamplerStates[ i ] = SamplerState.LinearWrap;
			}

			Render( camera, states, gameTime, 1.0f,  effectType );
		}
		
		/// <summary>
		/// 深度バッファに書き込まずにエフェクトを指定してレンダリング
		/// </summary>
		/// <param name="camera">カメラ情報</param>
		/// <param name="states">モデルの状態</param>
		/// <param name="gameTime">ゲームタイム</param>
		/// <param name="speedRate">アニメーションスピードの倍率</param>
		/// <param name="ambient">アンビエント色</param>
		/// <param name="diffuse">ディフューズ色</param>
		/// <param name="effectType">使用するエフェクトの種類</param>
		public void RenderWithOutDepth( Camera camera, ModelStates states, GameTime gameTime, float speedRate, Nullable<Vector4> ambient, Nullable<Vector4> diffuse, bool enableCulling, EffectManager.Type effectType ){
			mDevice.DepthStencilState = DepthStencilState.None;
			Render( camera, states, gameTime, speedRate, ambient, diffuse, enableCulling, effectType );
			mDevice.DepthStencilState = DepthStencilState.Default;
		}

		#endregion

		#region 面情報の取得

		/// <summary>
		/// ワールド変換後の全ての面を取得する
		/// </summary>
		/// <param name="faces">面を格納するリスト</param>
		/// <param name="states">モデルの状態</param>
		public void GetFaces( out List<Vector3[]> faces, ModelStates states ){
			// 初期化
			faces = new List<Vector3[]>();

			foreach ( ModelMesh mesh in mModel.Meshes ){
				foreach ( ModelMeshPart parts in mesh.MeshParts ){
					// 頂点情報を取得
					// 情報は座標, 法線, UVの3つ
					// ファイルが頂点の色情報を持っているとうまく動かない
					VertexPositionNormalTexture[] vertex = new VertexPositionNormalTexture[ parts.NumVertices ];
					parts.VertexBuffer.GetData< VertexPositionNormalTexture >( vertex );

					// インデックスバッファを取得 
					Int16[] index = new Int16[ parts.IndexBuffer.IndexCount ];
					parts.IndexBuffer.GetData<Int16>( index );

					// ワールド変換行列を算出
					Matrix worldMat = states.WorldMatrix;

					// 頂点情報を登録
					for ( int i = parts.StartIndex; i < parts.IndexBuffer.IndexCount; i += 3 ){
						// 頂点を変換
						Vector3[] v = new Vector3[ 3 ];
						for ( int j = 0; j < 3; j++ ){
							v[ j ] = Vector3.Transform( vertex[ index[ i + j ] ].Position, worldMat );
						}

						// リストに追加
						faces.Add( v );
					}
				}
			}
		}

		#endregion

		#region 衝突検出
		/// <summary>
		/// 境界球との衝突を検出する
		/// </summary>
		/// <param name="state">衝突を調べるモデルの状態</param>
		/// <param name="position">境界球の中心座標</param>
		/// <param name="range">境界球の半径</param>
		/// <returns>衝突しているかどうか</returns>
		public bool IsIntersect( ModelStates state, Vector3 position, float range ){
			foreach ( ModelMesh mesh in mModel.Meshes ){
				foreach ( ModelMeshPart parts in mesh.MeshParts ){
					// 頂点情報を取得
					// 情報は座標, 法線, UVの3つ
					// ファイルが頂点の色情報を持っているとうまく動かない
					VertexPositionNormalTexture[] vertex = new VertexPositionNormalTexture[ parts.NumVertices ];
					parts.VertexBuffer.GetData< VertexPositionNormalTexture >( vertex );

					// インデックスバッファを取得 
					Int16[] index = new Int16[ parts.IndexBuffer.IndexCount ];
					parts.IndexBuffer.GetData<Int16>( index );

					// ワールド変換行列を算出
					Matrix worldMat = state.WorldMatrix;

					// 各ポリゴンとの衝突を調べる
					for ( int i = parts.StartIndex; i < parts.IndexBuffer.IndexCount; i += 3 ){
						// 頂点を変換
						Vector3[] v = new Vector3[ 3 ];
						for ( int j = 0; j < 3; j++ ){
							v[ j ] = Vector3.Transform( vertex[ index[ i + j ] ].Position, worldMat );
						}

						// 当たったら終了
						if ( Collision.TestIntersectTriangleAndSphere( v[ 0 ], v[ 1 ], v[ 2 ], position, range ) ){
							return true;
						}
					}
				}
			}
			return false;
		}
		/// <summary>
		///  線分との衝突を検出する
		/// </summary>
		/// <param name="state">衝突を調べるモデルの状態</param>
		/// <param name="positionA">線分の始点</param>
		/// <param name="positionB">線分の終点</param>
		/// <param name="output">線分とポリゴンの交点の出力先</param>
		/// <returns>衝突しているかどうか</returns>
		public bool IsIntersect( ModelStates state, Vector3 positionA, Vector3 positionB, out Vector3 output ){
			output = Vector3.Zero;

			foreach ( ModelMesh mesh in mModel.Meshes ){
				foreach ( ModelMeshPart parts in mesh.MeshParts ){
					// 頂点情報を取得
					// 情報は座標, 法線, UVの3つ
					// ファイルが頂点の色情報を持っているとうまく動かない
					VertexPositionNormalTexture[] vertex = new VertexPositionNormalTexture[ parts.NumVertices ];
					parts.VertexBuffer.GetData< VertexPositionNormalTexture >( vertex );

					// インデックスバッファを取得 
					Int16[] index = new Int16[ parts.IndexBuffer.IndexCount ];
					parts.IndexBuffer.GetData<Int16>( index );


					// 各ポリゴンとの衝突を調べる
					for ( int i = parts.StartIndex; i < parts.NumVertices; i += 3 ){
						// 頂点を変換
						Vector3[] v = new Vector3[ 3 ];
						for ( int j = 0; j < 3; j++ ){
							v[ j ] = Vector3.Transform( vertex[ index[ i + j ] ].Position, state.WorldMatrix );
						}

						// 当たったら終了
						if ( Collision.TestIntersectTriangleAndLine( v[ 0 ], v[ 1 ], v[ 2 ], positionA, positionB, out output ) ){
							return true;
						}
					}
				}
			}
			return false;
		}

		#endregion

		public void Release(){
			// エフェクトをデフォルトに設定
			int meshIndex = 0;
			foreach ( ModelMesh mesh in mModel.Meshes ){
				foreach ( ModelMeshPart part in mesh.MeshParts ){
					part.Effect = mMeshData[ meshIndex ].Effect;
				}
				++meshIndex;
			}
		}

		/// <summary>
		/// テクスチャを登録する
		/// </summary>
		/// <param name="texture">登録するテクスチャ nullでデフォルトに戻す</param>
		public void SetTexture( Texture2D texture ){
			mTexture = texture;
		}

		#region プロパティ
		/// <summary>
		/// スキニングデータ
		/// </summary>
		public SkinningData SkinData{
			get{ return mSkinData; }
		}


		/// <summary>
		/// 最大の頂点
		/// </summary>
		public Vector3 MaxVertex{
			get{ return mMax; }
		}

		/// <summary>
		/// 最小の頂点
		/// </summary>
		public Vector3 MinVertex{
			get{ return mMin; }
		}

		/// <summary>
		/// 大きさ
		/// </summary>
		public Vector3 Scale{
			get{ return mScale; }
		}

		/// <summary>
		/// グラフィックスデバイスを登録する
		/// </summary>
		public static GraphicsDevice Device{
			set { mDevice = value; }
		}

		#endregion
	}
}

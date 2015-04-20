using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SonicLate{
	/// <summary>
	/// 連結ビルボード
	/// </summary>
	class LedBillBoard{
		private VertexBuffer mVertexBuffer = null;
		private Texture2D mTexture = null;
		private VertexPositionColorTexture[] mVertices = null;
		
		/// <summary>
		/// アルファブレンド設定
		/// </summary>
		private static BlendState mBlendState = new BlendState(){
			ColorSourceBlend = Blend.SourceAlpha,
			ColorDestinationBlend = Blend.One,
			AlphaSourceBlend = Blend.SourceAlpha,
			AlphaDestinationBlend = Blend.InverseSourceAlpha,
		};

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="device"></param>
		/// <param name="content"></param>
		/// <param name="fileName"></param>
		/// <param name="length"></param>
		public LedBillBoard( GraphicsDevice device, ContentManager content, string fileName, int length ){
			// テクスチャを読み込む
			mTexture = content.Load<Texture2D>( fileName );
			
			// 頂点を初期化
			mVertices = new VertexPositionColorTexture[ length * 4 ];
			for ( int i = 0; i < mVertices.Length; i++ ){
				Vector3 position = new Vector3();
				Vector2 uv = new Vector2();

				switch ( i % 4 ){
					case 0 :
						position = new Vector3( -0.5f, 0.5f, ( float )i / mVertices.Length );
						uv = new Vector2( 0.0f, 0.0f );
						break;
					case 1 :
						position = new Vector3( 0.5f, 0.5f, ( float )i / mVertices.Length );
						uv = new Vector2( 1.0f, 0.0f );
						break;
					case 2 :
						position = new Vector3( -0.5f, -0.5f, ( float )i / mVertices.Length );
						uv = new Vector2( 0.0f, 1.0f );
						break;
					case 3 :
						position = new Vector3( 0.5f, -0.5f, ( float )i / mVertices.Length );
						uv = new Vector2( 1.0f, 1.0f );
						break;
				}

				mVertices[ i ] = new VertexPositionColorTexture( position, Color.White, uv );
			}

			// バッファを生成
			mVertexBuffer = new VertexBuffer( device, typeof( VertexPositionColorTexture ), length * 4, BufferUsage.None );

			// バッファに登録
			mVertexBuffer.SetData( mVertices );
		}

		public void SetBoardPosition( int index, Camera camera, Vector3 position, float scale ){
			// 範囲外なら終了
			if ( index * 4 >= mVertices.Length ) return;

			// ビュー変換
			Vector3 viewPos = Vector3.Transform( position, camera.View );

			int n = index * 4;
			mVertices[ n + 0 ].Position = new Vector3( viewPos.X - scale, viewPos.Y + scale, viewPos.Z );
			mVertices[ n + 1 ].Position = new Vector3( viewPos.X + scale, viewPos.Y + scale, viewPos.Z );
			mVertices[ n + 2 ].Position = new Vector3( viewPos.X - scale, viewPos.Y - scale, viewPos.Z );
			mVertices[ n + 3 ].Position = new Vector3( viewPos.X + scale, viewPos.Y - scale, viewPos.Z );
		}

		public void SetBoardColor( int index, float a, float r, float g, float b ){
			int n = index * 4;
			for ( int i = n; i < n + 4 && i < mVertices.Length; i++ ){
				mVertices[ i ].Color = new Color( r, g, b, a );
			}
		}

		public void Render( Camera camera ){
			// バッファに登録
			mVertexBuffer.SetData( mVertices );

			BillBoard.VertexColorEnable = true;
			BillBoard.Render( mVertexBuffer, mBlendState, mTexture, Vector4.One, camera.Projection, Matrix.Identity );
			BillBoard.VertexColorEnable = false;
		}

		public int Length{
			get { return mVertices.Length / 4; }
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SonicLate {
	class HitStar{

		private BillBoard mBoard = null;
		private const int mNumStar = 5;
		private ModelStates[] mStates = new ModelStates[ mNumStar ];
		
		private float mAlpha = 0.0f;

		private const float mDistance = 50.0f;
		private const float mRadPerFrame = 0.05f;

		public HitStar( ContentManager content ){
			mBoard = new BillBoard( content, "Image/star_90" );
			
			Matrix rotaY = Matrix.CreateRotationY( ( float )( Math.PI * 2.0 ) / mStates.Length );
			for ( int i = 0; i < mStates.Length; i++ ){
				mStates[ i ] = new ModelStates( null );
				mStates[ i ].Scale *= 30.0f;
				mStates[ i ].PositionX = mDistance;

				if ( i > 0 ){
					mStates[ i ].Position = Vector3.Transform( mStates[ i - 1 ].Position, rotaY );
				}
			}
		}

		public void Initialize(){
			mAlpha = 0.0f;
		}

		public void Update(){
			Matrix rotaY = Matrix.CreateRotationY( mRadPerFrame );
			for ( int i = 0; i < mStates.Length; i++ ){
				mStates[ i ].Position = Vector3.Transform( mStates[ i ].Position, rotaY );
			}

			mAlpha += 1.0f / 60.0f;
		}

		public void Render( Vector3 position ){
			if ( mAlpha > 0.0f ){
				for ( int i = 0; i < mStates.Length; i++ ){
					ModelStates state = mStates[ i ].CopyPositionAngleScale();
					state.Position += position;
					TransparentModelManager.Add( mBoard, state, mAlpha );
				}
			}
		}
	}
}
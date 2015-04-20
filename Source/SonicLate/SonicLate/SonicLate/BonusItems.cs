using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace SonicLate{
	/// <summary>
	/// ボーナスアイテム
	/// </summary>
	class BonusItems{
		private List<Item> mData = null;
		private float mRange = 160.0f;

		class Item{
			public Vector3 Position;
			public bool Got;
			public Item( Vector3 position ){
				Position = position;
				Got = false;
			}
		}

		public BonusItems(){
			mData = new List<Item>();
		}

		public void Add( Vector3 position ){
			mData.Add( new Item( position ) );
		}

		public bool TestIntersect( Vector3 position, float range ){
			bool hit = false;
			for ( int i = 0; i < mData.Count; i++ ){
				if ( !mData[ i ].Got && Collision.TestIntersectShere( position, range, mData[ i ].Position, mRange ) ){
					mData[ i ].Got = true;
					hit = true;
				}
			}

			return hit;
		}

		public void Reset(){
			for ( int i = 0; i < mData.Count; i++ ){
				mData[ i ].Got = false;
			}
		}

		public void Draw( Camera camera ){
			for ( int i = 0; i < mData.Count; i++ ){
				Circle.Draw( camera, mData[ i ].Position, new Vector2( mRange * 2, mRange * 2 ) );
			}
		}
	}
}

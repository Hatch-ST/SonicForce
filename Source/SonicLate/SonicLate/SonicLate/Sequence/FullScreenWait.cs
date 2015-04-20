using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SonicLate{
	/// <summary>
	/// フルスクリーン時に少し待つ画面
	/// </summary>
	class FullScreenWait : GameChild{

		int mCount = 0;

		public FullScreenWait(){
		}

		public override GameChild Update( GameTime time ){
			GameChild next = this;

			if ( mCount > 240 ){
				next = new Title();
			}

			++mCount;

			return next;
		}

		public override void Draw( GameTime time ){
			mDevice.Clear( Color.Black );
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SonicLate{
	/// <summary>
	/// 隠しコマンドクラス
	/// </summary>
	class Command{

		private List<Buttons> mButtonData;
		private List<Keys> mKeyData;
		
		private int mButtonCount;
		private int mKeyCount;

		private bool mSucceeded = false;

		public Command(){
			mButtonData = new List<Buttons>();
			mButtonData.Add( Buttons.X );
			mButtonData.Add( Buttons.Y );
			mButtonData.Add( Buttons.X );
			mButtonData.Add( Buttons.Y );
			mButtonData.Add( Buttons.DPadRight );
			mButtonData.Add( Buttons.DPadRight );
			mButtonData.Add( Buttons.DPadLeft );
			mButtonData.Add( Buttons.DPadLeft );
			mButtonData.Add( Buttons.Y );
			mButtonData.Add( Buttons.B );

			mKeyData = new List<Keys>();
			mKeyData.Add( Keys.X );
			mKeyData.Add( Keys.Y );
			mKeyData.Add( Keys.X );
			mKeyData.Add( Keys.Y );
			mKeyData.Add( Keys.Right );
			mKeyData.Add( Keys.Right );
			mKeyData.Add( Keys.Left );
			mKeyData.Add( Keys.Left );
			mKeyData.Add( Keys.Y );
			mKeyData.Add( Keys.B );
		}

		/// <summary>
		/// 更新
		/// </summary>
		public void Update(){
			if ( InputManager.IsJustAnyButtonDown( PlayerIndex.One ) ){
				if ( InputManager.IsJustButtonDown( PlayerIndex.One, mButtonData[ mButtonCount ] ) ){
					++mButtonCount;
				}else{
					mButtonCount = 0;
				}
			}

			if ( InputManager.IsAnyKeyDown() ){
			    if ( InputManager.IsJustKeyDown( mKeyData[ mKeyCount ] ) ){
			        ++mKeyCount;
			    }else{
			        mKeyCount = 0;
			    }
			}

			if ( mButtonCount >= mButtonData.Count ){
				mButtonCount = 0;
				mSucceeded = true;
			}
			if ( mKeyCount >= mKeyData.Count ){
				mKeyCount = 0;
				mSucceeded = true;
			}
		}

		public void Reset(){
			mKeyCount = 0;
			mButtonCount = 0;
			mSucceeded = false;
		}

		public bool IsSucceed(){
			return mSucceeded;
		}
	}
}

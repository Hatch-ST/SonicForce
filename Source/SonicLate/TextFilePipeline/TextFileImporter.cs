using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

// TODO: ここでインポートする型を指定します。
using TImport = System.String;

namespace TextFilePipeline {
	[ContentImporter( ".txt", DisplayName = "テキストファイル" )]
	public class TextFileImporter : ContentImporter<TImport>{
		/// <summary>
		/// インポート
		/// </summary>
		public override TImport Import( string filename, ContentImporterContext context ){
			//StreamReader reader = new StreamReader( @filename, Encoding.UTF8 );
			
			//// 内容をすべて読み込む
			//string t = reader.ReadToEnd();

			//reader.Close();

			//return t;
			
			FileStream fs = new FileStream( @filename, FileMode.Open, FileAccess.Read );

			// バイナリで読み込む
			byte[] data = new byte[ fs.Length ];
			fs.Read( data, 0, data.Length );

			// テキストに変換 頭3バイトにゴミが入るっぽい
			String text = System.Text.Encoding.UTF8.GetString( data, 3, data.Length - 3 );

			fs.Close();

			return text;
		}
	}
}

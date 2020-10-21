using System.IO;
using MelonLoader;
using UnityEngine;

namespace TinCanImprovements {

	internal class TinCanImprovementsMod : MelonMod {

		public static AssetBundle assetBundle = null;

		public override void OnApplicationStart() {
			MemoryStream memoryStream;
			using (Stream stream = Assembly.GetManifestResourceStream("TinCanImprovements.res.TinCanImprovements")) {
				memoryStream = new MemoryStream((int) stream.Length);
				stream.CopyTo(memoryStream);
			}
			assetBundle = AssetBundle.LoadFromMemory(memoryStream.ToArray());

			Debug.Log($"[{Info.Name}] Version {Info.Version} loaded!");
		}
	}
}

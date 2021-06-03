using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEditor;

namespace SDG.Unturned.Tools
{
	public static partial class EditorAssetBundleHelper
	{
		/// <summary>
		/// Build an asset bundle by name.
		/// </summary>
		/// <param name="AssetBundleName">Name of an asset bundle registered in the editor.</param>
		/// <param name="OutputPath">Absolute path to directory to contain built asset bundle.</param>
		/// <param name="Multiplatform">Should mac and linux variants of asset bundle be built as well?</param>
		public static void Build(string AssetBundleName, string OutputPath, bool Multiplatform)
		{
			string[] AssetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(AssetBundleName);
			if(AssetPaths.Length < 1)
			{
				Debug.LogWarning("No assets in: " + AssetBundleName);
				return;
			}

			AssetBundleBuild[] Builds = new AssetBundleBuild[1];
			Builds[0].assetBundleName = AssetBundleName;
			Builds[0].assetNames = AssetPaths;

			// Saves some perf by disabling these unused loading options.
			BuildAssetBundleOptions Options = BuildAssetBundleOptions.DisableLoadAssetByFileName | BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;

			if(Multiplatform)
			{
				string OutputFile = OutputPath + '/' + AssetBundleName;
				string OutputManifest = OutputFile + ".manifest";

				string LinuxBundleName = MasterBundleHelper.getLinuxAssetBundleName(AssetBundleName);
				string LinuxOutputFile = OutputPath + '/' + LinuxBundleName;
				string LinuxManifest = LinuxOutputFile + ".manifest";

				string MacBundleName = MasterBundleHelper.getMacAssetBundleName(AssetBundleName);
				string MacOutputFile = OutputPath + '/' + MacBundleName;
				string MacManifest = MacOutputFile + ".manifest";

				// Delete existing files
				if(File.Exists(LinuxOutputFile))
					File.Delete(LinuxOutputFile);
				if(File.Exists(LinuxManifest))
					File.Delete(LinuxManifest);
				if(File.Exists(MacOutputFile))
					File.Delete(MacOutputFile);
				if(File.Exists(MacManifest))
					File.Delete(MacManifest);

				// Linux, then rename
				BuildPipeline.BuildAssetBundles(OutputPath, Builds, Options, BuildTarget.StandaloneLinux64);
				File.Move(OutputFile, LinuxOutputFile);
				File.Move(OutputManifest, LinuxManifest);

				// Mac, then rename
				BuildPipeline.BuildAssetBundles(OutputPath, Builds, Options, BuildTarget.StandaloneOSX);
				File.Move(OutputFile, MacOutputFile);
				File.Move(OutputManifest, MacManifest);
			}

			// Windows... finally done!
			BuildPipeline.BuildAssetBundles(OutputPath, Builds, Options, BuildTarget.StandaloneWindows64);

			// Unity (sometimes?) creates an empty bundle with the same name as the folder, so we delete it...
			string OutputDirName = Path.GetFileName(OutputPath);
			string EmptyBundlePath = OutputPath + "/" + OutputDirName;
			if(File.Exists(EmptyBundlePath))
			{
				File.Delete(EmptyBundlePath);
			}
			string EmptyManifestPath = EmptyBundlePath + ".manifest";
			if(File.Exists(EmptyManifestPath))
			{
				File.Delete(EmptyManifestPath);
			}

			HashAssetBundle(OutputPath + '/' + AssetBundleName);

#if GAME
			if(string.Equals(AssetBundleName, "core.masterbundle"))
			{
				PostBuildCoreMasterBundle();
			}
#endif
		}

		/// <summary>
		/// Combine per-platform hashes into a file for the server to load.
		/// </summary>
		private static void HashAssetBundle(string windowsFilePath)
		{
			string linuxFilePath = MasterBundleHelper.getLinuxAssetBundleName(windowsFilePath);
			string macFilePath = MasterBundleHelper.getMacAssetBundleName(windowsFilePath);

			if(!File.Exists(linuxFilePath) || !File.Exists(macFilePath))
			{
				Debug.Log("Skipping hash");
				return;
			}
			
			SHA1CryptoServiceProvider hashAlgo = new SHA1CryptoServiceProvider();
			byte[] windowsHash = hashAlgo.ComputeHash(File.ReadAllBytes(windowsFilePath));
			byte[] linuxHash = hashAlgo.ComputeHash(File.ReadAllBytes(linuxFilePath));
			byte[] macHash = hashAlgo.ComputeHash(File.ReadAllBytes(macFilePath));

			byte[] hashes = new byte[60];
			Array.Copy(windowsHash, 0, hashes, 0, 20);
			Array.Copy(linuxHash, 0, hashes, 20, 20);
			Array.Copy(macHash, 0, hashes, 40, 20);

#if UNTURNED_TEST
			Debug.LogFormat("Windows hash: {0}", Hash.toString(windowsHash));
			Debug.LogFormat("Linux hash: {0}", Hash.toString(linuxHash));
			Debug.LogFormat("Mac hash: {0}", Hash.toString(macHash));
#endif

			string hashFilePath = MasterBundleHelper.getHashFileName(windowsFilePath);
			File.WriteAllBytes(hashFilePath, hashes);
		}
	}
}

using System;
using UnityEditor;
using System.IO;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Task = System.Threading.Tasks.Task;

namespace Needle.AutoInstaller
{
    internal static class InstallationConfig
    {
        private static PackageData packageData;

        private static PackageData Data {
            get {
                if (packageData && AssetDatabase.GetAssetPath(packageData).EndsWith("PackageData.asset", StringComparison.Ordinal))
                    return packageData;

                // find local to here if not yet existing
                var folderPath
                    = // Path.GetDirectoryName(new StackFrame(true).GetFileName().Replace("\\","/").Replace(Application.dataPath, "Assets"));
                    PackagePath + "/Editor/AutoInstaller";
                var file = folderPath + "/PackageData.asset";
                if (!File.Exists(file)) {
                    packageData = null;
                    return packageData;
                }
                // file exists, refresh AssetDB
                //AssetDatabase.Refresh();
                //AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

                var data = AssetDatabase.LoadAssetAtPath<PackageData>(file);
                packageData = data ? data : null;

                return packageData;
            }
        }

        private static MonoScript monoScript;
        private static string PackagePath {
            get {
                if (!monoScript)
                    monoScript = MonoScript.FromScriptableObject(ScriptableObject.CreateInstance<PackageData>());
                var msPath = AssetDatabase.GetAssetPath(monoScript);
                return Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(msPath))));
            }
        }

        private const string InstallerName = "AutoInstaller";
        private static readonly bool DeleteInstallerWithoutAsking = true;
        private static string InstallationEditorPrefKey => Data ? Data.name : "AutoInstaller";
        private static string InstallationDontAskDeleteKey => InstallationEditorPrefKey + "_dontDelete";
        private const string LogPrefix = "<b>[Needle Installer]</b> ";

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorApplication.update += DelayedInit;
        }

        private static void DelayedInit() {
            if(Data) {
                EditorApplication.update -= DelayedInit;
                DelayedInitForReals();
            }
            else {
                Debug.Log("Waiting for Asset Database to catch up...");
            }
        }

        private static async void DelayedInitForReals() {
            // TODO we could do a bit better on 2019.3+ (?) with LockAssemblyReload to prevent recompilation/reloads
            if (!InstallationComplete && !isInstalling) {
                isInstalling = true;
                await Install();
            }

            // could install samples here
            isInstalling = false;
            await DeleteMe();
            AssetDatabase.Refresh();
            // Client.Resolve(); // should work in later 2020.2 versions
        }
        
        #if PACKAGE_INSTALLER_DEVELOPMENT
        [MenuItem("Tools/" + InstallerName + "/Clear All Editor Keys (Development)")]
        private static void ClearAllKeys() => DeleteEditorPrefs();
        #endif

        [MenuItem("Tools/" + InstallerName + "/Uninstall")]
        private static void CompleteUninstall()
        {
            foreach(var r in Data.registries)
                foreach(var p in Data.packages)
                    ManifestTools.PurgePackage(ManifestTools.ProjectManifestPath, p.name, r.name);
        }

        [MenuItem("Tools/" + InstallerName + "/Install")]
        private static async Task Install()
        {
            // if installation did run previously do not automatically run on recompile again
            // instead user should/could use menu items
            if (!DeleteInstallerWithoutAsking)
            {
                var installerCount = 0;
                if (EditorPrefs.HasKey(InstallationEditorPrefKey))
                {
                    installerCount = EditorPrefs.GetInt(InstallationEditorPrefKey);
                    if (installerCount > 0)
                        return;
                }

                EditorPrefs.SetInt(InstallationEditorPrefKey, ++installerCount);
            }

            bool success = true;
            var manifest = ManifestTools.ReadManifest(ManifestTools.ProjectManifestPath);
            foreach(var reg in Data.registries) {
                success &= manifest.AddOrUpdateRegistry(new RegistryInfo { Name = reg.name, Url = reg.url, Scopes = reg.scope.ToList() });
            }

            if (success) {
                manifest.WriteManifest(ManifestTools.ProjectManifestPath);

                foreach (var pac in Data.packages)
                    await InstallNow(pac.name, pac.version, pac.installType);

                AssetDatabase.Refresh();
                await Task.Delay(100);
                AssetDatabase.Refresh();
            }
            else {
              Debug.LogError("[PackageInstaller] And error occurred, please see console output. Installation will not proceed.");  
            }
        }

        private static bool InstallationComplete {
            get {
                if(!Data) return false;
                if(Data.packages == null) return false;

                foreach(var p in Data.packages)
                    if(!File.Exists("Packages/" + p.name + "/package.json")) return false;
                
                return true;
            }
        }

        private static bool isInstalling;
        private static float lastTryFindLogTime = -1000;

        private static async Task InstallNow(string packageName, string packageVersion, Package.InstallType installType)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (packageVersion == "latest" || installType != Package.InstallType.ExactVersion)
            {
                if (Time.time - lastTryFindLogTime > 10)
                {
                    // Debug.Log("[AR Simulation] Try find latest version of " + packageName);
                    lastTryFindLogTime = Time.time;
                }
                
                var version = await ManifestTools.TryGetLatestVersionFromPackMan(packageName, installType == Package.InstallType.Latest);
                if (version != null)
                {
                    packageVersion = version;
                    Log("Latest version for " + packageName + " is " + packageVersion + "; starting installation ✔");
                }
                else
                {
                    // "latest" was requested but not found.
                    if(packageVersion == "latest") {
                        LogError("Couldn't find a compatible version for " + packageName + ". Are you sure it's compatible with this version of Unity?");
                        return;
                    }
                    else {
                        Log("Install version for " + packageName + " is " + packageVersion + "; starting installation ✔");
                    }
                }
            }

            InstallFromPackageRegistry(ManifestTools.ProjectManifestPath, packageName, packageVersion);
            // ImportSample(quickstartSamples);
        }

        private static void InstallFromPackageRegistry(string manifestPath, string packageName, string packageVersion)
        {
            var manifest = ManifestTools.ReadManifest(manifestPath);
            var packageAlreadyInManifest = manifest.ContainsPackage(packageName);
            var packageActuallyExists = InstallationComplete;
            
            if (!packageAlreadyInManifest || !packageActuallyExists)
            {
                // only when we need credentials, e.g. GitHub packages
                // var credentialManager = new CredentialManager();
                // credentialManager.SetCredential(packageRegistryUrl, true, credentialToken);
                // credentialManager.Write();

                manifest.AddOrUpdatePackage(packageName,packageVersion);

#if !UNITY_2020_2_OR_NEWER
                EditorUtility.DisplayProgressBar("needle Installer", "Installing package " + packageName + "@" + packageVersion, 0);
#endif
                manifest.WriteManifest(manifestPath);
#if !UNITY_2020_2_OR_NEWER
                EditorUtility.ClearProgressBar();
#else
#endif
                AssetDatabase.Refresh();
                Log("Installed " + packageName + "@" + packageVersion + " ❤");
            }
            else
            {
                Log("Package " + packageName + " is already installed ❤");
            }
        }

        private static void Log(object o, Object context = null) {
#if UNITY_2019_1_OR_NEWER
            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, context, LogPrefix + o);
#else
            Debug.Log(LogPrefix + o, context);
#endif
        }
        
        private static void LogError(object obj)
        {
#if UNITY_2019_1_OR_NEWER
            Debug.LogFormat(LogType.Error, LogOption.NoStacktrace, null, LogPrefix + obj);
#else
            Debug.Log(obj);
#endif
        }

        private static void LogWarning(object obj)
        {
#if UNITY_2019_1_OR_NEWER
            Debug.LogFormat(LogType.Warning, LogOption.NoStacktrace, null, LogPrefix + obj);
#else
            Debug.Log(obj);
#endif
        }
        
        // private static void InstallFromGithub(string packageName, string packageVersion, bool forceOpen, bool showLogs)
        // {
        //     var originalManifest = ManifestTools.OpenManifest();

        //     var packageAlreadyInManifest = ManifestTools.ContainsPackage(ref originalManifest, packageName);
        //     var packageActuallyExists = File.Exists("Packages/" + packageName + "/package.json");

        //     if (forceOpen || !packageAlreadyInManifest || !packageActuallyExists)
        //     {
        //         var registryUrl = "https://npm.pkg.github.com/@needle-tools";

        //         ManifestTools.ShowCredentialsWindow(packageName, registryUrl, token =>
        //         {
        //             if (string.IsNullOrEmpty(token))
        //             {
        //                 Debug.LogError("No token specified. Can't add package");
        //                 return;
        //             }

        //             var manifest = ManifestTools.OpenManifest();

        //             ManifestTools.EnsureScopedRegistry(
        //                 ref manifest,
        //                 "needle-github",
        //                 registryUrl,
        //                 "com.needle"
        //             );

        //             var credentialManager = new CredentialManager();
        //             credentialManager.SetCredential(registryUrl, true, token);
        //             credentialManager.Write();

        //             ManifestTools.AddOrUpdatePackage(
        //                 ref manifest,
        //                 packageName,
        //                 packageVersion
        //             );

        //             ManifestTools.WriteManifest(manifest);
        //         });
        //     }
        //     else
        //     {
        //         if (showLogs)
        //             Debug.Log("Package \"" + packageName + "\"is already installed.");
        //     }
        // }

        private static async Task DeleteMe()
        {
            if (!DeleteInstallerWithoutAsking && EditorPrefs.HasKey(InstallationDontAskDeleteKey) && EditorPrefs.GetBool(InstallationDontAskDeleteKey)) return;

            if (!DeleteInstallerWithoutAsking)
            {
                var userWantsDelete = EditorUtility.DisplayDialog("Auto Installation", "Installation completed. Do you want to remove the installer?",
                    "Yes, remove installer",
                    "No, don't remove installer");
                if (!userWantsDelete)
                {
                    EditorPrefs.SetBool(InstallationDontAskDeleteKey, true);
                    return;
                }
            }
            
            /*
            var frame = new StackFrame(true);
            
            var fullPath = frame.GetFileName().Replace("\\","/");
            if (string.IsNullOrEmpty(fullPath)) return;
            fullPath = Path.GetDirectoryName(fullPath).Replace("\\","/");
            // HACK to remove the top folder as well
            fullPath = fullPath.Replace("Packages/com.needle.auto-installer/Editor/AutoInstaller", "Packages/com.needle.auto-installer");
            */
            var fullPath = Path.GetFullPath(PackagePath);
            // check that this is an embedded package, otherwise we must not delete it!
            var embeddedPackagesPath = Path.GetDirectoryName(Application.dataPath) + Path.DirectorySeparatorChar + "Packages";
            
            if (!string.IsNullOrEmpty(fullPath) && Directory.Exists(fullPath))
            {
                DeleteEditorPrefs();
                AssetDatabase.Refresh();
                
                while (EditorApplication.isUpdating || EditorApplication.isCompiling)
                {
                    await Task.Delay(500);
                }

                string GetDevLog()
                {
                    return "<b>[Development Version]</b> Installer not deleted from " + fullPath.Replace('\\', '/').Replace(Application.dataPath, "Assets");
                }
                
                if(fullPath.StartsWith(embeddedPackagesPath, StringComparison.Ordinal)) {
#if !PACKAGE_INSTALLER_DEVELOPMENT
                        // AssetDatabase.DeleteAsset(fullPath);
                        Directory.Delete(fullPath, true);
                        var metaPath = fullPath + ".meta";
                        if (File.Exists(metaPath))  File.Delete(metaPath);
                        Log("Installer deleted from <i>" + fullPath + "</i> ✔");
                        AssetDatabase.Refresh();
#else
                        Debug.Log(GetDevLog() + " (you're in development mode)");                        
#endif
                }
                else {
                    Debug.Log(GetDevLog() + " (only embedded packages are allowed to be deleted)");
                }
            }
        }
        
#if PACKAGE_INSTALLER_DEVELOPMENT
        [MenuItem("pfc/Installer Log Now")]
        private static void LogNow() {
            Debug.Log(PackagePath);
        }
#endif

        private static void DeleteEditorPrefs()
        {
            if (EditorPrefs.HasKey(InstallationEditorPrefKey))
                EditorPrefs.DeleteKey(InstallationEditorPrefKey);

            if(EditorPrefs.HasKey(InstallationDontAskDeleteKey)) 
                EditorPrefs.DeleteKey(InstallationDontAskDeleteKey);

        }
#endif
    }
}
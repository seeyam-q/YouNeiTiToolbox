using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Needle.AutoInstaller
{
    [System.Serializable]
    public class PackageData : ScriptableObject {
        public ScopedRegistry[] registries = null;
        public Package[] packages = null;
    }
    
    [System.Serializable]
    public class ScopedRegistry {
        public string name;
        public string url;
        public string[] scope = null;
        // this will show a credentials window
        // (and potentially install Halodi Package Registry Manager?)
        public bool needsCredentials = false;
    }

    [System.Serializable]
    public class Package {
        public string name;
        public string version;
        public enum InstallType {
            ExactVersion,
            Latest,
            LatestNonPreview
        }
        public InstallType installType = InstallType.LatestNonPreview;
    }
}

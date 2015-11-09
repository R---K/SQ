using UnityEngine;

using UnityEditor;


public class CreateBundleScript{
 [MenuItem("Assets/Build AssetBundle")]
        static void ExportResource () {
            string path = "BundleRes/02Bundle.unity3d";
            Object[] selection =  Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            BuildPipeline.BuildAssetBundle(Selection.activeObject, selection, path, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets);
        }
    }


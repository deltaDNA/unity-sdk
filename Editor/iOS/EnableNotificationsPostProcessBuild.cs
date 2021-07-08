////
// Copyright (c) 2016 deltaDNA Ltd. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS && UNITY_5_4_OR_NEWER
using UnityEditor.iOS.Xcode;
#endif
using System.Collections;
using System.Diagnostics;
using System.IO;

// http://answers.unity3d.com/questions/1225564/enable-unity-uses-remote-notifications.html
// https://unity3d.com/unity/whats-new/unity-5.4.0 (525606)
public sealed class EnableNotificationsPostProcessBuild {
    
    [PostProcessBuild(1)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
        #if (UNITY_IOS && UNITY_5_4_OR_NEWER)
        string preprocessor = path + "/Classes/Preprocessor.h";
        File.WriteAllText(
            preprocessor,
            File.ReadAllText(preprocessor)
                .Replace(
                    "UNITY_USES_REMOTE_NOTIFICATIONS 0",
                    "UNITY_USES_REMOTE_NOTIFICATIONS 1"));
        #endif
    }
}

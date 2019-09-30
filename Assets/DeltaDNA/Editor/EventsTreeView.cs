#if UNITY_2017_1_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;

namespace DeltaDNA.Editor{
   class EventsTreeView : TreeView{
            private readonly Action<Dictionary<string, object>> _callback;

            private List<object> data;

            public EventsTreeView(TreeViewState treeViewState, Action<Dictionary<string, object>> callback,
                List<object> data)
                : base(treeViewState){
                _callback = callback;
                this.data = data;
                Reload();
            }

            protected override TreeViewItem BuildRoot(){
                var rootElement = new TreeViewItem{id = 0, depth = -1, displayName = "Events"};
                int currentId = 1;
                foreach (var eventDataObj in data){
                    Dictionary<string, object> eventDataDict = eventDataObj as Dictionary<string, object>;
                    eventDataDict.Add("ddna_tree_type", "EVENT");
                    var currentNode = new DDNATreeViewItem(eventDataDict){
                        id = currentId,
                        displayName = eventDataDict["name"] as string
                    };
                    rootElement.AddChild(currentNode);
                    currentId++;
                    currentId = GenerateChildren(currentNode, eventDataDict, currentId, "parameters");
                }

                SetupDepthsFromParentsAndChildren(rootElement);

                return rootElement;
            }

            private int GenerateChildren(TreeViewItem parentNode, Dictionary<string, object> eventDataDict,
                int currentId, string searchTerm){
                if (eventDataDict.ContainsKey(searchTerm)){
                    List<object> paramsDict = eventDataDict[searchTerm] as List<object>;
                    foreach (var paramDictObj in paramsDict){
                        Dictionary<string, object> paramDict = paramDictObj as Dictionary<string, object>;
                        paramDict.Add("ddna_tree_type", "PARAMETER");
                        var currentNode = new DDNATreeViewItem(paramDict){
                            id = currentId,
                            displayName = paramDict["name"] as string
                        };
                        parentNode.AddChild(currentNode);
                        currentId++;
                        currentId = GenerateChildren(currentNode, paramDict, currentId, "children");
                    }
                }

                return currentId;
            }

            protected override void SelectionChanged(IList<int> selectedIds){
                DDNATreeViewItem selectedItem = FindItem(selectedIds.First(), rootItem) as DDNATreeViewItem;
                _callback(selectedItem.data);
            }

            class DDNATreeViewItem : TreeViewItem{
                public DDNATreeViewItem(Dictionary<string, object> data){
                    this.data = data;
                }

                public Dictionary<string, object> data{ get; }
            }
        }
}
#endif
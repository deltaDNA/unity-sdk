//
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

using System;
using System.Collections.Generic;

namespace DeltaDNA
{
    public class PopupEventArgs: EventArgs
    {
        public PopupEventArgs(string id, string type, string value)
        {
            this.ID = id;
            this.ActionType = type;
            this.ActionValue = value;
        }

        public string ID { get; set; }
        public string ActionType { get; set; }
        public string ActionValue { get; set; }
    }
        
    public interface IPopup
    {
        event EventHandler BeforePrepare;
        event EventHandler AfterPrepare;
        event EventHandler BeforeShow;
        event EventHandler BeforeClose;
        event EventHandler AfterClose;
        event EventHandler<PopupEventArgs> Dismiss;
        event EventHandler<PopupEventArgs> Action;

        void Prepare(Dictionary<string, object> configuration);
        void Show();
        void Close();

        void OnDismiss(PopupEventArgs eventArgs);
        void OnAction(PopupEventArgs eventArgs);
    }
}


using System;
using System.Collections;
using System.ComponentModel;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DeltaDNA{

    public class OrientationChange : MonoBehaviour{
        private event Action onChange;

        private Vector2 resolution; 
        private DeviceOrientation orientation;
        private bool running = true;

        private OrientationChange(){
            
        }

        public void Init(Action onChange){
            this.onChange = onChange;
        }
        

        void Start(){
            StartCoroutine(CheckForChange());
        }
        
        IEnumerator CheckForChange(){
            resolution = new Vector2(Screen.width, Screen.height);
            orientation = Input.deviceOrientation;

            while (running){
                bool changed = false; 
                if (resolution.x != Screen.width || resolution.y != Screen.height){
                    resolution = new Vector2(Screen.width, Screen.height);
                    changed = true;
                }
                if (orientation != Input.deviceOrientation){
                    orientation = Input.deviceOrientation;
                    changed = true;
                }

                if (changed && onChange != null) onChange();

                yield return new WaitForSeconds(0.05f);
            }
        }

        void OnDestroy(){
            onChange = null;
            running = false;
        }

    }
}
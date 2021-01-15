#if !UNITY_4
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace DeltaDNA{

    public abstract class SimpleDataStore<K, V>{
        
        private static object LOCK = new object();

        private readonly string location = Application.temporaryCachePath + "/deltadna/";
        private readonly string storename;

        private Dictionary<K, V> data;
        private char paramSeparator;
        
        internal SimpleDataStore(string directory, string storename, char paramSeparator = ' '){

            directory = !directory.EndsWith("/") ? directory + "/" : directory;
            
            this.location = Application.temporaryCachePath + "/deltadna/" + directory;
            this.paramSeparator = paramSeparator;
            this.storename = storename;
            
            lock (LOCK) {
                CreateDirectory();
                
                if (File.Exists(location + storename)) {
                    data = File
                        .ReadAllLines(location + storename)
                        .ToDictionary(
                            e => parseKey(e.Split(this.paramSeparator)[0]),
                            e => parseValue(e.Split(this.paramSeparator)[1]));
                } else {
                    data = new Dictionary<K, V>();
                }
            }
        }

        protected abstract K parseKey(string key);

        protected abstract V parseValue(string value);

      
        protected abstract string createLine(K key, V value);
        
        public void Put(K key, V value){
            lock (LOCK){
                data[key] = value;
            }
            Save();
            
        }

        
        public V GetOrDefault(K key, V defaultValue){
            lock (LOCK){
                if (data.ContainsKey(key)){
                    return data[key];
                }
            }
            return defaultValue;
        }

        internal void Save() {
            lock (LOCK) {
                CreateDirectory();

                
                File.WriteAllLines(
                    location + storename,
                    data.Select(e => this.createLine(e.Key, e.Value)).ToArray());
            }
        }

        internal void Clear() {
            lock (LOCK) {
                data.Clear();
                Save();
            }
        }

        private void CreateDirectory() {
            if (!Directory.Exists(location)){
                Directory.CreateDirectory(location);
            }
        }

        protected char getKeyValueSeparator(){
            return this.paramSeparator;
        }

    }
    
}

#endif
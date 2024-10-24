using System;
using System.Text;
using Data;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Logic
{
    public class GameController : MonoBehaviour
    {
        private bool _isPlaying;
        private static string _text = "Hello World! Type something here and press \"Start Simulation\"! The longer you write the more will happen! The longer the text, the more cool stuff might happen, so feel free to paste the first verse of your favourite song or something. NB: Might cause major lag!   ";
        private static float Zoom = -40f;

        private static float speedUntouchedFor = 0f;

        private void OnEnable()
        {
            QualitySettings.vSyncCount = 1;
            var cam = Camera.main;
            if (cam == null) return;
            cam.transform.position = new Vector3(0, 0, Zoom);
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 500, 200));
            DoGUI();
            GUILayout.EndArea();
        }

        private void DoGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"TimeScale: {Mathf.Round(Time.timeScale * 10) / 10}");
            var oldTimescale = Time.timeScale;
            Time.timeScale = GUILayout.HorizontalSlider(Time.timeScale, 0, 5f);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (oldTimescale == Time.timeScale)
            {
                speedUntouchedFor += Time.unscaledDeltaTime;
                if (speedUntouchedFor > 5)
                {
                    Time.timeScale = Mathf.Round(Time.timeScale * 10) / 10;
                    speedUntouchedFor = -1000;
                }
            }
            else
            {
                speedUntouchedFor = 0;
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            DoZoom();

            if (_isPlaying)
            {
                if (GUILayout.Button("Stop Simulation"))
                {
                    World.DisposeAllWorlds();
                    DefaultWorldInitialization.Initialize("Default World");
                    SceneManager.LoadScene(0);
                }

                GUILayout.EndHorizontal();

                return;
            }

            if (GUILayout.Button("Start Simulation"))
            {
                _isPlaying = true;
                var em = World.DefaultGameObjectInjectionWorld.EntityManager;
                var entity = em.CreateSingletonBuffer<FireworkInstruction>();
                var buffer = em.GetBuffer<FireworkInstruction>(entity);
                var bytes = Encoding.UTF8.GetBytes(_text + "\0\0\0\0");
                var previous = 0x70707070;
                var row = 0x11FF22EE33DD44CC;
                previous ^= bytes.Length;
                for (var i = 0; i < bytes.Length - 4; i += 4)
                {
                    var a = bytes[i + 0] ^ bytes[i + 1];
                    var b = bytes[i + 1] ^ bytes[i + 2];
                    var c = bytes[i + 2] ^ bytes[i + 0];
                    var d = a ^ bytes[i + 2] ^ bytes[i + 3];
                    var value = a | (b << 8) | (c << 16) | (d << 24);
                    value ^= previous;
                    value ^= i;
                    previous ^= bytes[i + 0] | (bytes[i + 1] << 8) | (bytes[i + 2] << 16) | (bytes[i + 2] << 24);
                    // ReSharper disable once IntVariableOverflowInUncheckedContext
                    previous ^= (int)row;
                    row = (row >> 8) | (row << 8);

                    buffer.Add(new FireworkInstruction {Data = (uint)value});
                }
            }

            GUILayout.EndHorizontal();

            _text = GUILayout.TextArea(_text);
        }

        private void DoZoom()
        {
            var cam = Camera.main;
            if (cam == null) return;
            if (GUILayout.Button("Zoom Closer"))
            {
                Zoom = -15;
                cam.transform.position = new Vector3(0, 0, Zoom);
            }

            if (GUILayout.Button("Zoom Close"))
            {
                Zoom = -40;
                cam.transform.position = new Vector3(0, 0, Zoom);
            }

            if (GUILayout.Button("Zoom Far"))
            {
                Zoom = -80;
                cam.transform.position = new Vector3(0, 0, Zoom);
            }

            if (GUILayout.Button("Zoom Farer"))
            {
                Zoom = -150;
                cam.transform.position = new Vector3(0, 0, Zoom);
            }
        }
    }
}
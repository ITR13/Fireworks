using System;
using System.Text;
using Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Logic
{
    public class GameController : MonoBehaviour
    {
        private bool _isPlaying;
        private static string _text = "Hello World! Type something here and press \"Start Simulation\"! The longer you write the more will happen! The longer the text, the more cool stuff might happen, so feel free to paste the first verse of your favourite song or something. NB: Might cause major lag!   ";
        private static float Zoom = -40f;

        private static float speedUntouchedFor = 0f;
        private float _guiScale;

        private static string[] _randomTexts = new[]
        {
            "The FitnessGram Pacer Test is a multistage aerobic capacity test that progressively gets more difficult as it continues. The 20 meter pacer test will begin in 30 seconds. Line up at the start. The running speed starts slowly but gets faster each minute after you hear this signal bodeboop. A sing lap should be completed every time you hear this sound. ding Remember to run in a straight line and run as long as possible. The second time you fail to complete a lap before the sound, your test is over. The test will begin on the word start. On your mark. Get ready!… Start. ding",
            "NARRATOR:\n(Black screen with text; The sound of buzzing bees can be heard)\nAccording to all known laws\nof aviation,\n :\nthere is no way a bee\nshould be able to fly.\n :\nIts wings are too small to get\nits fat little body off the ground.\n :\nThe bee, of course, flies anyway\n :\nbecause bees don't care\nwhat humans think is impossible.\nBARRY BENSON:\n(Barry is picking out a shirt)\nYellow, black. Yellow, black.\nYellow, black. Yellow, black.\n :\nOoh, black and yellow!\nLet's shake it up a little.\nJANET BENSON:\nBarry! Breakfast is ready!\nBARRY:\nComing!\n :\nHang on a second.\n(Barry uses his antenna like a phone)",
            "What the fuck did you just fucking say about me, you little bitch? Ill have you know I graduated top of my class in the Navy Seals, and Ive been involved in numerous secret raids on Al-Quaeda, and I have over 300 confirmed kills. I am trained in gorilla warfare and Im the top sniper in the entire US armed forces. You are nothing to me but just another target. I will wipe you the fuck out with precision the likes of which has never been seen before on this Earth, mark my fucking words. You think you can get away with saying that shit to me over the Internet? Think again, fucker. As we speak I am contacting my secret network of spies across the USA and your IP is being traced right now so you better prepare for the storm, maggot. The storm that wipes out the pathetic little thing you call your life. Youre fucking dead, kid. I can be anywhere, anytime, and I can kill you in over seven hundred ways, and thats just with my bare hands. Not only am I extensively trained in unarmed combat, but I have access to the entire arsenal of the United States Marine Corps and I will use it to its full extent to wipe your miserable ass off the face of the continent, you little shit. If only you could have known what unholy retribution your little clever comment was about to bring down upon you, maybe you would have held your fucking tongue. But you couldnt, you didn't, and now youre paying the price, you goddamn idiot. I will shit fury all over you and you will drown in it. You're fucking dead, kiddo.",
            "DO IT, just DO IT! Don't let your dreams be dreams. Yesterday, you said tomorrow. So just. DO IT! Make. your dreams. COME TRUE! Just... do it! Some people dream of success, while you're gonna wake up and work HARD at it! NOTHING IS IMPOSSIBLE!You should get to the point where anyone else would quit, and you're not gonna stop there. NO! What are you waiting for? ... DO IT! Just... DO IT! Yes you can! Just do it! If you're tired of starting over, stop. giving. up.",
            "Did you ever hear the tragedy of Darth Plagueis \"the wise\"? I thought not. It's not a story the Jedi would tell you. It's a Sith legend. Darth Plagueis was a Dark Lord of the Sith, so powerful and so wise he could use the Force to influence the midichlorians to create life... He had such a knowledge of the dark side that he could even keep the ones he cared about from dying. The dark side of the Force is a pathway to many abilities some consider to be unnatural. He became so powerful... the only thing he was afraid of was losing his power, which eventually, of course, he did. Unfortunately, he taught his apprentice everything he knew, then his apprentice killed him in his sleep. It's ironic he could save others from death, but not himself.",
            "\u2880\u2874\u2811\u2844\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28c0\u28c0\u28e4\u28e4\u28e4\u28c0\u2840\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800 \n\u2838\u2847\u2800\u283f\u2840\u2800\u2800\u2800\u28c0\u2874\u28bf\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28f7\u28e6\u2840\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800 \n\u2800\u2800\u2800\u2800\u2811\u2884\u28e0\u283e\u2801\u28c0\u28c4\u2848\u2819\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28c6\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800 \n\u2800\u2800\u2800\u2800\u2880\u2840\u2801\u2800\u2800\u2808\u2819\u281b\u2802\u2808\u28ff\u28ff\u28ff\u28ff\u28ff\u283f\u287f\u28bf\u28c6\u2800\u2800\u2800\u2800\u2800\u2800\u2800 \n\u2800\u2800\u2800\u2880\u287e\u28c1\u28c0\u2800\u2834\u2802\u2819\u28d7\u2840\u2800\u28bb\u28ff\u28ff\u282d\u28a4\u28f4\u28e6\u28e4\u28f9\u2800\u2800\u2800\u2880\u28b4\u28f6\u28c6 \n\u2800\u2800\u2880\u28fe\u28ff\u28ff\u28ff\u28f7\u28ee\u28fd\u28fe\u28ff\u28e5\u28f4\u28ff\u28ff\u287f\u2882\u2814\u289a\u287f\u28bf\u28ff\u28e6\u28f4\u28fe\u2801\u2838\u28fc\u287f \n\u2800\u2880\u285e\u2801\u2819\u283b\u283f\u281f\u2809\u2800\u281b\u28b9\u28ff\u28ff\u28ff\u28ff\u28ff\u28cc\u28a4\u28fc\u28ff\u28fe\u28ff\u285f\u2809\u2800\u2800\u2800\u2800\u2800 \n\u2800\u28fe\u28f7\u28f6\u2807\u2800\u2800\u28e4\u28c4\u28c0\u2840\u2808\u283b\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u2847\u2800\u2800\u2800\u2800\u2800\u2800 \n\u2800\u2809\u2808\u2809\u2800\u2800\u28a6\u2848\u28bb\u28ff\u28ff\u28ff\u28f6\u28f6\u28f6\u28f6\u28e4\u28fd\u2879\u28ff\u28ff\u28ff\u28ff\u2847\u2800\u2800\u2800\u2800\u2800\u2800 \n\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2809\u2832\u28fd\u287b\u28bf\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28f7\u28dc\u28ff\u28ff\u28ff\u2847\u2800\u2800\u2800\u2800\u2800\u2800 \n\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28b8\u28ff\u28ff\u28f7\u28f6\u28ee\u28ed\u28fd\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u2800\u2800\u2800\u2800\u2800\u2800\u2800 \n\u2800\u2800\u2800\u2800\u2800\u2800\u28c0\u28c0\u28c8\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u2807\u2800\u2800\u2800\u2800\u2800\u2800\u2800 \n\u2800\u2800\u2800\u2800\u2800\u2800\u28bf\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u2803\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800 \n\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2839\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u287f\u281f\u2801\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800 \n\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2809\u281b\u283b\u283f\u283f\u283f\u283f\u281b\u2809",
            "Sticking out your gyatt for the rizzler\nYou're so skibidi\nYou're so Fanum tax\nI just wanna be your sigma\nFreaking come here\nGive me your Ohio",
            "Crazy? I was crazy once.\nThey put me in a room.\nA rubber room.\nA rubber room with rats.\nThey put me in a rubber room with rubber rats.\nRubber rats? I hate rubber rats.\nThey make me crazy.\nCrazy? I was crazy once.\nThey put me in a room….",
            "Crazy? I was crazy once.\nThey put me in a rubber room.\nA round rubber room\nAnd told me to sit in the corner.\n\nCorner? There was no corner\nSo I sat in the middle of the floor\nSurrounded by rats.\n\nRat? I can't stand rats.\nThey tickled my feet.\nThey made me laugh.\nI laughed so hard I thought I would die\n\nDie? I did die in there.\nThey buried me in a hole.\nA hole six feet under the ground\nAnd that's when the worms came.\n\nWorms? I hate worms.\nThey burrowed into my skull And started to eat my brain.\nThey drove me crazy!\n\nCrazy? I was crazy once.\nThey put me in a rubber room.\nA round rubber room\nAnd told me to sit in the corner.",
            "We're no strangers to love\nYou know the rules and so do I (Do I)\nA full commitment's what I'm thinking of\nYou wouldn't get this from any other guy\n\nI just wanna tell you how I'm feeling\nGotta make you understand\n\nNever gonna give you up\nNever gonna let you down\nNever gonna run around and desert you\nNever gonna make you cry\nNever gonna say goodbye\nNever gonna tell a lie and hurt you",
            "頃□礶썦緶葓□限㳪□蜦赗昽쯘玐ⓧ□頳□唛凰픕苣驂骊角溉諱□䌯蜧□㇣Ɦ□퍎枣□菤↾㮃즉号㇀諆䱖갿㳊蓀䄑□䂺□曹亘□□□䪫훕士辞꼌퓼□抸□凔જ宭□□ૉ□왨ꯃ爭닟□쿉□哩랲骈□ά□戀狲承좂□重剩崥橜嚙□베□᱑□≸□芔龘쭯皆ః勻⊘尉□猝□ൺ□彅颎饳멽□䯇□煢蝽벪ͱ□□⬗厦漹乩셜□릭□ⷣ쌵䪟랺輶⌨뱏欀살□瀲ోδ□椹硫倛볈□澺无唊湹⽿ꭡ钹쪇魢□葆□녪錭艦Ỏ偬疴□□㶏□蓱￮뙔汚□龙ㅣ⒑□촛瞾塑涾□瀽噵竴□□□珏⤶蝴쯝䠋羥ὲ歄䁔□檩ȃ䞴□㥨□睎溉쯵꺗□Ⅱ매轅蓇쿰탁□컌쫡区痰䚷㽔□□嚸硢褔㨂谤□綵□嚮뮄滦□狚䆠",
            "200",
            "500",
            "1000",
        };


        private void OnEnable()
        {
            QualitySettings.vSyncCount = 1;
            var cam = Camera.main;
            if (cam == null) return;
            cam.transform.position = new Vector3(0, 0, Zoom);
        }

        private void Update()
        {
            _guiScale = math.cmin(new float2(Screen.width, Screen.height) / new float2(1920, 1080));
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 500, 200));
            DoGUI();
            GUILayout.EndArea();
        }

        private void DoGUI()
        {
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(_guiScale, _guiScale, _guiScale));
            GUILayout.BeginHorizontal();
            if (Time.timeScale <= 0) GUI.color = Color.red;
            GUILayout.Label($"TimeScale: {Mathf.Round(Time.timeScale * 10) / 10}");
            GUI.color = Color.white;
            var oldTimescale = Time.timeScale;
            Time.timeScale = GUILayout.HorizontalSlider(Time.timeScale, 0, 5f, GUILayout.Width(300));
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

            GUILayout.FlexibleSpace();
            if (!_isPlaying)
            {
                if (GUILayout.Button("Random"))
                {
                    _text = _randomTexts[Random.Range(0, _randomTexts.Length)];
                    if (int.TryParse(_text, out var num))
                    {
                        var random = new Unity.Mathematics.Random((uint)Random.Range(1, int.MaxValue));
                        var arr = new NativeArray<uint>(num / 4, Allocator.Temp);
                        for (var i = 0; i < arr.Length; i++)
                        {
                            arr[i] = random.NextUInt();
                        }

                        _text = new string(arr.Reinterpret<char>(4));
                    }
                }
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
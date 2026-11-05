using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Transitions
{
    // Fades the screen out and back in, around a scene load or anything else.
    // It creates itself the first time it's used and stays across scenes, so no scene setup is needed.
    //
    //   SceneFader.LoadScene("Chapter 1");              // fade out, load, fade in
    //   SceneFader.FadeOutIn(() => screen.Show());       // fade out, do something, fade in
    public class SceneFader : MonoBehaviour
    {
        // Change these to restyle every transition in the game.
        public static float FadeDuration = 0.5f;
        public static Color FadeColor = Color.black;

        private static SceneFader _instance;

        private CanvasGroup _group;
        private UnityEngine.UI.Image _image;
        private bool _busy;

        public static bool IsFading => _instance != null && _instance._busy;

        public static void LoadScene(string sceneName)
        {
            Instance.Run(() => SceneManager.LoadScene(sceneName), waitOneFrame: true);
        }

        public static void FadeOutIn(Action whileHidden)
        {
            Instance.Run(whileHidden, waitOneFrame: false);
        }

        private static SceneFader Instance
        {
            get
            {
                if (_instance == null) _instance = Create();
                return _instance;
            }
        }

        private static SceneFader Create()
        {
            var go = new GameObject("SceneFader", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup),
                typeof(UnityEngine.UI.GraphicRaycaster));
            DontDestroyOnLoad(go);

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // above every other UI

            var imageGo = new GameObject("Fade", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            imageGo.transform.SetParent(go.transform, false);
            var rect = imageGo.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var fader = go.AddComponent<SceneFader>();
            fader._group = go.GetComponent<CanvasGroup>();
            fader._image = imageGo.GetComponent<UnityEngine.UI.Image>();
            fader.SetAlpha(0f);
            return fader;
        }

        private void Run(Action whileHidden, bool waitOneFrame)
        {
            if (_busy) return;
            StartCoroutine(Fade(whileHidden, waitOneFrame));
        }

        private IEnumerator Fade(Action whileHidden, bool waitOneFrame)
        {
            _busy = true;
            _image.color = FadeColor;

            yield return Animate(0f, 1f);

            var sceneLoaded = false;
            void OnSceneLoaded(Scene scene, LoadSceneMode mode) => sceneLoaded = true;
            SceneManager.sceneLoaded += OnSceneLoaded;

            whileHidden?.Invoke();

            if (waitOneFrame)
            {
                // Wait until the new scene has actually loaded, then a couple of frames
                // so its Start code has run and the loading hitch is over before revealing it.
                // Give up after 10 seconds (e.g. a misspelled scene name), so the screen never stays black.
                var waited = 0f;
                while (!sceneLoaded && waited < 10f)
                {
                    waited += Time.unscaledDeltaTime;
                    yield return null;
                }
                yield return null;
                yield return null;
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;

            yield return Animate(1f, 0f);
            _busy = false;
        }

        private IEnumerator Animate(float from, float to)
        {
            // Unscaled time, so fades still work if the game is paused.
            // Each step is capped, so one slow frame (like right after loading) can't skip the fade.
            const float maxStep = 1f / 30f;

            var time = 0f;
            while (time < FadeDuration)
            {
                time += Mathf.Min(Time.unscaledDeltaTime, maxStep);
                SetAlpha(Mathf.Lerp(from, to, time / FadeDuration));
                yield return null;
            }
            SetAlpha(to);
        }

        private void SetAlpha(float alpha)
        {
            _group.alpha = alpha;
            // Block clicks while the screen is (partly) covered.
            _group.blocksRaycasts = alpha > 0.01f;
        }
    }
}

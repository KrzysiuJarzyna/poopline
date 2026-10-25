using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Poopline
{
    public sealed class RunnerHud : MonoBehaviour
    {
        private PooplineGame game;
        private Font font;
        private RectTransform safeRoot;
        private RectTransform progress;
        private Text status, title, message, actionText, soundText, cityTitle, mapSelection;
        private GameObject overlay, restartButton, jumpButton, pauseButton, mapScreen, controls, headerObject;
        private RectTransform mapCard;
        private Text[] cityLabels, cityDots;
        private int previousCity = -1;
        private Rect lastSafeArea;
        private Vector2Int lastScreen;
        private RunPhase previousPhase = (RunPhase)(-1);
        private float nextRefresh;
        private static readonly Color Ink = new Color32(17, 34, 40, 255);
        private static readonly Color Cream = new Color32(238, 241, 214, 255);
        private static readonly Color Lime = new Color32(190, 226, 131, 255);

        public void Initialize(PooplineGame owner)
        {
            game = owner;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvasGo = new GameObject("Poopline UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
            safeRoot = Rect("Safe area", canvasGo.transform);
            Stretch(safeRoot);

            var header = Panel("Header", safeRoot, new Color32(17, 34, 40, 242));
            headerObject = header.gameObject;
            header.anchorMin = new Vector2(0, 1); header.anchorMax = Vector2.one;
            header.pivot = new Vector2(0.5f, 1); header.sizeDelta = new Vector2(0, 116);
            var brand = Label("POOPLINE", header, 28, TextAnchor.MiddleLeft);
            cityTitle = brand;
            Place(brand.rectTransform, new Vector2(0, 1), new Vector2(164, -35), new Vector2(280, 44));
            status = Label("", header, 19, TextAnchor.MiddleLeft);
            Place(status.rectTransform, new Vector2(0, 1), new Vector2(230, -77), new Vector2(412, 32));
            pauseButton = Button("PAUZA", header, game.TogglePause, out _);
            Place((RectTransform)pauseButton.transform, Vector2.one, new Vector2(-80, -35), new Vector2(116, 44));
            var mute = Button("", header, game.ToggleSound, out soundText);
            Place((RectTransform)mute.transform, Vector2.one, new Vector2(-80, -81), new Vector2(116, 36));
            soundText.fontSize = 14;
            var mapButton = Button("MAPA", header, game.ReturnToMap, out _);
            Place((RectTransform)mapButton.transform, Vector2.one, new Vector2(-210, -35), new Vector2(116, 44));

            var track = Panel("Progress track", header, new Color32(55, 77, 74, 255));
            track.anchorMin = new Vector2(0, 0); track.anchorMax = new Vector2(1, 0);
            track.sizeDelta = new Vector2(0, 4); track.anchoredPosition = Vector2.zero;
            progress = Panel("Progress", track, Lime);
            Stretch(progress);

            controls = Rect("Movement controls", safeRoot).gameObject;
            Stretch((RectTransform)controls.transform);
            CreateDirectionButton("<", -1, new Vector2(80, 66));
            CreateDirectionButton(">", 1, new Vector2(210, 66));
            jumpButton = new GameObject("Jump button", typeof(RectTransform), typeof(Image), typeof(JumpButton));
            jumpButton.transform.SetParent(controls.transform, false);
            jumpButton.GetComponent<Image>().color = Lime;
            jumpButton.GetComponent<JumpButton>().Game = game;
            Place((RectTransform)jumpButton.transform, new Vector2(1, 0), new Vector2(-104, 66), new Vector2(160, 88));
            var jumpLabel = Label("SKOK", jumpButton.transform, 29, TextAnchor.MiddleCenter);
            jumpLabel.color = Ink; Stretch(jumpLabel.rectTransform);
            var hint = Label("A/D lub strzałki — ruch   •   Spacja — skok", controls.transform, 16, TextAnchor.MiddleLeft);
            Place(hint.rectTransform, Vector2.zero, new Vector2(234, 134), new Vector2(420, 32));

            overlay = Panel("Menu overlay", safeRoot, new Color32(8, 19, 23, 210)).gameObject;
            Stretch((RectTransform)overlay.transform);
            var card = Panel("Menu card", overlay.transform, Ink);
            Place(card, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560, 390));
            var eyebrow = Label("01  /  PROSTO Z RURY", card, 16, TextAnchor.MiddleCenter);
            eyebrow.color = Lime;
            Place(eyebrow.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -36), new Vector2(480, 30));
            title = Label("", card, 42, TextAnchor.MiddleCenter);
            Place(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -95), new Vector2(520, 70));
            message = Label("", card, 21, TextAnchor.MiddleCenter);
            Place(message.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -180), new Vector2(490, 100));
            var primary = Button("", card, game.PrimaryAction, out actionText);
            primary.GetComponent<Image>().color = Lime;
            actionText.color = Ink; actionText.fontSize = 23;
            Place((RectTransform)primary.transform, new Vector2(0.5f, 0), new Vector2(0, 102), new Vector2(440, 60));
            restartButton = Button("OD POCZĄTKU", card, game.Restart, out _);
            Place((RectTransform)restartButton.transform, new Vector2(0.5f, 0), new Vector2(-114, 45), new Vector2(212, 38));
            var chooseCity = Button("ZMIEŃ MIASTO", card, game.ReturnToMap, out _);
            Place((RectTransform)chooseCity.transform, new Vector2(0.5f, 0), new Vector2(114, 45), new Vector2(212, 38));
            // Keep sound available on the start, pause and result screens too.
            header.SetAsLastSibling();
            BuildCityMap();

            var events = new GameObject("Poopline input events", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.transform.SetParent(transform, false);
            var module = events.GetComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
            // Keyboard/gamepad menu commands are handled once by RunnerInput.
            module.move = null; module.submit = null; module.cancel = null;
            Refresh();
        }

        private void CreateDirectionButton(string label, float direction, Vector2 position)
        {
            var rect = Panel("Move " + (direction < 0 ? "left" : "right"), controls.transform, new Color32(61, 91, 91, 245));
            Place(rect, Vector2.zero, position, new Vector2(112, 88));
            var input = rect.gameObject.AddComponent<DirectionButton>();
            input.Game = game; input.Direction = direction;
            var text = Label(label, rect, 38, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
        }

        private void BuildCityMap()
        {
            mapScreen = Panel("City selection", safeRoot, Ink).gameObject;
            Stretch((RectTransform)mapScreen.transform);
            mapCard = Rect("Poland map card", mapScreen.transform);
            Place(mapCard, Vector2.one * 0.5f, Vector2.zero, new Vector2(560, 600));
            var caption = Label("POOPLINE  /  PODZIEMNA POLSKA", mapCard, 16, TextAnchor.MiddleCenter);
            caption.color = Lime;
            Place(caption.rectTransform, Vector2.one * 0.5f, new Vector2(0, 272), new Vector2(540, 30));
            var heading = Label("Gdzie schodzimy?", mapCard, 36, TextAnchor.MiddleCenter);
            Place(heading.rectTransform, Vector2.one * 0.5f, new Vector2(0, 228), new Vector2(540, 52));
            var map = Rect("Poland", mapCard);
            Place(map, Vector2.one * 0.5f, new Vector2(0, 28), new Vector2(500, 310));
            var graphic = map.gameObject.AddComponent<PolandMapGraphic>();
            graphic.color = new Color32(43, 72, 72, 255);
            graphic.raycastTarget = false;
            cityLabels = new Text[CityCatalog.All.Length];
            cityDots = new Text[CityCatalog.All.Length];
            Vector2[] offsets = { new Vector2(74, 9), new Vector2(-80, 12), new Vector2(85, 0),
                new Vector2(-82, -8), new Vector2(-70, -25), new Vector2(78, 9) };
            for (int i = 0; i < CityCatalog.All.Length; i++)
            {
                int index = i;
                SewerCity city = CityCatalog.All[i];
                Vector2 position = PolandMapGraphic.Project(city.Coordinates, map.sizeDelta);
                var pin = Button("●", map, () => game.SelectCity(index), out cityDots[i]);
                pin.name = "City pin: " + city.Id;
                pin.GetComponent<Image>().color = Color.clear;
                cityDots[i].fontSize = 30;
                Place((RectTransform)pin.transform, Vector2.one * 0.5f, position, new Vector2(44, 44));
                var label = Button(city.Name, map, () => game.SelectCity(index), out cityLabels[i]);
                label.name = "City label: " + city.Id;
                label.GetComponent<Image>().color = new Color32(17, 34, 40, 225);
                cityLabels[i].fontSize = 20;
                Place((RectTransform)label.transform, Vector2.one * 0.5f, position + offsets[i], new Vector2(120, 36));
            }
            mapSelection = Label("", mapCard, 24, TextAnchor.MiddleCenter);
            Place(mapSelection.rectTransform, Vector2.one * 0.5f, new Vector2(0, -157), new Vector2(540, 34));
            var help = Label("Ty decydujesz, kiedy i w którą stronę iść.\nCel: dotrzeć kanałami do oczyszczalni.", mapCard, 18, TextAnchor.MiddleCenter);
            Place(help.rectTransform, Vector2.one * 0.5f, new Vector2(0, -200), new Vector2(540, 56));
            var enter = Button("WEJDŹ DO KANAŁÓW", mapCard, game.PrimaryAction, out var enterText);
            enter.GetComponent<Image>().color = Lime; enterText.color = Ink; enterText.fontSize = 22;
            Place((RectTransform)enter.transform, Vector2.one * 0.5f, new Vector2(0, -256), new Vector2(440, 56));
            var keyboardHint = Label("Strzałki / pad: wybór miasta   •   Spacja / A: wejście", mapCard, 13, TextAnchor.MiddleCenter);
            Place(keyboardHint.rectTransform, Vector2.one * 0.5f, new Vector2(0, -296), new Vector2(550, 24));
        }

        private void Update()
        {
            if (game == null) return;
            ApplySafeArea();
            if (mapCard != null)
            {
                float scale = Mathf.Min(1, (safeRoot.rect.width - 24) / 560, (safeRoot.rect.height - 24) / 620);
                mapCard.localScale = Vector3.one * Mathf.Max(0.1f, scale);
            }
            if (previousPhase != game.State.Phase || Time.unscaledTime >= nextRefresh) Refresh();
        }

        private void ApplySafeArea()
        {
            Rect area = Screen.safeArea;
            var size = new Vector2Int(Screen.width, Screen.height);
            if (area == lastSafeArea && size == lastScreen) return;
            if (size.x <= 0 || size.y <= 0 || area.width <= 0 || area.height <= 0) return;
            safeRoot.anchorMin = new Vector2(area.xMin / size.x, area.yMin / size.y);
            safeRoot.anchorMax = new Vector2(area.xMax / size.x, area.yMax / size.y);
            lastSafeArea = area; lastScreen = size;
        }

        private void Refresh()
        {
            nextRefresh = Time.unscaledTime + 0.1f;
            RunPhase phase = game.State.Phase;
            cityTitle.text = game.CurrentCity.Name.ToUpperInvariant();
            status.text = $"BĄBELKI {game.State.Coins}/{game.TotalCoins}    META {Mathf.RoundToInt(game.Progress * 100)}%";
            soundText.text = game.Muted ? "DŹWIĘK: WYŁ." : "DŹWIĘK: WŁ.";
            progress.anchorMax = new Vector2(game.Progress, 1);
            if (previousCity != game.SelectedCityIndex || phase != previousPhase)
            {
                previousCity = game.SelectedCityIndex;
                mapSelection.text = game.CurrentCity.Name + "  /  rekord: " + game.BestCoins;
                for (int i = 0; i < cityLabels.Length; i++)
                    cityLabels[i].color = cityDots[i].color = i == previousCity ? game.CurrentCity.Accent : Cream;
            }
            if (phase == previousPhase) return;
            previousPhase = phase;
            bool onMap = phase == RunPhase.CityMap;
            mapScreen.SetActive(onMap);
            headerObject.SetActive(!onMap);
            overlay.SetActive(phase != RunPhase.Playing && !onMap);
            controls.SetActive(phase == RunPhase.Playing);
            pauseButton.SetActive(phase == RunPhase.Playing);
            restartButton.SetActive(phase == RunPhase.Paused);
            switch (phase)
            {
                case RunPhase.Ready:
                    title.text = "Mała wielka podróż";
                    message.text = "Idź w lewo lub w prawo i skacz.\nZnajdź drogę do oczyszczalni.\nZbieraj bąbelki w swoim tempie.";
                    actionText.text = "RUSZAMY!";
                    break;
                case RunPhase.Paused:
                    title.text = "Chwila przerwy";
                    message.text = "Kanał może poczekać.\nWróć, gdy będziesz gotowy.";
                    actionText.text = "WRACAMY";
                    break;
                case RunPhase.Won:
                    title.text = "Jesteś na miejscu!";
                    message.text = $"Oczyszczalnia osiągnięta w {game.State.Seconds:0.0} s.\nBąbelki: {game.State.Coins}/{game.TotalCoins}\nRekord miasta: {game.BestCoins}";
                    actionText.text = "JESZCZE RAZ";
                    break;
                case RunPhase.Lost:
                    title.text = "Mały zator…";
                    message.text = $"Przebyta droga: {Mathf.RoundToInt(game.Progress * 100)}%.\nMożesz zatrzymać się przed przeszkodą\ni wybrać moment skoku.";
                    actionText.text = "SPRÓBUJ PONOWNIE";
                    break;
            }
        }

        private static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }
        private static RectTransform Panel(string name, Transform parent, Color color)
        {
            var rect = Rect(name, parent);
            rect.gameObject.AddComponent<Image>().color = color;
            return rect;
        }
        private Text Label(string text, Transform parent, int size, TextAnchor alignment)
        {
            var rect = Rect("Text", parent);
            var label = rect.gameObject.AddComponent<Text>();
            label.font = font; label.text = text; label.fontSize = size;
            label.alignment = alignment; label.color = Cream;
            label.raycastTarget = false;
            return label;
        }
        private GameObject Button(string text, Transform parent, UnityEngine.Events.UnityAction action, out Text label)
        {
            var rect = Panel(text + " button", parent, new Color32(52, 76, 78, 255));
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<Image>();
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(action);
            label = Label(text, rect, 18, TextAnchor.MiddleCenter);
            Stretch(label.rectTransform);
            return rect.gameObject;
        }
        private static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position; rect.sizeDelta = size;
        }
        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}

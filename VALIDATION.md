# Sprawdzenie Poopline 0.5.0

26 października 2026. Unity **6000.5.1f1**, macOS, lokalny Unity Editor. Układ sceny: **8**.

## Wynik

**21/21 testów PlayMode**, zero błędów, pominięć i testów nieokreślonych. Czas zestawu: **208.72 s**. Zakończenie: **2026-10-26 16:38:38Z**. Raport: `TestResults/PlayMode-0.5.xml`, postęp: `TestResults/0.5-progress.txt`.

- Rzeczywisty ruch klawiaturą przez oba poziomy główną trasą, automatyczne przejście między etapami i ukończenie całości. Bez teleportacji i awaryjnych powrotów.
- Osobne przejście obu etapów alternatywnymi trasami, również bez awaryjnych powrotów. W drugim wariancie przejście wodą omija wejście na drabinę, a trasa przez boczne komory omija patrole.
- Mapa pod E oraz M: otwieranie, zamykanie oboma klawiszami i Escape. Zatrzymanie gracza w nurcie, szczurów i zegara. Zamknięcie mapy zachowuje wcześniej włączoną pauzę.
- Na początku tylko najbliższy odkryty fragment. Ukryte wyjście, oddzielne zapisy etapów, brak odkrywania przez narożnik ściany. Wnętrze cysterny nie jest oznaczane jako nieodkryta droga.
- Zachowanie odkryć po restarcie, odczyt lokalnego zapisu i rzeczywiste ponowne załadowanie sceny. Czyszczenie mapy oraz odrzucanie błędnego lub niepasującego zapisu.
- Punkty orientacyjne na mapie pojawiają się dopiero po odkryciu. Brak napisów TextMesh w świecie.
- Chodzenie, zatrzymanie i cofanie; brodzenie, nurt, ruch pod prąd oraz wychodzenie na oba brzegi w trzech miejscach.
- Przeciekający tunel prowadzi do komory i uruchamia punkt kontrolny. Kamera podąża za bocznymi odnogami.
- Wszystkie cztery górne powierzchnie: próby spadnięcia w ośmiu kierunkach, bez utraty wysokości.
- Wszystkie cztery drabiny: wejście i zejście, zatrzymanie, cofanie, pauza, zwolnienie kierunku po dotarciu. Boczne drabiny chwytają przed krawędzią; odejście po skosie nie przyciąga ponownie.
- Szczur przywraca punkt kontrolny właściwego etapu. Restart pozostaje w aktualnym etapie; restart po ukończeniu całości wraca do pierwszego.
- Pauza zatrzymuje wodę, krople, patrole i zegar, także w trakcie przejścia między etapami. Utrata fokusu zatrzymuje grę.
- Telefon obraca się z postacią, dalekie lampy pozostają wygaszone, brak światła otoczenia i słońca. Zasłaniające nadproże odsłania postać i wraca po wyjściu z tunelu.

Znana trasa z testu:

```text
Level 1: 26.5 seconds
Level 2: 32.1 seconds
Recoveries: 0
```

To czas gry bez szukania drogi. W testach pełnych tras zegar Unity działał z mnożnikiem 2, bez zmiany prędkości postaci czy zasad kolizji. Obecne poziomy celowo nie wymuszają trzech minut marszu; eksploracja odnóg i oglądanie mapy nie są ujęte w powyższych czasach.

## Obraz i scena

Sprawdzono podgląd rzeczywistej gry oraz mapę uruchomioną klawiszem E. Przejrzano obrazy z rozgrywki testowej: `TestResults/cistern.png`, `maze-explored.png`, `exploration-map.png` i `stage-2.png`. Mapa pokazuje fragment odwiedzonej sieci, pozycję i kierunek postaci, a pozostałe komory pozostają niewidoczne.

Walidacja sceny w Editorze: `POOPLINE_05_VALIDATED`. Dwa etapy, jedna postać, zero szczurów w pierwszym, trzy w drugim, cztery drabiny, dane map obu etapów, brak napisów w świecie i brak brakujących skryptów.

## Zakres i zapis

- Dwa zwarte układy komór z pętlami i alternatywnymi trasami, w jednej scenie. Nowe Play rozpoczyna etap pierwszy.
- Odkrycia mapy są zapisywane lokalnie w PlayerPrefs, osobno dla etapu i wersji układu. Zapis nie przywraca pozycji postaci ani ukończenia kampanii. Punkty kontrolne obowiązują w bieżącej rozgrywce.
- Testy używają własnej przestrzeni zapisu, usuwanej po każdym teście; nie odsłaniają map gracza.
- Woda jest animowaną geometrią z lokalnymi prądami; grafika i animacje pozostają prototypowe. Brak nagrań dźwiękowych.
- Silnik i pakiety bez aktualizacji. Nie wykonano nowego buildu macOS ani testów mobilnych. Stare aplikacje w `Builds` nie przedstawiają 0.5.
- Raporty, obrazy i kopia poprzedniej wersji (`Library/PooplineBackups/before-exploration-maze-20261026.tar.gz`) są lokalne, poza śledzonymi plikami gry.

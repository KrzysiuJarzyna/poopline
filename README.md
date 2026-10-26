# POOPLINE — odkrywanie kanałów (0.5)

Projekt do uruchamiania w Unity **6000.5.1f1**. Otwórz scenę `Assets/Poopline/Scenes/Poopline.unity` i kliknij **Play**. Pierwszy etap prowadzi automatycznie do drugiego. Nie ma trzeciego etapu.

## Układ poziomów

**01 — Pod kamienicami.** Krótki kolektor z wodą prowadzi do dwóch odnóg. Przeciekający tunel, okrągła cysterna, komora pomp i studnia pod podwórkiem tworzą sieć z pętlami. Można wracać inną drogą, przechodzić między komorami i odkrywać górną galerię. Bez przeciwników. Nie ma obowiązkowego marszu przez cztery długie odcinki ani założonego minimalnego czasu przejścia.

**02 — Żeliwne mosty.** Trzy wejścia w odnogi, dwa górne obejścia, cztery drabiny i trzy patrole szczurów. Woda, pomosty oraz boczne komory dają alternatywne drogi. Komora pod włazem i galeria pomp łączą się z dalszym odpływem. Kontakt ze szczurem przywraca ostatni punkt kontrolny w tym etapie.

Wyjścia mają murowany portal i chłodne, zielonkawe światło. W świecie nie ma instrukcji ani napisów. Podpowiedzi pozostają w niebieskim interfejsie. Każdy etap ma dwa automatyczne punkty kontrolne.

## Mapa i zapis odkryć

**E lub M** otwiera mapę bieżącego etapu. Pokazuje położenie i kierunek postaci, wodę, poznane przejścia, pomosty i drabiny. Nazwy charakterystycznych miejsc oraz symbol dalszego kanału pojawiają się dopiero po odkryciu. Żółte kropki na granicy odkrytego fragmentu wskazują miejsca, które można dalej zbadać.

Mapa odsłania tylko najbliższe otoczenie postaci (zasięg 4,5 m). Ściany, zakręty i maszyny zasłaniają dalsze fragmenty. Nie pokazuje całego pokoju od razu ani odległych pomieszczeń. Fizyczna ciemność w świecie pozostaje niezależna od pamięci mapy.

Odkrycia zapisują się lokalnie automatycznie, osobno dla każdego poziomu. Pozostają po kontakcie ze szczurem, restarcie etapu i ponownym Play. Przycisk **Wyczyść mapę tego etapu** pozwala rozpocząć odkrywanie od nowa; aktualne otoczenie pojawia się ponownie. Zmiana wersji układu izoluje stary zapis.

Zapis obejmuje **odkrytą mapę**, nie pozycję gracza ani ukończenie kampanii. Nowe Play rozpoczyna pierwszy etap. Punkty kontrolne działają w bieżącej rozgrywce.

## Sterowanie

| Czynność | Klawisze |
| --- | --- |
| Chodzenie / brodzenie | WASD lub strzałki |
| Drabiny | Kierunek zgodny z ustawieniem drabiny |
| Mapa / zamknięcie mapy | E lub M |
| Zamknięcie mapy, poza nią pauza | Escape |
| Restart bieżącego etapu | R |

Drabiny chwytają automatycznie. Po dotarciu na podest puść kierunek, zanim ruszysz dalej. Z górnych pomostów można zejść wyłącznie drabinami. Z nurtu można wyjść na oba brzegi i iść pod prąd.

Otwarcie mapy zatrzymuje postać, wodę, patrole oraz zegar. Zamknięcie nie usuwa wcześniej włączonej pauzy. Utrata fokusu również zatrzymuje grę; powrót do okna nie wznawia jej automatycznie. Restart po ukończeniu obu etapów wraca do pierwszego, zachowując mapy.

## Świat i edycja

Cegła, płyty posadzki, żeliwne kraty, pompy, przeciekające przewody i studnia pod podwórkiem nawiązują do starych polskich kanałów. Oświetlenie pochodzi z miejscowych lamp i telefonu. Zachowano cienie oraz odsłanianie architektury, gdy zasłania bohatera. Woda wpływa i odpływa za kratami, spada do ciemnych komór i kapie z rur. To animowana geometria i lokalne prądy, bez symulacji hydraulicznej.

Oba poziomy są zwykłymi obiektami jednej sceny: `01 — Pod kamienicami` i `02 — Żeliwne mosty`. Tylko pierwszy jest aktywny w zapisanej scenie. Postać, kamera i interfejs są wspólne.

**Tools → Poopline → Rebuild playable canal** odtwarza scenę z przepisu, zastępując ręczne zmiany. Nie jest potrzebne do zwykłego Play. Lokalna kopia wersji 0.4 znajduje się w `Library/PooplineBackups/before-exploration-maze-20261026.tar.gz`.

- `Runtime/SewerWorld.cs`, `SewerLevel.cs` — geometria, połączenia komór, dane mapy i punkty orientacyjne.
- `Runtime/ExplorationMap.cs`, `ExplorationMapView.cs` — odkrywanie, lokalny zapis i ekran mapy.
- `Runtime/PooplineGame.cs`, `RunnerInput.cs`, `RunnerHud.cs` — etapy, sterowanie i interfejs.
- `Runtime/SewerPlayer.cs`, `SewerLadder.cs`, `ElevatedWalkway.cs` — ruch i bezpieczne pomosty.
- Pozostałe elementy `Runtime/` — kamera, światło, zasłanianie postaci, woda i szczury.

## Sprawdzenie

**Tools → Poopline → Run PlayMode checks** uruchamia sprawdzenie w otwartym Editorze. Raport: `TestResults/PlayMode-0.5.xml`; postęp: `TestResults/0.5-progress.txt`. Testy przechodzą oba etapy rzeczywistym ruchem po głównej i alternatywnej trasie. Zapis map testowych jest odizolowany od odkryć gracza i usuwany po sprawdzeniu. Szczegółowy wynik znajduje się w `VALIDATION.md`.

Silnik i pakiety nie były aktualizowane. To lokalna wersja do testowania przez Play w Unity Editor. Stare aplikacje w `Builds` nie reprezentują 0.5. Grafika i animacje pozostają prototypowe; nie dodano nagrań dźwiękowych.

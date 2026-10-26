# POOPLINE — menu, zapis podróży i śluza (0.6)

Projekt do uruchamiania w Unity **6000.5.1f1**. Otwórz scenę `Assets/Poopline/Scenes/Poopline.unity` i kliknij **Play**. Pojawi się menu: **Nowa gra**, **Kontynuuj**, **Ustawienia**, **Sterowanie**, **Wyjdź**. Pierwszy etap prowadzi automatycznie do drugiego. Nie ma trzeciego etapu.

## Układ poziomów

**01 — Pod kamienicami.** Krótki kolektor z wodą prowadzi do dwóch odnóg. Przeciekający tunel, okrągła cysterna, komora pomp i studnia pod podwórkiem tworzą sieć z pętlami. Można wracać inną drogą, przechodzić między komorami i odkrywać górną galerię. Dalszą drogę zamyka zalana komora z kratą. Zawór przy starych pompach otwiera odpływ: woda opada, krata się podnosi, a gracz może przejść do następnego etapu. Bez przeciwników. Nie ma obowiązkowego marszu przez cztery długie odcinki ani założonego minimalnego czasu przejścia.

**02 — Żeliwne mosty.** Trzy wejścia w odnogi, dwa górne obejścia, cztery drabiny i trzy patrole szczurów. Woda, pomosty oraz boczne komory dają alternatywne drogi. Komora pod włazem i galeria pomp łączą się z dalszym odpływem. Kontakt ze szczurem przywraca ostatni punkt kontrolny w tym etapie.

Wyjścia mają murowany portal i chłodne, zielonkawe światło. W świecie nie ma instrukcji ani napisów. Podpowiedzi pozostają w niebieskim interfejsie. Pierwszy etap ma trzy automatyczne punkty kontrolne, drugi dwa.

## Menu, zapis i ustawienia

Menu pokazuje osobny, animowany fragment kanału: ceglany łuk, nieszczelną rurę, krople i wodę oświetloną miejscową lampą. Nie uruchamia rozgrywki w tle. **Kontynuuj** jest dostępne, gdy istnieje poprawny zapis nieukończonej podróży. **Nowa gra** pyta o zastąpienie istniejącego zapisu i map obu etapów.

Jeden lokalny slot przechowuje etap, ostatni punkt kontrolny, odkrycia obu map, stan śluzy oraz czas rozgrywki. Zapis następuje po wejściu na punkt, otwarciu odpływu, zmianie etapu, otwarciu mapy/pauzy, wyjściu do menu, utracie fokusu i okresowo podczas gry. Kontynuacja odtwarza **punkt kontrolny, nie ostatnie współrzędne**. Ponowne wejście zachowuje otwartą śluzę, także jeśli zapis nastąpił podczas odpływu; animacja jest wtedy zakończona. Po ukończeniu obu etapów menu proponuje nową grę.

Zapis w PlayerPrefs jest wersjonowany i sprawdzany przed odczytem. Przy niepoprawnym głównym zapisie gra próbuje poprzedniej poprawnej kopii. Niepasujące układy poziomów nie są wczytywane. Zmiana układu z 0.5 na 0.6 rozpoczyna nowe odkrycia. Nie ma kont, serwera ani synchronizacji między urządzeniami.

Ustawienia obejmują głośność ogólną, głośność tła i kapania oraz włączenie ruchu tła menu. Są zapisywane niezależnie od nowej gry. Sześć tymczasowych, oryginalnych efektów syntetycznych: szum, kropla, krok/brodzenie, zawór, odpływ i kliknięcie. Docelowe nagrania i miks są kolejną iteracją.

## Mapa i zapis odkryć

**E lub M** otwiera mapę bieżącego etapu. Pokazuje położenie i kierunek postaci, wodę, poznane przejścia, pomosty i drabiny. Nazwy charakterystycznych miejsc oraz symbol dalszego kanału pojawiają się dopiero po odkryciu. Żółte kropki na granicy odkrytego fragmentu wskazują miejsca, które można dalej zbadać.

Mapa odsłania tylko najbliższe otoczenie postaci (zasięg 4,5 m). Ściany, zakręty i maszyny zasłaniają dalsze fragmenty. Nie pokazuje całego pokoju od razu ani odległych pomieszczeń. Fizyczna ciemność w świecie pozostaje niezależna od pamięci mapy.

Odkrycia zapisują się lokalnie automatycznie, osobno dla każdego poziomu. Pozostają po kontakcie ze szczurem, restarcie etapu i ponownym Play. Przycisk **Wyczyść mapę tego etapu** pozwala rozpocząć odkrywanie od nowa; aktualne otoczenie pojawia się ponownie. Zmiana wersji układu izoluje stary zapis.

Po osuszeniu komory jej odkryty fragment zmienia się na mapie w przejście. Zawór i komora pojawiają się dopiero po dotarciu w ich okolice.

## Sterowanie

| Czynność | Klawisze |
| --- | --- |
| Chodzenie / brodzenie | WASD lub strzałki |
| Drabiny | Kierunek zgodny z ustawieniem drabiny |
| Mapa / zamknięcie mapy | E lub M |
| Zamknięcie mapy, poza nią pauza | Escape |
| Powrót do ostatniego punktu kontrolnego | R |
| Obrót zaworu w pobliżu pomp | Przytrzymaj F przez 1,5 sekundy |

Drabiny chwytają automatycznie. Po dotarciu na podest puść kierunek, zanim ruszysz dalej. Z górnych pomostów można zejść wyłącznie drabinami. Z nurtu można wyjść na oba brzegi i iść pod prąd.

Otwarcie mapy zatrzymuje postać, wodę, patrole oraz zegar. Zamknięcie nie usuwa wcześniej włączonej pauzy. Utrata fokusu również zatrzymuje grę; powrót do okna nie wznawia jej automatycznie. Pauza pozwala wrócić do gry, do punktu kontrolnego, ustawień albo menu. Nowa gra wraca do pierwszego etapu i czyści mapy. Śluza nie obraca się ani nie opróżnia podczas pauzy lub czytania mapy.

## Świat i edycja

Cegła, płyty posadzki, żeliwne kraty, pompy, przeciekające przewody i studnia pod podwórkiem nawiązują do starych polskich kanałów. Oświetlenie pochodzi z miejscowych lamp i telefonu. Zachowano cienie oraz odsłanianie architektury, gdy zasłania bohatera. Woda wpływa i odpływa za kratami, spada do ciemnych komór i kapie z rur. To animowana geometria i lokalne prądy, bez symulacji hydraulicznej. Śluza ma kontrolowane stany: zamknięta, odpływ przez 4,5 sekundy, otwarta. Fizyczna bariera znika dopiero po zakończeniu odpływu.

Oba poziomy są zwykłymi obiektami jednej sceny: `01 — Pod kamienicami` i `02 — Żeliwne mosty`. Tylko pierwszy jest aktywny w zapisanej scenie. Postać, kamera i interfejs są wspólne. W Play menu wyłącza oba poziomy i postać, a aktywuje własne tło.

**Tools → Poopline → Rebuild playable canal** odtwarza scenę z przepisu, zastępując ręczne zmiany. Nie jest potrzebne do zwykłego Play. Lokalna kopia zaakceptowanej wersji 0.5 znajduje się w `Library/PooplineBackups/before-menu-sluice-20261026.tar.gz`.

- `Runtime/SewerWorld.cs`, `SewerLevel.cs` — geometria, połączenia komór, dane mapy i punkty orientacyjne.
- `Runtime/ExplorationMap.cs`, `ExplorationMapView.cs` — odkrywanie, lokalny zapis i ekran mapy.
- `Runtime/JourneySave.cs` — zapis podróży, kopia awaryjna i ustawienia.
- `Runtime/GameMenuView.cs`, `SluicePuzzle.cs`, `SewerAudio.cs` — menu, zagadka i odgłosy.
- `Runtime/PooplineGame.cs`, `RunnerInput.cs`, `RunnerHud.cs` — etapy, sterowanie i interfejs.
- `Runtime/SewerPlayer.cs`, `SewerLadder.cs`, `ElevatedWalkway.cs` — ruch i bezpieczne pomosty.
- Pozostałe elementy `Runtime/` — kamera, światło, zasłanianie postaci, woda i szczury.

## Sprawdzenie

**Tools → Poopline → Run PlayMode checks** uruchamia sprawdzenie w otwartym Editorze. Raport: `TestResults/PlayMode-0.6.xml`; postęp: `TestResults/0.6-progress.txt`. Testy przechodzą oba etapy rzeczywistym ruchem po głównej i alternatywnej trasie. Testy sprawdzają też menu, śluzę, punkty kontrolne i odczyt zapisu po ponownym załadowaniu sceny. Zapisy, mapy i ustawienia testowe mają osobną przestrzeń danych, usuwaną po sprawdzeniu. Szczegółowy wynik znajduje się w `VALIDATION.md`.

Silnik i pakiety nie były aktualizowane. To lokalna wersja do testowania przez Play w Unity Editor. Stare aplikacje w `Builds` nie reprezentują 0.6. Grafika, animacje i dźwięki pozostają prototypowe.

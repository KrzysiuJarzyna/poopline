# POOPLINE — myśli bohatera i stopniowane podpowiedzi (0.6.2)

Projekt do uruchamiania w Unity **6000.5.1f1**. Otwórz scenę `Assets/Poopline/Scenes/Poopline.unity` i kliknij **Play**. Pojawi się menu: **Nowa gra**, **Kontynuuj**, **Ustawienia**, **Sterowanie**, **Wyjdź**. Pierwszy etap prowadzi automatycznie do drugiego. Nie ma trzeciego etapu.

## Układ poziomów

**01 — Pod kamienicami.** Krótki kolektor z wodą prowadzi do dwóch odnóg. Przeciekający tunel, okrągła cysterna, komora pomp i studnia pod podwórkiem tworzą sieć z pętlami. Można wracać inną drogą, przechodzić między komorami i odkrywać górną galerię. Obie trasy zbiegają się przed zalanym przejściem z kratą ustawioną przodem do kamery. Najpierw gracz widzi przeszkodę, potem może skręcić w boczny korytarz i podążyć za rurą do komory zaworu. Ten korytarz nie ma osobnego wejścia od wcześniejszych pomp. Rura łączy wlot w zalanym kanale, zawór i niższą studnię odpływu. Dopiero otwarcie zaworu osusza przejście; trzeba wrócić pod kratę i przejść do dalszego kanału. Bez przeciwników. Nie ma obowiązkowego marszu przez cztery długie odcinki ani założonego minimalnego czasu przejścia.

**02 — Żeliwne mosty.** Trzy wejścia w odnogi, dwa górne obejścia, cztery drabiny i trzy patrole szczurów. Woda, pomosty oraz boczne komory dają alternatywne drogi. Komora pod włazem i galeria pomp łączą się z dalszym odpływem. Kontakt ze szczurem przywraca ostatni punkt kontrolny w tym etapie.

Wyjścia mają murowany portal i chłodne, zielonkawe światło. W świecie nie ma tablic z instrukcjami ani podpisów komór. Narrację prowadzi bohater w dymkach nad głową; niebieski interfejs pokazuje rozdział, sterowanie i informacje o zapisie. Pierwszy etap ma trzy automatyczne punkty kontrolne, drugi dwa. Układ poziomów i krata pozostają takie jak w zaakceptowanej wersji 0.6.1.

## Bohater myśli na głos

Postać zauważa problem i formułuje przypuszczenia w pierwszej osobie. Nie dostaje od razu instrukcji rozwiązania ani stałej strzałki do zaworu. Krótki, jasny dymek podąża nad głową, łagodnie pojawia się i znika. Na ekranie jest najwyżej jedna myśl; aktualne zdarzenie zastępuje nieaktualną podpowiedź.

| Moment przy nierozwiązanej śluzie | Przykładowa myśl |
| --- | --- |
| Pierwsze spotkanie zalanego przejścia | „Tędy nie przejdę. Za dużo wody.” |
| Po 45 sekundach dalszej gry | „Może da się jakoś spuścić tę wodę?” |
| Po kolejnych 60 sekundach | „Ta rura… może prowadzi do zaworu odpływu? Sprawdzę boczny korytarz.” |
| Samodzielne znalezienie zaworu, niezależnie od czasu | „Jest zawór. Sprawdzę, czy otwiera odpływ.” |

Pierwsze trzy stopnie mają kilka wariantów tekstu. Jeden wariant pojawia się raz, bez zapętlonego przypominania. Liczony jest czas aktywnej rozgrywki po odkryciu przeszkody; menu, pauza, mapa i przejście między etapami nie przyspieszają podpowiedzi. To prosty próg czasu, nie ocena umiejętności gracza. Znalezienie zaworu zatrzymuje narastanie podpowiedzi. Po otwarciu odpływu bohater reaguje na schodzącą wodę i możliwość powrotu. Początki obu etapów oraz reakcja na szczura także korzystają z dymków.

Zapis przechowuje stopień podpowiedzi, odczekany czas oraz znalezienie zaworu. Kontynuacja i powrót do punktu nie powtarzają początku zagadki; nowa gra zeruje ten postęp. Nie odtwarzamy urwanej kwestii po ponownym załadowaniu sceny. Dymki są na razie nieme.

## Menu, zapis i ustawienia

Menu pokazuje osobny, animowany fragment kanału: ceglany łuk, nieszczelną rurę, krople i wodę oświetloną miejscową lampą. Nie uruchamia rozgrywki w tle. **Kontynuuj** jest dostępne, gdy istnieje poprawny zapis nieukończonej podróży. **Nowa gra** pyta o zastąpienie istniejącego zapisu i map obu etapów.

Jeden lokalny slot przechowuje etap, ostatni punkt kontrolny, odkrycia obu map, stan śluzy oraz czas rozgrywki. Zapis następuje po wejściu na punkt, otwarciu odpływu, zmianie etapu, otwarciu mapy/pauzy, wyjściu do menu, utracie fokusu i okresowo podczas gry. Kontynuacja odtwarza **punkt kontrolny, nie ostatnie współrzędne**. Ponowne wejście zachowuje otwartą śluzę, także jeśli zapis nastąpił podczas odpływu; animacja jest wtedy zakończona. Po ukończeniu obu etapów menu proponuje nową grę.

Zapis w PlayerPrefs jest wersjonowany i sprawdzany przed odczytem. Przy niepoprawnym głównym zapisie gra próbuje poprzedniej poprawnej kopii. Niepasujące układy poziomów nie są wczytywane. Wersja 0.6.2 zachowuje układ 10 i odczytuje zapisy z 0.6.1; brakujące dane podpowiedzi dostają wartości początkowe. Zapisy starszego układu 0.6.0 nie są zgodne. Aby obejrzeć całą narrację od początku, wybierz **Nowa gra**. Nie ma kont, serwera ani synchronizacji między urządzeniami.

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
| Obrót zaworu w bocznej komorze | Przytrzymaj F przez 1,5 sekundy |

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
- `Runtime/PlayerThoughts.cs`, `ThoughtBubbleView.cs` — katalog kwestii, czas wypowiedzi i dymki nad postacią. Progi oraz stan podpowiedzi zagadki są w `SluicePuzzle.cs`.
- `Runtime/PooplineGame.cs`, `RunnerInput.cs`, `RunnerHud.cs` — etapy, sterowanie i interfejs.
- `Runtime/SewerPlayer.cs`, `SewerLadder.cs`, `ElevatedWalkway.cs` — ruch i bezpieczne pomosty.
- Pozostałe elementy `Runtime/` — kamera, światło, zasłanianie postaci, woda i szczury.

## Sprawdzenie

**Tools → Poopline → Run PlayMode checks** uruchamia sprawdzenie w otwartym Editorze. Raport: `TestResults/PlayMode-0.6.2.xml`; postęp: `TestResults/0.6.2-progress.txt`. Testy przechodzą oba etapy rzeczywistym ruchem po głównej i alternatywnej trasie. Sprawdzają też menu, śluzę, punkty kontrolne, odczyt zapisu po ponownym załadowaniu sceny, progi podpowiedzi, zatrzymanie ich czasu i zgodność starszego zapisu. Na obu trasach gracz najpierw próbuje przejść przez zamkniętą śluzę, a dopiero później dociera do zaworu. Osobna kontrola połączeń mapy wyklucza wejście do zaworu z pominięciem przedsionka zalanego kanału. Zapisy, mapy i ustawienia testowe mają osobną przestrzeń danych, usuwaną po sprawdzeniu. Szczegółowy wynik znajduje się w `VALIDATION.md`.

Silnik i pakiety nie były aktualizowane. To lokalna wersja do testowania przez Play w Unity Editor. Stare aplikacje w `Builds` nie reprezentują 0.6.2. Grafika, animacje i dźwięki pozostają prototypowe.

## Zapisana wersja i następna iteracja

Commit `d42667a` — **Dodaj menu noir, zapis podróży i pierwszą śluzę** — zachowuje wersję 0.6.0 przed przebudową kolejności zagadki. Obejmuje menu, ustawienia, animowane tło, zapis etapu/punktu/map/śluzy, pierwszą zagadkę i sześć roboczych dźwięków. Szczegółowy opis oraz wynik 29/29 testów znajdują się w jego treści i dokumentacji tej wersji.

Wersja 0.6.1 zmieniła przebieg pierwszego etapu: **przeszkoda → boczna odnoga → zawór i odpływ → powrót → otwarte przejście**. Układ został zaakceptowany. Wersja 0.6.2 zachowuje tę drogę i zastępuje instrukcje narracją bohatera.

### Ustalenia do dalszego rozwoju — 26 października 2026

1. **Narracja całej gry: bohater myśli na głos.** Najpierw obserwacja, potem ostrożny pomysł, a bardziej konkretna wskazówka dopiero po czasie. W kolejnych zagadkach stosować tę samą zasadę i unikać instrukcji od wszechwiedzącego narratora.
2. **Głos nagra autor projektu.** Obecne dymki są prototypem przyszłych nagrań użytkownika. Kwestie mają identyfikatory zdarzenia i wariantu, np. `FloodBlocked.0`, `DrainIdea.1`, `ValveIdea.0`, dzięki czemu można przypisać każdej osobny plik nagrania. Przy wdrożeniu głosu dodać regulację głośności dialogów, zachować opcjonalne napisy, dopasować czas wyświetlania do długości nagrania i wyciszać tło na czas mowy. Nagrania nie zostały jeszcze dostarczone ani podłączone.
3. **Krata jest tymczasowa.** Przepuszczałaby wodę, więc nie wyjaśnia zalania ani jego ustąpienia. Na razie pozostaje zgodnie z decyzją użytkownika. Docelowo przeszkodą może być sam zatopiony, niski tunel: po spuszczeniu zbyt głębokiej wody odsłania się posadzka i bezpieczne przejście. Barierę ruchu powiązać z głębokością wody, a obecną podnoszoną kratę usunąć lub zastąpić rzeczywistą szczelną zasuwą w uzasadnionym miejscu instalacji.
4. **Dźwięki wymagają osobnej iteracji.** Menu i dźwięk spuszczanej wody zostały zaakceptowane. Tło, kroki i krople należy zastąpić bardziej naturalnym, oszczędnym brzmieniem. W zmianie narracji nie modyfikowano plików ani miksu audio.

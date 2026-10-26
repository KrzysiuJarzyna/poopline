# Sprawdzenie Poopline 0.6.2

26 października 2026. Unity **6000.5.1f1**, macOS, lokalny Unity Editor. Układ sceny: **10**. Poprzednia wersja menu, zapisów i śluzy jest zachowana w commicie `d42667a`.

## Wynik

**36/36 testów PlayMode**, zero niepowodzeń, pominięć i testów nieokreślonych. Czas zestawu: **317.33 s**. Zakończenie: **2026-10-26 18:12:23Z**. Raport: `TestResults/PlayMode-0.6.2.xml`, postęp: `TestResults/0.6.2-progress.txt`.

- Oba poziomy przechodzone rzeczywistym ruchem klawiatury, główną oraz alternatywną trasą. Obrót zaworu przez F, przejście przez osuszoną komorę, automatyczna zmiana etapu i ukończenie całości. Bez teleportacji ani awaryjnych powrotów w testach całych tras.
- Na obu trasach pierwszego etapu gracz najpierw spotyka zalane przejście i sprawdza jego fizyczną blokadę. Mapa nie pokazuje wtedy jeszcze zaworu. Następnie przechodzi bocznym korytarzem do pokrętła, otwiera odpływ i wraca do osuszonego przejścia.
- Kontrola połączeń wszystkich pól mapy potwierdza, że zawór jest osiągalny, ale nie istnieje droga do niego z pominięciem przedsionka zalanego kanału. Osobny test bezpośredniego ustawienia postaci przy zaworze potwierdza, że nie można osuszyć nieodkrytego przejścia.
- Zapis przed zamkniętą śluzą i ponowne załadowanie sceny przywracają punkt kontrolny, rozpoznaną przeszkodę i nierozwiązaną zagadkę. Po kontynuacji da się rzeczywistym ruchem przejść do bocznej komory i otworzyć odpływ.
- Menu wyłącza postać i oba poziomy, lecz krople w jego własnym tle nadal się poruszają. Wyłączenie ruchu tła zatrzymuje krople i kamerę. Powrót do gry przywraca aktywny etap.
- Zapis etapu, punktu kontrolnego, obu map i stanu śluzy po rzeczywistym ponownym załadowaniu sceny. Kontynuacja wraca do punktu, a nie ostatnich współrzędnych. R również przywraca punkt.
- Nowa gra wymaga potwierdzenia istniejącego zapisu, czyści obie mapy oraz zagadkę, zachowuje ustawienia. Enter nie zastępuje ukończonego zapisu bez potwierdzenia. Escape pozwala anulować także po ukończeniu kampanii.
- Niepoprawny JSON lub wersja układu nie są wczytywane; poprawna kopia awaryjna odzyskuje uszkodzony główny zapis. Pusty slot wyłącza kontynuację.
- Zalana komora blokuje ruch fizyczną kratą i nie pozwala uruchomić następnego etapu. F poza zasięgiem nie działa. Puszczenie klawisza cofa niedokończony obrót. Mapa i pauza zatrzymują obrót oraz odpływ.
- Po odpływie lustro wody leży poniżej podłogi, krata jest podniesiona, bariera wyłączona i można faktycznie przejść do wyjścia. Zapis podczas odpływu po odczycie odtwarza już przechodnią komorę. Powrót do punktu nie resetuje rozwiązanej zagadki.
- Sześć klipów WAV wczytuje się poprawnie. Źródło tła odtwarza dźwięk; głośność zero wycisza wszystkie źródła. Suwak tła nie wycisza interakcji. Pauza zatrzymuje odgłos odpływu. Ustawienia zostają zapisane niezależnie od podróży.
- Zachowane wcześniejsze testy: mapa E/M i Escape, ukryte nieodwiedzone komory, brak odkrywania przez narożnik ściany, oddzielne mapy, czyszczenie odkryć, punkty orientacyjne dopiero po odkryciu i brak napisów w świecie.
- Zachowane testy ruchu: nurt, wyjście na oba brzegi, chodzenie pod prąd, boczna odnoga, wszystkie cztery drabiny w obu kierunkach oraz cztery podesty z próbami spadnięcia w ośmiu kierunkach. Szczury przywracają punkt właściwego etapu.
- Telefon nadal obraca się z postacią, dalekie lampy są wygaszane, nie ma światła otoczenia ani słońca. Nadproża odsłaniają postać. Utrata fokusu zatrzymuje grę.

Czasy znanej trasy z testu:

```text
Level 1: 46.9 seconds
Level 2: 33.0 seconds
Recoveries: 0
```

To czas bez szukania drogi. Przy testowaniu całych tras zegar działał z mnożnikiem 2, bez zmiany prędkości postaci i kolizji. Eksploracja oraz samodzielne znalezienie zaworu nie są ujęte w tym pomiarze.

## Narracja i podpowiedzi

- Pierwsze spotkanie zalania pokazuje tylko obserwację bohatera. Drugą myśl uruchamia 45 sekund aktywnej rozgrywki, trzecią kolejne 60 sekund. Przed progami nie ma wcześniejszej wskazówki; po ostatniej nie ma zapętlonego przypominania.
- Mapa i pauza zatrzymują zarówno odmierzanie kolejnej podpowiedzi, jak i czas widocznego dymka. Menu zatrzymuje zegar i usuwa bieżącą wypowiedź.
- Znalezienie zaworu przerywa narastanie podpowiedzi, również gdy kolejna wskazówka miała się właśnie pojawić. Otwarcie odpływu i osuszenie zastępują nieaktualne myśli reakcją na rozwiązanie. Zmiana etapu usuwa myśli poprzedniej zagadki.
- Zapis i rzeczywiste ponowne załadowanie sceny zachowują stopień podpowiedzi, czas oczekiwania oraz znalezienie zaworu. Kontynuacja i powrót do punktu nie powtarzają pierwszego spotkania; nowa gra resetuje te dane.
- Zapis 0.6.1 bez pól narracji nadal się wczytuje. Niepoprawny stopień, niefinitywny czas lub czas poza dopuszczalnym zakresem są odrzucane.
- Róg ekranu pokazuje wyłącznie nazwę etapu. Usunięto stały tekst z rozwiązaniem i instrukcje dołączane do punktów kontrolnych. Narracja wejścia do etapów oraz reakcja na szczura używają pierwszej osoby. Informacje o sterowaniu i zapisie pozostały w HUD.

## Obraz i scena

Sprawdzono czytelność i położenie nowych dymków na obrazach z rzeczywistego Play: `TestResults/thought-first-encounter.png`, `thought-drain-idea.png`, `thought-valve-idea.png` oraz `thought-passage-open.png`. Dłuższa podpowiedź mieści się w dymku, nad sylwetką i poza panelami sterowania. Dymki są nakładką ekranową przypiętą do pozycji głowy, nie tablicami w świecie. Sprawdzono również obrazy zagadki: `TestResults/sluice-first-encounter.png`, `sluice-valve.png`, `sluice-drained.png`. Krata jest ustawiona przodem do kamery, z widoczną wodą za prętami. Ciągła rura biegnie przy ścianie od wlotu w zalanym kanale przez boczny korytarz i zawór do niższej studni odpływu. Obrazy menu, map i pozostałych części poziomów również powstają podczas testów.

Walidacja sceny w Editorze: `POOPLINE_062_VALIDATED`. Układ 10 i geometria poziomów nie były przebudowywane w iteracji narracji. Dwa etapy, jedna postać, tło menu, komplet elementów śluzy i sześć dźwięków; zero szczurów w pierwszym etapie, trzy w drugim, cztery drabiny, mapy obu etapów, brak TextMesh i brakujących skryptów.

Przed przebudową, w zapisanej wersji 0.6.0, ręcznie sprawdzono menu, ustawienia, anulowanie nowej gry oraz Stop → Play → Kontynuuj. W 0.6.2 regresję tych funkcji i zapisów obejmuje powyższy zestaw PlayMode. Menu i jego wygląd nie były zmieniane w tej iteracji. Po testach Unity usunął swoją tymczasową scenę testową.

Pierwszy pełny bieg zakończył się 33/36: nie osiągnięto oczekiwanego stanu w trzech wcześniejszych testach drabin, końca odpływu i wychodzenia z nurtu. Po dodaniu szczegółów diagnostycznych te same testy przeszły osobno 3/3 (`TestResults/Interaction-0.6.2.xml`, 63.96 s), a następnie cały zestaw 36/36. Nie zmieniano mechanik ani kryteriów tych testów. Przyczyny pierwszych niezgodności nie potwierdzono; zachowano pierwszy raport w `TestResults/PlayMode-0.6.2-first-run.xml`.

Końcowa ręczna kontrola Play pokazała menu wersji 0.6.2 i dostępne Kontynuuj z zachowanym zapisem użytkownika w drugim etapie. Nie rozpoczynano nowej gry ani nie zastępowano tego zapisu. Projekt pozostawiono poza Play na zapisanej scenie Poopline.

## Zakres i ograniczenia

- Jeden lokalny slot w PlayerPrefs, wersjonowany według układu. Narracja zachowuje zgodność zapisów układu 10 z wersji 0.6.1. Zapisy wcześniejszego układu 0.6.0 nie są wczytywane. Bez kont, serwera i synchronizacji między urządzeniami.
- Testy mają osobną przestrzeń zapisu, map oraz ustawień; po każdym teście sprzątają własne dane. Test ponownego załadowania sceny jest odrębny od sprawdzenia Stop/Play w Editorze.
- Dymki są nieme. Plan nagrań głosu autora projektu, zasad narracji i późniejszego zastąpienia kraty zapisano w sekcji dalszego rozwoju README. Krata pozostaje tymczasowym elementem zgodnie z decyzją użytkownika.
- Woda i śluza korzystają z animowanej geometrii oraz kontrolowanych stanów, bez pełnej symulacji hydraulicznej. Grafika i animacje są prototypowe.
- Efekty są oryginalnymi dźwiękami syntetycznymi, bez zewnętrznych sampli. Użytkownik zaakceptował dźwięk spuszczanej wody i odrzucił pozostałe brzmienia jako docelowe. Pliki audio i miks pozostały bez zmian w tej przebudowie; wymiana pozostałych dźwięków jest następną iteracją. Testy sprawdzają działanie źródeł i ustawień, nie jakość odsłuchu.
- Silnik i pakiety bez aktualizacji. Nie wykonano nowego buildu macOS ani testów mobilnych. Stare aplikacje w `Builds` nie reprezentują wersji 0.6.2.
- Raporty, obrazy i kopia poprzedniej wersji (`Library/PooplineBackups/before-menu-sluice-20261026.tar.gz`) są lokalne, poza śledzonymi plikami gry.

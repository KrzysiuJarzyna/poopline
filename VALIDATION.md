# Sprawdzenie Poopline 0.6.0

26 października 2026. Unity **6000.5.1f1**, macOS, lokalny Unity Editor. Układ sceny: **9**.

## Wynik

**29/29 testów PlayMode**, zero błędów, pominięć i testów nieokreślonych. Czas zestawu: **253.13 s**. Zakończenie: **2026-10-26 17:25:32Z**. Raport: `TestResults/PlayMode-0.6.xml`, postęp: `TestResults/0.6-progress.txt`.

- Oba poziomy przechodzone rzeczywistym ruchem klawiatury, główną oraz alternatywną trasą. Obrót zaworu przez F, przejście przez osuszoną komorę, automatyczna zmiana etapu i ukończenie całości. Bez teleportacji ani awaryjnych powrotów w testach całych tras.
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
Level 2: 32.6 seconds
Recoveries: 0
```

To czas bez szukania drogi. Przy testowaniu całych tras zegar działał z mnożnikiem 2, bez zmiany prędkości postaci i kolizji. Eksploracja oraz samodzielne znalezienie zaworu nie są ujęte w tym pomiarze.

## Obraz i scena

Ręcznie sprawdzono rzeczywiste menu i ekran ustawień w Play. Obrazy testowe: `TestResults/menu-noir.png`, `sluice-flooded.png`, `sluice-valve.png`, `sluice-drained.png` oraz dotychczasowe obrazy map i poziomów. Krata ma własne miejscowe światło, a przewód odpływu został odsunięty od głównej trasy, by nie zasłaniał postaci.

Walidacja sceny w Editorze: `POOPLINE_06_VALIDATED`. Dwa etapy, jedna postać, tło menu, komplet elementów śluzy i sześć dźwięków; zero szczurów w pierwszym etapie, trzy w drugim, cztery drabiny, mapy obu etapów, brak TextMesh i brakujących skryptów.

Ręczna próba w Editorze: Nowa gra → ruch → Escape → Zapisz i wróć do menu; potwierdzenie nowej gry i anulowanie; Stop → Play → aktywne Kontynuuj → przywrócone wejście pierwszego etapu oraz odkryta mapa. Sprawdzono ustawienia i przycisk Wyjdź, który zatrzymuje Play. Po zestawie testów uproszczono sam widok pauzy, aby nie dublował nagłówków HUD; tę zmianę ponownie skompilowano i sprawdzono ręcznie, także otwarcie i zamknięcie mapy z pauzy. Projekt pozostawiono poza Play, na zapisanej scenie Poopline.

## Zakres i ograniczenia

- Jeden lokalny slot w PlayerPrefs, wersjonowany według układu. Odczyt zapisów z układu 0.5 nie jest obsługiwany. Bez kont, serwera i synchronizacji między urządzeniami.
- Testy mają osobną przestrzeń zapisu, map oraz ustawień; po każdym teście sprzątają własne dane. Test ponownego załadowania sceny jest odrębny od sprawdzenia Stop/Play w Editorze.
- Woda i śluza korzystają z animowanej geometrii oraz kontrolowanych stanów, bez pełnej symulacji hydraulicznej. Grafika i animacje są prototypowe.
- Efekty są oryginalnymi dźwiękami syntetycznymi, bez zewnętrznych sampli. Zweryfikowano ich dane, ładowanie i działanie źródeł w Unity; nie wykonano odsłuchu na urządzeniu docelowym ani docelowego miksu.
- Silnik i pakiety bez aktualizacji. Nie wykonano nowego buildu macOS ani testów mobilnych. Stare aplikacje w `Builds` nie reprezentują wersji 0.6.
- Raporty, obrazy i kopia poprzedniej wersji (`Library/PooplineBackups/before-menu-sluice-20261026.tar.gz`) są lokalne, poza śledzonymi plikami gry.
